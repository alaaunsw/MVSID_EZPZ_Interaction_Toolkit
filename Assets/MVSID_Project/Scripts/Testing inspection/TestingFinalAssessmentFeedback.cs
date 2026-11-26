
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.InputSystem;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public class TestingFinalAssessmentFeedback : MonoBehaviour
{
    [Header("Camera")]
    public Camera playerCamera;
    public float distanceFromPlayer = 1.6f;

    [Header("Panels")]
    public GameObject summaryPanel;
    public GameObject reviewPanel;
    public GameObject completionPanel;

    [Header("Summary Text")]
    public TextMeshProUGUI traineeText;
    public TextMeshProUGUI overallScoreText;
    public TextMeshProUGUI resultText;
    public TextMeshProUGUI statisticsText;
    public TextMeshProUGUI safetyResultsText;
    public TextMeshProUGUI inspectionResultsText;
    public TextMeshProUGUI improvementText;

    [Header("Review Text")]
    public TextMeshProUGUI reviewTitleText;
    public TextMeshProUGUI reviewStatusText;
    public TextMeshProUGUI questionText;
    public TextMeshProUGUI submittedAnswerText;
    public TextMeshProUGUI expectedAnswerText;
    public TextMeshProUGUI answerImprovementText;
    public TextMeshProUGUI pageText;

    [Header("Buttons")]
    public Button reviewButton;
    public Button finishButton;
    public Button previousButton;
    public Button nextButton;
    public Button backButton;
    public Button reviewFinishButton;
    public Button exitButton;

    [Header("Panels To Hide")]
    public GameObject[] panelsToHide;

    [Header("After Experience")]
    public UnityEvent onExperienceFinished;

    [Header("Status")]
    [SerializeField] private bool feedbackOpened;
    [SerializeField] private bool experienceFinished;
    [SerializeField] private int currentPage;
    [SerializeField] private int totalPages;

    private bool cursorCaptured;
    private bool oldCursorVisible;
    private CursorLockMode oldCursorLock;

    private readonly Color yellow =
        new Color32(255, 213, 79, 255);

    private readonly Color green =
        new Color32(100, 235, 150, 255);

    private readonly Color red =
        new Color32(255, 110, 110, 255);

    private readonly Color grey =
        new Color32(180, 180, 180, 255);

    private readonly List<ReviewEntry> entries =
        new List<ReviewEntry>();

    private class ReviewEntry
    {
        public string title;
        public string question;
        public string submitted;
        public string expected;
        public string improvement;
        public bool correct;
        public bool scored;
        public bool completed;
    }

    // =====================================
    // BUILD ALL FEEDBACK PANELS
    // =====================================

    [ContextMenu("Build Final Assessment Feedback")]
    public void BuildFeedback()
    {
        if (Application.isPlaying)
        {
            Debug.LogWarning("Stop Play Mode first.");
            return;
        }

#if UNITY_EDITOR
        Undo.RecordObject(this, "Build Final Feedback");

        if (summaryPanel != null)
            Undo.DestroyObjectImmediate(summaryPanel);

        if (reviewPanel != null)
            Undo.DestroyObjectImmediate(reviewPanel);

        if (completionPanel != null)
            Undo.DestroyObjectImmediate(completionPanel);
#endif

        BuildSummaryPanel();
        BuildReviewPanel();
        BuildCompletionPanel();

        summaryPanel.SetActive(true);
        reviewPanel.SetActive(false);
        completionPanel.SetActive(false);

        PositionPanel(summaryPanel);

#if UNITY_EDITOR
        EditorUtility.SetDirty(this);

        EditorSceneManager.MarkSceneDirty(
            gameObject.scene
        );
#endif

        Debug.Log(
            "[ASSESSMENT] Final feedback panels built."
        );
    }

    // =====================================
    // SUMMARY PANEL
    // =====================================

    private void BuildSummaryPanel()
    {
        summaryPanel = CreateCanvas(
            "Testing_FinalAssessment_Summary",
            new Vector2(1250, 900)
        );

        Transform root = summaryPanel.transform;

        MakeText(
            root,
            "Title",
            "FINAL ASSESSMENT RESULTS",
            new Vector2(0, 395),
            new Vector2(1190, 65),
            40,
            Color.white
        );

        MakeText(
            root,
            "Subtitle",
            "INDEPENDENT TESTING COMPLETED",
            new Vector2(0, 340),
            new Vector2(1190, 45),
            27,
            yellow
        );

        traineeText = MakeText(
            root,
            "Trainee",
            "Trainee: --",
            new Vector2(0, 292),
            new Vector2(1190, 45),
            25,
            Color.white
        );

        // =====================================
        // OVERALL SCORE
        // =====================================

        RectTransform scoreBox = MakeBox(
            root,
            "ScoreBackground",
            new Vector2(-420, 147),
            new Vector2(350, 225)
        );

        overallScoreText = MakeText(
            scoreBox,
            "OverallScore",
            "--%",
            new Vector2(0, 35),
            new Vector2(320, 120),
            84,
            yellow
        );

        MakeText(
            scoreBox,
            "OverallLabel",
            "OVERALL ACCURACY",
            new Vector2(0, -35),
            new Vector2(320, 40),
            24,
            Color.white
        );

        resultText = MakeText(
            scoreBox,
            "PassStatus",
            "--",
            new Vector2(0, -80),
            new Vector2(330, 45),
            25,
            Color.white
        );

        // =====================================
        // SESSION STATISTICS
        // =====================================

        RectTransform statsBox = MakeBox(
            root,
            "StatisticsBackground",
            new Vector2(185, 147),
            new Vector2(810, 225)
        );

        statisticsText = MakeText(
            statsBox,
            "Statistics",
            "Session statistics",
            Vector2.zero,
            new Vector2(755, 195),
            31,
            Color.white
        );

        // =====================================
        // SAFETY RESULTS
        // =====================================

        RectTransform safetyBox = MakeBox(
            root,
            "SafetyResultsBackground",
            new Vector2(-300, -105),
            new Vector2(580, 250)
        );

        MakeText(
            safetyBox,
            "SafetyHeading",
            "SAFETY ASSESSMENT",
            new Vector2(0, 92),
            new Vector2(540, 45),
            27,
            yellow
        );

        safetyResultsText = MakeText(
            safetyBox,
            "SafetyResults",
            "Safety task results",
            new Vector2(0, -22),
            new Vector2(535, 175),
            25,
            Color.white
        );

        safetyResultsText.alignment =
            TextAlignmentOptions.MidlineLeft;

        // =====================================
        // INSPECTION RESULTS
        // =====================================

        RectTransform inspectionBox = MakeBox(
            root,
            "InspectionResultsBackground",
            new Vector2(300, -105),
            new Vector2(580, 250)
        );

        MakeText(
            inspectionBox,
            "InspectionHeading",
            "PIPELINE INSPECTION",
            new Vector2(0, 92),
            new Vector2(540, 45),
            27,
            yellow
        );

        inspectionResultsText = MakeText(
            inspectionBox,
            "InspectionResults",
            "Inspection task results",
            new Vector2(0, -25),
            new Vector2(540, 180),
            23,
            Color.white
        );

        inspectionResultsText.alignment =
            TextAlignmentOptions.MidlineLeft;

        // =====================================
        // IMPROVEMENT RECOMMENDATIONS
        // =====================================

        MakeText(
            root,
            "ImprovementHeading",
            "RECOMMENDED IMPROVEMENTS",
            new Vector2(0, -260),
            new Vector2(1170, 40),
            26,
            yellow
        );

        improvementText = MakeText(
            root,
            "ImprovementSummary",
            "--",
            new Vector2(0, -310),
            new Vector2(1170, 65),
            24,
            Color.white
        );

        reviewButton = MakeButton(
            root,
            "ReviewAnswersButton",
            "REVIEW ANSWERS",
            new Vector2(-300, -395),
            new Vector2(500, 75)
        );

        finishButton = MakeButton(
            root,
            "FinishExperienceButton",
            "FINISH EXPERIENCE",
            new Vector2(300, -395),
            new Vector2(500, 75)
        );
    }

    // =====================================
    // DETAILED REVIEW PANEL
    // =====================================

    private void BuildReviewPanel()
    {
        reviewPanel = CreateCanvas(
            "Testing_FinalAssessment_Review",
            new Vector2(1250, 900)
        );

        Transform root = reviewPanel.transform;

        MakeText(
            root,
            "Title",
            "ASSESSMENT ANSWER REVIEW",
            new Vector2(0, 395),
            new Vector2(1190, 65),
            38,
            Color.white
        );

        reviewTitleText = MakeText(
            root,
            "ReviewTaskTitle",
            "--",
            new Vector2(0, 330),
            new Vector2(1190, 55),
            29,
            yellow
        );

        reviewStatusText = MakeText(
            root,
            "AnswerStatus",
            "--",
            new Vector2(0, 280),
            new Vector2(1190, 45),
            26,
            Color.white
        );

        questionText = MakeText(
            root,
            "Question",
            "--",
            new Vector2(0, 210),
            new Vector2(1150, 100),
            30,
            Color.white
        );

        // =====================================
        // TRAINEE ANSWER
        // =====================================

        RectTransform left = MakeBox(
            root,
            "SubmittedAnswerBackground",
            new Vector2(-300, -10),
            new Vector2(580, 310)
        );

        MakeText(
            left,
            "SubmittedHeading",
            "YOUR ANSWER",
            new Vector2(0, 120),
            new Vector2(540, 45),
            27,
            yellow
        );

        submittedAnswerText = MakeText(
            left,
            "SubmittedAnswer",
            "--",
            new Vector2(0, -20),
            new Vector2(530, 220),
            30,
            Color.white
        );

        // =====================================
        // EXPECTED ANSWER
        // =====================================

        RectTransform right = MakeBox(
            root,
            "ExpectedAnswerBackground",
            new Vector2(300, -10),
            new Vector2(580, 310)
        );

        MakeText(
            right,
            "ExpectedHeading",
            "EXPECTED ANSWER / REQUIREMENT",
            new Vector2(0, 120),
            new Vector2(540, 45),
            25,
            yellow
        );

        expectedAnswerText = MakeText(
            right,
            "ExpectedAnswer",
            "--",
            new Vector2(0, -20),
            new Vector2(530, 220),
            30,
            Color.white
        );

        // =====================================
        // IMPROVEMENT GUIDANCE
        // =====================================

        MakeText(
            root,
            "ImprovementHeading",
            "HOW TO IMPROVE",
            new Vector2(0, -190),
            new Vector2(1180, 40),
            27,
            yellow
        );

        answerImprovementText = MakeText(
            root,
            "AnswerImprovement",
            "--",
            new Vector2(0, -250),
            new Vector2(1150, 85),
            26,
            Color.white
        );

        // =====================================
        // NAVIGATION BUTTONS
        // =====================================

        previousButton = MakeButton(
            root,
            "PreviousButton",
            "PREVIOUS",
            new Vector2(-440, -335),
            new Vector2(260, 65)
        );

        nextButton = MakeButton(
            root,
            "NextButton",
            "NEXT",
            new Vector2(440, -335),
            new Vector2(260, 65)
        );

        pageText = MakeText(
            root,
            "PageNumber",
            "1 / 1",
            new Vector2(0, -335),
            new Vector2(450, 55),
            27,
            Color.white
        );

        backButton = MakeButton(
            root,
            "BackToSummary",
            "BACK TO SUMMARY",
            new Vector2(-300, -405),
            new Vector2(500, 65)
        );

        reviewFinishButton = MakeButton(
            root,
            "ReviewFinishButton",
            "FINISH EXPERIENCE",
            new Vector2(300, -405),
            new Vector2(500, 65)
        );
    }

    // =====================================
    // COMPLETION PANEL
    // =====================================

    private void BuildCompletionPanel()
    {
        completionPanel = CreateCanvas(
            "Testing_ExperienceCompleted",
            new Vector2(1000, 550)
        );

        Transform root = completionPanel.transform;

        MakeText(
            root,
            "Title",
            "EXPERIENCE COMPLETED",
            new Vector2(0, 195),
            new Vector2(940, 80),
            45,
            yellow
        );

        MakeText(
            root,
            "CompletionMessage",
            "You have completed the Independent Testing " +
            "experience.\n\nYour assessment results have " +
            "been recorded for this session.\n\n" +
            "Thank you for participating.",
            new Vector2(0, 20),
            new Vector2(880, 240),
            32,
            Color.white
        );

        exitButton = MakeButton(
            root,
            "ExitButton",
            "EXIT EXPERIENCE",
            new Vector2(0, -190),
            new Vector2(480, 80)
        );
    }

    // =====================================
    // RUNTIME SETUP
    // =====================================

    private void Awake()
    {
        HideAllPanels();

        BindButton(reviewButton, ShowAnswerReview);
        BindButton(finishButton, FinishExperience);
        BindButton(previousButton, PreviousAnswer);
        BindButton(nextButton, NextAnswer);
        BindButton(backButton, ReturnToSummary);
        BindButton(reviewFinishButton, FinishExperience);
        BindButton(exitButton, ExitExperience);
    }

    private void BindButton(
        Button button,
        UnityAction action)
    {
        if (button == null)
            return;

        button.onClick.AddListener(action);

        InteractableGeneral ezpz =
            button.GetComponent<InteractableGeneral>();

        if (ezpz != null)
        {
            ezpz.onPrimaryInteract.AddListener(action);
        }
    }

    // =====================================
    // OPEN FINAL ASSESSMENT
    // =====================================

    public void ShowFinalFeedback()
    {
        if (feedbackOpened || experienceFinished)
            return;

        TestingAssessmentManager manager =
            TestingAssessmentManager.Instance;

        if (manager == null ||
            !manager.SessionStarted)
        {
            Debug.LogError(
                "[ASSESSMENT] No testing session found."
            );

            return;
        }

        if (!manager.AllTasksCompleted())
        {
            Debug.LogWarning(
                "[ASSESSMENT] Not all tasks are completed."
            );

            return;
        }

        if (summaryPanel == null ||
            reviewPanel == null ||
            completionPanel == null)
        {
            Debug.LogError(
                "[ASSESSMENT] Build feedback panels first."
            );

            return;
        }

        if (!manager.SessionCompleted)
        {
            manager.FinishSession();
        }

        HidePreviousPanels();

        PopulateSummary(manager);

        BuildReviewEntries(manager);

        CaptureCursor();

        feedbackOpened = true;

        ShowOnly(summaryPanel);

        Debug.Log(
            "[ASSESSMENT] Final feedback opened."
        );
    }

    // =====================================
    // POPULATE OVERALL SUMMARY
    // =====================================

    private void PopulateSummary(
        TestingAssessmentManager manager)
    {
        traineeText.text =
            "Trainee: " + manager.TraineeName +
            "   |   PIPE_0493";

        float overallScore =
            manager.GetOverallScore();

        overallScoreText.text =
            overallScore.ToString("0.#") + "%";

        bool passed = manager.HasPassed();

        resultText.text = passed

            ? "ASSESSMENT PASSED"

            : "FURTHER PRACTICE REQUIRED";

        resultText.color =
            passed ? green : red;

        // =====================================
        // SESSION STATISTICS
        // =====================================

        statisticsText.text =
            "Tasks completed: " +
            manager.GetCompletedTaskCount() +
            " / " +
            manager.GetTotalTaskCount() +

            "\nCompletion time: " +
            manager.GetFormattedDuration() +

            "\nSafety accuracy: " +
            manager.GetSafetyScore().ToString("0.#") +
            "%" +

            "\nInspection accuracy: " +
            manager.GetInspectionScore().ToString("0.#") +
            "%";

        StringBuilder safety =
            new StringBuilder();

        StringBuilder inspection =
            new StringBuilder();

        List<string> improvementAreas =
            new List<string>();

        // =====================================
        // INDIVIDUAL TASK RESULTS
        // =====================================

        foreach (var task in manager.Tasks)
        {
            string status;

            if (!task.completed)
            {
                status = "Incomplete";
            }
            else if (!task.contributesToScore ||
                     task.totalAnswers == 0)
            {
                status = "Completed";
            }
            else
            {
                status =
                    task.correctAnswers + "/" +
                    task.totalAnswers;
            }

            string line =
                task.taskName + ": " + status;

            if (task.taskID.StartsWith("SAFETY_"))
            {
                safety.AppendLine(line);
            }
            else if (
                task.taskID.StartsWith("INSPECTION_"))
            {
                inspection.AppendLine(line);
            }

            if (task.contributesToScore &&
                task.correctAnswers < task.totalAnswers)
            {
                improvementAreas.Add(task.taskName);
            }
        }

        safetyResultsText.text =
            safety.ToString();

        inspectionResultsText.text =
            inspection.ToString();

        improvementText.text =
            improvementAreas.Count == 0

            ? "All scored assessment criteria were met."

            : "Recommended practice: " +
              string.Join("; ", improvementAreas) +
              ". Review individual answers for details.";
    }

    // =====================================
    // PREPARE DETAILED REVIEW
    // =====================================

    private void BuildReviewEntries(
        TestingAssessmentManager manager)
    {
        entries.Clear();

        foreach (var task in manager.Tasks)
        {
            if (task.answers != null &&
                task.answers.Count > 0)
            {
                foreach (var answer in task.answers)
                {
                    entries.Add(new ReviewEntry
                    {
                        title = task.taskName,

                        question = answer.question,

                        submitted = answer.traineeAnswer,

                        expected = answer.correctAnswer,

                        improvement = answer.improvement,

                        correct = answer.isCorrect,

                        scored = task.contributesToScore,

                        completed = task.completed
                    });
                }
            }
            else
            {
                entries.Add(new ReviewEntry
                {
                    title = task.taskName,

                    question = "Task performance summary",

                    submitted =
                        task.correctAnswers + " / " +
                        task.totalAnswers +
                        " recorded criteria",

                    expected =
                        "Complete the assigned activity.",

                    improvement = task.improvement,

                    correct =
                        task.completed &&
                        task.correctAnswers ==
                        task.totalAnswers,

                    scored = task.contributesToScore,

                    completed = task.completed
                });
            }
        }

        currentPage = 0;

        totalPages = entries.Count;
    }

    // =====================================
    // SHOW ANSWER REVIEW
    // =====================================

    public void ShowAnswerReview()
    {
        if (!feedbackOpened ||
            experienceFinished ||
            entries.Count == 0)
        {
            return;
        }

        currentPage = 0;

        PopulateReviewPage();

        ShowOnly(reviewPanel);
    }

    // =====================================
    // DISPLAY INDIVIDUAL ANSWER
    // =====================================

    private void PopulateReviewPage()
    {
        if (entries.Count == 0)
            return;

        currentPage = Mathf.Clamp(
            currentPage,
            0,
            entries.Count - 1
        );

        ReviewEntry entry =
            entries[currentPage];

        reviewTitleText.text =
            entry.title.ToUpperInvariant();

        questionText.text =
            entry.question;

        submittedAnswerText.text =
            entry.submitted;

        expectedAnswerText.text =
            entry.expected;

        // =====================================
        // COMPLETION-ONLY TASKS
        // =====================================

        if (!entry.scored)
        {
            reviewStatusText.text = entry.completed

                ? "COMPLETED - NOT SCORED"

                : "INCOMPLETE - NOT SCORED";

            reviewStatusText.color =
                Color.white;

            submittedAnswerText.color =
                Color.white;

            answerImprovementText.text =
                string.IsNullOrWhiteSpace(
                    entry.improvement
                )

                ? "Completion-only activity. No additional " +
                  "accuracy marks are awarded."

                : entry.improvement;
        }

        // =====================================
        // SCORED ASSESSMENT ANSWERS
        // =====================================

        else
        {
            reviewStatusText.text = entry.correct

                ? "CORRECT ANSWER"

                : "INCORRECT ANSWER";

            reviewStatusText.color =
                entry.correct ? green : red;

            submittedAnswerText.color =
                entry.correct ? green : red;

            answerImprovementText.text =
                string.IsNullOrWhiteSpace(
                    entry.improvement
                )

                ? (entry.correct

                    ? "Assessment criterion met."

                    : "Review the expected answer.")

                : entry.improvement;
        }

        expectedAnswerText.color =
            Color.white;

        pageText.text =
            (currentPage + 1) +
            " / " +
            entries.Count;

        previousButton.interactable =
            currentPage > 0;

        nextButton.interactable =
            currentPage < entries.Count - 1;
    }

    // =====================================
    // PREVIOUS ANSWER
    // =====================================

    public void PreviousAnswer()
    {
        if (reviewPanel == null ||
            !reviewPanel.activeInHierarchy ||
            currentPage <= 0)
        {
            return;
        }

        currentPage--;

        PopulateReviewPage();
    }

    // =====================================
    // NEXT ANSWER
    // =====================================

    public void NextAnswer()
    {
        if (reviewPanel == null ||
            !reviewPanel.activeInHierarchy ||
            currentPage >= entries.Count - 1)
        {
            return;
        }

        currentPage++;

        PopulateReviewPage();
    }

    // =====================================
    // RETURN TO SUMMARY
    // =====================================

    public void ReturnToSummary()
    {
        if (!feedbackOpened ||
            experienceFinished)
        {
            return;
        }

        ShowOnly(summaryPanel);
    }

    // =====================================
    // FINISH EXPERIENCE
    // =====================================

    public void FinishExperience()
    {
        if (!feedbackOpened ||
            experienceFinished)
        {
            return;
        }

        experienceFinished = true;

        ShowOnly(completionPanel);

        Debug.Log(
            "[ASSESSMENT] Experience completed."
        );
    }

    // =====================================
    // EXIT EXPERIENCE
    // =====================================

    public void ExitExperience()
    {
        if (!experienceFinished)
            return;

        HideAllPanels();

        RestoreCursor();

        // Connect this event to your main menu
        // or preferred scene transition.

        onExperienceFinished?.Invoke();
    }

    // =====================================
    // DESKTOP PHYSICS INTERACTION
    // =====================================

    private void Update()
    {
        if (!feedbackOpened ||
            playerCamera == null ||
            Mouse.current == null)
        {
            return;
        }

        if (!Mouse.current.leftButton.wasPressedThisFrame)
            return;

        Ray ray =
            playerCamera.ScreenPointToRay(
                Mouse.current.position.ReadValue()
            );

        Button[] buttons =
        {
            reviewButton,
            finishButton,
            previousButton,
            nextButton,
            backButton,
            reviewFinishButton,
            exitButton
        };

        foreach (Button button in buttons)
        {
            if (button == null ||
                !button.interactable ||
                !button.gameObject.activeInHierarchy)
            {
                continue;
            }

            BoxCollider box =
                button.GetComponent<BoxCollider>();

            if (box != null &&
                box.Raycast(
                    ray,
                    out RaycastHit hit,
                    100f))
            {
                button.onClick.Invoke();

                break;
            }
        }
    }

    // =====================================
    // HIDE PREVIOUS PANELS
    // =====================================

    private void HidePreviousPanels()
    {
        if (panelsToHide == null)
            return;

        foreach (GameObject panel in panelsToHide)
        {
            if (panel == null ||
                panel == gameObject ||
                panel == summaryPanel ||
                panel == reviewPanel ||
                panel == completionPanel)
            {
                continue;
            }

            if (transform.IsChildOf(panel.transform))
                continue;

            panel.SetActive(false);
        }
    }

    // =====================================
    // HIDE ALL FEEDBACK PANELS
    // =====================================

    private void HideAllPanels()
    {
        if (summaryPanel != null)
            summaryPanel.SetActive(false);

        if (reviewPanel != null)
            reviewPanel.SetActive(false);

        if (completionPanel != null)
            completionPanel.SetActive(false);
    }

    // =====================================
    // DISPLAY SELECTED PANEL
    // =====================================

    private void ShowOnly(GameObject panel)
    {
        HideAllPanels();

        if (panel == null)
            return;

        PositionPanel(panel);

        panel.SetActive(true);
    }

    // =====================================
    // POSITION PANEL
    // =====================================

    private void PositionPanel(GameObject panel)
    {
        if (playerCamera == null)
            playerCamera = Camera.main;

        if (playerCamera == null ||
            panel == null)
        {
            return;
        }

        Transform cam =
            playerCamera.transform;

        panel.transform.position =
            cam.position +
            cam.forward * distanceFromPlayer;

        panel.transform.rotation =
            cam.rotation;

        Canvas canvas =
            panel.GetComponent<Canvas>();

        if (canvas != null)
        {
            canvas.worldCamera = playerCamera;
        }
    }

    // =====================================
    // DESKTOP CURSOR CONTROL
    // =====================================

    private void CaptureCursor()
    {
        if (cursorCaptured)
            return;

        oldCursorVisible = Cursor.visible;

        oldCursorLock = Cursor.lockState;

        cursorCaptured = true;

        Cursor.visible = true;

        Cursor.lockState = CursorLockMode.None;
    }

    private void RestoreCursor()
    {
        if (!cursorCaptured)
            return;

        Cursor.visible = oldCursorVisible;

        Cursor.lockState = oldCursorLock;

        cursorCaptured = false;
    }

    private void OnDisable()
    {
        RestoreCursor();
    }

    // =====================================
    // CREATE WORLD SPACE CANVAS
    // =====================================

    private GameObject CreateCanvas(
        string objectName,
        Vector2 size)
    {
        GameObject obj = new GameObject(
            objectName,
            typeof(RectTransform),
            typeof(Canvas),
            typeof(GraphicRaycaster),
            typeof(Image)
        );

        Canvas canvas =
            obj.GetComponent<Canvas>();

        canvas.renderMode =
            RenderMode.WorldSpace;

        canvas.worldCamera =
            playerCamera;

        canvas.overrideSorting = true;

        canvas.sortingOrder = 200;

        RectTransform rect =
            obj.GetComponent<RectTransform>();

        rect.sizeDelta = size;

        rect.localScale =
            Vector3.one * 0.0017f;

        Image image =
            obj.GetComponent<Image>();

        image.color =
            new Color32(10, 10, 10, 250);

        image.raycastTarget = false;

#if UNITY_EDITOR
        Undo.RegisterCreatedObjectUndo(
            obj,
            "Build Final Feedback Panel"
        );
#endif

        return obj;
    }

    // =====================================
    // CREATE RECT TRANSFORM
    // =====================================

    private RectTransform MakeRect(
        Transform parent,
        string objectName,
        Vector2 position,
        Vector2 size)
    {
        GameObject obj = new GameObject(
            objectName,
            typeof(RectTransform)
        );

        obj.transform.SetParent(parent, false);

        RectTransform rect =
            obj.GetComponent<RectTransform>();

        rect.anchorMin =
            Vector2.one * 0.5f;

        rect.anchorMax =
            Vector2.one * 0.5f;

        rect.pivot =
            Vector2.one * 0.5f;

        rect.anchoredPosition = position;

        rect.sizeDelta = size;

        rect.localScale = Vector3.one;

        return rect;
    }

    // =====================================
    // CREATE BACKGROUND BOX
    // =====================================

    private RectTransform MakeBox(
        Transform parent,
        string objectName,
        Vector2 position,
        Vector2 size)
    {
        RectTransform rect = MakeRect(
            parent,
            objectName,
            position,
            size
        );

        Image image =
            rect.gameObject.AddComponent<Image>();

        image.color =
            new Color32(35, 35, 35, 255);

        image.raycastTarget = false;

        return rect;
    }

    // =====================================
    // CREATE TEXT
    // =====================================

    private TextMeshProUGUI MakeText(
        Transform parent,
        string objectName,
        string content,
        Vector2 position,
        Vector2 size,
        float fontSize,
        Color colour)
    {
        RectTransform rect = MakeRect(
            parent,
            objectName,
            position,
            size
        );

        TextMeshProUGUI text =
            rect.gameObject.AddComponent<
                TextMeshProUGUI
            >();

        text.text = content;

        text.fontSize = fontSize;

        text.color = colour;

        text.alignment =
            TextAlignmentOptions.Center;

        text.raycastTarget = false;

        text.enableAutoSizing = true;

        text.fontSizeMin = 16;

        text.fontSizeMax = fontSize;

        return text;
    }

    // =====================================
    // CREATE BUTTON WITH EZPZ INTERACTION
    // =====================================

    private Button MakeButton(
        Transform parent,
        string objectName,
        string label,
        Vector2 position,
        Vector2 size)
    {
        RectTransform rect = MakeBox(
            parent,
            objectName,
            position,
            size
        );

        Image image =
            rect.GetComponent<Image>();

        image.color =
            new Color32(65, 65, 65, 255);

        image.raycastTarget = true;

        Button button =
            rect.gameObject.AddComponent<Button>();

        button.targetGraphic = image;

        ColorBlock colours = button.colors;

        colours.normalColor = Color.white;

        colours.highlightedColor = yellow;

        colours.pressedColor =
            new Color32(255, 235, 155, 255);

        button.colors = colours;

        MakeText(
            rect,
            "ButtonText",
            label,
            Vector2.zero,
            size - new Vector2(15, 10),
            27,
            Color.white
        );

        BoxCollider collider =
            rect.gameObject.AddComponent<
                BoxCollider
            >();

        collider.size =
            new Vector3(
                size.x,
                size.y,
                8
            );

        rect.gameObject.AddComponent<
            InteractableGeneral
        >();

        return button;
    }
}
