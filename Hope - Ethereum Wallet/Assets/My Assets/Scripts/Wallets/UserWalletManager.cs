﻿using Nethereum.Hex.HexTypes;
using Nethereum.JsonRpc.UnityClient;
using System;
using System.Numerics;

/// <summary>
/// Class for managing the ethereum wallet of the user.
/// </summary>
public sealed class UserWalletManager
{
    private readonly LedgerWallet ledgerWallet;
    private readonly TrezorWallet trezorWallet;
    private readonly HopeWallet hopeWallet;

    private IWallet activeWallet;

    private int accountNumber;

    /// <summary>
    /// The address of the main UserWallet.
    /// </summary>
    public string WalletAddress => activeWallet.GetAddress(accountNumber);

    /// <summary>
    /// Initializes the UserWallet given the settings to apply.
    /// </summary>
    /// <param name="settings"> The settings to initialize the wallet with. </param>
    /// <param name="popupManager"> The PopupManager to assign to the wallet. </param>
    /// <param name="ethereumNetworkManager"> The active EthereumNetworkManager to assign to the wallet. </param>
    /// <param name="dynamicDataCache"> The active DynamicDataCache. </param>
    /// <param name="ledgerWallet"> The active LedgerWallet. </param>
    /// <param name="trezorWallet"> The active TrezorWallet. </param>
    /// <param name="userWalletInfoManager"> The active UserWalletInfoManager. </param>
    /// <param name="walletSettings"> The player pref settings for the UserWallet. </param>
    public UserWalletManager(
        Settings settings,
        PopupManager popupManager,
        EthereumNetworkManager ethereumNetworkManager,
        DynamicDataCache dynamicDataCache,
        LedgerWallet ledgerWallet,
        TrezorWallet trezorWallet,
        HopeWalletInfoManager userWalletInfoManager,
        HopeWalletInfoManager.Settings walletSettings)
    {
        this.ledgerWallet = ledgerWallet;
        this.trezorWallet = trezorWallet;

        settings.safePassword.AddCharLookups(settings.safePasswordCharLookups);

        hopeWallet = new HopeWallet(settings.safePassword, popupManager, ethereumNetworkManager.CurrentNetwork, dynamicDataCache, userWalletInfoManager, walletSettings);
        activeWallet = hopeWallet;
    }

    /// <summary>
    /// Wrapper method for transferring a specified asset from the user's wallet to another ethereum address.
    /// </summary>
    /// <param name="tradableAsset"> The asset to transfer from the user's wallet. </param>
    /// <param name="gasLimit"> The gas limit to use for this asset transfer transaction. </param>
    /// <param name="gasPrice"> The gas price to use for this asset transfer transaction. </param>
    /// <param name="address"> The address to transfer the asset to. </param>
    /// <param name="amount"> The amount of the specified asset to send. </param>
    public void TransferAsset(TradableAsset tradableAsset, HexBigInteger gasLimit, HexBigInteger gasPrice, string address, dynamic amount)
        => tradableAsset.Transfer(this, gasLimit, gasPrice, address, amount);

    /// <summary>
    /// Signs a transaction using the main UserWallet.
    /// </summary>
    /// <typeparam name="T"> The type of the popup to display the transaction confirmation for. </typeparam>
    /// <param name="onTransactionSigned"> The action to call if the transaction is confirmed and signed. </param>
    /// <param name="gasLimit"> The gas limit to use with the transaction. </param>
    /// <param name="gasPrice"> The gas price to use with the transaction. </param>
    /// <param name="transactionInput"> The input that goes along with the transaction request. </param>
    [SecureCallEnd]
    public void SignTransaction<T>(
        Action<TransactionSignedUnityRequest> onTransactionSigned,
        BigInteger gasLimit,
        BigInteger gasPrice,
        params object[] transactionInput) where T : ConfirmTransactionPopupBase<T>
    {
        activeWallet.SignTransaction<T>(onTransactionSigned, gasLimit, gasPrice, WalletAddress, transactionInput);
    }

    /// <summary>
    /// Switches the active WalletType.
    /// </summary>
    /// <param name="newWalletType"> The new WalletType to use to get addresses/sign transactions. </param>
    public void SwitchWallet(WalletType newWalletType)
    {
        switch (newWalletType)
        {
            case WalletType.Ledger:
                activeWallet = ledgerWallet;
                break;
            case WalletType.Trezor:
                activeWallet = trezorWallet;
                break;
            case WalletType.Hope:
                activeWallet = hopeWallet;
                break;
        }
    }

    /// <summary>
    /// Attempts to load a wallet given a password.
    /// Calls the action if the wallet loaded successfully.
    /// </summary>
    [SecureCallEnd]
    public void UnlockWallet() => hopeWallet.Unlock();

    /// <summary>
    /// Attempts to create a wallet given a mnemonic phrase.
    /// Calls the action with the state of successful or unsuccessful wallet creation.
    /// </summary>
    [SecureCallEnd]
    public void CreateWallet() => hopeWallet.Create();

    /// <summary>
    /// Enum representing the type of the wallet.
    /// </summary>
    public enum WalletType
    {
        Ledger,
        Trezor,
        Hope
    }

    /// <summary>
    /// Class which contains all settings related to the wallet storage/loading/unlocking.
    /// </summary>
    [Serializable]
    public class Settings
    {
        public PlayerPrefPassword safePassword;
        public string[] safePasswordCharLookups;
    }
}