﻿using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Class that manages the scrollbar visibility when user hovers over the list that this class is attached to
/// </summary>
public class ListScrollbarManager : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
	[SerializeField] private InteractableScrollbar interactableScrollbar;
	[SerializeField] private GameObject scrollbarHandle;

	//private InteractableScrollbar interactableScrollbar;

	public bool Hovering { get; set; }

	/// <summary>
	/// Gets the InteractableScrollbar component
	/// </summary>
	//private void Awake() => interactableScrollbar = scrollbarHandle.GetComponent<InteractableScrollbar>();

	/// <summary>
	/// Animates the scrollbar into visibility
	/// </summary>
	/// <param name="eventData"> The PointerEventData </param>
	public void OnPointerEnter(PointerEventData eventData)
	{
		AnimateScrollbar(true);
		Hovering = true;
	}

	/// <summary>
	/// Animates the scrollbar out of visibility
	/// </summary>
	/// <param name="eventData"> The PointerEventData </param>
	public void OnPointerExit(PointerEventData eventData)
	{
		if (!interactableScrollbar.MouseHeldDown)
			AnimateScrollbar(false);

		Hovering = false;
	}

	/// <summary>
	/// Animates the scrollbar graphic
	/// </summary>
	/// <param name="isHovered"> If the mouse is hovered over the list or not </param>
	public void AnimateScrollbar(bool isHovered) => scrollbarHandle.AnimateGraphic(isHovered ? 1f : 0f, 0.15f);
}
