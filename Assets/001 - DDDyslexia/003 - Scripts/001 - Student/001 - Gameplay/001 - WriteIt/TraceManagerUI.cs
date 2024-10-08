using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TraceManagerUI : MonoBehaviour
{
    [Header("UI and Controllers")]
    [SerializeField] private GameObject loadingNoBG;
    [SerializeField] private APIController apiController;
    [SerializeField] private NotificationController notificationController;
    [SerializeField] private UserData userData;

    [Header("Game Settings")]
    [SerializeField] private float easyTime;
    [SerializeField] private float mediumTime;
    [SerializeField] private float hardTime;

    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI timerTMP;
    [SerializeField] private Button skipBtn;
    [SerializeField] private Button startBtn;
    [SerializeField] private GameObject difficultyMenu;
    [SerializeField] private GameObject writeItGameplay;

    [Header("Cameras")]
    [SerializeField] private Camera uiCamera;
    [SerializeField] private Camera renderTextureCamera;

    [Header("Tracing Settings")]
    [SerializeField] private RawImage rawImage;
    [SerializeField] private RectTransform rawImageRect;
    [SerializeField] private GameObject traceLRPrefab;
    [SerializeField] private List<TraceLetterItemData> traceLetterItemDatas;
    [SerializeField] private float pointRadius = 50f;
    [SerializeField] private float outOfBoundsThreshold = 100f;

    [Header("Debugging")]
    [SerializeField] private Difficulty difficulty;
    [SerializeField] private int currentLetterIndex = 0;
    [SerializeField] private int currentTraceIndex = 0;
    [SerializeField] private Vector3 worldPoint;
    [SerializeField] private bool canDrawLine;
    [SerializeField] private bool startGame;
    [SerializeField] private bool goingBack;
    [SerializeField] private LineRenderer currentLR;
    [SerializeField] private TraceLetterItemData currentLetter;
    [SerializeField] private float currentTime;

    private List<Vector2> drawnPoints = new List<Vector2>();
    private List<LineRenderer> instantiatedLRs = new List<LineRenderer>();

    void Update()
    {
        if (startGame && Input.GetMouseButton(0) && !goingBack)
        {
            Vector2 mousePosition = Input.mousePosition;
            if (RectTransformUtility.RectangleContainsScreenPoint(rawImageRect, mousePosition, uiCamera))
            {
                Vector2 localPoint = ConvertMouseToLocalPoint(mousePosition);
                worldPoint = ConvertLocalToViewportPoint(localPoint);
                ProcessMouseClick();
            }
        }

        CountdownTimer();
    }

    #region Game Control

    private void CountdownTimer()
    {
        if (!startGame) return;

        if (goingBack) return;

        if (currentTime <= 0)
        {
            startGame = false;
            FailedNextAssessment();
            return;
        }

        currentTime -= Time.deltaTime;
        timerTMP.text = FormatTime(currentTime);
    }

    private string FormatTime(float timeToDisplay)
    {
        timeToDisplay = Mathf.Clamp(timeToDisplay, 0, Mathf.Infinity);
        int minutes = Mathf.FloorToInt(timeToDisplay / 60);
        int seconds = Mathf.FloorToInt(timeToDisplay % 60);
        return string.Format("Timer: {0:00} : {1:00}", minutes, seconds);
    }

    public void StartWriting()
    {
        currentTime = GetTimeForDifficulty();
        timerTMP.text = FormatTime(currentTime);
        ToggleGameControls(false);
        startGame = true;
    }

    public void SelectDifficulty(int index)
    {
        difficulty = (Difficulty)index;
        ResetGameState();
        LoadCurrentLetter();
    }

    public void NextAssessment()
    {
        loadingNoBG.SetActive(true);
        goingBack = false;
        startGame = false;
        if (!HasNextLetter())
        {
            Debug.Log("SAVE SCORE");
            StartCoroutine(apiController.PostRequest("/writeit/savescore", "", new Dictionary<string, object>
            {
                { "score", GetScore() },
                { "letter", currentLetter.letter },
                { "difficulty", difficulty == Difficulty.EASY ? "Easy" : difficulty == Difficulty.MEDIUM ? "Medium" : difficulty == Difficulty.HARD ? "Hard" : "None" }
            }, false, (value) =>
            {
                NotifyCompletion("Congratulations! You reached the end of the alphabet assessment.", ResetGame);
            }, () =>
            {

                ResetCurrentGame();
            }));
        }
        else
        {
            Debug.Log("SAVE SCORE");
            StartCoroutine(apiController.PostRequest("/writeit/savescore", "", new Dictionary<string, object>
            {
                { "score", GetScore() },
                { "letter", currentLetter.letter },
                { "difficulty", difficulty == Difficulty.EASY ? "Easy" : difficulty == Difficulty.MEDIUM ? "Medium" : difficulty == Difficulty.HARD ? "Hard" : "None" }
            }, false, (value) =>
            {
                NotifyCompletion("You passed the test. Click OK for the next test.", () =>
                {
                    currentLetterIndex++;
                    LoadCurrentLetter();
                });
            }, () =>
            {
                ResetCurrentGame();
            }));
        }
    }

    public void FailedNextAssessment()
    {
        loadingNoBG.SetActive(true);
        goingBack = false;
        startGame = false;
        if (!HasNextLetter())
        {
            Debug.Log("SAVE SCORE");
            StartCoroutine(apiController.PostRequest("/writeit/savescore", "", new Dictionary<string, object>
            {
                { "score", GetScore() },
                { "letter", currentLetter.letter },
                { "difficulty", difficulty == Difficulty.EASY ? "Easy" : difficulty == Difficulty.MEDIUM ? "Medium" : difficulty == Difficulty.HARD ? "Hard" : "None" }
            }, false, (value) =>
            {
                NotifyCompletion("Congratulations! You reached the end of the alphabet assessment.", ResetGame);
            }, () =>
            {

                ResetCurrentGame();
            }));
        }
        else
        {
            Debug.Log("SAVE SCORE");
            StartCoroutine(apiController.PostRequest("/writeit/savescore", "", new Dictionary<string, object>
            {
                { "score", GetScore() },
                { "letter", currentLetter.letter },
                { "difficulty", difficulty == Difficulty.EASY ? "Easy" : difficulty == Difficulty.MEDIUM ? "Medium" : difficulty == Difficulty.HARD ? "Hard" : "None" }
            }, false, (value) =>
            {
                NotifyCompletion("You failed the test! Get ready for the next test.", () =>
                {
                    currentLetterIndex++;
                    LoadCurrentLetter();
                });
            }, () =>
            {
                ResetCurrentGame();
            }));
        }
    }

    public void Skip()
    {
        loadingNoBG.SetActive(true);
        goingBack = false;
        startGame = false;
        if (!HasNextLetter())
        {
            Debug.Log("SAVE SCORE");
            StartCoroutine(apiController.PostRequest("/writeit/savescore", "", new Dictionary<string, object>
            {
                { "score", "0" },
                { "letter", currentLetter.letter },
                { "difficulty", difficulty == Difficulty.EASY ? "Easy" : difficulty == Difficulty.MEDIUM ? "Medium" : difficulty == Difficulty.HARD ? "Hard" : "None" }
            }, false, (value) =>
            {
                NotifyCompletion("No more letters left. Returning to the menu.", ResetGame);
            }, () =>
            {
                ResetCurrentGame();
            }));
        }
        else
        {
            Debug.Log("SAVE SCORE");
            StartCoroutine(apiController.PostRequest("/writeit/savescore", "", new Dictionary<string, object>
            {
                { "score", "0" },
                { "letter", currentLetter.letter },
                { "difficulty", difficulty == Difficulty.EASY ? "Easy" : difficulty == Difficulty.MEDIUM ? "Medium" : difficulty == Difficulty.HARD ? "Hard" : "None" }
            }, false, (value) =>
            {
                currentLetterIndex++;
                LoadCurrentLetter();
            }, () =>
            {
                ResetCurrentGame();
            }));
        }
    }

    public void GoBack()
    {
        goingBack = true;
        notificationController.ShowConfirmation("Are you sure you want to stop the assessment?", ResetGame, () => goingBack = false);
    }

    private void ResetGame()
    {
        ResetAllLineRenderers();
        ToggleGameState(false);
        goingBack = false;
        startGame = false;
        currentLetter = null;
        timerTMP.text = UpdateTimerDisplay(GetTimeForDifficulty());
        startBtn.interactable = true;
        skipBtn.interactable = true;
    }

    private void ResetGameState()
    {
        // Reset LineRenderers and trace points
        ResetAllLineRenderers();

        // Disable gameplay UI and show the difficulty selection menu
        difficultyMenu.SetActive(true);
        writeItGameplay.SetActive(false);

        // Reset flags and states
        goingBack = false;
        startGame = false;

        // Destroy the current letter's GameObject if it exists
        if (currentLetter != null)
        {
            Destroy(currentLetter.gameObject);
            currentLetter = null;
        }

        // Reset timer and buttons
        timerTMP.text = UpdateTimerDisplay(0);
        startBtn.interactable = true;
        skipBtn.interactable = true;

        // Reset trace indices
        currentLetterIndex = 0;
        currentTraceIndex = 0;
    }

    private void ResetCurrentGame()
    {
        goingBack = false;
        startGame = false;

        // Reset LineRenderers and trace points
        ResetAllLineRenderers();
        LoadCurrentLetter();

        timerTMP.text = UpdateTimerDisplay(GetTimeForDifficulty());
        startBtn.interactable = true;
        skipBtn.interactable = true;
    }

    private void LoadCurrentLetter()
    {
        Destroy(currentLetter?.gameObject);
        currentLetter = Instantiate(traceLetterItemDatas[currentLetterIndex].gameObject).GetComponent<TraceLetterItemData>();
        ResetAllLineRenderers();
        timerTMP.text = UpdateTimerDisplay(GetTimeForDifficulty());
        startBtn.interactable = true;
        skipBtn.interactable = true;
    }

    private void ToggleGameState(bool isActive)
    {
        difficultyMenu.SetActive(!isActive);
        writeItGameplay.SetActive(isActive);
    }

    private void ToggleGameControls(bool isInteractable)
    {
        startBtn.interactable = isInteractable;
        skipBtn.interactable = isInteractable;
    }

    private bool HasNextLetter() => currentLetterIndex < traceLetterItemDatas.Count - 1;

    private float GetTimeForDifficulty() =>
        difficulty switch
        {
            Difficulty.EASY => easyTime,
            Difficulty.MEDIUM => mediumTime,
            _ => hardTime,
        };

    private void NotifyCompletion(string message, System.Action callback)
    {
        notificationController.ShowError(message, callback);
    }

    #endregion

    #region Tracing Functionality

    private void ProcessMouseClick()
    {
        var hit = Physics2D.Raycast(worldPoint, Vector2.zero);

        if (IsOutOfBounds(hit))
        {
            ResetAllLineRenderers();
            return;
        }

        if (!canDrawLine && hit.collider?.gameObject == currentLetter.startingTracePoints[currentTraceIndex].gameObject)
        {
            StartLineRenderer();
        }

        if (canDrawLine)
        {
            UpdateDrawing();
            CheckForTraceCompletion();
        }
    }

    private bool IsOutOfBounds(RaycastHit2D hit) =>
        hit.collider != null && hit.collider.CompareTag("WriteItOutOfBounds");

    private void StartLineRenderer()
    {
        currentLR = Instantiate(traceLRPrefab).GetComponent<LineRenderer>();
        instantiatedLRs.Add(currentLR);
        canDrawLine = true;
    }

    private void UpdateDrawing()
    {
        drawnPoints.Add(worldPoint);
        currentLR.positionCount = drawnPoints.Count;
        currentLR.SetPositions(ConvertToLineRendererPositions(drawnPoints));
    }

    private void CheckForTraceCompletion()
    {
        if (Vector2.Distance(worldPoint, traceLetterItemDatas[currentLetterIndex].endTracePoints[currentTraceIndex].position) < pointRadius)
        {
            if (currentTraceIndex < traceLetterItemDatas[currentLetterIndex].startingTracePoints.Length - 1)
            {
                currentTraceIndex++;
                ResetLineRenderer();
            }
            else
            {
                FinishTracing();
            }
        }
    }

    private void FinishTracing()
    {
        NextAssessment();
        ResetAllLineRenderers();
    }

    private void ResetLineRenderer()
    {
        currentLR = null;
        canDrawLine = false;
        drawnPoints.Clear();
    }

    private void ResetAllLineRenderers()
    {
        foreach (var lineRenderer in instantiatedLRs)
        {
            if (lineRenderer != null)
                Destroy(lineRenderer.gameObject);
        }

        instantiatedLRs.Clear();
        currentTraceIndex = 0;
        canDrawLine = false;
        drawnPoints.Clear();
    }

    #endregion

    #region Utility

    private Vector2 ConvertMouseToLocalPoint(Vector2 mousePosition)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(rawImageRect, mousePosition, uiCamera, out Vector2 localPoint);
        return localPoint;
    }

    private Vector3 ConvertLocalToViewportPoint(Vector2 localPoint)
    {
        Vector2 viewportClick = localPoint - rawImageRect.rect.min;
        viewportClick.y = (rawImageRect.rect.yMin * -1) - (localPoint.y * -1);
        viewportClick.x *= rawImage.uvRect.width / rawImageRect.rect.width;
        viewportClick.y *= rawImage.uvRect.height / rawImageRect.rect.height;
        viewportClick += rawImage.uvRect.min;

        return renderTextureCamera.ViewportToWorldPoint(viewportClick);
    }

    private Vector3[] ConvertToLineRendererPositions(List<Vector2> points)
    {
        Vector3[] worldPoints = new Vector3[points.Count];

        for (int i = 0; i < points.Count; i++)
        {
            // Convert 2D points to 3D by fixing Z to 0
            worldPoints[i] = new Vector3(points[i].x, points[i].y, 0);
        }

        return worldPoints;
    }

    private string UpdateTimerDisplay(float timeInSeconds)
    {
        // Calculate minutes and seconds
        int minutes = Mathf.FloorToInt(timeInSeconds / 60);
        int seconds = Mathf.FloorToInt(timeInSeconds % 60);

        // Format the time as MM:SS
        return string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    private string GetScore()
    {
        if (difficulty == Difficulty.EASY)
        {
            if (currentTime > 15) return "100";
            else if (currentTime > 5 && currentTime <= 15) return "50";
            else if (currentTime > 0 && currentTime <= 5) return "25";
            else return "0";
        }
        else if (difficulty == Difficulty.MEDIUM)
        {
            if (currentTime > 10) return "100";
            else if (currentTime > 5 && currentTime <= 10) return "50";
            else if (currentTime > 0 && currentTime <= 5) return "25";
            else return "0";
        }
        else if (difficulty == Difficulty.HARD)
        {
            if (currentTime > 2) return "100";
            else if (currentTime > 0 && currentTime <= 2) return "25";
            else return "0";
        }

        return "0";
    }

    #endregion
}