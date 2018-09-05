﻿using NBitcoin;
using Nethereum.HdWallet;
using Nethereum.Web3.Accounts;
using System.Linq;
using UnityEngine;
using System.Security.Cryptography;
using System.Text;
using Random = System.Random;
using SecureRandom = Org.BouncyCastle.Security.SecureRandom;
using Zenject;
using Hope.Security.Encryption;
using Hope.Security;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Management;
using System.Net.NetworkInformation;
using System;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Digests;
using System.IO;
using Hope.Security.Encryption.DPAPI;
using Hope.Security.HashGeneration;
using Hope.Security.ProtectedTypes.Types;
using Hope.Security.ProtectedTypes.Types.Base;
using System.ComponentModel;
using System.Numerics;
using Hope.Utils.Ethereum;
using System.Security;
using Nethereum.Hex.HexConvertors.Extensions;
using System.Runtime.InteropServices;
using System.Dynamic;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;
using System.Runtime.CompilerServices;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Reflection;
using System.Security.Permissions;
using Hope.Security.Encryption.Symmetric;
using Nethereum.Contracts;
using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts.Extensions;
using Nethereum.ABI.Model;
using Nethereum.RPC.Eth.DTOs;
using System.Collections;
using Nethereum.JsonRpc.UnityClient;
using System.Collections.Generic;
using UnityEngine.Assertions;
using Org.BouncyCastle.Crypto.Prng;
using Nethereum.Hex.HexTypes;
using NUnit.Framework;
using Assert = NUnit.Framework.Assert;
using Nethereum.Util;
using Nethereum.RPC.Eth.Transactions;
using Nethereum.Contracts.ContractHandlers;
using Nethereum.Signer;
using Hope.Random;
using Transaction = Nethereum.Signer.Transaction;
using Nethereum.RLP;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Converters;
using Hope.Random.Strings;
using Hope.Random.Bytes;
using Org.BouncyCastle.Asn1.Cms;
using Nethereum.RPC.NonceServices;
using Ledger.Net.Connectivity;
using Ledger.Net;
using Ledger.Net.Requests;
using Ledger.Net.Responses;
using Hid.Net;

public sealed class HopeTesting : MonoBehaviour
{
    public bool connect;

    private static readonly VendorProductIds[] wellKnownLedgerWallets = new VendorProductIds[] { new VendorProductIds(0x2c97), new VendorProductIds(0x2581, 0x3b7c) };
    private static readonly UsageSpecification[] usageSpecification = new[] { new UsageSpecification(0xffa0, 0x01) };

    public static async Task<LedgerManager> GetWindowsConnectedLedger()
    {
        var devices = new List<DeviceInformation>();

        var collection = WindowsHidDevice.GetConnectedDeviceInformations();

        foreach (var ids in wellKnownLedgerWallets)
        {
            if (ids.ProductId == null)
                devices.AddRange(collection.Where(c => c.VendorId == ids.VendorId));
            else
                devices.AddRange(collection.Where(c => c.VendorId == ids.VendorId && c.ProductId == ids.ProductId));
        }

        var retVal = devices.Find(d => usageSpecification == null || usageSpecification.Length == 0 || usageSpecification.Any(u => d.UsagePage == u.UsagePage && d.Usage == u.Usage));
        if (retVal == null)
            return null;

        var ledgerHidDevice = new WindowsHidDevice(retVal);
        await ledgerHidDevice.InitializeAsync();

        var ledgerManager = new LedgerManager(ledgerHidDevice);
        var address = await ledgerManager.GetAddressAsync(0, 0);

        return string.IsNullOrEmpty(address) ? null : ledgerManager;
    }

    private void Update()
    {
        if (!connect)
            return;

        //AsyncTaskScheduler.Schedule(() => SignTransaction(CreateTransaction()));
        //GetWindowsConnectedLedger();
        AsyncTaskScheduler.Schedule(GetWindowsConnectedLedger);
    }

    private Transaction CreateTransaction()
    {
        return new Transaction(
            0.ToBytesForRLPEncoding(), // Nonce
            1000000000.ToBytesForRLPEncoding(), // Gas price
            21000.ToBytesForRLPEncoding(), // Gas limit
            "0x8b069Ecf7BF230E153b8Ed903bAbf24403ccA203".HexToByteArray(), // Receiving address
            0.ToBytesForRLPEncoding(), // Ether value
            "".HexToByteArray(), // Data
            0.ToBytesForRLPEncoding(), // R
            0.ToBytesForRLPEncoding(), // S
            4);
    }

