
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;
using UnityEditor.SceneManagement;
#endif

public class GuidedTrainingSummary : MonoBehaviour
{
    [Header("Previous Training")]
    public GuidedEngineerCommunication engineerTraining;
    public GuidedInspectionMultipleChoice reportingTraining;

    [Header("Generated Panel")]
    public GameObject summaryPanel;

    // NEW: Training progress checklist.
    [Header("Training Progress Checklist")]
    public InspectionTaskChecklist taskChecklist;

    [Header("Automatic Opening")]
    public bool openAfterEngineerCommunication = true;

    [Header("Main Menu")]
    [Tooltip("Optional. Must be a scene included in Build Profiles.")]
    public string mainMenuSceneName = "";

    [Header("EZPZ Interaction Template")]
    public MaterialLossCalculatorKey calculatorKeyTemplate;
    public bool copyEZPZInteractions = true;

    private Transform recapScreen;
    private Transform feedbackScreen;
    private TMP_Text recapStatus;
    private TMP_Text overallFeedback;

    private bool opened;
    private int currentPage = 0;

    // NEW: Prevent duplicate checklist completion.
    private bool feedbackTaskCompleted;

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

    private readonly Color green =
        new Color32(28, 150, 85, 255);

    private readonly Color yellow =
        new Color32(255, 242, 202, 255);

    // =========================================
    // UNITY
    // =========================================

    private void Start()
    {
        FindReferences();
        FindUI();
        ConnectButtons();

        feedbackTaskCompleted = false;

        if (summaryPanel != null)
            summaryPanel.SetActive(false);
    }

    private void Update()
    {
        if (!openAfterEngineerCommunication ||
            opened ||
            engineerTraining == null)
            return;

        if (engineerTraining.HasCompletedCommunication())
            OpenSummary();
    }

    private void FindReferences()
    {
        if (engineerTraining == null)
        {
            engineerTraining =
                GetComponent<GuidedEngineerCommunication>();
        }

        if (reportingTraining == null)
        {
            reportingTraining =
                GetComponent<GuidedInspectionMultipleChoice>();
        }

        if (summaryPanel == null)
        {
            Transform found =
                transform.Find("GuidedTrainingSummaryPanel");

            if (found != null)
                summaryPanel = found.gameObject;
        }
    }

    // =========================================
    // EDITOR BUILDER
    // =========================================

