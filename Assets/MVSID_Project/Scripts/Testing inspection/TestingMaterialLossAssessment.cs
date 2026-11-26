
using System;
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

public class TestingMaterialLossAssessment : MonoBehaviour
{
    [Header("Player Camera")]
    public Camera playerCamera;
    public float distanceFromPlayer = 1.6f;

    [Header("Editable Assessment Objects")]
    public GameObject assessmentRoot;
    public TMP_InputField lossInput;
    public TMP_InputField percentageInput;

    public Button normalButton;
    public Button moderateButton;
    public Button criticalButton;
    public Button submitButton;

    public TextMeshProUGUI statusText;

    [Header("Assessment Colours")]
    public Color panelColor =
        new Color32(8, 8, 8, 230);

    public Color normalColor =
        new Color32(120, 120, 120, 255);

    public Color highlightColor =
        new Color32(255, 213, 79, 255);

    // ======================================
    // NEW: EXPECTED ASSESSMENT ANSWERS
    // ======================================

    [Header("Expected Assessment Answers")]

    public float expectedMaterialLoss = 3.41f;

    public float expectedPercentageLoss = 30.69f;

    public string expectedCondition = "Moderate";

    [Min(0f)]
    public float materialLossTolerance = 0.01f;

    [Min(0f)]
    public float percentageTolerance = 0.1f;

    // ======================================
    // RECORDED RESULTS
    // ======================================

    [Header("Recorded Results")]

    [SerializeField] private float materialLoss;

    [SerializeField] private float percentageLoss;

    [SerializeField]
    private string selectedCondition = "";

    [SerializeField] private bool submitted;

    [SerializeField] private bool materialLossCorrect;

    [SerializeField] private bool percentageLossCorrect;

    [SerializeField] private bool conditionCorrect;

    [SerializeField] private int correctAnswerCount;

    [SerializeField] private bool assessmentRecorded;

    [Header("After Submission")]
    public UnityEvent onAnswerSubmitted;

    private Image[] conditionImages = new Image[3];

    private Button[] conditionButtons;

    private bool assessmentOpen;

    private bool cursorWasVisible;

    private CursorLockMode previousLockMode;

    // ======================================
    // PUBLIC ASSESSMENT RESULTS
    // ======================================

    public float MaterialLoss => materialLoss;

    public float PercentageLoss => percentageLoss;

    public string SelectedCondition
        => selectedCondition;

    public bool IsSubmitted => submitted;

    public bool IsMaterialLossCorrect
        => submitted && materialLossCorrect;

    public bool IsPercentageLossCorrect
        => submitted && percentageLossCorrect;

    public bool IsConditionCorrect
        => submitted && conditionCorrect;

    public int CorrectAnswerCount
        => correctAnswerCount;

    public int TotalAnswers => 3;

    public bool IsAssessmentRecorded
        => assessmentRecorded;

    // ======================================
    // BUILD EDITABLE ASSESSMENT
    // ======================================

