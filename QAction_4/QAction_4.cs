using System;
using QAction_4;
using Skyline.DataMiner.Scripting;

/// <summary>
///     DataMiner QAction Class: Test Ember.
/// </summary>
public class QAction
{
	private EmberHandler emberHandler;

	/// <summary>
	///     The QAction entry point.
	/// </summary>
	/// <param name="protocol">Link with SLProtocol process.</param>
	public void Run(SLProtocolExt protocol)
	{
		try
		{
			int trigger = protocol.GetTriggerParameter();
			var endPoint = new GlowEndPoint();

			if (endPoint.Connect(protocol))
			{
				RunMain(protocol, endPoint, trigger);
			}

			endPoint.Close();
		}
		catch (Exception ex)
		{
			protocol.Log("QA" + protocol.QActionID + "|" + protocol.GetTriggerParameter() + "|Run|Exception thrown:" + Environment.NewLine + ex, LogType.Error, LogLevel.NoLogging);
		}
	}

	private void RunMain(SLProtocolExt protocol, GlowEndPoint endPoint, int trigger)
	{
		emberHandler = emberHandler ?? new EmberHandler();
		emberHandler.SetFields(protocol, endPoint);

		switch (trigger)
		{
			case 53:
				emberHandler.GlowEndPoint.HandleResponse(protocol, trigger);

				break;
			case 1:
				protocol.ClearAllKeys(Parameter.Requestednodestable.tablePid);
				emberHandler.elements.Clear();
				emberHandler.cursor = emberHandler.rootElement;
				var glow = emberHandler.rootElement.GetDirectory();
				GlowEndPoint.Write(protocol, glow);

				break;
		}
	}
}