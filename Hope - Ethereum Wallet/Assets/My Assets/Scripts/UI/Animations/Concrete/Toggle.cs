﻿using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;

public class Toggle : MonoBehaviour
{

	[SerializeField] private GameObject toggleBackground;
	[SerializeField] private GameObject toggleCircle;

	private readonly Color blueColor = new Color(0.388f, 0.694f, 1f);
	private readonly Color fadedColor = new Color(1f, 1f, 1f);

	private Action toggleClick;

	public bool IsToggledOn { get; set; }

	/// <summary>
	/// Sets the button listeners
	/// </summary>
	private void Awake()
	{
		toggleBackground.GetComponent<Button>().onClick.AddListener(ToggleClicked);
		toggleCircle.GetComponent<Button>().onClick.AddListener(ToggleClicked);
	}

    public void AddToggleListener(Action action)
    {
        if (toggleClick == null)
            toggleClick = action;
        else
            toggleClick += action;
    }

	/// <summary>
	/// Animates the Circle over to the left or right, and animates the colors of the circle and background image
	/// </summary>
	private void ToggleClicked()
	{
		toggleCircle.AnimateTransformX(IsToggledOn ? -12f : 12f, 0.1f);
		toggleCircle.GetComponent<Image>().DOColor(IsToggledOn ? fadedColor : blueColor, 0.1f);
		toggleBackground.GetComponent<Image>().DOColor(IsToggledOn ? fadedColor : blueColor, 0.1f);
		IsToggledOn = !IsToggledOn;

		toggleClick?.Invoke();
	}
}
