namespace QAction_1.Skyline.Ember.Protocol
{
	using System.Collections.Generic;

	public static class ParameterMapping
	{
		private const string Channels = "Channels";

		private const string IdentityIden = "identity";

		private const string LvpIdentifier = "LVP";

		private const string LvpOut = "lvp--out";

		private const string R3LayVirtualPatchBayIden = "R3LAYVirtualPatchBay";

		private const string Root = "Root";

		private static readonly string[] ChannelsPath = new[] { LvpOut, Root, LvpIdentifier, Channels };

		private static readonly string[] IdentityPath = new[] { R3LayVirtualPatchBayIden, IdentityIden };

		public static List<string[]> Paths => new List<string[]>
		{
			IdentityPath,
			ChannelsPath,
		};
	}
}