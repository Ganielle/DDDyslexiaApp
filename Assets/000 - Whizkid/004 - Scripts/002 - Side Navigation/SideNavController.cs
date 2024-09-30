using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SideNavController : MonoBehaviour
{
    [SerializeField] private StateController stateController;
    [SerializeField] private StateController.UserState userState;
    [SerializeField] private StateController.AppStates appState;

    [Header("UI BUTTON")]
    [SerializeField] private Image navUI;
    [SerializeField] private Color selectedColor;
    [SerializeField] private Color unselectedColor;

    [Header("UI TMP")]
    [SerializeField] private TextMeshProUGUI labelTMP;
    [SerializeField] private Color selectedTMPColor;
    [SerializeField] private Color unselectedTMPColor;

    [Header("UI LOGO")]
    [SerializeField] private Image logoUI;
    [SerializeField] private Color selectedLogoColor;
    [SerializeField] private Color unselectedLogoColor;

    private void OnEnable()
    {
        CheckNavSelected();
        stateController.OnStateChanged += StateChange;
    }

    private void OnDisable()
    {
        stateController.OnStateChanged -= StateChange;
    }

    private void StateChange(object sender, EventArgs e)
    {
        CheckNavSelected();
    }

    private void CheckNavSelected()
    {
        if (stateController.CurrentUserState != userState)
            return;

        if (stateController.CurrentAppState == appState)
        {
            navUI.color = selectedColor;
            labelTMP.color = selectedTMPColor;
            logoUI.color = selectedLogoColor;
        }
        else
        {
            navUI.color = unselectedColor;
            labelTMP.color = unselectedTMPColor;
            logoUI.color = unselectedLogoColor;
        }
    }
}
