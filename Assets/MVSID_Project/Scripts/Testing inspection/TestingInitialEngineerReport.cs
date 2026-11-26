
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public class TestingInitialEngineerReport : MonoBehaviour
{
    [Header("Camera")]
    public Camera playerCamera;
    public float distanceFromPlayer = 1.6f;

    [Header("Existing Testing Assessments")]
    public TestingVisualInspectionAssessment visualAssessment;
    public TestingMaterialLossAssessment materialAssessment;

    [Header("Existing Engineer Dialogue")]
    public TestingInspectionDialogue dialogue;

    [Header("Editable Report")]
    public GameObject reportRoot;
    public TextMeshProUGUI traineeReportText;
    public TextMeshProUGUI engineerInstructionText;
    public Button nextTaskButton;

    // ==========================================
    // NEW: RECORDED ASSESSMENT RESULTS
    // ==========================================

    [Header("Assessment Results")]

    [SerializeField]
    private bool reportCompleted;

    [SerializeField]
    private bool assessmentRecorded;

    [SerializeField]
    [TextArea(5, 10)]
    private string submittedReport = "";

    [Header("After Next Task")]
    public UnityEvent onNextTask;

    private bool reportOpen;
    private bool nextTaskStarted;

    public bool IsTaskCompleted
        => reportCompleted;

    public bool IsAssessmentRecorded
        => assessmentRecorded;

    public string SubmittedReport
        => submittedReport;

    private static readonly string[] observations =
    {
        "No visible damage",
        "Corrosion near the pipe support",
        "Cracking along the pipeline",
        "Active pipeline leakage"
    };

    // ==========================================
    // BUILD REPORT IN EDIT MODE
    // ==========================================

    [ContextMenu("Build Initial Engineer Report")]
    public void BuildReport()
    {
        if (Application.isPlaying)
        {
            Debug.LogWarning(
                "Stop Play Mode before building the report."
            );

            return;
        }

        if (reportRoot != null)
        {
#if UNITY_EDITOR
            Undo.DestroyObjectImmediate(reportRoot);
#else
            DestroyImmediate(reportRoot);
#endif
        }

        reportRoot = new GameObject(
            "Testing_InitialEngineerReport",
            typeof(RectTransform),
            typeof(Canvas),
            typeof(GraphicRaycaster)
        );

#if UNITY_EDITOR
        Undo.RegisterCreatedObjectUndo(
            reportRoot,
            "Build Initial Engineer Report"
        );
#endif

        Canvas canvas =
            reportRoot.GetComponent<Canvas>();

        canvas.renderMode = RenderMode.WorldSpace;
        canvas.worldCamera = playerCamera;
        canvas.overrideSorting = true;
        canvas.sortingOrder = 130;

        RectTransform root =
            reportRoot.GetComponent<RectTransform>();

        root.sizeDelta = new Vector2(1000, 620);
        root.localScale = Vector3.one * 0.0016f;

        // ======================================
        // MAIN REPORT PANEL
        // ======================================

        RectTransform panel = CreateRect(
            "MainPanel",
            reportRoot.transform,
            Vector2.zero,
            new Vector2(1000, 620)
        );

        AddImage(
            panel,
            new Color32(9, 9, 9, 240)
        );

        MakeText(
            "ReportTitle",
            panel,
            "INITIAL REPORT TO ENGINEER",
            new Vector2(0, 255),
            new Vector2(940, 70),
            39,
            Color.white
        );

        MakeText(
            "TaskNumber",
            panel,
            "TASK 04 - INITIAL INSPECTION REPORT",
            new Vector2(0, 195),
            new Vector2(920, 55),
            27,
            new Color32(255, 213, 79, 255)
        );

        // ======================================
        // REPORT CONTENT
        // ======================================

        RectTransform content = CreateRect(
            "ReportContent",
            panel,
            new Vector2(0, -10),
            new Vector2(930, 320)
        );

        AddImage(
            content,
            new Color32(30, 30, 30, 250)
        );

        MakeText(
            "TraineeHeading",
            content,
            "TRAINEE REPORT",
            new Vector2(-300, 119),
            new Vector2(270, 45),
            26,
            new Color32(255, 213, 79, 255)
        );

        traineeReportText = MakeText(
            "TraineeReportText",
            content,
            "Your submitted inspection findings will appear here.",
            new Vector2(0, 42),
            new Vector2(850, 95),
            27,
            Color.white
        );

        // ======================================
        // ENGINEER INSTRUCTIONS
        // ======================================

        MakeText(
            "EngineerHeading",
            content,
            "INSPECTION ENGINEER",
            new Vector2(-270, -50),
            new Vector2(370, 45),
            26,
            new Color32(255, 213, 79, 255)
        );

        engineerInstructionText = MakeText(
            "EngineerInstruction",
            content,
            "Report received. Check the current pipe " +
            "temperature and pressure using the available " +
            "instruments, then report your readings.",
            new Vector2(0, -116),
            new Vector2(850, 95),
            26,
            Color.white
        );

        // ======================================
        // NEXT TASK BUTTON
        // ======================================

        RectTransform buttonRect = CreateRect(
            "NextTaskButton",
            panel,
            new Vector2(305, -251),
            new Vector2(310, 70)
        );

        Image buttonImage = AddImage(
            buttonRect,
            new Color32(65, 65, 65, 255),
            true
        );

        nextTaskButton =
            buttonRect.gameObject.AddComponent<Button>();

        nextTaskButton.targetGraphic = buttonImage;

        nextTaskButton.transition =
            Selectable.Transition.ColorTint;

        ColorBlock colours =
            nextTaskButton.colors;

        colours.normalColor = Color.white;

        colours.highlightedColor =
            new Color32(255, 213, 79, 255);

        colours.pressedColor =
            new Color32(255, 235, 150, 255);

        nextTaskButton.colors = colours;

        MakeText(
            "NextTaskText",
            buttonRect,
            "NEXT TASK",
            Vector2.zero,
            new Vector2(285, 60),
            30,
            Color.white
        );

        BoxCollider collider =
            buttonRect.gameObject.AddComponent<
                BoxCollider
            >();

        collider.size =
            new Vector3(310, 70, 8);

        PositionReport();

        // Visible in Edit Mode for adjustment.

        reportRoot.SetActive(true);

#if UNITY_EDITOR
        EditorUtility.SetDirty(this);

        EditorSceneManager.MarkSceneDirty(
            gameObject.scene
        );
#endif

        Debug.Log(
            "[TESTING] Initial Engineer Report built."
        );
    }

    // ==========================================
    // UI RECT HELPER
    // ==========================================

    private RectTransform CreateRect(
        string name,
        Transform parent,
        Vector2 position,
        Vector2 size)
    {
        GameObject obj = new GameObject(
            name,
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

    // ==========================================
    // IMAGE HELPER
    // ==========================================

    private Image AddImage(
        RectTransform rect,
        Color colour,
        bool raycast = false)
    {
        Image image =
            rect.gameObject.AddComponent<Image>();

        image.color = colour;
        image.raycastTarget = raycast;

        return image;
    }

    // ==========================================
    // TEXT HELPER
    // ==========================================

    private TextMeshProUGUI MakeText(
        string name,
        Transform parent,
        string content,
        Vector2 position,
        Vector2 size,
        float fontSize,
        Color colour)
    {
        RectTransform rect = CreateRect(
            name,
            parent,
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

        text.enableWordWrapping = true;
        text.raycastTarget = false;

        return text;
    }

    // ==========================================
    // RUNTIME INITIALISATION
    // ==========================================

    private void Awake()
    {
        reportOpen = false;
        nextTaskStarted = false;
        reportCompleted = false;
        assessmentRecorded = false;

        submittedReport = "";

        if (reportRoot == null)
            return;

        if (nextTaskButton != null)
        {
            nextTaskButton.onClick.AddListener(
                NextTask
            );
        }

        reportRoot.SetActive(false);
    }

    // ==========================================
    // POSITION REPORT
    // ==========================================

    private void PositionReport()
    {
        if (reportRoot == null)
            return;

        if (playerCamera == null)
        {
            playerCamera = Camera.main;
        }

        if (playerCamera == null)
            return;

        Transform cam =
            playerCamera.transform;

        reportRoot.transform.position =
            cam.position +
            cam.forward * distanceFromPlayer;

        reportRoot.transform.rotation =
            cam.rotation;

        Canvas canvas =
            reportRoot.GetComponent<Canvas>();

        if (canvas != null)
        {
            canvas.worldCamera = playerCamera;
        }
    }

    // ==========================================
    // SHOW INITIAL ENGINEER REPORT
    // ==========================================

    // Called after material-loss submission.

    public void ShowReport()
    {
        if (reportCompleted ||
            reportOpen)
        {
            return;
        }

        if (reportRoot == null ||
            materialAssessment == null ||
            !materialAssessment.IsSubmitted)
        {
            Debug.LogWarning(
                "[TESTING] Report not ready or not built."
            );

            return;
        }

        if (visualAssessment == null ||
            !visualAssessment.IsSubmitted)
        {
            Debug.LogWarning(
                "[TESTING] Visual inspection " +
                "assessment has not been submitted."
            );

            return;
        }

        nextTaskStarted = false;
        reportOpen = true;

        // ======================================
        // RETRIEVE TRAINEE'S VISUAL OBSERVATION
        // ======================================

        string observation = "Not recorded";

        int index =
            visualAssessment.SelectedAnswer;

        if (index >= 0 &&
            index < observations.Length)
        {
            observation = observations[index];
        }

        // ======================================
        // GENERATE ACTUAL TRAINEE REPORT
        // ======================================

        // Do not replace incorrect answers
        // with the expected answers.

        submittedReport =
            "I inspected PIPE_0493 and recorded: " +
            observation + ".\n" +

            "Calculated material loss: " +
            materialAssessment.MaterialLoss
                .ToString("0.##") +
            " mm.\n" +

            "Calculated material loss percentage: " +
            materialAssessment.PercentageLoss
                .ToString("0.##") +
            "%.\n" +

            "Selected condition: " +
            materialAssessment.SelectedCondition +
            ".";

        if (traineeReportText != null)
        {
            traineeReportText.text =
                submittedReport;
        }

        // Neutral engineer response.
        // No immediate accuracy feedback.

        if (engineerInstructionText != null)
        {
            engineerInstructionText.text =
                "Report received. Check the current pipe " +
                "temperature and pressure using the available " +
                "instruments, then report your readings.";
        }

        PositionReport();

        reportRoot.SetActive(true);

        Debug.Log(
            "[TESTING] Initial Engineer Report displayed."
        );
    }

    // ==========================================
    // EXISTING DESKTOP INTERACTION
    // ==========================================

    private void Update()
    {
        if (!reportOpen ||
            nextTaskStarted ||
            playerCamera == null ||
            nextTaskButton == null ||
            Mouse.current == null)
        {
            return;
        }

        if (!Mouse.current.leftButton.wasPressedThisFrame)
            return;

        Vector2 mousePos =
            Mouse.current.position.ReadValue();

        if (Cursor.lockState ==
            CursorLockMode.Locked)
        {
            mousePos = new Vector2(
                Screen.width * 0.5f,
                Screen.height * 0.5f
            );
        }

        Ray ray =
            playerCamera.ScreenPointToRay(
                mousePos
            );

        BoxCollider box =
            nextTaskButton.GetComponent<
                BoxCollider
            >();

        if (box != null &&
            box.Raycast(
                ray,
                out RaycastHit hit,
                100f))
        {
            NextTask();
        }
    }

    // ==========================================
    // COMPLETE INITIAL REPORT
    // ==========================================

    public void NextTask()
    {
        if (!reportOpen ||
            nextTaskStarted ||
            reportCompleted)
        {
            return;
        }

        nextTaskStarted = true;
        reportOpen = false;

        reportCompleted = true;

        // ======================================
        // NEW: SAVE TASK 4 RESULTS
        // ======================================

        SaveAssessmentResults();

        // Hide report.

        if (reportRoot != null)
        {
            reportRoot.SetActive(false);
        }

        // Preserve existing engineer dialogue.

        if (dialogue != null)
        {
            dialogue.ShowEngineerMessage();
        }

        // Continue to instrument readings.

        onNextTask?.Invoke();

        Debug.Log(
            "[TESTING] Engineer requested instrument readings."
        );
    }

    // ==========================================
    // NEW: SAVE CENTRAL ASSESSMENT RESULTS
    // ==========================================

    public void SaveAssessmentResults()
    {
        if (!reportCompleted ||
            assessmentRecorded)
        {
            return;
        }

        TestingAssessmentManager manager =
            TestingAssessmentManager.Instance;

        if (manager == null ||
            !manager.SessionStarted ||
            manager.SessionCompleted)
        {
            Debug.LogWarning(
                "[ASSESSMENT] No active testing session. " +
                "Initial report retained locally."
            );

            return;
        }

        TestingAssessmentManager.TaskRecord task =
            manager.FindTask("INSPECTION_04");

        if (task == null)
        {
            Debug.LogWarning(
                "[ASSESSMENT] Task INSPECTION_04 " +
                "was not registered."
            );

            return;
        }

        if (task.completed)
        {
            Debug.LogWarning(
                "[ASSESSMENT] Task 4 already recorded."
            );

            return;
        }

        // ======================================
        // COMPLETION-ONLY ASSESSMENT
        // ======================================

        // The report uses answers already scored
        // in Tasks 2 and 3.
        //
        // Exclude Task 4 from the overall accuracy
        // calculation to prevent double scoring.

        task.contributesToScore = false;

        // Preserve the actual trainee report.
        // The boolean represents report completion,
        // not the accuracy of its contents.

        manager.RecordAnswer(
            "INSPECTION_04",

            "Submit initial inspection report to the engineer.",

            submittedReport,

            "Initial inspection findings communicated " +
            "to the engineer. Answer accuracy is assessed " +
            "separately in Tasks 2 and 3.",

            true,

            ""
        );

        manager.RecordTaskResult(
            "INSPECTION_04",

            1,

            1,

            0,

            true,

            ""
        );

        assessmentRecorded = true;

        Debug.Log(
            "[ASSESSMENT] Task 4 initial engineer " +
            "report recorded."
        );

        Debug.Log(
            "[ASSESSMENT] Task 4 is completion only " +
            "and excluded from accuracy scoring."
        );
    }

    // ==========================================
    // RESET REPORT
    // ==========================================

    public void ResetReport()
    {
        reportOpen = false;

        nextTaskStarted = false;

        reportCompleted = false;

        assessmentRecorded = false;

        submittedReport = "";

        if (Application.isPlaying)
        {
            TestingAssessmentManager manager =
                TestingAssessmentManager.Instance;

            if (manager != null &&
                manager.SessionStarted &&
                !manager.SessionCompleted)
            {
                TestingAssessmentManager.TaskRecord task =
                    manager.FindTask("INSPECTION_04");

                if (task != null)
                {
                    task.answers.Clear();

                    task.mistakeDetails.Clear();

                    task.correctAnswers = 0;
                    task.totalAnswers = 0;
                    task.incorrectAttempts = 0;

                    task.completed = false;

                    task.contributesToScore = false;

                    task.improvement = "";
                }
            }
        }

        if (traineeReportText != null)
        {
            traineeReportText.text =
                "Your submitted inspection findings " +
                "will appear here.";
        }

        if (reportRoot != null)
        {
            reportRoot.SetActive(false);
        }

        Debug.Log(
            "[ASSESSMENT] Initial engineer report reset."
        );
    }
}
