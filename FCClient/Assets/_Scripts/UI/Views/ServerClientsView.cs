using System.Collections.Generic;
using FCData.Network.Server.Management;
using Graphics.MVVM;
using UI.ViewModels;
using UnityEngine;
using UnityEngine.UI;
#pragma warning disable 649

namespace UI.Views
{
	public class ServerClientsView : View<ServerClientsViewModel>
	{
		[SerializeField] private ServerClientViewRecord _recordPrefab;
		[SerializeField] private Transform _grid;
		[SerializeField] private Button _refreshButton;
		private List<ServerClientViewRecord> _records;

		protected override void OnAwake()
		{
			_records = new List<ServerClientViewRecord>();
			_refreshButton.onClick.AddListener(Refresh);
			base.OnAwake();
			InvokeRepeating(nameof(Refresh), 1, 1);
		}

		private void Clear()
		{
			foreach (ServerClientViewRecord record in _records)
			{
				Destroy(record.gameObject);
			}
			_records.Clear();
		}

		[PropertyBind]
		private List<ConnectedClientData> _clients
		{
			get => null;
			set
			{
				Clear();
				if (value != null)
				{
					foreach (ConnectedClientData recordData in value)
					{
						var instance = Instantiate(_recordPrefab);
						instance.Init(recordData);
						instance.transform.SetParent(_grid);
						_records.Add(instance);
					}
				}
			}
		}

		private void Refresh()
		{
			ViewModel.Refresh();
		}
	}
}
