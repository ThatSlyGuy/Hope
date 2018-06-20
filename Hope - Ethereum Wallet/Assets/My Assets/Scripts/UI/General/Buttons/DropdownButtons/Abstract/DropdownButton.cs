﻿using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class DropdownButton : ImageButton, IObserveLeftClick, IObserveRightClick
{

    public Text text;
    public DropdownButtonInfo[] dropdownButtons;

    private List<Button> buttonList = new List<Button>();
    private GameObject mainButtonObj;

    private MouseClickObserver mouseClickObserver;

    [Inject]
    public void Construct(MouseClickObserver mouseClickObserver) => this.mouseClickObserver = mouseClickObserver;

    protected override void OnAwake()
    {
        mainButtonObj = gameObject;
        Button.onClick.AddListener(ChangeButtonDropdown);
    }

    private void OnDisable()
    {
        Button.onClick.RemoveAllListeners();
    }

    private void ChangeButtonDropdown()
    {
        if (buttonList.Count > 0)
            CloseButtonDropdown();
        else
            OpenButtonDropdown();
    }

    public void CloseButtonDropdown()
    {
        buttonList.SafeForEach(button => Destroy(button.gameObject));
        buttonList.Clear();

        mouseClickObserver.RemoveLeftClickObserver(this);
        mouseClickObserver.RemoveRightClickObserver(this);
    }

    private void OpenButtonDropdown()
    {
        var buttonToInstantiate = mainButtonObj;
        foreach (DropdownButtonInfo button in dropdownButtons)
        {
            var newButton = Instantiate(buttonToInstantiate, transform);
            var rectTransform = newButton.GetComponent<RectTransform>();
            var dropdownComponent = newButton.GetComponent<DropdownButton>();
            dropdownComponent.buttonImage.sprite = button.buttonImage;
            dropdownComponent.text.text = button.buttonText;
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.localPosition = new Vector3(0f, (buttonList.Count + 1) * -rectTransform.rect.size.y, 0f);

            Destroy(dropdownComponent);

            var buttonComponent = newButton.GetComponent<Button>();
            if (button.onClickAction != null)
                buttonComponent.onClick.AddListener(button.onClickAction);
            buttonComponent.onClick.AddListener(CloseButtonDropdown);

            buttonList.Add(buttonComponent);
            buttonToInstantiate = newButton;
        }

        mouseClickObserver.AddLeftClickObserver(this);
        mouseClickObserver.AddRightClickObserver(this);
    }

    public void OnRightClick(ClickType clickType) => CheckBounds(clickType);

    public void OnLeftClick(ClickType clickType) => CheckBounds(clickType);

    private void CheckBounds(ClickType clickType)
    {
        if (clickType != ClickType.Down)
            return;

        var mousePosition = Input.mousePosition;
        var inBounds = false;

        buttonList.ForEach(button =>
        {
            if (button.GetButtonBounds().Contains(mousePosition))
                inBounds = true;
        });

        if (!inBounds && !Button.GetButtonBounds().Contains(mousePosition))
            CloseButtonDropdown();
    }

}