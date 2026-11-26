
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.InputSystem;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public class TestingFinalInspectionReport : MonoBehaviour
{
    [Header("Existing Assessment Systems")]
    public TestingVisualInspectionAssessment visualAssessment;
    public TestingMaterialLossAssessment materialAssessment;
    public TestingInstrumentReadingsReport instrumentReport;
    public TestingPressureReduction pressureReduction;
    public TestingPressureReductionReport pressureReport;
    public TestingCorrosionRiskVisualisation riskVisualisation;
    public TestingInspectionDialogue dialogue;

    [Header("Camera")]
    public Camera playerCamera;
    public float distanceFromPlayer = 1.6f;

    [Header("Final Report Panel")]
    public GameObject reportPanel;
    public TextMeshProUGUI traineeNameText;
    public TextMeshProUGUI findingsText;
    public TextMeshProUGUI measurementsText;
    public TextMeshProUGUI activitiesText;
    public Button submitButton;

    [Header("Previous Panels")]
    public GameObject[] panelsToHide;

    [Header("Assessment Results")]
    [SerializeField] private bool reportOpen;
    [SerializeField] private bool taskCompleted;
    [SerializeField] private bool assessmentRecorded;

    [SerializeField]
    [TextArea(8, 15)]
    private string submittedReport = "";

    [Header("After Final Submission")]
    public UnityEvent onFinalReportSubmitted;

    private bool cursorCaptured;
    private bool previousCursorVisible;
    private CursorLockMode previousCursorLock;

    private readonly Color yellow =
        new Color32(255, 213, 79, 255);

    public bool IsTaskCompleted => taskCompleted;

    public bool IsAssessmentRecorded => assessmentRecorded;

    public string SubmittedReport => submittedReport;

    // =====================================
    // BUILD FINAL REPORT PANEL
    // =====================================

    [ContextMenu("Build Final Inspection Report")]
    public void BuildPanel()
    {
        if (Application.isPlaying)
        {
            Debug.LogWarning("Stop Play Mode first.");
            return;
        }

#if UNITY_EDITOR
        Undo.RecordObject(this, "Build Final Report");

        if (reportPanel != null)
            Undo.DestroyObjectImmediate(reportPanel);
#endif

        reportPanel = new GameObject(
            "Testing_FinalInspectionReport_Form",
            typeof(RectTransform),
            typeof(Canvas),
            typeof(GraphicRaycaster)
        );

#if UNITY_EDITOR
        Undo.RegisterCreatedObjectUndo(
            reportPanel,
            "Build Final Inspection Report"
        );
#endif

        Canvas canvas = reportPanel.GetComponent<Canvas>();

        canvas.renderMode = RenderMode.WorldSpace;
        canvas.worldCamera = playerCamera;
        canvas.overrideSorting = true;
        canvas.sortingOrder = 190;

        RectTransform root =
            reportPanel.GetComponent<RectTransform>();

        root.sizeDelta = new Vector2(1100, 850);
        root.localScale = Vector3.one * 0.0017f;

        RectTransform main = CreateRect(
            "MainPanel", root,
            Vector2.zero,
            new Vector2(1100, 850)
        );

        AddImage(main, new Color32(10, 10, 10, 245));

        MakeText(
            "Title", main,
            "FINAL INSPECTION REPORT",
            new Vector2(0, 370),
            new Vector2(1040, 65),
            38, Color.white
        );

        MakeText(
            "TaskTitle", main,
            "TASK 09 - REVIEW AND SUBMIT FINDINGS",
            new Vector2(0, 310),
            new Vector2(1040, 50),
            28, yellow
        );

        traineeNameText = MakeText(
            "TraineeName", main,
            "Trainee: --",
            new Vector2(0, 260),
            new Vector2(1000, 40),
            24, Color.white
        );

        MakeText(
            "PipelineInfo", main,
            "PIPE_0493  |  CARBON STEEL",
            new Vector2(0, 215),
            new Vector2(1000, 45),
            27, yellow
        );

        // LEFT COLUMN

        RectTransform left = CreateRect(
            "InspectionFindings",
            main,
            new Vector2(-260, 0),
            new Vector2(490, 360)
        );

        AddImage(left, new Color32(30, 30, 30, 255));

        MakeText(
            "FindingsHeading", left,
            "INSPECTION FINDINGS",
            new Vector2(0, 145),
            new Vector2(460, 45),
            26, yellow
        );

        findingsText = MakeText(
            "FindingsText", left,
            "Submitted inspection findings",
            new Vector2(0, -20),
            new Vector2(440, 260),
            27, Color.white
        );

        findingsText.alignment =
            TextAlignmentOptions.MidlineLeft;

        // RIGHT COLUMN

        RectTransform right = CreateRect(
            "OperatingConditions",
            main,
            new Vector2(260, 0),
            new Vector2(490, 360)
        );

        AddImage(right, new Color32(30, 30, 30, 255));

        MakeText(
            "MeasurementsHeading", right,
            "OPERATING CONDITIONS",
            new Vector2(0, 145),
            new Vector2(460, 45),
            26, yellow
        );

        measurementsText = MakeText(
            "MeasurementsText", right,
            "Submitted instrument readings",
            new Vector2(0, -20),
            new Vector2(440, 260),
            27, Color.white
        );

        measurementsText.alignment =
            TextAlignmentOptions.MidlineLeft;

        // COMPLETED ACTIVITIES

        activitiesText = MakeText(
            "ActivitiesText", main,
            "Completed inspection activities",
            new Vector2(0, -245),
            new Vector2(1000, 105),
            25, Color.white
        );

        submitButton = CreateButton(
            "SubmitFinalReportButton",
            main,
            "SUBMIT FINAL REPORT",
            new Vector2(0, -355),
            new Vector2(550, 80)
        );

        // Automatically find controllers on
        // TestingHUDCanvas where possible.

        if (visualAssessment == null)
            visualAssessment =
                GetComponent<TestingVisualInspectionAssessment>();

        if (materialAssessment == null)
            materialAssessment =
                GetComponent<TestingMaterialLossAssessment>();

        if (instrumentReport == null)
            instrumentReport =
                GetComponent<TestingInstrumentReadingsReport>();

        if (pressureReduction == null)
            pressureReduction =
                GetComponent<TestingPressureReduction>();

        if (pressureReport == null)
            pressureReport =
                GetComponent<TestingPressureReductionReport>();

        if (riskVisualisation == null)
            riskVisualisation =
                GetComponent<TestingCorrosionRiskVisualisation>();

        if (dialogue == null)
            dialogue =
                GetComponent<TestingInspectionDialogue>();

        PositionPanel();

        reportPanel.SetActive(true);

#if UNITY_EDITOR
        EditorUtility.SetDirty(this);
        EditorSceneManager.MarkSceneDirty(gameObject.scene);
#endif

        Debug.Log("[TESTING] Final inspection report built.");
    }

    // =====================================
    // RUNTIME SETUP
    // =====================================

    private void Awake()
    {
        reportOpen = false;
        taskCompleted = false;
        assessmentRecorded = false;
        submittedReport = "";

        if (reportPanel != null)
            reportPanel.SetActive(false);

        if (submitButton != null)
        {
            submitButton.onClick.AddListener(SubmitFinalReport);

            InteractableGeneral ezpz =
                submitButton.GetComponent<InteractableGeneral>();

            if (ezpz != null)
                ezpz.onPrimaryInteract.AddListener(
                    SubmitFinalReport
                );
        }
    }

    // =====================================
    // CHECK PREVIOUS TASKS
    // =====================================

    private bool PreviousTasksCompleted()
    {
        return visualAssessment != null &&
               visualAssessment.IsSubmitted &&

               materialAssessment != null &&
               materialAssessment.IsSubmitted &&

               instrumentReport != null &&
               instrumentReport.IsSubmitted &&

               pressureReduction != null &&
               pressureReduction.IsReductionCompleted &&

               pressureReport != null &&
               pressureReport.IsTaskCompleted &&

               riskVisualisation != null &&
               riskVisualisation.IsTaskCompleted;
    }

    // =====================================
    // OPEN FINAL REPORT
    // =====================================

    public void ShowFinalReport()
    {
        if (reportOpen || taskCompleted)
            return;

        if (!PreviousTasksCompleted())
        {
            Debug.LogWarning(
                "[TESTING] Previous inspection tasks " +
                "are not complete or references are missing."
            );
            return;
        }

        if (reportPanel == null ||
            findingsText == null ||
            measurementsText == null ||
            activitiesText == null ||
            submitButton == null)
        {
            Debug.LogError(
                "[TESTING] Build and assign the final report first."
            );
            return;
        }

        if (panelsToHide != null)
        {
            foreach (GameObject panel in panelsToHide)
            {
                if (panel == null ||
                    panel == gameObject ||
                    panel == reportPanel)
                    continue;

                if (transform.IsChildOf(panel.transform) ||
                    reportPanel.transform.IsChildOf(panel.transform))
                    continue;

                panel.SetActive(false);
            }
        }

        TestingAssessmentManager manager =
            TestingAssessmentManager.Instance;

        string traineeName = manager != null
            ? manager.TraineeName
            : "Trainee";

        if (traineeNameText != null)
            traineeNameText.text = "Trainee: " + traineeName;

        // Actual answers submitted by the trainee.

        string findings =
            "Visual observation:\n" +
            visualAssessment.SubmittedAnswer +

            "\n\nMaterial loss: " +
            FormatNumber(materialAssessment.MaterialLoss) +
            " mm" +

            "\nPercentage loss: " +
            FormatNumber(materialAssessment.PercentageLoss) +
            "%" +

            "\nReported condition: " +
            materialAssessment.SelectedCondition;

        string measurements =
            "Initial pressure:\n" +
            FormatNumber(instrumentReport.SubmittedPressure) +
            " psi" +

            "\n\nOperating temperature:\n" +
            FormatNumber(instrumentReport.SubmittedTemperature) +
            " °C" +

            "\n\nUpdated pressure:\n" +
            FormatNumber(pressureReport.SubmittedPressure) +
            " psi";

        string activities =
            "Simulated pressure-control operation: Completed\n" +
            "Corrosion consequence demonstration: Completed";

        findingsText.text = findings;
        measurementsText.text = measurements;
        activitiesText.text = activities;

        submittedReport =
            "Trainee: " + traineeName +
            "\nPipeline: PIPE_0493" +
            "\nMaterial: Carbon Steel" +
            "\n\nINSPECTION FINDINGS\n" +
            findings +
            "\n\nOPERATING CONDITIONS\n" +
            measurements +
            "\n\nCOMPLETED ACTIVITIES\n" +
            activities;

        PositionPanel();

        previousCursorVisible = Cursor.visible;
        previousCursorLock = Cursor.lockState;
        cursorCaptured = true;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        reportOpen = true;
        reportPanel.SetActive(true);

        Debug.Log("[TESTING] Final inspection report opened.");
    }

    // =====================================
    // SUBMIT FINAL REPORT
    // =====================================

    public void SubmitFinalReport()
    {
        if (!reportOpen || taskCompleted)
            return;

        TestingAssessmentManager manager =
            TestingAssessmentManager.Instance;

        if (manager == null ||
            !manager.SessionStarted ||
            manager.SessionCompleted)
        {
            Debug.LogWarning(
                "[ASSESSMENT] An active session is required."
            );
            return;
        }

        // Make sure no earlier assessment
        // records are missing.

        foreach (var task in manager.Tasks)
        {
            if (task.taskID != "INSPECTION_09" &&
                !task.completed)
            {
                Debug.LogWarning(
                    "[ASSESSMENT] Missing task: " +
                    task.taskID
                );
                return;
            }
        }

        TestingAssessmentManager.TaskRecord finalTask =
            manager.FindTask("INSPECTION_09");

        if (finalTask == null)
        {
            Debug.LogError(
                "[ASSESSMENT] INSPECTION_09 not registered."
            );
            return;
        }

        taskCompleted = true;

        // Completion only: do not score the same
        // findings twice.

        finalTask.contributesToScore = false;

        if (!finalTask.completed)
        {
            manager.RecordAnswer(
                "INSPECTION_09",

                "Submit the final inspection report.",

                submittedReport,

                "Consolidate and submit all recorded " +
                "inspection findings.",

                true,

                ""
            );

            manager.RecordTaskResult(
                "INSPECTION_09",
                1,
                1,
                0,
                true,
                ""
            );
        }

        assessmentRecorded = finalTask.completed;

        if (!assessmentRecorded)
        {
            taskCompleted = false;
            return;
        }

        ClosePanel();

        // Freeze session duration before
        // opening the feedback screens.

        manager.FinishSession();

        if (dialogue != null)
        {
            dialogue.ShowMessage(
                "INSPECTION ENGINEER",

                "Your final inspection report has been " +
                "received. You have completed the testing " +
                "activities. Review your assessment results."
            );
        }

        Debug.Log(
            "[ASSESSMENT] Final inspection report submitted."
        );

        onFinalReportSubmitted?.Invoke();
    }

    // =====================================
    // DESKTOP MOUSE SUPPORT
    // =====================================

    private void Update()
    {
        if (!reportOpen ||
            playerCamera == null ||
            submitButton == null ||
            Mouse.current == null)
            return;

        if (!Mouse.current.leftButton.wasPressedThisFrame)
            return;

        Ray ray = playerCamera.ScreenPointToRay(
            Mouse.current.position.ReadValue()
        );

        BoxCollider box =
            submitButton.GetComponent<BoxCollider>();

        if (box != null &&
            box.Raycast(ray, out RaycastHit hit, 100f))
        {
            SubmitFinalReport();
        }
    }

    // =====================================
    // POSITION PANEL
    // =====================================

    private void PositionPanel()
    {
        if (reportPanel == null)
            return;

        if (playerCamera == null)
            playerCamera = Camera.main;

        if (playerCamera == null)
            return;

        Transform cam = playerCamera.transform;

        reportPanel.transform.position =
            cam.position +
            cam.forward * distanceFromPlayer;

        reportPanel.transform.rotation =
            cam.rotation;

        Canvas canvas = reportPanel.GetComponent<Canvas>();

        if (canvas != null)
            canvas.worldCamera = playerCamera;
    }

    private void ClosePanel()
    {
        reportOpen = false;

        if (reportPanel != null &&
            reportPanel.activeSelf)
        {
            reportPanel.SetActive(false);
        }

        if (cursorCaptured)
        {
            Cursor.visible = previousCursorVisible;
            Cursor.lockState = previousCursorLock;
            cursorCaptured = false;
        }
    }

    private void OnDisable()
    {
        ClosePanel();
    }

    private string FormatNumber(float value)
    {
        return value.ToString(
            "0.########",
            CultureInfo.InvariantCulture
        );
    }

    // =====================================
    // UI HELPERS
    // =====================================

    private RectTransform CreateRect(
        string objectName,
        Transform parent,
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

        rect.anchorMin = Vector2.one * 0.5f;
        rect.anchorMax = Vector2.one * 0.5f;
        rect.pivot = Vector2.one * 0.5f;
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        rect.localRotation = Quaternion.identity;
        rect.localScale = Vector3.one;

        return rect;
    }

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

    private TextMeshProUGUI MakeText(
        string objectName,
        Transform parent,
        string content,
        Vector2 position,
        Vector2 size,
        float fontSize,
        Color colour)
    {
        RectTransform rect = CreateRect(
            objectName,
            parent,
            position,
            size
        );

        TextMeshProUGUI text =
            rect.gameObject.AddComponent<TextMeshProUGUI>();

        text.text = content;
        text.fontSize = fontSize;
        text.color = colour;
        text.alignment = TextAlignmentOptions.Center;
        text.raycastTarget = false;

        text.enableAutoSizing = true;
        text.fontSizeMin = 20;
        text.fontSizeMax = fontSize;

        return text;
    }

    private Button CreateButton(
        string objectName,
        Transform parent,
        string label,
        Vector2 position,
        Vector2 size)
    {
        RectTransform rect = CreateRect(
            objectName,
            parent,
            position,
            size
        );

        Image image = AddImage(
            rect,
            new Color32(65, 65, 65, 255),
            true
        );

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
            "ButtonText",
            rect,
            label,
            Vector2.zero,
            size - new Vector2(15, 10),
            29,
            Color.white
        );

        BoxCollider collider =
            rect.gameObject.AddComponent<BoxCollider>();

        collider.size = new Vector3(
            size.x,
            size.y,
            8
        );

        rect.gameObject.AddComponent<InteractableGeneral>();

        return button;
    }
}
