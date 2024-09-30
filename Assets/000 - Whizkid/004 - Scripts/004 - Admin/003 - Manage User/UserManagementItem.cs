using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UserManagementItem : MonoBehaviour
{
    [SerializeField] private StateController stateController;
    [SerializeField] private AdminUserViewController adminUserViewController;

    [SerializeField] private TextMeshProUGUI firstnameTMP;
    [SerializeField] private TextMeshProUGUI lastnameTMP;

    [SerializeField] private Button viewBtn;

    [Header("DEBUGGER")]
    [SerializeField] private string userID;

    public void InitializeData(string firstname, string lastname, string userID)
    {
        firstnameTMP.text = firstname;
        lastnameTMP.text = lastname;
        this.userID = userID;

        if (userID == "")
            viewBtn.gameObject.SetActive(false);
        else
            viewBtn.gameObject.SetActive(true);
    }

    public void ViewData()
    {
        adminUserViewController.userid = userID;
        stateController.CurrentAppState = StateController.AppStates.ADMINUSERVIEW;
    }
}
