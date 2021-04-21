using System.Collections.Generic;
using FCData.WebService.Responses;
using Graphics.MVVM;
using UI.ViewModels;
using UnityEngine;
using UnityEngine.UI;
#pragma warning disable 649

namespace UI.Views
{
	public class AvailableServersView : View<AvailableServersViewModel>
	{
		[SerializeField] private ServerRecord _serverRecordPrefab;
		[SerializeField] private Transform _serversGrid;
		[SerializeField] private List<ServerRecord> _records;
		[SerializeField] [ButtonBind("Refresh")] private Button _refreshButton;

		protected override void OnAwake()
		{
			base.OnAwake();
			InvokeRepeating("Refresh", 1, 10);
			InvokeRepeating("RefreshConnectButtons", 1, 1);
		}

		[PropertyBind]
		private List<ServerInfo> _availableServers
		{
			get => null;
			set
			{
				Clear();
				if (value != null)
				{
					foreach (ServerInfo server in value)
					{
						var instance = Instantiate(_serverRecordPrefab);
						instance.Init(server);
						instance.Refresh(ViewModel.ClientNetworkData.MyId == 0);
						instance.transform.SetParent(_serversGrid);
						_records.Add(instance);
					}
				}
			}
		}

		private void RefreshConnectButtons() => _records.ForEach(c => c.Refresh(ViewModel.ClientNetworkData.MyId == 0));

		private void Refresh() => ViewModel.Refresh();

		private void Clear()
		{
			if (_records != null)
			{
				foreach (ServerRecord serverRecord in _records)
				{
					Destroy(serverRecord.gameObject);
				}
				_records.Clear();
			}
		}
	}
}