    [ContextMenu("Build Material Loss Assessment")]
    public void BuildAssessment()
    {
        if (Application.isPlaying)
        {
            Debug.LogWarning(
                "Stop Play Mode before building."
            );

            return;
        }

#if UNITY_EDITOR
        if (assessmentRoot != null)
            Undo.DestroyObjectImmediate(
                assessmentRoot
            );
#endif

        assessmentRoot = new GameObject(
            "Testing_MaterialLoss_Assessment",
            typeof(RectTransform),
            typeof(Canvas),
            typeof(GraphicRaycaster)
        );

#if UNITY_EDITOR
        Undo.RegisterCreatedObjectUndo(
            assessmentRoot,
            "Build Material Loss Assessment"
        );
#endif

        Canvas canvas =
            assessmentRoot.GetComponent<Canvas>();

        canvas.renderMode = RenderMode.WorldSpace;

        canvas.worldCamera = playerCamera;

        canvas.overrideSorting = true;

        canvas.sortingOrder = 120;

        RectTransform root =
            assessmentRoot.GetComponent<RectTransform>();

        root.sizeDelta =
            new Vector2(1050, 750);

        root.localScale =
            Vector3.one * 0.0017f;

        RectTransform panel = MakeRect(
            "MainPanel",
            assessmentRoot.transform,
            Vector2.zero,
            new Vector2(1050, 750)
        );

        MakeImage(panel, panelColor);

        // ======================================
        // TITLE
        // ======================================

        MakeText(
            "Title",
            panel,
            "PIPELINE MATERIAL LOSS ASSESSMENT",
            new Vector2(0, 320),
            new Vector2(990, 65),
            34,
            Color.white
        );

        MakeText(
            "TaskTitle",
            panel,
            "TASK 02 - CALCULATE MATERIAL LOSS",
            new Vector2(0, 251),
            new Vector2(930, 50),
            29,
            highlightColor
        );

        MakeText(
            "Instructions",
            panel,
            "Review the PIPE_0493 information card. " +
            "Enter your calculated results below.",
            new Vector2(0, 185),
            new Vector2(930, 85),
            26,
            Color.white
        );

        // ======================================
        // MATERIAL LOSS INPUT
        // ======================================

        MakeText(
            "LossLabel",
            panel,
            "MATERIAL LOSS (mm)",
            new Vector2(-245, 105),
            new Vector2(470, 45),
            26,
            Color.white
        );

        MakeText(
            "PercentageLabel",
            panel,
            "MATERIAL LOSS (%)",
            new Vector2(245, 105),
            new Vector2(470, 45),
            26,
            Color.white
        );

        lossInput = MakeInput(
            "MaterialLossInput",
            panel,
            new Vector2(-245, 45),
            "Enter loss in mm"
        );

        percentageInput = MakeInput(
            "PercentageLossInput",
            panel,
            new Vector2(245, 45),
            "Enter percentage"
        );

        // ======================================
        // CONDITION SELECTION
        // ======================================

        MakeText(
            "ConditionLabel",
            panel,
            "SELECT PIPELINE CONDITION",
            new Vector2(0, -65),
            new Vector2(900, 55),
            29,
            Color.white
        );

        normalButton = MakeButton(
            "NormalButton",
            panel,
            "NORMAL",
            new Vector2(-325, -145),
            new Vector2(300, 75)
        );

        moderateButton = MakeButton(
            "ModerateButton",
            panel,
            "MODERATE",
            new Vector2(0, -145),
            new Vector2(300, 75)
        );

        criticalButton = MakeButton(
            "CriticalButton",
            panel,
            "CRITICAL",
            new Vector2(325, -145),
            new Vector2(300, 75)
        );

        // ======================================
        // SUBMISSION BUTTON
        // ======================================

        submitButton = MakeButton(
            "SubmitButton",
            panel,
            "SUBMIT FINDINGS",
            new Vector2(0, -250),
            new Vector2(450, 80)
        );

        statusText = MakeText(
            "StatusText",
            panel,
            "",
            new Vector2(0, -325),
            new Vector2(950, 50),
            22,
            highlightColor
        );

        RefreshReferences();

        PositionInFrontOfCamera();

        // Visible for manual editing in Edit Mode.

        assessmentRoot.SetActive(true);

#if UNITY_EDITOR
        EditorUtility.SetDirty(this);

        EditorSceneManager.MarkSceneDirty(
            gameObject.scene
        );
#endif

        Debug.Log(
            "[TESTING] Editable material loss assessment built."
        );
    }

    // ======================================
    // UI RECT HELPER
    // ======================================

