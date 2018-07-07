﻿using Hope.Security.ProtectedTypes.Types;
using TMPro;
using UnityEngine.UI;
using Zenject;

/// <summary>
/// Menu which lets the user create a new wallet by first choosing a password and name for the wallet.
/// </summary>
public sealed class CreateWalletMenu : Menu<CreateWalletMenu>
{

    public Button createWalletButton;
    public Button backButton;
    public TMP_InputField walletNameField;
    public TMP_InputField passwordField;

    private ProtectedStringDataCache protectedStringDataCache;

    /// <summary>
    /// Adds the required dependencies into this class.
    /// </summary>
    /// <param name="protectedStringDataCache"> The active ProtectedStringDataCache. </param>
    [Inject]
    public void Construct(ProtectedStringDataCache protectedStringDataCache) => this.protectedStringDataCache = protectedStringDataCache;

    /// <summary>
    /// Adds the button listeners.
    /// </summary>
    private void Start()
    {
        createWalletButton.onClick.AddListener(CreateWalletNameAndPass);
        backButton.onClick.AddListener(OnBackPressed);
    }

    /// <summary>
    /// Sets up the wallet name and password and opens the next menu.
    /// </summary>
    private void CreateWalletNameAndPass()
    {
        protectedStringDataCache.SetData(new ProtectedString(passwordField.text), 0);
        protectedStringDataCache.SetData(new ProtectedString(walletNameField.name), 1);

        // Open next menu
    }

    public override void OnBackPressed()
    {
        if (!Animator.Animating)
            uiManager.CloseMenu();
    }
}