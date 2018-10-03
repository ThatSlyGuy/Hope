﻿using Ledger.Net.Connectivity;
using Nethereum.HdWallet;
using System;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public sealed class OpenLedgerWalletMenu : Menu<OpenLedgerWalletMenu>, IPeriodicUpdater
{
    public event Action OnLedgerLoadStart;
    public event Action OnLedgerLoadEnd;

    public event Action OnLedgerConnected;
    public event Action OnLedgerDisconnected;

    [SerializeField] private Button openLedgerWalletButton;

    private LedgerWallet ledgerWallet;
    private PeriodicUpdateManager periodicUpdateManager;

    private bool connected;

    public float UpdateInterval => 2f;

    [Inject]
    public void Construct(LedgerWallet ledgerWallet, PeriodicUpdateManager periodicUpdateManager)
    {
        this.ledgerWallet = ledgerWallet;
        this.periodicUpdateManager = periodicUpdateManager;
    }

    private void Start()
    {
        openLedgerWalletButton.onClick.AddListener(OpenWallet);
    }

    private void OnEnable()
    {
        UserWalletManager.OnWalletLoadSuccessful += OpenMainWalletMenu;
        UserWalletManager.OnWalletLoadUnsuccessful += OnWalletLoadUnsuccessful;
        periodicUpdateManager.AddPeriodicUpdater(this, true);
    }

    private void OnDisable()
    {
        UserWalletManager.OnWalletLoadSuccessful -= OpenMainWalletMenu;
        UserWalletManager.OnWalletLoadUnsuccessful -= OnWalletLoadUnsuccessful;
        periodicUpdateManager.RemovePeriodicUpdater(this);
    }

    private void OpenMainWalletMenu()
    {
        uiManager.OpenMenu<OpenWalletMenu>();
        uiManager.DestroyUnusedMenus();
    }

    private void OpenWallet()
    {
        OnLedgerLoadStart?.Invoke();
        ledgerWallet.InitializeAddresses();
    }

    private void OnWalletLoadUnsuccessful()
    {
        OnLedgerLoadEnd?.Invoke();
    }

    public async void PeriodicUpdate()
    {
        var ledgerManager = LedgerConnector.GetWindowsConnectedLedger();
        var address = ledgerManager == null
            ? null
            : (await ledgerManager.GetPublicKeyResponse(Wallet.ELECTRUM_LEDGER_PATH.Replace("x", "0"), false, false).ConfigureAwait(false))?.Address;

        if (string.IsNullOrEmpty(address))
        {
            if (connected)
                MainThreadExecutor.QueueAction(() => OnLedgerDisconnected?.Invoke());

            connected = false;
        }
        else
        {
            if (!connected)
                MainThreadExecutor.QueueAction(() => OnLedgerConnected?.Invoke());

            connected = true;
        }
    }
}