
using UnityEngine;
using UnityEngine.UI;
using TMPro;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;
using UnityEditor.SceneManagement;
#endif

public class GuidedInspectionMultipleChoice : MonoBehaviour
{
    [Header("Previous Training")]
    public GuidedMaterialLossTraining materialLossTraining;
    public GameObject conditionTrainingPanel;

    [Header("Separate Multiple Choice Panel")]
    public GameObject multipleChoicePanel;

    // NEW: Training progress checklist.
    [Header("Training Progress Checklist")]
    public InspectionTaskChecklist taskChecklist;

    [Header("Training Transition")]
    public bool openAfterConditionTraining = true;

    [Header("EZPZ Interaction")]
    public MaterialLossCalculatorKey calculatorKeyTemplate;
    public bool copyEZPZInteractions = true;

    private Transform content;
    private GameObject questionScreen;
    private GameObject previewScreen;
    private GameObject submittedScreen;

    private TMP_Text fieldNumberText;
    private TMP_Text headingText;
    private TMP_Text instructionText;
    private TMP_Text explanationText;
    private TMP_Text feedbackText;
    private TMP_Text previewText;
    private TMP_Text submissionText;

    private Button[] choiceButtons = new Button[3];
    private TMP_Text[] choiceLabels = new TMP_Text[3];

    private Button checkButton;
    private Button nextButton;
    private Button editButton;
    private Button submitButton;

    private int currentField;
    private int selectedChoice = -1;

    private bool answerCorrect;
    private bool reportingOpened;
    private bool reportSubmitted;

    private readonly string[] recordedAnswers =
        new string[7];

    // =========================================
    // COLOURS
    // =========================================

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
        new Color32(23, 125, 76, 255);

    private readonly Color red =
        new Color32(177, 66, 54, 255);

    // =========================================
    // SEVEN REPORTING QUESTIONS
    // =========================================

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
        "Which pipe are you reporting?",

        "Where was the defect found?",

        "Which statement correctly describes the finding?",

        "Which wall thickness measurements are correct?",

        "Which calculated material loss results are correct?",

        "How should this finding be classified?",

        "What should happen after recording the finding?"
    };

    private readonly string[] explanations =
    {
        "The engineer needs to identify the correct pipe.",

        "The location helps the engineer find " +
        "the affected section.",

        "Describe the observed defect without " +
        "inventing additional damage.",

        "The report must contain the correct " +
        "original and remaining wall thickness.",

        "These calculations describe the extent " +
        "of the simulated wall loss.",

        "Use the condition ranges taught " +
        "in the previous activity.",

        "Inspection findings must be referred " +
        "for authorised engineering assessment."
    };

    private readonly string[,] answers =
    {
        {
            "PIPE_0043",
            "PIPE_0493",
            "PIPE_0007"
        },

        {
            "Inside the control room",
            "At the pipe outlet",
            "At the affected pipe support"
        },

        {
            "Corrosion observed at the pipe support",
            "No visible damage was observed",
            "A confirmed active leak was observed"
        },

        {
            "11.11 mm original / 7.70 mm remaining",
            "7.70 mm original / 11.11 mm remaining",
            "11.11 mm original / 3.41 mm remaining"
        },

        {
            "2.41 mm loss / 20.69%",
            "5.41 mm loss / 48.69%",
            "3.41 mm loss / 30.69%"
        },

        {
            "Normal",
            "Moderate",
            "Critical"
        },

        {
            "Refer the finding for engineering assessment",
            "Repair the pipe without authorisation",
            "Ignore the finding and close the inspection"
        }
    };

    private readonly int[] correctAnswers =
    {
        1, 2, 0, 0, 2, 1, 0
    };

    private readonly string[] successMessages =
    {
        "Correct. The pipe ID has been recorded.",

        "Correct. The affected location has been recorded.",

        "Correct. The observed defect has been recorded.",

        "Correct. Both measurements have been recorded.",

        "Correct. The calculated results have been recorded.",

        "Correct. The condition is Moderate.",

        "Correct. The finding requires engineering review."
    };

    private readonly string[] retryMessages =
    {
        "Look at the pipe ID on the inspection card.",

        "Think about where the corrosion was inspected.",

        "Select only what was observed in the scenario.",

        "Review the original and remaining thickness values.",

        "Review your material-loss calculation.",

        "Compare the 3.41 mm loss with the condition ranges.",

        "Engineering decisions require authorisation."
    };

    // =========================================
    // UNITY LIFECYCLE
    // =========================================

    private void Start()
    {
        FindReferences();
        FindUI();
        ConnectButtons();

        if (multipleChoicePanel != null)
        {
            ShowQuestion(0);
            multipleChoicePanel.SetActive(false);
        }
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
            OpenMultipleChoiceTraining();
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
                conditionTrainingPanel = found.gameObject;
        }

        if (multipleChoicePanel == null)
        {
            Transform found =
                transform.Find(
                    "GuidedInspectionMultipleChoicePanel"
                );

            if (found != null)
                multipleChoicePanel = found.gameObject;
        }
    }

    // =========================================
    // BUILD NEW PANEL IN EDIT MODE
    // =========================================

    [ContextMenu("Build Multiple Choice Report")]
    public void BuildMultipleChoiceReport()
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

        // This creates a separate panel.
        // Existing report and keyboard panels
        // are not modified.

        Transform existing =
            transform.Find(
                "GuidedInspectionMultipleChoicePanel"
            );

        if (existing == null)
        {
            GameObject obj = new GameObject(
                "GuidedInspectionMultipleChoicePanel",
                typeof(RectTransform),
                typeof(Image)
            );

            obj.transform.SetParent(
                transform,
                false
            );

            multipleChoicePanel = obj;
        }
        else
        {
            multipleChoicePanel = existing.gameObject;
        }

