﻿using Nethereum.Hex.HexTypes;
using System.Collections.Generic;
using System.Numerics;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using Random = System.Random;

/// <summary>
/// Class used for locking purpose.
/// </summary>
public sealed partial class LockPRPSPopup : OkCancelPopupComponent<LockPRPSPopup>, IEtherBalanceObservable, IEnterButtonObservable
{
	[SerializeField]
	private Button threeMonthsButton,
				   sixMonthsButton,
				   twelveMonthsButton;

	[SerializeField] private HopeInputField amountInputField;

	[SerializeField] private Slider slider;

	[SerializeField]
	private TMP_Text transactionFeeText,
					 prpsBalanceText,
				     dubiBalanceText,
					 dubiRewardText,
					 maxText;

	[SerializeField] private Toggle maxToggle;

	[SerializeField] private TooltipItem[] tooltipItems;

	private LockedPRPSManager lockedPRPSManager;
	private EtherBalanceObserver etherBalanceObserver;
	private UserWalletManager userWalletManager;
	private Hodler hodlerContract;
	private ButtonClickObserver buttonClickObserver;

	/// <summary>
	/// Manages the gas for the LockPRPSPopup.
	/// </summary>
	public GasManager Gas { get; private set; }

	/// <summary>
	/// Manages the amount of purpose to lock for the LockPRPSPopup.
	/// </summary>
	public AmountManager Amount { get; private set; }

	/// <summary>
	/// Manages the amount of time for locking purpose.
	/// </summary>
	public TimeManager Time { get; private set; }

	/// <summary>
	/// The current ether balance of the wallet.
	/// </summary>
	public dynamic EtherBalance { get; set; }

    /// <summary>
    /// Adds all dependencies to the LockPRPSPopup.
    /// </summary>
    /// <param name="tradableAssetPriceManager"> The active TradableAssetPriceManager. </param>
    /// <param name="currencyManager"> The active CurrencyManager. </param>
    /// <param name="lockPRPSManager"> The active LockPRPSManager. </param>
    /// <param name="lockedPRPSManager"> The active LockedPRPSManager. </param>
    /// <param name="gasPriceObserver"> The active GasPriceObserver. </param>
    /// <param name="etherBalanceObserver"> The active EtherBalanceObserver. </param>
    /// <param name="hodlerContract"> The active Hodler smart contract. </param>
    /// <param name="userWalletManager"> The active UserWalletManager. </param>
    [Inject]
	public void Construct(
        TradableAssetPriceManager tradableAssetPriceManager,
        CurrencyManager currencyManager,
		LockPRPSManager lockPRPSManager,
		LockedPRPSManager lockedPRPSManager,
		GasPriceObserver gasPriceObserver,
		EtherBalanceObserver etherBalanceObserver,
		Hodler hodlerContract,
		UserWalletManager userWalletManager,
		ButtonClickObserver buttonClickObserver)
	{
		this.lockedPRPSManager = lockedPRPSManager;
		this.etherBalanceObserver = etherBalanceObserver;
		this.userWalletManager = userWalletManager;
		this.hodlerContract = hodlerContract;
		this.buttonClickObserver = buttonClickObserver;
		etherBalanceObserver.SubscribeObservable(this);
		buttonClickObserver.SubscribeObservable(this);

		Gas = new GasManager(tradableAssetPriceManager, currencyManager, lockPRPSManager, gasPriceObserver, slider, transactionFeeText, this);
		Amount = new AmountManager(lockPRPSManager, maxToggle, amountInputField, prpsBalanceText, dubiBalanceText, dubiRewardText, tooltipItems[2]);
		Time = new TimeManager(Amount, threeMonthsButton, sixMonthsButton, twelveMonthsButton, dubiRewardText);

		if (lockPRPSManager.PRPSBalance == 0)
		{
			maxToggle.SetInteractable(false);
			maxText.color = UIColors.LightGrey;
		}

		bool showTooltips = SecurePlayerPrefs.GetBool(PlayerPrefConstants.SETTING_SHOW_TOOLTIPS);

		foreach (TooltipItem tooltip in tooltipItems)
		{
			if (showTooltips)
				tooltip.PopupManager = popupManager;
			else if (tooltip.infoIcon)
				tooltip.gameObject.SetActive(false);
			else
				tooltip.enabled = false;
		}
	}

	/// <summary>
	/// Closes all the managers for the LockPRPSPopup and the ether balance observer.
	/// </summary>
	protected override void OnDestroy()
	{
        base.OnDestroy();

		Gas.Stop();
		Amount.Stop();
		Time.Stop();

		etherBalanceObserver.UnsubscribeObservable(this);
		buttonClickObserver.UnsubscribeObservable(this);
	}

	/// <summary>
	/// Updates the lock button interactability based on the managers.
	/// </summary>
	private void Update() => okButton.interactable = EtherBalance >= Gas.TransactionFee && Gas.IsValid && !amountInputField.Error && Time.IsValid;

	/// <summary>
	/// Locks purpose based on all values entered.
	/// </summary>
	public override void OkButton()
	{
		hodlerContract.Hodl(userWalletManager,
							new HexBigInteger(Gas.TransactionGasLimit),
							Gas.TransactionGasPrice.FunctionalGasPrice,
							GenerateUnusedBigInteger(lockedPRPSManager.UsedIds),
							Amount.AmountToLock,
							Time.MonthsToLock);
	}

	/// <summary>
	/// Generates a BigInteger.
	/// If a collection is passed in, makes sure the new random is not already contained in the collection.
	/// </summary>
	/// <param name="numbersToIgnore"> The collection of numbers to ensure the new random number is not a part of. </param>
	/// <returns> The newly created BigInteger. </returns>
	public static BigInteger GenerateUnusedBigInteger(ICollection<BigInteger> numbersToIgnore)
	{
		var rand = new Random();

		if (numbersToIgnore == null)
			return new BigInteger(rand.Next());

		var val = rand.Next();
		while (numbersToIgnore.Contains(val))
			val = rand.Next();

		numbersToIgnore.Add(val);

		return val;
	}

	/// <summary>
	/// The enter button is pressed
	/// </summary>
	/// <param name="clickType"> The ClickType </param>
	public void EnterButtonPressed(ClickType clickType)
	{
		if (clickType == ClickType.Down && okButton.interactable && popupManager.ActivePopupType != typeof(ConfirmLockPopup))
			okButton.Press();
	}
}