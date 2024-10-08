using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BuildItController : MonoBehaviour
{
    [Header("Game Settings")]
    [SerializeField] private GameObject loadingNoBG;
    [SerializeField] private APIController apiController;
    [SerializeField] private NotificationController notificationController;
    [SerializeField] private UserData userData;

    [SerializeField] private float easyTime, mediumTime, hardTime;
    [SerializeField] private List<BuildItLetterData> letterData;

    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI timerTMP;
    [SerializeField] private Button skipBtn, startBtn;
    [SerializeField] private GameObject difficultyMenu, buildItGameplay;

    [Header("Cameras")]
    public Camera uiCamera, renderTextureCamera;

    [Header("Render Texture UI")]
    public RawImage rawImage;
    public RectTransform rawImageRect;

    [Header("DEBUGGER")]
    [SerializeField] private Difficulty difficulty;
    [SerializeField] private int currentLetterIndex;
    [SerializeField] private bool startGame, goingBack;
    [SerializeField] private float currentTime;
    [SerializeField] private BuildItLetterData currentLetter;
    [SerializeField] private List<bool> rightPositions;

    private GameObject selectedPiece;
    private bool isDragging;
    private const float snapDistance = 0.1f;

    private void Update()
    {
        HandleMouseInput();
        CountdownTimer();
    }

    #region Game Control Methods

    private void CountdownTimer()
    {
        if (!startGame || goingBack) return;

        currentTime -= Time.deltaTime;
        timerTMP.text = FormatTime(currentTime);

        if (currentTime <= 0)
        {
            startGame = false;
            FailedNextAssessment();
        }
    }

    private string FormatTime(float time)
    {
        time = Mathf.Clamp(time, 0, Mathf.Infinity);
        int minutes = Mathf.FloorToInt(time / 60);
        int seconds = Mathf.FloorToInt(time % 60);
        return $"Timer: {minutes:00} : {seconds:00}";
    }

    public void StartBuilding()
    {
        currentTime = GetTimeByDifficulty();
        timerTMP.text = FormatTime(currentTime);

        ResetGameState();
        startBtn.interactable = false;
        skipBtn.interactable = false;
        startGame = true;
    }

    public void SelectDifficulty(int index)
    {
        difficulty = (Difficulty)index;
        currentTime = GetTimeByDifficulty();
        timerTMP.text = FormatTime(currentTime);

        ResetGameState();
        PrepareNextLetter();
        difficultyMenu.SetActive(false);
        buildItGameplay.SetActive(true);
    }

    private float GetTimeByDifficulty()
    {
        return difficulty switch
        {
            Difficulty.EASY => easyTime,
            Difficulty.MEDIUM => mediumTime,
            _ => hardTime,
        };
    }

    public void NextAssessment()
    {
        loadingNoBG.SetActive(true);
        StartNextLetter(() =>
        {
            StartCoroutine(apiController.PostRequest("/buildit/savescore", "", new Dictionary<string, object>
            {
                { "score", GetScore() },
                { "letter", currentLetter.letter },
                { "difficulty", difficulty == Difficulty.EASY ? "Easy" : difficulty == Difficulty.MEDIUM ? "Medium" : difficulty == Difficulty.HARD ? "Hard" : "None" }
            }, false, (value) =>
            {
                notificationController.ShowError("Congratulations! You passed the test. Click ok for the next test.", () =>
                {
                    startBtn.interactable = true;
                    skipBtn.interactable = true;
                });
            }, () =>
            {

            }));
        });
    }

    public void FailedNextAssessment()
    {
        loadingNoBG.SetActive(true);
        StartNextLetter(() =>
        {
            StartCoroutine(apiController.PostRequest("/buildit/savescore", "", new Dictionary<string, object>
            {
                { "score", "0" },
                { "letter", currentLetter.letter },
                { "difficulty", difficulty == Difficulty.EASY ? "Easy" : difficulty == Difficulty.MEDIUM ? "Medium" : difficulty == Difficulty.HARD ? "Hard" : "None" }
            }, false, (value) =>
            {
                notificationController.ShowError("You failed the test! Get ready for the next test.", () =>
                {
                    startBtn.interactable = true;
                    skipBtn.interactable = true;
                });
            }, () =>
            {

            }));
        });
    }

    private void StartNextLetter(System.Action onCompletion)
    {
        startGame = false;
        goingBack = false;

        if (currentLetterIndex >= letterData.Count - 1)
        {
            StartCoroutine(apiController.PostRequest("/buildit/savescore", "", new Dictionary<string, object>
            {
                { "score", GetScore() },
                { "letter", currentLetter.letter },
                { "difficulty", difficulty == Difficulty.EASY ? "Easy" : difficulty == Difficulty.MEDIUM ? "Medium" : difficulty == Difficulty.HARD ? "Hard" : "None" }
            }, false, (value) =>
            {
                notificationController.ShowError("Congratulations! You reached the end of the alphabet assessment.", DoneGame);
            }, () =>
            {

            }));
            return;
        }

        onCompletion.Invoke();
        currentLetterIndex++;
        ResetGameState();
        PrepareNextLetter();
    }

    private void PrepareNextLetter()
    {
        Destroy(currentLetter?.gameObject);
        currentLetter = Instantiate(letterData[currentLetterIndex].gameObject).GetComponent<BuildItLetterData>();
    }

    public void Skip()
    {
        if (currentLetterIndex >= letterData.Count - 1)
        {
            notificationController.ShowConfirmation(
                "Are you sure you want to skip this alphabet? There's no next alphabet left, if you skip this you would return to menu.",
                () =>
                {
                    loadingNoBG.SetActive(true);
                    StartCoroutine(apiController.PostRequest("/buildit/savescore", "", new Dictionary<string, object>
                    {
                        { "score", "0" },
                        { "letter", currentLetter.letter },
                        { "difficulty", difficulty == Difficulty.EASY ? "Easy" : difficulty == Difficulty.MEDIUM ? "Medium" : difficulty == Difficulty.HARD ? "Hard" : "None" }
                    }, false, (value) =>
                    {
                        DoneGame();
                    }, () =>
                    {

                    }));
                }, null);
        }
        else
        {
            loadingNoBG.SetActive(true);
            StartCoroutine(apiController.PostRequest("/buildit/savescore", "", new Dictionary<string, object>
            {
                { "score", "0" },
                { "letter", currentLetter.letter },
                { "difficulty", difficulty == Difficulty.EASY ? "Easy" : difficulty == Difficulty.MEDIUM ? "Medium" : difficulty == Difficulty.HARD ? "Hard" : "None" }
            }, false, (value) =>
            {
                currentLetterIndex++;
                PrepareNextLetter();
                ResetGameState();
            }, () =>
            {

            }));
        }
    }

    public void GoBack()
    {
        goingBack = true;
        notificationController.ShowConfirmation("Are you sure you want to stop the assessment?", DoneGame, () => goingBack = false);
    }

    private void DoneGame()
    {
        ResetGameState();
        difficultyMenu.SetActive(true);
        buildItGameplay.SetActive(false);
    }

    private void ResetGameState()
    {
        rightPositions.Clear();
        selectedPiece = null;
        isDragging = false;
        startGame = false;
    }

    #endregion

    #region Input Handling

    private void HandleMouseInput()
    {
        if (Input.GetMouseButton(0))
        {
            if (!startGame) return;

            if (goingBack) return;

            Vector2 mousePosition = Input.mousePosition;

            if (RectTransformUtility.RectangleContainsScreenPoint(rawImageRect, mousePosition, uiCamera))
            {
                Vector2 localPoint = ConvertMouseToLocalPoint(mousePosition);
                Vector3 worldPoint = ConvertLocalToViewportPoint(localPoint);

                if (!isDragging)
                    ProcessMouseClick(worldPoint);
                else
                    DragSelectedPiece(worldPoint);
            }
        }
        else if (isDragging && Input.GetMouseButtonUp(0))
        {
            DropPiece();
        }
    }

    private void ProcessMouseClick(Vector3 worldPoint)
    {
        var hit = Physics2D.Raycast(worldPoint, Vector2.zero);
        if (hit.collider != null && hit.collider.CompareTag("BuildItLetter"))
        {
            selectedPiece = hit.collider.gameObject;
            isDragging = true;
        }
    }

    private void DragSelectedPiece(Vector3 worldPoint)
    {
        if (selectedPiece != null)
        {
            selectedPiece.transform.position = new Vector3(worldPoint.x, worldPoint.y, selectedPiece.transform.position.z);
        }
    }

    private void DropPiece()
    {
        if (selectedPiece == null) return;

        int pieceIndex = System.Array.IndexOf(currentLetter.puzzlePieces, selectedPiece);
        if (pieceIndex >= 0 && pieceIndex < currentLetter.snapZones.Length)
        {
            GameObject snapZone = currentLetter.snapZones[pieceIndex];
            if (Vector3.Distance(selectedPiece.transform.position, snapZone.transform.position) <= snapDistance)
            {
                SnapToZone(selectedPiece, snapZone);
            }
        }

        isDragging = false;
        selectedPiece = null;
    }

    private void SnapToZone(GameObject piece, GameObject snapZone)
    {
        piece.transform.position = snapZone.transform.position;
        piece.GetComponent<Collider2D>().enabled = false;
        rightPositions.Add(true);

        if (rightPositions.Count >= currentLetter.snapZones.Length)
            NextAssessment();
    }

    #endregion

    #region Utility Methods

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

    // Debug gizmos
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(Vector3.zero, Vector3.forward * 10f);
    }
}
