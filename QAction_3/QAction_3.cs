using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Lawo.EmberPlusSharp.Model;
using Lawo.EmberPlusSharp.S101;
using Lawo.Threading.Tasks;
using Newtonsoft.Json;
using Skyline.DataMiner.Scripting;

/// <summary>
///     DataMiner QAction Class: Ember QAction.
/// </summary>
public static class QAction
{
	/// <summary>
	///     The QAction entry point.
	/// </summary>
	/// <param name="protocol">Link with SLProtocol process.</param>
	public static void Run(SLProtocolExt protocol)
	{
		try
		{
			string[] ipAndPort = Convert.ToString(protocol.GetParameter(2)).Split(':');

			string ip = ipAndPort[0];
			var port = Convert.ToInt32(ipAndPort[1]);
			protocol.Log($"{ip}:{port}");

			// This is necessary so that we can execute async code in a console application.
			async Task AsyncMethod()
			{
				// Establish S101 protocol
				using (var client = await ConnectAsync(ip, port).ConfigureAwait(false))

				// Retrieve *all* elements in the provider database and store them in a local copy
				using (var consumer = await Consumer<MyRoot>.CreateAsync(client).ConfigureAwait(false))
				{
					WriteChildren(protocol, consumer.Root);

					consumer.Dispose();
					client.Dispose();
					protocol.DisposeResources();
				}
			}

			AsyncPump.Run(AsyncMethod, CancellationToken.None);
		}
		catch (Exception ex)
		{
			protocol.Log("QA" + protocol.QActionID + "|" + protocol.GetTriggerParameter() + "|Run|Exception thrown:" + Environment.NewLine + ex, LogType.Error, LogLevel.NoLogging);
		}
	}

	private static async Task<S101Client> ConnectAsync(string host, int port)
	{
		// Create TCP connection
		var tcpClient = new TcpClient();
		await tcpClient.ConnectAsync(host, port).ConfigureAwait(false);

		// Establish S101 protocol
		// S101 provides message packaging, CRC integrity checks and a keep-alive mechanism.
		var stream = tcpClient.GetStream();

		return new S101Client(tcpClient, (bytes, i, count, token) => stream.ReadAsync(bytes, i, count, token), (bytes, i, count, token) => stream.WriteAsync(bytes, i, count, token));
	}

	private static object GetValue(IParameter childParameter)
	{
		var type = childParameter.Type;

		switch (type)
		{
			case ParameterType.Integer:
				return childParameter.Factor != null && childParameter.Factor != 0 ? (double)childParameter.Value / childParameter.Factor : Convert.ToInt32(childParameter.Value);
			case ParameterType.Real:
				return Convert.ToDouble(childParameter.Value);
			case ParameterType.String:
				return Convert.ToString(childParameter.Value).Trim();
			case ParameterType.Boolean:
				return Convert.ToInt32(childParameter.Value);
			case ParameterType.Trigger:
				return childParameter.Value;
			case ParameterType.Enum:
				return Convert.ToInt32(childParameter.Value);
			case ParameterType.Octets:
				return Encoding.ASCII.GetString((byte[])childParameter.Value);
			default:
				return new ArgumentOutOfRangeException($"Type: {type}, Value: {Convert.ToString(childParameter.Value)}");
		}
	}

	private static void ProcessChildFunction(SLProtocolExt protocol, IFunction childFunction)
	{
		var functionRow = new EmberfunctionstableQActionRow
		{
			Emberfunctionsidentifier_301 = childFunction.Identifier,
			Emberfunctionsnumber_302 = childFunction.Number,
			Emberfunctionsparent_303 = childFunction.Parent.Identifier,
			Emberfunctionsidentifierpath_304 = childFunction.IdentifierPath,
			Emberfunctionsjoinedpath_305 = String.Join(".", childFunction.Path),
			Emberfunctionsdescription_306 = childFunction.Description,
			Emberfunctionsstate_307 = Convert.ToInt32(childFunction.IsOnline),
			Emberfunctionsjoinedidentifierpath_308 = childFunction.GetPath(),
			Emberfunctionsarguments_309 = JsonConvert.SerializeObject(childFunction.Arguments),
			Emberfunctionsresult_310 = JsonConvert.SerializeObject(childFunction.Result),
		};

		protocol.emberfunctionstable.SetRow(functionRow, true);
	}

