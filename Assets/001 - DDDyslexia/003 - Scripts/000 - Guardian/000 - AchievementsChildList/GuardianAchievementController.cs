using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GuardianAchievementController : MonoBehaviour
{
    [SerializeField] private APIController apiController;

    [Space]
    [SerializeField] private GameObject noBgLoading;
    [SerializeField] private GameObject childObjPrefab;

    [Space]
    [SerializeField] private GameObject childListObj;
    [SerializeField] private GameObject gameplayObj;

    [Space]
    [SerializeField] private Transform childListParent;

    [Space]
    [SerializeField] private TextMeshProUGUI readitChildScore;
    [SerializeField] private TextMeshProUGUI builditChildScore;
    [SerializeField] private TextMeshProUGUI writeitChildScore;
    [SerializeField] private GameObject readitDifficultyObj;
    [SerializeField] private GameObject readitScoreObj;
    [SerializeField] private GameObject writeitScoreObj;
    [SerializeField] private GameObject writeitDifficultyObj;
    [SerializeField] private GameObject builditScoreObj;
    [SerializeField] private GameObject builditDifficultyObj;

    [Header("DEBUGGER")]
    public string selectedChildren;

    public void InstantiateChildrens()
    {
        noBgLoading.SetActive(true);
        childListParent.gameObject.SetActive(false);
        StartCoroutine(apiController.GetRequest("/children/getchildrenlist", "", false, (data) =>
        {
            List<ChildrenData> tempdata = JsonConvert.DeserializeObject<List<ChildrenData>>(data.ToString());
            StartCoroutine(ChildrentSetData(tempdata));
        }, null));
    }

    IEnumerator ChildrentSetData(List<ChildrenData> tempdata)
    {
        while (childListParent.childCount > 0)
        {
            for (int a = 0; a < childListParent.childCount; a++)
            {
                Destroy(childListParent.GetChild(a).gameObject);
                yield return null;
            }
            yield return null;
        }

        for (int a = 0; a < tempdata.Count; a++)
        {
            GameObject children = Instantiate(childObjPrefab, childListParent);

            children.GetComponent<ChidListItem>().SetData(tempdata[a].id, tempdata[a].fullname, this, () => 
            {
                gameplayObj.SetActive(true);
                childListObj.SetActive(false);
            });

            yield return null;
        }

        childListParent.gameObject.SetActive(true);
        noBgLoading.SetActive(false);
    }

    public void ShowReadItScore(string difficulty)
    {
        noBgLoading.SetActive(true);
        readitChildScore.gameObject.SetActive(false);
        StartCoroutine(apiController.GetRequest($"/readit/getchildscore", $"?childid={selectedChildren}&difficulty={difficulty}", false, (data) =>
        {
            readitChildScore.text = data.ToString();
            readitChildScore.gameObject.SetActive(true);
            readitScoreObj.SetActive(true);
            readitDifficultyObj.SetActive(false);
        }, null));
    }

    public void ShowWriteItScore(string difficulty)
    {
        noBgLoading.SetActive(true);
        writeitChildScore.gameObject.SetActive(false);
        StartCoroutine(apiController.GetRequest($"/writeit/getchildscore", $"?childid={selectedChildren}&difficulty={difficulty}", false, (data) =>
        {
            writeitChildScore.text = data.ToString();
            writeitChildScore.gameObject.SetActive(true);
            writeitScoreObj.SetActive(true);
            writeitDifficultyObj.SetActive(false);
        }, null));
    }

    public void ShowBuildItScore(string difficulty)
    {
        noBgLoading.SetActive(true);
        builditChildScore.gameObject.SetActive(false);
        StartCoroutine(apiController.GetRequest($"/buildit/getchildscore", $"?childid={selectedChildren}&difficulty={difficulty}", false, (data) =>
        {
            builditChildScore.text = data.ToString();
            builditChildScore.gameObject.SetActive(true);
            builditScoreObj.SetActive(true);
            readitDifficultyObj.SetActive(false);
        }, null));
    }
}

[System.Serializable]
public class ChildrenData
{
    public string id;
    public string fullname;
}