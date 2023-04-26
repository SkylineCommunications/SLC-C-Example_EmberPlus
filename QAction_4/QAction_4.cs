using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using Newtonsoft.Json;
using QAction_1.Skyline;
using QAction_4;
using Skyline.DataMiner.Net;
using Skyline.DataMiner.Scripting;

/// <summary>
///     DataMiner QAction Class: Test Ember.
/// </summary>
public class QAction
{
	private readonly GlowEndPoint glowEndPoint;

	private Element rootElement;
	private Element cursor;

	/// <summary>
	///     Constructs the single instance of the Program class.
	/// </summary>
	/// <param name="endPoint">Connected GlowEndPoint.</param>
	private QAction(SLProtocol protocol, GlowEndPoint endPoint)
	{
		glowEndPoint = endPoint;

		rootElement = Element.CreateRoot(null, 0, null, ElementType.Root, null);
		cursor = rootElement;

		glowEndPoint.GlowRootReady += (_, e) => OnGlowRootReady(protocol, e);

		glowEndPoint.Notification += (_, e) => OnNotification(protocol, e);
	}

	/// <summary>
	///     The QAction entry point.
	/// </summary>
	/// <param name="protocol">Link with SLProtocol process.</param>
	public static void Run(SLProtocol protocol)
	{
		try
		{
			int trigger = protocol.GetTriggerParameter();
			var endPoint = new GlowEndPoint(protocol);

			if (endPoint.Connect(protocol))
			{
				var program = new QAction(protocol, endPoint);

				program.RunMain(protocol, trigger);
			}

			endPoint.Close();
		}
		catch (Exception ex)
		{
			protocol.Log("QA" + protocol.QActionID + "|" + protocol.GetTriggerParameter() + "|Run|Exception thrown:" + Environment.NewLine + ex, LogType.Error, LogLevel.NoLogging);
		}
	}

	private void RunMain(SLProtocol protocol, int trigger)
	{
		switch (trigger)
		{
			case 53:
				glowEndPoint.HandleResponse(protocol, trigger);

				break;
			case 1:
				GlowEndPoint.Write(protocol, rootElement.GetDirectory());

				break;
		}
	}

	private void OnNotification(SLProtocol protocol, NotificationEventArgs e)
	{
		protocol.Log("QA" + protocol.QActionID + "|OnNotification|Message\n" + e, LogType.Error, LogLevel.NoLogging);
	}

	private void OnGlowRootReady(SLProtocol protocol, GlowRootReadyEventArgs e)
	{
		if (!e.Root.Accept(rootElement, null))
		{
			return;
		}

		foreach (var elem in cursor.Children)
		{
			protocol.Log("QA" + protocol.QActionID + "|OnGlowRootReady|Message\n" + elem.PrintElement(), LogType.Error, LogLevel.NoLogging);
			//protocol.Log("QA" + protocol.QActionID + "|OnGlowRootReady|Message\n" + JsonConvert.SerializeObject(elem), LogType.Error, LogLevel.NoLogging);
			// GlowEndPoint.Write(Protocol, elem.GetDirectory());
		}

		WritePrompt(protocol);
	}

	/// <summary>
	///     Writes out an input prompt to the console, including the current cursor's path.
	/// </summary>
	private void WritePrompt(SLProtocol protocol)
	{
		var elem = cursor;
		var idents = new LinkedList<string>();

		while (elem.Type != ElementType.Root)
		{
			idents.AddFirst(elem.Identifier);
			elem = elem.Parent;
		}

		var buffer = new StringBuilder();
		var index = 0;

		foreach (string ident in idents)
		{
			if (index > 0)
			{
				buffer.Append("/");
			}

			buffer.Append(ident);
			index++;
		}

		protocol.Log("QA" + protocol.QActionID + "|WritePrompt|" + buffer, LogType.DebugInfo, LogLevel.NoLogging);
	}
}