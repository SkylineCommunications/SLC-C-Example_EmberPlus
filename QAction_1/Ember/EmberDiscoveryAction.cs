namespace QAction_1.Ember
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using EmberLib.Glow;
	using QAction_1.Ember.Protocol;
	using Skyline.DataMiner.Scripting;

	public class EmberDiscoveryAction : EmberAction
	{
		private const string RootNumber = "1";

		private const string RootString = "Root";

		// ember path - user friendly path
		private readonly Dictionary<int[], string[]> emberTree = new Dictionary<int[], string[]>();

		private readonly Queue<int[]> pathsToPoll = new Queue<int[]>();

		private int[] currentPollPath = Array.Empty<int>();

		private int[] lastRequestPath = Array.Empty<int>();

		private bool updateReceivedForWrongRequestedNode;

		public EmberDiscoveryAction(SLProtocol protocol, Configuration configuration)
			: base(protocol, configuration)
		{
			pathsToPoll.Enqueue(Array.Empty<int>());
			LastExecutionTime = null;
			Retries = 0;
		}

		public override void Continue()
		{
			Retries++;
			SendGetDirectoryRequest(new[] { currentPollPath });
		}

		public override void Execute()
		{
			LastExecutionTime = DateTime.Now.ToOADate();
			PollNextPath();
		}

		public override int[] ProcessReceivedGlow(EmberData emberData, GlowContainer glowContainer, int[] validateLastRequestPath)
		{
			// Check if the ValidateLastRequestPath matches the current Parent, else skip the request.
			lastRequestPath = validateLastRequestPath;

			updateReceivedForWrongRequestedNode = false;
			Walk(glowContainer);

			if (updateReceivedForWrongRequestedNode)
			{
				return validateLastRequestPath;
			}

			LastExecutionTime = DateTime.Now.ToOADate();

			if (pathsToPoll.Count != 0)
			{
				Done = false;
				PollNextPath();
			}
			else
			{
				Done = true;

				// Update Ember tree and Reverse Ember Tree
				UpdateEmberTrees(emberData);

				// Clear the poll queue as it may contain invalid Ember paths
				emberData.PollActions.Clear();

				protocol.SetParameter(Configurations.DiscoveryNodeProgressPid, "done");
			}

			return lastRequestPath;
		}

		protected override void OnNode(GlowNodeBase glow, int[] path)
		{
			string joinedPath = String.Join(".", path);
			var parentPath = new int[path.Length - 1];
			string joinedParentPath = String.Join(".", parentPath);
			Array.Copy(path, parentPath, parentPath.Length);

			if (joinedParentPath != String.Join(".", lastRequestPath) && !joinedPath.Equals(RootNumber) /*root*/ && !String.Join(".", lastRequestPath).Equals(joinedPath))
			{
				protocol.Log(
					"QA" + protocol.QActionID + "|Discovery.OnNode|joinedParentPath != LastRequestPath: " + String.Join(".", lastRequestPath) + " for incoming Node: " + joinedPath + " --> Skipping",
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
			var parentPath = new int[path.Length - 1];
			Array.Copy(path, parentPath, parentPath.Length);

			if (String.Join(".", parentPath) != String.Join(".", lastRequestPath) && !String.Join(".", path).Equals(RootNumber) && !String.Join(".", lastRequestPath).Equals(String.Join(".", path)))
			{
				// if parentRequest doesn't match path,   abort mission!  don't execute the pollNextPath() !!!
				updateReceivedForWrongRequestedNode = true;
			}
		}

		private bool IsBranchToDiscover(string[] nodePath)
		{
			if (nodePath == null || nodePath.Length == 0)
			{
				return true;
			}

			return ParameterMapping.BranchesToSkip.All(branch => !nodePath.SequenceEqual(branch));
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

			if (path.Length < Configurations.MaxDepth && IsBranchToDiscover(friendlyPath))
			{
				pathsToPoll.Enqueue(path);
			}
		}

		private void PollNextPath()
		{
			currentPollPath = pathsToPoll.Dequeue();
			lastRequestPath = currentPollPath ?? Array.Empty<int>();

			int[] parametersToSet = new[] { Configurations.DiscoveredNodesCountPid, Configurations.DiscoveryNodeProgressPid };

			protocol.SetParameters(parametersToSet, new object[] { emberTree.Count, currentPollPath == Array.Empty<int>() ? RootString : String.Join(".", currentPollPath) });
			SendGetDirectoryRequest(new[] { currentPollPath });
		}

		private void UpdateEmberTrees(EmberData emberData)
		{
			foreach (int[] key in emberTree.Keys)
			{
				if (!emberData.EmberTree.ContainsKey(emberTree[key]))
				{
					emberData.EmberTree.Add(emberTree[key], key);
					emberData.ReverseEmberTree.Add(key, emberTree[key]);
				}
				else
				{
					// Friendly path is known -> check if ember path changed
					if (emberData.ReverseEmberTree.ContainsKey(key))
					{
						continue;
					}

					// Ember path changed
					// Remove old entries
					emberData.ReverseEmberTree.Remove(emberData.EmberTree[emberTree[key]]);
					emberData.EmberTree.Remove(emberTree[key]);

					// Add new entries
					emberData.EmberTree.Add(emberTree[key], key);
					emberData.ReverseEmberTree.Add(key, emberTree[key]);
				}
			}
		}
	}
}