    private async Task SignTransaction(Transaction transaction)
    {
        var ledgerManager = await LedgerConnector.GetWindowsConnectedLedger();

        if (ledgerManager == null)
            return;

        ledgerManager.SetCoinNumber(60);

        var derivationData = Ledger.Net.Helpers.GetDerivationPathData(ledgerManager.CurrentCoin.App, ledgerManager.CurrentCoin.CoinNumber, 0, 0, false, ledgerManager.CurrentCoin.IsSegwit);

        var firstRequest = new EthereumAppSignTransactionRequest(derivationData.Concat(transaction.GetRLPEncoded()).ToArray());

        var response = await ledgerManager.SendRequestAsync<EthereumAppSignTransactionResponse, EthereumAppSignTransactionRequest>(firstRequest);

        if (!response.IsSuccess)
        {
            Debug.Log("SUCCESSFUL");
        }
        else
        {
            response.SignatureV.Log();
            response.SignatureR.LogArray();
            response.SignatureS.LogArray();
        }
    }

    //private void Start()
    //{
    //    uint v = 44;
    //    byte[] r = null;
    //    byte[] s = null;

    //    byte[] signature = EthECDSASignatureFactory.FromComponents(r, s, (byte)v).ToDER();

    //    //EthSendRawTransaction ethSendRawTransaction = new EthSendRawTransaction();
    //    //ethSendRawTransaction.SendRequestAsync()
    //    //EthSendRawTransactionUnityRequest ethSendRawTransactionUnityRequest = new EthSendRawTransactionUnityRequest("");
    //    //ethSendRawTransactionUnityRequest.SendRequest()
    //}

    //private void Start()
    //{
    //    Transaction transaction = new Transaction(
    //        0.ToBytesForRLPEncoding(), // Nonce
    //        1000000000.ToBytesForRLPEncoding(), // Gas price
    //        21000.ToBytesForRLPEncoding(), // Gas limit
    //        "0x8b069Ecf7BF230E153b8Ed903bAbf24403ccA203".HexToByteArray(), // Receiving address
    //        0.ToBytesForRLPEncoding(), // Ether value
    //        "".HexToByteArray(), // Data
    //        0.ToBytesForRLPEncoding(), // R
    //        0.ToBytesForRLPEncoding(), // S
    //        4); // V (chainId)

    //    transaction.GetRLPEncoded().LogArray();
    //}

    //public string code;

    //private string previousCode;
    //private TwoFactorAuthenticator _2fa;
    //private SetupCode setupCode;

    //private void Start()
    //{
    //    string key = RandomString.Secure.SHA3.GetString("testPassword", 256).Keccak_128();
    //    key.Log();

    //    Debug.Log("==================================");

    //    _2fa = new TwoFactorAuthenticator();

    //    setupCode = _2fa.GenerateSetupCode("Hope Wallet", key, 256, 256);
    //    setupCode.Account.Log();
    //    setupCode.AccountSecretKey.Log();
    //    setupCode.ManualEntryKey.Log();
    //    setupCode.QrCodeSetupImageUrl.Log();
    //}

    //private void Update()
    //{
    //    if (code == previousCode)
    //        return;

    //    previousCode = code;

    //    TwoFactorAuthenticator authenticator = new TwoFactorAuthenticator();
    //    authenticator.ValidateTwoFactorPIN(RandomString.Secure.SHA3.GetString("testPassword", 256).Keccak_128(), code).Log();
    //}

    [ContextMenu("Delete Player Prefs")]
    public void DeletePrefs()
    {
        PlayerPrefs.DeleteAll();
    }

    //private void AnonymousStuff()
    //{
    //    var thing = new { Name = "Something", Age = 50 };
    //    var things = new[] { new { Name = "Something1", Age = 25 }, new { Name = "Something2", Age = 35 } };

    //    dynamic obj = new ExpandoObject();
    //    obj.Stuff = new ExpandoObject[20];
    //    obj.Stuff[0].Something = "wow";
    //    obj.Name = "MyName";
    //    obj.Age = 22;

    //    Debug.Log(obj.Name);

    //}

}