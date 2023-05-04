using System;
using QAction_1.Ember;
using QAction_1.Ember.Protocol;
using Skyline.DataMiner.Scripting;

/// <summary>
///     DataMiner QAction Class: Handle Ember Requests and Responses.
/// </summary>
public class QAction
{
	private Configuration configurations;

	private EmberHandler emberHandler;

	/// <summary>
	///     The QAction entry point.
	/// </summary>
	/// <param name="protocol">Link with SLProtocol process.</param>
	public void Run(SLProtocol protocol)
	{
		try
		{
			int trigger = protocol.GetTriggerParameter();

			if (emberHandler == null)
			{
				var s101Params = new S101Params(50, 51, Parameter.s101requestdata_52, Parameter.s101responsedata_53);
				configurations = new Configuration(Parameter.discoverembertree_1, s101Params, 50, Parameter.discoverednodescount_60, Parameter.nodediscoveryprogress_61);

				emberHandler = new EmberHandler(protocol, configurations);
			}

			if (trigger == 11 /*pollParameters*/)
			{
				foreach (string[] path in ParameterMapping.Paths)
				{
					emberHandler.PollParameters(path);
				}
			}
			else if (trigger == configurations.DiscoverEmberTreePid)
			{
				emberHandler.DiscoverEmberTree();
			}
			else if (trigger == configurations.S101Pids.S101ResponseDataPid)
			{
				object receivedData = protocol.GetData("PARAMETER", trigger);
				emberHandler.ResponseReceived(receivedData);
			}
			else
			{
				protocol.Log("QA" + protocol.QActionID + "|Run|Trigger not implemented: " + trigger, LogType.Error, LogLevel.NoLogging);
			}
		}
		catch (Exception ex)
		{
			protocol.Log("QA" + protocol.QActionID + "|" + protocol.GetTriggerParameter() + "|Run|Exception thrown:" + Environment.NewLine + ex, LogType.Error, LogLevel.NoLogging);
		}
	}
}