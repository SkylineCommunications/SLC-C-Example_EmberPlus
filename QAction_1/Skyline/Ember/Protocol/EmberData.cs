namespace QAction_1.Skyline.Ember.Protocol
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using QAction_1.Skyline.DataMiner.Scripting.Solutions.Ember;

	public class EmberData
	{
		// Joined friendly path - ember id path
		public Dictionary<string, int[]> EmberTree { get; } = new Dictionary<string, int[]>();

		public int[][] ParameterPaths { get; set; }

		public Queue<EmberAction> PollActions { get; } = new Queue<EmberAction>();

		public Dictionary<string[], object> PolledParameters { get; } = new Dictionary<string[], object>();

		public Dictionary<string, string[]> ReverseEmberTree { get; } = new Dictionary<string, string[]>();

		public void SetParameterPaths(string[] parameterPath)
		{
			string joinedPath = String.Join(".", parameterPath);
			ParameterPaths = GetPaths(joinedPath);
		}

		private int[][] GetPaths(string path)
		{
			return EmberTree.Keys.Where(key => key.StartsWith(path)).Select(key => EmberTree[key]).ToArray();
		}
	}
}