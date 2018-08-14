﻿using Hope.Security.HashGeneration;
using Hope.Utils.Random;
using Nethereum.Hex.HexConvertors.Extensions;
using Org.BouncyCastle.Crypto.Digests;
using System;
using System.Threading.Tasks;

/// <summary>
/// Class used for decrypting wallet data.
/// </summary>
public sealed class WalletDecryptor : SecureObject
{
    private readonly PlayerPrefPassword playerPrefPassword;
    private readonly DynamicDataCache dynamicDataCache;
    private readonly UserWalletInfoManager.Settings walletSettings;

    /// <summary>
    /// Initializes the <see cref="WalletDecryptor"/> with the references to needed objects.
    /// </summary>
    /// <param name="playerPrefPassword"> The <see cref="PlayerPrefPassword"/> object used to encrypt the wallet data. </param>
    /// <param name="dynamicDataCache"> The <see cref="DynamicDataCache"/> used for retrieving the number of the wallet we are decrypting. </param>
    /// <param name="walletSettings"> The settings for the <see cref="UserWallet"/>. </param>
    public WalletDecryptor(
        PlayerPrefPassword playerPrefPassword,
        DynamicDataCache dynamicDataCache,
        UserWalletInfoManager.Settings walletSettings)
    {
        this.playerPrefPassword = playerPrefPassword;
        this.dynamicDataCache = dynamicDataCache;
        this.walletSettings = walletSettings;
    }

    /// <summary>
    /// Decrypts the wallet given the user's password.
    /// </summary>
    /// <param name="password"> The user's password to the wallet. </param>
    /// <param name="onWalletDecrypted"> Action called once the wallet has been decrypted, passing the <see langword="byte"/>[] seed of the wallet and <see langword="string"/> wallet derivation. </param>
    public void DecryptWallet(string password, Action<byte[], string> onWalletDecrypted)
    {
        int walletNum = (int)dynamicDataCache.GetData("walletnum");

        MainThreadExecutor.QueueAction(() =>
        {
            playerPrefPassword.PopulatePrefDictionary(walletNum);

            string[] hashLvls = new string[4];
            for (int i = 0; i < hashLvls.Length; i++)
                hashLvls[i] = SecurePlayerPrefs.GetString(walletNum + walletSettings.walletHashLvlPrefName + (i + 1));

            string derivation = SecurePlayerPrefs.GetString(walletSettings.walletDerivationPrefName + walletNum);

            AsyncTaskScheduler.Schedule(() => AsyncDecryptWallet(
                hashLvls,
                SecurePlayerPrefs.GetString(walletSettings.walletDataPrefName + walletNum),
                derivation,
                password,
                walletNum,
                onWalletDecrypted));
        });
    }

    /// <summary>
    /// Decrypts the wallet asynchronously.
    /// </summary>
    /// <param name="hashes"> Different hash levels used for multi level encryption of the wallet seed. </param>
    /// <param name="encryptedSeed"> The encrypted seed of the wallet. </param>
    /// <param name="derivation"> The wallet's derivation path. </param>
    /// <param name="password"> The user's password to the wallet. </param>
    /// <param name="walletNum"> The number of the wallet to decrypt. </param>
    /// <param name="onWalletDecrypted"> Action called once the wallet has been decrypted, passing the <see langword="byte"/>[] seed of the wallet and the <see langword="string"/> wallet derivation. </param>
    /// <returns> Task returned which represents the decryption processing. </returns>
    private async Task AsyncDecryptWallet(
        string[] hashes,
        string encryptedSeed,
        string derivation,
        string password,
        int walletNum,
        Action<byte[], string> onWalletDecrypted)
    {
        string encryptionPassword = playerPrefPassword.ExtractEncryptionPassword(password);

        AdvancedSecureRandom secureRandom = await Task.Run(() =>
            new AdvancedSecureRandom(
                new Blake2bDigest(),
                walletSettings.walletCountPrefName,
                walletSettings.walletDataPrefName,
                walletSettings.walletDerivationPrefName,
                walletSettings.walletEncryptionEntropy,
                walletSettings.walletHashLvlPrefName,
                walletSettings.walletInfoPrefName,
                walletSettings.walletNamePrefName,
                walletSettings.walletPasswordPrefName,
                walletNum,
                encryptionPassword)).ConfigureAwait(false);

        byte[] decryptedSeed = null;
        using (var dataEncryptor = new DataEncryptor(secureRandom))
        {
            string lvl1EncryptHash = (dataEncryptor.Decrypt(hashes[0]) + dataEncryptor.Decrypt(hashes[1])).GetSHA256Hash();
            string lvl2EncryptHash = (dataEncryptor.Decrypt(hashes[2]) + dataEncryptor.Decrypt(hashes[3])).GetSHA256Hash();

            decryptedSeed = dataEncryptor.Decrypt(dataEncryptor.Decrypt(encryptedSeed, lvl2EncryptHash), lvl1EncryptHash).HexToByteArray();
        }

        onWalletDecrypted?.Invoke(decryptedSeed, derivation);
    }
}