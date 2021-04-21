using FCData.Attributes;
using FCData.Common;
using FCData.DataUtils.DI;
using FCData.Events;
using FCData.Network.Client;
using FCData.Network.Client.Connector;

namespace Client
{
	public class ClientDisconnectionHandlerEvent : FCGameEvent
	{
		public ClientDisconnectionReason Reason { get; }
		public ClientDisconnectionHandlerEvent(ClientDisconnectionReason reason) => Reason = reason;
	}

	[FCRegister(true)]
	public class ClientDisconnectionHandler : IClientDisconnectionHandler
	{
		private readonly FCClientNetworkData _clientNetworkData;

		public ClientDisconnectionHandler(FCClientNetworkData clientNetworkData)
		{
			_clientNetworkData = clientNetworkData;
		}

		public void OnClientDisconnected(ClientDisconnectionReason reason)
		{
			FCMainThreadActionsQueue actionQueue = God.PrayFor<FCMainThreadActionsQueue>();
			actionQueue.Enqueue(() =>
			{
				FCGameEventsManager.Publish(this, new ClientDisconnectionHandlerEvent(reason));
				_clientNetworkData.MyId = 0;
			});

			this.Log(reason.ToString(), LogLevel.Error);
		}
	}
}