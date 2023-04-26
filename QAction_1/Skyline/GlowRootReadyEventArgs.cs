namespace QAction_1.Skyline
{
	using System;
	using EmberLib.Glow;

	public class GlowRootReadyEventArgs : EventArgs
	{
		public GlowRootReadyEventArgs(GlowContainer root)
		{
			Root = root;
		}

		public GlowContainer Root { get; }
	}
}