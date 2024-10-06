using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public enum Difficulty
{
    NONE,
    EASY,
    MEDIUM,
    HARD
}

public class ChildReadItController : MonoBehaviour
{
    [SerializeField] private GameObject loadingNoBG;
    [SerializeField] private APIController apiController;
    [SerializeField] private NotificationController notificationController;
    [SerializeField] private UserData userData;

    [Space]
    [SerializeField] private List<ReadItItem> items;
    [SerializeField] private float easyTime;
    [SerializeField] private float mediumTime;
    [SerializeField] private float hardTime;

    [Space]
    [SerializeField] private int sampleWindow = 128;
    [SerializeField] private float silenceThreshold = 0.02f; // Adjust threshold for silence detection
    [SerializeField] private float maxSilenceDuration = 1.5f; // Max silence duration before stopping recording

    [Space]
    [SerializeField] private GameObject difficultyMenu;
    [SerializeField] private GameObject readItGameplay;

    [Space]
    [SerializeField] private TextMeshProUGUI letterTMP;
    [SerializeField] private TextMeshProUGUI timerTMP;

    [Space]
    [SerializeField] private Button skipBtn;
    [SerializeField] private Button startBtn;

    [Space]
    [SerializeField] private AudioSource sfxAudioSource; 

    [Header("DEBUGGER")]
    [SerializeField] private Difficulty difficulty;
    [SerializeField] private int readitIndex;
    [SerializeField] private float currentTime;
    [SerializeField] private bool startReadIt;
    [SerializeField] private bool goingBack;
    [SerializeField] private float silenceTimer = 0f;
    [SerializeField] private string microphoneName;
    [SerializeField] private AudioClip recordClip;
    [SerializeField] private int position;

    private void Update()
    {
        DetectMicrophone();
    }

    private void SetReadItData()
    {
        startReadIt = false;
        goingBack = false;

        letterTMP.text = items[readitIndex].Letter;

        if (difficulty == Difficulty.EASY) currentTime = easyTime;
        else if (difficulty == Difficulty.MEDIUM) currentTime = mediumTime;
        else currentTime = easyTime;

        timerTMP.text = UpdateTimerDisplay(currentTime);
        startBtn.interactable = true;
        skipBtn.interactable = true;
        position = 0;
        silenceTimer = 0f;
        microphoneName = Microphone.devices[1];
    }

    private void DetectMicrophone()
    {
        if (startReadIt && !goingBack)
        {
            if (Microphone.IsRecording(microphoneName))
            {
                position = Microphone.GetPosition(microphoneName);
            }

            if (DetectSilence())
            {
                StopRecording();
            }
        }
    }

    private void StopRecord()
    {
        startReadIt = false;
        Microphone.End(microphoneName);
    }

    private void StopRecording()
    {
        loadingNoBG.SetActive(true);

        startReadIt = false;

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

        StartCoroutine(AssessmentAPI(filePath, myGUID.ToString()));
    }

