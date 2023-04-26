namespace QAction_1.Skyline.Ember.Protocol
{
	public class S101Params
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="S101Params" /> class.
		/// </summary>
		/// <param name="s101RequestDataPid">The parameter ID of the request data.</param>
		public S101Params(int s101RequestDataPid)
		{
			S101RequestDataPid = s101RequestDataPid;
		}

		/// <summary>
		///     Returns: Parameter.S101RequestDataPid.
		/// </summary>
		public int S101RequestDataPid { get; }
	}
}