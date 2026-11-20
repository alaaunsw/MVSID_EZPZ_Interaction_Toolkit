
using UnityEngine;
using UnityEngine.UI;
using TMPro;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;
using UnityEditor.SceneManagement;
#endif

public class GuidedEngineerCommunication : MonoBehaviour
{
    [Header("Previous Reporting Activity")]
    public GuidedInspectionMultipleChoice reporting;

    [Header("Engineer Communication Panel")]
    public GameObject communicationPanel;
    public bool openAfterReportSubmission = true;

    // NEW: Training progress checklist.
    [Header("Training Progress Checklist")]
    public InspectionTaskChecklist taskChecklist;

    [Header("EZPZ Interaction")]
    public MaterialLossCalculatorKey calculatorKeyTemplate;
    public bool copyEZPZInteractions = true;

    private TMP_Text stageLabel;
    private TMP_Text heading;
    private TMP_Text description;
    private TMP_Text messageText;
    private TMP_Text feedbackText;
    private TMP_Text primaryButtonText;

    private GameObject messageBox;
    private Button[] choices = new Button[3];
    private TMP_Text[] choiceLabels = new TMP_Text[3];
    private Button primaryButton;

    private int stage;
    private int selected = -1;
    private bool answerVerified;
    private bool started;
    private bool completed;

    // NEW: Prevent duplicate checklist completion.
    private bool engineeringTaskCompleted;

    private readonly bool[] completedTasks =
        new bool[3];

    private readonly Color navy =
        new Color32(26, 73, 135, 255);

    private readonly Color dark =
        new Color32(43, 55, 72, 255);

    private readonly Color paleBlue =
        new Color32(226, 237, 249, 255);

    private readonly Color grey =
        new Color32(243, 245, 248, 255);

    private readonly Color gold =
        new Color32(211, 167, 71, 255);

    private readonly Color selectedBlue =
        new Color32(45, 113, 188, 255);

    private readonly Color green =
        new Color32(20, 125, 75, 255);

    private readonly Color red =
        new Color32(175, 65, 45, 255);

    // ==========================================
    // COMMUNICATION STAGES
    // ==========================================

    // 0 = Waiting for engineer
    // 1 = Read engineer's response
    // 2 = Understand instructions
    // 3 = Record affected location
    // 4 = Preserve inspection evidence
    // 5 = Update inspection status
    // 6 = Acknowledge instructions
    // 7 = Completed

    private readonly string[] stageNames =
    {
        "Await Engineer Review",
        "Read the Engineer's Response",
        "Understand the Instructions",
        "Record the Affected Location",
        "Preserve Inspection Evidence",
        "Update Inspection Status",
        "Acknowledge the Instructions",
        "Communication Completed"
    };

    private readonly string[] instructions =
    {
        "Your inspection report has been submitted. " +
        "Wait for the simulated engineer's instructions.",

        "The engineer has reviewed your report. " +
        "Read the message carefully.",

        "What is the engineer asking you to do?",

        "Which location should you record " +
        "in the inspection follow-up?",

        "What should you do with the inspection evidence?",

        "Which status should you assign " +
        "to this inspection finding?",

        "How should you respond to the engineer?",

        "You have completed the inspection reporting " +
        "and engineer communication training."
    };

    private readonly string[,] answers =
    {
        {
            "", "", ""
        },

        {
            "", "", ""
        },

        {
            "Repair the damaged pipe immediately",
            "Document the finding and await engineering assessment",
            "Ignore the defect because it is Moderate"
        },

        {
            "PIPE_0493 — affected pipe support",
            "The control room",
            "A different pipe with no observed defect"
        },

        {
            "Delete the inspection evidence",
            "Modify the measurements to remove the defect",
            "Retain the inspection record and supporting evidence"
        },

        {
            "Closed — no further action required",
            "Pending Engineering Assessment",
            "Repair Completed"
        },

        {
            "Understood. I recorded the affected location, " +
            "retained the evidence, and marked the finding " +
            "pending engineering assessment.",

            "I repaired the pipe without authorisation.",

            "I will ignore the finding until the next inspection."
        },

        {
            "", "", ""
        }
    };

