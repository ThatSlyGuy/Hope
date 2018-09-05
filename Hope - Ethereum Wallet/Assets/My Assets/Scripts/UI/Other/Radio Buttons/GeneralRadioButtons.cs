﻿
public sealed class GeneralRadioButtons : SingleChoiceButtons
{
	/// <summary>
	/// Changes the visuals of the newly active, and previously active radio button
	/// </summary>
	/// <param name="activeButton"> the index of the button being changed </param>
	/// <param name="active"> Whether the button is currently active or not </param>
	protected override void SetRadioButtonVisuals(int activeButton, bool active)
	{
		base.SetRadioButtonVisuals(activeButton, active);

		transform.GetChild(activeButton).GetChild(0).gameObject.AnimateColor(active ? UIColors.White : UIColors.LightGrey, 0.15f);
	}
}
