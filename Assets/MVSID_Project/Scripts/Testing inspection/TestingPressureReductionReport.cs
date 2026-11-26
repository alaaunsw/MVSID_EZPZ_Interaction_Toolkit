
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public class TestingPressureReductionReport : MonoBehaviour
{
    [Header("Existing Systems")]
    public TestingInspectionDialogue dialogue;
    public TestingPressureReduction pressureReduction;

    [Header("Camera")]
    public Camera playerCamera;
    public float distanceFromPlayer = 1.6f;

    [Header("Task 7 Panel")]
    public GameObject reportPanel;
    public TMP_InputField pressureInput;
    public Button submitButton;

    [Header("Previous Panels")]
    public GameObject[] panelsToHide;

    // =====================================
    // NEW: ASSESSMENT SETTINGS
    // =====================================

    [Header("Assessment Settings")]

    [Min(0.0001f)]
    public float pressureTolerance = 0.01f;

    // =====================================
    // RECORDED ASSESSMENT RESULTS
    // =====================================

    [Header("Assessment Results")]

    [SerializeField]
    private bool taskStarted;

    [SerializeField]
    private bool taskCompleted;

    [SerializeField]
    private bool correctPressure;

    [SerializeField]
    private float submittedPressure;

    [SerializeField]
    private float expectedPressure;

    [SerializeField]
    private bool assessmentRecorded;

    [Header("After Report Submission")]
    public UnityEvent onReportSubmitted;

    private bool reportOpen;

    private bool cursorWasVisible;

    private CursorLockMode previousCursorLock;

    private bool cursorStateCaptured;

    // =====================================
    // PUBLIC ASSESSMENT RESULTS
    // =====================================

    public bool IsTaskCompleted
        => taskCompleted;

    public bool IsPressureCorrect
        => correctPressure;

    public float SubmittedPressure
        => submittedPressure;

    public float ExpectedPressure
        => expectedPressure;

    public bool IsAssessmentRecorded
        => assessmentRecorded;

    // =====================================
    // BUILD FORM IN EDIT MODE
    // =====================================

    [ContextMenu("Build Pressure Reduction Report Panel")]
    public void BuildForm()
    {
        if (Application.isPlaying)
        {
            Debug.LogWarning(
                "Stop Play Mode before building."
            );

            return;
        }

#if UNITY_EDITOR
        Undo.RecordObject(
            this,
            "Build Pressure Reduction Report"
        );

        if (reportPanel != null)
        {
            Undo.DestroyObjectImmediate(reportPanel);
        }
#endif

        reportPanel = new GameObject(
            "Testing_PressureReduction_Form",
            typeof(RectTransform),
            typeof(Canvas),
            typeof(GraphicRaycaster)
        );

#if UNITY_EDITOR
        Undo.RegisterCreatedObjectUndo(
            reportPanel,
            "Build Pressure Reduction Report"
        );
#endif

        Canvas canvas =
            reportPanel.GetComponent<Canvas>();

        canvas.renderMode = RenderMode.WorldSpace;

        canvas.worldCamera = playerCamera;

        canvas.overrideSorting = true;

        canvas.sortingOrder = 170;

        RectTransform root =
            reportPanel.GetComponent<RectTransform>();

        root.sizeDelta =
            new Vector2(950, 520);

        root.localScale =
            Vector3.one * 0.0017f;

        // =====================================
        // MAIN BACKGROUND
        // =====================================

        RectTransform background = CreateRect(
            "MainPanel",
            reportPanel.transform,
            Vector2.zero,
            new Vector2(950, 520)
        );

        AddImage(
            background,
            new Color32(10, 10, 10, 235)
        );

        // Main title.

        CreateText(
            "Title",
            background,
            "PRESSURE REDUCTION REPORT",
            new Vector2(0, 195),
            new Vector2(890, 65),
            37,
            Color.white
        );

        // Yellow task title.

        CreateText(
            "TaskTitle",
            background,
            "TASK 07 - REPORT UPDATED PRESSURE",
            new Vector2(0, 135),
            new Vector2(850, 55),
            29,
            new Color32(255, 213, 79, 255)
        );

        // Instructions.

        CreateText(
            "Instructions",
            background,
            "Enter the updated pressure reading " +
            "observed on PIPE_0493.",
            new Vector2(0, 65),
            new Vector2(860, 80),
            27,
            Color.white
        );

        // Pressure label.

        CreateText(
            "PressureLabel",
            background,
            "PRESSURE READING (psi)",
            new Vector2(0, -15),
            new Vector2(820, 45),
            29,
            Color.white
        );

        // Pressure input.

        pressureInput = CreateInput(
            "PressureInput",
            background,
            new Vector2(0, -80),
            "Enter updated pressure"
        );

        // Submit button.

        submitButton = CreateButton(
            "SubmitReportButton",
            background,
            "SUBMIT REPORT",
            new Vector2(0, -185),
            new Vector2(460, 75)
        );

        // Automatically find existing systems.

        if (dialogue == null)
        {
            dialogue =
                GetComponent<
                    TestingInspectionDialogue
                >();
        }

        if (pressureReduction == null)
        {
            pressureReduction =
                GetComponent<
                    TestingPressureReduction
                >();
        }

        PositionForm();

        // Visible in Edit Mode for adjustments.

        reportPanel.SetActive(true);

#if UNITY_EDITOR
        EditorUtility.SetDirty(this);

        EditorSceneManager.MarkSceneDirty(
            gameObject.scene
        );
#endif

        Debug.Log(
            "[TESTING] Pressure reduction report built."
        );
    }

    // =====================================
    // UI RECT HELPER
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

        rect.anchorMin =
            Vector2.one * 0.5f;

        rect.anchorMax =
            Vector2.one * 0.5f;

        rect.pivot =
            Vector2.one * 0.5f;

        rect.anchoredPosition = position;

        rect.sizeDelta = size;

        rect.localRotation =
            Quaternion.identity;

        rect.localScale = Vector3.one;

        return rect;
    }

    // =====================================
    // IMAGE HELPER
    // =====================================

    private Image AddImage(
        RectTransform rect,
        Color color,
        bool raycastTarget = false)
    {
        Image image =
            rect.gameObject.AddComponent<Image>();

        image.color = color;

        image.raycastTarget = raycastTarget;

        return image;
    }

    // =====================================
    // TEXT HELPER
    // =====================================

    private TextMeshProUGUI CreateText(
        string objectName,
        Transform parent,
        string value,
        Vector2 position,
        Vector2 size,
        float fontSize,
        Color color)
    {
        RectTransform rect = CreateRect(
            objectName,
            parent,
            position,
            size
        );

        TextMeshProUGUI text =
            rect.gameObject.AddComponent<
                TextMeshProUGUI
            >();

        text.text = value;

        text.fontSize = fontSize;

        text.color = color;

        text.alignment =
            TextAlignmentOptions.Center;

        text.raycastTarget = false;

        return text;
    }

    // =====================================
    // INPUT FIELD CREATION
    // =====================================

    private TMP_InputField CreateInput(
        string objectName,
        Transform parent,
        Vector2 position,
        string placeholder)
    {
        RectTransform root = CreateRect(
            objectName,
            parent,
            position,
            new Vector2(650, 75)
        );

        Image background = AddImage(
            root,
            new Color32(50, 50, 50, 255),
            true
        );

        TMP_InputField input =
            root.gameObject.AddComponent<
                TMP_InputField
            >();

        RectTransform viewport = CreateRect(
            "TextArea",
            root,
            Vector2.zero,
            new Vector2(615, 65)
        );

        TextMeshProUGUI inputText = CreateText(
            "InputText",
            viewport,
            "",
            Vector2.zero,
            new Vector2(600, 60),
            30,
            Color.white
        );

        inputText.alignment =
            TextAlignmentOptions.MidlineLeft;

        TextMeshProUGUI placeholderText = CreateText(
            "Placeholder",
            viewport,
            placeholder,
            Vector2.zero,
            new Vector2(600, 60),
            26,
            new Color32(175, 175, 175, 255)
        );

        placeholderText.alignment =
            TextAlignmentOptions.MidlineLeft;

        input.textViewport = viewport;

        input.textComponent = inputText;

        input.placeholder = placeholderText;

        input.targetGraphic = background;

        input.contentType =
            TMP_InputField.ContentType.DecimalNumber;

        input.lineType =
            TMP_InputField.LineType.SingleLine;

        input.characterLimit = 12;

        input.SetTextWithoutNotify("");

        BoxCollider box =
            root.gameObject.AddComponent<
                BoxCollider
            >();

        box.size =
            new Vector3(650, 75, 8);

        return input;
    }

    // =====================================
    // BUTTON CREATION
    // =====================================

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

        button.transition =
            Selectable.Transition.ColorTint;

        ColorBlock colours = button.colors;

        colours.normalColor = Color.white;

        colours.highlightedColor =
            new Color32(255, 213, 79, 255);

        colours.pressedColor =
            new Color32(255, 235, 155, 255);

        button.colors = colours;

        CreateText(
            "ButtonText",
            rect,
            label,
            Vector2.zero,
            size - new Vector2(15, 10),
            30,
            Color.white
        );

        BoxCollider box =
            rect.gameObject.AddComponent<
                BoxCollider
            >();

        box.size =
            new Vector3(
                size.x,
                size.y,
                8
            );

        return button;
    }

    // =====================================
    // RUNTIME SETUP
    // =====================================

    private void Awake()
    {
        taskStarted = false;

        taskCompleted = false;

        correctPressure = false;

        submittedPressure = 0f;

        expectedPressure = 0f;

        assessmentRecorded = false;

        reportOpen = false;

        if (reportPanel != null)
        {
            reportPanel.SetActive(false);
        }

        if (submitButton != null)
        {
            submitButton.onClick.AddListener(
                SubmitReport
            );
        }
    }

    // =====================================
    // POSITION FORM
    // =====================================

    private void PositionForm()
    {
        if (reportPanel == null)
            return;

        if (playerCamera == null)
        {
            playerCamera = Camera.main;
        }

        if (playerCamera == null)
            return;

        Transform cam =
            playerCamera.transform;

        reportPanel.transform.position =
            cam.position +
            cam.forward * distanceFromPlayer;

        reportPanel.transform.rotation =
            cam.rotation;

        Canvas canvas =
            reportPanel.GetComponent<Canvas>();

        if (canvas != null)
        {
            canvas.worldCamera = playerCamera;
        }
    }

    // =====================================
    // OPEN TASK 7
    // =====================================

    public void OpenReport()
    {
        if (taskStarted || taskCompleted)
            return;

        if (pressureReduction == null ||
            !pressureReduction.IsReductionCompleted)
        {
            Debug.LogWarning(
                "[TESTING] Complete pressure reduction first."
            );

            return;
        }

        if (reportPanel == null ||
            pressureInput == null)
        {
            Debug.LogError(
                "[TESTING] Build Task 7 report first."
            );

            return;
        }

        taskStarted = true;

        // Capture the actual pressure achieved
        // in the completed Task 6 simulation.

        expectedPressure =
            pressureReduction.AchievedPressure;

        // =====================================
        // HIDE PREVIOUS ASSESSMENT PANELS
        // =====================================

        if (panelsToHide != null)
        {
            foreach (GameObject panel in panelsToHide)
            {
                if (panel != null &&
                    panel != reportPanel &&
                    panel != gameObject)
                {
                    // Do not disable a parent containing
                    // this controller or the current report.

                    if (transform.IsChildOf(panel.transform) ||
                        reportPanel.transform.IsChildOf(
                            panel.transform
                        ))
                    {
                        continue;
                    }

                    panel.SetActive(false);
                }
            }
        }

        PositionForm();

        pressureInput.SetTextWithoutNotify("");

        reportPanel.SetActive(true);

        reportOpen = true;

        // Enable desktop mouse interaction.

        cursorWasVisible = Cursor.visible;

        previousCursorLock = Cursor.lockState;

        cursorStateCaptured = true;

        Cursor.lockState = CursorLockMode.None;

        Cursor.visible = true;

        Debug.Log(
            "[TESTING] Task 7 report opened."
        );
    }

    // =====================================
    // DESKTOP MOUSE SUPPORT
    // =====================================

    private void Update()
    {
        if (!reportOpen ||
            playerCamera == null ||
            Mouse.current == null)
        {
            return;
        }

        if (!Mouse.current.leftButton.wasPressedThisFrame)
            return;

        Vector2 mousePosition =
            Mouse.current.position.ReadValue();

        Ray ray =
            playerCamera.ScreenPointToRay(
                mousePosition
            );

        if (pressureInput != null)
        {
            BoxCollider inputBox =
                pressureInput.GetComponent<
                    BoxCollider
                >();

            if (inputBox != null &&
                inputBox.Raycast(
                    ray,
                    out RaycastHit inputHit,
                    100f))
            {
                FocusPressureInput();

                return;
            }
        }

        if (submitButton != null)
        {
            BoxCollider buttonBox =
                submitButton.GetComponent<
                    BoxCollider
                >();

            if (buttonBox != null &&
                buttonBox.Raycast(
                    ray,
                    out RaycastHit buttonHit,
                    100f))
            {
                SubmitReport();
            }
        }
    }

    // =====================================
    // FOCUS INPUT
    // =====================================

    // Can also be connected to EZPZ.

    public void FocusPressureInput()
    {
        if (!reportOpen ||
            pressureInput == null)
        {
            return;
        }

        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(
                pressureInput.gameObject
            );
        }

        pressureInput.Select();

        pressureInput.ActivateInputField();
    }

    // =====================================
    // SUBMIT REPORT
    // =====================================

    public void SubmitReport()
    {
        if (!taskStarted ||
            taskCompleted ||
            !reportOpen)
        {
            return;
        }

        if (pressureInput == null ||
            pressureReduction == null)
        {
            return;
        }

        bool valid = float.TryParse(
            pressureInput.text.Trim(),

            NumberStyles.Float,

            CultureInfo.InvariantCulture,

            out float pressure
        );

        if (!valid)
        {
            valid = float.TryParse(
                pressureInput.text.Trim(),

                NumberStyles.Float,

                CultureInfo.CurrentCulture,

                out pressure
            );
        }

        // Validate input format only.

        if (!valid ||
            pressure < 0f ||
            float.IsNaN(pressure) ||
            float.IsInfinity(pressure))
        {
            return;
        }

        submittedPressure = pressure;

        // =====================================
        // EVALUATE UPDATED PRESSURE INTERNALLY
        // =====================================

        correctPressure =
            Mathf.Abs(
                submittedPressure -
                expectedPressure
            ) < pressureTolerance;

        taskCompleted = true;

        // =====================================
        // NEW: SAVE ASSESSMENT RESULTS
        // =====================================

        SaveAssessmentResults();

        Debug.Log(
            "[TESTING] Task 7 submitted. Pressure: " +
            submittedPressure +
            " psi."
        );

        CloseForm();

        // =====================================
        // EXISTING ENGINEER RESPONSE
        // =====================================

        // Same response for all submitted answers.
        // No immediate assessment feedback.

        if (dialogue != null)
        {
            dialogue.ShowMessage(
                "INSPECTION ENGINEER",

                "Report received. Your updated pressure " +
                "reading has been recorded. Before completing " +
                "your final report, observe the potential " +
                "consequences of continued corrosion."
            );
        }

        // Preserve existing Task 8 progression.

        onReportSubmitted?.Invoke();
    }

    // =====================================
    // NEW: SAVE CENTRAL ASSESSMENT RESULTS
    // =====================================

    public void SaveAssessmentResults()
    {
        if (!taskCompleted ||
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
                "Updated pressure retained locally."
            );

            return;
        }

        TestingAssessmentManager.TaskRecord task =
            manager.FindTask("INSPECTION_07");

        if (task == null)
        {
            Debug.LogWarning(
                "[ASSESSMENT] Task INSPECTION_07 " +
                "was not registered."
            );

            return;
        }

        if (task.completed)
        {
            Debug.LogWarning(
                "[ASSESSMENT] Task 7 already recorded."
            );

            return;
        }

        string improvement =
            correctPressure

                ? ""

                : "Review the updated pressure gauge " +
                  "after the simulated pressure reduction. " +
                  "Check the needle position, gauge scale " +
                  "and measurement units before submitting " +
                  "the updated reading.";

        // =====================================
        // RECORD ACTUAL TRAINEE ANSWER
        // =====================================

        manager.RecordAnswer(
            "INSPECTION_07",

            "Report the updated operating pressure " +
            "of PIPE_0493 after the simulated reduction.",

            FormatNumber(submittedPressure) +
            " psi",

            FormatNumber(expectedPressure) +
            " psi",

            correctPressure,

            improvement
        );

        // =====================================
        // RECORD TASK COMPLETION AND ACCURACY
        // =====================================

        task.contributesToScore = true;

        manager.RecordTaskResult(
            "INSPECTION_07",

            correctPressure ? 1 : 0,

            1,

            correctPressure ? 0 : 1,

            true,

            improvement
        );

        assessmentRecorded = true;

        Debug.Log(
            "[ASSESSMENT] Task 7 updated pressure " +
            "report recorded."
        );
    }

    // =====================================
    // FORMAT NUMBERS FOR FINAL FEEDBACK
    // =====================================

    private string FormatNumber(float value)
    {
        return value.ToString(
            "0.#########",
            CultureInfo.InvariantCulture
        );
    }

    // =====================================
    // CLOSE FORM
    // =====================================

    private void CloseForm()
    {
        reportOpen = false;

        if (pressureInput != null)
        {
            pressureInput.DeactivateInputField();
        }

        if (reportPanel != null &&
            reportPanel.activeSelf)
        {
            reportPanel.SetActive(false);
        }

        if (cursorStateCaptured)
        {
            Cursor.lockState =
                previousCursorLock;

            Cursor.visible =
                cursorWasVisible;

            cursorStateCaptured = false;
        }
    }

    // =====================================
    // EXISTING CLEANUP
    // =====================================

    private void OnDisable()
    {
        CloseForm();
    }

    // =====================================
    // RESET TASK
    // =====================================

    public void ResetReport()
    {
        taskStarted = false;

        taskCompleted = false;

        correctPressure = false;

        submittedPressure = 0f;

        expectedPressure = 0f;

        assessmentRecorded = false;

        // Clear the corresponding central task
        // during an active development session.

        if (Application.isPlaying)
        {
            TestingAssessmentManager manager =
                TestingAssessmentManager.Instance;

            if (manager != null &&
                manager.SessionStarted &&
                !manager.SessionCompleted)
            {
                TestingAssessmentManager.TaskRecord task =
                    manager.FindTask("INSPECTION_07");

                if (task != null)
                {
                    task.answers.Clear();

                    task.mistakeDetails.Clear();

                    task.correctAnswers = 0;

                    task.totalAnswers = 1;

                    task.incorrectAttempts = 0;

                    task.completed = false;

                    task.improvement = "";
                }
            }
        }

        if (pressureInput != null)
        {
            pressureInput.SetTextWithoutNotify("");
        }

        CloseForm();

        Debug.Log(
            "[ASSESSMENT] Updated pressure report reset."
        );
    }
}
