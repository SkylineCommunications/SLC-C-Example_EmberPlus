namespace QAction_4
{
	using System.Collections.Generic;
	using System.Linq;
	using EmberLib.Glow;
	using QAction_1.Skyline;
	using Skyline.DataMiner.Scripting;
	using SLNetMessages = Skyline.DataMiner.Net.Messages;

	public class EmberHandler
	{
		internal readonly Queue<Element> elements;

		private readonly List<string> polledNodes;

		internal Element cursor;

		internal GlowEndPoint GlowEndPoint;

		internal Element rootElement;

		public EmberHandler()
		{
			polledNodes = new List<string>();
			elements = new Queue<Element>();
		}

		public void SetFields(SLProtocolExt protocol, GlowEndPoint endPoint)
		{
			GlowEndPoint = endPoint;
			rootElement = Element.CreateElement(protocol, null, 0, null, ElementType.Root);

			GlowEndPoint.GlowRootReady += (_, e) => OnGlowRootReady(protocol, e);

			GlowEndPoint.Notification += (_, e) => OnNotification(protocol, e);
		}

		internal static void UpdateDiscoveredNodes(SLProtocolExt protocol, Element element)
		{
			if (element.Identifier == null)
			{
				protocol.AddRow(Parameter.Requestednodestable.tablePid, new RequestednodestableQActionRow { Requestednodesidentifier_121 = "Root", Requestednodesnumber_122 = -1 });
			}
			else
			{
				protocol.AddRow(
					Parameter.Requestednodestable.tablePid,
					new RequestednodestableQActionRow { Requestednodesidentifier_121 = element.Identifier, Requestednodesnumber_122 = element.Number });
			}
		}

		internal void CheckNextElementInQueue(SLProtocolExt protocol)
		{
			if (!elements.Any())
			{
				return;
			}

			cursor = elements.Dequeue();

			polledNodes.Add(cursor.Identifier);
			var glow = cursor.GetDirectory();
			GlowEndPoint.Write(protocol, glow);
		}

		private static void NodeWithNodes(SLProtocolExt protocol, Element elem)
		{
			if (elem.Type == ElementType.Node)
			{
				protocol.Log("QA" + protocol.QActionID + "|OnGlowRootReady|Node|" + elem.PrintNode(), LogType.DebugInfo, LogLevel.NoLogging);
			}
		}

		private static void NodeWithParameters(SLProtocolExt protocol, Element elem)
		{
			if (elem.Type == ElementType.Parameter)
			{
				protocol.Log("QA" + protocol.QActionID + "|OnGlowRootReady|Parameter|" + elem.PrintParameter(), LogType.DebugInfo, LogLevel.NoLogging);
			}
		}

		private void OnGlowRootReady(SLProtocolExt protocol, GlowRootReadyEventArgs e)
		{
			cursor.Walk(e.Root);

			foreach (var nodeChild in cursor.Children.Where(x => x.Type == ElementType.Node).Select(x => x))
			{
				NodeWithParameters(protocol, nodeChild);
				NodeWithNodes(protocol, nodeChild);
				UpdateDiscoveredNodes(protocol, nodeChild);
				nodeChild.AddEmberElementToTable(protocol);

				if (protocol.Exists(Parameter.Requestednodestable.tablePid, nodeChild.Identifier))
				{
					continue;
				}

				elements.Enqueue(nodeChild);

				foreach (var child in nodeChild.Children.Where(x => x.Type == ElementType.Node).Select(x => x))
				{
					elements.Enqueue(child);
					// var glow = child.GetDirectory();
					// GlowEndPoint.Write(protocol, glow);
				}
			}

			CheckNextElementInQueue(protocol);
			//protocol.Log("QA" + protocol.QActionID + "|OnGlowRootReady|polledNodes: " + string.Join("|", polledNodes), LogType.DebugInfo, LogLevel.NoLogging);
		}

		private void OnNotification(SLProtocolExt protocol, NotificationEventArgs e)
		{
			protocol.Log("QA" + protocol.QActionID + "|OnNotification|Message\n" + e, LogType.Error, LogLevel.NoLogging);
		}
	}
}