    [ContextMenu("Build Recap and Feedback")]
    public void BuildSummary()
    {
        if (Application.isPlaying)
        {
            Debug.LogWarning(
                "Stop Play Mode before building the summary."
            );

            return;
        }

        FindReferences();

        Canvas canvas = GetComponent<Canvas>();

        if (canvas == null ||
            canvas.renderMode != RenderMode.WorldSpace)
        {
            Debug.LogError(
                "Attach this script to the World Space " +
                "MaterialLossTrainingCanvas."
            );

            return;
        }

        Transform found =
            transform.Find("GuidedTrainingSummaryPanel");

        if (found == null)
        {
            GameObject obj = new GameObject(
                "GuidedTrainingSummaryPanel",
                typeof(RectTransform),
                typeof(Image)
            );

            obj.transform.SetParent(
                transform, false
            );

            summaryPanel = obj;
        }
        else
        {
            summaryPanel = found.gameObject;
        }

#if UNITY_EDITOR

        Undo.RegisterFullObjectHierarchyUndo(
            summaryPanel,
            "Build Recap and Feedback"
        );

#endif

        Transform root = summaryPanel.transform;

        SetRect(root, 0, 0, 820, 760);

        Image background =
            summaryPanel.GetComponent<Image>();

        if (background == null)
        {
            background =
                summaryPanel.AddComponent<Image>();
        }

        background.color = Color.white;
        background.raycastTarget = true;

        // =========================================
        // RECAP SCREEN
        // =========================================

        recapScreen = MakeContainer(
            root,
            "RecapScreen",
            0, 0, 820, 760
        );

        MakeText(
            recapScreen,
            "RecapTitle",
            "TRAINING RECAP",
            29, navy, FontStyles.Bold,
            0, 319, 730, 45,
            TextAlignmentOptions.Left
        );

        MakeText(
            recapScreen,
            "RecapStep",
            "STEP 8",
            19, navy, FontStyles.Bold,
            298, 275, 130, 30,
            TextAlignmentOptions.Right
        );

        MakePanel(
            recapScreen,
            "RecapDivider",
            gold,
            0, 254, 730, 3
        );

        MakeText(
            recapScreen,
            "RecapHeading",
            "Review Your Training",
            31, dark, FontStyles.Bold,
            0, 204, 730, 55,
            TextAlignmentOptions.Left
        );

        MakeText(
            recapScreen,
            "RecapInstruction",
            "Review the activities you completed.",
            21, dark, FontStyles.Normal,
            0, 156, 730, 43,
            TextAlignmentOptions.Left
        );

        string[] headings =
        {
            "Visual Inspection",
            "Pipeline Data",
            "Wall Thickness Loss",
            "Condition Classification",
            "Engineer Communication"
        };

        string[] details =
        {
            "Identified corrosion at the pipe support",

            "Reviewed PIPE_0493 inspection measurements",

            "Calculated 3.41 mm thickness loss (30.69%)",

            "Classified the condition as Moderate",

            "Submitted findings and followed instructions"
        };

        for (int i = 0; i < 5; i++)
        {
            float y = 89f - i * 91f;

            Transform row = MakePanel(
                recapScreen,
                "RecapRow_" + (i + 1),
                paleBlue,
                0, y, 730, 80
            );

            MakeText(
                row,
                "CheckMark",
                "✓",
                32, green, FontStyles.Bold,
                -322, 0, 50, 58,
                TextAlignmentOptions.Center
            );

            MakeText(
                row,
                "Heading",
                headings[i],
                21, navy, FontStyles.Bold,
                22, 17, 600, 33,
                TextAlignmentOptions.Left
            );

            MakeText(
                row,
                "Description",
                details[i],
                17, dark, FontStyles.Normal,
                22, -16, 600, 30,
                TextAlignmentOptions.Left
            );
        }

        recapStatus = MakeText(
            recapScreen,
            "RecapStatus",
            "Complete all training activities to continue.",
            17, dark, FontStyles.Normal,
            0, -386, 730, 30,
            TextAlignmentOptions.Center
        );

        MakeActionButton(
            recapScreen,
            "RecapNextButton",
            "VIEW FEEDBACK",
            "NEXT",
            0, -334, 730, 54,
            navy, Color.white
        );

        // Move status above the button.

        SetRect(
            recapStatus.transform,
            0, -289, 730, 30
        );

        // =========================================
        // FEEDBACK SCREEN
        // =========================================

        feedbackScreen = MakeContainer(
            root,
            "FeedbackScreen",
            0, 0, 820, 760
        );

        MakeText(
            feedbackScreen,
            "FeedbackTitle",
            "PERFORMANCE FEEDBACK",
            29, navy, FontStyles.Bold,
            0, 319, 730, 45,
            TextAlignmentOptions.Left
        );

        MakeText(
            feedbackScreen,
            "FeedbackStep",
            "STEP 9",
            19, navy, FontStyles.Bold,
            298, 275, 130, 30,
            TextAlignmentOptions.Right
        );

        MakePanel(
            feedbackScreen,
            "FeedbackDivider",
            gold,
            0, 254, 730, 3
        );

        MakeText(
            feedbackScreen,
            "FeedbackHeading",
            "Training Feedback and Performance",
            29, dark, FontStyles.Bold,
            0, 202, 730, 56,
            TextAlignmentOptions.Left
        );

        MakeText(
            feedbackScreen,
            "FeedbackIntro",
            "Here is feedback based on the guided " +
            "inspection activities you completed.",
            20, dark, FontStyles.Normal,
            0, 147, 730, 52,
            TextAlignmentOptions.Left
        );

        Transform resultBox = MakePanel(
            feedbackScreen,
            "OverallResultBox",
            new Color32(225, 245, 232, 255),
            0, 28, 730, 144
        );

        MakeText(
            resultBox,
            "SuccessIcon",
            "✓",
            52, green, FontStyles.Bold,
            -298, 0, 80, 94,
            TextAlignmentOptions.Center
        );

        overallFeedback = MakeText(
            resultBox,
            "OverallResultText",
            "GOOD JOB!\n" +
            "You completed the guided inspection " +
            "and communication activities.",
            22, navy, FontStyles.Bold,
            45, 0, 550, 110,
            TextAlignmentOptions.Left
        );

        Transform improvementBox = MakePanel(
            feedbackScreen,
            "ImprovementBox",
            yellow,
            0, -140, 730, 155
        );

        MakeText(
            improvementBox,
            "ImprovementIcon",
            "!",
            48,
            new Color32(185, 125, 15, 255),
            FontStyles.Bold,
            -298, 0, 80, 100,
            TextAlignmentOptions.Center
        );

        MakeText(
            improvementBox,
            "ImprovementText",
            "<b>AREA FOR IMPROVEMENT</b>\n" +
            "Keep practising accurate inspection notes, " +
            "checking calculations, and following " +
            "engineering instructions. Inspect only " +
            "when authorised and follow site safety procedures.",
            19, dark, FontStyles.Normal,
            45, 0, 550, 133,
            TextAlignmentOptions.Left
        );

        MakeActionButton(
            feedbackScreen,
            "BackToRecapButton",
            "BACK TO RECAP",
            "BACK",
            -184, -318, 340, 55,
            paleBlue, navy
        );

        MakeActionButton(
            feedbackScreen,
            "MainMenuButton",
            "MAIN MENU",
            "MENU",
            184, -318, 340, 55,
            navy, Color.white
        );

        FindUI();
        ShowRecap();

        summaryPanel.transform.SetAsLastSibling();

#if UNITY_EDITOR

        if (copyEZPZInteractions)
            ConfigureEZPZInteractions();

#endif

        summaryPanel.SetActive(false);

#if UNITY_EDITOR

        EditorUtility.SetDirty(this);

        EditorSceneManager.MarkSceneDirty(
            gameObject.scene
        );

#endif

        Debug.Log(
            "Training recap and feedback panel built."
        );
    }

