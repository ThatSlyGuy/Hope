﻿using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class which initializes and manages all contracts for the game.
/// </summary>
public class TokenContractManager
{

    public static event Action OnTokensLoaded;
    public static event Action<TradableAsset> OnTokenAdded;
    public static event Action<string> OnTokenRemoved;

    private readonly Settings settings;
    private readonly PopupManager popupManager;
    private readonly TradableAssetImageManager tradableAssetImageManager;
    private readonly UserWalletManager userWalletManager;

    private readonly Queue<string> tokensToInitialize;
    private readonly Dictionary<string, string> addressSymbolPair;
    private readonly Dictionary<string, int> addressButtonIndices;

    private int tokenPrefIndex,
                savedTokenCount,
                counter;

    /// <summary>
    /// Initializes the TokenContractManager by creating all collections and getting the settings.
    /// </summary>
    /// <param name="settings"> The settings to use with this TokenContractManager. </param>
    /// <param name="popupManager"> The active PopupManager. </param>
    public TokenContractManager(Settings settings, 
        PopupManager popupManager,
        TradableAssetImageManager tradableAssetImageManager,
        UserWalletManager userWalletManager) : base()
    {
        this.settings = settings;
        this.popupManager = popupManager;
        this.tradableAssetImageManager = tradableAssetImageManager;
        this.userWalletManager = userWalletManager;

        tokensToInitialize = new Queue<string>();
        addressSymbolPair = new Dictionary<string, string>();
        addressButtonIndices = new Dictionary<string, int>();

        AddSavedTokensToQueue();
    }

    /// <summary>
    /// Gets the index of an asset which was saved to the prefs.
    /// </summary>
    /// <param name="tokenAddress"> The address of the token to find the index for. </param>
    /// <returns> The index of the token. </returns>
    public int GetTokenIndex(string tokenAddress)
    {
        if (!addressButtonIndices.ContainsKey(tokenAddress))
            return 0;

        return addressButtonIndices[tokenAddress];
    }

    /// <summary>
    /// Adds a token to the list of tokens in the ContractManager.
    /// </summary>
    /// <param name="tokenAddress"> The token address of the token to add to the ContractManager. </param>
    public void AddToken(string tokenAddress)
    {
        var fixedAddress = tokenAddress.ToLower();

        if (SecurePlayerPrefs.HasKey(fixedAddress))
            return;

        popupManager.GetPopup<LoadingPopup>();

        InitializeToken(fixedAddress, null, (abi, asset) =>
        {
            UpdateTradableAssets(asset, () =>
            {
                string tokenPref = fixedAddress + "-" + asset.AssetSymbol;

                SecurePlayerPrefs.SetString(settings.tokenPrefName + (tokenPrefIndex++), tokenPref);
                SecurePlayerPrefs.SetString(tokenPref, abi);

                addressSymbolPair.Add(fixedAddress, asset.AssetSymbol);
                popupManager.CloseActivePopup();
            });
        });
    }

    /// <summary>
    /// Removes the token address from the list of tokens in the player prefs and the TradableAssetManager.
    /// </summary>
    /// <param name="addressToRemove"> The contract address of the token to remove. </param>
    public void RemoveToken(string addressToRemove)
    {
        bool startRemoving = false;

        for (int i = 0; ; i++)
        {
            string prefName = settings.tokenPrefName + i;
            string nextPrefName = settings.tokenPrefName + (i + 1);

            if (!SecurePlayerPrefs.HasKey(prefName))
                break;

            string tokenPref = SecurePlayerPrefs.GetString(prefName);
            string tokenAddress = GetTokenAddressFromPref(tokenPref);

            if (tokenAddress.EqualsIgnoreCase(addressToRemove))
            {
                tokenPrefIndex--;
                startRemoving = true;

                addressSymbolPair.Remove(tokenAddress);
                SecurePlayerPrefs.DeleteKey(addressToRemove + "-" + GetTokenSymbolFromPref(tokenPref));
                OnTokenRemoved?.Invoke(addressToRemove);
            }

            if (startRemoving)
            {
                SecurePlayerPrefs.DeleteKey(prefName);

                if (!SecurePlayerPrefs.HasKey(nextPrefName))
                    break;

                SecurePlayerPrefs.SetString(prefName, SecurePlayerPrefs.GetString(nextPrefName));
            }
        }

    }

    /// <summary>
    /// Starts to load the tokens which were put in queue earlier.
    /// Also adds the first TradableAsset which would be the EtherAsset.
    /// </summary>
    /// <param name="onTokensLoaded"> Action to call once the tokens have finshed loading. </param>
    public void StartTokenLoad(Action onTokensLoaded = null)
    {
        var popup = popupManager.GetPopup<LoadingPopup>();
        popup.SetLoadingText("wallet");

        var loadAction = (() => popupManager.CloseActivePopup()) + onTokensLoaded;

        new EtherAsset(asset => UpdateTradableAssets(asset, () => CheckLoadStatus(loadAction)), tradableAssetImageManager, userWalletManager);

        LoadTokensFromQueue(loadAction);
    }