#if UNITY_EDITOR

        Undo.RegisterFullObjectHierarchyUndo(
            multipleChoicePanel,
            "Build Multiple Choice Report"
        );

#endif

        Transform root =
            multipleChoicePanel.transform;

        SetRect(root, 0, 0, 820, 760);

        Image background =
            multipleChoicePanel.GetComponent<Image>();

        if (background == null)
        {
            background =
                multipleChoicePanel.AddComponent<Image>();
        }

        background.color = Color.white;
        background.raycastTarget = true;

        Transform mainContent = MakeContainer(
            root,
            "MultipleChoiceContent",
            0, 0, 820, 760
        );

        content = mainContent;

        // =====================================
        // HEADER
        // =====================================

        MakeText(
            content,
            "ReportTitle",
            "REPORT YOUR FINDINGS",
            27, navy, FontStyles.Bold,
            -60, 326, 610, 45,
            TextAlignmentOptions.Left
        );

        MakeText(
            content,
            "StepTitle",
            "STEP 6",
            19, navy, FontStyles.Bold,
            304, 285, 110, 30,
            TextAlignmentOptions.Right
        );

        MakePanel(
            content,
            "GoldDivider",
            gold,
            0, 264, 730, 3
        );

        // =====================================
        // QUESTION SCREEN
        // =====================================

        Transform question = MakeContainer(
            content,
            "QuestionScreen",
            0, 0, 780, 570
        );

        questionScreen = question.gameObject;

        fieldNumberText = MakeText(
            question,
            "FieldNumber",
            "REPORT FIELD 1 OF 7",
            18, navy, FontStyles.Bold,
            -225, 198, 260, 34,
            TextAlignmentOptions.Left
        );

        headingText = MakeText(
            question,
            "LearningHeading",
            "Pipe Identification",
            31, dark, FontStyles.Bold,
            0, 146, 710, 55,
            TextAlignmentOptions.Left
        );

        instructionText = MakeText(
            question,
            "InstructionText",
            "",
            22, dark, FontStyles.Normal,
            0, 86, 710, 50,
            TextAlignmentOptions.Left
        );

        Transform explanationBox = MakePanel(
            question,
            "WhyThisMattersBox",
            grey,
            0, 5, 710, 88
        );

        explanationText = MakeText(
            explanationBox,
            "ExplanationText",
            "",
            19, dark, FontStyles.Normal,
            0, 0, 668, 78,
            TextAlignmentOptions.Left
        );

        // =====================================
        // THREE ANSWER BUTTONS
        // =====================================

        for (int i = 0; i < 3; i++)
        {
            float y = -88 - i * 66;

            Button button = MakeButton(
                question,
                "ChoiceButton_" + i,
                "OPTION " + (i + 1),
                0, y, 710, 56,
                paleBlue, navy
            );

            EnsureChoiceKey(
                button,
                "SELECT_" + i
            );

            choiceButtons[i] = button;

            choiceLabels[i] =
                button.GetComponentInChildren<
                    TMP_Text
                >(true);
        }

        feedbackText = MakeText(
            question,
            "FeedbackText",
            "Select the correct answer.",
            18, dark, FontStyles.Normal,
            0, -273, 710, 38,
            TextAlignmentOptions.Center
        );

        checkButton = MakeButton(
            question,
            "CheckAnswerButton",
            "CHECK ANSWER",
            0, -325, 710, 55,
            navy, Color.white
        );

        EnsureChoiceKey(
            checkButton,
            "CHECK"
        );

        nextButton = MakeButton(
            question,
            "NextFieldButton",
            "NEXT FIELD",
            0, -325, 710, 55,
            navy, Color.white
        );

        EnsureChoiceKey(
            nextButton,
            "NEXT"
        );

        // =====================================
        // REPORT PREVIEW
        // =====================================

        Transform preview = MakeContainer(
            content,
            "ReportPreviewScreen",
            0, 0, 780, 570
        );

        previewScreen = preview.gameObject;

        MakeText(
            preview,
            "PreviewHeading",
            "REVIEW YOUR INSPECTION REPORT",
            28, navy, FontStyles.Bold,
            0, 210, 710, 48,
            TextAlignmentOptions.Left
        );

        previewText = MakeText(
            preview,
            "PreviewBody",
            "",
            19, dark, FontStyles.Normal,
            0, -3, 710, 360,
            TextAlignmentOptions.Left
        );

        editButton = MakeButton(
            preview,
            "EditReportButton",
            "REVIEW AGAIN",
            -184, -245, 340, 54,
            paleBlue, navy
        );

        EnsureChoiceKey(
            editButton,
            "EDIT"
        );

        submitButton = MakeButton(
            preview,
            "SubmitReportButton",
            "SUBMIT TO ENGINEER",
            184, -245, 340, 54,
            navy, Color.white
        );

        EnsureChoiceKey(
            submitButton,
            "SUBMIT"
        );

        // =====================================
        // SUBMISSION SCREEN
        // =====================================

        Transform submitted = MakeContainer(
            content,
            "ReportSubmittedScreen",
            0, 0, 780, 570
        );

        submittedScreen = submitted.gameObject;

        MakeText(
            submitted,
            "SubmittedHeading",
            "REPORT SUBMITTED",
            32, navy, FontStyles.Bold,
            0, 155, 700, 55,
            TextAlignmentOptions.Center
        );

        submissionText = MakeText(
            submitted,
            "SubmissionText",
            "",
            23, dark, FontStyles.Normal,
            0, -10, 690, 255,
            TextAlignmentOptions.Center
        );

        // =====================================
        // FINALISE THE PANEL
        // =====================================

        FindUI();
        ShowQuestion(0);

        multipleChoicePanel.transform
            .SetAsLastSibling();

