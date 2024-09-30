using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class StoryAdminUserDataController : MonoBehaviour
{
    [SerializeField] private NotificationController notificationController;
    [SerializeField] private APIController apiController;
    [SerializeField] private StateController stateController;
    [SerializeField] private StateController.AppStates appState;
    [SerializeField] private StateController.UserState userState;
    [SerializeField] private GameObject noBGLoading;

    [Header("ASSESSMENT")]
    [SerializeField] private TextMeshProUGUI fullnameTMP;
    [SerializeField] private TextMeshProUGUI scoreTMP;
    [SerializeField] private Image scoreSlider;
    [SerializeField] private TextMeshProUGUI titleTMP;
    [SerializeField] private TextMeshProUGUI assessmentScoreTMP;
    [SerializeField] private TextMeshProUGUI assessmentAccuracyTMP;
    [SerializeField] private TextMeshProUGUI assessmentSpeedTMP;
    [SerializeField] private TextMeshProUGUI assessmentProsodyTMP;

    [Header("PREVIOUS PROSODY")]
    [SerializeField] private TextMeshProUGUI prosodyPitchTMP;
    [SerializeField] private TextMeshProUGUI prosodyIntensityTMP;
    [SerializeField] private TextMeshProUGUI prosodyTempoTMP;

    [Header("RECORD")]
    [SerializeField] private AudioSource recordSource;
    [SerializeField] private Button playBtn;
    [SerializeField] private Button stopBtn;

    [Header("STORY TRANSCRIPT")]
    [SerializeField] private TextMeshProUGUI storyTitle;
    [SerializeField] private TextMeshProUGUI referenceText;
    [SerializeField] private StoryData aBigWhiteHen;
    [SerializeField] private StoryData inSearchOfFlower;
    [SerializeField] private StoryData littleRedRidingHood;
    [SerializeField] private StoryData playingCatch;
    [SerializeField] private StoryData theNaughtyMonkey;

    [Header("DEBUGGER")]
    public string selectedStoryID;
    public string fullname;
    [SerializeField] private string currentRecordPath;

    //  =========================

    Coroutine AssessmentAPI;

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
        CheckAssessmentData();
    }

    private void CheckAssessmentData()
    {
        if (stateController.CurrentUserState != userState) return;

        if (stateController.CurrentAppState != appState)
        {
            if (AssessmentAPI != null)
            {
                StopCoroutine(AssessmentAPI);
                AssessmentAPI = null;
            }
            return;
        }

        noBGLoading.SetActive(true);

        AssessmentAPI = StartCoroutine(apiController.GetRequest("/story/viewstoryassessmentdataadmin", $"?historyid={selectedStoryID}", true, (response) =>
        {
            if (response.ToString() != "")
            {
                StoryHistoryItemData tempdata = JsonConvert.DeserializeObject<StoryHistoryItemData>(response.ToString());

                fullnameTMP.text = fullname;
                scoreTMP.text = $"{tempdata.score:n0}%";
                scoreSlider.fillAmount = tempdata.score / 100;
                titleTMP.text = tempdata.title;
                assessmentScoreTMP.text = $"{tempdata.score:n0}%";
                assessmentAccuracyTMP.text = tempdata.accuracy.ToString("n0");
                assessmentSpeedTMP.text = tempdata.speed.ToString("n0");
                assessmentProsodyTMP.text = tempdata.prosody.ToString("n0");
                currentRecordPath = tempdata.recordfile;

                prosodyPitchTMP.text = tempdata.pitch.ToString("n0");
                prosodyIntensityTMP.text = tempdata.intensity.ToString("n0");
                prosodyTempoTMP.text = tempdata.tempo.ToString("n0");

                StoryData tempstory;

                switch (tempdata.title)
                {
                    case "A BIG WHITE HEN":
                        tempstory = aBigWhiteHen;
                        break;
                    case "IN SEARCH OF FLOWERS":
                        tempstory = inSearchOfFlower;
                        break;
                    case "LITTLE RED RIDING HOOD":
                        tempstory = littleRedRidingHood;
                        break;
                    case "PLAYING CATCH":
                        tempstory = playingCatch;
                        break;
                    case "THE NAUGHTY MONKEY":
                        tempstory = theNaughtyMonkey;
                        break;
                    default:
                        tempstory = theNaughtyMonkey;
                        break;
                }

                storyTitle.text = tempstory.Title;

                ShowStoryTranscription(tempstory.Content, tempdata.transcript);

                StartCoroutine(GetAudio());

                AssessmentAPI = null;
            }
        }, null));
    }

    IEnumerator GetAudio()
    {
        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip($"{apiController.url}/{currentRecordPath}", AudioType.WAV))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
            {
                notificationController.ShowError("Audio file not found on server! Please contact customer support for more details", null);
                playBtn.gameObject.SetActive(false);
            }
            else
            {
                AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
                recordSource.clip = clip;
            }

            noBGLoading.SetActive(false);
        }
    }

    public void PlayRecord()
    {
        playBtn.gameObject.SetActive(false);
        stopBtn.gameObject.SetActive(true);
        recordSource.Play();
    }

    public void StopRecord()
    {
        stopBtn.gameObject.SetActive(false);
        playBtn.gameObject.SetActive(true);
        recordSource.Stop();
    }

    private void ShowStoryTranscription(string storyText, string transcriptionText)
    {
        // Convert texts to lowercase for case-insensitive comparison
        string lowerStoryText = storyText.ToLower();
        string lowerTranscriptionText = transcriptionText.ToLower();

        List<string> storyWords = TokenizeText(lowerStoryText);
        List<string> transcriptionWords = TokenizeText(lowerTranscriptionText);

        HashSet<string> storyWordSet = new HashSet<string>(storyWords);
        HashSet<string> transcriptionWordSet = new HashSet<string>(transcriptionWords);

        List<string> wordsInStoryNotInTranscription = new List<string>();
        List<string> wordsInTranscriptionNotInStory = new List<string>();

        foreach (string word in storyWordSet)
        {
            if (!transcriptionWordSet.Contains(word))
            {
                wordsInStoryNotInTranscription.Add(word);
            }
        }

        foreach (string word in transcriptionWordSet)
        {
            if (!storyWordSet.Contains(word))
            {
                wordsInTranscriptionNotInStory.Add(word);
            }
        }

        // Build highlighted text for TextMeshProUGUI
        referenceText.text = BuildHighlightedText(transcriptionWords, storyWordSet, "#0c7409", "red");
    }

    List<string> TokenizeText(string text)
    {
        // Simple tokenization by splitting text into words
        char[] delimiters = new char[] { ' ', '.', ',', '!', '?', '\"', '\n', '\r', ';', ':' };
        return new List<string>(text.Split(delimiters, System.StringSplitOptions.RemoveEmptyEntries));
    }

    string BuildHighlightedText(List<string> words, HashSet<string> comparisonSet, string existingColor, string notColor)
    {
        string result = "";
        foreach (string word in words)
        {
            if (comparisonSet.Contains(word))
            {
                result += $"<color={existingColor}>{word}</color> ";
            }
            else
            {
                result += $"<color={notColor}>{word}</color> ";
            }
        }
        return result;
    }
}
