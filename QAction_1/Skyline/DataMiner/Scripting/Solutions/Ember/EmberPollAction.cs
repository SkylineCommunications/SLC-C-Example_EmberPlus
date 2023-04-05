namespace QAction_1.Skyline.DataMiner.Scripting.Solutions.Ember
{
	using System;
	using System.Linq;
	using EmberLib.Glow;
	using global::Skyline.DataMiner.Scripting;
	using Newtonsoft.Json;
	using QAction_1.Skyline.Ember.Protocol;

	public class EmberPollAction : EmberAction
	{
		public EmberPollAction(SLProtocol protocol, EmberData emberData, Configuration configuration)
			: base(protocol, configuration, emberData)
		{
		}

		public override bool Equals(object obj)
		{
			var other = obj as EmberPollAction;

			if (other == null)
			{
				return false;
			}

			string joinedPaths = String.Join("/", EmberData.ParameterPaths.Select(x => String.Join(".", x)));
			string otherJoinedPaths = String.Join("/", other.EmberData.ParameterPaths.Select(x => String.Join(".", x)));

			return joinedPaths.Equals(otherJoinedPaths);
		}

		public override void Execute()
		{
			SendGetDirectoryRequest(EmberData.ParameterPaths);
		}

		public override int GetHashCode()
		{
			string joinedPaths = String.Join("/", EmberData.ParameterPaths.Select(x => String.Join(".", x)));

			return joinedPaths.GetHashCode();
		}

		public override string ProcessReceivedGlow(EmberData node, GlowContainer glowContainer, string validateLastRequestPath)
		{
			Walk(glowContainer);

			Done = true;
			HandlePolledInformation();

			return null;
		}

		protected override void OnNode(GlowNodeBase glow, int[] path)
		{
			// Check if Ember Tree contains an entry for this node.
			// It could be that the communication failed during the discovery and that one or more nodes were not discovered.
			var parentPath = new int[path.Length - 1];
			Array.Copy(path, parentPath, parentPath.Length);
			string joinedParentPath = String.Join(".", parentPath);
			string joinedNodePath = String.Join(".", path);

			if (EmberData.ReverseEmberTree.ContainsKey(joinedParentPath) && !EmberData.ReverseEmberTree.ContainsKey(joinedNodePath))
			{
				string[] friendlyParentPath = EmberData.ReverseEmberTree[joinedParentPath];
				var friendlyNodePath = new string[friendlyParentPath.Length + 1];
				Array.Copy(friendlyParentPath, friendlyNodePath, friendlyParentPath.Length);
				friendlyNodePath[friendlyParentPath.Length] = glow.Identifier;
				string joinedFriendlyNodePath = String.Join(".", friendlyNodePath);

				EmberData.ReverseEmberTree.Add(joinedNodePath, friendlyNodePath);
				EmberData.EmberTree.Add(joinedFriendlyNodePath, path);

				protocol.Log("QA" + protocol.QActionID + "|EmberPollAction.OnNode|Added missing path: " + joinedFriendlyNodePath + "; Path:" + joinedNodePath, LogType.Information, LogLevel.NoLogging);
				protocol.SetParameter(Configurations.DiscoveredNodesCountPid, EmberData.EmberTree.Count);
			}
		}

		protected override void OnParameter(GlowParameterBase glow, int[] path)
		{
			var parentPath = new int[path.Length - 1];
			Array.Copy(path, parentPath, parentPath.Length);
			string joinedParentPath = String.Join(".", parentPath);

			if (!EmberData.ReverseEmberTree.ContainsKey(joinedParentPath))
			{
				return;
			}

			string[] friendlyParentPath = EmberData.ReverseEmberTree[joinedParentPath];
			var friendlyParameterPath = new string[friendlyParentPath.Length + 1];
			Array.Copy(friendlyParentPath, friendlyParameterPath, friendlyParentPath.Length);
			friendlyParameterPath[friendlyParentPath.Length] = glow.Identifier;

			EmberData.PolledParameters[friendlyParameterPath] = ConvertGlowValue(glow);
		}

		private void FetchAndUpdate()
		{
			protocol.Log("QA" + protocol.QActionID + "|polledParameters|\n" + JsonConvert.SerializeObject(EmberData.PolledParameters), LogType.DebugInfo, LogLevel.NoLogging);
			protocol.Log("QA" + protocol.QActionID + "|EmberTree|\n" + JsonConvert.SerializeObject(EmberData.EmberTree), LogType.DebugInfo, LogLevel.NoLogging);
			protocol.Log("QA" + protocol.QActionID + "|ReverseEmberTree|\n" + JsonConvert.SerializeObject(EmberData.ReverseEmberTree), LogType.DebugInfo, LogLevel.NoLogging);

			foreach (var polledParameter in EmberData.PolledParameters)
			{
			}

			EmberData.PolledParameters.Clear();
		}

		private void HandlePolledInformation()
		{
			if (EmberData.PolledParameters != null && EmberData.PolledParameters.Count > 0)
			{
				FetchAndUpdate();
			}
		}
	}
}