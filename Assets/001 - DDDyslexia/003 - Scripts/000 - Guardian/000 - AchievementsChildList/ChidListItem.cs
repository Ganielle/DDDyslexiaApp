using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ChidListItem : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI childNameTMP;

    [Header("DEBUGGER")]
    [SerializeField] private string userid;
    [SerializeField] private GuardianAchievementController controller;

    //  =================

    private Action finalAction;

    //  =================

    public void SetData(string userid, string fullname, GuardianAchievementController controller, Action action)
    {
        this.userid = userid;
        childNameTMP.text = fullname;
        finalAction = action;
        this.controller = controller;
    }

    public void SelectBtn()
    {
        controller.selectedChildren = userid;
        finalAction?.Invoke();
    }
}