    private readonly int[] correctAnswers =
    {
        -1, -1, 1, 0, 2, 1, 0, -1
    };

    private readonly string[] successMessages =
    {
        "",

        "",

        "Correct. Follow the engineer's documented instructions.",

        "Correct. The affected location has been recorded.",

        "Correct. The inspection evidence has been retained.",

        "Correct. The finding is pending engineering assessment.",

        "Correct. Your acknowledgement is appropriate.",

        ""
    };

    private readonly string[] retryMessages =
    {
        "",

        "",

        "The engineer has not authorised a repair " +
        "or an operational change.",

        "Review the pipe ID and location in your report.",

        "Evidence must be retained for engineering review.",

        "The finding has not yet been resolved or repaired.",

        "Acknowledge only actions actually completed.",

        ""
    };

    // ==========================================
    // UNITY
    // ==========================================

    private void Start()
    {
        FindReferences();
        FindUI();
        ConnectButtons();

        engineeringTaskCompleted = false;

        if (communicationPanel != null)
            communicationPanel.SetActive(false);
    }

    private void Update()
    {
        if (!openAfterReportSubmission ||
            started ||
            reporting == null)
        {
            return;
        }

        if (reporting.IsReportSubmitted())
            OpenCommunicationTraining();
    }

    // ==========================================
    // REFERENCES
    // ==========================================

    private void FindReferences()
    {
        if (reporting == null)
        {
            reporting =
                GetComponent<
                    GuidedInspectionMultipleChoice
                >();
        }

        if (communicationPanel == null)
        {
            Transform found = transform.Find(
                "GuidedEngineerCommunicationPanel"
            );

            if (found != null)
                communicationPanel = found.gameObject;
        }
    }

    // ==========================================
    // BUILD IN EDIT MODE
    // ==========================================

    [ContextMenu("Build Engineer Communication Panel")]
    public void BuildPanel()
    {
        if (Application.isPlaying)
        {
            Debug.LogWarning(
                "Stop Play Mode before building."
            );

            return;
        }

        FindReferences();

        Canvas canvas = GetComponent<Canvas>();

        if (canvas == null ||
            canvas.renderMode != RenderMode.WorldSpace)
        {
            Debug.LogError(
                "Attach this script to the existing " +
                "World Space training Canvas."
            );

            return;
        }

        Transform existing = transform.Find(
            "GuidedEngineerCommunicationPanel"
        );

        if (existing == null)
        {
            GameObject obj = new GameObject(
                "GuidedEngineerCommunicationPanel",
                typeof(RectTransform),
                typeof(Image)
            );

            obj.transform.SetParent(
                transform, false
            );

            communicationPanel = obj;
        }
        else
        {
            communicationPanel = existing.gameObject;
        }

#if UNITY_EDITOR

        Undo.RegisterFullObjectHierarchyUndo(
            communicationPanel,
            "Build Engineer Communication Panel"
        );

#endif

        Transform root =
            communicationPanel.transform;

        SetRect(root, 0, 0, 820, 760);

        Image background =
            communicationPanel.GetComponent<Image>();

        if (background == null)
        {
            background =
                communicationPanel.AddComponent<Image>();
        }

        background.color = Color.white;
        background.raycastTarget = true;

        // ======================================
        // HEADER
        // ======================================

        MakeText(
            root,
            "PanelTitle",
            "ENGINEER COMMUNICATION",
            27, navy, FontStyles.Bold,
            -45, 326, 640, 42,
            TextAlignmentOptions.Left
        );

        stageLabel = MakeText(
            root,
            "StageLabel",
            "STEP 7",
            18, navy, FontStyles.Bold,
            285, 282, 145, 32,
            TextAlignmentOptions.Right
        );

        MakePanel(
            root,
            "GoldDivider",
            gold,
            0, 264, 730, 3
        );

        // ======================================
        // INSTRUCTIONS
        // ======================================

        heading = MakeText(
            root,
            "LearningHeading",
            "Await Engineer Review",
            30, dark, FontStyles.Bold,
            0, 209, 710, 52,
            TextAlignmentOptions.Left
        );

        description = MakeText(
            root,
            "InstructionText",
            "",
            21, dark, FontStyles.Normal,
            0, 150, 710, 62,
            TextAlignmentOptions.Left
        );

        messageBox = MakePanel(
            root,
            "EngineerMessageBackground",
            grey,
            0, 40, 710, 137
        ).gameObject;

        messageText = MakeText(
            messageBox.transform,
            "EngineerMessageText",
            "",
            19, dark, FontStyles.Normal,
            0, 0, 660, 123,
            TextAlignmentOptions.Left
        );

        // ======================================
        // THREE ANSWER BUTTONS
        // ======================================

        for (int i = 0; i < 3; i++)
        {
            float y = -82 - i * 65;

            Button button = MakeButton(
                root,
                "ChoiceButton_" + i,
                "OPTION " + (i + 1),
                0, y, 710, 56,
                paleBlue, navy
            );

            EnsureKey(
                button,
                "CHOICE_" + i
            );

            choices[i] = button;

            choiceLabels[i] =
                button.GetComponentInChildren<
                    TMP_Text
                >(true);
        }

        // ======================================
        // FEEDBACK
        // ======================================

        feedbackText = MakeText(
            root,
            "FeedbackText",
            "",
            18, dark, FontStyles.Normal,
            0, -277, 710, 47,
            TextAlignmentOptions.Center
        );

        // ======================================
        // ACTION BUTTON
        // ======================================

        primaryButton = MakeButton(
            root,
            "PrimaryButton",
            "CONTINUE",
            0, -328, 710, 55,
            navy, Color.white
        );

        EnsureKey(
            primaryButton,
            "PRIMARY"
        );

        FindUI();
        ShowStage(0);

        communicationPanel.transform
            .SetAsLastSibling();

#if UNITY_EDITOR

        if (copyEZPZInteractions)
            ConfigureEZPZInteractions();

#endif

        communicationPanel.SetActive(false);

#if UNITY_EDITOR

        EditorUtility.SetDirty(this);

        EditorSceneManager.MarkSceneDirty(
            gameObject.scene
        );

#endif

        Debug.Log(
            "Engineer Communication panel built."
        );
    }

