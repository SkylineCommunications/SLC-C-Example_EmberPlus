using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Lawo.EmberPlusSharp.Model;
using Lawo.EmberPlusSharp.S101;
using Lawo.Threading.Tasks;
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
				using (var client = await ConnectAsync(ip, port))

					// Retrieve *all* elements in the provider database and store them in a local copy
				using (var consumer = await Consumer<MyRoot>.CreateAsync(client))
				{
					WriteChildren(protocol, consumer.Root);

					//// Get the root of the local database.
					// INode root = consumer.Root;

					consumer.Dispose();
					client.Dispose();
					//// For now just output the number of direct children under the root node.
					// Console.WriteLine(root.Children.Count);
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
		await tcpClient.ConnectAsync(host, port);

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
				return new ArgumentOutOfRangeException();
		}
	}

	private static void WriteChildren(SLProtocolExt protocol, INode node)
	{
		foreach (var child in node.Children)
		{
			if (child is INode childNode)
			{
				var nodeRow = new EmbernodestableQActionRow
				{
					Embernodesidentifier_101 = childNode.Identifier,
					Embernodesnumber_102 = childNode.Number,
					Embernodesparent_103 = childNode.Parent.Identifier,
					Embernodespath_104 = childNode.GetPath(),
				};

				protocol.embernodestable.SetRow(nodeRow, true);

				WriteChildren(protocol, childNode);
			}

			if (child is IParameter childParameter)
			{
				object value = GetValue(childParameter);

				var parameterRow = new EmberparameterstableQActionRow
				{
					Emberparametersidentifier_111 = childParameter.Identifier,
					Emberparametersnumber_112 = childParameter.Number,
					Emberparametersparent_113 = childParameter.Parent.Identifier,
					Emberparameterstype_114 = childParameter.Type,
					Emberparametersvalue_115 = Convert.ToString(value),
					Emberparameterspath_116 = childParameter.GetPath(),
				};

				protocol.emberparameterstable.SetRow(parameterRow, true);
			}
		}
	}

	// Note that the most-derived subtype MyRoot needs to be passed to the generic base class.
	private sealed class MyRoot : DynamicRoot<MyRoot>
	{
	}
}