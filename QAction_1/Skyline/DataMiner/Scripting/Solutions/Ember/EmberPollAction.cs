namespace QAction_1.Skyline.DataMiner.Scripting.Solutions.Ember
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using EmberLib.Glow;
	using global::Skyline.DataMiner.Scripting;
	using Newtonsoft.Json;
	using QAction_1.Skyline.Ember.Protocol;

	public class EmberPollAction : EmberAction
	{
		private readonly ParameterMapping parameterMapping;

		private readonly int[][] paths;

		private readonly Dictionary<string[], object> polledParameters = new Dictionary<string[], object>();

		private readonly SLProtocol protocol;

		public EmberPollAction(SLProtocol protocol, ParameterMapping parameterMapping, Configuration configuration)
			: base(configuration)
		{
			this.protocol = protocol;
			this.parameterMapping = parameterMapping;
			paths = parameterMapping.ParameterPaths;
			LastExecutionTime = null;
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

			string joinedPaths = String.Join("/", paths.Select(x => String.Join(".", x)));
			string otherJoinedPaths = String.Join("/", other.paths.Select(x => String.Join(".", x)));

			return joinedPaths.Equals(otherJoinedPaths);
		}

		public override void Execute()
		{
			LastExecutionTime = DateTime.Now.ToOADate();
			SendGetDirectoryRequest(protocol, paths);
		}

		public override int GetHashCode()
		{
			string joinedPaths = String.Join("/", paths.Select(x => String.Join(".", x)));

			return joinedPaths.GetHashCode();
		}

		public override int[] ProcessReceivedGlow(ParameterMapping mapping, GlowContainer glowContainer, int[] validateLastRequestPath)
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

			if (parameterMapping.ReverseEmberTree.ContainsKey(parentPath) && !parameterMapping.ReverseEmberTree.ContainsKey(path))
			{
				string[] friendlyParentPath = parameterMapping.ReverseEmberTree[parentPath];
				var friendlyNodePath = new string[friendlyParentPath.Length + 1];
				Array.Copy(friendlyParentPath, friendlyNodePath, friendlyParentPath.Length);
				friendlyNodePath[friendlyParentPath.Length] = glow.Identifier;

				parameterMapping.ReverseEmberTree.Add(path, friendlyNodePath);
				parameterMapping.EmberTree.Add(friendlyNodePath, path);

				protocol.Log(
					"QA" + protocol.QActionID + "|EmberPollAction.OnNode|Added missing path: " + String.Join(".", friendlyNodePath) + "; Path:" + String.Join(".", path),
					LogType.Information,
					LogLevel.NoLogging);

				protocol.SetParameter(Configurations.DiscoveredNodesCountPid, parameterMapping.EmberTree.Count);
			}
		}

		protected override void OnParameter(GlowParameterBase glow, int[] path)
		{
			var parentPath = new int[path.Length - 1];
			Array.Copy(path, parentPath, parentPath.Length);

			if (!parameterMapping.ReverseEmberTree.ContainsKey(parentPath))
			{
				return;
			}

			string[] friendlyParentPath = parameterMapping.ReverseEmberTree[parentPath];
			var friendlyParameterPath = new string[friendlyParentPath.Length + 1];
			Array.Copy(friendlyParentPath, friendlyParameterPath, friendlyParentPath.Length);
			friendlyParameterPath[friendlyParentPath.Length] = glow.Identifier;

			polledParameters[friendlyParameterPath] = ConvertGlowValue(glow);
		}

		private void FetchAndUpdate()
		{
			protocol.Log("QA" + protocol.QActionID + "|polledParameters|\n" + JsonConvert.SerializeObject(polledParameters), LogType.DebugInfo, LogLevel.NoLogging);
			protocol.Log("QA" + protocol.QActionID + "|EmberTree|\n" + JsonConvert.SerializeObject(parameterMapping.EmberTree), LogType.DebugInfo, LogLevel.NoLogging);
			protocol.Log("QA" + protocol.QActionID + "|ReverseEmberTree|\n" + JsonConvert.SerializeObject(parameterMapping.ReverseEmberTree), LogType.DebugInfo, LogLevel.NoLogging);

			foreach (var polledParameter in polledParameters)
			{
			}

			polledParameters.Clear();
		}

		private void HandlePolledInformation()
		{
			if (polledParameters != null && polledParameters.Count > 0)
			{
				FetchAndUpdate();
			}
		}
	}
}