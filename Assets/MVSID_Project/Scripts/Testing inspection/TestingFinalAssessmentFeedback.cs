
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public class TestingFinalAssessmentFeedback : MonoBehaviour
{
    // =====================================
    // CAMERA AND PANEL POSITION
    // =====================================

    [Header("Camera")]
    public Camera playerCamera;

    public float distanceFromPlayer = 2.2f;

    public float verticalOffset = 0f;

    public float panelScale = 0.0015f;

    [Header("Navigation Settings")]
    [Min(0.1f)]
    public float navigationDelay = 0.4f;

    private float nextNavigationTime;

    private Vector3 feedbackPosition;
    private Quaternion feedbackRotation;

    private bool feedbackPositionCaptured;

    // =====================================
    // PANELS
    // =====================================

    [Header("Panels")]
    public GameObject summaryPanel;
    public GameObject reviewPanel;
    public GameObject completionPanel;

    // =====================================
    // SUMMARY TEXT
    // =====================================

    [Header("Summary Text")]
    public TextMeshProUGUI traineeText;
    public TextMeshProUGUI overallScoreText;
    public TextMeshProUGUI resultText;
    public TextMeshProUGUI statisticsText;
    public TextMeshProUGUI safetyResultsText;
    public TextMeshProUGUI inspectionResultsText;
    public TextMeshProUGUI improvementText;

    // =====================================
    // REVIEW TEXT
    // =====================================

    [Header("Review Text")]
    public TextMeshProUGUI reviewTitleText;
    public TextMeshProUGUI reviewStatusText;
    public TextMeshProUGUI questionText;
    public TextMeshProUGUI submittedAnswerText;
    public TextMeshProUGUI expectedAnswerText;
    public TextMeshProUGUI answerImprovementText;
    public TextMeshProUGUI pageText;

    // =====================================
    // LONG REPORT REVIEW
    // =====================================

    [Header("Long Report Review")]
    public GameObject standardAnswerLayout;
    public GameObject longReportLayout;

    public TextMeshProUGUI reportLeftText;
    public TextMeshProUGUI reportRightText;
    public TextMeshProUGUI reportRequirementText;

    // =====================================
    // BUTTONS
    // =====================================

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

    // =====================================
    // STATUS
    // =====================================

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
        public string taskID;
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
            Debug.LogWarning(
                "Stop Play Mode before building panels."
            );

            return;
        }

#if UNITY_EDITOR
        Undo.RecordObject(
            this,
            "Build Final Assessment Feedback"
        );

        if (summaryPanel != null)
            Undo.DestroyObjectImmediate(summaryPanel);

        if (reviewPanel != null)
            Undo.DestroyObjectImmediate(reviewPanel);

        if (completionPanel != null)
            Undo.DestroyObjectImmediate(completionPanel);
#endif

        feedbackPositionCaptured = false;

        BuildSummaryPanel();
        BuildReviewPanel();
        BuildCompletionPanel();

        PositionPanel(summaryPanel);

        summaryPanel.SetActive(true);
        reviewPanel.SetActive(false);
        completionPanel.SetActive(false);

#if UNITY_EDITOR
        EditorUtility.SetDirty(this);

        EditorSceneManager.MarkSceneDirty(
            gameObject.scene
        );
