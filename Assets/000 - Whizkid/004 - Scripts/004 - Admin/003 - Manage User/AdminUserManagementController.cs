using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AdminUserManagementController : MonoBehaviour
{
    [SerializeField] private GameObject noBGLoading;
    [SerializeField] private StateController stateController;
    [SerializeField] private StateController.AppStates appState;
    [SerializeField] private StateController.UserState userState;
    [SerializeField] private APIController apiController;

    [Space]
    [SerializeField] private GameObject studentItemListParent;

    [Header("SEARCH")]
    [SerializeField] private TMP_InputField fullnameTMP;

    [Header("PAGINATION")]
    [SerializeField] private TMP_InputField pageTMP;
    [SerializeField] private Button previousBtn;
    [SerializeField] private Button nextBtn;

    [Header("USER LIST")]
    [SerializeField] private List<UserManagementItem> studentList;

    [Header("DEBUGGER")]
    [SerializeField] private int paginationIndex;
    [SerializeField] private int totalPages;

    //  ===============================

    Coroutine HistoryAPI;

    //  ===============================

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
        ShowUserListAtStart();
    }

    private void ShowUserListAtStart()
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

        studentItemListParent.SetActive(false);

        HistoryAPI = StartCoroutine(apiController.GetRequest("/users/getstudentlist", $"?fullname={fullnameTMP.text}&page={paginationIndex}&limit=10", false, (response) => 
        {
            if (response.ToString() != "")
            {
                StudentList tempdata = JsonConvert.DeserializeObject<StudentList>(response.ToString());

                totalPages = tempdata.totalpages;

                if (tempdata.list.Length <= 0)
                {
                    for (int a = 0; a < studentList.Count; a++)
                    {
                        studentList[a].InitializeData("", "", "");
                    }
                }
                else
                {
                    for (int a = 0; a < studentList.Count; a++)
                    {
                        if (a < tempdata.list.Length)
                            studentList[a].InitializeData($"{tempdata.list[a].firstname}", $"{tempdata.list[a].lastname}", $"{tempdata.list[a].userid}");
                        else
                            studentList[a].InitializeData("", "", "");
                    }
                }

                pageTMP.text = (paginationIndex + 1).ToString("n0");

                CheckPaginationButtons();

                studentItemListParent.SetActive(true);
            }
        }, null));
    }

    public void SearchUser()
    {
        if (HistoryAPI != null)
        {
            StopCoroutine(HistoryAPI);
            HistoryAPI = null;
        }

        paginationIndex = 0;

        noBGLoading.SetActive(true);

        studentItemListParent.SetActive(false);

        HistoryAPI = StartCoroutine(apiController.GetRequest("/users/getstudentlist", $"?fullname={fullnameTMP.text}&page={paginationIndex}&limit=10", false, (response) =>
        {
            if (response.ToString() != "")
            {
                StudentList tempdata = JsonConvert.DeserializeObject<StudentList>(response.ToString());

                totalPages = tempdata.totalpages;

                if (tempdata.list.Length <= 0)
                {
                    for (int a = 0; a < studentList.Count; a++)
                    {
                        studentList[a].InitializeData("", "", "");
                    }
                }
                else
                {
                    for (int a = 0; a < studentList.Count; a++)
                    {
                        if (a < tempdata.list.Length)
                            studentList[a].InitializeData($"{tempdata.list[a].firstname}", $"{tempdata.list[a].lastname}", $"{tempdata.list[a].userid}");
                        else
                            studentList[a].InitializeData("", "", "");
                    }
                }

                pageTMP.text = (paginationIndex + 1).ToString("n0");

                CheckPaginationButtons();

                studentItemListParent.SetActive(true);
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

        HistoryAPI = StartCoroutine(apiController.GetRequest("/users/getstudentlist", $"?fullname={fullnameTMP.text}&page={paginationIndex}&limit=10", false, (response) =>
        {
            if (response.ToString() != "")
            {
                StudentList tempdata = JsonConvert.DeserializeObject<StudentList>(response.ToString());

                totalPages = tempdata.totalpages;

                if (tempdata.list.Length <= 0)
                {
                    for (int a = 0; a < studentList.Count; a++)
                    {
                        studentList[a].InitializeData("", "", "");
                    }
                }
                else
                {
                    for (int a = 0; a < studentList.Count; a++)
                    {
                        if (a < tempdata.list.Length)
                            studentList[a].InitializeData($"{tempdata.list[a].firstname}", $"{tempdata.list[a].lastname}", $"{tempdata.list[a].userid}");
                        else
                            studentList[a].InitializeData("", "", "");
                    }
                }

                pageTMP.text = (paginationIndex + 1).ToString("n0");

                CheckPaginationButtons();

                studentItemListParent.SetActive(true);
            }
        }, null));
    }

    public void PreviousPagination()
    {
        paginationIndex--;

        pageTMP.text = (paginationIndex + 1).ToString("n0");

        noBGLoading.SetActive(true);

        HistoryAPI = StartCoroutine(apiController.GetRequest("/users/getstudentlist", $"?fullname={fullnameTMP.text}&page={paginationIndex}&limit=10", false, (response) =>
        {
            if (response.ToString() != "")
            {
                StudentList tempdata = JsonConvert.DeserializeObject<StudentList>(response.ToString());

                totalPages = tempdata.totalpages;

                if (tempdata.list.Length <= 0)
                {
                    for (int a = 0; a < studentList.Count; a++)
                    {
                        studentList[a].InitializeData("", "", "");
                    }
                }
                else
                {
                    for (int a = 0; a < studentList.Count; a++)
                    {
                        if (a < tempdata.list.Length)
                            studentList[a].InitializeData($"{tempdata.list[a].firstname}", $"{tempdata.list[a].lastname}", $"{tempdata.list[a].userid}");
                        else
                            studentList[a].InitializeData("", "", "");
                    }
                }

                pageTMP.text = (paginationIndex + 1).ToString("n0");

                CheckPaginationButtons();

                studentItemListParent.SetActive(true);
            }
        }, null));
    }
}

[System.Serializable]
public class StudentList
{
    public StudentListData[] list;
    public int totalpages;
}

[System.Serializable]
public class StudentListData
{
    public string userid;
    public string firstname;
    public string lastname;
}
