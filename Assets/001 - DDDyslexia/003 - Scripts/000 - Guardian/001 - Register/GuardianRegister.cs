using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

public class GuardianRegister : MonoBehaviour
{
    [SerializeField] private APIController apiController;
    [SerializeField] private NotificationController notificationController;
    [SerializeField] private GameObject noBgLoader;

    [Space]
    [SerializeField] private TMP_InputField usernameTMP;
    [SerializeField] private TMP_InputField passwordTMP;
    [SerializeField] private TMP_InputField firstnameTMP;
    [SerializeField] private TMP_InputField lastnameTMP;

    [Space]
    [SerializeField] private GameObject guardianRegister;
    [SerializeField] private GameObject guardianLogin;

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

        StartCoroutine(apiController.PostRequest("/parent/createparent", "", new Dictionary<string, object>
        {
            { "username", usernameTMP.text },
            { "password", passwordTMP.text },
            { "firstname", firstnameTMP.text },
            { "lastname", lastnameTMP.text }
        }, true, (response) =>
        {
            notificationController.ShowError("Guardian created!", () =>
            {
                usernameTMP.text = "";
                passwordTMP.text = "";
                firstnameTMP.text = "";
                lastnameTMP.text = "";

                guardianLogin.SetActive(true);
                guardianRegister.SetActive(false);
                noBgLoader.SetActive(false);
            });
        }, () => noBgLoader.SetActive(false), false));
    }
}
