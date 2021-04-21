using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Client;
using FCData.Attributes;
using FCData.Common;
using FCData.Network.Client;
using FCData.Network.Client.Connector;
using FCData.Network.Multicast;
using FCData.WebService;
using FCData.WebService.Requests;
using FCData.WebService.Responses;
using Graphics.MVVM;

namespace UI.ViewModels
{
	[FCRegister(false)]
	public class AvailableServersViewModel : ViewModelBase
	{
		private bool _connected;
		private static bool _dirty;
		private readonly IClientConnector _clientConnector;
		private readonly IFCWebServiceProvider _webServiceProvider;
		internal readonly FCClientNetworkData ClientNetworkData;
		private readonly MulticastReceiver _multicastReceiver;

		private List<ServerInfo> _availableServers { get; set; }

		public AvailableServersViewModel(IClientConnector connector, IFCWebServiceProvider webServiceProvider, FCClientNetworkData clientNetworkData, MulticastReceiver multicastReceiver)
		{
			_clientConnector = connector;
			_webServiceProvider = webServiceProvider;
			_availableServers = new List<ServerInfo>();
			this.ClientNetworkData = clientNetworkData;

			ClientNetworkData.AuthorizationToken = _webServiceProvider.Authorize("abc", "def").Result;
			_multicastReceiver = multicastReceiver;
		}

		public override async void Refresh()
		{
			base.Refresh();
			RefreshServers();
			await DiscoverLocalServer();

			// Use dirty flag because multicast callback comes from other thread and it can't cause fire on property change directly.
			if (_dirty)
			{
				_dirty = false;
				FireOnPropertyChanged(() => _availableServers);
			}
		}

		public async Task DiscoverLocalServer()
		{
			await _multicastReceiver.Receive(OnServerDiscoveredCallback).ConfigureAwait(false);
		}

		private void OnServerDiscoveredCallback(PublishServerStateRequest serverData)
		{
			if (serverData != null)
			{
				this.Log($"DiscoverLocalServer: {serverData.LocalIpAddress}");
				ServerInfo serverInfo = new ServerInfo(
					serverData.Name,
					serverData.PublicIpAddress,
					serverData.LocalIpAddress,
					serverData.Port, 
					serverData.State == ServerState.Available,
					true);

				if (!_availableServers.Contains(serverInfo))
				{
					_availableServers.Add(serverInfo);
					_dirty = true;
				}
			}
		}

		private void RefreshServers()
		{
			var servers = _webServiceProvider.GetServers().Result;
			if (servers != null)
			{
				// Remove all requests from web service
				int removed = _availableServers.RemoveAll(s => !s.FromMulticast);
				foreach (ServerInfo server in servers.Servers)
				{
					if (!_availableServers.Contains(server))
					{
						_availableServers.Add(server);
						_dirty = true;
					}
						
				}

				if (removed > 0)
					_dirty = true;
			}
		}

		[FCRegisterEventHandler(typeof(LocalServerDiscoveredEvent))]
		private void OnLocalServerDiscoveredEvent(object sender, EventArgs arg)
		{
			LocalServerDiscoveredEvent e = arg as LocalServerDiscoveredEvent;
			this.Log($"On discovered server event: {e.DiscoveredAddress}");
		}

		[FCRegisterEventHandler(typeof(ServerRecordConnectClickedEvent))]
		private void OnServerRecordConnectClickedEvent(object sender, EventArgs arg)
		{
			if (_connected) return;

			ServerRecordConnectClickedEvent e = arg as ServerRecordConnectClickedEvent;
			_clientConnector.Open(e.Info);
			_connected = true;
		}

		[FCRegisterEventHandler(typeof(ClientDisconnectionHandlerEvent))]
		private void OnClientDisconnectionHandlerEvent(object sender, EventArgs arg)
		{
			_connected = false;
		}
	}
}
