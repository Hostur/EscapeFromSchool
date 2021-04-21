using System;
using System.Collections.Generic;
using System.Linq;
using FCData.Attributes;
using FCData.Common;
using FCData.Network.Multicast;
using FCData.Network.Server.Management;
using Graphics.MVVM;

namespace UI.ViewModels
{
	[FCRegister(false)]
	public class ServerClientsViewModel : ViewModelBase
	{
		private readonly FCConnectedClients _connectedClients;
		private List<ConnectedClientData> _clients => _connectedClients.Clients;

		public ServerClientsViewModel(FCConnectedClients connectedClients)
		{
			_connectedClients = connectedClients;
		}

		[FCRegisterEventHandler(typeof(ServerClientDisconnectButtonClickedEvent))]
		private void OnServerClientDisconnectButtonClickedEvent(object sender, EventArgs arg)
		{
			ServerClientDisconnectButtonClickedEvent e = arg as ServerClientDisconnectButtonClickedEvent;
			_connectedClients.Disconnect(e.ClientData);
		}

		public override void Refresh()
		{
			base.Refresh();
			_connectedClients.Refresh();
			FireOnPropertyChanged(() => _clients);
		}
	}
}
