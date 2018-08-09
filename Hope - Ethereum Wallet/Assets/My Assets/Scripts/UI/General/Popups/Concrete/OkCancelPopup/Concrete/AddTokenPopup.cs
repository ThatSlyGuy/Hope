﻿using UnityEngine.UI;
using Hope.Utils.EthereumUtils;
using TMPro;
using Zenject;
using UnityEngine;
using System;
using static ERC20.Queries;

/// <summary>
/// Class which is manages the popup for adding a token to the list of tokens.
/// </summary>
public sealed class AddTokenPopup : OkCancelPopupComponent<AddTokenPopup>
{
    public event Action<Status> OnStatusChanged;

    [SerializeField] private TMP_InputField addressField, symbolField, decimalsField;
    [SerializeField] private Image tokenIcon;
    [SerializeField] private TextMeshProUGUI tokenSymbol;

    private TokenListManager tokenListManager;
    private TradableAssetImageManager tradableAssetImageManager;

    new private string name;
    private string symbol;
    private int? decimals;

    private bool updatedName, updatedSymbol, updatedDecimals, updatedLogo;

    private int previousAddressLength;

    /// <summary> 
    /// Injects dependencies into this popup.
    /// </summary>
    [Inject]
    public void Construct(
        TokenListManager tokenListManager,
        TradableAssetImageManager tradableAssetImageManager)
    {
        this.tokenListManager = tokenListManager;
        this.tradableAssetImageManager = tradableAssetImageManager;
    }

    /// <summary>
    /// Gets the input field in the children and makes sure the ok button is disabled.
    /// </summary>
    protected override void OnStart()
    {
        addressField.onValueChanged.AddListener(OnAddressChanged);
        symbolField.onValueChanged.AddListener(OnSymbolChanged);
        decimalsField.onValueChanged.AddListener(OnDecimalsChanged);
    }

    /// <summary>
    /// Start the token add process via the ContractManager.
    /// </summary>
    protected override void OnOkClicked()
    {
        //tokenContractManager.AddToken(addressField.text);

    }

    private void OnSymbolChanged(string value)
    {

    }

    private void OnDecimalsChanged(string value)
    {

    }

    /// <summary>
    /// Method called every time the text in the input field changed.
    /// Sets the button to interactable if the text is a valid ethereum address.
    /// </summary>
    /// <param name="address"> The inputted text in the address input field. </param>
    private void OnAddressChanged(string address)
    {
        if (address.Length == 43 || previousAddressLength == 43)
        {
            previousAddressLength = address.Length;
            addressField.text = address.LimitEnd(42);
            return;
        }

        bool validAddress = AddressUtils.IsValidEthereumAddress(addressField.text);
        CheckForInvalidAddress(validAddress);
        CheckForValidAddress(validAddress);
    }

    private void CheckForInvalidAddress(bool validAddress)
    {
        if (validAddress)
            return;

        OnStatusChanged?.Invoke(Status.NoTokenFound);
        okButton.interactable = false;
    }

    private void CheckForValidAddress(bool validAddress)
    {
        if (!validAddress)
            return;

        addressField.readOnly = true;

        bool existsInTokenList = tokenListManager.AddableTokens.Contains(addressField.text);
        CheckTokenList(existsInTokenList);
        CheckTokenContract(existsInTokenList);
    }

    private void CheckTokenList(bool existsInTokenList)
    {
        if (!existsInTokenList)
            return;

        AddableTokenJson addableToken = tokenListManager.AddableTokens[addressField.text];
        TokenInfoJson tokenInfo = addableToken.tokenInfo;
        name = tokenInfo.name;
        symbol = tokenInfo.symbol;
        decimals = tokenInfo.decimals;

        tokenSymbol.text = symbol;
        tradableAssetImageManager.LoadImage(symbol, icon => tokenIcon.sprite = icon);

        OnStatusChanged?.Invoke(Status.ValidToken);

        addressField.readOnly = false;
        okButton.interactable = true;
    }

    private void CheckTokenContract(bool existsInTokenList)
    {
        if (existsInTokenList)
            return;

        updatedName = false;
        updatedSymbol = false;
        updatedDecimals = false;
        updatedLogo = false;

        OnStatusChanged?.Invoke(Status.Loading);

        SimpleContractQueries.QueryStringOutput<Name>(addressField.text, null, output => NameQueryCompleted(output.Value));
        SimpleContractQueries.QueryStringOutput<Symbol>(addressField.text, null, output => SymbolQueryCompleted(output.Value));
        SimpleContractQueries.QueryUInt256Output<Decimals>(addressField.text, null, output => DecimalsQueryCompleted(output.Value));
    }

    private void NameQueryCompleted(string value)
    {
        name = string.IsNullOrEmpty(value) ? name : value;
        CheckLoadStatus(ref updatedName);
    }

    private void SymbolQueryCompleted(string value)
    {
        symbol = string.IsNullOrEmpty(value) ? string.Empty : value;
        name = string.IsNullOrEmpty(name) ? symbol : name;

        tradableAssetImageManager.LoadImage(symbol, LogoQueryCompleted);

        CheckLoadStatus(ref updatedSymbol);
    }

    private void DecimalsQueryCompleted(dynamic value)
    {
        decimals = value == null ? (int?)null : (int)value;
        CheckLoadStatus(ref updatedDecimals);
    }

    private void LogoQueryCompleted(Sprite value)
    {
        tokenIcon.sprite = value;
        CheckLoadStatus(ref updatedLogo);
    }

    private void CheckLoadStatus(ref bool updatingVar)
    {
        updatingVar = true;

        if (updatedName && updatedSymbol && updatedDecimals && updatedLogo)
        {
            if (string.IsNullOrEmpty(symbol) || !decimals.HasValue)
            {
                decimalsField.text = string.Empty;
                symbolField.text = string.Empty;

                OnStatusChanged?.Invoke(Status.InvalidToken);

                okButton.interactable = false;
            }
            else
            {
                tokenSymbol.text = symbol;
                tokenListManager.Add(addressField.text, name, symbol, decimals.Value);

                OnStatusChanged?.Invoke(Status.ValidToken);

                okButton.interactable = true;
            }

            addressField.readOnly = false;
        }
    }

    /// <summary>
    /// The status of the AddTokenPopup.
    /// Loading - The entered address is being searched for the name/symbol/decimals.
    /// NoTokenFound - The entered address is not a full length address and cannot be searched for.
    /// InvalidToken - The entered address was searched for but cannot be verified as a valid address, therefore the fields for Symbol and Decimals needs to be available.
    /// ValidToken - The entered address was searched for and found, therefore the image and symbol text can be displayed.
    /// </summary>
    public enum Status { Loading, NoTokenFound, InvalidToken, ValidToken };
}