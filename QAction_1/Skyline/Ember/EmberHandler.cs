namespace QAction_1.Skyline.Ember
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using EmberLib;
	using EmberLib.Framing;
	using EmberLib.Glow;
	using EmberLib.Glow.Framing;
	using global::Skyline.DataMiner.Net;
	using global::Skyline.DataMiner.Scripting;
	using QAction_1.Skyline.DataMiner.Scripting.Solutions.Ember;
	using QAction_1.Skyline.Ember.Protocol;

	public class EmberHandler
	{
		private readonly ParameterMapping parameterMapping;

		private readonly Queue<EmberAction> pollActions = new Queue<EmberAction>();

		private EmberAction currentPollAction;

		private GlowReader glowReader;

		private int[] lastRequestedPath = Array.Empty<int>();

		public EmberHandler(Configuration configurations)
		{
			parameterMapping = new ParameterMapping();
			Configurations = configurations;
		}

		private Configuration Configurations { get; }

		public void DiscoverEmberTree(SLProtocol protocol)
		{
			pollActions.Enqueue(new EmberDiscoveryAction(protocol, Configurations, pollActions));
			ExecuteNextAction(protocol);
		}

		public void PollParameters(SLProtocol protocol, string[] parameterPath)
		{
			parameterMapping.SetParameterPaths(parameterPath);

			if (parameterMapping.ParameterPaths.GetLength(0) > 0)
			{
				pollActions.Enqueue(new EmberPollAction(protocol, parameterMapping, Configurations));
			}

			ExecuteNextAction(protocol);
		}

		public void ResponseReceived(SLProtocol protocol, int responsePid)
		{
			if (glowReader == null)
			{
				glowReader = new GlowReader((_, e) => GlowReceived(protocol, e.Root), (_, e) => KeepAliveRequestReceived(protocol, e));
				glowReader.PackageReceived += (_, e) => PackageReceived(protocol, e);
				glowReader.FramingError += (_, e) => GlowFramingError(protocol, e.Message);
				glowReader.Error += (_, e) => GlowError(protocol, e.Message);
			}

			var response = ReadResponse(protocol, responsePid);
			HandleResponse(protocol, response);
		}

		private static byte[] BuildFrameData(object[] receivedData)
		{
			byte[] data = receivedData.Select(Convert.ToByte).ToArray();

			var framedData = new byte[data.Length + 2];
			framedData[0] = 0xFE;
			Array.Copy(data, 0, framedData, 1, data.Length);
			framedData[framedData.Length - 1] = 0xFF;

			return framedData;
		}

		private static void GlowError(SLProtocol protocol, string message)
		{
			protocol.Log("QA" + protocol.QActionID + "|GlowError|Message: " + message, LogType.Error, LogLevel.NoLogging);
		}

		private static void GlowFramingError(SLProtocol protocol, string message)
		{
			protocol.Log("QA" + protocol.QActionID + "|GlowFramingError|Message: " + message, LogType.Error, LogLevel.NoLogging);
		}

		private static void PackageReceived(SLProtocol protocol, FramingReader.PackageReceivedArgs e)
		{
			protocol.Log("QA" + protocol.QActionID + "|Package Received|MessageId " + e.MessageId, LogType.Information, LogLevel.NoLogging);
		}

		private static byte[] ReadResponse(SLProtocol protocol, int responsePid)
		{
			var receivedData = (object[])protocol.GetData("PARAMETER", responsePid);

			if (receivedData.Length == 0)
			{
				throw new Exception();
			}

			return BuildFrameData(receivedData);
		}

		private void ExecuteNextAction(SLProtocol protocol)
		{
			if ((currentPollAction == null || currentPollAction.Done) && pollActions.Count > 0)
			{
				currentPollAction = pollActions.Dequeue();
				currentPollAction.Execute();
			}
			else if (currentPollAction != null && !currentPollAction.Done)
			{
				if (currentPollAction is EmberDiscoveryAction)
				{
					// Ember Tree discovery was not successful
					protocol.Log("QA" + protocol.QActionID + "|ExecuteNextAction|Ember Tree Polling Failed -> restarting action", LogType.Error, LogLevel.NoLogging);
					pollActions.Enqueue(new EmberDiscoveryAction(protocol, Configurations, pollActions));
				}

				currentPollAction = null;
				ExecuteNextAction(protocol);
			}
		}

		private void ExecuteNextAction2(SLProtocol protocol)
		{
			if ((currentPollAction == null || currentPollAction.Done) && pollActions.Count > 0)
			{
				currentPollAction = pollActions.Dequeue();
				currentPollAction.Execute();
			}
			else if (currentPollAction != null && !currentPollAction.Done && currentPollAction.Timeout)
			{
				if (!currentPollAction.RetriesExceeded)
				{
					protocol.Log("QA" + protocol.QActionID + "|ExecuteNextAction|Previous action was not executed in time -> retrying action" + currentPollAction, LogType.Information, LogLevel.NoLogging);
					currentPollAction.Continue();
				}
				else
				{
					protocol.Log("QA" + protocol.QActionID + "|ExecuteNextAction|Previous action was not executed in time and exceeded retries -> execute next action" + currentPollAction, LogType.Error, LogLevel.NoLogging);

					if (currentPollAction is EmberDiscoveryAction)
					{
						// Ember Tree discovery was not successful
						protocol.Log("QA" + protocol.QActionID + "|ExecuteNextAction|Ember Tree Polling Failed -> restarting action", LogType.Error, LogLevel.NoLogging);
						pollActions.Enqueue(new EmberDiscoveryAction(protocol, Configurations, pollActions));
					}

					currentPollAction = null;
					ExecuteNextAction2(protocol);
				}
			}
			else
			{
				// Do nothing.
			}
		}

		private void GlowReceived(SLProtocol protocol, EmberNode root)
		{
			if (!(root is GlowContainer glowContainer) || currentPollAction == null)
			{
				return;
			}

			bool isDiscoveryMode = currentPollAction is EmberDiscoveryAction;
			int[] validateLastRequestPath = isDiscoveryMode ? lastRequestedPath : Array.Empty<int>();
			int[] newLastRequestedPath = currentPollAction.ProcessReceivedGlow(parameterMapping, glowContainer, validateLastRequestPath);
			lastRequestedPath = newLastRequestedPath == Array.Empty<int>() ? lastRequestedPath : newLastRequestedPath;

			if (!currentPollAction.Done)
			{
				return;
			}

			if (isDiscoveryMode)
			{
				protocol.Log("QA" + protocol.QActionID + "|GlowReceived|Ember Discovery Done -> enqueue poll actions", LogType.Information, LogLevel.NoLogging);

				foreach (string[] path in Vendor.Device.Api.MyTree.Paths)
				{
					PollParameters(protocol, path);
				}
			}

			ExecuteNextAction(protocol);
		}

		private void HandleResponse(SLProtocol protocol, byte[] response)
		{
			try
			{
				glowReader.ReadBytes(response);
			}
			catch (Exception e)
			{
				protocol.Log("QA" + protocol.QActionID + "|HandleResponse|Error" + Environment.NewLine + e, LogType.Error, LogLevel.NoLogging);
			}
		}

		private void KeepAliveRequestReceived(SLProtocol protocol, FramingReader.KeepAliveRequestReceivedArgs e)
		{
			protocol.SetParameterBinary(Configurations.S101Pids.S101RequestDataPid, e.Response);
			protocol.CheckTrigger(Configurations.SendEmberRequestTrigger);
		}
	}
}