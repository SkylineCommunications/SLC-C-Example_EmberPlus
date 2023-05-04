namespace QAction_1.Ember
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using EmberLib.Glow;
	using QAction_1.Ember.Protocol;
	using Skyline.DataMiner.Scripting;

	public class EmberPollAction : EmberAction
	{
		private readonly EmberData emberData;

		public EmberPollAction(SLProtocol protocol, EmberData emberData, Configuration configuration)
			: base(protocol, configuration)
		{
			LastExecutionTime = null;
			this.emberData = emberData;
			Retries = 0;
		}

		public override void Continue()
		{
			Retries++;
			Execute();
		}

		public override bool Equals(object obj)
		{
			if (!(obj is EmberPollAction other))
			{
				return false;
			}

			string joinedPaths = String.Join("/", emberData.ParameterPaths.Select(x => String.Join(".", x)));
			string otherJoinedPaths = String.Join("/", other.emberData.ParameterPaths.Select(x => String.Join(".", x)));

			return joinedPaths.Equals(otherJoinedPaths);
		}

		public override void Execute()
		{
			LastExecutionTime = DateTime.Now.ToOADate();
			SendGetDirectoryRequest(emberData.ParameterPaths);
		}

		public override int GetHashCode()
		{
			string joinedPaths = String.Join("/", emberData.ParameterPaths.Select(x => String.Join(".", x)));

			return joinedPaths.GetHashCode();
		}

		public override int[] ProcessReceivedGlow(EmberData emberData, GlowContainer glowContainer, int[] validateLastRequestPath)
		{
			Walk(glowContainer);
			LastExecutionTime = DateTime.Now.ToOADate();

			Done = true;
			HandlePolledInformation();

			return Array.Empty<int>();
		}

		protected override void OnNode(GlowNodeBase glow, int[] path)
		{
			// Check if Ember Tree contains an entry for this node.
			// It could be that the communication failed during the discovery and that one or more nodes were not discovered.
			var parentPath = new int[path.Length - 1];
			Array.Copy(path, parentPath, parentPath.Length);

			if (!emberData.ReverseEmberTree.ContainsKey(parentPath) || emberData.ReverseEmberTree.ContainsKey(path))
			{
				return;
			}

			string[] friendlyParentPath = emberData.ReverseEmberTree[parentPath];
			var friendlyNodePath = new string[friendlyParentPath.Length + 1];
			Array.Copy(friendlyParentPath, friendlyNodePath, friendlyParentPath.Length);
			friendlyNodePath[friendlyParentPath.Length] = glow.Identifier;

			emberData.ReverseEmberTree.Add(path, friendlyNodePath);
			emberData.EmberTree.Add(friendlyNodePath, path);

			// Add Parameter mappings if possible
			emberData.UpdateMappings();

			protocol.Log(
				"QA" + protocol.QActionID + "|EmberPollAction.OnNode|Added missing path: " + String.Join(".", friendlyNodePath) + "; Path:" + String.Join(".", path),
				LogType.Information,
				LogLevel.NoLogging);

			protocol.SetParameter(Configurations.DiscoveredNodesCountPid, emberData.EmberTree.Count);
		}

		protected override void OnParameter(GlowParameterBase glow, int[] path)
		{
			var parentPath = new int[path.Length - 1];
			Array.Copy(path, parentPath, parentPath.Length);

			if (!emberData.ReverseEmberTree.ContainsKey(parentPath))
			{
				return;
			}

			string[] friendlyParentPath = emberData.ReverseEmberTree[parentPath];
			var friendlyParameterPath = new string[friendlyParentPath.Length + 1];
			Array.Copy(friendlyParentPath, friendlyParameterPath, friendlyParentPath.Length);
			friendlyParameterPath[friendlyParentPath.Length] = glow.Identifier;

			if (emberData.EmberParameterMap.ContainsKey(friendlyParameterPath))
			{
				emberData.PolledParameters[friendlyParameterPath] = ConvertGlowValue(glow);
			}
		}

		private void FetchAndUpdate()
		{
			var standAloneParameters = new Dictionary<int, object>();

			foreach (var polledParameter in emberData.PolledParameters)
			{
				protocol.Log("QA" + protocol.QActionID + "|FetchAndUpdate|Value" + Convert.ToString(polledParameter.Value), LogType.DebugInfo, LogLevel.NoLogging);
				int parameterId = emberData.EmberParameterMap[polledParameter.Key];

				switch (parameterId)
				{
					// Choose table id for case
					default:
						if (!standAloneParameters.ContainsKey(parameterId))
						{
							standAloneParameters.Add(parameterId, polledParameter.Value);
						}

						break;
				}
			}

			UpdateStandaloneParameters(standAloneParameters);

			emberData.PolledParameters.Clear();
		}

		private void HandlePolledInformation()
		{
			if (emberData.PolledParameters != null && emberData.PolledParameters.Count > 0)
			{
				FetchAndUpdate();
			}
		}

		private void UpdateStandaloneParameters(Dictionary<int, object> polledParameters)
		{
			protocol.SetParameters(polledParameters.Keys.ToArray(), polledParameters.Values.ToArray());
		}
	}
}