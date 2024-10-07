using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TraceManagerUI : MonoBehaviour
{
    [SerializeField] private GameObject loadingNoBG;
    [SerializeField] private APIController apiController;
    [SerializeField] private NotificationController notificationController;
    [SerializeField] private UserData userData;

    [Space]
    [SerializeField] private float easyTime;
    [SerializeField] private float mediumTime;
    [SerializeField] private float hardTime;

    [Space]
    [SerializeField] private TextMeshProUGUI timerTMP;
    [SerializeField] private Button skipBtn;
    [SerializeField] private Button startBtn;

    [Space]
    [SerializeField] private GameObject difficultyMenu;
    [SerializeField] private GameObject writeItGameplay;

    [Header("Cameras")]
    public Camera uiCamera;                    // Main perspective camera
    public Camera renderTextureCamera;

    [Header("UI Elements")]
    public RawImage rawImage;                  // The UI component showing the Render Texture
    public RectTransform rawImageRect;         // RectTransform of the Raw Image
    public GameObject traceLRPrefab;           // Prefab for the LineRenderer

    [Header("Tracing Settings")]
    public List<TraceLetterItemData> traceLetterItemDatas;
    public float pointRadius = 50f;            // Radius within which mouse input is valid
    public float outOfBoundsThreshold = 100f;  // Distance threshold to determine out-of-bounds

    [Header("DEBUGGER")]
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

    private List<Vector2> drawnPoints = new List<Vector2>();  // Points that user has traced
    private List<LineRenderer> instantiatedLRs = new List<LineRenderer>();  // Track instantiated LineRenderers

    void Update()
    {
        CountdownTimer();

        if (Input.GetMouseButton(0)) // Left mouse button is pressed
        {
            if (!startGame) return;
            else if (goingBack) return;

            Vector2 mousePosition = Input.mousePosition;

            // Check if the mouse is within the bounds of the Raw Image
            if (RectTransformUtility.RectangleContainsScreenPoint(rawImageRect, mousePosition, uiCamera))
            {
                // Convert mouse input to world space and check collisions
                Vector2 localPoint = ConvertMouseToLocalPoint(mousePosition);
                worldPoint = ConvertLocalToViewportPoint(localPoint);

                

                ProcessMouseClick();
            }
        }
    }

    #region GAME CONTROL PANEL

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
        timerTMP.text = UpdateTimerDisplay(currentTime);
    }

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

    public void StartWriting()
    {
        if (difficulty == Difficulty.EASY) currentTime = easyTime;
        else if (difficulty == Difficulty.MEDIUM) currentTime = mediumTime;
        else currentTime = easyTime;
        timerTMP.text = UpdateTimerDisplay(currentTime);

        startBtn.interactable = false;
        skipBtn.interactable = false;
        startGame = true;
    }

    public void SelectDifficulty(int index)
    {
        difficulty = (Difficulty)index;

        if (difficulty == Difficulty.EASY) currentTime = easyTime;
        else if (difficulty == Difficulty.MEDIUM) currentTime = mediumTime;
        else currentTime = easyTime;
        timerTMP.text = UpdateTimerDisplay(currentTime);

        startBtn.interactable = true;
        skipBtn.interactable = true;
        currentLetterIndex = 0;
        currentTraceIndex = 0;
        startGame = false;
        currentLetter = Instantiate(traceLetterItemDatas[currentLetterIndex].gameObject).GetComponent<TraceLetterItemData>();
        ResetAllLineRenderers();
    }

    public void NextAssessment()
    {
        if (currentLetterIndex >= traceLetterItemDatas.Count - 1)
        {
            notificationController.ShowError("Congratulations! You reached the end of the alphabet assessment.", () =>
            {
                ResetGame();
            });
            return;
        }

        //  DELETE THIS SHIT
        notificationController.ShowError("Congratulations! You passed the test. Click ok for the next test.", () =>
        {
            startBtn.interactable = true;
            skipBtn.interactable = true;
        });

        currentLetterIndex++;
        Destroy(currentLetter.gameObject);
        currentLetter = Instantiate(traceLetterItemDatas[currentLetterIndex].gameObject).GetComponent<TraceLetterItemData>();
    }

    public void FailedNextAssessment()
    {
        if (currentLetterIndex >= traceLetterItemDatas.Count - 1)
        {
            notificationController.ShowError("Congratulations! You reached the end of the alphabet assessment.", () =>
            {
                ResetGame();
            });
            return;
        }

        //  DELETE THIS SHIT
        notificationController.ShowError("You failed the test! Get ready for the next test.", () =>
        {
            startBtn.interactable = true;
            skipBtn.interactable = true;
        });

        currentLetterIndex++;
        Destroy(currentLetter.gameObject);
        currentLetter = Instantiate(traceLetterItemDatas[currentLetterIndex].gameObject).GetComponent<TraceLetterItemData>();
    }

    public void Skip()
    {
        if (currentLetterIndex >= traceLetterItemDatas.Count - 1)
        {
            notificationController.ShowError("Are you sure you want to skip this alphabet? There's no next alphabet left, if you skip this you would return to menu.", () =>
            {
                ResetGame();
            });
            return;
        }
        currentLetterIndex++;
        Destroy(currentLetter.gameObject);
        currentLetter = Instantiate(traceLetterItemDatas[currentLetterIndex].gameObject).GetComponent<TraceLetterItemData>();
        ResetAllLineRenderers();
    }
    public void GoBack()
    {
        goingBack = true;
        notificationController.ShowConfirmation("Are you sure you want to stop the assessment?", () =>
        {
            ResetGame();
        }, () => goingBack = false);
    }

    private void ResetGame()
    {
        ResetAllLineRenderers();
        difficultyMenu.SetActive(true);
        writeItGameplay.SetActive(false);
        goingBack = false;
        startGame = false;
        Destroy(currentLetter.gameObject);
        currentLetter = null;
    }

    #endregion

    #region TRACE FUNCTIONALITY

    // Check if the point goes out of bounds (based on a threshold distance)
    private bool IsOutOfBounds(RaycastHit2D point)
    {
        if (point.collider == null) return false;

        if (!canDrawLine) return false;

        if (!point.collider.CompareTag("WriteItOutOfBounds")) return false;  // Bounds check invalid if no more traces

        return true;
    }

    // Process mouse click, check hit, and update line drawing
    private void ProcessMouseClick()
    {
        var hit = Physics2D.Raycast(worldPoint, Vector2.zero);

        // Check if the drawing is out of bounds
        if (IsOutOfBounds(hit))
        {
            ResetAllLineRenderers();   // Reset all lines and trace index
            return;
        }

        if (canDrawLine)
        {
            UpdateDrawing();
        }

        Debug.Log(hit.collider.gameObject.name);

        if (hit.collider == null || !hit.collider.CompareTag("WriteIt"))
            return;

        // Handle start of tracing
        HandleTraceStart(hit);

        // Handle drawing continuation
        if (canDrawLine)
        {
            UpdateDrawing();
            CheckForTraceCompletion();
        }
    }

    // Handles starting a new trace if the first point is hit
    private void HandleTraceStart(RaycastHit2D hit)
    {
        Debug.Log(hit.collider.gameObject == currentLetter.startingTracePoints[currentTraceIndex].gameObject);
        if (!canDrawLine && hit.collider.gameObject == currentLetter.startingTracePoints[currentTraceIndex].gameObject)
        {
            currentLR = Instantiate(traceLRPrefab).GetComponent<LineRenderer>();
            instantiatedLRs.Add(currentLR);  // Track the instantiated LineRenderer
            canDrawLine = true;
        }
    }

    // Updates the LineRenderer by adding points
    private void UpdateDrawing()
    {
        drawnPoints.Add(worldPoint);
        currentLR.positionCount = drawnPoints.Count;
        currentLR.SetPositions(ConvertToLineRendererPositions(drawnPoints));
    }

    // Checks if the current trace is complete and moves to the next trace point
    private void CheckForTraceCompletion()
    {
        if (Vector2.Distance(worldPoint, traceLetterItemDatas[currentLetterIndex].endTracePoints[currentTraceIndex].position) < pointRadius)
        {
            if (currentTraceIndex < traceLetterItemDatas[currentLetterIndex].startingTracePoints.Length - 1)
            {
                Debug.Log("Trace Completed!");
                ResetLineRenderer();
                currentTraceIndex++;
            }
            else
            {
                // All traces completed
                FinishTracing();
            }
        }
    }

    // Resets the current line and drawing state for the next trace point
    private void ResetLineRenderer()
    {
        currentLR = null;
        canDrawLine = false;
        drawnPoints.Clear();
    }

    // Finishes the tracing sequence
    private void FinishTracing()
    {
        Debug.Log("All traces completed!");

        //  PUT THIS IN API CALL SUCCESSFUL CALL
        NextAssessment();
        ResetAllLineRenderers();
    }

    // Reset all instantiated LineRenderers and reset the trace index
    private void ResetAllLineRenderers()
    {
        foreach (var lineRenderer in instantiatedLRs)
        {
            if (lineRenderer != null)
                Destroy(lineRenderer.gameObject);  // Destroy each LineRenderer object
        }


        instantiatedLRs.Clear();  // Clear the list of LineRenderers
        currentTraceIndex = 0;    // Reset the trace index
        canDrawLine = false;      // Stop drawing
        drawnPoints.Clear();      // Clear drawn points
        Debug.Log("Out of bounds! Resetting traces.");
    }

    // Convert mouse position to local point on the Raw Image
    private Vector2 ConvertMouseToLocalPoint(Vector2 mousePosition)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(rawImageRect, mousePosition, uiCamera, out Vector2 localPoint);
        return localPoint;
    }

    // Convert the local point to world space via viewport
    private Vector3 ConvertLocalToViewportPoint(Vector2 localPoint)
    {
        Vector2 viewportClick = localPoint - rawImageRect.rect.min;
        viewportClick.y = (rawImageRect.rect.yMin * -1) - (localPoint.y * -1);
        viewportClick.x *= rawImage.uvRect.width / rawImageRect.rect.width;
        viewportClick.y *= rawImage.uvRect.height / rawImageRect.rect.height;
        viewportClick += rawImage.uvRect.min;

        return renderTextureCamera.ViewportToWorldPoint(viewportClick);
    }

    // Convert traced points to world positions for LineRenderer
    private Vector3[] ConvertToLineRendererPositions(List<Vector2> points)
    {
        Vector3[] worldPoints = new Vector3[points.Count];
        for (int i = 0; i < points.Count; i++)
        {
            worldPoints[i] = new Vector3(points[i].x, points[i].y, 0);  // Z axis fixed to 0
        }
        return worldPoints;
    }

    // Draw gizmos for debugging
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(worldPoint, Vector3.forward * 10f);
    }

    #endregion
}
