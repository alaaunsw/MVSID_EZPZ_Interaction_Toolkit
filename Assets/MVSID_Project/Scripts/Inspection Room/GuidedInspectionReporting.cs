
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class GuidedInspectionReporting : MonoBehaviour
{
    [Header("Previous Activities")]
    public GuidedMaterialLossTraining materialLossTraining;
    public GameObject conditionTrainingPanel;

    [Header("Reporting Panel")]
    public GameObject reportingPanel;

    [Header("Transition")]
    public bool openAfterConditionTraining = true;

    [Header("Input Settings")]
    public bool focusInputWhenFieldOpens = false;

    [Header("UI Colours")]
    public Color navy =
        new Color32(26, 73, 135, 255);

    public Color dark =
        new Color32(43, 55, 72, 255);

    public Color paleBlue =
        new Color32(235, 244, 253, 255);

    public Color grey =
        new Color32(243, 245, 248, 255);

    public Color gold =
        new Color32(211, 167, 71, 255);

    private TMP_Text stageText;
    private TMP_Text headingText;
    private TMP_Text instructionText;
    private TMP_Text reasonText;
    private TMP_Text hintText;
    private TMP_Text feedbackText;
    private TMP_Text previewText;
    private TMP_Text submitStatusText;

    private TMP_InputField reportInput;

    private Button hintButton;
    private Button checkButton;
    private Button nextButton;
    private Button editButton;
    private Button submitButton;

    private GameObject previewSection;
    private GameObject submittedSection;

    private int currentField;
    private bool fieldChecked;
    private bool hintVisible;
    private bool reportingOpened;
    private bool reportSubmitted;

    private readonly string[] reportValues =
        new string[7];

    private readonly string[] fieldNames =
    {
        "Pipe Identification",
        "Defect Location",
        "Observed Finding",
        "Wall Thickness Measurements",
        "Calculated Material Loss",
        "Condition Classification",
        "Recommended Follow-Up"
    };

    private readonly string[] instructions =
    {
        "Enter the pipe identification number shown " +
        "on the inspection information card.",

        "Describe the location where the defect " +
        "was found.",

        "Describe the visible defect observed " +
        "during the inspection.",

        "Record the original and remaining wall " +
        "thickness values in millimetres.",

        "Record the calculated thickness loss " +
        "and material loss percentage.",

        "Enter the condition classification " +
        "identified in the previous activity.",

        "Describe the appropriate next action " +
        "for the inspection finding."
    };

    private readonly string[] explanations =
    {
        "The engineer must know which asset " +
        "is being reported.",

        "The location helps the engineer find " +
        "the affected section.",

        "The report must describe what was observed. " +
        "Do not assume damage that was not inspected.",

        "These measurements are the basis for " +
        "the material-loss calculation.",

        "The calculated values describe the extent " +
        "of wall thickness loss.",

        "The classification summarises the " +
        "condition using the training criteria.",

        "The trainee records and escalates the " +
        "finding for authorised engineering review."
    };

    private readonly string[] hints =
    {
        "Look at the top of the pipe information card. " +
        "The ID is PIPE_0493.",

        "The corrosion is located at the pipe support.",

        "Describe the corrosion visible at the " +
        "affected pipe support.",

        "Original: 11.11 mm\nRemaining: 7.70 mm",

        "Thickness loss: 3.41 mm\n" +
        "Material loss: 30.69%",

        "A loss greater than 2 mm and no greater " +
        "than 5 mm is Moderate.",

        "Refer the affected pipe for engineering " +
        "assessment. Do not attempt unauthorised repairs."
    };

    // =========================================
    // UNITY LIFECYCLE
    // =========================================

    private void Start()
    {
        FindReferences();
        FindUI();
        ConnectButtons();

        PrepareInitialState();

        if (reportingPanel != null)
            reportingPanel.SetActive(false);
    }

    private void Update()
    {
        if (!openAfterConditionTraining ||
            reportingOpened ||
            conditionTrainingPanel == null)
        {
            return;
        }

        Transform title =
            conditionTrainingPanel.transform.Find(
                "LearningHeading"
            );

        if (title == null)
            return;

        TMP_Text text =
            title.GetComponent<TMP_Text>();

        if (text != null &&
            text.text.Contains(
                "Condition Identification Completed"
            ))
        {
            OpenReportingTraining();
        }
    }

    // =========================================
    // FIND REFERENCES
    // =========================================

    private void FindReferences()
    {
        if (materialLossTraining == null)
        {
            materialLossTraining =
                GetComponentInChildren<
                    GuidedMaterialLossTraining
                >(true);
        }

        if (conditionTrainingPanel == null)
        {
            Transform found =
                transform.Find(
                    "PipeConditionTrainingPanel"
                );

            if (found != null)
            {
                conditionTrainingPanel =
                    found.gameObject;
            }
        }

        if (reportingPanel == null)
        {
            Transform found =
                transform.Find(
                    "GuidedInspectionReportPanel"
                );

            if (found != null)
            {
                reportingPanel =
                    found.gameObject;
            }
        }
    }

    // =========================================
    // EDIT MODE PANEL BUILDER
    // =========================================

    [ContextMenu("Build Guided Inspection Report Panel")]
    public void BuildReportPanel()
    {
        if (Application.isPlaying)
        {
            Debug.LogWarning(
                "Stop Play Mode before building."
            );

            return;
        }

        Canvas canvas =
            GetComponent<Canvas>();

        if (canvas == null ||
            canvas.renderMode != RenderMode.WorldSpace)
        {
            Debug.LogError(
                "Attach GuidedInspectionReporting " +
                "to the existing World Space Canvas."
            );

            return;
        }

        FindReferences();

        Transform existing =
            transform.Find(
                "GuidedInspectionReportPanel"
            );

        if (existing == null)
        {
            GameObject obj =
                new GameObject(
                    "GuidedInspectionReportPanel",
                    typeof(RectTransform),
                    typeof(Image)
                );

            obj.transform.SetParent(
                transform,
                false
            );

            reportingPanel = obj;
        }
        else
        {
            reportingPanel =
                existing.gameObject;
        }

        Transform root =
            reportingPanel.transform;

        SetRect(
            root,
            0, 0,
            820, 760
        );

        Image background =
            reportingPanel.GetComponent<Image>();

        background.color = Color.white;
        background.raycastTarget = true;

        // =====================================
        // HEADER
        // =====================================

        MakeText(
            root,
            "ReportTitle",
            "REPORT YOUR FINDINGS",
            27,
            navy,
            FontStyles.Bold,
            0, 323,
            730, 40
        );

        MakeText(
            root,
            "StepText",
            "STEP 6",
            18,
            navy,
            FontStyles.Bold,
            294, 284,
            140, 32
        );

        MakePanel(
            root,
            "GoldDivider",
            gold,
            0, 265,
            730, 3
        );

        stageText = MakeText(
            root,
            "ReportStageText",
            "REPORT FIELD 1 OF 7",
            18,
            navy,
            FontStyles.Bold,
            -240, 284,
            250, 34
        );

        // =====================================
        // GUIDED INSTRUCTION
        // =====================================

        headingText = MakeText(
            root,
            "LearningHeading",
            "Pipe Identification",
            32,
            dark,
            FontStyles.Bold,
            0, 203,
            730, 56
        );

        instructionText = MakeText(
            root,
            "InstructionText",
            "",
            23,
            dark,
            FontStyles.Normal,
            0, 137,
            730, 80
        );

        Transform explanationPanel =
            MakePanel(
                root,
                "WhyThisMattersBackground",
                grey,
                0, 38,
                730, 91
            );

        reasonText = MakeText(
            explanationPanel,
            "ReasonText",
            "",
            20,
            dark,
            FontStyles.Normal,
            0, 0,
            685, 80
        );

        // =====================================
        // REPORT INPUT
        // =====================================

        MakeText(
            root,
            "InputHeading",
            "WRITE YOUR REPORT ENTRY",
            20,
            navy,
            FontStyles.Bold,
            0, -47,
            730, 36
        );

        CreateInputField(
            root,
            "ReportInput",
            0, -112,
            730, 77
        );

        // =====================================
        // HINTS
        // =====================================

        hintButton = MakeButton(
            root,
            "HintButton",
            "SHOW HINT",
            -270, -184,
            190, 47,
            paleBlue,
            navy
        );

        hintText = MakeText(
            root,
            "HintText",
            "",
            18,
            navy,
            FontStyles.Normal,
            97, -184,
            530, 57
        );

        feedbackText = MakeText(
            root,
            "FeedbackText",
            "",
            19,
            dark,
            FontStyles.Normal,
            0, -238,
            730, 50
        );

        // =====================================
        // CHECK / NEXT BUTTONS
        // =====================================

        checkButton = MakeButton(
            root,
            "CheckEntryButton",
            "CHECK ENTRY",
            0, -315,
            730, 55,
            navy,
            Color.white
        );

        nextButton = MakeButton(
            root,
            "NextFieldButton",
            "NEXT FIELD",
            0, -315,
            730, 55,
            navy,
            Color.white
        );

        // =====================================
        // REPORT PREVIEW SECTION
        // =====================================

        Transform previewRoot =
            MakePanel(
                root,
                "ReportPreviewSection",
                Color.white,
                0, 0,
                760, 600
            );

        previewSection =
            previewRoot.gameObject;

        MakeText(
            previewRoot,
            "PreviewHeading",
            "REVIEW YOUR INSPECTION REPORT",
            27,
            navy,
            FontStyles.Bold,
            0, 255,
            700, 48
        );

        previewText = MakeText(
            previewRoot,
            "ReportPreviewText",
            "",
            20,
            dark,
            FontStyles.Normal,
            0, 13,
            700, 390
        );

        editButton = MakeButton(
            previewRoot,
            "EditReportButton",
            "EDIT REPORT",
            -184, -249,
            340, 55,
            paleBlue,
            navy
        );

        submitButton = MakeButton(
            previewRoot,
            "SubmitReportButton",
            "SUBMIT TO ENGINEER",
            184, -249,
            340, 55,
            navy,
            Color.white
        );

        // =====================================
        // SUBMITTED SECTION
        // =====================================

        Transform submittedRoot =
            MakePanel(
                root,
                "ReportSubmittedSection",
                Color.white,
                0, 0,
                760, 460
            );

        submittedSection =
            submittedRoot.gameObject;

        MakeText(
            submittedRoot,
            "SubmittedHeading",
            "REPORT SUBMITTED",
            32,
            navy,
            FontStyles.Bold,
            0, 145,
            680, 55
        );

        submitStatusText = MakeText(
            submittedRoot,
            "SubmittedStatus",
            "Submitted - Awaiting Engineer Review",
            22,
            dark,
            FontStyles.Normal,
            0, 0,
            680, 150
        );

        // =====================================
        // INITIAL STATE
        // =====================================

        FindUI();

        PrepareInitialState();

        // Keep the generated layout editable
        // in the Unity Hierarchy.
        reportingPanel.transform.SetAsLastSibling();

        reportingPanel.SetActive(false);

#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);

        UnityEditor.SceneManagement.EditorSceneManager
            .MarkSceneDirty(gameObject.scene);
#endif

        Debug.Log(
            "Guided Inspection Report Panel created. " +
            "Only Field 1 will appear when enabled."
        );
    }

    // =========================================
    // RECTANGLE
    // =========================================

    private Transform MakePanel(
        Transform parent,
        string name,
        Color color,
        float x,
        float y,
        float width,
        float height)
    {
        Transform child =
            parent.Find(name);

        if (child == null)
        {
            GameObject obj =
                new GameObject(
                    name,
                    typeof(RectTransform),
                    typeof(Image)
                );

            obj.transform.SetParent(
                parent,
                false
            );

            child = obj.transform;
        }

        SetRect(
            child,
            x, y,
            width, height
        );

        Image image =
            child.GetComponent<Image>();

        if (image != null)
        {
            image.color = color;
            image.raycastTarget = false;
        }

        return child;
    }

    // =========================================
    // TEXT
    // =========================================

    private TMP_Text MakeText(
        Transform parent,
        string name,
        string value,
        int size,
        Color color,
        FontStyles style,
        float x,
        float y,
        float width,
        float height)
    {
        Transform child =
            parent.Find(name);

        if (child == null)
        {
            GameObject obj =
                new GameObject(
                    name,
                    typeof(RectTransform),
                    typeof(TextMeshProUGUI)
                );

            obj.transform.SetParent(
                parent,
                false
            );

            child = obj.transform;
        }

        SetRect(
            child,
            x, y,
            width, height
        );

        TMP_Text text =
            child.GetComponent<TMP_Text>();

        text.text = value;
        text.fontSize = size;
        text.fontStyle = style;
        text.color = color;

        text.alignment =
            TextAlignmentOptions.Left;

        text.verticalAlignment =
            VerticalAlignmentOptions.Middle;

        text.enableWordWrapping = true;
        text.raycastTarget = false;

        return text;
    }

    // =========================================
    // BUTTON
    // =========================================

    private Button MakeButton(
        Transform parent,
        string name,
        string label,
        float x,
        float y,
        float width,
        float height,
        Color background,
        Color foreground)
    {
        Transform child =
            parent.Find(name);

        if (child == null)
        {
            GameObject obj =
                new GameObject(
                    name,
                    typeof(RectTransform),
                    typeof(Image),
                    typeof(Button),
                    typeof(BoxCollider)
                );

            obj.transform.SetParent(
                parent,
                false
            );

            child = obj.transform;
        }

        SetRect(
            child,
            x, y,
            width, height
        );

        Image image =
            child.GetComponent<Image>();

        image.color = background;
        image.raycastTarget = true;

        Button button =
            child.GetComponent<Button>();

        button.targetGraphic = image;
        button.interactable = true;

        BoxCollider collider =
            child.GetComponent<BoxCollider>();

        if (collider != null)
        {
            collider.center = Vector3.zero;

            collider.size =
                new Vector3(
                    width,
                    height,
                    4f
                );
        }

        TMP_Text labelText =
            MakeText(
                child,
                "ButtonText",
                label,
                21,
                foreground,
                FontStyles.Bold,
                0, 0,
                width - 15,
                height - 8
            );

        labelText.alignment =
            TextAlignmentOptions.Center;

        return button;
    }

    // =========================================
    // INPUT FIELD
    // =========================================

    private void CreateInputField(
        Transform parent,
        string name,
        float x,
        float y,
        float width,
        float height)
    {
        Transform existing =
            parent.Find(name);

        if (existing == null)
        {
            GameObject obj =
                new GameObject(
                    name,
                    typeof(RectTransform),
                    typeof(Image),
                    typeof(TMP_InputField),
                    typeof(BoxCollider)
                );

            obj.transform.SetParent(
                parent,
                false
            );

            existing = obj.transform;
        }

        SetRect(
            existing,
            x, y,
            width, height
        );

        Image background =
            existing.GetComponent<Image>();

        background.color =
            new Color32(247, 249, 252, 255);

        background.raycastTarget = true;

        TMP_InputField input =
            existing.GetComponent<TMP_InputField>();

        input.interactable = true;
        input.readOnly = false;

        input.contentType =
            TMP_InputField.ContentType.Standard;

        input.keyboardType =
            TouchScreenKeyboardType.Default;

        input.shouldHideMobileInput = false;

        input.lineType =
            TMP_InputField.LineType.MultiLineNewline;

        input.characterLimit = 300;
        input.targetGraphic = background;

        // -------------------------------------
        // TEXT VIEWPORT
        // -------------------------------------

        Transform viewport =
            existing.Find("Text Area");

        if (viewport == null)
        {
            GameObject area =
                new GameObject(
                    "Text Area",
                    typeof(RectTransform),
                    typeof(RectMask2D)
                );

            area.transform.SetParent(
                existing,
                false
            );

            viewport = area.transform;
        }

        SetRect(
            viewport,
            0, 0,
            width - 30,
            height - 12
        );

        TMP_Text inputText =
            MakeText(
                viewport,
                "Text",
                "",
                22,
                dark,
                FontStyles.Normal,
                0, 0,
                width - 55,
                height - 20
            );

        TMP_Text placeholder =
            MakeText(
                viewport,
                "Placeholder",
                "Enter your report details here...",
                20,
                Color.gray,
                FontStyles.Italic,
                0, 0,
                width - 55,
                height - 20
            );

        inputText.raycastTarget = false;
        placeholder.raycastTarget = false;

        input.textViewport =
            viewport.GetComponent<RectTransform>();

        input.textComponent = inputText;
        input.placeholder = placeholder;

        // -------------------------------------
        // COLLIDER
        // -------------------------------------

        BoxCollider collider =
            existing.GetComponent<BoxCollider>();

        if (collider != null)
        {
            collider.center = Vector3.zero;

            collider.size =
                new Vector3(
                    width,
                    height,
                    4f
                );
        }
    }

    // =========================================
    // RECT TRANSFORM
    // =========================================

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
            new Vector3(
                x,
                y,
                0
            );

        rect.sizeDelta =
            new Vector2(
                width,
                height
            );

        rect.localScale =
            Vector3.one;

        rect.localRotation =
            Quaternion.identity;
    }

    // =========================================
    // FIND UI COMPONENTS
    // =========================================

    private void FindUI()
    {
        if (reportingPanel == null)
            return;

        Transform root =
            reportingPanel.transform;

        stageText =
            FindText(root, "ReportStageText");

        headingText =
            FindText(root, "LearningHeading");

        instructionText =
            FindText(root, "InstructionText");

        Transform reasonPanel =
            root.Find(
                "WhyThisMattersBackground"
            );

        reasonText =
            FindText(
                reasonPanel,
                "ReasonText"
            );

        hintText =
            FindText(root, "HintText");

        feedbackText =
            FindText(root, "FeedbackText");

        Transform input =
            root.Find("ReportInput");

        if (input != null)
        {
            reportInput =
                input.GetComponent<TMP_InputField>();
        }

        hintButton =
            FindButton(root, "HintButton");

        checkButton =
            FindButton(root, "CheckEntryButton");

        nextButton =
            FindButton(root, "NextFieldButton");

        Transform preview =
            root.Find("ReportPreviewSection");

        if (preview != null)
        {
            previewSection =
                preview.gameObject;

            previewText =
                FindText(
                    preview,
                    "ReportPreviewText"
                );

            editButton =
                FindButton(
                    preview,
                    "EditReportButton"
                );

            submitButton =
                FindButton(
                    preview,
                    "SubmitReportButton"
                );
        }

        Transform submitted =
            root.Find("ReportSubmittedSection");

        if (submitted != null)
        {
            submittedSection =
                submitted.gameObject;

            submitStatusText =
                FindText(
                    submitted,
                    "SubmittedStatus"
                );
        }
    }

    private TMP_Text FindText(
        Transform parent,
        string name)
    {
        if (parent == null)
            return null;

        Transform child =
            parent.Find(name);

        return child != null
            ? child.GetComponent<TMP_Text>()
            : null;
    }

    private Button FindButton(
        Transform parent,
        string name)
    {
        if (parent == null)
            return null;

        Transform child =
            parent.Find(name);

        return child != null
            ? child.GetComponent<Button>()
            : null;
    }

    // =========================================
    // CONNECT UI BUTTONS
    // =========================================

    private void ConnectButtons()
    {
        Bind(
            hintButton,
            ToggleHint
        );

        Bind(
            checkButton,
            CheckEntry
        );

        Bind(
            nextButton,
            NextField
        );

        Bind(
            editButton,
            EditReport
        );

        Bind(
            submitButton,
            SubmitReport
        );
    }

    private void Bind(
        Button button,
        UnityEngine.Events.UnityAction action)
    {
        if (button == null)
            return;

        button.onClick.RemoveListener(action);
        button.onClick.AddListener(action);
    }

    // =========================================
    // PREPARE INITIAL LAYOUT
    // =========================================

    private void PrepareInitialState()
    {
        if (reportingPanel == null)
            return;

        currentField = 0;
        fieldChecked = false;
        hintVisible = false;

        if (previewSection != null)
            previewSection.SetActive(false);

        if (submittedSection != null)
            submittedSection.SetActive(false);

        SetEntryObjectsVisible(true);

        if (stageText != null)
            stageText.text = "REPORT FIELD 1 OF 7";

        if (headingText != null)
            headingText.text = fieldNames[0];

        if (instructionText != null)
            instructionText.text = instructions[0];

        if (reasonText != null)
        {
            reasonText.text =
                "<b>WHY THIS MATTERS</b>\n" +
                explanations[0];
        }

        if (hintText != null)
            hintText.text = "";

        if (feedbackText != null)
        {
            feedbackText.text =
                "Enter the requested information.";

            feedbackText.color = dark;
        }

        if (reportInput != null)
        {
            reportInput.SetTextWithoutNotify("");
        }

        if (hintButton != null)
        {
            SetButtonLabel(
                hintButton,
                "SHOW HINT"
            );
        }

        if (checkButton != null)
            checkButton.gameObject.SetActive(true);

        if (nextButton != null)
            nextButton.gameObject.SetActive(false);
    }

    // =========================================
    // SHOW / HIDE ENTRY OBJECTS
    // =========================================

    private void SetEntryObjectsVisible(bool visible)
    {
        if (reportingPanel == null)
            return;

        string[] names =
        {
            "ReportStageText",
            "LearningHeading",
            "InstructionText",
            "WhyThisMattersBackground",
            "InputHeading",
            "ReportInput",
            "HintButton",
            "HintText",
            "FeedbackText",
            "CheckEntryButton",
            "NextFieldButton"
        };

        foreach (string name in names)
        {
            Transform child =
                reportingPanel.transform.Find(name);

            if (child != null)
            {
                child.gameObject.SetActive(visible);
            }
        }
    }

    // =========================================
    // OPEN REPORT TRAINING
    // =========================================

    [ContextMenu("Open Reporting Training")]
    public void OpenReportingTraining()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning(
                "Use this command during Play Mode."
            );

            return;
        }

        FindReferences();
        FindUI();
        ConnectButtons();

        if (reportingPanel == null)
        {
            Debug.LogWarning(
                "Build the report panel first."
            );

            return;
        }

        reportingOpened = true;
        reportSubmitted = false;

        for (int i = 0; i < reportValues.Length; i++)
        {
            reportValues[i] = "";
        }

        if (conditionTrainingPanel != null)
        {
            conditionTrainingPanel.SetActive(false);
        }

        reportingPanel.SetActive(true);
        reportingPanel.transform.SetAsLastSibling();

        ShowField(0);

        Debug.Log(
            "Step 6: Guided Inspection Reporting started."
        );
    }

    // =========================================
    // SHOW REPORT FIELD
    // =========================================

    private void ShowField(int index)
    {
        currentField =
            Mathf.Clamp(index, 0, 6);

        fieldChecked = false;
        hintVisible = false;

        if (previewSection != null)
            previewSection.SetActive(false);

        if (submittedSection != null)
            submittedSection.SetActive(false);

        SetEntryObjectsVisible(true);

        if (stageText != null)
        {
            stageText.text =
                "REPORT FIELD " +
                (currentField + 1) +
                " OF 7";
        }

        if (headingText != null)
        {
            headingText.text =
                fieldNames[currentField];
        }

        if (instructionText != null)
        {
            instructionText.text =
                instructions[currentField];
        }

        if (reasonText != null)
        {
            reasonText.text =
                "<b>WHY THIS MATTERS</b>\n" +
                explanations[currentField];
        }

        if (hintText != null)
            hintText.text = "";

        if (hintButton != null)
        {
            SetButtonLabel(
                hintButton,
                "SHOW HINT"
            );
        }

        if (reportInput != null)
        {
            reportInput.SetTextWithoutNotify(
                reportValues[currentField] ?? ""
            );

            reportInput.ForceLabelUpdate();
        }

        SetFeedback(
            "Enter the requested information.",
            dark
        );

        if (checkButton != null)
            checkButton.gameObject.SetActive(true);

        if (nextButton != null)
            nextButton.gameObject.SetActive(false);

        if (Application.isPlaying &&
            focusInputWhenFieldOpens)
        {
            FocusReportInput();
        }
    }

    // =========================================
    // ACTIVATE REPORT INPUT
    // =========================================

    public void FocusReportInput()
    {
        if (reportInput == null)
        {
            FindUI();
        }

        if (reportInput == null)
        {
            Debug.LogWarning(
                "ReportInput was not found."
            );

            return;
        }

        if (!reportInput.gameObject.activeInHierarchy)
        {
            Debug.LogWarning(
                "ReportInput is not active."
            );

            return;
        }

        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(
                reportInput.gameObject
            );
        }
        else
        {
            Debug.LogWarning(
                "No active EventSystem found. " +
                "Check UI_EventSystem in the Hierarchy."
            );
        }

        reportInput.Select();
        reportInput.ActivateInputField();

        Debug.Log(
            "Report input activated."
        );
    }

    // =========================================
    // HINT BUTTON
    // =========================================

    public void ToggleHint()
    {
        hintVisible = !hintVisible;

        if (hintText != null)
        {
            hintText.text =
                hintVisible
                ? hints[currentField]
                : "";
        }

        if (hintButton != null)
        {
            SetButtonLabel(
                hintButton,
                hintVisible
                ? "HIDE HINT"
                : "SHOW HINT"
            );
        }
    }

    // =========================================
    // CHECK ENTRY
    // =========================================

    public void CheckEntry()
    {
        if (reportInput == null)
        {
            Debug.LogWarning(
                "Report input is missing."
            );

            return;
        }

        string value =
            reportInput.text.Trim();

        if (string.IsNullOrEmpty(value))
        {
            SetFeedback(
                "Please enter the report information.",
                Color.red
            );

            return;
        }

        if (!ValidateField(currentField, value))
        {
            SetFeedback(
                "Check the inspection information " +
                "and try again. Use SHOW HINT if needed.",
                new Color32(175, 65, 45, 255)
            );

            return;
        }

        reportValues[currentField] = value;
        fieldChecked = true;

        reportInput.DeactivateInputField();

        SetFeedback(
            "Good. Your report entry has been recorded.",
            new Color32(20, 123, 71, 255)
        );

        if (checkButton != null)
            checkButton.gameObject.SetActive(false);

        if (nextButton != null)
        {
            nextButton.gameObject.SetActive(true);

            SetButtonLabel(
                nextButton,
                currentField == 6
                ? "REVIEW REPORT"
                : "NEXT FIELD"
            );
        }
    }

    // =========================================
    // FIELD VALIDATION
    // =========================================

    private bool ValidateField(
        int index,
        string value)
    {
        string text =
            value.ToLowerInvariant();

        switch (index)
        {
            case 0:
                return
                    text.Replace(" ", "") ==
                    "pipe_0493";

            case 1:
                return
                    text.Contains("support");

            case 2:
                return
                    text.Contains("corrosion") ||
                    text.Contains("corroded");

            case 3:
                return
                    text.Contains("11.11") &&
                    text.Contains("7.70");

            case 4:
                return
                    text.Contains("3.41") &&
                    text.Contains("30.69");

            case 5:
                return
                    text.Contains("moderate");

            case 6:
                return
                    text.Contains("engineer") ||
                    text.Contains("assessment") ||
                    text.Contains("review");

            default:
                return false;
        }
    }

    // =========================================
    // FEEDBACK
    // =========================================

    private void SetFeedback(
        string message,
        Color color)
    {
        if (feedbackText == null)
            return;

        feedbackText.text = message;
        feedbackText.color = color;
    }

    private void SetButtonLabel(
        Button button,
        string label)
    {
        if (button == null)
            return;

        TMP_Text text =
            button.GetComponentInChildren<
                TMP_Text
            >(true);

        if (text != null)
            text.text = label;
    }

    // =========================================
    // NEXT FIELD
    // =========================================

    public void NextField()
    {
        if (!fieldChecked)
            return;

        if (currentField < 6)
        {
            ShowField(currentField + 1);
        }
        else
        {
            ShowReportPreview();
        }
    }

    // =========================================
    // PREVIEW REPORT
    // =========================================

    public void ShowReportPreview()
    {
        if (reportingPanel == null)
            return;

        if (reportInput != null)
            reportInput.DeactivateInputField();

        SetEntryObjectsVisible(false);

        if (previewSection != null)
            previewSection.SetActive(true);

        if (submittedSection != null)
            submittedSection.SetActive(false);

        if (previewText != null)
        {
            string report = "";

            for (int i = 0; i < reportValues.Length; i++)
            {
                report +=
                    "<b>" +
                    fieldNames[i] +
                    "</b>\n" +
                    EscapeRichText(
                        reportValues[i] ?? ""
                    ) +
                    "\n\n";
            }

            previewText.text = report;
        }
    }

    private string EscapeRichText(string value)
    {
        return value
            .Replace("&", "&amp;")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;");
    }

    // =========================================
    // EDIT REPORT
    // =========================================

    public void EditReport()
    {
        ShowField(0);
    }

    // =========================================
    // SUBMIT REPORT
    // =========================================

    public void SubmitReport()
    {
        if (reportSubmitted)
            return;

        foreach (string entry in reportValues)
        {
            if (string.IsNullOrWhiteSpace(entry))
            {
                Debug.LogWarning(
                    "Cannot submit incomplete report."
                );

                return;
            }
        }

        reportSubmitted = true;

        SetEntryObjectsVisible(false);

        if (previewSection != null)
            previewSection.SetActive(false);

        if (submittedSection != null)
            submittedSection.SetActive(true);

        if (submitStatusText != null)
        {
            submitStatusText.text =
                "Your inspection report has been " +
                "submitted to the simulated engineer.\n\n" +

                "STATUS: AWAITING ENGINEER REVIEW\n\n" +

                "Do not attempt repairs or make " +
                "operational changes without authorisation.";
        }

        Debug.Log(
            "Simulated inspection report submitted."
        );
    }

    // =========================================
    // ACCESS REPORT RESULTS
    // =========================================

    public bool IsReportSubmitted()
    {
        return reportSubmitted;
    }

    public string GetReportText()
    {
        string report = "";

        for (int i = 0; i < reportValues.Length; i++)
        {
            report +=
                fieldNames[i] +
                ": " +
                (reportValues[i] ?? "") +
                "\n";
        }

        return report;
    }
}
