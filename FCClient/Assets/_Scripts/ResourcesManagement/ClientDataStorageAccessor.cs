using System.IO;
using FCData;
using FCData.Attributes;
using FCData.DataUtils;
using UnityEngine;

namespace ResourcesManagement
{
	[FCRegister(true)]
	public class ClientDataStorageAccessor : IDataStorageAccessor
	{
		private readonly IFCNetworkSettings _networkSettings;
		private string _storagePath = null;

		public ClientDataStorageAccessor(IFCNetworkSettings networkSettings)
		{
			_networkSettings = networkSettings;
		}

		public string GetBaseStoragePath() => _storagePath ?? (_storagePath = GetBaseStoragePathInternal());

		private string GetBaseStoragePathInternal()
		{
			return Path.Combine(Path.Combine(Application.persistentDataPath, "RageQuitGames"), _networkSettings.ServerName);
		}
	}
}
