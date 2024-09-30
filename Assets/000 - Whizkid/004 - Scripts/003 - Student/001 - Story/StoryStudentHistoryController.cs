using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StoryStudentHistoryController : MonoBehaviour
{
    [SerializeField] private GameObject noBGLoading;
    [SerializeField] private StateController stateController;
    [SerializeField] private StateController.AppStates appState;
    [SerializeField] private StateController.UserState userState;
    [SerializeField] private APIController apiController;

    [Header("PAGINATION")]
    [SerializeField] private TMP_InputField pageTMP;
    [SerializeField] private Button previousBtn;
    [SerializeField] private Button nextBtn;

    [Header("HISTORY")]
    [SerializeField] private List<StoryStudentHistoryItem> historyItems;
    [SerializeField] private GameObject historyListParent;

    [Header("DEBUGGER")]
    [SerializeField] private int paginationIndex;
    [SerializeField] private int totalPages;

    //  =========================

    Coroutine HistoryAPI;

    //  =========================

    private void OnEnable()
    {
        stateController.OnStateChanged += StateChange;
    }

    private void OnDisable()
    {
        stateController.OnStateChanged -= StateChange;
    }

    private void StateChange(object sender, EventArgs e)
    {
        CallHistoryAPI();
    }

    private void CallHistoryAPI()
    {
        if (stateController.CurrentUserState != userState)
            return;

        if (stateController.CurrentAppState != appState)
        {
            if (HistoryAPI != null)
            {
                StopCoroutine(HistoryAPI);
                HistoryAPI = null;
            }
            paginationIndex = 0;
            totalPages = 0;
            pageTMP.text = (paginationIndex + 1).ToString("n0");
            return;
        }

        noBGLoading.SetActive(true);

        historyListParent.SetActive(false);

        HistoryAPI = StartCoroutine(apiController.GetRequest("/story/viewlistassessments", $"?page={paginationIndex}&limit=10", false, (response) =>
        {
            if (response.ToString() != "")
            {
                StoryHistory tempdata = JsonConvert.DeserializeObject<StoryHistory>(response.ToString());

                totalPages = tempdata.totalpages;

                if (tempdata.history.Length <= 0)
                {
                    for (int a = 0; a < historyItems.Count; a++)
                    {
                        historyItems[a].HistoryData("", "", "NO DATA YET!", 0);
                    }
                }
                else
                {
                    for (int a = 0; a < historyItems.Count; a++)
                    {
                        if (a < tempdata.history.Length)
                            historyItems[a].HistoryData($"{tempdata.history[a].storyid}", $"{tempdata.history[a].createdAt}", $"{tempdata.history[a].title}", tempdata.history[a].score);
                        else
                            historyItems[a].HistoryData("", "", "NO DATA YET!", 0);
                    }
                }

                pageTMP.text = (paginationIndex + 1).ToString("n0");

                CheckPaginationButtons();

                historyListParent.SetActive(true);
            }
        }, null));
    }

    private void CheckPaginationButtons()
    {
        if (paginationIndex <= 0)
        {
            previousBtn.interactable = false;

            if (totalPages - 1 == paginationIndex)
            {
                nextBtn.interactable = false;
            }
            else
            {
                nextBtn.interactable = true;
            }
        }
        else if (paginationIndex > 0 && paginationIndex < totalPages - 1)
        {
            previousBtn.interactable = true;
            nextBtn.interactable = true;
        }
        else if (paginationIndex >= totalPages - 1)
        {
            previousBtn.interactable = true;
            nextBtn.interactable = false;
        }
    }

    public void NextPagination()
    {
        paginationIndex++;

        pageTMP.text = (paginationIndex + 1).ToString("n0");

        noBGLoading.SetActive(true);

        HistoryAPI = StartCoroutine(apiController.GetRequest("/story/viewlistassessments", $"?page={paginationIndex}&limit=10", false, (response) =>
        {
            if (response.ToString() != "")
            {
                StoryHistory tempdata = JsonConvert.DeserializeObject<StoryHistory>(response.ToString());

                totalPages = tempdata.totalpages;

                if (tempdata.history.Length <= 0)
                {
                    for (int a = 0; a < historyItems.Count; a++)
                    {
                        historyItems[a].HistoryData("", "", "NO DATA YET!", 0);
                    }
                }
                else
                {
                    for (int a = 0; a < historyItems.Count; a++)
                    {
                        if (a < tempdata.history.Length)
                            historyItems[a].HistoryData($"{tempdata.history[a].storyid}", $"{tempdata.history[a].createdAt}", $"{tempdata.history[a].title}", tempdata.history[a].score);
                        else
                            historyItems[a].HistoryData("", "", "NO DATA YET!", 0);
                    }
                }

                pageTMP.text = (paginationIndex + 1).ToString("n0");

                CheckPaginationButtons();

                historyListParent.SetActive(true);
            }
        }, null));
    }

    public void PreviousPagination()
    {
        paginationIndex--;

        pageTMP.text = (paginationIndex + 1).ToString("n0");

        noBGLoading.SetActive(true);

        HistoryAPI = StartCoroutine(apiController.GetRequest("/story/viewlistassessments", $"?page={paginationIndex}&limit=10", false, (response) =>
        {
            if (response.ToString() != "")
            {
                StoryHistory tempdata = JsonConvert.DeserializeObject<StoryHistory>(response.ToString());

                totalPages = tempdata.totalpages;

                if (tempdata.history.Length <= 0)
                {
                    for (int a = 0; a < historyItems.Count; a++)
                    {
                        historyItems[a].HistoryData("", "", "NO DATA YET!", 0);
                    }
                }
                else
                {
                    for (int a = 0; a < historyItems.Count; a++)
                    {
                        if (a < tempdata.history.Length)
                            historyItems[a].HistoryData($"{tempdata.history[a].storyid}", $"{tempdata.history[a].createdAt}", $"{tempdata.history[a].title}", tempdata.history[a].score);
                        else
                            historyItems[a].HistoryData("", "", "NO DATA YET!", 0);
                    }
                }

                pageTMP.text = (paginationIndex + 1).ToString("n0");

                CheckPaginationButtons();

                historyListParent.SetActive(true);
            }
        }, null));
    }
}

[System.Serializable]
public class StoryHistory
{
    public StoryHistoryData[] history;
    public int totalpages;
}

[System.Serializable]
public class StoryHistoryData
{
    public string storyid;
    public string title;
    public float score;
    public string createdAt;
}