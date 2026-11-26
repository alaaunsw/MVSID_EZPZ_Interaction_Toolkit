
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

public class TestingInstrumentReadingsReport : MonoBehaviour
{
    [Header("Existing Systems")]
    public TestingPipelineMeasurements measurements;
    public TestingInspectionDialogue dialogue;

    [Header("Camera")]
    public Camera playerCamera;
    public float distanceFromPlayer = 1.6f;

    [Header("Measurement Form")]
    public GameObject reportPanel;
    public TMP_InputField pressureInput;
    public TMP_InputField temperatureInput;
    public Button submitButton;
    public TextMeshProUGUI statusText;

    // =====================================
    // NEW: EXPECTED INSTRUMENT READINGS
    // =====================================

    [Header("Expected Assessment Answers")]

    public float expectedPressure = 150f;

    public float expectedTemperature = 6.1f;

    [Min(0f)]
    public float pressureTolerance = 0.01f;

    [Min(0f)]
    public float temperatureTolerance = 0.1f;

    // =====================================
    // RECORDED ASSESSMENT RESULTS
    // =====================================

    [Header("Recorded Results")]

    [SerializeField]
    private float submittedPressure;

    [SerializeField]
    private float submittedTemperature;

    [SerializeField]
    private bool submitted;

    [SerializeField]
    private bool pressureCorrect;

    [SerializeField]
    private bool temperatureCorrect;

    [SerializeField]
    private int correctAnswerCount;

    [SerializeField]
    private bool assessmentRecorded;

    [Header("After Submission")]
    public UnityEvent onReadingsSubmitted;

    private bool reportOpen;

    private bool cursorWasVisible;

    private CursorLockMode previousCursorLock;

    private bool cursorStateCaptured;

    private bool warnedAboutEventSystem;

    // =====================================
    // PUBLIC ASSESSMENT RESULTS
    // =====================================

    public float SubmittedPressure
        => submittedPressure;

    public float SubmittedTemperature
        => submittedTemperature;

    public bool IsSubmitted
        => submitted;

    public bool IsPressureCorrect
        => submitted && pressureCorrect;

    public bool IsTemperatureCorrect
        => submitted && temperatureCorrect;

    public int CorrectAnswerCount
        => correctAnswerCount;

    public int TotalAnswers => 2;

    public bool IsAssessmentRecorded
        => assessmentRecorded;

    // =====================================
    // BUILD FORM IN EDIT MODE
    // =====================================

