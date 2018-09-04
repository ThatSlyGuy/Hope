﻿using System;

/// <summary>
/// Class which manages the different ethereum networks.
/// </summary>
public sealed class EthereumNetworkManager : InjectableSingleton<EthereumNetworkManager>
{
    private readonly Settings settings;

    private readonly EthereumNetwork rinkeby,
                                     mainnet;

    /// <summary>
    /// Gets the currently active network.
    /// </summary>
    public EthereumNetwork CurrentNetwork { get { return GetCurrentNetwork(); } }

    /// <summary>
    /// Initializes the network manager by setting up the different networks.
    /// </summary>
    /// <param name="settings"> The <see cref="Settings"/> to apply to this <see cref="EthereumNetworkManager"/>. </param>
    public EthereumNetworkManager(Settings settings) : base()
    {
        this.settings = settings;

        mainnet = new EthereumNetwork("https://mainnet.infura.io", "https://api.etherscan.io/api?");
        rinkeby = new EthereumNetwork("https://rinkeby.infura.io", "https://api-rinkeby.etherscan.io/api?");
    }

    /// <summary>
    /// Gets the currently active network.
    /// </summary>
    /// <returns> The currently active EthereumNetwork. </returns>
    private EthereumNetwork GetCurrentNetwork()
    {
        switch (settings.networkType)
        {
            case NetworkType.Mainnet:
                return mainnet;
            default:
            case NetworkType.Rinkeby:
                return rinkeby;
        }
    }

    /// <summary>
    /// Class which contains the settings for the ethereum network manager.
    /// </summary>
    [Serializable]
    public class Settings
    {
        public NetworkType networkType;
    }

    /// <summary>
    /// Enum which holds the different network types.
    /// </summary>
    [Serializable]
    public enum NetworkType { Mainnet = 1, Rinkeby = 4 };
}
