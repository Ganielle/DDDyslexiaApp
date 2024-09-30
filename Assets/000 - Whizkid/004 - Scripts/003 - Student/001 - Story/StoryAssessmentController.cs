using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class StoryAssessmentController : MonoBehaviour
{
    [SerializeField] private UserData userData;
    [SerializeField] private APIController apiController;
    [SerializeField] private NotificationController notificationController;
    [SerializeField] private StateController stateController;
    [SerializeField] private StateController.UserState userState;
    [SerializeField] private StateController.AppStates appState;
    [SerializeField] private GameObject loadingNoBG;

    [Header("STORY OBJS")]
    [SerializeField] private GameObject instructionObj;

    [Header("STORY CONTENT")]
    [SerializeField] private Image storyBG;
    [SerializeField] private TextMeshProUGUI storyTitleTMP;
    [SerializeField] private TextMeshProUGUI storyContentTMP;

    [Header("ASSESSMENT")]
    [SerializeField] private GameObject assessmentObj;
    [SerializeField] private TextMeshProUGUI scoreAssessmentTMP;
    [SerializeField] private TextMeshProUGUI accuracyAssessmentTMP;
    [SerializeField] private TextMeshProUGUI speedAssessmentTMP;
    [SerializeField] private TextMeshProUGUI prosodyAssessmentTMP;

    [Header("DEBUGGER")]
    [SerializeField] private StoryData currentStoryData;
    [SerializeField] private string microphoneName;
    [SerializeField] private bool startingStory;
    [SerializeField] private int position;

    //  ========================

    private AudioClip recordClip;

    Coroutine StoryAssessmentAPI;

    //  ========================

    private void OnEnable()
    {
        stateController.OnStateChanged += StateChanged;
    }

    private void OnDisable()
    {
        stateController.OnStateChanged -= StateChanged;
    }

    private void StateChanged(object sender, EventArgs e)
    {
        BeforeStartStory();
        CancelAssessmentNavigation();
    }

    private void Update()
    {
        if (startingStory)
        {
            if (Microphone.IsRecording(microphoneName))
            {
                position = Microphone.GetPosition(microphoneName);
            }
        }
    }

    private void CancelAssessmentNavigation()
    {
        if (stateController.CurrentAppState == appState) return;

        if (!startingStory) return;

        Microphone.End(microphoneName);

        currentStoryData = null;
        microphoneName = "";
        startingStory = false;
        position = 0;
    }

    private void BeforeStartStory()
    {
        if (stateController.CurrentUserState != userState) return;

        if (stateController.CurrentAppState != appState) return;

        instructionObj.SetActive(true);

        storyTitleTMP.text = currentStoryData.Title;
        storyContentTMP.text = currentStoryData.Content;
        storyBG.sprite = currentStoryData.StoryBG;
    }

    public void SelectStory(StoryData tempStory)
    {
        notificationController.ShowConfirmation($"Are you sure you want to read {tempStory.Title}?", () =>
        {
            currentStoryData = tempStory;
            stateController.CurrentAppState = appState;
        }, null);
    }

    public void StartStoryAssessment()
    {
        if (stateController.CurrentUserState != userState) return;

        if (stateController.CurrentAppState != appState)
        {
            if (StoryAssessmentAPI != null) StopCoroutine(StoryAssessmentAPI);
            return;
        }

        if (startingStory) return;

        startingStory = true;

        microphoneName = Microphone.devices[0];
        recordClip = Microphone.Start(microphoneName, false, 360, 44100);

        instructionObj.SetActive(false);
    }

    public void StopRecording()
    {
        loadingNoBG.SetActive(true);

        startingStory = false;

        Microphone.End(microphoneName);

        // Capture the current clip data
        var soundData = new float[recordClip.samples * recordClip.channels];
        recordClip.GetData(soundData, 0);

        // Create a shortened array for the data that was used for recording
        var newData = new float[position * recordClip.channels];

        // Copy the used samples to a new array
        for (int i = 0; i < newData.Length; i++)
        {
            newData[i] = soundData[i];
        }

        // One does not simply shorten an AudioClip,
        // so we make a new one with the appropriate length
        var newClip = AudioClip.Create(
            recordClip.name,
            position,
            recordClip.channels,
            recordClip.frequency,
            false,
            false
        );

        newClip.SetData(newData, 0); // Give it the data from the old clip

        // Replace the old clip
        Destroy(recordClip);
        recordClip = newClip;

        Guid myGUID = Guid.NewGuid();

        string filePath = Path.Combine(Application.persistentDataPath, $"{myGUID}.wav");
        SavWav.Save(filePath, recordClip);

        StoryAssessmentAPI = StartCoroutine(AssessmentAPI(filePath, myGUID.ToString()));
    }

    public void GoBackToDashboard()
    {
        currentStoryData = null;
        microphoneName = "";
        startingStory = false;
        position = 0;
        stateController.CurrentAppState = StateController.AppStates.USERDASHBOARD;
    }

    public void CancelAssessment()
    {
        notificationController.ShowConfirmation("Are you sure you want to cancel the assessment?", () =>
        {
            Microphone.End(microphoneName);
            GoBackToDashboard();
        }, null);
    }

    IEnumerator AssessmentAPI(string filePath, string uuid)
    {
        byte[] audioBytes = File.ReadAllBytes(filePath);
        WWWForm form = new WWWForm();
        form.AddBinaryData("story", audioBytes, $"{uuid}.wav", "audio/wav");
        form.AddField("referencestory", currentStoryData.Content);
        form.AddField("storytitle", currentStoryData.Title);

        UnityWebRequest apiRquest = UnityWebRequest.Post($"{apiController.url}/story/assessment", form);
        apiRquest.SetRequestHeader("Accept", "application/json");
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
                        notificationController.ShowError("There's a problem with the server! Please try again later.", () => 
                        {
                            stateController.CurrentAppState = StateController.AppStates.USERDASHBOARD;
                            StoryAssessmentAPI = null;
                            loadingNoBG.SetActive(false);
                        });
                        yield break;
                    }

                    if (apiresponse["message"].ToString() != "success")
                    {
                        //  ERROR PANEL HERE
                        if (!apiresponse.ContainsKey("data"))
                        {
                            Debug.Log("Error API CALL! Error Code: " + response);
                            notificationController.ShowError("Error Process! Error Code: " + apiresponse["message"].ToString(), () =>
                            stateController.CurrentAppState = StateController.AppStates.USERDASHBOARD);

                            loadingNoBG.SetActive(false);
                            StoryAssessmentAPI = null;

                            yield break;
                        }
                        Debug.Log($"Error API CALL! Error Code: {response}");
                        notificationController.ShowError($"{apiresponse["data"]}", () =>
                            stateController.CurrentAppState = StateController.AppStates.USERDASHBOARD);

                        loadingNoBG.SetActive(false);
                        StoryAssessmentAPI = null;
                        yield break;
                    }

                    if (!apiresponse.ContainsKey("data"))
                    {
                        notificationController.ShowError($"There's a problem with the server! Please try again later or contact customer support", () =>
                            stateController.CurrentAppState = StateController.AppStates.USERDASHBOARD);

                        StoryAssessmentAPI = null;
                    }

                    Debug.Log("SUCCESS API CALL");
                    Debug.Log(response);

                    StoryStatistics tempdata = JsonConvert.DeserializeObject<StoryStatistics>(apiresponse["data"].ToString());

                    scoreAssessmentTMP.text = $"{tempdata.score:n0}%";
                    accuracyAssessmentTMP.text = tempdata.accuracy.ToString("n0");
                    speedAssessmentTMP.text = tempdata.speed.ToString("n0");
                    prosodyAssessmentTMP.text = $"{tempdata.prosody:n0}%";

                    loadingNoBG.SetActive(false);

                    assessmentObj.SetActive(true);

                    StoryAssessmentAPI = null;
                }
                catch (Exception ex)
                {
                    //  ERROR PANEL HERE
                    loadingNoBG.SetActive(false);
                    Debug.Log("Error API CALL! Error Code: " + response);
                    notificationController.ShowError("There's a problem with the server! Please try again later.", () =>
                            stateController.CurrentAppState = StateController.AppStates.USERDASHBOARD);

                    StoryAssessmentAPI = null;
                }
            }
            else
            {
                //  ERROR PANEL HERE
                loadingNoBG.SetActive(false);
                Debug.Log("Error API CALL! Error Code: " + response);
                notificationController.ShowError("There's a problem with the server! Please try again later.", () =>
                            stateController.CurrentAppState = StateController.AppStates.USERDASHBOARD);

                StoryAssessmentAPI = null;
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
                        loadingNoBG.SetActive(false);
                        Debug.Log("Error API CALL! Error Code: " + apiRquest.downloadHandler.text);
                        notificationController.ShowError($"{apiresponse["data"]}", () =>
                            stateController.CurrentAppState = StateController.AppStates.USERDASHBOARD);
                        StoryAssessmentAPI = null;
                        break;
                    case 300:
                        loadingNoBG.SetActive(false);
                        Debug.Log("Error API CALL! Error Code: " + apiRquest.downloadHandler.text);
                        notificationController.ShowError($"{apiresponse["data"]}", () =>
                            stateController.CurrentAppState = StateController.AppStates.USERDASHBOARD);
                        StoryAssessmentAPI = null;
                        break;
                    case 301:
                        loadingNoBG.SetActive(false);
                        Debug.Log("Error API CALL! Error Code: " + apiRquest.downloadHandler.text);
                        notificationController.ShowError($"{apiresponse["data"]}", () =>
                            stateController.CurrentAppState = StateController.AppStates.USERDASHBOARD);
                        StoryAssessmentAPI = null;
                        break;
                }
            }
            catch (Exception ex)
            {
                loadingNoBG.SetActive(false);
                Debug.Log("Error API CALL! Error Code: " + apiRquest.result + ", " + apiRquest.downloadHandler.text);
                notificationController.ShowError("There's a problem with your internet connection! Please check your connection and try again.", () =>
                            stateController.CurrentAppState = StateController.AppStates.USERDASHBOARD);
                StoryAssessmentAPI = null;
            }
        }
    }
}
