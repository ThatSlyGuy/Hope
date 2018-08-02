using Hope.Security.Injection;
using Hope.Utils.EthereumUtils;
using System;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Zenject;

/// <summary>
/// Class which installs all dependencies.
/// </summary>
public sealed class AppInstaller : MonoInstaller<AppInstaller>
{
    public AppSettingsInstaller appSettings;
    public TradableAssetButtonManager.Settings tradableAssetButtonSettings;
    public EthereumTransactionButtonManager.Settings transactionButtonSettings;
    public WalletListMenu.Settings walletListMenuSettings;
    public UIManager.UIProvider uiProvider;

    /// <summary>
    /// Installs all bindings.
    /// </summary>
    public override void InstallBindings()
    {
        BindSettings();
        BindSingletonTypes();
        BindFactories();
    }

    /// <summary>
    /// Binds all settings dependencies for certain classes.
    /// </summary>
    private void BindSettings()
    {
        Container.BindInstance(tradableAssetButtonSettings).AsSingle().NonLazy();
        Container.BindInstance(transactionButtonSettings).AsSingle().NonLazy();
        Container.BindInstance(walletListMenuSettings).AsSingle().NonLazy();
        Container.BindInstance(uiProvider).AsSingle().NonLazy();
    }

    /// <summary>
    /// Binds all singleton types to their required dependencies.
    /// </summary>
    private void BindSingletonTypes()
    {
        Container.BindInterfacesAndSelfTo<UpdateManager>().AsSingle().NonLazy();
        Container.Bind<PeriodicUpdateManager>().AsSingle().NonLazy();

        Container.Bind<ContractUtils>().AsSingle().NonLazy();
        Container.Bind<GasUtils>().AsSingle().NonLazy();
        Container.Bind<WalletUtils>().AsSingle().NonLazy();

        Container.Bind<MainThreadExecutor>().AsSingle().NonLazy();

        Container.Bind<DynamicDataCache>().AsSingle().NonLazy();

        Container.Bind<CoinList>().AsSingle().NonLazy();

        Container.Bind<SecurePlayerPrefs>().AsSingle().NonLazy();
        Container.Bind<SecurePlayerPrefsAsync>().AsSingle().NonLazy();

        Container.Bind<DebugManager>().AsSingle().NonLazy();
        Container.Bind<ExceptionManager>().AsSingle().NonLazy();
        Container.Bind<AssemblyInjectionDetector>().AsSingle().NonLazy();

        Container.Bind<TradableAssetManager>().AsSingle().NonLazy();
        Container.Bind<TradableAssetButtonManager>().AsSingle().NonLazy();
        Container.Bind<TradableAssetImageManager>().AsSingle().NonLazy();

        Container.Bind<LockPRPSManager>().AsSingle().NonLazy();
        Container.Bind<LockedPRPSManager>().AsSingle().NonLazy();
        Container.Bind<EthereumTransactionManager>().AsSingle().NonLazy();
        Container.Bind<EthereumTransactionButtonManager>().AsSingle().NonLazy();
		Container.Bind<ContactsManager>().AsSingle().NonLazy();

		Container.Bind<UserWalletManager>().AsSingle().NonLazy();

        Container.Bind<TokenContractManager>().AsSingle().NonLazy();

        Container.Bind<EthereumNetworkManager>().AsSingle().NonLazy();
        Container.Bind<GasPriceObserver>().AsSingle().NonLazy();
        Container.Bind<EtherBalanceObserver>().AsSingle().NonLazy();

        Container.BindInstance(GetComponent<UIManager>()).AsSingle().NonLazy();
        Container.Bind<OptionsDropdownActionAssigner>().AsSingle().NonLazy();
        Container.Bind<MenuFactoryManager>().AsSingle().NonLazy();
        Container.Bind<PopupManager>().AsSingle().NonLazy();
        Container.Bind<ButtonClickObserver>().AsSingle().NonLazy();
        Container.Bind<MouseClickObserver>().AsSingle().NonLazy();
        Container.Bind<PopupButtonObserver>().AsSingle().NonLazy();

        Container.Bind<Hodler>().AsSingle().NonLazy();
        Container.Bind<PRPS>().AsSingle().NonLazy();
        Container.Bind<DUBI>().AsSingle().NonLazy();
    }

