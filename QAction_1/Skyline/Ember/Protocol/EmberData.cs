namespace QAction_1.Skyline.Ember.Protocol
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using QAction_1.Skyline.DataMiner.Scripting.Solutions.Ember;

	public class EmberData
	{
		// Friendly path - ember id path
		public Dictionary<string[], int[]> EmberTree { get; } = new Dictionary<string[], int[]>();

		public int[][] ParameterPaths { get; set; }

		public Queue<EmberAction> PollActions { get; } = new Queue<EmberAction>();

		public Dictionary<string[], object> PolledParameters { get; } = new Dictionary<string[], object>();

		public Dictionary<int[], string[]> ReverseEmberTree { get; } = new Dictionary<int[], string[]>();

		public void SetParameterPaths(string[] parameterPath)
		{
			ParameterPaths = GetPaths(parameterPath);
		}

		private int[][] GetPaths(string[] parameterPath)
		{
			return EmberTree.Keys.Where(key => String.Join(".", key).StartsWith(String.Join(".", parameterPath))).Select(key => EmberTree[key]).ToArray();
		}
	}
}