﻿using UnityEngine;

public class OpenWalletMenuAnimator : UIAnimator
{

	[SerializeField] private GameObject background;
	[SerializeField] private GameObject assetList;
	[SerializeField] private GameObject taskBarButtons;
	[SerializeField] private GameObject transactionList;
	[SerializeField] private GameObject assetIcon;
	[SerializeField] private GameObject assetName;
	[SerializeField] private GameObject assetAmount;
	[SerializeField] private GameObject buyButton;
	[SerializeField] private GameObject sellButton;

	/// <summary>
	/// Animates the UI elements of the form into view
	/// </summary>
	protected override void AnimateIn()
	{
		background.AnimateGraphic(1f, 0.15f,
			() => assetIcon.AnimateGraphicAndScale(1f, 1f, 0.15f,
			() => assetName.AnimateGraphicAndScale(0.85f, 1f, 0.15f,
			() => assetAmount.AnimateGraphicAndScale(0.65f, 1f, 0.15f,
			() => buyButton.AnimateGraphicAndScale(1f, 1f, 0.15f,
			() => sellButton.AnimateGraphicAndScale(1f, 1f, 0.15f, FinishedAnimating))))));

		AnimateList(assetList.transform.GetChild(0).GetChild(0), 0);
		AnimateList(transactionList.transform.GetChild(0).GetChild(0), 0);
		AnimateList(taskBarButtons.transform, 0);
	}

	/// <summary>
	/// Animates the UI elements of the form out of view
	/// </summary>
	protected override void AnimateOut()
	{
		FinishedAnimating();
	}

	/// <summary>
	/// Animates a given list of objects one by one
	/// </summary>
	/// <param name="objectTransform"> The parent object of the entire list of objects </param>
	/// <param name="index"> The index of object in the list </param>
	private void AnimateList(Transform objectTransform, int index)
	{
		if (index == objectTransform.childCount - 1)
			objectTransform.GetChild(index).gameObject.AnimateScaleX(1f, 0.15f);
		else
			objectTransform.GetChild(index).gameObject.AnimateScaleX(1f, 0.15f, () => AnimateList(objectTransform, ++index));
	}
}