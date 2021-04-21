
using FCData;

namespace Configuration.NetworkSettings
{
	public class ClientAndPublicServerNetworkSettings : IFCNetworkSettings
	{
		public int FrameRate { get; set; } = 30;
		public int ClientsCapacity { get; } = 5;
		public int ServerAliveEntitiesCapacity { get; } = 5;
		public int SingleRequestsQueueCapacity { get; } = 24;
		public int DefaultServerPort { get; } = 6337;
		public bool ForwardPorts { get; } = false;
		public bool PublishServer { get; } = true;
		public int MillisecondsRequestsDelay { get; } = 0;
		public string ServerName { get; } = "Public server";
		public bool MockWebService { get; } = true;
		public string WebServiceAddress { get; } = string.Empty;
		public SetupType SetupType { get; } = SetupType.ClientAndPublicServer;
		public int MapWidth { get; } = 1200;
		public int MapHeight { get; } = 1000;
		public int ChunkSize { get; } = 40;
		public int BigChunkSize { get; } = 120;
		public int MaximumRttFramesForNetworkWarning { get; } = 12;
		public int MaximumNetworkWarnings { get; } = 30;
		public int MaximumNetworkErrors { get; } = 8;
		public string MulticastGroupAddress { get; } = "224.5.6.7";
		public int MulticastGroupPort { get; } = 6330;
		public bool UseServerMulticastPropagation { get; } = true;
	}
}
