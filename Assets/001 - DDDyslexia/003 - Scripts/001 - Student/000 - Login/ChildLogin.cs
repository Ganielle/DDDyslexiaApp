using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

public class ChildLogin : MonoBehaviour
{
    [SerializeField] private APIController apiController;
    [SerializeField] private UserData userData;
    [SerializeField] private NotificationController notificationController;
    [SerializeField] private GameObject noBgLoader;

    [Header("TMP")]
    [SerializeField] private TMP_InputField usernameTMP;
    [SerializeField] private TMP_InputField passwordTMP;

    [Space]
    [SerializeField] private GameObject childLogin;
    [SerializeField] private GameObject childObj;
    [SerializeField] private GameObject childIntro;
    [SerializeField] private GameObject childMenu;

    public void Login()
    {
        noBgLoader.SetActive(true);
        if (usernameTMP.text == "")
        {
            notificationController.ShowError("Please input username first!", null);
            noBgLoader.SetActive(false);
            return;
        }

        if (passwordTMP.text == "")
        {
            notificationController.ShowError("Please input password first!", null);
            noBgLoader.SetActive(false);
            return;
        }

        StartCoroutine(LoginAPI());
    }

    public void Logout()
    {
        notificationController.ShowConfirmation("Are you sure you want to logout?", () =>
        {
            userData.Username = "";
            userData.Token = "";

            usernameTMP.text = "";
            passwordTMP.text = "";

            childLogin.SetActive(true);
            childObj.SetActive(false);
            childIntro.SetActive(true);
            childMenu.SetActive(false);

        }, null);
    }

    IEnumerator LoginAPI()
    {
        UnityWebRequest apiRquest = UnityWebRequest.Get($"{apiController.url}/auth/loginstudent?username={usernameTMP.text}&password={passwordTMP.text}");
        apiRquest.SetRequestHeader("Content-Type", "application/json");

        yield return apiRquest.SendWebRequest();

        if (apiRquest.result == UnityWebRequest.Result.Success)
        {
            string response = apiRquest.downloadHandler.text;

            if (response[0] == '{' && response[response.Length - 1] == '}')
            {
                try
                {
                    Dictionary<string, object> apiresponse = JsonConvert.DeserializeObject<Dictionary<string, object>>(response);

                    if (!apiresponse.ContainsKey("message"))
                    {
                        //  ERROR PANEL HERE
                        Debug.Log("Error API CALL! Error Code: " + response);
                        notificationController.ShowError("There's a problem with the server! Please try again later.", null);
                        noBgLoader.SetActive(false);
                        yield break;
                    }

                    if (apiresponse["message"].ToString() != "success")
                    {
                        //  ERROR PANEL HERE
                        if (apiresponse.ContainsKey("data"))
                        {
                            Debug.Log(apiresponse["data"].ToString());
                            notificationController.ShowError(apiresponse["data"].ToString(), null);

                            yield break;
                        }
                        notificationController.ShowError("There's a problem with the server! Please try again later.", null);
                        yield break;
                    }

                    if (!apiresponse.ContainsKey("data"))
                    {
                        notificationController.ShowError("There's a problem with the server! Please try again later.", null);
                        yield break;
                    }

                    Dictionary<string, object> dataresponse = JsonConvert.DeserializeObject<Dictionary<string, object>>(apiresponse["data"].ToString());

                    userData.Token = dataresponse["token"].ToString();
                    userData.UserID = dataresponse["id"].ToString();
                    userData.Username = usernameTMP.text;

                    childObj.SetActive(true);
                    childLogin.SetActive(false);

                    noBgLoader.SetActive(false);
                }
                catch (Exception ex)
                {
                    Debug.Log("Error API CALL! Error Code: " + response);
                    notificationController.ShowError("There's a problem with the server! Please try again later.", null);
                    noBgLoader.SetActive(false);
                }
            }
        }
        else
        {
            try
            {
                Dictionary<string, object> apiresponse = JsonConvert.DeserializeObject<Dictionary<string, object>>(apiRquest.downloadHandler.text);

                switch (apiRquest.responseCode)
                {
                    case 400:
                        notificationController.ShowError($"{apiresponse["data"]}", null);
                        noBgLoader.SetActive(false);
                        break;
                    case 300:
                        notificationController.ShowError($"{apiresponse["data"]}", null);
                        noBgLoader.SetActive(false);
                        break;
                    case 301:
                        notificationController.ShowError($"{apiresponse["data"]}", null);
                        noBgLoader.SetActive(false);
                        break;
                }
            }
            catch (Exception ex)
            {
                Debug.Log("Error API CALL! Error Code: " + apiRquest.result + ", " + apiRquest.downloadHandler.text);
                notificationController.ShowError("There's a problem with your internet connection! Please check your connection and try again.", null);
                noBgLoader.SetActive(false);
            }
        }
    }
}
