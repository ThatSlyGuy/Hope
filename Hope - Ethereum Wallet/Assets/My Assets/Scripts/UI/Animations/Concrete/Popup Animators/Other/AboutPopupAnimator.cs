﻿using UnityEngine;

public class AboutPopupAnimator : UIAnimator
{
	[SerializeField] private GameObject hopeVersionSection;

	/// <summary>
	/// Animates the unique elements of this form into view
	/// </summary>
	protected override void AnimateUniqueElementsIn() => hopeVersionSection.AnimateScale(1f, 0.15f, FinishedAnimating);
}
