

namespace SLC.Lib.EmberPlus
{
	// API
}

namespace Vendor.Device.Api
{
	using System.Collections.Generic;
	// Device mapping

	public static class MyTree
	{
		public static List<string[]> Paths => new List<string[]>
		{
			new[] { "log" },
			new[] { "identity" },
			new[] { "system" },
			new[] { "warmstart" },
			new[] { "ptp" },
			new[] { "IoControl" },
			new[] { "RAVENNA top level schema" },
			new[] { "sync" },
			new[] { "services" },
		};
	}
}