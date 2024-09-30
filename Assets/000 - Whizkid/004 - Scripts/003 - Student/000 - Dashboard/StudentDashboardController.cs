using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StudentDashboardController : MonoBehaviour
{
    [SerializeField] private NotificationController notificationController;
    [SerializeField] private StateController stateController;
    [SerializeField] private StateController.UserState userState;
    [SerializeField] private StateController.AppStates appState;
    [SerializeField] private APIController apiController;
    [SerializeField] private UserData userData;

    [Header("USER INFO")]
    [SerializeField] private TextMeshProUGUI usernameTMP;

    [Header("DASHBAORD")]
    [SerializeField] private GameObject loaderObj;
    [SerializeField] private List<GameObject> dashboardObjs;

    [Header("PREVIOUS RESULTS")]
    [SerializeField] private Image percentageImg;
    [SerializeField] private TextMeshProUGUI percentageScoreTMP;
    [SerializeField] private TextMeshProUGUI percentageTitleStoryTMP;

    [Header("PREVIOUS STATISTICS")]
    [SerializeField] private TextMeshProUGUI statisticsScoreTMP;
    [SerializeField] private TextMeshProUGUI statisticsAccuracyTMP;
    [SerializeField] private TextMeshProUGUI statisticsSpeedTMP;
    [SerializeField] private TextMeshProUGUI statisticsProsodyTMP;

    [Header("PREVIOUS PROSODY")]
    [SerializeField] private TextMeshProUGUI prosodyPitchTMP;
    [SerializeField] private TextMeshProUGUI prosodyIntensityTMP;
    [SerializeField] private TextMeshProUGUI prosodyTempoTMP;

    //  =========================

    Coroutine dashboardAPI;

    //  =========================

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
        if (stateController.CurrentUserState == userState)
            CallAPI();
    }

    private void CallAPI()
    {
        if (dashboardAPI != null) StopCoroutine(dashboardAPI);

        if (stateController.CurrentAppState == appState)
        {
            usernameTMP.text = userData.Username;

            loaderObj.SetActive(true);

            foreach (GameObject gameObj in dashboardObjs)
                gameObj.SetActive(false);

            dashboardAPI = StartCoroutine(apiController.GetRequest("/dashboard/getstudentdashboard", "", false, (response) =>
            {
                if (response.ToString() != "")
                {
                    StoryAssessment responsedata = JsonConvert.DeserializeObject<StoryAssessment>(response.ToString());

                    if (responsedata.title == "")
                    {
                        percentageImg.fillAmount = 0;
                        percentageScoreTMP.text = "No score yet";
                        percentageTitleStoryTMP.text = "No title yet";

                        statisticsScoreTMP.text = "0";
                        statisticsAccuracyTMP.text = "0";
                        statisticsSpeedTMP.text = "0";
                        statisticsProsodyTMP.text = "0";
                    }
                    else
                    {
                        percentageImg.fillAmount = responsedata.statistics.score / 100;
                        percentageScoreTMP.text = $"{responsedata.statistics.score:n0}%";
                        percentageTitleStoryTMP.text = responsedata.title;

                        statisticsScoreTMP.text = responsedata.statistics.score.ToString("n0");
                        statisticsAccuracyTMP.text = responsedata.statistics.accuracy.ToString("n0");
                        statisticsSpeedTMP.text = responsedata.statistics.speed.ToString("n0");
                        statisticsProsodyTMP.text = responsedata.statistics.prosody.ToString("n0");

                        prosodyPitchTMP.text = responsedata.statistics.pitch.ToString("n0");
                        prosodyIntensityTMP.text = responsedata.statistics.intensity.ToString("n0");
                        prosodyTempoTMP.text = responsedata.statistics.tempo.ToString("n0");
                    }
                }
                else
                {
                    percentageImg.fillAmount = 0;
                    percentageScoreTMP.text = "No score yet";
                    percentageTitleStoryTMP.text = "No title yet";

                    statisticsScoreTMP.text = "0";
                    statisticsAccuracyTMP.text = "0";
                    statisticsSpeedTMP.text = "0";
                    statisticsProsodyTMP.text = "0";

                    prosodyPitchTMP.text = "0";
                    prosodyIntensityTMP.text = "0";
                    prosodyTempoTMP.text = "0";
                }

                loaderObj.SetActive(false);

                foreach (GameObject gameObj in dashboardObjs)
                    gameObj.SetActive(true);

                dashboardAPI = null;

            }, null));
        }
    }

    public void Logout()
    {
        notificationController.ShowConfirmation("Are you sure you want to logout?", () =>
        {
            userData.Token = "";
            userData.Username = "";
            stateController.CurrentUserState = StateController.UserState.NONE;
            stateController.CurrentAppState = StateController.AppStates.NONE;
        }, null);
    }
}

[System.Serializable]
public class StoryAssessment
{
    public string title;
    public StoryStatistics statistics;
}

[System.Serializable]
public class StoryStatistics
{
    public float score;
    public float accuracy;
    public float speed;
    public float prosody;
    public float pitch;
    public float intensity;
    public float tempo;
}