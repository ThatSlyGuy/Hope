﻿using Hope.Utils.EthereumUtils;
using Nethereum.Hex.HexTypes;
using System;
using UnityEngine;

/// <summary>
/// Class which contains functions and info for an ERC20 token.
/// </summary>
public class TokenContract : ContractBase
{

    private const string FUNC_TRANSFER = "transfer";
    private const string FUNC_BALANCEOF = "balanceOf";
    private const string FUNC_TOTALSUPPLY = "totalSupply";
    private const string FUNC_NAME = "name";
    private const string FUNC_SYMBOL = "symbol";
    private const string FUNC_DECIMALS = "decimals";

    /// <summary>
    /// The name of the token.
    /// </summary>
    public string TokenName { get; private set; }

    /// <summary>
    /// The symbol of the token.
    /// </summary>
    public string TokenSymbol { get; private set; }

    /// <summary>
    /// The amount decimal places this token has.
    /// </summary>
    public int TokenDecimals { get; private set; }

    /// <summary>
    /// The function names of this token contract.
    /// </summary>
    protected override string[] FunctionNames => new string[] { FUNC_TRANSFER, FUNC_BALANCEOF, FUNC_TOTALSUPPLY, FUNC_NAME, FUNC_SYMBOL, FUNC_DECIMALS };

    /// <summary>
    /// Initializes the Contract in ContractBase with the address and abi.
    /// </summary>
    /// <param name="contractAddress"> The address of the contract. </param>
    /// <param name="abi"> The abi of the contract. </param>
    /// <param name="onContractInitialized"> Action to call when the contract has been fully initialized. </param>
    public TokenContract(string contractAddress, string abi, Action<ContractBase, string> onContractInitialized = null) : base(contractAddress, abi, onContractInitialized) { }

    /// <summary>
    /// Gets the token balance of an address.
    /// </summary>
    /// <param name="address"> The address to check the balance of. </param>
    /// <param name="onBalanceReceived"> Callback action which should pass in the received balance of Gold tokens on the address. </param>
    public void BalanceOf(string address, Action<dynamic> onBalanceReceived)
        => this.ContractViewCall<dynamic>(this[FUNC_BALANCEOF],
            balance => onBalanceReceived?.Invoke(balance == null ? 0 : SolidityUtils.ConvertFromUInt(balance, TokenDecimals)), address);

    /// <summary>
    /// Gets the total supply of this ERC20 token contract.
    /// </summary>
    /// <param name="onSupplyReceived"> Callback action which should pass in the total supply of this token. </param>
    public void TotalSupply(Action<dynamic> onSupplyReceived)
        => this.ContractViewCall<dynamic>(this[FUNC_TOTALSUPPLY], supply => onSupplyReceived?.Invoke(SolidityUtils.ConvertFromUInt(supply, TokenDecimals)));

    /// <summary>
    /// Transfers a certain number of tokens of this contract from a wallet to another address.
    /// </summary>
    /// <param name="userWallet"> The wallet to transfer the tokens from. </param>
    /// <param name="gasLimit"> The gas limit to use when sending the tokens. </param>
    /// <param name="gasPrice"> The gas price to use when sending the tokens. </param>
    /// <param name="address"> The address to transfer the tokens to. </param>
    /// <param name="amount"> The amount of tokens to transfer. </param>
    public void Transfer(UserWallet userWallet, HexBigInteger gasLimit, HexBigInteger gasPrice, string address, decimal amount)
    {
        userWallet.SignTransaction<ConfirmSendAssetPopup>(request =>
        {
            this.ExecuteContractFunction(this[FUNC_TRANSFER],
                                         request,
                                         userWallet.Address,
                                         gasLimit,
                                         gasPrice,
                                         () => Debug.Log("Successfully sent " + amount + " " + TokenSymbol + " to address " + address),
                                         address,
                                         SolidityUtils.ConvertToUInt(amount, TokenDecimals));
        }, gasLimit, gasPrice, address, ContractAddress, amount);
    }

    /// <summary>
    /// Initializes the name, symbol, and decimals of the token.
    /// </summary>
    /// <param name="onContractInitialized"> Action to call when the contract has been fully initialized. </param>
    protected override void InitializeExtra(Action<ContractBase, string> onContractInitialized)
    {
        this.ContractViewCall<string>(this[FUNC_NAME], tokenName => this.ContractViewCall<string>(this[FUNC_SYMBOL], tokenSymbol =>
        this.ContractViewCall<dynamic>(this[FUNC_DECIMALS], tokenDecimals =>
        {
            TokenName = string.IsNullOrEmpty(tokenName) ? tokenSymbol : tokenName;
            TokenSymbol = tokenSymbol;
            TokenDecimals = tokenDecimals == null ? 0 : (int)tokenDecimals;

            onContractInitialized?.Invoke(this, ContractABI);
        })));
    }
}