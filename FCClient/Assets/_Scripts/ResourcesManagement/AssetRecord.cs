using System.Collections.Generic;
using System.Threading.Tasks;
using FCData.Common;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace ResourcesManagement
{
	public class AssetRecord
	{
		public string Name { get; private set; }
		public bool Loaded;
		public bool Loading;
		private Object _object;
		private int _refCount;
		private HashSet<Object> _refHolders = new HashSet<Object>();

		public AssetRecord(string name)
		{
			Name = name;
		}

		public AssetRecord(string name, Object obj)
		{
			Name = name;
			_object = obj;
			Loaded = true;
		}

		/// <summary>
		/// Decrease ref count.
		/// </summary>
		public void Release(Object refHolder)
		{
			if (!_refHolders.Contains(refHolder))
			{
				this.Log($"{refHolder.name} trying to release asset {Name} but he is not a reference holder.", LogLevel.Warning);
			}
			else
			{
				--_refCount;
				_refHolders.Remove(refHolder);
			}

			if (_refCount < 1)
			{
				Addressables.Release(_object);
			}
		}

		public async Task Load(Object whoIsLoading)
		{
			if (!_refHolders.Contains(whoIsLoading))
			{
				++_refCount;
				_refHolders.Add(whoIsLoading);
			}

			if (!Loaded && !Loading)
			{
				await InternalLoad().ConfigureAwait(true);
			}
		}

		private async Task InternalLoad()
		{
			Loading = true;
			_object = await Addressables.LoadAssetAsync<Object>(Name).Task;
			Loading = false;
			Loaded = true;
		}

		public async Task<T> GetAsset<T>(Object whoAskingFor) where T : Object
		{
			await Load(whoAskingFor).ConfigureAwait(true);
			return _object as T;
		}
	}
}
