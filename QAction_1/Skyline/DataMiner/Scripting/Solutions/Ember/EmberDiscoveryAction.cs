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

		private readonly List<string[]> branchesToSkip = new List<string[]>();

		// ember path - user friendly path
		private readonly Dictionary<int[], string[]> emberTree = new Dictionary<int[], string[]>();

		private readonly Queue<int[]> pathsToPoll = new Queue<int[]>();

		private readonly Queue<EmberAction> pollActions;

		private readonly SLProtocol protocol;

		private int[] currentPollPath = Array.Empty<int>();

		private int[] lastRequestPath = Array.Empty<int>();

		private bool updateReceivedForWrongRequestedNode;

		public EmberDiscoveryAction(SLProtocol protocol, Configuration configuration, Queue<EmberAction> pollActions)
			: base(configuration)
		{
			this.protocol = protocol;
			this.pollActions = pollActions;
			pathsToPoll.Enqueue(Array.Empty<int>());
			LastExecutionTime = null;
			Retries = 0;
		}

		public override void Continue()
		{
			Retries++;
			SendGetDirectoryRequest(protocol, new[] { currentPollPath });
		}

		public override void Execute()
		{
			LastExecutionTime = DateTime.Now.ToOADate();
			PollNextPath();
		}

		public override int[] ProcessReceivedGlow(ParameterMapping mapping, GlowContainer glowContainer, int[] validateLastRequestPath)
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
				UpdateEmberTrees(mapping);

				// Clear the poll queue as it may contain invalid Ember paths
				pollActions.Clear();

				protocol.SetParameter(Configurations.DiscoveryNodeProgressPid, "done");
			}

			return lastRequestPath;
		}

		protected override void OnNode(GlowNodeBase glow, int[] path)
		{
			var parentPath = new int[path.Length - 1];
			string joinedPath = String.Join(".", path);
			Array.Copy(path, parentPath, parentPath.Length);

			if (!parentPath.SequenceEqual(lastRequestPath) && !joinedPath.Equals(RootNumber) /*root*/ && !lastRequestPath.SequenceEqual(path))
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

			if (!parentPath.SequenceEqual(lastRequestPath) && !String.Join(".", path).Equals(RootNumber) && !lastRequestPath.SequenceEqual(path))
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

			foreach (string[] branch in branchesToSkip)
			{
				if (nodePath.SequenceEqual(branch))
				{
					return false;
				}
			}

			return true;
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
			SendGetDirectoryRequest(protocol, new[] { currentPollPath });
		}

		private void UpdateEmberTrees(ParameterMapping parameterMapping)
		{
			foreach (int[] key in emberTree.Keys)
			{
				if (!parameterMapping.EmberTree.ContainsKey(emberTree[key]))
				{
					parameterMapping.EmberTree.Add(emberTree[key], key);
					parameterMapping.ReverseEmberTree.Add(key, emberTree[key]);
				}
				else
				{
					// Friendly path is known -> check if ember path changed
					if (parameterMapping.ReverseEmberTree.ContainsKey(key))
					{
						continue;
					}

					// Ember path changed
					// Remove old entries
					parameterMapping.ReverseEmberTree.Remove(parameterMapping.EmberTree[emberTree[key]]);
					parameterMapping.EmberTree.Remove(emberTree[key]);

					// Add new entries
					parameterMapping.EmberTree.Add(emberTree[key], key);
					parameterMapping.ReverseEmberTree.Add(key, emberTree[key]);
				}
			}
		}
	}
}