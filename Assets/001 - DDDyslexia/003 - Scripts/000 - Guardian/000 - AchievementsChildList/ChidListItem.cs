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

    //  =================

    private Action finalAction;

    //  =================

    public void SetData(string userid, Action action)
    {
        this.userid = userid;
        finalAction = action;
    }

    public void SelectBtn()
    {
        finalAction?.Invoke();
    }
}
