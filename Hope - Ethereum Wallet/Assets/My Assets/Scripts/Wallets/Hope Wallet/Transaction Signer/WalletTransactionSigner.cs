﻿using Nethereum.HdWallet;
using Nethereum.JsonRpc.UnityClient;
using System;

/// <summary>
/// Class used for signing transactions before sending them.
/// </summary>
public sealed class WalletTransactionSigner
{
    private readonly MemoryEncryptor passwordEncryptor;
    private readonly WalletDecryptor walletDecryptor;
    private readonly HopeWalletInfoManager hopeWalletInfoManager;
    private readonly EthereumNetworkManager ethereumNetworkManager;

    /// <summary>
    /// Initializes the <see cref="WalletTransactionSigner"/> by assigning all references.
    /// </summary>
    /// <param name="playerPrefPassword"> The <see cref="PlayerPrefPasswordDerivation"/> instance to assign to the <see cref="WalletDecryptor"/>. </param>
    /// <param name="dynamicDataCache"> The active <see cref="DynamicDataCache"/> to assign to the <see cref="WalletDecryptor"/>. </param>
    /// <param name="ethereumNetworkManager"> The active <see cref="EthereumNetworkManager"/>. </param>
    /// <param name="passwordEncryptor"> The <see cref="MemoryEncryptor"/> instance used to encrypt the password. </param>
    /// <param name="hopeWalletInfoManager"> The active <see cref="HopeWalletInfoManager"/>. </param>
    public WalletTransactionSigner(
        PlayerPrefPasswordDerivation playerPrefPassword,
        DynamicDataCache dynamicDataCache,
        EthereumNetworkManager ethereumNetworkManager,
        MemoryEncryptor passwordEncryptor,
        HopeWalletInfoManager hopeWalletInfoManager)
    {
        this.ethereumNetworkManager = ethereumNetworkManager;
        this.passwordEncryptor = passwordEncryptor;
        this.hopeWalletInfoManager = hopeWalletInfoManager;

        walletDecryptor = new WalletDecryptor(playerPrefPassword, dynamicDataCache);
    }

    /// <summary>
    /// Signs a transaction and passes the result through an Action as a <see cref="TransactionSignedUnityRequest"/>.
    /// </summary>
    /// <param name="walletAddress"> The wallet address to sign the transaction with. </param>
    /// <param name="path"> The path of the wallet to sign the transaction with. </param>
    /// <param name="encryptedPasswordBytes"> The encrypted password to use to decrypt the wallet. </param>
    /// <param name="onRequestReceived"> Action called with the <see cref="TransactionSignedUnityRequest"/> once the transaction was signed. </param>
    [SecureCallEnd]
    public void SignTransaction(string walletAddress, string path, byte[] encryptedPasswordBytes, Action<TransactionSignedUnityRequest> onRequestReceived)
    {
        var plainTextBytes = passwordEncryptor.Decrypt(encryptedPasswordBytes);
        walletDecryptor.DecryptWallet(hopeWalletInfoManager.GetWalletInfo(walletAddress), plainTextBytes, seed =>
        {
            TransactionSignedUnityRequest request = new TransactionSignedUnityRequest(
                new Wallet(seed, path).GetAccount(walletAddress),
                ethereumNetworkManager.CurrentNetwork.NetworkUrl);

            MainThreadExecutor.QueueAction(() => onRequestReceived?.Invoke(request));
            ClearData(seed, plainTextBytes);
        });
    }

    /// <summary>
    /// Clears the <see langword="byte"/>[] data of the wallet seed and collects any garbage.
    /// </summary>
    /// <param name="seed"> The <see langword="byte"/>[] seed used to sign the transaction. </param>
    private void ClearData(params byte[][] seed)
    {
        foreach (var byteData in seed)
            byteData.ClearBytes();

        GC.Collect();
    }
}