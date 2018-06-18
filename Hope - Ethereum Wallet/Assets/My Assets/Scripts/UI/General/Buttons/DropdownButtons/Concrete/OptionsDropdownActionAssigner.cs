﻿using UnityEngine;

/// <summary>
/// Class which assigns the actions for each dropdown button in the options menu.
/// </summary>
public class OptionsDropdownActionAssigner
{

    public OptionsDropdownActionAssigner(UIManager.Settings uiSettings, PopupManager popupManager)
    {
        uiSettings.generalSettings.dropdowns.extraOptionsDropdowns[0].onClickAction = () => popupManager.GetPopup<PRPSHodlPopup>();
    }
}