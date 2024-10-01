using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GuardianRegisterChild : MonoBehaviour
{
    [SerializeField] private UserData userData;
    [SerializeField] private APIController apiController;
    [SerializeField] private NotificationController notificationController;
    [SerializeField] private GameObject noBgLoader;

    [Space]
    [SerializeField] private TMP_InputField usernameTMP;
    [SerializeField] private TMP_InputField passwordTMP;
    [SerializeField] private TMP_InputField firstnameTMP;
    [SerializeField] private TMP_InputField lastnameTMP;

    public void Register()
    {
        if (usernameTMP.text == "")
        {
            notificationController.ShowError("Please input username", null);
            return;
        }
        else if (passwordTMP.text == "")
        {
            notificationController.ShowError("Please input password", null);
            return;
        }
        else if (firstnameTMP.text == "")
        {
            notificationController.ShowError("Please input first name", null);
            return;
        }
        else if (lastnameTMP.text == "")
        {
            notificationController.ShowError("Please input last name", null);
            return;
        }

        noBgLoader.SetActive(true);

        StartCoroutine(apiController.PostRequest("/children/createstudents", "", new Dictionary<string, object>
        {
            { "parentid", userData.UserID },
            { "studentusername", usernameTMP.text },
            { "password", passwordTMP.text },
            { "firstname", firstnameTMP.text },
            { "lastname", lastnameTMP.text }
        }, true, (response) =>
        {
            notificationController.ShowError("Child account created!", () =>
            {
                usernameTMP.text = "";
                passwordTMP.text = "";
                firstnameTMP.text = "";
                lastnameTMP.text = "";

                noBgLoader.SetActive(false);
            });
        }, () => noBgLoader.SetActive(false)));
    }
}