    private RectTransform MakeRect(
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

    // ======================================
    // IMAGE HELPER
    // ======================================

    private Image MakeImage(
        RectTransform rect,
        Color color,
        bool raycast = false)
    {
        Image image =
            rect.gameObject.AddComponent<Image>();

        image.color = color;

        image.raycastTarget = raycast;

        return image;
    }

    // ======================================
    // TEXT HELPER
    // ======================================

    private TextMeshProUGUI MakeText(
        string name,
        Transform parent,
        string content,
        Vector2 position,
        Vector2 size,
        float fontSize,
        Color color)
    {
        RectTransform rect = MakeRect(
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

        text.color = color;

        text.alignment =
            TextAlignmentOptions.Center;

        text.enableWordWrapping = true;

        text.raycastTarget = false;

        return text;
    }

    // ======================================
    // INPUT FIELD CREATION
    // ======================================

    private TMP_InputField MakeInput(
        string name,
        Transform parent,
        Vector2 position,
        string placeholder)
    {
        RectTransform root = MakeRect(
            name,
            parent,
            position,
            new Vector2(460, 85)
        );

        MakeImage(
            root,
            new Color32(45, 45, 45, 255),
            true
        );

        TMP_InputField input =
            root.gameObject.AddComponent<
                TMP_InputField
            >();

        RectTransform textArea = MakeRect(
            "TextArea",
            root,
            Vector2.zero,
            new Vector2(425, 70)
        );

        TextMeshProUGUI text = MakeText(
            "InputText",
            textArea,
            "",
            Vector2.zero,
            new Vector2(420, 65),
            32,
            Color.white
        );

        TextMeshProUGUI placeholderText = MakeText(
            "Placeholder",
            textArea,
            placeholder,
            Vector2.zero,
            new Vector2(420, 65),
            27,
            new Color32(170, 170, 170, 255)
        );

        input.textViewport = textArea;

        input.textComponent = text;

        input.placeholder = placeholderText;

        input.targetGraphic =
            root.GetComponent<Image>();

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
            new Vector3(460, 85, 8);

        return input;
    }

    // ======================================
    // BUTTON CREATION
    // ======================================

    private Button MakeButton(
        string name,
        Transform parent,
        string label,
        Vector2 position,
        Vector2 size)
    {
        RectTransform rect = MakeRect(
            name,
            parent,
            position,
            size
        );

        Image image = MakeImage(
            rect,
            new Color32(55, 55, 55, 255),
            true
        );

        Button button =
            rect.gameObject.AddComponent<Button>();

        button.targetGraphic = image;

        button.transition =
            Selectable.Transition.ColorTint;

        ColorBlock colors = button.colors;

        colors.normalColor = Color.white;

        colors.highlightedColor = highlightColor;

        colors.pressedColor = highlightColor;

        colors.selectedColor = Color.white;

        button.colors = colors;

        MakeText(
            "ButtonText",
            rect,
            label,
            Vector2.zero,
            size - new Vector2(12, 12),
            27,
            Color.white
        );

        BoxCollider box =
            rect.gameObject.AddComponent<
                BoxCollider
            >();

        box.size = new Vector3(
            size.x,
            size.y,
            8
        );

        return button;
    }

    // ======================================
    // RUNTIME SETUP
    // ======================================

    private void Awake()
    {
        if (!Application.isPlaying)
            return;

        submitted = false;

        materialLoss = 0f;

        percentageLoss = 0f;

        selectedCondition = "";

        materialLossCorrect = false;

        percentageLossCorrect = false;

        conditionCorrect = false;

        correctAnswerCount = 0;

        assessmentRecorded = false;

        RefreshReferences();

        if (assessmentRoot == null)
        {
            Debug.LogError(
                "[TESTING] Build Material Loss Assessment " +
                "in Edit Mode first."
            );

            return;
        }

        assessmentRoot.SetActive(false);

        if (normalButton != null)
        {
            normalButton.onClick.AddListener(
                () => SelectCondition("Normal")
            );
        }

        if (moderateButton != null)
        {
            moderateButton.onClick.AddListener(
                () => SelectCondition("Moderate")
            );
        }

        if (criticalButton != null)
        {
            criticalButton.onClick.AddListener(
                () => SelectCondition("Critical")
            );
        }

        if (submitButton != null)
        {
            submitButton.onClick.AddListener(
                SubmitFindings
            );
        }
    }

    // ======================================
    // REFRESH REFERENCES
    // ======================================

    private void RefreshReferences()
    {
        conditionButtons = new Button[]
        {
            normalButton,
            moderateButton,
            criticalButton
        };

        conditionImages = new Image[3];

        for (int i = 0; i < 3; i++)
        {
            if (conditionButtons[i] != null)
            {
                conditionImages[i] =
                    conditionButtons[i]
                    .GetComponent<Image>();
            }
        }
    }

    // ======================================
    // POSITION ASSESSMENT
    // ======================================

    private void PositionInFrontOfCamera()
    {
        if (assessmentRoot == null)
            return;

        if (playerCamera == null)
            playerCamera = Camera.main;

        if (playerCamera == null)
            return;

        Transform cam =
            playerCamera.transform;

        assessmentRoot.transform.position =
            cam.position +
            cam.forward * distanceFromPlayer;

        assessmentRoot.transform.rotation =
            cam.rotation;

        Canvas canvas =
            assessmentRoot.GetComponent<Canvas>();

        if (canvas != null)
        {
            canvas.worldCamera = playerCamera;
        }
    }

    // ======================================
    // OPEN ASSESSMENT
    // ======================================

    public void ShowQuestion()
    {
        if (submitted ||
            assessmentOpen ||
            assessmentRoot == null)
        {
            return;
        }

        PositionInFrontOfCamera();

        assessmentRoot.SetActive(true);

        assessmentOpen = true;

        // Preserve original desktop cursor behaviour.

        cursorWasVisible = Cursor.visible;

        previousLockMode = Cursor.lockState;

        Cursor.lockState = CursorLockMode.None;

        Cursor.visible = true;

        if (statusText != null)
        {
            statusText.text = "";
        }

        UpdateConditionColours();

        Debug.Log(
            "[TESTING] Material loss assessment opened."
        );
    }

    // ======================================
    // CONDITION SELECTION
    // ======================================

    public void SelectCondition(string condition)
    {
        if (!assessmentOpen || submitted)
            return;

        if (condition != "Normal" &&
            condition != "Moderate" &&
            condition != "Critical")
        {
            return;
        }

        selectedCondition = condition;

        UpdateConditionColours();

        Debug.Log(
            "[TESTING] Condition selected: " +
            condition
        );
    }

    private void UpdateConditionColours()
    {
        string[] names =
        {
            "Normal",
            "Moderate",
            "Critical"
        };

        for (int i = 0; i < 3; i++)
        {
            if (conditionImages[i] == null)
                continue;

            // Yellow indicates the trainee's selection.
            // It does not indicate answer correctness.

            conditionImages[i].color =
                names[i] == selectedCondition

                    ? highlightColor

                    : new Color32(
                        55, 55, 55, 255
                    );
        }
    }

    // ======================================
    // DESKTOP PHYSICS INPUT SUPPORT
    // ======================================

    private void Update()
    {
        if (!assessmentOpen ||
            submitted ||
            Mouse.current == null ||
            playerCamera == null)
        {
            return;
        }

        if (!Mouse.current.leftButton.wasPressedThisFrame)
            return;

        Ray ray =
            playerCamera.ScreenPointToRay(
                Mouse.current.position.ReadValue()
            );

        // Check input fields first.

        if (HitObject(lossInput, ray))
        {
            FocusInput(lossInput);

            return;
        }

        if (HitObject(percentageInput, ray))
        {
            FocusInput(percentageInput);

            return;
        }

        if (HitButton(normalButton, ray))
        {
            SelectCondition("Normal");

            return;
        }

        if (HitButton(moderateButton, ray))
        {
            SelectCondition("Moderate");

            return;
        }

        if (HitButton(criticalButton, ray))
        {
            SelectCondition("Critical");

            return;
        }

        if (HitButton(submitButton, ray))
        {
            SubmitFindings();
        }
    }

    // ======================================
    // INPUT RAYCAST
    // ======================================

    private bool HitObject(
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

    // ======================================
    // BUTTON RAYCAST
    // ======================================

    private bool HitButton(
        Button button,
        Ray ray)
    {
        if (button == null)
            return false;

        BoxCollider box =
            button.GetComponent<BoxCollider>();

        return box != null &&
               box.Raycast(
                   ray,
                   out RaycastHit hit,
                   100f
               );
    }

    // ======================================
    // FOCUS INPUT
    // ======================================

    private void FocusInput(
        TMP_InputField input)
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

    // ======================================
    // SUBMIT RESULTS
    // ======================================

    public void SubmitFindings()
    {
        if (!assessmentOpen || submitted)
            return;

        if (lossInput == null ||
            percentageInput == null)
        {
            return;
        }

        bool validLoss = TryReadNumber(
            lossInput.text,
            out float loss
        );

        bool validPercentage = TryReadNumber(
            percentageInput.text,
            out float percentage
        );

        // Validate input format only.
        // Do not reveal answer correctness.

        if (!validLoss ||
            !validPercentage ||
            loss < 0f ||
            percentage < 0f ||
            percentage > 100f ||
            string.IsNullOrEmpty(selectedCondition))
        {
            if (statusText != null)
            {
                statusText.text =
                    "Enter both calculations and select " +
                    "a pipeline condition.";
            }

            return;
        }

        materialLoss = loss;

        percentageLoss = percentage;

        submitted = true;

        // ======================================
        // NEW: EVALUATE ALL THREE ANSWERS
        // ======================================

        EvaluateAnswers();

        // Record before opening the next task.

        SaveAssessmentResults();

        Debug.Log(
            "[TESTING] Material loss assessment submitted. " +
            "Loss: " + materialLoss +
            " mm | Percentage: " + percentageLoss +
            "% | Condition: " + selectedCondition
        );

        // No immediate correctness feedback.

        CloseAssessment();

        // Preserve existing assessment sequence.

        onAnswerSubmitted?.Invoke();
    }

    // ======================================
    // NEW: EVALUATE ANSWERS INTERNALLY
    // ======================================

    private void EvaluateAnswers()
    {
        const float precisionAllowance = 0.00001f;

        materialLossCorrect =
            Mathf.Abs(
                materialLoss -
                expectedMaterialLoss
            )
            <= materialLossTolerance +
               precisionAllowance;

        percentageLossCorrect =
            Mathf.Abs(
                percentageLoss -
                expectedPercentageLoss
            )
            <= percentageTolerance +
               precisionAllowance;

        conditionCorrect =
            string.Equals(
                selectedCondition,
                expectedCondition,
                StringComparison.OrdinalIgnoreCase
            );

        correctAnswerCount = 0;

        if (materialLossCorrect)
            correctAnswerCount++;

        if (percentageLossCorrect)
            correctAnswerCount++;

        if (conditionCorrect)
            correctAnswerCount++;
    }

    // ======================================
    // NEW: SAVE CENTRAL ASSESSMENT RESULTS
    // ======================================

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
                "Material loss results retained locally."
            );

            return;
        }

        TestingAssessmentManager.TaskRecord task =
            manager.FindTask("INSPECTION_03");

        if (task == null)
        {
            Debug.LogWarning(
                "[ASSESSMENT] Task INSPECTION_03 " +
                "was not registered."
            );

            return;
        }

        if (task.completed)
        {
            Debug.LogWarning(
                "[ASSESSMENT] Task 3 already recorded."
            );

            return;
        }

        // ======================================
        // ANSWER 1: MATERIAL LOSS
        // ======================================

        manager.RecordAnswer(
            "INSPECTION_03",

            "Calculate material loss in millimetres.",

            FormatNumber(materialLoss) + " mm",

            FormatNumber(expectedMaterialLoss) + " mm",

            materialLossCorrect,

            materialLossCorrect

                ? ""

                : "Calculate material loss by subtracting " +
                  "the remaining wall thickness from " +
                  "the reference wall thickness."
        );

        // ======================================
        // ANSWER 2: PERCENTAGE LOSS
        // ======================================

        manager.RecordAnswer(
            "INSPECTION_03",

            "Calculate percentage material loss.",

            FormatNumber(percentageLoss) + "%",

            FormatNumber(expectedPercentageLoss) + "%",

            percentageLossCorrect,

            percentageLossCorrect

                ? ""

                : "Divide material loss by reference " +
                  "wall thickness, then multiply by 100."
        );

        // ======================================
        // ANSWER 3: PIPELINE CONDITION
        // ======================================

        manager.RecordAnswer(
            "INSPECTION_03",

            "Determine the pipeline condition.",

            selectedCondition,

            expectedCondition,

            conditionCorrect,

            conditionCorrect

                ? ""

                : "Review the predefined material loss " +
                  "classification thresholds for this " +
                  "training scenario."
        );

        // ======================================
        // RECORD COMPLETE TASK RESULT
        // ======================================

        string improvement = "";

        if (!materialLossCorrect)
        {
            improvement +=
                "Review the wall thickness subtraction. ";
        }

        if (!percentageLossCorrect)
        {
            improvement +=
                "Review the percentage material loss formula. ";
        }

        if (!conditionCorrect)
        {
            improvement +=
                "Review the scenario condition thresholds.";
        }

        manager.RecordTaskResult(
            "INSPECTION_03",

            correctAnswerCount,

            TotalAnswers,

            TotalAnswers - correctAnswerCount,

            true,

            improvement.Trim()
        );

        assessmentRecorded = true;

        Debug.Log(
            "[ASSESSMENT] Task 3 material loss results recorded."
        );
    }

