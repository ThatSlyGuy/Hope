﻿using UnityEngine;

/// <summary>
/// Class which animates the ChooseWalletMenu.
/// </summary>
public sealed class ChooseWalletMenuAnimator : UIAnimator
{
    [SerializeField] private GameObject ledgerButton;
    [SerializeField] private GameObject hopeButton;

	/// <summary>
	/// Animates the unique elements of this form into view
	/// </summary>
	protected override void AnimateUniqueElementsIn()
	{
		ledgerButton.AnimateGraphicAndScale(1f, 1f, 0.2f);
		hopeButton.AnimateGraphicAndScale(1f, 1f, 0.2f, FinishedAnimating);
	}
}
