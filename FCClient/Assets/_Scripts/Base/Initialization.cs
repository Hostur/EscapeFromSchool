using System;
using System.Collections;
using Base;
using Configuration.NetworkSettings;
using FCData;
using FCData.Common;
using FCData.DataUtils.DI;
using FCModules;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;

#pragma warning disable 649

[Serializable]
public class SetupConfiguration
{
	public SetupType SetupType;

	public IFCNetworkSettings NetworkSettings
	{
		get
		{
			switch (SetupType)
			{
				case SetupType.LocalGameplay: return new LocalGameplayNetworkSettings();
				case SetupType.StandaloneClient: return new StandaloneClientNetworkSettings();
				case SetupType.ClientAndPublicServer: return new ClientAndPublicServerNetworkSettings();
				case SetupType.LocalHostWithGameplay: return new LocalHostWithGameplayNetworkSettings();
				case SetupType.StandaloneServer: return new StandaloneServerNetworkSettings();
				default: throw new Exception("Standalone server setup not supported.");
			}
		}
	}
}

public class Initialization : FCBehaviour
{
	[SerializeField] private SetupConfiguration _setup;

	protected override async void OnAwake()
	{
		base.OnAwake();
		OnStart();
	}

	private void OnStart()
	{
		Initialize(_setup.SetupType.GetConfiguration(), _setup.NetworkSettings);
	}

	private void Initialize(IFCModulesConfiguration modulesConfiguration, IFCNetworkSettings networkSettings)
	{
		God.WorldCreation(
			new FCUnityLogger(),
			new FCClientAssemblyDefinition(modulesConfiguration, networkSettings),
			new FCDataAssemblyDefinition(networkSettings),
			new FCModuleAssemblyDefinition(networkSettings));
		FCLogger.InitLogger(God.PrayFor<IFCLogger>());


		this.Log("Initialized", LogLevel.EditorInfo);
	}
}
