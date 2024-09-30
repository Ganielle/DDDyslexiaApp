using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class APIController : MonoBehaviour
{
    public string url;
    [SerializeField] private UserData userData;
    [SerializeField] private NotificationController notificationController;
    [SerializeField] private GameObject noBGLoding;

    public IEnumerator GetRequest(string route, string query, bool loaderEndState, System.Action<System.Object> callback, System.Action errorAction)
    {
        while (userData.Token == "") yield return null;

        UnityWebRequest apiRquest = UnityWebRequest.Get(url + route + query);
        apiRquest.SetRequestHeader("Content-Type", "application/json");
        apiRquest.SetRequestHeader("Authorization", "Bearer " + userData.Token);

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
                        errorAction?.Invoke();
                        yield break;
                    }

                    if (apiresponse["message"].ToString() != "success")
                    {
                        //  ERROR PANEL HERE
                        Debug.Log(apiresponse["data"].ToString());
                        errorAction?.Invoke();
                        yield break;
                    }

                    if (apiresponse.ContainsKey("data"))
                        callback?.Invoke(apiresponse["data"].ToString());
                    else
                        callback?.Invoke("");

                    noBGLoding.SetActive(loaderEndState);
                    Debug.Log("SUCCESS API CALL");
                }
                catch (Exception ex)
                {
                    //  ERROR PANEL HERE
                    Debug.Log("Error API CALL! Error Code: " + ex.Message);
                    Debug.Log("API response: " + response);
                    notificationController.ShowError("There's a problem with the server! Please try again later.", null);
                    errorAction?.Invoke();
                }
            }
            else
            {
                //  ERROR PANEL HERE
                Debug.Log("Error API CALL! Error Code: " + response);
                notificationController.ShowError("There's a problem with the server! Please try again later.", null);
                errorAction?.Invoke();
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
                        errorAction?.Invoke();
                        break;
                    case 300:
                        notificationController.ShowError($"{apiresponse["data"]}", null);
                        errorAction?.Invoke();
                        break;
                    case 301:
                        notificationController.ShowError($"{apiresponse["data"]}", null);
                        errorAction?.Invoke();
                        break;
                }
            }
            catch (Exception ex)
            {
                Debug.Log("Error API CALL! Error Code: " + apiRquest.result + ", " + apiRquest.downloadHandler.text);
                notificationController.ShowError("There's a problem with your internet connection! Please check your connection and try again.", null);
                errorAction?.Invoke();
            }
        }
    }

    public IEnumerator PostRequest(string route, string query, Dictionary<string, object> paramsBody, bool loaderEndState, System.Action<System.Object> callback, System.Action errorAction)
    {
        while (userData.Token == "") yield return null;

        UnityWebRequest apiRquest = UnityWebRequest.PostWwwForm(url + route + query, UnityWebRequest.kHttpVerbPOST);

        byte[] credBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(paramsBody));
        UploadHandler uploadHandler = new UploadHandlerRaw(credBytes);

        apiRquest.uploadHandler = uploadHandler;

        apiRquest.SetRequestHeader("Content-Type", "application/json");
        apiRquest.SetRequestHeader("Authorization", "Bearer " + userData.Token);

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

                        errorAction?.Invoke();
                        yield break;
                    }

                    if (apiresponse["message"].ToString() != "success")
                    {
                        //  ERROR PANEL HERE
                        if (!apiresponse.ContainsKey("data"))
                        {
                            Debug.Log("Error API CALL! Error Code: " + response);
                            notificationController.ShowError("Error Process! Error Code: " + apiresponse["message"].ToString(), () => errorAction?.Invoke());

                            yield break;
                        }
                        Debug.Log($"Error API CALL! Error Code: {apiresponse["data"]}");
                        notificationController.ShowError($"{apiresponse["data"]}", () => errorAction?.Invoke());
                        yield break;
                    }
                    if (apiresponse.ContainsKey("data"))
                    {

                        callback?.Invoke(apiresponse["data"].ToString());
                    }
                    else
                    {
                        callback?.Invoke("");
                    }

                    noBGLoding.SetActive(loaderEndState);

                    Debug.Log("SUCCESS API CALL");
                }
                catch (Exception ex)
                {
                    //  ERROR PANEL HERE
                    Debug.Log("Error API CALL! Error Code: " + ex.Message);
                    notificationController.ShowError("There's a problem with the server! Please try again later.", null);
                    errorAction?.Invoke();
                }
            }
            else
            {
                //  ERROR PANEL HERE
                Debug.Log("Error API CALL! Error Code: " + response);
                notificationController.ShowError("There's a problem with the server! Please try again later.", null);
                errorAction?.Invoke();
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
                        errorAction?.Invoke();
                        break;
                    case 300:
                        notificationController.ShowError($"{apiresponse["data"]}", null);
                        errorAction?.Invoke();
                        break;
                    case 301:
                        notificationController.ShowError($"{apiresponse["data"]}", null);
                        errorAction?.Invoke();
                        break;
                }
            }
            catch (Exception ex)
            {
                Debug.Log("Error API CALL! Error Code: " + apiRquest.result + ", " + apiRquest.downloadHandler.text);
                notificationController.ShowError("There's a problem with your internet connection! Please check your connection and try again.", null);
                errorAction?.Invoke();
            }
        }
    }
}