	private static void ProcessChildMatrix(SLProtocolExt protocol, IMatrix childMatrix)
	{
		var matrixRow = new EmbermatrixtableQActionRow
		{
			Embermatrixidentifier_201 = childMatrix.Identifier,
			Embermatrixnumber_202 = childMatrix.Number,
			Embermatrixparent_203 = childMatrix.Parent.Identifier,
			Embermatrixidentifierpath_204 = childMatrix.IdentifierPath,
			Embermatrixjoinedpath_205 = String.Join(".", childMatrix.Path),
			Embermatrixdescription_206 = childMatrix.Description,
			Embermatrixstate_207 = Convert.ToInt32(childMatrix.IsOnline),
			Embermatrixjoinedidentifierpath_208 = childMatrix.GetPath(),
			Embermatrixmaximumtotalconnects_209 = childMatrix.MaximumTotalConnects,
			Embermatrixmaximumconnectspertarget_210 = childMatrix.MaximumConnectsPerTarget,
			Embermatrixparameterslocations_211 = JsonConvert.SerializeObject(childMatrix.ParametersLocation),
			Embermatrixgainparameternumber_212 = childMatrix.GainParameterNumber,
			Embermatrixlabels_213 = JsonConvert.SerializeObject(childMatrix.Labels),
			Embermatrixtargets_214 = JsonConvert.SerializeObject(childMatrix.Targets),
			Embermatrixsources_215 = JsonConvert.SerializeObject(childMatrix.Sources),
			Embermatrixconnections_216 = JsonConvert.SerializeObject(childMatrix.Connections),
		};

		protocol.embermatrixtable.SetRow(matrixRow, true);
	}

	private static void ProcessChildNode(SLProtocolExt protocol, INode childNode)
	{
		var nodeRow = new EmbernodestableQActionRow
		{
			Embernodesidentifier_101 = childNode.Identifier,
			Embernodesnumber_102 = childNode.Number,
			Embernodesparent_103 = childNode.Parent.Identifier,
			Embernodesidentifierpath_104 = childNode.IdentifierPath,
			Embernodesjoinedpath_105 = String.Join(".", childNode.Path),
			Embernodesdescription_106 = childNode.Description,
			Embernodesstate_107 = Convert.ToInt32(childNode.IsOnline),
			Embernodesjoinedidentifierpath_108 = childNode.GetPath(),
		};

		protocol.embernodestable.SetRow(nodeRow, true);

		WriteChildren(protocol, childNode);
	}

	private static void ProcessChildParameter(SLProtocolExt protocol, IParameter childParameter)
	{
		object value = GetValue(childParameter);

		var parameterRow = new EmberparameterstableQActionRow
		{
			Emberparametersidentifier_111 = childParameter.Identifier,
			Emberparametersnumber_112 = childParameter.Number,
			Emberparametersparent_113 = childParameter.Parent.Identifier,
			Emberparametersidentifierpath_114 = childParameter.IdentifierPath,
			Emberparametersjoinedpath_115 = String.Join(".", childParameter.Path),
			Emberparametersdescription_116 = childParameter.Description,
			Emberparametersstate_117 = Convert.ToInt32(childParameter.IsOnline),
			Emberparametersjoinedidentifierpath_118 = childParameter.GetPath(),
			Emberparameterstype_119 = childParameter.Type,
			Emberparametersvalue_120 = Convert.ToString(value),
			Emberparametersminimum_121 = Convert.ToInt32(childParameter.Minimum),
			Emberparametersmaximum_122 = Convert.ToInt32(childParameter.Maximum),
			Emberparametersaccess_123 = childParameter.Access.ToString(),
			Emberparametersiswritable_124 = Convert.ToInt32(childParameter.IsWriteable),
			Emberparametersformat_125 = childParameter.Format,
			Emberparametersfactor_126 = childParameter.Factor,
			Emberparametersformula_127 = childParameter.Formula,
			Emberparametersdefaultvalue_128 = childParameter.DefaultValue,
			Emberparametersenummap_129 = JsonConvert.SerializeObject(childParameter.EnumMap),
		};

		protocol.emberparameterstable.SetRow(parameterRow, true);
	}

	private static void WriteChildren(SLProtocolExt protocol, INode node)
	{
		foreach (var child in node.Children)
		{
			switch (child)
			{
				case IMatrix childMatrix:
					ProcessChildMatrix(protocol, childMatrix);

					break;

				case INode childNode:
					ProcessChildNode(protocol, childNode);

					break;

				case IFunction childFunction:
					ProcessChildFunction(protocol, childFunction);

					break;

				case IParameter childParameter:
					ProcessChildParameter(protocol, childParameter);

					break;
				default:
					throw new ArgumentOutOfRangeException($"Type: {child.GetType()}");
			}
		}
	}

	// Note that the most-derived subtype MyRoot needs to be passed to the generic base class.
	private sealed class MyRoot : DynamicRoot<MyRoot>
	{
	}
}