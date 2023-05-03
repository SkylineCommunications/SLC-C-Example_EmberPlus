using System;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Lawo.EmberPlusSharp.Model;
using Lawo.EmberPlusSharp.S101;
using Lawo.Threading.Tasks;
using Skyline.DataMiner.Scripting;

/// <summary>
///     DataMiner QAction Class: Test Ember.
/// </summary>
public class QAction
{
	private readonly StringBuilder sb = new StringBuilder();

	/// <summary>
	///     The QAction entry point.
	/// </summary>
	/// <param name="protocol">Link with SLProtocol process.</param>
	public void Run(SLProtocolExt protocol)
	{
		try
		{
			var ipAndPort = Convert.ToString(protocol.GetParameter(2)).Split(':');

			var ip = ipAndPort[0];
			var port = Convert.ToInt32(ipAndPort[1]);

			// This is necessary so that we can execute async code in a console application.
			AsyncPump.Run(
				async () =>
				{
					// Establish S101 protocol
					using (var client = await ConnectAsync(ip, port))

					// Retrieve *all* elements in the provider database and store them in a local copy
					using (var consumer = await Consumer<MyRoot>.CreateAsync(client))
					{
						sb.AppendLine("<Node>");

						// protocol.Log("\n<Node>");
						WriteChildren(protocol, consumer.Root);

						//// Get the root of the local database.
						// INode root = consumer.Root;

						//// For now just output the number of direct children under the root node.
						// Console.WriteLine(root.Children.Count);
					}
				});

			protocol.Log(sb.ToString());
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

		return new S101Client(tcpClient, stream.ReadAsync, stream.WriteAsync);
	}

	private void WriteChildren(SLProtocolExt protocol, INode node)
	{
		foreach (var child in node.Children)
		{
			if (child is INode childNode)
			{
				sb.AppendLine($"<Node identifier=\"{child.Identifier}\" path=\"{child.Tag}\">");

				// protocol.Log($"\n<Node identifier=\"{child.Identifier}\" path=\"{child.Tag}\">");
				WriteChildren(protocol, childNode);
			}
			else
			{
				if (child is IParameter childParameter)
				{
					sb.AppendLine($"<Parameter identifier=\"{child.Identifier}\" value=\"{childParameter.Value}\" path=\"{childParameter.Tag}\"></Parameter>");

					// protocol.Log($"\n<Parameter identifier=\"{child.Identifier}\" value=\"{childParameter.Value}\" path=\"{childParameter.Tag}\"></Parameter>");
				}
			}
		}

		sb.AppendLine("</Node>");

		// protocol.Log("\n</Node>");
	}

	// Note that the most-derived subtype MyRoot needs to be passed to the generic base class.
	private sealed class MyRoot : DynamicRoot<MyRoot>
	{
	}
}