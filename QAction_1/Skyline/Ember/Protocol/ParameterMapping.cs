namespace QAction_1.Skyline.Ember.Protocol
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	public class ParameterMapping
	{
		// Friendly path - ember id path
		public Dictionary<string[], int[]> EmberTree { get; } = new Dictionary<string[], int[]>();

		public int[][] ParameterPaths { get; set; }

		

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