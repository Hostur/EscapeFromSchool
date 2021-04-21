using Autofac;
using Client;
using FCData;
using FCData.Common;
using FCData.DataUtils;
using FCData.DataUtils.DI;
using FCData.Network.Client;
using FCData.Network.Client.Connector;
using FCData.Network.Multicast;
using FCData.Network.Server.Management;
using FCData.WebService;
using FCModules;
using Localization;
using ResourcesManagement;
using UI.ViewModels;

public class FCClientAssemblyDefinition : FCAssemblyDefinition
{
	private readonly IFCModulesConfiguration _modulesConfiguration;

	public FCClientAssemblyDefinition(IFCModulesConfiguration modulesConfiguration, IFCNetworkSettings networkSettings) : base(networkSettings)
	{
		_modulesConfiguration = modulesConfiguration;
	}

	public override void Register(ContainerBuilder builder, IFCLogger logger)
	{
		builder.Register(c => logger)
			.As<IFCLogger>()
			.SingleInstance();

		builder.Register(c => new ClientDisconnectionHandler(c.Resolve<FCClientNetworkData>()))
			.As<ClientDisconnectionHandler>()
			.As<IClientDisconnectionHandler>()
			.SingleInstance();

		builder.Register(c => _modulesConfiguration)
			.As<IFCModulesConfiguration>()
			.SingleInstance();

		builder.Register(c => NetworkSettings)
			.As<IFCNetworkSettings>()
			.Keyed<object>(typeof(IFCNetworkSettings).FullName)
			.SingleInstance();

		builder.Register(c => new GameTranslator())
			.As<GameTranslator>()
			.Keyed<object>(typeof(GameTranslator).FullName)
			.SingleInstance();

		builder.Register(c => new ClientDataStorageAccessor(c.Resolve<IFCNetworkSettings>()))
			.As<IDataStorageAccessor>()
			.SingleInstance();

		builder.Register(c => new StandaloneDataStorage(c.Resolve<IDataStorageAccessor>()))
			.As<StandaloneDataStorage>()
			.As<IDataStorage>()
			.Keyed<object>(typeof(IDataStorage).FullName)
			.Keyed<object>(typeof(StandaloneDataStorage).FullName)
			.SingleInstance();

		RegisterViewModels(builder);
	}

	private void RegisterViewModels(ContainerBuilder builder)
	{
		builder.Register(c => new AvailableServersViewModel(c.Resolve<IClientConnector>(),
				c.Resolve<IFCWebServiceProvider>(),
				c.Resolve<FCClientNetworkData>(),
				c.Resolve<MulticastReceiver>()))
			.As<AvailableServersViewModel>()
			.Keyed<object>(typeof(AvailableServersViewModel).FullName)
			.SingleInstance();

		builder.Register(c => new ServerClientsViewModel(c.Resolve<FCConnectedClients>()))
			.As<ServerClientsViewModel>()
			.Keyed<object>(typeof(ServerClientsViewModel).FullName)
			.SingleInstance();
	}
}