    /// <summary>
    /// Adds the tokens saved in the PlayerPrefs to the queue.
    /// Will be fully loaded later.
    /// </summary>
    private void AddSavedTokensToQueue()
    {
        for (int i = 0; ; i++)
        {
            string prefName = settings.tokenPrefName + i;

            if (!SecurePlayerPrefs.HasKey(prefName))
            {
                tokenPrefIndex = i;
                break;
            }

            string addressPref = SecurePlayerPrefs.GetString(prefName);

            tokensToInitialize.Enqueue(addressPref);
            addressButtonIndices.Add(GetTokenAddressFromPref(addressPref), i + 1);
        }

        savedTokenCount = tokensToInitialize.Count + 1;
    }

    /// <summary>
    /// Loads the tokens from the TokensToInitialize queue recursively, until there are no more tokens to be loaded.
    /// </summary>
    /// <param name="onLoadingFinished"> Action to call once the token loading has completed. </param>
    private void LoadTokensFromQueue(Action onLoadingFinished = null)
    {
        if (tokensToInitialize.Count == 0)
            return;

        string tokenPref = tokensToInitialize.Dequeue();
        string tokenAbi = SecurePlayerPrefs.GetString(tokenPref);
        string tokenAddress = GetTokenAddressFromPref(tokenPref);
        string tokenSymbol = GetTokenSymbolFromPref(tokenPref);

        if (addressSymbolPair.ContainsKey(tokenAddress))
        {
            onLoadingFinished?.Invoke();
            return;
        }

        addressSymbolPair.Add(tokenAddress, tokenSymbol);
        InitializeToken(tokenAddress, tokenAbi, (abi, asset) => UpdateTradableAssets(asset, () => CheckLoadStatus(onLoadingFinished)));
        LoadTokensFromQueue(onLoadingFinished);
    }

    /// <summary>
    /// Checks the load status of the tokens, and if it is finished, executes the action and the event.
    /// </summary>
    /// <param name="onLoadFinished"> The action to execute once the load has finished. </param>
    private void CheckLoadStatus(Action onLoadFinished)
    {
        if (++counter < savedTokenCount)
            return;

        OnTokensLoaded?.Invoke();
        onLoadFinished?.Invoke();
    }

    /// <summary>
    /// Gets the token address from the token player pref.
    /// </summary>
    /// <param name="tokenPref"> The player pref of the token. </param>
    /// <returns> The token address of this pref. </returns>
    private string GetTokenAddressFromPref(string tokenPref) => tokenPref.Substring(0, tokenPref.IndexOf("-")).ToLower();

    /// <summary>
    /// Gets the token symbol from the token player pref.
    /// </summary>
    /// <param name="tokenPref"> The player pref of the token. </param>
    /// <returns> The token symbol of this pref. </returns>
    private string GetTokenSymbolFromPref(string tokenPref)
    {
        int symbolStartIndex = tokenPref.IndexOf("-");
        return tokenPref.Substring(symbolStartIndex + 1, tokenPref.Length - symbolStartIndex - 1);
    }

    /// <summary>
    /// Initializes a TokenContract and TokenAsset given a contract address and abi.
    /// </summary>
    /// <param name="contractAddress"> The address of the token contract. </param>
    /// <param name="contractAbi"> The abi of the contract. </param>
    /// <param name="onTokenAssetCreated"> Action called once the TokenContract and TokenAsset have finished being created. String param is abi, TradableAsset param is the TokenAsset. </param>
    private void InitializeToken(string contractAddress, string contractAbi, Action<string, TradableAsset> onTokenAssetCreated)
        => new TokenContract(contractAddress, contractAbi, (contract, abi) 
            => new TokenAsset(contract as TokenContract, asset => onTokenAssetCreated?.Invoke(abi, asset), tradableAssetImageManager, userWalletManager));

    /// <summary>
    /// Adds the newly created TradableAsset to the TradableAssetManager and TradableAssetButtonManager.
    /// </summary>
    /// <param name="tradableAsset"> The asset to add. </param>
    /// <param name="onUpdateFinished"> Action to call once the tradable asset updating has finished. </param>
    private void UpdateTradableAssets(TradableAsset tradableAsset, Action onUpdateFinished)
    {
        if (tradableAsset.AssetSymbol != null)
            OnTokenAdded?.Invoke(tradableAsset);

        onUpdateFinished?.Invoke();
    }

    /// <summary>
    /// Class which contains the settings for the TokenContractManager.
    /// </summary>
    [Serializable]
    public class Settings
    {
        public string tokenPrefName;
    }
}