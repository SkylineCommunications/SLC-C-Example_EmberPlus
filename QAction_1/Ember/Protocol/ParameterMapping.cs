namespace QAction_1.Ember.Protocol
{
	using System.Collections.Generic;
	using Skyline.DataMiner.Scripting;

	public static class ParameterMapping
	{
		private const string CompanyIden = "company";

		private const string IdentityIden = "identity";

		private const string ProductIden = "product";

		private const string RoleIden = "role";

		private const string SerialIden = "serial";

		private const string VersionIden = "version";

		public static List<string[]> BranchesToSkip => new List<string[]>
		{
			new[] { "system" },
			new[] { "warmstart" },
			new[] { "ptp" },
			new[] { "IoControl" },
			new[] { "ravenna" },
			new[] { "sync" },
			new[] { "services" },
		};

		public static Dictionary<string[], int> EmberParameterMap => new Dictionary<string[], int>
		{
			// Identity
			{ new[] { IdentityIden, ProductIden }, Parameter.identityproduct_500 },
			{ new[] { IdentityIden, CompanyIden }, Parameter.identitycompany_501 },
			{ new[] { IdentityIden, VersionIden }, Parameter.identityversion_502 },
			{ new[] { IdentityIden, RoleIden }, Parameter.identityrole_503 },
			{ new[] { IdentityIden, SerialIden }, Parameter.identityserial_504 },

			// Table Data
		};

		public static List<string[]> Paths => new List<string[]>
		{
			new[] { IdentityIden },
			new[] { "log" },
		};
	}
}