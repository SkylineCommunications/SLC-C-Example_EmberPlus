using System;
using QAction_1.Skyline.Ember;
using QAction_1.Skyline.Ember.Protocol;
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
			protocol.Log(Convert.ToString(protocol.GetParameter(Parameter.s101responsedata_53)));

			int trigger = protocol.GetTriggerParameter();

			if (emberHandler == null)
			{
				var s101Params = new S101Params(Parameter.s101requestdata_52);
				configurations = new Configuration(Parameter.discoverembertree_1, s101Params, 50, Parameter.discoverednodescount_60, Parameter.nodediscoveryprogress_61);

				emberHandler = new EmberHandler(configurations);
			}

			if (trigger == Parameter.polltables_10 /*pollParameters*/)
			{
				foreach (string[] path in Vendor.Device.Api.MyTree.Paths)
				{
					emberHandler.PollParameters(protocol, path);
				}
			}
			else if (trigger == configurations.DiscoverEmberTreePid)
			{
				emberHandler.DiscoverEmberTree(protocol);
			}
			else if (trigger == Parameter.s101responsedata_53)
			{
				emberHandler.ResponseReceived(protocol, trigger);
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