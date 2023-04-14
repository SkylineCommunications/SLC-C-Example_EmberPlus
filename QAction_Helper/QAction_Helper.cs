// <auto-generated>This is auto-generated code by DIS. Do not modify.</auto-generated>
using System.ComponentModel;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Skyline.DataMiner.Scripting
{
public static class Parameter
{
	/// <summary>PID: 1 | Type: read</summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public const int discoverembertree_1 = 1;
	/// <summary>PID: 1 | Type: read</summary>
	public const int discoverembertree = 1;
	/// <summary>PID: 10 | Type: read</summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public const int polltables_10 = 10;
	/// <summary>PID: 10 | Type: read</summary>
	public const int polltables = 10;
	/// <summary>PID: 11 | Type: read</summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public const int pollparameters_11 = 11;
	/// <summary>PID: 11 | Type: read</summary>
	public const int pollparameters = 11;
	/// <summary>PID: 52 | Type: read</summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public const int s101requestdata_52 = 52;
	/// <summary>PID: 52 | Type: read</summary>
	public const int s101requestdata = 52;
	/// <summary>PID: 53 | Type: read</summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public const int s101responsedata_53 = 53;
	/// <summary>PID: 53 | Type: read</summary>
	public const int s101responsedata = 53;
	/// <summary>PID: 60 | Type: read</summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public const int discoverednodescount_60 = 60;
	/// <summary>PID: 60 | Type: read</summary>
	public const int discoverednodescount = 60;
	/// <summary>PID: 61 | Type: read</summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public const int nodediscoveryprogress_61 = 61;
	/// <summary>PID: 61 | Type: read</summary>
	public const int nodediscoveryprogress = 61;
	/// <summary>PID: 500 | Type: read</summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public const int identityproduct_500 = 500;
	/// <summary>PID: 500 | Type: read</summary>
	public const int identityproduct = 500;
	/// <summary>PID: 501 | Type: read</summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public const int identitycompany_501 = 501;
	/// <summary>PID: 501 | Type: read</summary>
	public const int identitycompany = 501;
	/// <summary>PID: 502 | Type: read</summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public const int identityversion_502 = 502;
	/// <summary>PID: 502 | Type: read</summary>
	public const int identityversion = 502;
	/// <summary>PID: 503 | Type: read</summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public const int identityrole_503 = 503;
	/// <summary>PID: 503 | Type: read</summary>
	public const int identityrole = 503;
	/// <summary>PID: 504 | Type: read</summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public const int identityserial_504 = 504;
	/// <summary>PID: 504 | Type: read</summary>
	public const int identityserial = 504;
	public class Write
	{
		/// <summary>PID: 62 | Type: write</summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public const int startnodediscovery_62 = 62;
		/// <summary>PID: 62 | Type: write</summary>
		public const int startnodediscovery = 62;
	}
}
public class WriteParameters
{
	/// <summary>PID: 62  | Type: write | DISCREETS: Start Node Discovery = 1</summary>
	public System.Object Startnodediscovery {get { return Protocol.GetParameter(62); }set { Protocol.SetParameter(62, value); }}
	public SLProtocolExt Protocol;
	public WriteParameters(SLProtocolExt protocol)
	{
		Protocol = protocol;
	}
}
public interface SLProtocolExt : SLProtocol
{
	object Discoverembertree_1 { get; set; }
	object Discoverembertree { get; set; }
	object Polltables_10 { get; set; }
	object Polltables { get; set; }
	object Pollparameters_11 { get; set; }
	object Pollparameters { get; set; }
	object S101bof_header { get; set; }
	object S101eof_trailer { get; set; }
	object S101requestdata_52 { get; set; }
	object S101requestdata { get; set; }
	object S101responsedata_53 { get; set; }
	object S101responsedata { get; set; }
	object Discoverednodescount_60 { get; set; }
	object Discoverednodescount { get; set; }
	object Nodediscoveryprogress_61 { get; set; }
	object Nodediscoveryprogress { get; set; }
	object Startnodediscovery_62 { get; set; }
	object Startnodediscovery { get; set; }
	object Identityproduct_500 { get; set; }
	object Identityproduct { get; set; }
	object Identitycompany_501 { get; set; }
	object Identitycompany { get; set; }
	object Identityversion_502 { get; set; }
	object Identityversion { get; set; }
	object Identityrole_503 { get; set; }
	object Identityrole { get; set; }
	object Identityserial_504 { get; set; }
	object Identityserial { get; set; }
	WriteParameters Write { get; set; }
}
public class ConcreteSLProtocolExt : ConcreteSLProtocol, SLProtocolExt
{
	/// <summary>PID: 1  | Type: read</summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public System.Object Discoverembertree_1 {get { return GetParameter(1); }set { SetParameter(1, value); }}
	/// <summary>PID: 1  | Type: read</summary>
	public System.Object Discoverembertree {get { return GetParameter(1); }set { SetParameter(1, value); }}
	/// <summary>PID: 10  | Type: read</summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public System.Object Polltables_10 {get { return GetParameter(10); }set { SetParameter(10, value); }}
	/// <summary>PID: 10  | Type: read</summary>
	public System.Object Polltables {get { return GetParameter(10); }set { SetParameter(10, value); }}
	/// <summary>PID: 11  | Type: read</summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public System.Object Pollparameters_11 {get { return GetParameter(11); }set { SetParameter(11, value); }}
	/// <summary>PID: 11  | Type: read</summary>
	public System.Object Pollparameters {get { return GetParameter(11); }set { SetParameter(11, value); }}
	/// <summary>PID: 50  | Type: header</summary>
	public System.Object S101bof_header {get { return GetParameter(50); }set { SetParameter(50, value); }}
	/// <summary>PID: 51  | Type: trailer</summary>
	public System.Object S101eof_trailer {get { return GetParameter(51); }set { SetParameter(51, value); }}
	/// <summary>PID: 52  | Type: read</summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public System.Object S101requestdata_52 {get { return GetParameter(52); }set { SetParameter(52, value); }}
	/// <summary>PID: 52  | Type: read</summary>
	public System.Object S101requestdata {get { return GetParameter(52); }set { SetParameter(52, value); }}
	/// <summary>PID: 53  | Type: read</summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public System.Object S101responsedata_53 {get { return GetParameter(53); }set { SetParameter(53, value); }}
	/// <summary>PID: 53  | Type: read</summary>
	public System.Object S101responsedata {get { return GetParameter(53); }set { SetParameter(53, value); }}
	/// <summary>PID: 60  | Type: read</summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public System.Object Discoverednodescount_60 {get { return GetParameter(60); }set { SetParameter(60, value); }}
	/// <summary>PID: 60  | Type: read</summary>
	public System.Object Discoverednodescount {get { return GetParameter(60); }set { SetParameter(60, value); }}
	/// <summary>PID: 61  | Type: read | EXCEPTIONS: N/A = 0, Finished = 1</summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public System.Object Nodediscoveryprogress_61 {get { return GetParameter(61); }set { SetParameter(61, value); }}
	/// <summary>PID: 61  | Type: read | EXCEPTIONS: N/A = 0, Finished = 1</summary>
	public System.Object Nodediscoveryprogress {get { return GetParameter(61); }set { SetParameter(61, value); }}
	/// <summary>PID: 62  | Type: write | DISCREETS: Start Node Discovery = 1</summary>
	public System.Object Startnodediscovery_62 {get { return GetParameter(62); }set { SetParameter(62, value); }}
	/// <summary>PID: 62  | Type: write | DISCREETS: Start Node Discovery = 1</summary>
	public System.Object Startnodediscovery {get { return Write.Startnodediscovery; }set { Write.Startnodediscovery = value; }}
	/// <summary>PID: 500  | Type: read</summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public System.Object Identityproduct_500 {get { return GetParameter(500); }set { SetParameter(500, value); }}
	/// <summary>PID: 500  | Type: read</summary>
	public System.Object Identityproduct {get { return GetParameter(500); }set { SetParameter(500, value); }}
	/// <summary>PID: 501  | Type: read</summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public System.Object Identitycompany_501 {get { return GetParameter(501); }set { SetParameter(501, value); }}
	/// <summary>PID: 501  | Type: read</summary>
	public System.Object Identitycompany {get { return GetParameter(501); }set { SetParameter(501, value); }}
	/// <summary>PID: 502  | Type: read</summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public System.Object Identityversion_502 {get { return GetParameter(502); }set { SetParameter(502, value); }}
	/// <summary>PID: 502  | Type: read</summary>
	public System.Object Identityversion {get { return GetParameter(502); }set { SetParameter(502, value); }}
	/// <summary>PID: 503  | Type: read</summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public System.Object Identityrole_503 {get { return GetParameter(503); }set { SetParameter(503, value); }}
	/// <summary>PID: 503  | Type: read</summary>
	public System.Object Identityrole {get { return GetParameter(503); }set { SetParameter(503, value); }}
	/// <summary>PID: 504  | Type: read</summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public System.Object Identityserial_504 {get { return GetParameter(504); }set { SetParameter(504, value); }}
	/// <summary>PID: 504  | Type: read</summary>
	public System.Object Identityserial {get { return GetParameter(504); }set { SetParameter(504, value); }}
	public WriteParameters Write { get; set; }
	public ConcreteSLProtocolExt()
	{
		Write = new WriteParameters(this);
	}
}
}
