using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class StateController : MonoBehaviour
{
    private event EventHandler StateChanged;
    public event EventHandler OnStateChanged
    {
        add
        {
            if (StateChanged == null || !StateChanged.GetInvocationList().Contains(value))
                StateChanged += value;
        }
        remove { StateChanged -= value; }
    }
    public enum AppStates
    {
        NONE,
        USERDASHBOARD,
        USERSTORY,
        USERSTORYLIST,
        USERSTORYASSESSMENT,
        USERLEADERBOARD,
        USESTORYHISTORY,
        USERSTORYHISTORYITEM,
        ADMINDASHBOARD,
        ADMINSTORYHISTORY,
        ADMINSTORYDATA,
        ADMINUSERMANAGEMENT,
        ADMINUSERVIEW
    }
    public AppStates CurrentAppState
    {
        get => currentAppState;
        set
        {
            if (currentAppState != lastAppState) lastAppState = currentAppState;

            currentAppState = value;
            StateChanged?.Invoke(this, EventArgs.Empty);
        }
    }
    public AppStates LastAppState
    {
        get => lastAppState;
    }

    private event EventHandler UserChanged;
    public event EventHandler OnUserChanged
    {
        add
        {
            if (UserChanged == null || !UserChanged.GetInvocationList().Contains(value))
                UserChanged += value;
        }
        remove { UserChanged -= value; }
    }
    public enum UserState
    {
        NONE,
        STUDENT,
        TEACHER
    }
    public UserState CurrentUserState
    {
        get => currentUserState;
        set
        {
            if (currentUserState != lastUserState) lastUserState = currentUserState;

            currentUserState = value;
            UserChanged?.Invoke(this, EventArgs.Empty);
        }
    }
    public UserState LastUserState
    {
        get => lastUserState;
    }


    [Header("MAIN PANELS")]
    [SerializeField] private GameObject login;
    [SerializeField] private GameObject userPanel;
    [SerializeField] private GameObject adminPanel;

    [Header("USER PANELS")]
    [SerializeField] private GameObject userDashboard;
    [SerializeField] private GameObject userStory;
    [SerializeField] private GameObject userStoryList;
    [SerializeField] private GameObject userStoryAssessment;
    [SerializeField] private GameObject userStoryHistory;
    [SerializeField] private GameObject userStoryHistoryItem;

    [Header("ADMIN PANELS")]
    [SerializeField] private GameObject adminDashboard;
    [SerializeField] private GameObject adminStoryHistory;
    [SerializeField] private GameObject adminStoryData;
    [SerializeField] private GameObject adminUserManagement;
    [SerializeField] private GameObject adminUserView;

    [Header("DEBUGGER")]
    [SerializeField] private AppStates currentAppState;
    [SerializeField] private AppStates lastAppState;
    [SerializeField] private UserState currentUserState;
    [SerializeField] private UserState lastUserState;

    private void OnEnable()
    {
        OnStateChanged += AppStateChanged;
        OnUserChanged += LoginUserChanged;
    }

    private void OnDisable()
    {
        OnStateChanged -= AppStateChanged;
        OnUserChanged -= LoginUserChanged;
    }

    private void LoginUserChanged(object sender, EventArgs e)
    {
        LoggedUserPanelActivators();
    }

    private void AppStateChanged(object sender, EventArgs e)
    {
        PanelActivators();
    }

    private void LoggedUserPanelActivators()
    {
        if (LastUserState == UserState.NONE)
            login.SetActive(false);

        else if (LastUserState == UserState.STUDENT)
            userPanel.SetActive(false);

        else if (LastUserState == UserState.TEACHER)
            adminPanel.SetActive(false);

        if (CurrentUserState == UserState.NONE)
            login.SetActive(true);

        else if (CurrentUserState == UserState.STUDENT)
            userPanel.SetActive(true);

        else if (CurrentUserState == UserState.TEACHER)
            adminPanel.SetActive(true);
    }

    private void PanelActivators()
    {
        #region LAST STATE

        if (LastAppState == AppStates.USERDASHBOARD)
            userDashboard.SetActive(false);

        else if (LastAppState == AppStates.USERSTORY)
            userStory.SetActive(false);

        else if (LastAppState == AppStates.USERSTORYLIST)
            userStoryList.SetActive(false);

        else if (LastAppState == AppStates.USERSTORYASSESSMENT)
            userStoryAssessment.SetActive(false);

        else if (LastAppState == AppStates.USESTORYHISTORY)
            userStoryHistory.SetActive(false);

        else if (LastAppState == AppStates.USERSTORYHISTORYITEM)
            userStoryHistoryItem.SetActive(false);

        else if (LastAppState == AppStates.ADMINDASHBOARD)
            adminDashboard.SetActive(false);

        else if (LastAppState == AppStates.ADMINSTORYHISTORY)
            adminStoryHistory.SetActive(false);

        else if (LastAppState == AppStates.ADMINSTORYDATA)
            adminStoryData.SetActive(false);

        else if (LastAppState == AppStates.ADMINUSERMANAGEMENT)
            adminUserManagement.SetActive(false);

        else if (LastAppState == AppStates.ADMINUSERVIEW)
            adminUserView.SetActive(false);

        #endregion

        #region CURRENT STATE

        if (CurrentAppState == AppStates.USERDASHBOARD)
            userDashboard.SetActive(true);

        else if (CurrentAppState == AppStates.USERSTORY)
            userStory.SetActive(true);

        else if (CurrentAppState == AppStates.USERSTORYLIST)
            userStoryList.SetActive(true);

        else if (CurrentAppState == AppStates.USERSTORYASSESSMENT)
            userStoryAssessment.SetActive(true);

        else if (CurrentAppState == AppStates.USESTORYHISTORY)
            userStoryHistory.SetActive(true);

        else if (CurrentAppState == AppStates.USERSTORYHISTORYITEM)
            userStoryHistoryItem.SetActive(true);

        else if (CurrentAppState == AppStates.ADMINDASHBOARD)
            adminDashboard.SetActive(true);

        else if (CurrentAppState == AppStates.ADMINSTORYHISTORY)
            adminStoryHistory.SetActive(true);

        else if (CurrentAppState == AppStates.ADMINSTORYDATA)
            adminStoryData.SetActive(true);

        else if (CurrentAppState == AppStates.ADMINUSERMANAGEMENT)
            adminUserManagement.SetActive(true);

        else if (CurrentAppState == AppStates.ADMINUSERVIEW)
            adminUserView.SetActive(true);

        #endregion
    }

    public void ChangeAppState(int index)
    {
        CurrentAppState = (AppStates)index;
    }
}
