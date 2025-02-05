﻿using Nethereum.ABI.FunctionEncoding.Attributes;

/// <summary>
/// Class which mimics the ethereum ERC20 token standard and contains most functions which are active in the token standard.
/// </summary>
public sealed partial class ERC20
{
    /// <summary>
    /// Class which contains the different queries for receiving data from the ERC20 token contract.
    /// </summary>
    public static partial class Queries
    {
        /// <summary>
        /// Class which contains the data needed to read the decimal count of the ERC20 token contract.
        /// </summary>
        [Function("decimals", "uint256")]
        public sealed class Decimals : ContractFunction
        {
        }
    }
}