    // ==========================================
    // CREATE UI
    // ==========================================

    private Transform MakePanel(
        Transform parent,
        string name,
        Color color,
        float x,
        float y,
        float width,
        float height)
    {
        Transform child = parent.Find(name);

        if (child == null)
        {
            GameObject obj = new GameObject(
                name,
                typeof(RectTransform),
                typeof(Image)
            );

            obj.transform.SetParent(
                parent, false
            );

            child = obj.transform;
        }

        SetRect(
            child, x, y, width, height
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

    private TMP_Text MakeText(
        Transform parent,
        string name,
        string content,
        int size,
        Color color,
        FontStyles style,
        float x,
        float y,
        float width,
        float height,
        TextAlignmentOptions alignment)
    {
        Transform child = parent.Find(name);

        if (child == null)
        {
            GameObject obj = new GameObject(
                name,
                typeof(RectTransform),
                typeof(TextMeshProUGUI)
            );

            obj.transform.SetParent(
                parent, false
            );

            child = obj.transform;
        }

        SetRect(
            child, x, y, width, height
        );

        TMP_Text text =
            child.GetComponent<TMP_Text>();

        text.text = content;
        text.fontSize = size;
        text.fontStyle = style;
        text.color = color;
        text.alignment = alignment;

        text.verticalAlignment =
            VerticalAlignmentOptions.Middle;

        text.enableWordWrapping = true;
        text.raycastTarget = false;

        return text;
    }

    private Button MakeButton(
        Transform parent,
        string name,
        string caption,
        float x,
        float y,
        float width,
        float height,
        Color background,
        Color foreground)
    {
        Transform child = parent.Find(name);

        if (child == null)
        {
            GameObject obj = new GameObject(
                name,
                typeof(RectTransform),
                typeof(Image),
                typeof(Button),
                typeof(BoxCollider)
            );

            obj.transform.SetParent(
                parent, false
            );

            child = obj.transform;
        }

        SetRect(
            child, x, y, width, height
        );

        Image image =
            child.GetComponent<Image>();

        if (image == null)
        {
            image =
                child.gameObject.AddComponent<Image>();
        }

        image.color = background;
        image.raycastTarget = true;

        Button button =
            child.GetComponent<Button>();

        if (button == null)
        {
            button =
                child.gameObject.AddComponent<Button>();
        }

        button.targetGraphic = image;
        button.interactable = true;

        button.transition =
            Selectable.Transition.None;

        BoxCollider collider =
            child.GetComponent<BoxCollider>();

        if (collider == null)
        {
            collider =
                child.gameObject.AddComponent<BoxCollider>();
        }

        collider.center = Vector3.zero;

        collider.size = new Vector3(
            width, height, 4f
        );

        TMP_Text label = MakeText(
            child,
            "ButtonText",
            caption,
            19, foreground, FontStyles.Bold,
            0, 0,
            width - 24,
            height - 8,
            TextAlignmentOptions.Center
        );

        label.raycastTarget = false;

        return button;
    }

    private EngineerCommunicationButton EnsureKey(
        Button button,
        string action)
    {
        EngineerCommunicationButton key =
            button.GetComponent<
                EngineerCommunicationButton
            >();

        if (key == null)
        {
            key = button.gameObject.AddComponent<
                EngineerCommunicationButton
            >();
        }

        key.training = this;
        key.action = action;

        return key;
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

    // ==========================================
    // FIND UI
    // ==========================================

    private void FindUI()
    {
        if (communicationPanel == null)
            return;

        Transform root =
            communicationPanel.transform;

        stageLabel =
            FindText(root, "StageLabel");

        heading =
            FindText(root, "LearningHeading");

        description =
            FindText(root, "InstructionText");

        Transform message =
            root.Find("EngineerMessageBackground");

        messageBox =
            message != null
            ? message.gameObject
            : null;

        messageText = FindText(
            message,
            "EngineerMessageText"
        );

        feedbackText =
            FindText(root, "FeedbackText");

        for (int i = 0; i < 3; i++)
        {
            Transform found = root.Find(
                "ChoiceButton_" + i
            );

            choices[i] =
                found != null
                ? found.GetComponent<Button>()
                : null;

            choiceLabels[i] =
                found != null
                ? found.GetComponentInChildren<
                    TMP_Text
                >(true)
                : null;
        }

        Transform primary =
            root.Find("PrimaryButton");

        primaryButton =
            primary != null
            ? primary.GetComponent<Button>()
            : null;

        primaryButtonText =
            primary != null
            ? primary.GetComponentInChildren<
                TMP_Text
            >(true)
            : null;
    }

    private TMP_Text FindText(
        Transform parent,
        string name)
    {
        if (parent == null)
            return null;

        Transform found =
            parent.Find(name);

        return found != null
            ? found.GetComponent<TMP_Text>()
            : null;
    }

    // ==========================================
    // CONNECT UNITY BUTTONS
    // ==========================================

    private void ConnectButtons()
    {
        if (communicationPanel == null)
            return;

        EngineerCommunicationButton[] keys =
            communicationPanel.GetComponentsInChildren<
                EngineerCommunicationButton
            >(true);

        foreach (EngineerCommunicationButton key in keys)
        {
            key.training = this;

            Button button =
                key.GetComponent<Button>();

            if (button == null)
                continue;

            button.onClick.RemoveListener(
                key.Activate
            );

            button.onClick.AddListener(
                key.Activate
            );
        }
    }

    public void HandleAction(string action)
    {
        if (completed)
            return;

        if (action.StartsWith("CHOICE_"))
        {
            int index;

            if (int.TryParse(
                action.Substring(7),
                out index
            ))
            {
                SelectChoice(index);
            }

            return;
        }

        if (action == "PRIMARY")
            PrimaryAction();
    }

    // ==========================================
    // OPEN AFTER REPORT SUBMISSION
    // ==========================================

    [ContextMenu("Open Engineer Communication")]
    public void OpenCommunicationTraining()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning(
                "Open communication in Play Mode."
            );

            return;
        }

        FindReferences();
        FindUI();
        ConnectButtons();

        if (communicationPanel == null)
        {
            Debug.LogError(
                "Build Engineer Communication Panel first."
            );

            return;
        }

        started = true;
        completed = false;

        // NEW: Reset checklist completion guard.
        engineeringTaskCompleted = false;

        for (int i = 0;
             i < completedTasks.Length;
             i++)
        {
            completedTasks[i] = false;
        }

        if (reporting != null &&
            reporting.multipleChoicePanel != null)
        {
            reporting.multipleChoicePanel
                .SetActive(false);
        }

        communicationPanel.SetActive(true);

        communicationPanel.transform
            .SetAsLastSibling();

        ShowStage(0);
    }

    // ==========================================
    // DISPLAY TRAINING STAGES
    // ==========================================

    private void ShowStage(int nextStage)
    {
        stage = Mathf.Clamp(
            nextStage, 0, 7
        );

        selected = -1;
        answerVerified = false;

        if (heading != null)
            heading.text = stageNames[stage];

        if (stageLabel != null)
            stageLabel.text = "STEP 7";

        if (description != null)
            description.text = instructions[stage];

        // Engineer message or status.

        string message =
            GetStageMessage();

        if (messageBox != null)
            messageBox.SetActive(true);

        if (messageText != null)
            messageText.text = message;

        bool requiresChoice =
            stage >= 2 && stage <= 6;

        for (int i = 0; i < 3; i++)
        {
            if (choices[i] == null)
                continue;

            choices[i].gameObject.SetActive(
                requiresChoice
            );

            if (!requiresChoice)
                continue;

            Image image =
                choices[i].GetComponent<Image>();

            if (image != null)
                image.color = paleBlue;

            if (choiceLabels[i] != null)
            {
                choiceLabels[i].text =
                    answers[stage, i];

                choiceLabels[i].color = navy;
            }
        }

        if (feedbackText != null)
        {
            feedbackText.color = dark;

            feedbackText.text =
                requiresChoice
                ? "Select an answer, then check it."
                : "";
        }

        if (primaryButton != null)
        {
            primaryButton.gameObject.SetActive(
                stage != 7
            );
        }

        if (primaryButtonText != null)
        {
            switch (stage)
            {
                case 0:

                    primaryButtonText.text =
                        "RECEIVE ENGINEER RESPONSE";

                    break;

                case 1:

                    primaryButtonText.text =
                        "I HAVE READ THE INSTRUCTIONS";

                    break;

                default:

                    primaryButtonText.text =
                        requiresChoice
                        ? "CHECK RESPONSE"
                        : "CONTINUE";

                    break;
            }
        }

        // ==========================================
        // FINAL ENGINEER COMMUNICATION STAGE
        // ==========================================

        if (stage == 7)
        {
            completed = true;

            if (feedbackText != null)
            {
                feedbackText.color = green;

                feedbackText.text =
                    "All communication tasks completed.";
            }

            // NEW: Complete the fifth checklist task.

            CompleteEngineeringAssessmentTask();

            Debug.Log(
                "Step 7: Engineer Communication completed."
            );
        }
    }

    // ==========================================
    // NEW: COMPLETE CHECKLIST TASK 5
    // ==========================================

    private void CompleteEngineeringAssessmentTask()
    {
        if (!Application.isPlaying)
            return;

        if (engineeringTaskCompleted)
            return;

        if (!completed)
            return;

        // Require a submitted report.

        if (reporting == null ||
            !reporting.IsReportSubmitted())
        {
            Debug.LogWarning(
                "Inspection report must be submitted first."
            );

            return;
        }

        // Ensure all three communication tasks
        // have been successfully completed.

        if (!completedTasks[0] ||
            !completedTasks[1] ||
            !completedTasks[2])
        {
            Debug.LogWarning(
                "Complete all engineer communication tasks."
            );

            return;
        }

        // Find the checklist automatically
        // if the Inspector reference is missing.

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

        // Complete Task 5:
        // Wait for engineering assessment.

        taskChecklist.CompleteTask4();

        engineeringTaskCompleted = true;

        Debug.Log(
            "Training Checklist: " +
            "Engineering communication completed."
        );
    }

    // ==========================================
    // ENGINEER MESSAGES
    // ==========================================

    private string GetStageMessage()
    {
        switch (stage)
        {
            case 0:

                return
                    "<b>REPORT STATUS</b>\n" +
                    "PIPE_0493: SUBMITTED\n" +
                    "Awaiting simulated engineer review.\n" +
                    "No repair or operational changes authorised.";

            case 1:
            case 2:

                return
                    "<b>ENGINEER:</b>\n" +
                    "Report received for PIPE_0493. " +
                    "Record the affected pipe support location, " +
                    "retain the inspection evidence, and mark " +
                    "the finding as pending engineering assessment. " +
                    "Do not attempt repairs or change operations " +
                    "without authorisation.";

            case 3:

                return
                    "<b>TASK 1 — RECORD LOCATION</b>\n" +
                    "Identify the affected asset and support " +
                    "location from your inspection report.";

            case 4:

                return
                    "<b>TASK 2 — PRESERVE EVIDENCE</b>\n" +
                    "Keep the inspection measurements and " +
                    "supporting observations available for review.";

            case 5:

                return
                    "<b>TASK 3 — UPDATE STATUS</b>\n" +
                    "This finding has not yet received an " +
                    "engineering disposition.";

            case 6:

                return
                    "<b>ENGINEER:</b>\n" +
                    "Please confirm that the requested " +
                    "documentation tasks are complete.";

            case 7:

                return
                    "<b>TRAINING COMPLETE</b>\n" +
                    "PIPE_0493 location recorded.\n" +
                    "Inspection evidence retained.\n" +
                    "Status: Pending Engineering Assessment.\n" +
                    "Acknowledgement sent to simulated engineer.";

            default:

                return "";
        }
    }

    // ==========================================
    // SELECT AN ANSWER
    // ==========================================

    private void SelectChoice(int index)
    {
        if (stage < 2 ||
            stage > 6 ||
            answerVerified ||
            index < 0 ||
            index > 2)
        {
            return;
        }

        selected = index;

        for (int i = 0; i < 3; i++)
        {
            if (choices[i] == null)
                continue;

            Image image =
                choices[i].GetComponent<Image>();

            if (image != null)
            {
                image.color =
                    i == selected
                    ? selectedBlue
                    : paleBlue;
            }

            if (choiceLabels[i] != null)
            {
                choiceLabels[i].color =
                    i == selected
                    ? Color.white
                    : navy;
            }
        }

        if (feedbackText != null)
        {
            feedbackText.color = dark;

            feedbackText.text =
                "Selection recorded. Press CHECK RESPONSE.";
        }
    }

    // ==========================================
    // HANDLE PRIMARY BUTTON
    // ==========================================

    public void PrimaryAction()
    {
        if (completed)
            return;

        if (stage == 0 || stage == 1)
        {
            ShowStage(stage + 1);

            return;
        }

        if (stage >= 2 && stage <= 6)
        {
            if (!answerVerified)
            {
                CheckResponse();
            }
            else
            {
                ShowStage(stage + 1);
            }
        }
    }

    // ==========================================
    // CHECK RESPONSE
    // ==========================================

    private void CheckResponse()
    {
        if (selected < 0)
        {
            SetFeedback(
                "Please select an answer.",
                red
            );

            return;
        }

        if (selected != correctAnswers[stage])
        {
            SetFeedback(
                retryMessages[stage],
                red
            );

            return;
        }

        answerVerified = true;

        if (stage == 3)
            completedTasks[0] = true;

        if (stage == 4)
            completedTasks[1] = true;

        if (stage == 5)
            completedTasks[2] = true;

        SetFeedback(
            successMessages[stage],
            green
        );

        if (primaryButtonText != null)
        {
            primaryButtonText.text =
                stage == 6
                ? "COMPLETE COMMUNICATION"
                : "CONTINUE";
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

    // ==========================================
    // PUBLIC STATUS
    // ==========================================

    public bool HasCompletedCommunication()
    {
        return completed;
    }

    public bool HasRecordedLocation()
    {
        return completedTasks[0];
    }

    public bool HasPreservedEvidence()
    {
        return completedTasks[1];
    }

    public bool HasUpdatedStatus()
    {
        return completedTasks[2];
    }

    // ==========================================
    // EDIT MODE PREVIEW
    // ==========================================

    [ContextMenu("Preview Engineer Communication")]
    public void PreviewCommunication()
    {
        if (Application.isPlaying)
            return;

        FindReferences();
        FindUI();

        if (communicationPanel == null)
            return;

        communicationPanel.SetActive(true);

        ShowStage(0);

#if UNITY_EDITOR

        EditorSceneManager.MarkSceneDirty(
            gameObject.scene
        );

#endif
    }

    [ContextMenu("Hide Engineer Communication Preview")]
    public void HideCommunicationPreview()
    {
        if (Application.isPlaying)
            return;

        if (communicationPanel != null)
            communicationPanel.SetActive(false);

#if UNITY_EDITOR

        EditorSceneManager.MarkSceneDirty(
            gameObject.scene
        );

#endif
    }

    // ==========================================
    // OPTIONAL EZPZ AUTO-CONNECTION
    // ==========================================

#if UNITY_EDITOR

    [ContextMenu("Connect Engineer EZPZ Interactions")]
    public void ConfigureEZPZInteractions()
    {
        if (Application.isPlaying)
            return;

        FindReferences();

        if (communicationPanel == null ||
            calculatorKeyTemplate == null)
        {
            Debug.LogWarning(
                "Assign a working calculator key " +
                "and build the communication panel."
            );

            return;
        }

        Component donor = null;

        foreach (Component component in
            calculatorKeyTemplate
                .GetComponents<Component>())
        {
            if (component != null &&
                component.GetType().Name ==
                    "InteractableGeneral")
            {
                donor = component;

                break;
            }
        }

        if (donor == null)
        {
            Debug.LogWarning(
                "The calculator key template has no " +
                "InteractableGeneral component."
            );

            return;
        }

        int connected = 0;
        int unresolved = 0;

        EngineerCommunicationButton[] keys =
            communicationPanel.GetComponentsInChildren<
                EngineerCommunicationButton
            >(true);

        foreach (EngineerCommunicationButton key in keys)
        {
            Component interaction = null;

            foreach (Component component in
                key.GetComponents<Component>())
            {
                if (component != null &&
                    component.GetType().Name ==
                        "InteractableGeneral")
                {
                    interaction = component;

                    break;
                }
            }

            if (interaction == null)
            {
                ComponentUtility.CopyComponent(
                    donor
                );

                bool pasted =
                    ComponentUtility.PasteComponentAsNew(
                        key.gameObject
                    );

                if (!pasted)
                {
                    unresolved++;

                    continue;
                }

                foreach (Component component in
                    key.GetComponents<Component>())
                {
                    if (component != null &&
                        component.GetType().Name ==
                            "InteractableGeneral")
                    {
                        interaction = component;

                        break;
                    }
                }
            }

            if (interaction == null)
            {
                unresolved++;

                continue;
            }

            SerializedObject serialized =
                new SerializedObject(interaction);

            SerializedProperty iterator =
                serialized.GetIterator();

            bool rewired = false;

            while (iterator.Next(true))
            {
                if (!iterator.isArray ||
                    !iterator.propertyPath.EndsWith(
                        "m_PersistentCalls.m_Calls"
                    ))
                {
                    continue;
                }

                for (int i = 0;
                     i < iterator.arraySize;
                     i++)
                {
                    SerializedProperty call =
                        iterator.GetArrayElementAtIndex(i);

                    SerializedProperty target =
                        call.FindPropertyRelative(
                            "m_Target"
                        );

                    SerializedProperty method =
                        call.FindPropertyRelative(
                            "m_MethodName"
                        );

                    if (target == null ||
                        method == null)
                    {
                        continue;
                    }

                    if (target.objectReferenceValue ==
                        calculatorKeyTemplate)
                    {
                        target.objectReferenceValue =
                            key;

                        method.stringValue =
                            "Activate";

                        rewired = true;
                    }
                }
            }

            if (rewired)
            {
                serialized.ApplyModifiedProperties();

                EditorUtility.SetDirty(interaction);

                connected++;
            }
            else
            {
                unresolved++;
            }
        }

        Debug.Log(
            "Engineer EZPZ callbacks connected: " +
            connected
        );

        if (unresolved > 0)
        {
            Debug.LogWarning(
                unresolved +
                " buttons need their EZPZ " +
                "Primary Interact events checked."
            );
        }

        EditorSceneManager.MarkSceneDirty(
            gameObject.scene
        );
    }

#endif

}
