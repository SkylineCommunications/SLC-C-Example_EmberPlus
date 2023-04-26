namespace QAction_4
{
	using System;
	using System.Linq;
	using System.Net.Sockets;
	using EmberLib;
	using EmberLib.Framing;
	using EmberLib.Glow;
	using EmberLib.Glow.Framing;
	using QAction_1.Skyline;
	using Skyline.DataMiner.Net.Exceptions;
	using Skyline.DataMiner.Scripting;
	using Skyline.Protocol.Communication.Serial;

	public class GlowEndPoint : GlowWalker, IDisposable
	{
		private static readonly Action<SLProtocol, byte[]> sendRequestAction = RequestHandler.SendRequest;

		private GlowReader glowReader;

		private int lastGlowFramingError = -1;

		public GlowEndPoint(SLProtocol protocol)
		{
			Protocol = protocol;
		}

		/// <summary>
		///     Raised when a valid ember/glow package has been read.
		/// </summary>
		public event EventHandler<GlowRootReadyEventArgs> GlowRootReady;

		/// <summary>
		///     Raised when encountering errors.
		/// </summary>
		public event EventHandler<NotificationEventArgs> Notification;

		private SLProtocol Protocol { get; }

		/// <summary>
		///     Sends an ember/glow tree to the remote host.
		/// </summary>
		/// <param name="protocol">Link with SLProtocol process.</param>
		/// <param name="glow">The root of the tree to send.</param>
		public static void Write(SLProtocol protocol, GlowContainer glow)
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
		public bool Connect(SLProtocol protocol)
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

		protected override void OnCommand(GlowCommand glow, int[] path)
		{
			Protocol.Log("QA" + Protocol.QActionID + "|MethodName|OnCommand", LogType.DebugInfo, LogLevel.NoLogging);
		}

		protected override void OnFunction(GlowFunctionBase glow, int[] path)
		{
			Protocol.Log("QA" + Protocol.QActionID + "|MethodName|OnFunction", LogType.DebugInfo, LogLevel.NoLogging);
		}

		protected virtual void OnGlowRootReady(SLProtocol protocol, GlowRootReadyEventArgs e)
		{
			GlowRootReady?.Invoke(protocol, e);
		}

		protected override void OnInvocationResult(GlowInvocationResult glow)
		{
			Protocol.Log("QA" + Protocol.QActionID + "|MethodName|OnInvocationResult", LogType.DebugInfo, LogLevel.NoLogging);
		}

		protected override void OnMatrix(GlowMatrixBase glow, int[] path)
		{
			Protocol.Log("QA" + Protocol.QActionID + "|MethodName|OnMatrix", LogType.DebugInfo, LogLevel.NoLogging);
		}

		protected override void OnNode(GlowNodeBase glow, int[] path)
		{
			Protocol.Log("QA" + Protocol.QActionID + "|MethodName|OnNode", LogType.DebugInfo, LogLevel.NoLogging);
		}

		protected virtual void OnNotification(SLProtocol protocol, NotificationEventArgs e)
		{
			Notification?.Invoke(protocol, e);
		}

		protected override void OnParameter(GlowParameterBase glow, int[] path)
		{
			Protocol.Log("QA" + Protocol.QActionID + "|MethodName|OnParameter", LogType.DebugInfo, LogLevel.NoLogging);
		}

		protected override void OnStreamEntry(GlowStreamEntry glow)
		{
			Protocol.Log("QA" + Protocol.QActionID + "|MethodName|OnStreamEntry", LogType.DebugInfo, LogLevel.NoLogging);
		}

		protected override void OnTemplate(GlowTemplateBase glow, int[] path)
		{
			Protocol.Log("QA" + Protocol.QActionID + "|MethodName|OnTemplate", LogType.DebugInfo, LogLevel.NoLogging);
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

		private static GlowContainer GetNextNode(GlowElement glow, int[] path)
		{
			var qnode = new GlowQualifiedNode(path)
			{
				Children = new GlowElementCollection(GlowTags.QualifiedNode.Children),
			};

			qnode.Children.Insert(glow);
			GlowElement qualified = qnode;

			var root = GlowRootElementCollection.CreateRoot();
			root.Insert(qualified);

			return root;
		}

		private static byte[] ReadResponse(SLProtocol protocol, int responsePid)
		{
			var receivedData = (object[])protocol.GetData("PARAMETER", responsePid);

			if (receivedData.Length == 0)
			{
				throw new DataMinerParameterValueException("Invalid value.");
			}

			return BuildFrameData(receivedData);
		}

		/// <summary>
		///     Closes the endpoint.
		/// </summary>
		internal void Close()
		{
			glowReader?.Dispose();

			glowReader = null;
		}

		private void GlowFramingError(SLProtocol protocol, FramingReader.FramingErrorArgs e)
		{
			protocol.Log("QA" + protocol.QActionID + "|MethodName|GlowFramingError", LogType.DebugInfo, LogLevel.NoLogging);
		}

		/// <summary>
		///     Invoked everytime _glowReader encounters an error in the inbound data.
		/// </summary>
		private void GlowReader_Error(SLProtocol protocol, GlowReader.ErrorArgs e)
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
		private void GlowReader_KeepAliveRequestReceived(SLProtocol protocol, FramingReader.KeepAliveRequestReceivedArgs e)
		{
			protocol.SetParameterBinary(52, e.Response);
			protocol.CheckTrigger(50);
		}

		/// <summary>
		///     Invoked everytime _glowReader has decoded a complete Ember tree.
		/// </summary>
		private void GlowReader_RootReady(SLProtocol protocol, AsyncDomReader.RootReadyArgs e)
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

		internal void HandleResponse(SLProtocol protocol, int trigger)
		{
			byte[] response = ReadResponse(protocol, trigger);

			try
			{
				glowReader.ReadBytes(response);
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

		private void PackageReceived(SLProtocol protocol, FramingReader.PackageReceivedArgs e)
		{
			protocol.Log("QA" + protocol.QActionID + "|MethodName|PackageReceived", LogType.DebugInfo, LogLevel.NoLogging);
		}
	}
}