#endif

        Debug.Log(
            "[ASSESSMENT] Updated feedback panels built."
        );
    }

    // =====================================
    // SUMMARY PANEL
    // =====================================

    private void BuildSummaryPanel()
    {
        summaryPanel = CreateCanvas(
            "Testing_FinalAssessment_Summary",
            new Vector2(1400, 1000)
        );

        Transform root = summaryPanel.transform;

        MakeText(
            root,
            "Title",
            "FINAL ASSESSMENT RESULTS",
            new Vector2(0, 445),
            new Vector2(1340, 65),
            43,
            Color.white
        );

        MakeText(
            root,
            "Subtitle",
            "INDEPENDENT TESTING COMPLETED",
            new Vector2(0, 390),
            new Vector2(1340, 45),
            29,
            yellow
        );

        traineeText = MakeText(
            root,
            "Trainee",
            "Trainee: --",
            new Vector2(0, 342),
            new Vector2(1320, 45),
            28,
            Color.white
        );

        // =====================================
        // OVERALL SCORE
        // =====================================

        RectTransform scoreBox = MakeBox(
            root,
            "ScoreBackground",
            new Vector2(-470, 195),
            new Vector2(400, 230)
        );

        overallScoreText = MakeText(
            scoreBox,
            "OverallScore",
            "--%",
            new Vector2(0, 35),
            new Vector2(370, 110),
            88,
            yellow
        );

        MakeText(
            scoreBox,
            "OverallLabel",
            "OVERALL ACCURACY",
            new Vector2(0, -35),
            new Vector2(370, 45),
            27,
            Color.white
        );

        resultText = MakeText(
            scoreBox,
            "PassStatus",
            "--",
            new Vector2(0, -82),
            new Vector2(380, 45),
            25,
            Color.white
        );

        // =====================================
        // SESSION STATISTICS
        // =====================================

        RectTransform statsBox = MakeBox(
            root,
            "StatisticsBackground",
            new Vector2(210, 195),
            new Vector2(920, 230)
        );

        statisticsText = MakeText(
            statsBox,
            "Statistics",
            "Session statistics",
            Vector2.zero,
            new Vector2(860, 205),
            34,
            Color.white
        );

        // =====================================
        // SAFETY RESULTS
        // =====================================

        RectTransform safetyBox = MakeBox(
            root,
            "SafetyResultsBackground",
            new Vector2(-345, -105),
            new Vector2(650, 350)
        );

        MakeText(
            safetyBox,
            "SafetyHeading",
            "SAFETY ASSESSMENT",
            new Vector2(0, 143),
            new Vector2(610, 45),
            30,
            yellow
        );

        safetyResultsText = MakeText(
            safetyBox,
            "SafetyResults",
            "Safety task results",
            new Vector2(0, -20),
            new Vector2(600, 260),
            30,
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
            new Vector2(345, -105),
            new Vector2(650, 350)
        );

        MakeText(
            inspectionBox,
            "InspectionHeading",
            "PIPELINE INSPECTION",
            new Vector2(0, 143),
            new Vector2(610, 45),
            30,
            yellow
        );

        inspectionResultsText = MakeText(
            inspectionBox,
            "InspectionResults",
            "Inspection task results",
            new Vector2(0, -20),
            new Vector2(610, 265),
            26,
            Color.white
        );

        inspectionResultsText.alignment =
            TextAlignmentOptions.MidlineLeft;

        inspectionResultsText.fontSizeMin = 20;

        // =====================================
        // RECOMMENDATIONS
        // =====================================

        MakeText(
            root,
            "ImprovementHeading",
            "RECOMMENDED IMPROVEMENTS",
            new Vector2(0, -305),
            new Vector2(1300, 40),
            28,
            yellow
        );

        improvementText = MakeText(
            root,
            "ImprovementSummary",
            "--",
            new Vector2(0, -365),
            new Vector2(1300, 75),
            27,
            Color.white
        );

        // =====================================
        // SUMMARY BUTTONS
        // =====================================

        reviewButton = MakeButton(
            root,
            "ReviewAnswersButton",
            "REVIEW ANSWERS",
            new Vector2(-345, -450),
            new Vector2(580, 70)
        );

        finishButton = MakeButton(
            root,
            "FinishExperienceButton",
            "FINISH EXPERIENCE",
            new Vector2(345, -450),
            new Vector2(580, 70)
        );
    }

    // =====================================
    // REVIEW PANEL
    // =====================================

    private void BuildReviewPanel()
    {
        reviewPanel = CreateCanvas(
            "Testing_FinalAssessment_Review",
            new Vector2(1400, 1000)
        );

        Transform root = reviewPanel.transform;

        MakeText(
            root,
            "Title",
            "ASSESSMENT ANSWER REVIEW",
            new Vector2(0, 445),
            new Vector2(1340, 65),
            40,
            Color.white
        );

        reviewTitleText = MakeText(
            root,
            "ReviewTaskTitle",
            "--",
            new Vector2(0, 387),
            new Vector2(1320, 50),
            30,
            yellow
        );

        reviewStatusText = MakeText(
            root,
            "AnswerStatus",
            "--",
            new Vector2(0, 337),
            new Vector2(1320, 45),
            27,
            Color.white
        );

        questionText = MakeText(
            root,
            "Question",
            "--",
            new Vector2(0, 268),
            new Vector2(1300, 85),
            30,
            Color.white
        );

        // =====================================
        // STANDARD ANSWER LAYOUT
        // =====================================

        standardAnswerLayout = MakeRect(
            root,
            "StandardAnswerLayout",
            Vector2.zero,
            new Vector2(1400, 1000)
        ).gameObject;

        Transform standardRoot =
            standardAnswerLayout.transform;

        RectTransform left = MakeBox(
            standardRoot,
            "SubmittedAnswerBackground",
            new Vector2(-345, 25),
            new Vector2(650, 380)
        );

        MakeText(
            left,
            "SubmittedHeading",
            "YOUR ANSWER",
            new Vector2(0, 155),
            new Vector2(610, 45),
            28,
            yellow
        );

        submittedAnswerText = MakeText(
            left,
            "SubmittedAnswer",
            "--",
            new Vector2(0, -20),
            new Vector2(600, 290),
            31,
            Color.white
        );

        RectTransform right = MakeBox(
            standardRoot,
            "ExpectedAnswerBackground",
            new Vector2(345, 25),
            new Vector2(650, 380)
        );

        MakeText(
            right,
            "ExpectedHeading",
            "EXPECTED ANSWER / REQUIREMENT",
            new Vector2(0, 155),
            new Vector2(610, 45),
            26,
            yellow
        );

        expectedAnswerText = MakeText(
            right,
            "ExpectedAnswer",
            "--",
            new Vector2(0, -20),
            new Vector2(600, 290),
            31,
            Color.white
        );

        // =====================================
        // NEW: LONG REPORT LAYOUT
        // =====================================

        longReportLayout = MakeRect(
            root,
            "LongReportLayout",
            Vector2.zero,
            new Vector2(1400, 1000)
        ).gameObject;

        Transform reportRoot =
            longReportLayout.transform;

        RectTransform reportBox = MakeBox(
            reportRoot,
            "ReportBackground",
            new Vector2(0, 48),
            new Vector2(1340, 330)
        );

        MakeText(
            reportBox,
            "ReportHeading",
            "YOUR SUBMITTED INSPECTION REPORT",
            new Vector2(0, 135),
            new Vector2(1280, 45),
            27,
            yellow
        );

        reportLeftText = MakeText(
            reportBox,
            "ReportLeftColumn",
            "--",
            new Vector2(-325, -22),
            new Vector2(605, 235),
            24,
            Color.white
        );

        reportLeftText.alignment =
            TextAlignmentOptions.TopLeft;

        reportLeftText.fontSizeMin = 15;

        reportRightText = MakeText(
            reportBox,
            "ReportRightColumn",
            "--",
            new Vector2(325, -22),
            new Vector2(605, 235),
            24,
            Color.white
        );

        reportRightText.alignment =
            TextAlignmentOptions.TopLeft;

        reportRightText.fontSizeMin = 15;

        RectTransform requirementBox = MakeBox(
            reportRoot,
            "ReportRequirementBackground",
            new Vector2(0, -172),
            new Vector2(1340, 95)
        );

        MakeText(
            requirementBox,
            "RequirementHeading",
            "ASSESSMENT REQUIREMENT",
            new Vector2(0, 24),
            new Vector2(1280, 35),
            23,
            yellow
        );

        reportRequirementText = MakeText(
            requirementBox,
            "ReportRequirement",
            "--",
            new Vector2(0, -20),
            new Vector2(1270, 45),
            24,
            Color.white
        );

        longReportLayout.SetActive(false);

        // =====================================
        // IMPROVEMENT GUIDANCE
        // =====================================

        MakeText(
            root,
            "ImprovementHeading",
            "HOW TO IMPROVE",
            new Vector2(0, -250),
            new Vector2(1320, 40),
            28,
            yellow
        );

        answerImprovementText = MakeText(
            root,
            "AnswerImprovement",
            "--",
            new Vector2(0, -303),
            new Vector2(1300, 65),
            25,
            Color.white
        );

        // =====================================
        // NAVIGATION
        // =====================================

        previousButton = MakeButton(
            root,
            "PreviousButton",
            "PREVIOUS",
            new Vector2(-510, -385),
            new Vector2(300, 65)
        );

        nextButton = MakeButton(
            root,
            "NextButton",
            "NEXT",
            new Vector2(510, -385),
            new Vector2(300, 65)
        );

        pageText = MakeText(
            root,
            "PageNumber",
            "1 / 1",
            new Vector2(0, -385),
            new Vector2(400, 55),
            29,
            Color.white
        );

        backButton = MakeButton(
            root,
            "BackToSummary",
            "BACK TO SUMMARY",
            new Vector2(-345, -455),
            new Vector2(580, 65)
        );

        reviewFinishButton = MakeButton(
            root,
            "ReviewFinishButton",
            "FINISH EXPERIENCE",
            new Vector2(345, -455),
            new Vector2(580, 65)
        );
    }

    // =====================================
    // COMPLETION PANEL
    // =====================================

    private void BuildCompletionPanel()
    {
        completionPanel = CreateCanvas(
            "Testing_ExperienceCompleted",
            new Vector2(1100, 600)
        );

        Transform root = completionPanel.transform;

        MakeText(
            root,
            "Title",
            "EXPERIENCE COMPLETED",
            new Vector2(0, 225),
            new Vector2(1040, 80),
            46,
            yellow
        );

        MakeText(
            root,
            "CompletionMessage",

            "You have completed the Independent Testing " +
            "experience.\n\n" +

            "Your assessment results have been recorded " +
            "for this session.\n\n" +

            "Thank you for participating.",

            new Vector2(0, 30),
            new Vector2(1000, 260),
            34,
            Color.white
        );

        exitButton = MakeButton(
            root,
            "ExitButton",
            "EXIT EXPERIENCE",
            new Vector2(0, -215),
            new Vector2(500, 85)
        );
    }

    // =====================================
    // RUNTIME SETUP
    // =====================================

    private void Awake()
    {
        feedbackOpened = false;
        experienceFinished = false;

        currentPage = 0;
        totalPages = 0;

        nextNavigationTime = 0f;

        feedbackPositionCaptured = false;

        HideAllPanels();

        BindButton(
            reviewButton,
            ShowAnswerReview
        );

        BindButton(
            finishButton,
            FinishExperience
        );

        BindButton(
            previousButton,
            PreviousAnswer
        );

        BindButton(
            nextButton,
            NextAnswer
        );

        BindButton(
            backButton,
            ReturnToSummary
        );

        BindButton(
            reviewFinishButton,
            FinishExperience
        );

        BindButton(
            exitButton,
            ExitExperience
        );
    }

    // =====================================
    // BUTTON BINDING
    // =====================================

    private void BindButton(
        Button button,
        UnityAction action)
    {
        if (button == null)
            return;

        button.onClick.RemoveListener(action);
        button.onClick.AddListener(action);

        InteractableGeneral ezpz =
            button.GetComponent<InteractableGeneral>();

        if (ezpz != null)
        {
            ezpz.onPrimaryInteract.RemoveListener(
                action
            );

            ezpz.onPrimaryInteract.AddListener(
                action
            );
        }
    }

    // =====================================
    // PREVENT DUPLICATE BUTTON EVENTS
    // =====================================

    private bool CanNavigate()
    {
        if (Time.unscaledTime < nextNavigationTime)
        {
            return false;
        }

        nextNavigationTime =
            Time.unscaledTime +
            Mathf.Max(0.1f, navigationDelay);

        return true;
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
            completionPanel == null ||
            standardAnswerLayout == null ||
            longReportLayout == null)
        {
            Debug.LogError(
                "[ASSESSMENT] Rebuild the updated " +
                "feedback panels first."
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

        // Capture one consistent panel position
        // for the entire final feedback experience.

        feedbackPositionCaptured = false;

        CaptureFeedbackPosition();

        feedbackOpened = true;

        ShowOnly(summaryPanel);

        Debug.Log(
            "[ASSESSMENT] Final feedback opened."
        );
    }

    // =====================================
    // POPULATE SUMMARY
    // =====================================

    private void PopulateSummary(
        TestingAssessmentManager manager)
    {
        traineeText.text =
            "Trainee: " +
            manager.TraineeName +
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

        statisticsText.text =

            "Tasks completed: " +
            manager.GetCompletedTaskCount() +
            " / " +
            manager.GetTotalTaskCount() +

            "\nCompletion time: " +
            manager.GetFormattedDuration() +

            "\nSafety accuracy: " +
            manager.GetSafetyScore()
                .ToString("0.#") + "%" +

            "\nInspection accuracy: " +
            manager.GetInspectionScore()
                .ToString("0.#") + "%";

        StringBuilder safety =
            new StringBuilder();

        StringBuilder inspection =
            new StringBuilder();

        List<string> improvementAreas =
            new List<string>();

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

            string taskName =
                GetSummaryTaskName(
                    task.taskID,
                    task.taskName
                );

            string line =
                taskName + ": " + status;

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
                improvementAreas.Add(
                    task.taskName
                );
            }
        }

        safetyResultsText.text =
            safety.ToString().TrimEnd();

        inspectionResultsText.text =
            inspection.ToString().TrimEnd();

        improvementText.text =
            improvementAreas.Count == 0

            ? "All scored assessment criteria were met."

            : "Recommended practice: " +
              string.Join("; ", improvementAreas) +
              ". Review individual answers for details.";
    }

    // =====================================
    // SHORTER SUMMARY TASK LABELS
    // =====================================

    private string GetSummaryTaskName(
        string taskID,
        string originalName)
    {
        switch (taskID)
        {
            case "INSPECTION_01":
                return "Pipeline Identification";

            case "INSPECTION_02":
                return "Visual Inspection";

            case "INSPECTION_03":
                return "Material Loss";

            case "INSPECTION_04":
                return "Initial Engineer Report";

            case "INSPECTION_05":
                return "Instrument Readings";

            case "INSPECTION_06":
                return "Pressure Reduction";

            case "INSPECTION_07":
                return "Updated Pressure";

            case "INSPECTION_08":
                return "Corrosion Risk Simulation";

            case "INSPECTION_09":
                return "Final Inspection Report";

            default:
                return originalName;
        }
    }

    // =====================================
    // PREPARE ALL REVIEW ENTRIES
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
                        taskID = task.taskID,

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
                    taskID = task.taskID,

                    title = task.taskName,

                    question =
                        "Task performance summary",

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

        Debug.Log(
            "[ASSESSMENT] Review entries created: " +
            totalPages
        );
    }

    // =====================================
    // OPEN ANSWER REVIEW
    // =====================================

    public void ShowAnswerReview()
    {
        if (!feedbackOpened ||
            experienceFinished ||
            entries.Count == 0)
        {
            return;
        }

        if (reviewPanel.activeInHierarchy)
            return;

        if (!CanNavigate())
            return;

        currentPage = 0;

        ShowOnly(reviewPanel);

        PopulateReviewPage();
    }

    // =====================================
    // DISPLAY INDIVIDUAL REVIEW PAGE
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
            (entry.title ?? "").ToUpperInvariant();

        questionText.text =
            entry.question ?? "";

        submittedAnswerText.text =
            entry.submitted ?? "";

        expectedAnswerText.text =
            entry.expected ?? "";

        // =====================================
        // SELECT CORRECT ANSWER LAYOUT
        // =====================================

        bool isReport =
            entry.taskID == "INSPECTION_04" ||
            entry.taskID == "INSPECTION_09";

        bool isLongAnswer =
            !string.IsNullOrEmpty(entry.submitted) &&
            (
                entry.submitted.Length > 250 ||
                entry.submitted.Split('\n').Length > 8
            );

        bool useReportLayout =
            isReport || isLongAnswer;

        standardAnswerLayout.SetActive(
            !useReportLayout
        );

        longReportLayout.SetActive(
            useReportLayout
        );

        if (useReportLayout)
        {
            PopulateLongReport(
                entry.submitted,
                entry.expected
            );
        }

        // =====================================
        // COMPLETION-ONLY ACTIVITY
        // =====================================

        if (!entry.scored)
        {
            reviewStatusText.text =
                entry.completed

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
        // SCORED ANSWERS
        // =====================================

        else
        {
            reviewStatusText.text =
                entry.correct

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

                ? (
                    entry.correct

                    ? "Assessment criterion met."

                    : "Review the expected answer."
                  )

                : entry.improvement;
        }

        expectedAnswerText.color =
            Color.white;

        pageText.text =
            (currentPage + 1) +
            " / " +
            entries.Count;

        SetNavigationButton(
            previousButton,
            currentPage > 0
        );

        SetNavigationButton(
            nextButton,
            currentPage < entries.Count - 1
        );

        Debug.Log(
            "[ASSESSMENT] Review page " +
            (currentPage + 1) +
            " / " +
            entries.Count +
            " | " +
            entry.title
        );
    }

    // =====================================
    // NEW: LONG REPORT DISPLAY
    // =====================================

    private void PopulateLongReport(
        string submitted,
        string expected)
    {
        string content =
            submitted ?? "";

        content = content.Replace("\r", "");

        string[] rawLines =
            content.Split('\n');

        List<string> lines =
            new List<string>();

        // Remove unnecessary blank lines.
        // Preserve the recorded report contents.

        foreach (string rawLine in rawLines)
        {
            string line = rawLine.Trim();

            if (!string.IsNullOrWhiteSpace(line))
            {
                lines.Add(line);
            }
        }

        int midpoint =
            Mathf.CeilToInt(lines.Count / 2f);

        // Try to split at an existing report section.

        for (int i = 1; i < lines.Count; i++)
        {
            string upper =
                lines[i].ToUpperInvariant();

            if (upper == "OPERATING CONDITIONS" ||
                upper == "OPERATING CONDITIONS:")
            {
                midpoint = i;
                break;
            }
        }

        StringBuilder left =
            new StringBuilder();

        StringBuilder right =
            new StringBuilder();

        for (int i = 0; i < lines.Count; i++)
        {
            if (i < midpoint)
            {
                left.AppendLine(lines[i]);
            }
            else
            {
                right.AppendLine(lines[i]);
            }
        }

        reportLeftText.text =
            left.ToString().TrimEnd();

        reportRightText.text =
            right.ToString().TrimEnd();

        reportRequirementText.text =
            expected ?? "";

        // Check whether additional space
        // is needed for unusually long reports.

        reportLeftText.ForceMeshUpdate();
        reportRightText.ForceMeshUpdate();

        if (reportLeftText.isTextOverflowing ||
            reportRightText.isTextOverflowing)
        {
            Debug.LogWarning(
                "[ASSESSMENT] Long report exceeds " +
                "the review layout. Full data remains " +
                "stored in the assessment manager."
            );
        }
    }

    // =====================================
    // ENABLE OR DISABLE NAVIGATION
    // =====================================

    private void SetNavigationButton(
        Button button,
        bool available)
    {
        if (button == null)
            return;

        button.interactable = available;

        BoxCollider collider =
            button.GetComponent<BoxCollider>();

        if (collider != null)
        {
            collider.enabled = available;
        }

        Image image =
            button.GetComponent<Image>();

        if (image != null)
        {
            image.color = available

                ? new Color32(65, 65, 65, 255)

                : new Color32(35, 35, 35, 255);
        }

        TextMeshProUGUI label =
            button.GetComponentInChildren<
                TextMeshProUGUI
            >(true);

        if (label != null)
        {
            label.color = available

                ? Color.white

                : grey;
        }
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

        if (!CanNavigate())
            return;

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

        if (!CanNavigate())
            return;

        // Always advance exactly one entry.

        currentPage++;

        PopulateReviewPage();
    }

    // =====================================
    // RETURN TO SUMMARY
    // =====================================

    public void ReturnToSummary()
    {
        if (!feedbackOpened ||
            experienceFinished ||
            reviewPanel == null ||
            !reviewPanel.activeInHierarchy)
        {
            return;
        }

        if (!CanNavigate())
            return;

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

        if (!CanNavigate())
            return;

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
        if (!experienceFinished ||
            completionPanel == null ||
            !completionPanel.activeInHierarchy)
        {
            return;
        }

        if (!CanNavigate())
            return;

        HideAllPanels();

        RestoreCursor();

        onExperienceFinished?.Invoke();
    }

    // =====================================
    // HIDE PREVIOUS INSPECTION PANELS
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

            if (summaryPanel.transform.IsChildOf(
                    panel.transform) ||
                reviewPanel.transform.IsChildOf(
                    panel.transform) ||
                completionPanel.transform.IsChildOf(
                    panel.transform))
            {
                continue;
            }

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
    // SHOW ONE PANEL AT A TIME
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
    // NEW: CAPTURE UPRIGHT PANEL POSITION
    // =====================================

    private void CaptureFeedbackPosition()
    {
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
        }

        if (playerCamera == null)
        {
            Debug.LogWarning(
                "[ASSESSMENT] Player camera not assigned."
            );

            return;
        }

        Transform cam =
            playerCamera.transform;

        // Ignore vertical camera movement.
        // Keep the panel upright.

        Vector3 forward =
            Vector3.ProjectOnPlane(
                cam.forward,
                Vector3.up
            );

        if (forward.sqrMagnitude < 0.001f)
        {
            forward = Vector3.ProjectOnPlane(
                cam.up,
                Vector3.up
            );
        }

        if (forward.sqrMagnitude < 0.001f)
        {
            forward = Vector3.forward;
        }

        forward.Normalize();

        feedbackPosition =
            cam.position +
            forward * distanceFromPlayer +
            Vector3.up * verticalOffset;

        feedbackRotation =
            Quaternion.LookRotation(
                forward,
                Vector3.up
            );

        feedbackPositionCaptured = true;
    }

    // =====================================
    // POSITION FEEDBACK PANEL
    // =====================================

    private void PositionPanel(GameObject panel)
    {
        if (panel == null)
            return;

        if (!feedbackPositionCaptured ||
            !Application.isPlaying)
        {
            CaptureFeedbackPosition();
        }

        if (!feedbackPositionCaptured)
            return;

        panel.transform.SetPositionAndRotation(
            feedbackPosition,
            feedbackRotation
        );

        panel.transform.localScale =
            Vector3.one * panelScale;

        Canvas canvas =
            panel.GetComponent<Canvas>();

        if (canvas != null)
        {
            canvas.worldCamera =
                playerCamera;
        }
    }

    // =====================================
    // CURSOR CONTROL
    // =====================================

    private void CaptureCursor()
    {
        if (cursorCaptured)
            return;

        oldCursorVisible =
            Cursor.visible;

        oldCursorLock =
            Cursor.lockState;

        cursorCaptured = true;

        Cursor.lockState =
            CursorLockMode.None;

        Cursor.visible = true;
    }

    private void RestoreCursor()
    {
        if (!cursorCaptured)
            return;

        Cursor.visible =
            oldCursorVisible;

        Cursor.lockState =
            oldCursorLock;

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
            Vector3.one * panelScale;

        Image image =
            obj.GetComponent<Image>();

        // Fully opaque background.

        image.color =
            new Color32(10, 10, 10, 255);

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

        rect.anchoredPosition =
            position;

        rect.sizeDelta = size;

        rect.localScale =
            Vector3.one;

        rect.localRotation =
            Quaternion.identity;

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

        text.fontSizeMin = 14;

        text.fontSizeMax = fontSize;

        // Prevent text from extending outside
        // its allocated answer area.

        text.overflowMode =
            TextOverflowModes.Truncate;

        text.margin =
            new Vector4(8, 4, 8, 4);

        return text;
    }

    // =====================================
    // CREATE BUTTON
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

        ColorBlock colours =
            button.colors;

        colours.normalColor = Color.white;

        colours.highlightedColor =
            yellow;

        colours.pressedColor =
            new Color32(255, 235, 155, 255);

        colours.disabledColor =
            new Color32(100, 100, 100, 255);

        button.colors = colours;

        MakeText(
            rect,
            "ButtonText",
            label,
            Vector2.zero,
            size - new Vector2(15, 10),
            28,
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

        // Preserve existing EZPZ interaction.

        rect.gameObject.AddComponent<
            InteractableGeneral
        >();

        return button;
    }
}
