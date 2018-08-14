﻿using System;
using UnityEngine;

public abstract class UIAnimator : MonoBehaviour
{
	[SerializeField] private bool animateOnEnable;

	[SerializeField] private GameObject blocker;
	[SerializeField] private GameObject dim;
	[SerializeField] protected GameObject form;
	[SerializeField] private GameObject blur;
	[SerializeField] protected GameObject formTitle;
	[SerializeField] protected GameObject popupContainer;

	protected Vector2 startingPosition;

	protected Action onAnimationFinished;

	private bool animating;

	// COMMENT THIS CLASSS!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!

	private void OnEnable()
	{
		if (popupContainer != null)
		{
			Vector2 currentMousePosition = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
			Vector2 updatedPosition = new Vector2(GetUpdatedValue(Screen.width / 2, currentMousePosition.x), GetUpdatedValue(Screen.height / 2, currentMousePosition.y));

			popupContainer.transform.localPosition = updatedPosition;
			startingPosition = updatedPosition;
		}

		if (animateOnEnable)
			AnimateEnable();
	}

	private float GetUpdatedValue(float ScreenDimensionInHalf, float currentPosition)
	{
		return currentPosition > ScreenDimensionInHalf ? currentPosition - ScreenDimensionInHalf : (ScreenDimensionInHalf - currentPosition) * -1f;
	}

	public bool Animating
	{
		get { return animating; }
		protected set { ChangeAnimationState(value); }
	}

	public void AnimateEnable(Action onAnimationFinished = null)
	{
		this.onAnimationFinished = onAnimationFinished;

		ChangeAnimationState(true);
		AnimateBasicElements(true);
	}

	public void AnimateDisable(Action onAnimationFinished = null)
	{
		this.onAnimationFinished = onAnimationFinished;

		ChangeAnimationState(true);
		AnimateBasicElements(false);
	}

	protected void AnimateBasicElements(bool animateIn)
	{
		if (popupContainer != null)
			popupContainer.AnimateTransform(animateIn ? Vector2.zero : startingPosition, 0.2f);

		float endValue = animateIn ? 1f : 0f;

		if (dim != null) dim.AnimateGraphic(endValue, 0.2f);

		if (blur != null) blur.AnimateScale(endValue, 0.2f);

		if (formTitle != null) formTitle.AnimateGraphicAndScale(0.85f, 1f, animateIn ? 0.3f : 0.2f);

		if (form != null)
			form.AnimateGraphicAndScale(1f, endValue, 0.2f, () => OnCompleteAction(animateIn));
		else
			OnCompleteAction(animateIn);
	}

	private void OnCompleteAction(bool animateIn)
	{
		if (animateIn)
		{
			AnimateUniqueElementsIn();
		}
		else
		{
			if (popupContainer == null)
				AnimateUniqueElementsOut();
			else
				FinishedAnimating();
		}
	}

	protected abstract void AnimateUniqueElementsIn();

	protected virtual void AnimateUniqueElementsOut() { }

	protected void FinishedAnimating()
	{
		ChangeAnimationState(false);
		onAnimationFinished?.Invoke();
	}

	private void ChangeAnimationState(bool state)
	{
		if (blocker != null)
			blocker.SetActive(state);

		animating = state;
	}

}