    // =========================================
    // UI FACTORIES
    // =========================================

    private Transform MakeContainer(
        Transform parent,
        string name,
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
                typeof(RectTransform)
            );

            obj.transform.SetParent(
                parent, false
            );

            child = obj.transform;
        }

        SetRect(
            child, x, y, width, height
        );

        return child;
    }

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

        image.color = color;
        image.raycastTarget = false;

        return child;
    }

    private TMP_Text MakeText(
        Transform parent,
        string name,
        string value,
        int size,
        Color color,
        FontStyles fontStyle,
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

        text.text = value;
        text.fontSize = size;
        text.fontStyle = fontStyle;
        text.color = color;
        text.alignment = alignment;

        text.verticalAlignment =
            VerticalAlignmentOptions.Middle;

        text.enableWordWrapping = true;
        text.raycastTarget = false;

        return text;
    }

    private Button MakeActionButton(
        Transform parent,
        string name,
        string label,
        string action,
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

        TMP_Text caption = MakeText(
            child,
            "ButtonText",
            label,
            20,
            foreground,
            FontStyles.Bold,
            0, 0,
            width - 15,
            height - 8,
            TextAlignmentOptions.Center
        );

        TrainingSummaryButton control =
            child.GetComponent<TrainingSummaryButton>();

        if (control == null)
        {
            control =
                child.gameObject.AddComponent<
                    TrainingSummaryButton
                >();
        }

        control.controller = this;
        control.action = action;

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
            new Vector2(.5f, .5f);

        rect.anchorMax =
            new Vector2(.5f, .5f);

        rect.pivot =
            new Vector2(.5f, .5f);

        rect.anchoredPosition3D =
            new Vector3(x, y, 0);

        rect.sizeDelta =
            new Vector2(width, height);

        rect.localScale = Vector3.one;

        rect.localRotation =
            Quaternion.identity;
    }

    // =========================================
    // FIND AND CONNECT
    // =========================================

    private void FindUI()
    {
        if (summaryPanel == null)
            return;

        recapScreen =
            summaryPanel.transform.Find(
                "RecapScreen"
            );

        feedbackScreen =
            summaryPanel.transform.Find(
                "FeedbackScreen"
            );

        recapStatus =
            FindText(
                recapScreen,
                "RecapStatus"
            );

        Transform result =
            feedbackScreen != null
            ? feedbackScreen.Find("OverallResultBox")
            : null;

        overallFeedback =
            FindText(
                result,
                "OverallResultText"
            );
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

    private void ConnectButtons()
    {
        if (summaryPanel == null)
            return;

        TrainingSummaryButton[] buttons =
            summaryPanel.GetComponentsInChildren<
                TrainingSummaryButton
            >(true);

        foreach (TrainingSummaryButton key in buttons)
        {
            key.controller = this;

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
    // OPEN AND NAVIGATE
    // =========================================

    [ContextMenu("Open Training Summary")]
    public void OpenSummary()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning(
                "Open Training Summary during Play Mode."
            );

            return;
        }

        FindReferences();
        FindUI();
        ConnectButtons();

        if (summaryPanel == null)
        {
            Debug.LogError(
                "Build Recap and Feedback first."
            );

            return;
        }

        opened = true;

        if (engineerTraining != null &&
            engineerTraining.communicationPanel != null)
        {
            engineerTraining.communicationPanel
                .SetActive(false);
        }

        summaryPanel.SetActive(true);

        summaryPanel.transform
            .SetAsLastSibling();

        ShowRecap();
    }

    public void HandleAction(string action)
    {
        switch (action)
        {
            case "NEXT":

                ShowFeedback();
                break;

            case "BACK":

                ShowRecap();
                break;

            case "MENU":

                GoToMainMenu();
                break;
        }
    }

    // =========================================
    // SHOW TRAINING RECAP
    // =========================================

    private void ShowRecap()
    {
        currentPage = 0;

        if (recapScreen != null)
        {
            recapScreen.gameObject.SetActive(true);
        }

        if (feedbackScreen != null)
        {
            feedbackScreen.gameObject.SetActive(false);
        }

        if (recapStatus != null)
        {
            bool done =
                engineerTraining != null &&
                engineerTraining.HasCompletedCommunication();

            recapStatus.text = done
                ? "Guided training activities completed."
                : "Recap preview — training not yet completed.";
        }
    }

    // =========================================
    // SHOW PERFORMANCE FEEDBACK
    // =========================================

    private void ShowFeedback()
    {
        currentPage = 1;

        if (recapScreen != null)
        {
            recapScreen.gameObject.SetActive(false);
        }

        if (feedbackScreen != null)
        {
            feedbackScreen.gameObject.SetActive(true);
        }

        if (overallFeedback != null)
        {
            bool completed =
                engineerTraining != null &&
                engineerTraining.HasCompletedCommunication();

            overallFeedback.text = completed
                ? "<b>GOOD JOB!</b>\n" +
                  "You completed the guided inspection, " +
                  "reporting, and engineer communication activities."

                : "<b>TRAINING PREVIEW</b>\n" +
                  "Complete the activities to receive " +
                  "your completion confirmation.";
        }

        // NEW: Complete the final checklist task.

        CompleteReceiveFeedbackTask();
    }

    // =========================================
    // NEW: COMPLETE CHECKLIST TASK 6
    // =========================================

    private void CompleteReceiveFeedbackTask()
    {
        // Prevent Edit Mode previews from
        // completing the training task.

        if (!Application.isPlaying)
            return;

        // Prevent duplicate completion.

        if (feedbackTaskCompleted)
            return;

        // Require the actual summary activity.

        if (!opened)
            return;

        // Require the feedback screen to be visible.

        if (feedbackScreen == null ||
            !feedbackScreen.gameObject.activeInHierarchy)
        {
            return;
        }

        // Require successful report submission.

        if (reportingTraining == null ||
            !reportingTraining.IsReportSubmitted())
        {
            Debug.LogWarning(
                "Complete inspection reporting first."
            );

            return;
        }

        // Require completed engineer communication.

        if (engineerTraining == null ||
            !engineerTraining.HasCompletedCommunication())
        {
            Debug.LogWarning(
                "Complete engineer communication first."
            );

            return;
        }

        // Require all three engineering follow-up tasks.

        if (!engineerTraining.HasRecordedLocation() ||
            !engineerTraining.HasPreservedEvidence() ||
            !engineerTraining.HasUpdatedStatus())
        {
            Debug.LogWarning(
                "Complete the required engineering tasks."
            );

            return;
        }

        // Automatically find the checklist
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

        // Complete Task 6: Receive feedback.

        taskChecklist.CompleteTask5();

        feedbackTaskCompleted = true;

        Debug.Log(
            "Training Checklist: Receive feedback completed."
        );
    }

    // =========================================
    // MAIN MENU
    // =========================================

    public void GoToMainMenu()
    {
        if (string.IsNullOrWhiteSpace(mainMenuSceneName))
        {
            Debug.LogWarning(
                "Main Menu scene name is not set. " +
                "Assign the scene in the Inspector."
            );

            return;
        }

        if (!Application.CanStreamedLevelBeLoaded(
            mainMenuSceneName))
        {
            Debug.LogError(
                "Main Menu scene is not included " +
                "in the Unity Build Profiles scene list: " +
                mainMenuSceneName
            );

            return;
        }

        SceneManager.LoadScene(
            mainMenuSceneName
        );
    }

    // =========================================
    // EDIT MODE PREVIEW
    // =========================================

    [ContextMenu("Preview Recap")]
    public void PreviewRecap()
    {
        if (Application.isPlaying)
            return;

        FindReferences();
        FindUI();

        if (summaryPanel == null)
            return;

        summaryPanel.SetActive(true);

        ShowRecap();

#if UNITY_EDITOR

        EditorSceneManager.MarkSceneDirty(
            gameObject.scene
        );

#endif
    }

    [ContextMenu("Preview Performance Feedback")]
    public void PreviewFeedback()
    {
        if (Application.isPlaying)
            return;

        FindReferences();
        FindUI();

        if (summaryPanel == null)
            return;

        summaryPanel.SetActive(true);

        ShowFeedback();

#if UNITY_EDITOR

        EditorSceneManager.MarkSceneDirty(
            gameObject.scene
        );

#endif
    }

    [ContextMenu("Hide Summary Preview")]
    public void HidePreview()
    {
        if (Application.isPlaying)
            return;

        if (summaryPanel != null)
        {
            summaryPanel.SetActive(false);
        }

#if UNITY_EDITOR

        EditorSceneManager.MarkSceneDirty(
            gameObject.scene
        );

#endif
    }

    // =========================================
    // AUTOMATIC EZPZ EVENT COPYING
    // =========================================

#if UNITY_EDITOR

    [ContextMenu("Connect Recap EZPZ Interactions")]
    public void ConfigureEZPZInteractions()
    {
        if (Application.isPlaying)
            return;

        FindReferences();

        if (summaryPanel == null ||
            calculatorKeyTemplate == null)
        {
            Debug.LogWarning(
                "Build the panel and assign an EZPZ " +
                "calculator key template first."
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
                "The calculator template does not have " +
                "an InteractableGeneral component."
            );

            return;
        }

        int connected = 0;
        int unresolved = 0;

        TrainingSummaryButton[] keys =
            summaryPanel.GetComponentsInChildren<
                TrainingSummaryButton
            >(true);

        foreach (TrainingSummaryButton key in keys)
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

                EditorUtility.SetDirty(
                    interaction
                );

                connected++;
            }
            else
            {
                unresolved++;
            }
        }

        Debug.Log(
            "Summary EZPZ callbacks connected: " +
            connected
        );

        if (unresolved > 0)
        {
            Debug.LogWarning(
                unresolved +
                " interaction callbacks must be checked " +
                "in the Inspector before VR testing."
            );
        }

        EditorSceneManager.MarkSceneDirty(
            gameObject.scene
        );
    }

#endif

}
