﻿using Hope.Utils.EthereumUtils;
using Nethereum.Hex.HexTypes;
using System;
using System.Numerics;

/// <summary>
/// Class which represents an ethereum token that can be traded from one address to another.
/// </summary>
public class TokenAsset : TradableAsset
{

    /// <summary>
    /// The TokenContract of this asset.
    /// </summary>
    public TokenContract TokenContract { get; }

    /// <summary>
    /// Initializes this TradableAsset with the TokenContract of the token.
    /// </summary>
    /// <param name="tokenContract"> The TokenContract of this asset. </param>
    /// <param name="onAssetCreated"> Callback to execute once the asset has been initialized and the current balance received. </param>
    /// <param name="tradableAssetImageManager"> The active TradableAssetImageManager. </param>
    /// <param name="userWalletManager"> The active UserWalletManager. </param>
    public TokenAsset(
        TokenContract tokenContract, 
        Action<TradableAsset> onAssetCreated,
        TradableAssetImageManager tradableAssetImageManager,
        UserWalletManager userWalletManager) :
        base(tokenContract.ContractAddress, tokenContract.TokenSymbol, tokenContract.TokenName, tokenContract.TokenDecimals, tradableAssetImageManager, userWalletManager)
    {
        TokenContract = tokenContract;
        InitializeBasicInfo(onAssetCreated);
    }

    /// <summary>
    /// Gets the balance of this token of a specified UserWallet.
    /// </summary>
    /// <param name="userWalletManager"> The wallet to get the balance for. </param>
    /// <param name="onBalanceReceived"> Callback to execute once the balance has been received, with the balance as a parameter. </param>
    public override void GetBalance(UserWalletManager userWalletManager, Action<dynamic> onBalanceReceived) => TokenContract.BalanceOf(userWalletManager.WalletAddress, onBalanceReceived);

    /// <summary>
    /// Transfers a specified amount of this token from a wallet to another ethereum address.
    /// </summary>
    /// <param name="userWalletManager"> The wallet to transfer tokens from. </param>
    /// <param name="gasLimit"> The gas limit to use for sending the token asset. </param>
    /// <param name="gasPrice"> The gas price to use for sending the token asset. </param>
    /// <param name="address"> The address to transfer the tokens to. </param>
    /// <param name="amount"> The amount of tokens to transfer. </param>
    public override void Transfer(UserWalletManager userWalletManager, HexBigInteger gasLimit, HexBigInteger gasPrice, string address, decimal amount) 
        => TokenContract.Transfer(userWalletManager, gasLimit, gasPrice, address, amount);

    /// <summary>
    /// Gets the gas limit for the transfer of this token from the user's address to another address.
    /// </summary>
    /// <param name="receivingAddress"> The address to be receiving the tokens. </param>
    /// <param name="amount"> The amount of tokens to send and test the gas limit for. </param>
    /// <param name="onLimitReceived"> Action to execute when the gas limit has been received. </param>
    public override void GetTransferGasLimit(string receivingAddress, dynamic amount, Action<BigInteger> onLimitReceived) 
        => GasUtils.EstimateGasLimit(TokenContract["transfer"], userWalletManager.WalletAddress, onLimitReceived, receivingAddress, SolidityUtils.ConvertToUInt(amount, TokenContract.TokenDecimals));
}
