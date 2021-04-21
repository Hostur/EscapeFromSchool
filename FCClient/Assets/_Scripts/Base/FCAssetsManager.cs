using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using FCData.Common;
using ResourcesManagement;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Object = UnityEngine.Object;

public static class FCAssetsManager
{
	/// <summary>
	/// Keep reference to already loaded assets.
	/// We can preload assets into this dictionary to load them faster in the other place.
	/// Preloading assets doesn't increase their ref counts so preloader is not a real asset user.
	/// </summary>
	private static Dictionary<string, AssetRecord> _assets = new Dictionary<string, AssetRecord>(8);

	private static void OnLabelReferenceLoaded<T>(IList<T> loaded) where T : Object
	{
		foreach (T t in loaded)
		{
			if (!_assets.ContainsKey(t.name))
			{
				_assets.Add(t.name, new AssetRecord(t.name, t));
			}
		}
	}

	private static void OnAssetReferenceLoaded<T>(T loaded) where T : Object
	{
		if (!_assets.ContainsKey(loaded.name))
		{
			_assets.Add(loaded.name, new AssetRecord(loaded.name, loaded));
		}
	}

	//public static async Task<T> InstantiateAsync<T>(this Object whoIsAskingFor, AssetReference assetReference, Vector3 position, Quaternion rotation)
	//	where T : Object
	//{
	//	T result = await assetReference.InstantiateAsync().Task as T;
	//	OnAssetReferenceLoaded(result);

	//	var asset = await _assets[result.name].GetAsset<T>(whoIsAskingFor).ConfigureAwait(true);
	//	return await Addressables.InstantiateAsync(asset, position, rotation).Task as T;
	//}

	public static IEnumerator LoadAsync(this Object whoIsAskingFor, AssetReference assetReference,
		Action<GameObject> callback)
	{
		Addressables.LoadAssetAsync<GameObject>(assetReference).Completed += op =>
		{
			if (op.Status == AsyncOperationStatus.Succeeded)
			{
				callback?.Invoke(op.Result);
			}
		};
		yield return null;
		//AsyncOperationHandle<GameObject> handle = assetReference.LoadAssetAsync<GameObject>();
		//	yield return handle;
		//	callback?.Invoke(handle.Result as T);
	}

	//public static async Task<T> Load<T>(this Object whoIsAskingFor, AssetReference assetReference) where T : Object
	//{
	//	T result = await assetReference.InstantiateAsync().Task as T;
	//	OnAssetReferenceLoaded(result);

	//	return await _assets[result.name].GetAsset<T>(whoIsAskingFor).ConfigureAwait(true);
	//}

	public static async Task PreLoad<T>(this AssetLabelReference labelReference) where T : Object
	{
		var result = await Addressables.LoadAssetsAsync<T>(labelReference.labelString, null).Task;
		OnLabelReferenceLoaded<T>(result);
	}

	public static void Release(this Object caller, Object asset)
	{
		if (!_assets.ContainsKey(asset.name))
		{
			caller.Log($"{caller.name} trying to release non existing asset: {asset.name}");
			return;
		}

		_assets[asset.name].Release(caller);
	}
}


