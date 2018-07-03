﻿using Hope.Security.Encryption.DPAPI;
using Hope.Security.HashGeneration;
using Hope.Utils.EthereumUtils;
using System;
using System.Threading.Tasks;

namespace Hope.Security.Encryption
{
    /// <summary>
    /// Class which executes wallet encryption tasks asynchronously.
    /// </summary>
    public static class AsyncWalletEncryptionNew
    {

        /// <summary>
        /// Gets the encryption password asynchronously.
        /// </summary>
        /// <param name="safePassword"> The SafePassword object to extract the encryption password from. </param>
        /// <param name="userPassword"> The user's password for accessing the wallet, for extra layer of security. </param>
        /// <param name="onPasswordExtracted"> Action to call once the password has been extracted from the PlayerPrefPassword object. </param>
        /// <param name="generateNew"> Whether to generate a new encryption password or extract an old one. </param>
        public static async void GetEncryptionPasswordAsync(PlayerPrefPassword safePassword, string userPassword,
            Action<string> onPasswordExtracted, bool generateNew = false)
        {
            if (string.IsNullOrEmpty(userPassword))
            {
                onPasswordExtracted?.Invoke(null);
            }
            else
            {
                var prefPassAction = generateNew ?
                    safePassword.GenerateEncryptionPassword(userPassword) : safePassword.ExtractEncryptionPassword(userPassword);

                var password = await Task.Run(() => prefPassAction).ConfigureAwait(false);

                onPasswordExtracted?.Invoke(password.GetSHA512Hash());
            }
        }


        public static async void EncryptWalletAsync(string mnemonicPhrase, string encryptionPassword, Action<string, string, string, string, string> onWalletEncrypted)
        {
            SplitString splitPass = encryptionPassword.SplitHalf();
            SplitString lvl12string = splitPass.firstHalf.SplitHalf();
            SplitString lvl34string = splitPass.secondHalf.SplitHalf();

            string hash1 = await Task.Run(() => lvl12string.firstHalf.GetSHA384Hash()).ConfigureAwait(false);
            string hash2 = await Task.Run(() => lvl12string.secondHalf.GetSHA384Hash()).ConfigureAwait(false);
            string hash3 = await Task.Run(() => lvl34string.firstHalf.GetSHA384Hash()).ConfigureAwait(false);
            string hash4 = await Task.Run(() => lvl34string.secondHalf.GetSHA384Hash()).ConfigureAwait(false);
            string combinedHashes = hash1 + hash2 + hash3 + hash4;
            string encryptedPhrase = await Task.Run(() => mnemonicPhrase.AESEncrypt(combinedHashes).Protect(combinedHashes)).ConfigureAwait(false);

            onWalletEncrypted?.Invoke(hash1, hash2, hash3, hash4, encryptedPhrase);
        }

        /// <summary>
        /// Decrypts the data from a WalletData object asynchronously.
        /// </summary>
        /// <param name="walletData"> The wallet to decrypt. </param>
        /// <param name="encryptionPassword"> The password used to encrypt the wallet. </param>
        /// <param name="onWalletDecrypted"> Action called once the wallet has been decrypted with the private key and seed as parameters. </param>
        public static async void DecryptWalletAsync(string encryptionPassword, Action<string, string> onWalletDecrypted)
        {

        }

    }

}