    // ======================================
    // FORMAT NUMBERS FOR FINAL FEEDBACK
    // ======================================

    private string FormatNumber(float value)
    {
        return value.ToString(
            "0.#########",
            CultureInfo.InvariantCulture
        );
    }

    // ======================================
    // READ NUMERIC INPUT
    // ======================================

    private bool TryReadNumber(
        string input,
        out float value)
    {
        bool parsed = float.TryParse(
            input,
            NumberStyles.Float,
            CultureInfo.InvariantCulture,
            out value
        );

        if (!parsed)
        {
            parsed = float.TryParse(
                input,
                NumberStyles.Float,
                CultureInfo.CurrentCulture,
                out value
            );
        }

        // Reject NaN and infinity.

        return parsed &&
               !float.IsNaN(value) &&
               !float.IsInfinity(value);
    }

    // ======================================
    // CLOSE ASSESSMENT
    // ======================================

    private void CloseAssessment()
    {
        assessmentOpen = false;

        if (assessmentRoot != null)
        {
            assessmentRoot.SetActive(false);
        }

        Cursor.lockState = previousLockMode;

        Cursor.visible = cursorWasVisible;
    }

    // ======================================
    // RESET ASSESSMENT
    // ======================================

    public void ResetAssessment()
    {
        submitted = false;

        materialLoss = 0f;

        percentageLoss = 0f;

        selectedCondition = "";

        materialLossCorrect = false;

        percentageLossCorrect = false;

        conditionCorrect = false;

        correctAnswerCount = 0;

        assessmentRecorded = false;

        // Clear the corresponding assessment record
        // during an active testing session.

        if (Application.isPlaying)
        {
            TestingAssessmentManager manager =
                TestingAssessmentManager.Instance;

            if (manager != null &&
                manager.SessionStarted &&
                !manager.SessionCompleted)
            {
                TestingAssessmentManager.TaskRecord task =
                    manager.FindTask("INSPECTION_03");

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

        if (lossInput != null)
        {
            lossInput.SetTextWithoutNotify("");
        }

        if (percentageInput != null)
        {
            percentageInput.SetTextWithoutNotify("");
        }

        if (statusText != null)
        {
            statusText.text = "";
        }

        UpdateConditionColours();

        if (assessmentOpen)
        {
            CloseAssessment();
        }
        else if (assessmentRoot != null)
        {
            assessmentRoot.SetActive(false);
        }

        Debug.Log(
            "[ASSESSMENT] Material loss assessment reset."
        );
    }
}
