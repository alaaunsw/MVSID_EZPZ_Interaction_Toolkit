
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PipeConditionTraining : MonoBehaviour
{
    [Header("Existing Training")]
    public GuidedMaterialLossTraining materialLossTraining;
    public GameObject materialLossPanel;

    [Header("Condition Training")]
    public GameObject conditionPanel;

    [Header("Training Progress Checklist")]
    public InspectionTaskChecklist taskChecklist;

    [Header("Automatic Transition")]
    public bool openAfterCalculation = true;
    public float transitionDelay = 2f;

    private TMP_Text pipeIDText;
    private TMP_Text headingText;
    private TMP_Text instructionText;
    private TMP_Text thicknessLossText;
    private TMP_Text feedbackText;

    private Button normalButton;
    private Button moderateButton;
    private Button criticalButton;
    private Button checkButton;
    private Button continueButton;

    private string selectedCondition = "";
    private bool answerCorrect;
    private bool activityOpened;
    private bool completionDetected;
    private float completionTime;
    private int lastActionFrame = -1;

    // NEW: Prevent duplicate checklist completion.
    private bool conditionTaskCompleted;

    private readonly Color navy =
        new Color32(26, 73, 135, 255);

    private readonly Color dark =
        new Color32(43, 55, 72, 255);

    private readonly Color grey =
        new Color32(243, 245, 248, 255);

    private readonly Color green =
        new Color32(221, 244, 229, 255);

    private readonly Color yellow =
        new Color32(255, 240, 201, 255);

    private readonly Color red =
        new Color32(255, 226, 226, 255);

    private readonly Color gold =
        new Color32(211, 167, 71, 255);

    // =========================================
    // UNITY
    // =========================================

    private void Start()
    {
        FindReferences();
        FindControls();
        BindButtons();

        conditionTaskCompleted = false;

        if (conditionPanel != null)
            conditionPanel.SetActive(false);
    }

    private void Update()
    {
        if (!openAfterCalculation ||
            activityOpened ||
            materialLossTraining == null)
            return;

        if (!IsCalculationCompleted())
        {
            completionDetected = false;
            return;
        }

        if (!completionDetected)
        {
            completionDetected = true;
            completionTime = Time.time;
        }

        if (Time.time - completionTime >=
            transitionDelay)
        {
            ShowConditionTraining();
        }
    }

    private void FindReferences()
    {
        if (materialLossTraining == null)
        {
            materialLossTraining =
                GetComponentInChildren<
                    GuidedMaterialLossTraining
                >(true);
        }

        if (materialLossPanel == null &&
            materialLossTraining != null)
        {
            materialLossPanel =
                materialLossTraining.gameObject;
        }

        if (conditionPanel == null)
        {
            Transform found =
                transform.Find(
                    "PipeConditionTrainingPanel"
                );

            if (found != null)
                conditionPanel = found.gameObject;
        }
    }

    private bool IsCalculationCompleted()
    {
        if (materialLossTraining == null)
            return false;

        TMP_Text step =
            materialLossTraining.stepText;

        if (step != null &&
            step.text.ToUpperInvariant()
                .Contains("COMPLETED"))
        {
            return true;
        }

        TMP_Text feedback =
            materialLossTraining.feedbackText;

        if (feedback != null &&
            feedback.text.ToLowerInvariant()
                .Contains("training completed"))
        {
            return true;
        }

        return false;
    }

    // =========================================
    // BUILD PANEL IN EDIT MODE
    // =========================================

    [ContextMenu("Build Pipe Condition Training Panel")]
    public void BuildConditionPanel()
    {
        if (Application.isPlaying)
        {
            Debug.LogWarning(
                "Stop Play Mode before building."
            );

            return;
        }

        FindReferences();

        Canvas canvas =
            GetComponent<Canvas>();

        if (canvas == null ||
            canvas.renderMode != RenderMode.WorldSpace)
        {
            Debug.LogError(
                "Attach PipeConditionTraining to " +
                "the World Space training Canvas."
            );

            return;
        }

        Transform old =
            transform.Find(
                "PipeConditionTrainingPanel"
            );

        if (old != null)
        {
            // Preserve the existing panel.

            conditionPanel = old.gameObject;
        }
        else
        {
            conditionPanel = new GameObject(
                "PipeConditionTrainingPanel",
                typeof(RectTransform),
                typeof(Image)
            );

            conditionPanel.transform.SetParent(
                transform, false
            );
        }

        RectTransform panelRect =
            conditionPanel.GetComponent<RectTransform>();

        SetRect(
            panelRect, 0, 0, 820, 820
        );

        Image panelImage =
            conditionPanel.GetComponent<Image>();

        panelImage.color = Color.white;
        panelImage.raycastTarget = true;

        // =========================================
        // HEADER
        // =========================================

        CreateText(
            conditionPanel.transform,
            "SectionTitle",
            "PIPELINE INSPECTION TRAINING",
            22, navy, FontStyles.Bold,
            0, 348, 730, 40
        );

        pipeIDText = CreateText(
            conditionPanel.transform,
            "PipeIDText",
            "PIPE_0493",
            17, dark, FontStyles.Normal,
            -270, 306, 190, 30
        );

        CreateText(
            conditionPanel.transform,
            "StepNumber",
            "STEP 5",
            19, navy, FontStyles.Bold,
            296, 306, 130, 30
        );

        CreateRectangle(
            conditionPanel.transform,
            "GoldDivider",
            gold,
            0, 284, 730, 3
        );

        // =========================================
        // ACTIVITY INTRODUCTION
        // =========================================

        headingText = CreateText(
            conditionPanel.transform,
            "LearningHeading",
            "Identify the Pipe Condition",
            33, dark, FontStyles.Bold,
            0, 232, 730, 55
        );

        instructionText = CreateText(
            conditionPanel.transform,
            "InstructionText",
            "Compare the thickness loss with " +
            "the condition ranges below.",
            21, dark, FontStyles.Normal,
            0, 177, 730, 55
        );

        // =========================================
        // CONDITION REFERENCE CARDS
        // =========================================

        CreateConditionCard(
            "NormalCard",
            "NORMAL",
            "Loss <= 2 mm",
            "Dataset average: 0.92 mm",
            -245,
            green
        );

        CreateConditionCard(
            "ModerateCard",
            "MODERATE",
            "2 mm < Loss <= 5 mm",
            "Dataset average: 3.53 mm",
            0,
            yellow
        );

        CreateConditionCard(
            "CriticalCard",
            "CRITICAL",
            "Loss > 5 mm",
            "Dataset average: 7.46 mm",
            245,
            red
        );

        // =========================================
        // TRAINEE MEASUREMENT
        // =========================================

        CreateRectangle(
            conditionPanel.transform,
            "CalculatedLossBackground",
            new Color32(235, 244, 253, 255),
            0, -58, 730, 81
        );

        CreateText(
            conditionPanel.transform,
            "LossCaption",
            "YOUR CALCULATED THICKNESS LOSS",
            17, dark, FontStyles.Normal,
            0, -38, 700, 26
        );

        thicknessLossText = CreateText(
            conditionPanel.transform,
            "CalculatedThicknessLoss",
            "3.41 mm",
            29, navy, FontStyles.Bold,
            0, -76, 700, 37
        );

        CreateText(
            conditionPanel.transform,
            "QuestionText",
            "Which condition best describes this pipe?",
            23, dark, FontStyles.Bold,
            0, -132, 730, 40
        );

        // =========================================
        // CONDITION SELECTION BUTTONS
        // =========================================

        normalButton = CreateButton(
            "NormalButton",
            "NORMAL",
            -245, -200, 225, 55
        );

        moderateButton = CreateButton(
            "ModerateButton",
            "MODERATE",
            0, -200, 225, 55
        );

        criticalButton = CreateButton(
            "CriticalButton",
            "CRITICAL",
            245, -200, 225, 55
        );

        feedbackText = CreateText(
            conditionPanel.transform,
            "FeedbackText",
            "Select a condition to continue.",
            19, dark, FontStyles.Normal,
            0, -268, 730, 60
        );

        checkButton = CreateButton(
            "CheckConditionButton",
            "CHECK CONDITION",
            0, -345, 730, 57
        );

        continueButton = CreateButton(
            "ContinueConditionButton",
            "CONTINUE",
            0, -345, 730, 57
        );

        FindControls();

        conditionPanel.SetActive(false);
        conditionPanel.transform.SetAsLastSibling();

        Debug.Log(
            "Pipe Condition Training Panel built."
        );
    }

    // =========================================
    // UI CREATION
    // =========================================

    private void CreateConditionCard(
        string name,
        string condition,
        string range,
        string average,
        float x,
        Color background)
    {
        Transform card = CreateRectangle(
            conditionPanel.transform,
            name,
            background,
            x, 73, 225, 151
        );

        CreateText(
            card,
            "ConditionName",
            condition,
            22, navy, FontStyles.Bold,
            0, 49, 215, 35
        );

        CreateText(
            card,
            "ThicknessRange",
            range,
            18, dark, FontStyles.Bold,
            0, 0, 214, 42
        );

        CreateText(
            card,
            "DatasetAverage",
            average,
            16, dark, FontStyles.Normal,
            0, -49, 214, 42
        );
    }

    private Transform CreateRectangle(
        Transform parent,
        string name,
        Color color,
        float x,
        float y,
        float width,
        float height)
    {
        Transform existing = parent.Find(name);

        if (existing == null)
        {
            GameObject obj = new GameObject(
                name,
                typeof(RectTransform),
                typeof(Image)
            );

            obj.transform.SetParent(
                parent, false
            );

            existing = obj.transform;
        }

        SetRect(
            existing, x, y, width, height
        );

        Image image =
            existing.GetComponent<Image>();

        if (image != null)
        {
            image.color = color;
            image.raycastTarget = false;
        }

        return existing;
    }

    private TMP_Text CreateText(
        Transform parent,
        string name,
        string value,
        float size,
        Color color,
        FontStyles style,
        float x,
        float y,
        float width,
        float height)
    {
        Transform existing = parent.Find(name);

        if (existing == null)
        {
            GameObject obj = new GameObject(
                name,
                typeof(RectTransform),
                typeof(TextMeshProUGUI)
            );

            obj.transform.SetParent(
                parent, false
            );

            existing = obj.transform;
        }

        SetRect(
            existing, x, y, width, height
        );

        TMP_Text text =
            existing.GetComponent<TMP_Text>();

        text.text = value;
        text.fontSize = size;
        text.fontStyle = style;
        text.color = color;

        text.alignment =
            TextAlignmentOptions.Center;

        text.verticalAlignment =
            VerticalAlignmentOptions.Middle;

        text.enableWordWrapping = true;
        text.raycastTarget = false;

        return text;
    }

    private Button CreateButton(
        string name,
        string caption,
        float x,
        float y,
        float width,
        float height)
    {
        Transform existing =
            conditionPanel.transform.Find(name);

        if (existing == null)
        {
            GameObject obj = new GameObject(
                name,
                typeof(RectTransform),
                typeof(Image),
                typeof(Button),
                typeof(BoxCollider)
            );

            obj.transform.SetParent(
                conditionPanel.transform, false
            );

            existing = obj.transform;
        }

        SetRect(
            existing, x, y, width, height
        );

        Image image =
            existing.GetComponent<Image>();

        image.color = navy;
        image.raycastTarget = true;

        Button button =
            existing.GetComponent<Button>();

        button.targetGraphic = image;

        BoxCollider collider =
            existing.GetComponent<BoxCollider>();

        if (collider != null)
        {
            collider.center = Vector3.zero;

            collider.size = new Vector3(
                width, height, 4f
            );
        }

        CreateText(
            existing,
            "ButtonText",
            caption,
            20, Color.white, FontStyles.Bold,
            0, 0,
            width - 12,
            height - 8
        );

        return button;
    }

    private void SetRect(
        Transform target,
        float x,
        float y,
        float width,
        float height)
    {
        RectTransform rect =
            target as RectTransform;

        if (rect == null)
            return;

        rect.anchorMin =
            new Vector2(0.5f, 0.5f);

        rect.anchorMax =
            new Vector2(0.5f, 0.5f);

        rect.pivot =
            new Vector2(0.5f, 0.5f);

        rect.anchoredPosition3D =
            new Vector3(x, y, 0);

        rect.sizeDelta =
            new Vector2(width, height);

        rect.localScale = Vector3.one;

        rect.localRotation =
            Quaternion.identity;
    }

    // =========================================
    // BUTTON REFERENCES AND EVENTS
    // =========================================

    private void FindControls()
    {
        if (conditionPanel == null)
            return;

        Transform root =
            conditionPanel.transform;

        pipeIDText =
            FindText(root, "PipeIDText");

        headingText =
            FindText(root, "LearningHeading");

        instructionText =
            FindText(root, "InstructionText");

        thicknessLossText =
            FindText(
                root,
                "CalculatedThicknessLoss"
            );

        feedbackText =
            FindText(root, "FeedbackText");

        normalButton =
            FindButton(root, "NormalButton");

        moderateButton =
            FindButton(root, "ModerateButton");

        criticalButton =
            FindButton(root, "CriticalButton");

        checkButton =
            FindButton(root, "CheckConditionButton");

        continueButton =
            FindButton(root, "ContinueConditionButton");
    }

    private TMP_Text FindText(
        Transform root,
        string name)
    {
        Transform child = root.Find(name);

        return child != null
            ? child.GetComponent<TMP_Text>()
            : null;
    }

    private Button FindButton(
        Transform root,
        string name)
    {
        Transform child = root.Find(name);

        return child != null
            ? child.GetComponent<Button>()
            : null;
    }

    private void BindButtons()
    {
        if (normalButton != null)
        {
            normalButton.onClick.AddListener(
                ChooseNormal
            );
        }

        if (moderateButton != null)
        {
            moderateButton.onClick.AddListener(
                ChooseModerate
            );
        }

        if (criticalButton != null)
        {
            criticalButton.onClick.AddListener(
                ChooseCritical
            );
        }

        if (checkButton != null)
        {
            checkButton.onClick.AddListener(
                CheckCondition
            );
        }

        if (continueButton != null)
        {
            continueButton.onClick.AddListener(
                ContinueTraining
            );
        }
    }

    // =========================================
    // OPEN ACTIVITY
    // =========================================

    [ContextMenu("Show Condition Training")]
    public void ShowConditionTraining()
    {
        FindReferences();
        FindControls();

        if (conditionPanel == null)
        {
            Debug.LogWarning(
                "Build the condition panel first."
            );

            return;
        }

        activityOpened = true;
        answerCorrect = false;
        selectedCondition = "";

        // NEW: Reset checklist completion guard.
        conditionTaskCompleted = false;

        PipeInspectionData data =
            materialLossTraining != null &&
            materialLossTraining.pipeTarget != null
            ? materialLossTraining.pipeTarget
                .GetInspectionData()
            : null;

        if (data != null)
        {
            float loss =
                data.referenceThicknessMM -
                data.remainingThicknessMM;

            if (pipeIDText != null)
                pipeIDText.text = data.pipeID;

            if (thicknessLossText != null)
            {
                thicknessLossText.text =
                    loss.ToString("F2") + " mm";
            }
        }

        if (feedbackText != null)
        {
            feedbackText.text =
                "Select a condition to continue.";

            feedbackText.color = dark;
        }

        ResetChoiceButton(normalButton);
        ResetChoiceButton(moderateButton);
        ResetChoiceButton(criticalButton);

        if (checkButton != null)
            checkButton.gameObject.SetActive(true);

        if (continueButton != null)
            continueButton.gameObject.SetActive(false);

        // Hide previous activity and show Step 5.

        if (materialLossPanel != null)
            materialLossPanel.SetActive(false);

        conditionPanel.SetActive(true);

        conditionPanel.transform.SetAsLastSibling();

        Debug.Log(
            "Step 5: Pipe condition training opened."
        );
    }

    // =========================================
    // CONDITION SELECTION
    // =========================================

    public void ChooseNormal()
    {
        SelectCondition("Normal");
    }

    public void ChooseModerate()
    {
        SelectCondition("Moderate");
    }

    public void ChooseCritical()
    {
        SelectCondition("Critical");
    }

    private void SelectCondition(string choice)
    {
        if (answerCorrect)
            return;

        if (lastActionFrame == Time.frameCount)
            return;

        lastActionFrame = Time.frameCount;

        selectedCondition = choice;

        ResetChoiceButton(normalButton);
        ResetChoiceButton(moderateButton);
        ResetChoiceButton(criticalButton);

        Button selectedButton = null;

        if (choice == "Normal")
            selectedButton = normalButton;

        if (choice == "Moderate")
            selectedButton = moderateButton;

        if (choice == "Critical")
            selectedButton = criticalButton;

        if (selectedButton != null)
        {
            Image image =
                selectedButton.GetComponent<Image>();

            if (image != null)
            {
                image.color =
                    new Color32(42, 117, 190, 255);
            }
        }

        if (feedbackText != null)
        {
            feedbackText.text =
                choice +
                " selected. Press CHECK CONDITION.";

            feedbackText.color = dark;
        }
    }

    private void ResetChoiceButton(Button button)
    {
        if (button == null)
            return;

        Image image =
            button.GetComponent<Image>();

        if (image != null)
            image.color = navy;
    }

    // =========================================
    // CHECK ANSWER
    // =========================================

    public void CheckCondition()
    {
        if (answerCorrect)
            return;

        if (string.IsNullOrEmpty(selectedCondition))
        {
            SetFeedback(
                "Select Normal, Moderate or Critical.",
                dark
            );

            return;
        }

        if (materialLossTraining == null ||
            materialLossTraining.pipeTarget == null)
        {
            SetFeedback(
                "Pipe inspection data is unavailable.",
                Color.red
            );

            return;
        }

        PipeInspectionData data =
            materialLossTraining.pipeTarget
                .GetInspectionData();

        if (data == null)
            return;

        float loss =
            data.referenceThicknessMM -
            data.remainingThicknessMM;

        string correctCondition;

        if (loss <= 2f)
        {
            correctCondition = "Normal";
        }
        else if (loss <= 5f)
        {
            correctCondition = "Moderate";
        }
        else
        {
            correctCondition = "Critical";
        }

        if (selectedCondition == correctCondition)
        {
            answerCorrect = true;

            SetFeedback(
                "Correct! " +
                loss.ToString("F2") +
                " mm falls within the " +
                correctCondition + " range.",
                new Color32(20, 123, 71, 255)
            );

            if (checkButton != null)
                checkButton.gameObject.SetActive(false);

            if (continueButton != null)
                continueButton.gameObject.SetActive(true);
        }
        else
        {
            SetFeedback(
                "Not quite. Compare your thickness " +
                "loss with the range boundaries " +
                "and try again.",
                new Color32(175, 65, 45, 255)
            );
        }
    }

    private void SetFeedback(
        string message,
        Color color)
    {
        if (feedbackText == null)
            return;

        feedbackText.text = message;
        feedbackText.color = color;
    }

    // =========================================
    // FINISH ACTIVITY
    // =========================================

    public void ContinueTraining()
    {
        if (!answerCorrect)
            return;

        if (headingText != null)
        {
            headingText.text =
                "Condition Identification Completed";
        }

        if (instructionText != null)
        {
            instructionText.text =
                "You have successfully interpreted " +
                "the pipe thickness loss and " +
                "identified its condition.";
        }

        SetFeedback(
            "Condition classification completed.",
            new Color32(20, 123, 71, 255)
        );

        if (normalButton != null)
            normalButton.gameObject.SetActive(false);

        if (moderateButton != null)
            moderateButton.gameObject.SetActive(false);

        if (criticalButton != null)
            criticalButton.gameObject.SetActive(false);

        if (continueButton != null)
            continueButton.gameObject.SetActive(false);

        // NEW: Complete Task 3.

        CompleteCorrosionIdentificationTask();

        Debug.Log(
            "Pipe condition training completed."
        );
    }

    // =========================================
    // NEW: COMPLETE CHECKLIST TASK 3
    // =========================================

    private void CompleteCorrosionIdentificationTask()
    {
        // Prevent duplicate completion.

        if (conditionTaskCompleted)
            return;

        // Require a correct classification.

        if (!answerCorrect)
            return;

        // Find the checklist automatically if
        // the Inspector reference is missing.

        if (taskChecklist == null)
        {
            taskChecklist =
                FindFirstObjectByType<
                    InspectionTaskChecklist
                >();
        }

        if (taskChecklist == null)
        {
            Debug.LogWarning(
                "Inspection Task Checklist not assigned."
            );

            return;
        }

        // Mark Identify Corrosion as completed.

        taskChecklist.CompleteTask2();

        conditionTaskCompleted = true;

        Debug.Log(
            "Training Checklist: " +
            "Identify corrosion completed."
        );
    }
}