    /// <summary>
    /// Binds the required factories to their prefabs and spawn transforms.
    /// </summary>
    private void BindFactories()
    {
        BindButtonFactories();
        BindPopupFactories();
        BindMenuFactories();
    }

    /// <summary>
    /// Binds the factories for the buttons.
    /// </summary>
    private void BindButtonFactories()
    {
        BindButtonFactory<TransactionInfoButton>(transactionButtonSettings.spawnTransform);
        BindButtonFactory<AssetButton>(tradableAssetButtonSettings.spawnTransform);
        BindButtonFactory<WalletButton>(walletListMenuSettings.walletButtonSpawnTransform);
        BindButtonFactory<LockedPRPSItemButton>(null);
		BindButtonFactory<ContactButton>(null);
    }

    /// <summary>
    /// Binds the factories for the popups.
    /// </summary>
    private void BindPopupFactories()
    {
        appSettings.uiSettings.menuSettings.popups.ForEach(popup => InvokeGenericMethod(this, "BindPopupFactory", popup.GetComponent<PopupBase>().GetType()));
	}

    /// <summary>
    /// Binds the factories for the ui menus.
    /// </summary>
    private void BindMenuFactories()
    {
        appSettings.uiSettings.menuSettings.menus.ForEach(menu => InvokeGenericMethod(this, "BindMenuFactory", menu.GetComponent<Menu>().GetType()));
    }

    /// <summary>
    /// Binds the types for a menu factory.
    /// </summary>
    /// <typeparam name="TMenu"> The type of the menu. </typeparam>
    private void BindMenuFactory<TMenu>() where TMenu : Menu<TMenu>
    {
        BindFactory<TMenu, Menu<TMenu>.Factory>(uiProvider.uiRoot.transform, appSettings.uiSettings.menuSettings.menus);
    }

    /// <summary>
    /// Binds the types for a popup factory.
    /// </summary>
    /// <typeparam name="TPopup"> The type of the popup. </typeparam>
    private void BindPopupFactory<TPopup>() where TPopup : FactoryPopup<TPopup>
    {
        BindFactory<TPopup, FactoryPopup<TPopup>.Factory>(uiProvider.uiRoot.transform, appSettings.uiSettings.menuSettings.popups);
    }

    /// <summary>
    /// Binds the types for a button factory.
    /// </summary>
    /// <typeparam name="TButton"> The type of the button. </typeparam>
    /// <param name="spawnTransform"> The spawn transform to have the factory create the buttons under. </param>
    private void BindButtonFactory<TButton>(Transform spawnTransform) where TButton : FactoryButton<TButton>
    {
        BindFactory<TButton, FactoryButton<TButton>.Factory>(spawnTransform, appSettings.uiSettings.menuSettings.factoryButtons);
    }

    /// <summary>
    /// Binds a certain factory type under a spawn transform.
    /// </summary>
    /// <typeparam name="TType"> The type of the object we are creating. </typeparam>
    /// <typeparam name="TFactory"> The type of the factory being created. </typeparam>
    /// <param name="spawnTransform"> The transform where the factory will create objects under. </param>
    /// <param name="objectsToSearch"></param>
    private void BindFactory<TType, TFactory>(Transform spawnTransform, GameObject[] objectsToSearch) where TType : MonoBehaviour where TFactory : Factory<TType>
    {
        Container.BindFactory<TType, TFactory>()
                 .FromComponentInNewPrefab(objectsToSearch.Select(obj => obj.GetComponentInChildren<TType>()).Single(type => type != null).gameObject.SelectParent())
                 .UnderObjectTransform(spawnTransform);
    }

    /// <summary>
    /// Invokes a generic method using reflection.
    /// </summary>
    /// <param name="instance"> The instance of the object to invoke the generic method with. </param>
    /// <param name="methodName"> The name of the generic method to invoke. </param>
    /// <param name="genericParams"> The generic parameters of the method. </param>
    /// <param name="methodParams"> The actual parameters of the method. </param>
    private void InvokeGenericMethod(object instance, string methodName, params Type[] genericParams)
    {
        instance.GetType().GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.InvokeMethod | BindingFlags.Instance)
                          .MakeGenericMethod(genericParams)
                          .Invoke(instance, null);
    }
}