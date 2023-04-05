namespace QAction_1.Skyline.DataMiner.Scripting.Solutions.Ember
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using EmberLib.Glow;
	using global::Skyline.DataMiner.Scripting;
	using QAction_1.Skyline.Ember.Protocol;

	public class EmberDiscoveryAction : EmberAction
	{
		private const string RootNumber = "1";
		private const string RootString = "Root";

		// ember path - user friendly path
		private readonly Dictionary<int[], string[]> emberTree = new Dictionary<int[], string[]>();

		private readonly Queue<int[]> pathsToPoll = new Queue<int[]>();

		private string lastRequestPath = String.Empty;

		private bool updateReceivedForWrongRequestedNode;

		public EmberDiscoveryAction(SLProtocol protocol, Configuration configuration, EmberData emberData)
			: base(protocol, configuration, emberData)
		{
		}

		public override void Execute()
		{
			// SendGetDirectoryRequest(new[] { currentPollPath });
			PollNextPath();
		}

		public override string ProcessReceivedGlow(EmberData node, GlowContainer glowContainer, string validateLastRequestPath)
		{
			// Check if the ValidateLastRequestPath matches the current Parent, else skip the request.
			lastRequestPath = validateLastRequestPath;

			updateReceivedForWrongRequestedNode = false;
			Walk(glowContainer);

			if (updateReceivedForWrongRequestedNode)
			{
				return validateLastRequestPath;
			}

			if (pathsToPoll.Count != 0)
			{
				Done = false;
				PollNextPath();
			}
			else
			{
				Done = true;

				// Update Ember tree and Reverse Ember Tree
				UpdateEmberTrees(node);

				// Clear the poll queue as it may contain invalid Ember paths
				EmberData.PollActions.Clear();

				protocol.SetParameter(Configurations.DiscoveryNodeProgressPid, "done");
			}

			return lastRequestPath;
		}

		protected override void OnNode(GlowNodeBase glow, int[] path)
		{
			string joinedPath = String.Join(".", path);
			var parentPath = new int[path.Length - 1];
			Array.Copy(path, parentPath, parentPath.Length);
			string joinedParentPath = String.Join(".", parentPath);

			if (joinedParentPath != lastRequestPath && !joinedPath.Equals("1") /*root*/ && !lastRequestPath.Equals(joinedPath))
			{
				protocol.Log(
					"QA" + protocol.QActionID + "|Discovery.OnNode|joinedParentPath != LastRequestPath: " + lastRequestPath + " for incoming Node: " + joinedPath + " --> Skipping",
					LogType.Information,
					LogLevel.NoLogging);

				updateReceivedForWrongRequestedNode = true;

				return;
			}

			if (emberTree.ContainsKey(path))
			{
				return;
			}

			NewEmberTreePath(glow, path, parentPath);
		}

		protected override void OnParameter(GlowParameterBase glow, int[] path)
		{
			// Do nothing - only interested in nodes
			string joinedPath = String.Join(".", path);
			var parentPath = new int[path.Length - 1];
			Array.Copy(path, parentPath, parentPath.Length);
			string joinedParentPath = String.Join(".", parentPath);

			if (joinedParentPath != lastRequestPath && !joinedPath.Equals(RootNumber) && !lastRequestPath.Equals(joinedPath))
			{
				// if parentRequest doesn't match path,   abort mission!  don't execute the pollNextPath() !!!
				updateReceivedForWrongRequestedNode = true;
			}
		}

		private void NewEmberTreePath(GlowNodeBase glow, int[] path, int[] parentPath)
		{
			string glowIdentifier = glow.Identifier;
			string[] friendlyPath;

			if (!emberTree.TryGetValue(parentPath, out string[] _))
			{
				friendlyPath = new[] { glowIdentifier };
			}
			else
			{
				friendlyPath = new string[parentPath.Length + 1];
				Array.Copy(parentPath, friendlyPath, parentPath.Length);
				friendlyPath[parentPath.Length] = glowIdentifier;
			}

			emberTree.Add(path, friendlyPath);

			if (path.Length < Configurations.MaxDepth && !friendlyPath.Any())
			{
				pathsToPoll.Enqueue(path);
			}
		}

		private void PollNextPath()
		{
			int[] currentPollPath = pathsToPoll.Dequeue();
			lastRequestPath = String.Join(".", currentPollPath ?? new int[] { });

			int[] parametersToSet = new[] { Configurations.DiscoveredNodesCountPid, Configurations.DiscoveryNodeProgressPid };

			protocol.SetParameters(parametersToSet, new object[] { emberTree.Count, currentPollPath == null ? RootString : String.Join(".", currentPollPath) });
			SendGetDirectoryRequest(new[] { currentPollPath });
		}

		private void UpdateEmberTrees(EmberData node)
		{
			foreach (int[] key in emberTree.Keys)
			{
				string joinedFriendlyPath = String.Join(".", emberTree[key]);
				string joinedEmberPath = String.Join(".", key);

				if (!node.EmberTree.ContainsKey(joinedFriendlyPath))
				{
					node.EmberTree.Add(String.Join(".", emberTree[key]), key);
					node.ReverseEmberTree.Add(String.Join(".", key), emberTree[key]);
				}
				else
				{
					// Friendly path is known -> check if ember path changed
					if (node.ReverseEmberTree.ContainsKey(joinedEmberPath))
					{
						continue;
					}

					// Ember path changed
					// Remove old entries
					node.ReverseEmberTree.Remove(String.Join(".", node.EmberTree[joinedFriendlyPath]));
					node.EmberTree.Remove(joinedFriendlyPath);

					// Add new entries
					node.EmberTree.Add(String.Join(".", emberTree[key]), key);
					node.ReverseEmberTree.Add(String.Join(".", key), emberTree[key]);
				}
			}
		}
	}
}