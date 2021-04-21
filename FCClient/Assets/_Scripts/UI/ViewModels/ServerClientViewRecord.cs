using FCData.Events;
using FCData.Network.Server.Management;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

#pragma warning disable 649

namespace UI.ViewModels
{
	internal class ServerClientDisconnectButtonClickedEvent : FCGameEvent
	{
		public ConnectedClientData ClientData { get; }
		public ServerClientDisconnectButtonClickedEvent(ConnectedClientData clientData) => ClientData = clientData;
	}

	public class ServerClientViewRecord : MonoBehaviour
	{
		private ConnectedClientData _connectedClient;
		public int InternalId;
		[SerializeField] private TMP_Text _internalId;
		[SerializeField] private TMP_Text _publicId;
		[SerializeField] private TMP_Text _clientIp;
		[SerializeField] private TMP_Text _udpPort;
		[SerializeField] private TMP_Text _forwardedUdpPort;
		[SerializeField] private TMP_Text _rtt;
		[SerializeField] private Button _disconnectButton;
		

		internal void Init(ConnectedClientData clientData)
		{
			Refresh(clientData);
			_disconnectButton.onClick.AddListener(Disconnect);
		}

		internal void Refresh(ConnectedClientData connectedClientData)
		{
			_connectedClient = connectedClientData;
			InternalId = _connectedClient.InternalId;
			_internalId.text = _connectedClient.InternalId.ToString();
			_publicId.text = _connectedClient.PublicId.ToString();
			_clientIp.text = _connectedClient.Ip.ToString();
			_udpPort.text = _connectedClient.UdpPort.ToString();
			_forwardedUdpPort.text = _connectedClient.ForwardedUdpPort.ToString();
			_rtt.text = _connectedClient.RttFrames.ToString();
		}

		private void Disconnect() => FCGameEventsManager.Publish(this, new ServerClientDisconnectButtonClickedEvent(_connectedClient));
	}
}