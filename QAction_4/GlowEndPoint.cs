namespace QAction_4
{
	using System;
	using System.Linq;
	using System.Net.Sockets;
	using BerLib;
	using EmberLib;
	using EmberLib.Framing;
	using EmberLib.Glow;
	using EmberLib.Glow.Framing;
	using QAction_1.Skyline;
	using Skyline.DataMiner.Net.Exceptions;
	using Skyline.DataMiner.Scripting;
	using Skyline.Protocol.Communication.Serial;

	public class GlowEndPoint : IDisposable
	{
		private static readonly Action<SLProtocolExt, byte[]> sendRequestAction = RequestHandler.SendRequest;

		private GlowReader glowReader;

		private int lastGlowFramingError = -1;

		/// <summary>
		///     Raised when a valid ember/glow package has been read.
		/// </summary>
		public event EventHandler<GlowRootReadyEventArgs> GlowRootReady;

		/// <summary>
		///     Raised when encountering errors.
		/// </summary>
		public event EventHandler<NotificationEventArgs> Notification;

		/// <summary>
		///     Sends an ember/glow tree to the remote host.
		/// </summary>
		/// <param name="protocol">Link with SLProtocol process.</param>
		/// <param name="glow">The root of the tree to send.</param>
		public static void Write(SLProtocolExt protocol, GlowContainer glow)
		{
			var glowOutPut = new GlowOutput(true, 1024, 0x00, (_, e) => sendRequestAction.Invoke(protocol, e.FramedPackage));

			using (glowOutPut)
			{
				glow.Encode(glowOutPut);
				glowOutPut.Finish();
			}
		}

		/// <summary>
		///     Synchronously connects to the remote host.
		/// </summary>
		/// <param name="protocol">Link with SLProtocol process.</param>
		/// <param name="trigger">Trigger that executes the QAction.</param>
		/// <returns>True if the connection attempt completed successfully, otherwise false.</returns>
		public bool Connect(SLProtocolExt protocol)
		{
			Close();

			try
			{
				if (glowReader == null)
				{
					glowReader = new GlowReader((_, e) => GlowReader_RootReady(protocol, e), (_, e) => GlowReader_KeepAliveRequestReceived(protocol, e));
					glowReader.PackageReceived += (_, e) => PackageReceived(protocol, e);
					glowReader.FramingError += (_, e) => GlowFramingError(protocol, e);
					glowReader.Error += (_, e) => GlowReader_Error(protocol, e);
				}

				return true;
			}
			catch (DataMinerParameterValueException e)
			{
				protocol.Log("QA" + protocol.QActionID + "|Connect|Error" + Environment.NewLine + e, LogType.Error, LogLevel.NoLogging);
			}
			catch (Exception e)
			{
				protocol.Log("QA" + protocol.QActionID + "|Connect|Error" + Environment.NewLine + e, LogType.Error, LogLevel.NoLogging);
			}

			return false;
		}

		public void Dispose()
		{
			Close();

			Notification = null;
		}

		protected virtual void OnGlowRootReady(SLProtocolExt protocol, GlowRootReadyEventArgs e)
		{
			GlowRootReady?.Invoke(protocol, e);
		}

		protected virtual void OnNotification(SLProtocolExt protocol, NotificationEventArgs e)
		{
			Notification?.Invoke(protocol, e);
		}

		/// <summary>
		///     Closes the endpoint.
		/// </summary>
		internal void Close()
		{
			glowReader?.Dispose();

			glowReader = null;
		}

		internal void HandleResponse(SLProtocolExt protocol, int trigger)
		{
			byte[] response = ReadResponse(protocol, trigger);

			try
			{
				glowReader.ReadBytes(response);
			}
			catch (BerException e)
			{
				// TODO: implement buffer mechanism for packet longer than 1024 bytes
			}
			catch (SocketException ex)
			{
				OnNotification(protocol, new NotificationEventArgs(ex.Message));
			}
			catch (ObjectDisposedException e)
			{
				protocol.Log("QA" + protocol.QActionID + "|HandleResponse|Error" + Environment.NewLine + e, LogType.Error, LogLevel.NoLogging);
			}
			catch (Exception e)
			{
				protocol.Log("QA" + protocol.QActionID + "|HandleResponse|Error" + Environment.NewLine + e, LogType.Error, LogLevel.NoLogging);
			}
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

		private static byte[] ReadResponse(SLProtocolExt protocol, int responsePid)
		{
			var receivedData = (object[])protocol.GetData("PARAMETER", responsePid);

			if (receivedData.Length == 0)
			{
				throw new DataMinerParameterValueException("Invalid value.");
			}

			return BuildFrameData(receivedData);
		}

		private void GlowFramingError(SLProtocolExt protocol, FramingReader.FramingErrorArgs e)
		{
			protocol.Log("QA" + protocol.QActionID + "|GlowFramingError", LogType.DebugInfo, LogLevel.NoLogging);
		}

		/// <summary>
		///     Invoked everytime _glowReader encounters an error in the inbound data.
		/// </summary>
		private void GlowReader_Error(SLProtocolExt protocol, GlowReader.ErrorArgs e)
		{
			if (e.ErrorCode == lastGlowFramingError)
			{
				return;
			}

			OnNotification(protocol, new NotificationEventArgs($"Error: {e.Message}"));

			lastGlowFramingError = e.ErrorCode;
		}

		/// <summary>
		///     Invoked everytime _glowReader has unframed a Keep-Alive request.
		/// </summary>
		private void GlowReader_KeepAliveRequestReceived(SLProtocolExt protocol, FramingReader.KeepAliveRequestReceivedArgs e)
		{
			protocol.Log("QA" + protocol.QActionID + "|GlowReader_KeepAliveRequestReceived", LogType.DebugInfo, LogLevel.NoLogging);
			protocol.SetParameterBinary(52, e.Response);
			protocol.CheckTrigger(50);
		}

		/// <summary>
		///     Invoked everytime _glowReader has decoded a complete Ember tree.
		/// </summary>
		private void GlowReader_RootReady(SLProtocolExt protocol, AsyncDomReader.RootReadyArgs e)
		{
			if (e.Root is GlowContainer root)
			{
				OnGlowRootReady(protocol, new GlowRootReadyEventArgs(root));
			}
			else
			{
				OnNotification(protocol, new NotificationEventArgs($"Unexpected Ember Root: {e.Root} ({e.Root.GetType()})"));
			}
		}

		private void PackageReceived(SLProtocolExt protocol, FramingReader.PackageReceivedArgs e)
		{
			// protocol.Log("QA" + protocol.QActionID + "|PackageReceived", LogType.DebugInfo, LogLevel.NoLogging);
		}
	}
}