    IEnumerator AssessmentAPI(string filePath, string uuid)
    {
        byte[] audioBytes = File.ReadAllBytes(filePath);
        WWWForm form = new WWWForm();
        form.AddBinaryData("story", audioBytes, $"{uuid}.wav", "audio/wav");
        form.AddField("referenceletter", items[readitIndex].Letter);
        form.AddField("number", readitIndex);

        UnityWebRequest apiRquest = UnityWebRequest.Post($"{apiController.url}/readit/assessment", form);
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
                            difficultyMenu.SetActive(true);
                            readItGameplay.SetActive(false);
                        });
                        loadingNoBG.SetActive(false);

                        yield break;
                    }

                    if (apiresponse["message"].ToString() != "success")
                    {
                        //  ERROR PANEL HERE
                        if (!apiresponse.ContainsKey("data"))
                        {
                            Debug.Log("Error API CALL! Error Code: " + response);
                            notificationController.ShowError("Error Process! Error Code: " + apiresponse["message"].ToString(), () =>
                            {
                                difficultyMenu.SetActive(true);
                                readItGameplay.SetActive(false);
                                loadingNoBG.SetActive(false);
                            });

                            yield break;
                        }
                        Debug.Log($"Error API CALL! Error Code: {response}");
                        notificationController.ShowError($"{apiresponse["data"]}", () =>
                        {
                            difficultyMenu.SetActive(true);
                            readItGameplay.SetActive(false);
                        });

                        loadingNoBG.SetActive(false);
                        yield break;
                    }

                    if (!apiresponse.ContainsKey("data"))
                    {
                        notificationController.ShowError($"There's a problem with the server! Please try again later or contact customer support", () =>
                        {
                            difficultyMenu.SetActive(true);
                            readItGameplay.SetActive(false);
                        });

                        loadingNoBG.SetActive(false);

                        yield break;
                    }

                    Debug.Log("SUCCESS API CALL");
                    Debug.Log(response);

                    StoryStatistics tempdata = JsonConvert.DeserializeObject<StoryStatistics>(apiresponse["data"].ToString());

                    notificationController.ShowError($"Assessment Done!\n\n" +
                    $"Total Score: {tempdata.score:n0}%\n" +
                    $"Accuracy: {tempdata.accuracy:n0}\n" +
                    $"Speed: {tempdata.speed:n0}\n" +
                    $"Prosody: {tempdata.prosody:n0}%", () =>
                    {
                        NextAssessment();
                    });

                    loadingNoBG.SetActive(false);
                }
                catch (Exception ex)
                {
                    //  ERROR PANEL HERE
                    loadingNoBG.SetActive(false);
                    Debug.Log("Error API CALL! Error Code: " + response);
                    notificationController.ShowError("There's a problem with the server! Please try again later.", () =>
                    {
                        difficultyMenu.SetActive(true);
                        readItGameplay.SetActive(false);
                    });

                    loadingNoBG.SetActive(false);
                }
            }
            else
            {
                //  ERROR PANEL HERE
                loadingNoBG.SetActive(false);
                Debug.Log("Error API CALL! Error Code: " + response);
                notificationController.ShowError("There's a problem with the server! Please try again later.", () =>
                {
                    difficultyMenu.SetActive(true);
                    readItGameplay.SetActive(false);
                });

                loadingNoBG.SetActive(false);
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
                        {
                            difficultyMenu.SetActive(true);
                            readItGameplay.SetActive(false);
                        });

                        loadingNoBG.SetActive(false);
                        break;
                    case 300:
                        loadingNoBG.SetActive(false);
                        Debug.Log("Error API CALL! Error Code: " + apiRquest.downloadHandler.text);
                        notificationController.ShowError($"{apiresponse["data"]}", () =>
                        {
                            difficultyMenu.SetActive(true);
                            readItGameplay.SetActive(false);
                        });

                        loadingNoBG.SetActive(false);
                        break;
                    case 301:
                        loadingNoBG.SetActive(false);
                        Debug.Log("Error API CALL! Error Code: " + apiRquest.downloadHandler.text);
                        notificationController.ShowError($"{apiresponse["data"]}", () =>
                        {
                            difficultyMenu.SetActive(true);
                            readItGameplay.SetActive(false);
                        });

                        loadingNoBG.SetActive(false);
                        break;
                }
            }
            catch (Exception ex)
            {
                loadingNoBG.SetActive(false);
                Debug.Log("Error API CALL! Error Code: " + apiRquest.result + ", " + apiRquest.downloadHandler.text);
                notificationController.ShowError("There's a problem with your internet connection! Please check your connection and try again.", () =>
                {
                    difficultyMenu.SetActive(true);
                    readItGameplay.SetActive(false);
                });

                loadingNoBG.SetActive(false);
            }
        }
    }

    public void SelectDifficulty(int index)
    {
        difficulty = (Difficulty)index;
        SetReadItData();
    }

    public void ListenOnAlphabet()
    {
        sfxAudioSource.PlayOneShot(items[readitIndex].LetterClip);
    }

    public void StartReading()
    {
        startBtn.interactable = false;
        skipBtn.interactable = false;
        recordClip = Microphone.Start(microphoneName, true, 360, 44100);
        startReadIt = true;
    }

    public void NextAssessment()
    {
        if (readitIndex >= items.Count - 1)
        {
            notificationController.ShowConfirmation("Congratulations! You reached the end of the alphabet assessment.", () =>
            {
                difficultyMenu.SetActive(true);
                readItGameplay.SetActive(false);
            }, null);
            return;
        }
        readitIndex++;
        SetReadItData();
    }

    public void Skip()
    {
        if (readitIndex >= items.Count - 1)
        {
            notificationController.ShowConfirmation("Are you sure you want to skip this alphabet? There's no next alphabet left, if you skip this you would return to menu.", () =>
            {
                difficultyMenu.SetActive(true);
                readItGameplay.SetActive(false);
            }, null);
            return;
        }

        readitIndex++;
        SetReadItData();
    }

    public void GoBack()
    {
        goingBack = true;
        notificationController.ShowConfirmation("Are you sure you want to stop the assessment?", () =>
        {
            StopRecord();
            difficultyMenu.SetActive(true);
            readItGameplay.SetActive(false);
        }, () => goingBack = false);
    }

    #region HELPER FUNCTIONS

    string UpdateTimerDisplay(float timeToDisplay)
    {
        // Clamp time to prevent negative values
        timeToDisplay = Mathf.Clamp(timeToDisplay, 0, Mathf.Infinity);

        // Get minutes and seconds from the float time
        int minutes = Mathf.FloorToInt(timeToDisplay / 60);
        int seconds = Mathf.FloorToInt(timeToDisplay % 60);

        // Format string with leading zeroes (e.g., 00 : 20)
        return string.Format("Timer: {0:00} : {1:00}", minutes, seconds);
    }

    bool DetectSilence()
    {
        float currentVolume = GetMicrophoneVolume();

        // If volume is below the threshold, start the silence timer
        if (currentVolume < silenceThreshold)
        {
            silenceTimer += Time.deltaTime;

            // If silence lasts long enough, return true
            if (silenceTimer >= maxSilenceDuration)
            {
                return true;
            }
        }
        else
        {
            // If the user is speaking, reset the silence timer
            silenceTimer = 0f;
        }

        return false;
    }

    float GetMicrophoneVolume()
    {
        // Get the current position of the microphone
        int micPosition = Microphone.GetPosition(null);

        // Make sure the micPosition is greater than sampleWindow
        if (micPosition < sampleWindow || micPosition - sampleWindow < 0)
        {
            // If the microphone has not recorded enough samples yet, return 0
            return 0f;
        }

        float[] samples = new float[sampleWindow];
        recordClip.GetData(samples, micPosition - sampleWindow); // Get data safely

        float sum = 0f;
        foreach (float sample in samples)
        {
            sum += Mathf.Abs(sample); // Sum absolute values of samples
        }

        return sum / sampleWindow; // Return average volume
    }

    #endregion
}
