namespace QAction_1.Ember.Protocol
{
	public class Configuration
	{
		public Configuration(int discoverEmberTreePid, S101Params s101Pids, int sendEmberRequestTrigger, int discoveredNodesCountPid, int discoveryNodeProgressPid, int maxDepth = 10,
			int maxRetries = 3, int timeOutSeconds = 10)
		{
			DiscoverEmberTreePid = discoverEmberTreePid;
			S101Pids = s101Pids;
			SendEmberRequestTrigger = sendEmberRequestTrigger;
			DiscoveredNodesCountPid = discoveredNodesCountPid;
			DiscoveryNodeProgressPid = discoveryNodeProgressPid;
			MaxDepth = maxDepth;
			MaxRetries = maxRetries;
			TimeOutSeconds = timeOutSeconds;
		}

		/// <summary>
		///     Returns: Parameter.DiscoveredNodesCount.
		/// </summary>
		public int DiscoveredNodesCountPid { get; }

		/// <summary>
		///     Returns: Parameter.DiscoverEmberTree.
		/// </summary>
		public int DiscoverEmberTreePid { get; }

		/// <summary>
		///     Returns: Parameter.DiscoveredNodesCount.
		/// </summary>
		public int DiscoveryNodeProgressPid { get; }

		/// <summary>
		///     Returns: S101 Pids.
		/// </summary>
		public S101Params S101Pids { get; }

		/// <summary>
		///     Returns: Trigger.SendEmberRequestTrigger.
		/// </summary>
		public int SendEmberRequestTrigger { get; }

		internal int MaxDepth { get; }

		internal int MaxRetries { get; }

		internal int TimeOutSeconds { get; }
	}
}