    [ContextMenu("Build Instrument Readings Form")]
    public void BuildForm()
    {
        if (Application.isPlaying)
        {
            Debug.LogWarning(
                "[TESTING] Stop Play Mode before building."
            );

            return;
        }

#if UNITY_EDITOR
        if (reportPanel != null)
        {
            Undo.DestroyObjectImmediate(reportPanel);
        }
#endif

        reportPanel = new GameObject(
            "Testing_InstrumentReadings_Form",
            typeof(RectTransform),
            typeof(Canvas),
            typeof(GraphicRaycaster)
        );

#if UNITY_EDITOR
        Undo.RegisterCreatedObjectUndo(
            reportPanel,
            "Build Instrument Readings Form"
        );
#endif

        Canvas canvas =
            reportPanel.GetComponent<Canvas>();

        canvas.renderMode = RenderMode.WorldSpace;

        canvas.worldCamera = playerCamera;

        canvas.overrideSorting = true;

        canvas.sortingOrder = 160;

        RectTransform root =
            reportPanel.GetComponent<RectTransform>();

        root.sizeDelta =
            new Vector2(950, 700);

        root.localScale =
            Vector3.one * 0.0017f;

        // =====================================
        // MAIN PANEL
        // =====================================

        RectTransform background = CreateRect(
            "MainPanel",
            reportPanel.transform,
            Vector2.zero,
            new Vector2(950, 700)
        );

        AddImage(
            background,
            new Color32(10, 10, 10, 235)
        );

        CreateText(
            "Title",
            background,
            "PIPELINE INSTRUMENT READINGS",
            new Vector2(0, 285),
            new Vector2(890, 65),
            37,
            Color.white
        );

        CreateText(
            "TaskTitle",
            background,
            "TASK 05 - RECORD MEASUREMENTS",
            new Vector2(0, 220),
            new Vector2(850, 55),
            29,
            new Color32(255, 213, 79, 255)
        );

        CreateText(
            "Instructions",
            background,
            "Enter the pressure and temperature readings " +
            "observed on PIPE_0493.",
            new Vector2(0, 145),
            new Vector2(860, 80),
            27,
            Color.white
        );

        // =====================================
        // PRESSURE INPUT
        // =====================================

        CreateText(
            "PressureLabel",
            background,
            "PRESSURE READING (psi)",
            new Vector2(0, 60),
            new Vector2(820, 45),
            29,
            Color.white
        );

        pressureInput = CreateInput(
            "PressureInput",
            background,
            new Vector2(0, -5),
            "Enter pressure"
        );

        // =====================================
        // TEMPERATURE INPUT
        // =====================================

        CreateText(
            "TemperatureLabel",
            background,
            "TEMPERATURE READING (C)",
            new Vector2(0, -90),
            new Vector2(820, 45),
            29,
            Color.white
        );

        temperatureInput = CreateInput(
            "TemperatureInput",
            background,
            new Vector2(0, -155),
            "Enter temperature"
        );

        // =====================================
        // SUBMIT BUTTON
        // =====================================

        submitButton = CreateButton(
            "SubmitReadingsButton",
            background,
            "SUBMIT READINGS",
            new Vector2(0, -255),
            new Vector2(460, 75)
        );

        statusText = CreateText(
            "StatusText",
            background,
            "",
            new Vector2(0, -315),
            new Vector2(850, 45),
            22,
            new Color32(255, 213, 79, 255)
        );

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
            "[TESTING] Instrument readings form built."
        );
    }

    // =====================================
    // UI RECT HELPER
    // =====================================

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

        rect.anchorMin = Vector2.one * 0.5f;

        rect.anchorMax = Vector2.one * 0.5f;

        rect.pivot = Vector2.one * 0.5f;

        rect.anchoredPosition = position;

        rect.sizeDelta = size;

        rect.localRotation = Quaternion.identity;

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
        string name,
        Transform parent,
        string value,
        Vector2 position,
        Vector2 size,
        float fontSize,
        Color color)
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

        text.text = value;

        text.fontSize = fontSize;

        text.color = color;

        text.alignment =
            TextAlignmentOptions.Center;

        text.enableWordWrapping = true;

        text.raycastTarget = false;

        return text;
    }

    // =====================================
    // INPUT FIELD CREATION
    // =====================================

    private TMP_InputField CreateInput(
        string name,
        Transform parent,
        Vector2 position,
        string placeholder)
    {
        RectTransform root = CreateRect(
            name,
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

        BoxCollider collider =
            root.gameObject.AddComponent<BoxCollider>();

        collider.size =
            new Vector3(650, 75, 8);

        return input;
    }

    // =====================================
    // BUTTON CREATION
    // =====================================

    private Button CreateButton(
        string name,
        Transform parent,
        string label,
        Vector2 position,
        Vector2 size)
    {
        RectTransform rect = CreateRect(
            name,
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

        colours.normalColor =
            Color.white;

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

        BoxCollider collider =
            rect.gameObject.AddComponent<BoxCollider>();

        collider.size =
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
        submitted = false;

        submittedPressure = 0f;

        submittedTemperature = 0f;

        pressureCorrect = false;

        temperatureCorrect = false;

        correctAnswerCount = 0;

        assessmentRecorded = false;

        reportOpen = false;

        if (reportPanel != null)
        {
            reportPanel.SetActive(false);
        }

        if (submitButton != null)
        {
            submitButton.onClick.AddListener(
                SubmitReadings
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
    // SHOW REPORT
    // =====================================

    public void ShowReport()
    {
        if (submitted || reportOpen)
            return;

        // Preserve the existing requirement
        // to inspect both instruments.

        if (measurements != null &&
            (!measurements.PressureInspected ||
             !measurements.TemperatureInspected))
        {
            Debug.LogWarning(
                "[TESTING] Inspect both instruments first."
            );

            return;
        }

        if (reportPanel == null)
        {
            Debug.LogError(
                "[TESTING] Build the instrument form first."
            );

            return;
        }

        PositionForm();

        reportPanel.SetActive(true);

        reportOpen = true;

        cursorWasVisible = Cursor.visible;

        previousCursorLock = Cursor.lockState;

        cursorStateCaptured = true;

        Cursor.lockState = CursorLockMode.None;

        Cursor.visible = true;

        if (statusText != null)
        {
            statusText.text = "";
        }

        if (EventSystem.current == null &&
            !warnedAboutEventSystem)
        {
            warnedAboutEventSystem = true;

            Debug.LogWarning(
                "[TESTING] No EventSystem found. " +
                "TMP keyboard input needs an EventSystem."
            );
        }

        // Preserve existing engineer instructions.

        if (dialogue != null)
        {
            dialogue.ShowMessage(
                "INSPECTION ENGINEER",

                "You have checked both instruments. " +
                "Enter the observed pressure and temperature " +
                "readings, then submit your findings."
            );
        }

        Debug.Log(
            "[TESTING] Instrument readings form opened."
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

        if (HitInput(pressureInput, ray))
        {
            FocusInput(pressureInput);

            return;
        }

        if (HitInput(temperatureInput, ray))
        {
            FocusInput(temperatureInput);

            return;
        }

        if (submitButton != null)
        {
            BoxCollider box =
                submitButton.GetComponent<BoxCollider>();

            if (box != null &&
                box.Raycast(
                    ray,
                    out RaycastHit hit,
                    100f))
            {
                SubmitReadings();
            }
        }
    }

    // =====================================
    // INPUT RAYCAST
    // =====================================

    private bool HitInput(
        TMP_InputField input,
        Ray ray)
    {
        if (input == null)
            return false;

        BoxCollider box =
            input.GetComponent<BoxCollider>();

        return box != null &&
               box.Raycast(
                   ray,
                   out RaycastHit hit,
                   100f
               );
    }

    // =====================================
    // FOCUS INPUT
    // =====================================

    private void FocusInput(TMP_InputField input)
    {
        if (input == null)
            return;

        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(
                input.gameObject
            );
        }

        input.Select();

        input.ActivateInputField();
    }

    // =====================================
    // SUBMIT READINGS
    // =====================================

    public void SubmitReadings()
    {
        if (submitted || !reportOpen)
            return;

        if (pressureInput == null ||
            temperatureInput == null)
        {
            Debug.LogError(
                "[TESTING] Input references are missing."
            );

            return;
        }

        bool pressureValid = TryReadValue(
            pressureInput.text,
            out float pressure
        );

        bool temperatureValid = TryReadValue(
            temperatureInput.text,
            out float temperature
        );

        // Validate input format.
        // Do not reveal answer correctness.

        if (!pressureValid ||
            !temperatureValid ||
            pressure < 0f ||
            float.IsNaN(pressure) ||
            float.IsNaN(temperature) ||
            float.IsInfinity(pressure) ||
            float.IsInfinity(temperature))
        {
            if (statusText != null)
            {
                statusText.text =
                    "Enter valid pressure and temperature readings.";
            }

            return;
        }

        // Store actual trainee submissions.

        submittedPressure = pressure;

        submittedTemperature = temperature;

        submitted = true;

        // =====================================
        // NEW: EVALUATE INSTRUMENT READINGS
        // =====================================

        EvaluateReadings();

        // Save before progressing to Task 6.

        SaveAssessmentResults();

        Debug.Log(
            "[TESTING] Readings submitted: " +
            submittedPressure +
            " psi, " +
            submittedTemperature +
            " C."
        );

        CloseForm();

        // Neutral engineer acknowledgement.
        // Do not reveal correct or incorrect answers.

        if (dialogue != null)
        {
            dialogue.ShowMessage(
                "INSPECTION ENGINEER",

                "Your instrument readings have been recorded. " +
                "Proceed to the next inspection activity."
            );
        }

        // Preserve existing task progression.

        onReadingsSubmitted?.Invoke();
    }

    // =====================================
    // NEW: EVALUATE READINGS INTERNALLY
    // =====================================

    private void EvaluateReadings()
    {
        const float precisionAllowance = 0.00001f;

        pressureCorrect =
            Mathf.Abs(
                submittedPressure -
                expectedPressure
            )
            <= pressureTolerance +
               precisionAllowance;

        temperatureCorrect =
            Mathf.Abs(
                submittedTemperature -
                expectedTemperature
            )
            <= temperatureTolerance +
               precisionAllowance;

        correctAnswerCount = 0;

        if (pressureCorrect)
        {
            correctAnswerCount++;
        }

        if (temperatureCorrect)
        {
            correctAnswerCount++;
        }
    }

    // =====================================
    // NEW: SAVE CENTRAL ASSESSMENT RESULTS
    // =====================================

    public void SaveAssessmentResults()
    {
        if (!submitted ||
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
                "Instrument readings retained locally."
            );

            return;
        }

        TestingAssessmentManager.TaskRecord task =
            manager.FindTask("INSPECTION_05");

        if (task == null)
        {
            Debug.LogWarning(
                "[ASSESSMENT] Task INSPECTION_05 " +
                "was not registered."
            );

            return;
        }

        if (task.completed)
        {
            Debug.LogWarning(
                "[ASSESSMENT] Task 5 already recorded."
            );

            return;
        }

        // =====================================
        // ANSWER 1: PRESSURE READING
        // =====================================

        manager.RecordAnswer(
            "INSPECTION_05",

            "Record the initial operating pressure " +
            "of PIPE_0493.",

            FormatNumber(submittedPressure) +
            " psi",

            FormatNumber(expectedPressure) +
            " psi",

            pressureCorrect,

            pressureCorrect

                ? ""

                : "Review the pressure gauge scale, " +
                  "needle position and measurement units " +
                  "before recording the pressure reading."
        );

        // =====================================
        // ANSWER 2: TEMPERATURE READING
        // =====================================

        manager.RecordAnswer(
            "INSPECTION_05",

            "Record the operating temperature " +
            "of PIPE_0493.",

            FormatNumber(submittedTemperature) +
            " °C",

            FormatNumber(expectedTemperature) +
            " °C",

            temperatureCorrect,

            temperatureCorrect

                ? ""

                : "Review the temperature instrument " +
                  "display and record the observed value " +
                  "using the correct measurement units."
        );

        // =====================================
        // COMBINED IMPROVEMENT RECOMMENDATIONS
        // =====================================

        string improvement = "";

        if (!pressureCorrect)
        {
            improvement +=
                "Practise accurately reading the " +
                "pressure gauge. ";
        }

        if (!temperatureCorrect)
        {
            improvement +=
                "Practise accurately reading the " +
                "temperature instrument.";
        }

        // =====================================
        // RECORD TASK COMPLETION
        // =====================================

        task.contributesToScore = true;

        manager.RecordTaskResult(
            "INSPECTION_05",

            correctAnswerCount,

            TotalAnswers,

            TotalAnswers - correctAnswerCount,

            true,

            improvement.Trim()
        );

        assessmentRecorded = true;

        Debug.Log(
            "[ASSESSMENT] Task 5 instrument " +
            "readings recorded."
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
    // READ NUMERIC INPUT
    // =====================================

    private bool TryReadValue(
        string input,
        out float value)
    {
        if (float.TryParse(
            input,
            NumberStyles.Float,
            CultureInfo.InvariantCulture,
            out value))
        {
            return true;
        }

        return float.TryParse(
            input,
            NumberStyles.Float,
            CultureInfo.CurrentCulture,
            out value
        );
    }

    // =====================================
    // CLOSE FORM
    // =====================================

    private void CloseForm()
    {
        reportOpen = false;

        if (reportPanel != null)
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
    // RESET REPORT
    // =====================================

    public void ResetReport()
    {
        submitted = false;

        submittedPressure = 0f;

        submittedTemperature = 0f;

        pressureCorrect = false;

        temperatureCorrect = false;

        correctAnswerCount = 0;

        assessmentRecorded = false;

        // Clear the corresponding central task.

        if (Application.isPlaying)
        {
            TestingAssessmentManager manager =
                TestingAssessmentManager.Instance;

            if (manager != null &&
                manager.SessionStarted &&
                !manager.SessionCompleted)
            {
                TestingAssessmentManager.TaskRecord task =
                    manager.FindTask("INSPECTION_05");

                if (task != null)
                {
                    task.answers.Clear();

                    task.mistakeDetails.Clear();

                    task.correctAnswers = 0;

                    task.totalAnswers = TotalAnswers;

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

        if (temperatureInput != null)
        {
            temperatureInput.SetTextWithoutNotify("");
        }

        if (statusText != null)
        {
            statusText.text = "";
        }

        CloseForm();

        Debug.Log(
            "[ASSESSMENT] Instrument readings report reset."
        );
    }
}
