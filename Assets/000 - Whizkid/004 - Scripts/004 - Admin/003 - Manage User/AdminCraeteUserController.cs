using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class AdminCraeteUserController : MonoBehaviour
{
    [SerializeField] private APIController apiController;
    [SerializeField] private NotificationController notificationController;
    [SerializeField] private AdminUserManagementController userManagementController;
    [SerializeField] private GameObject noBGLoading;
    [SerializeField] private GameObject createUserParent;

    [Space]
    [SerializeField] private TMP_InputField usernameTMP;
    [SerializeField] private TMP_InputField passwordTMP;
    [SerializeField] private TMP_InputField firstNameTMP;
    [SerializeField] private TMP_InputField lastNameTMP;

    public void CreateUser()
    {
        if (usernameTMP.text == "")
        {
            notificationController.ShowError("Please input username first.", null);
            return;
        }

        else if (passwordTMP.text == "")
        {
            notificationController.ShowError("Please input password first.", null);
            return;
        }

        else if (firstNameTMP.text == "")
        {
            notificationController.ShowError("Please input first name first.", null);
            return;
        }

        else if (lastNameTMP.text == "")
        {
            notificationController.ShowError("Please input last name first.", null);
            return;
        }

        noBGLoading.SetActive(true);

        notificationController.ShowConfirmation("Are your sure you want to register this student?", () =>
        {
            StartCoroutine(apiController.PostRequest("/users/createstudents", "", new Dictionary<string, object>
            {
                { "studentusername", usernameTMP.text },
                { "password", passwordTMP.text },
                { "firstname", firstNameTMP.text },
                { "lastname", lastNameTMP.text }
            }, true, (response) =>
            {
                notificationController.ShowError("User created!", () =>
                {
                    usernameTMP.text = "";
                    passwordTMP.text = "";
                    firstNameTMP.text = "";
                    lastNameTMP.text = "";

                    userManagementController.SearchUser();
                });
            }, () => noBGLoading.SetActive(false)));
        }, null);
    }

    public void CloseCreateUser()
    {
        createUserParent.SetActive(false);
        usernameTMP.text = "";
        passwordTMP.text = "";
        firstNameTMP.text = "";
        lastNameTMP.text = "";
    }
}
