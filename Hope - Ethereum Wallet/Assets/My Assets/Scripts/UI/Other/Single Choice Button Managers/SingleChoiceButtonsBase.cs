﻿using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages a set of buttons and manages the change in visuals when one button is pressed
/// </summary>
public class SingleChoiceButtonsBase : MonoBehaviour
{
	public event Action<int> OnButtonChanged;

	public int PreviouslyActiveButton { get; private set; }

	/// <summary>
	/// Sets the variables of the radio buttons
	/// </summary>
	private void Awake()
	{
		for (int i = 0; i < transform.childCount; i++)
			SetButtonListener(i);
	}

	/// <summary>
	/// Sets the button listener for the given index
	/// </summary>
	/// <param name="index"> The index of the button in the hiearchy </param>
	private void SetButtonListener(int index) => transform.GetChild(index).GetComponent<Button>().onClick.AddListener(() => ButtonClicked(index));

	/// <summary>
	/// A new button has been clicked
	/// </summary>
	/// <param name="newlyActiveButton"> The number of the button in the hiearchy that is active </param>
	public void ButtonClicked(int newlyActiveButton)
	{
		SetButtonVisuals(PreviouslyActiveButton, false);
		SetButtonVisuals(newlyActiveButton, true);

		OnButtonChanged?.Invoke(newlyActiveButton);

		PreviouslyActiveButton = newlyActiveButton;
	}

	/// <summary>
	/// Changes the visuals of the newly active, and previously active button
	/// </summary>
	/// <param name="buttonNum"> the index of the button being changed </param>
	/// <param name="active"> Whether the button is currently active or not </param>
	protected virtual void SetButtonVisuals(int buttonNum, bool active) => transform.GetChild(buttonNum).GetComponent<Button>().interactable = !active;
}
