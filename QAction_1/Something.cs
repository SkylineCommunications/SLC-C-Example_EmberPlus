namespace Skyline.DataMiner.Scripting
{
	public static class Trigger
	{
		public static readonly int SendEmberRequest_50 = 50;
	}
}

namespace Skyline.Protocol.Communication.Serial
{
	using Skyline.DataMiner.Scripting;

	public static class RequestHandler
	{
		public static void SendRequest(SLProtocolExt protocol, byte[] requestBytes)
		{
			protocol.SetParameterBinary(Parameter.s101requestdata_52, requestBytes);
			protocol.CheckTrigger(Trigger.SendEmberRequest_50);
		}
	}
}