#if UNITY_EDITOR

        if (copyEZPZInteractions)
            ConfigureEZPZInteractions();

#endif

        multipleChoicePanel.SetActive(false);

#if UNITY_EDITOR

        EditorUtility.SetDirty(this);

        EditorSceneManager.MarkSceneDirty(
            gameObject.scene
        );

#endif

        Debug.Log(
            "Separate multiple-choice reporting panel built."
        );
    }

    // =========================================
    // UI FACTORY
    // =========================================

    private Transform MakeContainer(
        Transform parent,
        string name,
        float x, float y,
        float width, float height)
    {
        Transform found = parent.Find(name);

        if (found == null)
        {
            GameObject obj = new GameObject(
                name,
                typeof(RectTransform)
            );

            obj.transform.SetParent(
                parent, false
            );

            found = obj.transform;
        }

        SetRect(
            found, x, y, width, height
        );

        return found;
    }

    private Transform MakePanel(
        Transform parent,
        string name,
        Color color,
        float x, float y,
        float width, float height)
    {
        Transform found = parent.Find(name);

        if (found == null)
        {
            GameObject obj = new GameObject(
                name,
                typeof(RectTransform),
                typeof(Image)
            );

            obj.transform.SetParent(
                parent, false
            );

            found = obj.transform;
        }

        SetRect(
            found, x, y, width, height
        );

        Image image =
            found.GetComponent<Image>();

        if (image != null)
        {
            image.color = color;
            image.raycastTarget = false;
        }

        return found;
    }

    private TMP_Text MakeText(
        Transform parent,
        string name,
        string value,
        int size,
        Color color,
        FontStyles style,
        float x, float y,
        float width, float height,
        TextAlignmentOptions alignment)
    {
        Transform found = parent.Find(name);

        if (found == null)
        {
            GameObject obj = new GameObject(
                name,
                typeof(RectTransform),
                typeof(TextMeshProUGUI)
            );

            obj.transform.SetParent(
                parent, false
            );

            found = obj.transform;
        }

        SetRect(
            found, x, y, width, height
        );

        TMP_Text text =
            found.GetComponent<TMP_Text>();

        text.text = value;
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
        float x, float y,
        float width, float height,
        Color background,
        Color foreground)
    {
        Transform found = parent.Find(name);

        if (found == null)
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

            found = obj.transform;
        }

        SetRect(
            found, x, y, width, height
        );

        Image image =
            found.GetComponent<Image>();

        if (image == null)
        {
            image =
                found.gameObject.AddComponent<Image>();
        }

        image.color = background;
        image.raycastTarget = true;

        Button button =
            found.GetComponent<Button>();

        if (button == null)
        {
            button =
                found.gameObject.AddComponent<Button>();
        }

        button.targetGraphic = image;
        button.interactable = true;

        button.transition =
            Selectable.Transition.None;

        BoxCollider collider =
            found.GetComponent<BoxCollider>();

        if (collider == null)
        {
            collider =
                found.gameObject.AddComponent<BoxCollider>();
        }

        collider.center = Vector3.zero;

        collider.size = new Vector3(
            width, height, 4f
        );

        TMP_Text text = MakeText(
            found,
            "ButtonText",
            caption,
            20, foreground, FontStyles.Bold,
            0, 0,
            width - 28,
            height - 10,
            TextAlignmentOptions.Center
        );

        text.raycastTarget = false;

        return button;
    }

    private ReportChoiceButton EnsureChoiceKey(
        Button button,
        string action)
    {
        ReportChoiceButton key =
            button.GetComponent<ReportChoiceButton>();

        if (key == null)
        {
            key = button.gameObject.AddComponent<
                ReportChoiceButton
            >();
        }

        key.reporting = this;
        key.action = action;

        return key;
    }

    private void SetRect(
        Transform target,
        float x, float y,
        float width, float height)
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
    // FIND GENERATED UI
    // =========================================

    private void FindUI()
    {
        if (multipleChoicePanel == null)
            return;

        content =
            multipleChoicePanel.transform.Find(
                "MultipleChoiceContent"
            );

        if (content == null)
            return;

        Transform question =
            content.Find("QuestionScreen");

        Transform preview =
            content.Find("ReportPreviewScreen");

        Transform submitted =
            content.Find("ReportSubmittedScreen");

        questionScreen =
            question != null
            ? question.gameObject
            : null;

        previewScreen =
            preview != null
            ? preview.gameObject
            : null;

        submittedScreen =
            submitted != null
            ? submitted.gameObject
            : null;

        if (question != null)
        {
            fieldNumberText =
                FindText(
                    question,
                    "FieldNumber"
                );

            headingText =
                FindText(
                    question,
                    "LearningHeading"
                );

            instructionText =
                FindText(
                    question,
                    "InstructionText"
                );

            Transform explanationBox =
                question.Find(
                    "WhyThisMattersBox"
                );

            explanationText =
                FindText(
                    explanationBox,
                    "ExplanationText"
                );

            feedbackText =
                FindText(
                    question,
                    "FeedbackText"
                );

            for (int i = 0; i < 3; i++)
            {
                choiceButtons[i] = FindButton(
                    question,
                    "ChoiceButton_" + i
                );

                if (choiceButtons[i] != null)
                {
                    choiceLabels[i] =
                        choiceButtons[i]
                        .GetComponentInChildren<
                            TMP_Text
                        >(true);
                }
            }

            checkButton = FindButton(
                question,
                "CheckAnswerButton"
            );

            nextButton = FindButton(
                question,
                "NextFieldButton"
            );
        }

        if (preview != null)
        {
            previewText =
                FindText(
                    preview,
                    "PreviewBody"
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

        if (submitted != null)
        {
            submissionText =
                FindText(
                    submitted,
                    "SubmissionText"
                );
        }
    }

    private TMP_Text FindText(
        Transform parent,
        string name)
    {
        if (parent == null)
            return null;

        Transform found = parent.Find(name);

        return found != null
            ? found.GetComponent<TMP_Text>()
            : null;
    }

    private Button FindButton(
        Transform parent,
        string name)
    {
        if (parent == null)
            return null;

        Transform found = parent.Find(name);

        return found != null
            ? found.GetComponent<Button>()
            : null;
    }

    // =========================================
    // CONNECT BUTTONS
    // =========================================

    private void ConnectButtons()
    {
        if (content == null)
            return;

        ReportChoiceButton[] keys =
            content.GetComponentsInChildren<
                ReportChoiceButton
            >(true);

        foreach (ReportChoiceButton key in keys)
        {
            key.reporting = this;

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

    // =========================================
    // HANDLE BUTTON ACTIONS
    // =========================================

    public void HandleAction(string action)
    {
        if (string.IsNullOrEmpty(action))
            return;

        if (action.StartsWith("SELECT_"))
        {
            int index;

            if (int.TryParse(
                action.Substring(7),
                out index
            ))
            {
                SelectAnswer(index);
            }

            return;
        }

        switch (action)
        {
            case "CHECK":

                CheckAnswer();
                break;

            case "NEXT":

                NextField();
                break;

            case "EDIT":

                ReviewAgain();
                break;

            case "SUBMIT":

                SubmitReport();
                break;
        }
    }

    // =========================================
    // OPEN THE ACTIVITY
    // =========================================

    [ContextMenu("Open Multiple Choice Reporting")]
    public void OpenMultipleChoiceTraining()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning(
                "Open this activity during Play Mode."
            );

            return;
        }

        FindReferences();
        FindUI();
        ConnectButtons();

        if (multipleChoicePanel == null ||
            questionScreen == null)
        {
            Debug.LogError(
                "Build Multiple Choice Report first."
            );

            return;
        }

        reportingOpened = true;
        reportSubmitted = false;

        for (int i = 0;
             i < recordedAnswers.Length;
             i++)
        {
            recordedAnswers[i] = "";
        }

        if (conditionTrainingPanel != null)
        {
            conditionTrainingPanel.SetActive(false);
        }

        multipleChoicePanel.SetActive(true);

        multipleChoicePanel.transform
            .SetAsLastSibling();

        ShowQuestion(0);

        Debug.Log(
            "Step 6: Multiple-choice reporting started."
        );
    }

    // =========================================
    // SHOW EACH QUESTION
    // =========================================

    private void ShowQuestion(int index)
    {
        if (questionScreen == null)
            return;

        currentField =
            Mathf.Clamp(index, 0, 6);

        selectedChoice = -1;
        answerCorrect = false;

        questionScreen.SetActive(true);

        if (previewScreen != null)
            previewScreen.SetActive(false);

        if (submittedScreen != null)
            submittedScreen.SetActive(false);

        if (fieldNumberText != null)
        {
            fieldNumberText.text =
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

        if (explanationText != null)
        {
            explanationText.text =
                "<b>WHY THIS MATTERS</b>\n" +
                explanations[currentField];
        }

        for (int i = 0; i < 3; i++)
        {
            Button button = choiceButtons[i];

            if (button == null)
                continue;

            button.gameObject.SetActive(true);

            Image image =
                button.GetComponent<Image>();

            if (image != null)
                image.color = paleBlue;

            if (choiceLabels[i] != null)
            {
                choiceLabels[i].text =
                    answers[currentField, i];

                choiceLabels[i].color = navy;
            }
        }

        SetFeedback(
            "Choose one answer, then press CHECK ANSWER.",
            dark
        );

        if (checkButton != null)
            checkButton.gameObject.SetActive(true);

        if (nextButton != null)
            nextButton.gameObject.SetActive(false);
    }

    // =========================================
    // SELECT ANSWER
    // =========================================

    public void SelectAnswer(int index)
    {
        if (answerCorrect ||
            index < 0 ||
            index > 2)
        {
            return;
        }

        selectedChoice = index;

        for (int i = 0; i < 3; i++)
        {
            if (choiceButtons[i] == null)
                continue;

            Image image =
                choiceButtons[i]
                .GetComponent<Image>();

            if (image != null)
            {
                image.color =
                    i == selectedChoice
                    ? selectedBlue
                    : paleBlue;
            }

            if (choiceLabels[i] != null)
            {
                choiceLabels[i].color =
                    i == selectedChoice
                    ? Color.white
                    : navy;
            }
        }

        SetFeedback(
            "Answer selected. Press CHECK ANSWER.",
            dark
        );
    }

    // =========================================
    // CHECK ANSWER
    // =========================================

    public void CheckAnswer()
    {
        if (answerCorrect)
            return;

        if (selectedChoice < 0)
        {
            SetFeedback(
                "Please select an answer first.",
                red
            );

            return;
        }

        if (selectedChoice !=
            correctAnswers[currentField])
        {
            SetFeedback(
                retryMessages[currentField],
                red
            );

            return;
        }

        answerCorrect = true;

        recordedAnswers[currentField] =
            answers[currentField, selectedChoice];

        SetFeedback(
            successMessages[currentField],
            green
        );

        if (checkButton != null)
        {
            checkButton.gameObject.SetActive(false);
        }

        if (nextButton != null)
        {
            nextButton.gameObject.SetActive(true);

            TMP_Text label =
                nextButton.GetComponentInChildren<
                    TMP_Text
                >(true);

            if (label != null)
            {
                label.text =
                    currentField == 6
                    ? "REVIEW REPORT"
                    : "NEXT FIELD";
            }
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
    // NEXT QUESTION
    // =========================================

    public void NextField()
    {
        if (!answerCorrect)
            return;

        if (currentField < 6)
        {
            ShowQuestion(currentField + 1);
        }
        else
        {
            ShowReportPreview();
        }
    }

    // =========================================
    // REPORT PREVIEW
    // =========================================

    public void ShowReportPreview()
    {
        if (questionScreen != null)
            questionScreen.SetActive(false);

        if (submittedScreen != null)
            submittedScreen.SetActive(false);

        if (previewScreen != null)
            previewScreen.SetActive(true);

        if (previewText == null)
            return;

        string report =
            "<b>PIPELINE INSPECTION REPORT</b>\n\n";

        for (int i = 0;
             i < recordedAnswers.Length;
             i++)
        {
            report +=
                "<b>" +
                fieldNames[i] +
                ":</b> " +
                recordedAnswers[i] +
                "\n\n";
        }

        previewText.text = report;
    }

    public void ReviewAgain()
    {
        if (reportSubmitted)
            return;

        // Preserve the collected answers.
        // The trainee can redo the questions.

        ShowQuestion(0);
    }

    // =========================================
    // SUBMIT TO ENGINEER
    // =========================================

    public void SubmitReport()
    {
        if (reportSubmitted)
            return;

        foreach (string answer in recordedAnswers)
        {
            if (string.IsNullOrEmpty(answer))
            {
                Debug.LogWarning(
                    "Complete all seven report fields."
                );

                return;
            }
        }

        reportSubmitted = true;

        if (questionScreen != null)
            questionScreen.SetActive(false);

        if (previewScreen != null)
            previewScreen.SetActive(false);

        if (submittedScreen != null)
            submittedScreen.SetActive(true);

        if (submissionText != null)
        {
            submissionText.text =
                "Inspection report for PIPE_0493 " +
                "submitted to the simulated engineer.\n\n" +
                "<b>STATUS: AWAITING ENGINEER REVIEW</b>\n\n" +
                "Preserve the inspection evidence. " +
                "Do not attempt repairs or change " +
                "operating conditions without authorisation.";
        }

        // NEW: Complete Task 4 - Record findings.

        CompleteRecordFindingsTask();

        Debug.Log(
            "PIPE_0493 inspection report submitted."
        );
    }

    // =========================================
    // NEW: COMPLETE CHECKLIST TASK 4
    // =========================================

    private void CompleteRecordFindingsTask()
    {
        // Require successful report submission.

        if (!reportSubmitted)
            return;

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

        // Complete Record Findings.

        taskChecklist.CompleteTask3();

        Debug.Log(
            "Training Checklist: " +
            "Record findings completed."
        );
    }

    // =========================================
    // REPORT STATUS
    // =========================================

    public bool IsReportSubmitted()
    {
        return reportSubmitted;
    }

    public string GetReportText()
    {
        string report =
            "PIPELINE INSPECTION REPORT\n";

        for (int i = 0;
             i < recordedAnswers.Length;
             i++)
        {
            report +=
                fieldNames[i] +
                ": " +
                recordedAnswers[i] +
                "\n";
        }

        return report;
    }

    // =========================================
    // EDIT MODE PREVIEW
    // =========================================

    [ContextMenu("Preview First Report Question")]
    public void PreviewFirstQuestion()
    {
        if (Application.isPlaying)
            return;

        FindReferences();
        FindUI();

        if (multipleChoicePanel == null ||
            questionScreen == null)
        {
            return;
        }

        multipleChoicePanel.SetActive(true);

        ShowQuestion(0);

#if UNITY_EDITOR

        EditorSceneManager.MarkSceneDirty(
            gameObject.scene
        );

#endif
    }

    [ContextMenu("Hide Multiple Choice Preview")]
    public void HideMultipleChoicePreview()
    {
        if (Application.isPlaying)
            return;

        if (multipleChoicePanel != null)
        {
            multipleChoicePanel.SetActive(false);
        }

#if UNITY_EDITOR

        EditorSceneManager.MarkSceneDirty(
            gameObject.scene
        );

#endif
    }

    // =========================================
    // AUTOMATIC EZPZ SETUP
    // =========================================

#if UNITY_EDITOR

    [ContextMenu("Connect Multiple Choice EZPZ")]
    public void ConfigureEZPZInteractions()
    {
        if (Application.isPlaying)
            return;

        FindReferences();
        FindUI();

        if (content == null)
        {
            Debug.LogWarning(
                "Build the multiple-choice report first."
            );

            return;
        }

        if (calculatorKeyTemplate == null)
        {
            Debug.LogWarning(
                "Assign a working CalculatorKey " +
                "with InteractableGeneral attached."
            );

            return;
        }

        Component donorInteraction = null;

        foreach (Component component in
            calculatorKeyTemplate
                .GetComponents<Component>())
        {
            if (component != null &&
                component.GetType().Name ==
                    "InteractableGeneral")
            {
                donorInteraction = component;
                break;
            }
        }

        if (donorInteraction == null)
        {
            Debug.LogWarning(
                "The calculator template has no " +
                "InteractableGeneral component."
            );

            return;
        }

        int connected = 0;
        int unresolved = 0;

        ReportChoiceButton[] buttons =
            content.GetComponentsInChildren<
                ReportChoiceButton
            >(true);

        foreach (ReportChoiceButton key in buttons)
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
                    donorInteraction
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
            "EZPZ multiple-choice callbacks connected: " +
            connected
        );

        if (unresolved > 0)
        {
            Debug.LogWarning(
                unresolved +
                " buttons need their EZPZ " +
                "Primary Interact callbacks checked."
            );
        }

        EditorSceneManager.MarkSceneDirty(
            gameObject.scene
        );
    }

#endif

}
