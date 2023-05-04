namespace QAction_1.Ember
{
	using System;
	using System.Linq;
	using System.Text;
	using EmberLib;
	using EmberLib.Framing;
	using EmberLib.Glow;
	using EmberLib.Glow.Framing;
	using QAction_1.Ember.Protocol;
	using Skyline.DataMiner.Scripting;

	public class EmberHandler
	{
		private readonly SLProtocol protocol;

		private EmberAction currentPollAction;

		private GlowReader glowReader;

		private int[] lastRequestedPath = Array.Empty<int>();

		public EmberHandler(
			SLProtocol protocol,
			Configuration configurations)
		{
			this.protocol = protocol;
			Configurations = configurations;
			EmberData = new EmberData();
		}

		private Configuration Configurations { get; }

		private EmberData EmberData { get; }

		public void DiscoverEmberTree()
		{
			EmberData.PollActions.Enqueue(new EmberDiscoveryAction(protocol, Configurations));
			ExecuteNextAction();
		}

		public void PollParameters(string[] parameterPath)
		{
			EmberData.SetParameterPaths(parameterPath);

			if (EmberData.ParameterPaths.GetLength(0) > 0)
			{
				EmberData.PollActions.Enqueue(new EmberPollAction(protocol, EmberData, Configurations));
			}

			ExecuteNextAction();
		}

		public void ResponseReceived(object receivedData)
		{
			if (glowReader == null)
			{
				glowReader = new GlowReader((_, e) => GlowReceived(e.Root), (_, e) => KeepAliveRequestReceived(e));
				glowReader.PackageReceived += (_, e) => PackageReceived(e);
				glowReader.FramingError += (_, e) => GlowFramingError(e.Message);
				glowReader.Error += (_, e) => GlowError(e.Message);
			}

			HandleResponse(receivedData);
		}

		private void ExecuteNextAction()
		{
			if ((currentPollAction == null || currentPollAction.Done) && EmberData.PollActions.Count > 0)
			{
				currentPollAction = EmberData.PollActions.Dequeue();
				currentPollAction.Execute();
			}
			else if (currentPollAction != null && !currentPollAction.Done && currentPollAction.Timeout)
			{
				if (!currentPollAction.RetriesExceeded)
				{
					protocol.Log(
						"QA" + protocol.QActionID + "|ExecuteNextAction|Previous action was not executed in time -> retrying action: " + currentPollAction,
						LogType.Information,
						LogLevel.NoLogging);

					currentPollAction.Continue();
				}
				else
				{
					protocol.Log(
						"QA" + protocol.QActionID + "|ExecuteNextAction|Previous action was not executed in time and exceeded retries -> execute next action: " + currentPollAction,
						LogType.Error,
						LogLevel.NoLogging);

					if (currentPollAction is EmberDiscoveryAction)
					{
						// Ember Tree discovery was not successful
						protocol.Log("QA" + protocol.QActionID + "|ExecuteNextAction|Ember Tree Polling Failed -> restarting action", LogType.Error, LogLevel.NoLogging);
						EmberData.PollActions.Enqueue(new EmberDiscoveryAction(protocol, Configurations));
					}

					currentPollAction = null;
					ExecuteNextAction();
				}
			}
		}

		private void GlowError(string message)
		{
			protocol.Log("QA" + protocol.QActionID + "|GlowError|Message: " + message, LogType.Error, LogLevel.NoLogging);
		}

		private void GlowFramingError(string message)
		{
			protocol.Log("QA" + protocol.QActionID + "|GlowFramingError|Message: " + message, LogType.Error, LogLevel.NoLogging);
		}

		private void GlowReceived(EmberNode root)
		{
			if (!(root is GlowContainer glowContainer) || currentPollAction == null)
			{
				return;
			}

			bool isDiscoveryMode = currentPollAction is EmberDiscoveryAction;
			int[] validateLastRequestPath = isDiscoveryMode ? lastRequestedPath : Array.Empty<int>();
			int[] newLastRequestedPath = currentPollAction.ProcessReceivedGlow(EmberData, glowContainer, validateLastRequestPath);
			lastRequestedPath = newLastRequestedPath == Array.Empty<int>() ? lastRequestedPath : newLastRequestedPath;

			if (!currentPollAction.Done)
			{
				return;
			}

			if (isDiscoveryMode)
			{
				protocol.Log("QA" + protocol.QActionID + "|GlowReceived|Ember Discovery Done -> enqueue poll actions", LogType.Information, LogLevel.NoLogging);

				foreach (string[] path in ParameterMapping.Paths)
				{
					PollParameters(path);
				}
			}

			ExecuteNextAction();
		}

		private void HandleResponse(object receivedData)
		{
			if (receivedData == null)
			{
				protocol.Log("QA" + protocol.QActionID + "|HandleResponse|ERR: Received data is invalid!", LogType.Error, LogLevel.NoLogging);

				return;
			}

			var objectArray = (object[])receivedData;

			if (objectArray.Length == 0)
			{
				protocol.Log("QA" + protocol.QActionID + "|HandleResponse|ERR: Received data is invalid! Content:\n" + String.Join(".", objectArray.Select(Convert.ToString)), LogType.Error, LogLevel.NoLogging);

				return;
			}

			try
			{
				byte[] data = objectArray.Select(Convert.ToByte).ToArray();

				var framedData = new byte[data.Length + 2];
				framedData[0] = 0xFE;
				Array.Copy(data, 0, framedData, 1, data.Length);
				framedData[framedData.Length - 1] = 0xFF;

				glowReader.ReadBytes(framedData);
			}
			catch (Exception e)
			{
				protocol.Log("QA" + protocol.QActionID + "|HandleResponse|Error" + Environment.NewLine + e, LogType.Error, LogLevel.NoLogging);
			}
		}

		private void KeepAliveRequestReceived(FramingReader.KeepAliveRequestReceivedArgs e)
		{
			protocol.SetParameterBinary(Configurations.S101Pids.S101RequestDataPid, e.Response);
			protocol.CheckTrigger(Configurations.SendEmberRequestTrigger);
		}

		private void PackageReceived(FramingReader.PackageReceivedArgs e)
		{
			// protocol.Log("QA" + protocol.QActionID + "|Package Received|MessageId " + e.MessageId, LogType.Information, LogLevel.NoLogging);
		}
	}
}