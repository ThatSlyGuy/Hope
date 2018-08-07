﻿using System;
using System.Collections.Generic;

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

    private readonly Queue<string> tokensToInitialize = new Queue<string>();
    private readonly Dictionary<string, int> addressButtonIndices = new Dictionary<string, int>();

    private readonly SecurePlayerPrefList<TokenInfoJson> tokens;

    private int tokenPrefIndex,
                savedTokenCount,
                counter;

    /// <summary>
    /// Initializes the TokenContractManager by creating all collections and getting the settings.
    /// </summary>
    /// <param name="settings"> The settings to use with this TokenContractManager. </param>
    /// <param name="popupManager"> The active PopupManager. </param>
    /// <param name="tradableAssetImageManager"> The active TradableAssetImageManager. </param>
    /// <param name="userWalletManager"></param>
    public TokenContractManager(Settings settings,
        PopupManager popupManager,
        TradableAssetImageManager tradableAssetImageManager,
        UserWalletManager userWalletManager)
    {
        this.settings = settings;
        this.popupManager = popupManager;
        this.tradableAssetImageManager = tradableAssetImageManager;
        this.userWalletManager = userWalletManager;

        tokens = new SecurePlayerPrefList<TokenInfoJson>(settings.tokenPrefName);
        //AddSavedTokensToQueue();
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
    /// <param name="ignorePopup"> Whether the loading popup should be ignored. </param>
    public void AddToken(string tokenAddress)
    {
        //var tokenPref = tokenAddress.ToLower();

        //if (SecurePlayerPrefs.HasKey(tokenPref))
        //    return;

        //if (!ignorePopup)
        //    popupManager.GetPopup<LoadingPopup>();

        //InitializeToken(tokenPref, null, (abi, asset) =>
        //{
        //    UpdateTradableAssets(asset, () =>
        //    {
        //        SecurePlayerPrefs.SetString(settings.tokenPrefName + (tokenPrefIndex++), tokenPref);
        //        SecurePlayerPrefs.SetString(tokenPref, abi);
        //        popupManager.CloseActivePopup();
        //    });
        //});
        AddToken(tokenAddress, true);
    }

    public void AddToken(string tokenAddress, bool showLoadingPopup)
    {
        tokenAddress = tokenAddress.ToLower();

        if (tokens.Contains(tokenAddress))
            return;

        if (showLoadingPopup)
            popupManager.GetPopup<LoadingPopup>();

        ERC20 erc20Token = null;
        erc20Token = new ERC20(tokenAddress, () => AddToken(erc20Token.ContractAddress, erc20Token.Name, erc20Token.Symbol, erc20Token.Decimals.Value));
    }

    public void AddToken(string tokenAddress, string tokenName, string tokenSymbol, int tokenDecimals)
    {
        ERC20 erc20Token = new ERC20(tokenAddress, tokenName, tokenSymbol, tokenDecimals);
        tokens.Add(new TokenInfoJson(tokenAddress, tokenName, tokenSymbol, tokenDecimals));

        new ERC20TokenAsset(erc20Token, asset => UpdateTradableAssets(asset, () =>
        {
            if (popupManager.ActivePopupType == typeof(LoadingPopup))
                popupManager.CloseActivePopup();
        }), tradableAssetImageManager, userWalletManager);
    }

    /// <summary>
    /// Removes the token address from the list of tokens in the player prefs and the TradableAssetManager.
    /// </summary>
    /// <param name="tokenAddress"> The contract address of the token to remove. </param>
    public void RemoveToken(string tokenAddress)
    {
        //bool startRemoving = false;

        //for (int i = 0; ; i++)
        //{
        //    string prefName = settings.tokenPrefName + i;
        //    string nextPrefName = settings.tokenPrefName + (i + 1);

        //    if (!SecurePlayerPrefs.HasKey(prefName))
        //        break;

        //    string tokenAddress = SecurePlayerPrefs.GetString(prefName);

        //    if (tokenAddress.EqualsIgnoreCase(addressToRemove))
        //    {
        //        tokenPrefIndex--;
        //        startRemoving = true;

        //        SecurePlayerPrefs.DeleteKey(tokenAddress);
        //        OnTokenRemoved?.Invoke(addressToRemove);
        //    }

        //    if (startRemoving)
        //    {
        //        SecurePlayerPrefs.DeleteKey(prefName);

        //        if (!SecurePlayerPrefs.HasKey(nextPrefName))
        //            break;

        //        SecurePlayerPrefs.SetString(prefName, SecurePlayerPrefs.GetString(nextPrefName));
        //    }
        //}
        tokens.Remove(tokenAddress);
        OnTokenRemoved?.Invoke(tokenAddress);
    }

    /// <summary>
    /// Starts to load the tokens which were put in queue earlier.
    /// Also adds the first TradableAsset which would be the EtherAsset.
    /// </summary>
    /// <param name="onTokensLoaded"> Action to call once the tokens have finshed loading. </param>
    public void StartTokenLoad(Action onTokensLoaded)
    {
        var popup = popupManager.GetPopup<LoadingPopup>();
        popup.Text = "Loading wallet";

        Action onLoadFinished = (() => popupManager.CloseAllPopups()) + onTokensLoaded;
        new EtherAsset(asset => UpdateTradableAssets(asset, () => LoadToken(0, onLoadFinished)), tradableAssetImageManager, userWalletManager);
    }

    private void LoadToken(int index, Action onTokenLoadFinished)
    {
        if (CheckLoadStatus(index, onTokenLoadFinished))
            return;

        TokenInfoJson tokenInfo = tokens[index];
        ERC20 erc20Token = new ERC20(tokenInfo.address, tokenInfo.name, tokenInfo.symbol, tokenInfo.decimals);
        new ERC20TokenAsset(erc20Token, asset => UpdateTradableAssets(asset, () => LoadToken(++index, onTokenLoadFinished)), tradableAssetImageManager, userWalletManager);
    }

    private bool CheckLoadStatus(int index, Action onTokenLoadFinished)
    {
        if (index == tokens.Count)
        {
            OnTokensLoaded?.Invoke();
            onTokenLoadFinished?.Invoke();
            return true;
        }

        return false;
    }

    /// <summary>
    /// Adds the tokens saved in the PlayerPrefs to the queue.
    /// Will be fully loaded later.
    /// </summary>
    //private void AddSavedTokensToQueue()
    //{
    //    for (int i = 0; ; i++)
    //    {
    //        string prefName = settings.tokenPrefName + i;

    //        if (!SecurePlayerPrefs.HasKey(prefName))
    //        {
    //            tokenPrefIndex = i;
    //            break;
    //        }

    //        string addressPref = SecurePlayerPrefs.GetString(prefName);

    //        tokensToInitialize.Enqueue(addressPref);
    //        addressButtonIndices.Add(addressPref, i + 1);
    //    }

    //    savedTokenCount = tokensToInitialize.Count + 1;
    //}

    /// <summary>
    /// Loads the tokens from the TokensToInitialize queue recursively, until there are no more tokens to be loaded.
    /// </summary>
    /// <param name="onLoadingFinished"> Action to call once the token loading has completed. </param>
    //private void LoadTokensFromQueue(Action onLoadingFinished = null)
    //{
    //    if (tokensToInitialize.Count == 0)
    //        return;

    //    string tokenAddress = tokensToInitialize.Dequeue();
    //    string tokenAbi = SecurePlayerPrefs.GetString(tokenAddress);

    //    InitializeToken(tokenAddress, tokenAbi, (_, asset) => UpdateTradableAssets(asset, () => CheckLoadStatus(onLoadingFinished)));
    //    LoadTokensFromQueue(onLoadingFinished);
    //}

    /// <summary>
    /// Checks the load status of the tokens, and if it is finished, executes the action and the event.
    /// </summary>
    /// <param name="onLoadFinished"> The action to execute once the load has finished. </param>
    //private void CheckLoadStatus(Action onLoadFinished)
    //{
    //    if (++counter < savedTokenCount)
    //        return;

    //    OnTokensLoaded?.Invoke();
    //    onLoadFinished?.Invoke();
    //}

    /// <summary>
    /// Initializes a TokenContract and TokenAsset given a contract address and abi.
    /// </summary>
    /// <param name="contractAddress"> The address of the token contract. </param>
    /// <param name="contractAbi"> The abi of the contract. </param>
    /// <param name="onTokenAssetCreated"> Action called once the TokenContract and TokenAsset have finished being created. String param is abi, TradableAsset param is the TokenAsset. </param>
    //private void InitializeToken(string contractAddress, string contractAbi, Action<string, TradableAsset> onTokenAssetCreated)
    //    => new TokenContract(contractAddress, contractAbi, (contract, abi)
    //        => new ERC20TokenAsset(contract as TokenContract, asset => onTokenAssetCreated?.Invoke(abi, asset), tradableAssetImageManager, userWalletManager));

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