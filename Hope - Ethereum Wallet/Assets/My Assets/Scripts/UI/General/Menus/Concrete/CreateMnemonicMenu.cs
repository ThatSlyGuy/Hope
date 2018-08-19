﻿using Hope.Security.ProtectedTypes.Types;
using Hope.Utils.Ethereum;
using NBitcoin;
using Nethereum.HdWallet;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

/// <summary>
/// Class used for creating a new ethereum wallet.
/// </summary>
public sealed class CreateMnemonicMenu : Menu<CreateMnemonicMenu>, IEnterButtonObservable, ITabButtonObservable
{
    [SerializeField] private GameObject[] objects;

    [SerializeField] private Button copyMnemonic,
									generateNewWords,
								    confirmButton;

    private DynamicDataCache dynamicDataCache;
	private ButtonClickObserver buttonClickObserver;

    private readonly List<TMP_InputField> wordFields = new List<TMP_InputField>();

	private List<Selectable> wordFieldSelectables = new List<Selectable>();

	/// <summary>
	/// Adds the DynamicDataCache dependency.
	/// </summary>
	/// <param name="dynamicDataCache"> The active DynamicDataCache. </param>
	/// <param name="buttonClickObserver"> The active ButtonClickObserver. </param>
	[Inject]
    public void Construct(DynamicDataCache dynamicDataCache, ButtonClickObserver buttonClickObserver)
	{
		this.dynamicDataCache = dynamicDataCache;
		this.buttonClickObserver = buttonClickObserver;
	}

    /// <summary>
    /// Initializes this class by creating a new mnemonic phrase and setting the text to it.
    /// </summary>
    protected override void OnAwake()
    {
		confirmButton.onClick.AddListener(ConfirmWords);
        copyMnemonic.onClick.AddListener(CopyMnemonic);
        generateNewWords.onClick.AddListener(GenerateMnemonic);
	}

	/// <summary>
	/// Opens the exit confirmation popup and enables the note text
	/// </summary>
	protected override void OpenExitConfirmationPopup() => popupManager.GetPopup<ExitConfirmationPopup>(true)?.SetDetails(true);

	/// <summary>
	/// Generates the mnemonic words and sets the initial text fields to the text.
	/// </summary>
	[SecureCallEnd]
    private void OnEnable()
    {
		buttonClickObserver.SubscribeObservable(this);
		GenerateMnemonic();
		UpdateWordFields();
	}

	/// <summary>
	/// Unsubscribes the the current buttonClickObserver
	/// </summary>
	private void OnDisable() => buttonClickObserver.UnsubscribeObservable(this);

	/// <summary>
	/// Opens the confirm words menu.
	/// </summary>
	public void ConfirmWords() => uiManager.OpenMenu<ConfirmMnemonicMenu>();

	/// <summary>
	/// Generates the mnemonic phrase.
	/// </summary>
	public void GenerateMnemonic() => dynamicDataCache.SetData("mnemonic", new ProtectedString(new Wallet(Wordlist.English, WordCount.Twelve).Phrase));

	/// <summary>
	/// Copies the mnemonic phrase to the clipboard.
	/// </summary>
	[SecureCallEnd]
    public void CopyMnemonic()
    {
        using (var mnemonic = (dynamicDataCache.GetData("mnemonic") as ProtectedString)?.CreateDisposableData())
            ClipboardUtils.CopyToClipboard(mnemonic.Value);
    }

    /// <summary>
    /// Updates the word text objects with the initial mnemonic phrase.
    /// </summary>
    [SecureCaller]
    private void UpdateWordFields()
    {
        for (int i = 0; i < objects.Length; i++)
            wordFields.Add(objects[i].GetComponent<TMP_InputField>());

        using (var mnemonic = (dynamicDataCache.GetData("mnemonic") as ProtectedString)?.CreateDisposableData())
        {
            string[] splitWords = WalletUtils.GetMnemonicWords(mnemonic.Value);

            for (int i = 0; i < objects.Length; i++)
                wordFields[i].text = splitWords[i];
        }

		for (int i = 0; i < 12; i++)
			wordFieldSelectables.Add(wordFields[i]);
	}

	public void EnterButtonPressed(ClickType clickType)
	{
		if (clickType != ClickType.Down)
			return;

		SelectableExtensions.MoveToNextSelectable(wordFieldSelectables);
	}

	public void TabButtonPressed(ClickType clickType)
	{
		if (clickType != ClickType.Down)
			return;

		SelectableExtensions.MoveToNextSelectable(wordFieldSelectables);
	}
}