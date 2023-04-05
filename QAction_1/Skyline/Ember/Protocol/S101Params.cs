namespace QAction_1.Skyline.Ember.Protocol
{
	public class S101Params
	{
		public S101Params(int s101BofPid, int s101EofPid, int s101RequestDataPid, int s101ResponseDataPid)
		{
			S101BofPid = s101BofPid;
			S101EofPid = s101EofPid;
			S101RequestDataPid = s101RequestDataPid;
			S101ResponseDataPid = s101ResponseDataPid;
		}

		/// <summary>
		///     Returns: Parameter.S101BofPid.
		/// </summary>
		public int S101BofPid { get; }

		/// <summary>
		///     Returns: Parameter.S101EofPid.
		/// </summary>
		public int S101EofPid { get; }

		/// <summary>
		///     Returns: Parameter.S101RequestDataPid.
		/// </summary>
		public int S101RequestDataPid { get; }

		/// <summary>
		///     Returns: Parameter.S101ResponseDataPid.
		/// </summary>
		public int S101ResponseDataPid { get; }
	}
}