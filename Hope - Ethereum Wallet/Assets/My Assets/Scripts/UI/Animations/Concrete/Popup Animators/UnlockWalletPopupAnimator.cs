﻿using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Class which animates the UnlockWalletPopup.
/// </summary>
public class UnlockWalletPopupAnimator : UIAnimator
{

	[SerializeField] private GameObject dim;
	[SerializeField] private GameObject form;
	[SerializeField] private GameObject title;
	[SerializeField] private GameObject passwordInputField;
	[SerializeField] private GameObject signInButton;

	/// <summary>
	/// Initializes the necessary variables that haven't already been initialized in the inspector
	/// </summary>
	private void Awake()
	{
		passwordInputField.GetComponent<TMP_InputField>().onValueChanged.AddListener(InputFieldChanged);
		passwordInputField.GetComponent<TMP_InputField>().text = "";
	}

	/// <summary>
	/// Animates the UI elements of the form into view
	/// </summary>
	protected override void AnimateIn()
	{
		dim.AnimateGraphic(1f, 0.2f);
		form.AnimateGraphicAndScale(1f, 1f, 0.2f,
			() => title.AnimateScaleX(1f, 0.1f, 
			() => passwordInputField.AnimateScaleX(1f, 0.1f,
			() => signInButton.AnimateScaleX(1f, 0.1f, FinishedAnimating))));
	}

	/// <summary>
	/// Animates the UI elements of the form out of view
	/// </summary>
	protected override void AnimateOut()
	{
		title.AnimateScaleX(0f, 0.2f,
			() => form.AnimateGraphicAndScale(0f, 0f, 0.15f, FinishedAnimating));

		passwordInputField.AnimateScaleX(0f, 0.15f);
		signInButton.AnimateScaleX(0f, 0.15f);
	}

    /// <summary>
    /// Sets the button to interactable if the input field is not empty
    /// </summary>
    /// <param name="str"> The current string in the password input field </param>
    private void InputFieldChanged(string str) => signInButton.GetComponent<Button>().interactable = !string.IsNullOrEmpty(str);
}
