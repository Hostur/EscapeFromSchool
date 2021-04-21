using System.Collections;
using System.Collections.Generic;
using FCData.Events;
using FCData.WebService.Responses;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
#pragma warning disable 649

namespace UI.ViewModels
{
	public class ServerRecordConnectClickedEvent : FCGameEvent
	{
		public ServerInfo Info { get; }
		public ServerRecordConnectClickedEvent(ServerInfo info) => Info = info;
	}

	public class ServerRecord : MonoBehaviour
	{
		[SerializeField] private Button _connectButton;
		[SerializeField] private TMP_Text _serverName;
		private ServerInfo _serverInfo;

		public void Init(ServerInfo info)
		{
			_serverInfo = info;
			_connectButton.onClick.AddListener(OnClick);
			_serverName.text = info.Name;
		}

		public void Refresh(bool active)
		{
			_connectButton.interactable = active;
		}

		private void OnClick()
		{
			FCGameEventsManager.Publish(this, new ServerRecordConnectClickedEvent(_serverInfo));
		}
	}
}