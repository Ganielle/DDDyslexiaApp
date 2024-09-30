using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StoryStudentHistoryItem : MonoBehaviour
{
    [SerializeField] private StateController stateController;
    [SerializeField] private StoryUserAssessment storyHistoryItem;
    [SerializeField] private TextMeshProUGUI dateTMP;
    [SerializeField] private TextMeshProUGUI titleTMP;
    [SerializeField] private TextMeshProUGUI scoreTMP;
    [SerializeField] private Button viewBtn;

    [Header("DEBUGGER")]
    [SerializeField] private string historyID;

    public void HistoryData(string id, string date, string title, float score)
    {
        dateTMP.text = date;
        titleTMP.text = title;
        scoreTMP.text = $"{score:n0}%";
        historyID = id;

        if (historyID == "")
            viewBtn.interactable = false;
        else
            viewBtn.interactable = true;
    }

    public void ViewBtn()
    {
        storyHistoryItem.selectedStoryID = historyID;
        stateController.CurrentAppState = StateController.AppStates.USERSTORYHISTORYITEM;
    }
}
