﻿using TMPro;
using UnityEngine;

/// <summary>
/// The info popup
/// </summary>
public sealed class InfoPopup : FactoryPopup<InfoPopup>
{
	[SerializeField] private TextMeshProUGUI title;
	[SerializeField] private TextMeshProUGUI body;
	[SerializeField] private GameObject infoIcon;
	[SerializeField] private GameObject errorIcon;

	public PopupManager PopupManager => popupManager;

	/// <summary>
	/// Sets the UI elements of the info popup
	/// </summary>
	/// <param name="titleText"> The title text string being set </param>
	/// <param name="bodyText"> The body text string being set </param>
	/// <param name="isInfoIcon"> Checks if the user is hovering over an info icon or error icon </param>
	/// <param name="iconPosition"> The icon so that the popup can animate next to it </param>
	public void SetUIElements(string titleText, string bodyText, InteractableIcon.IconType iconType, Vector2 iconPosition)
	{
		title.text = titleText;
		body.text = bodyText;
		infoIcon.SetActive(iconType == InteractableIcon.IconType.Info);
		errorIcon.SetActive(iconType == InteractableIcon.IconType.Error);
		transform.position = new Vector2(iconPosition.x + 10f, iconPosition.y);
	}
}
