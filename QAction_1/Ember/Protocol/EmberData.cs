namespace QAction_1.Ember.Protocol
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	public class EmberData
	{
		public Dictionary<string[], int> EmberParameterMap { get; } = new Dictionary<string[], int>();

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

		public void UpdateMappings()
		{
			foreach (string[] key in ParameterMapping.EmberParameterMap.Keys.Where(key => !EmberParameterMap.ContainsKey(key)))
			{
				EmberParameterMap.Add(key, ParameterMapping.EmberParameterMap[key]);
			}
		}

		private int[][] GetPaths(string[] parameterPath)
		{
			return EmberTree.Keys.Where(key => String.Join(".", key).StartsWith(String.Join(".", parameterPath))).Select(key => EmberTree[key]).ToArray();
		}
	}
}