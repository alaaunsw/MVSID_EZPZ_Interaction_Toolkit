
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;
using UnityEditor.SceneManagement;
#endif

public class GuidedTrainingCompletion : MonoBehaviour
{
    [Header("Previous Training")]
    public GuidedTrainingSummary trainingSummary;
    public GuidedEngineerCommunication engineerTraining;
    public GuidedInspectionMultipleChoice reportingTraining;

    [Header("Generated Completion Panel")]
    public GameObject completionPanel;

    [Header("Scene Navigation")]
    public string mainMenuSceneName = "";
    public string testingModeSceneName = "";

    [Header("Progress")]
    public string completionSaveKey =
        "PIPE_0493_GuidedTrainingComplete";

    [Header("EZPZ Interaction")]
    public MaterialLossCalculatorKey calculatorKeyTemplate;
    public bool copyEZPZInteractions = true;

    private Button finishButton;
    private TMP_Text statusText;
    private bool completed;

    private readonly Color navy =
        new Color32(26, 73, 135, 255);

    private readonly Color darkBlue =
        new Color32(12, 51, 84, 255);

    private readonly Color blue =
        new Color32(10, 99, 170, 255);

    private readonly Color paleBlue =
        new Color32(226, 237, 249, 255);

    private readonly Color green =
        new Color32(24, 170, 91, 255);

    private void Start()
    {
        FindReferences();
        ConnectButtons();

        if (completionPanel != null)
            completionPanel.SetActive(false);
    }

    private void FindReferences()
    {
        if (trainingSummary == null)
            trainingSummary =
                GetComponent<GuidedTrainingSummary>();

        if (engineerTraining == null)
            engineerTraining =
                GetComponent<GuidedEngineerCommunication>();

        if (reportingTraining == null)
            reportingTraining =
                GetComponent<GuidedInspectionMultipleChoice>();

        if (completionPanel == null)
        {
            Transform found = transform.Find(
                "GuidedTrainingCompletionPanel"
            );

            if (found != null)
                completionPanel = found.gameObject;
        }

        if (trainingSummary != null &&
            trainingSummary.summaryPanel != null)
        {
            Transform feedback =
                trainingSummary.summaryPanel.transform.Find(
                    "FeedbackScreen"
                );

            if (feedback != null)
            {
                Transform finish =
                    feedback.Find("FinishTrainingButton");

                if (finish != null)
                    finishButton =
                        finish.GetComponent<Button>();
            }
        }

        if (completionPanel != null)
        {
            Transform label =
                completionPanel.transform.Find(
                    "StatusText"
                );

            if (label != null)
                statusText =
                    label.GetComponent<TMP_Text>();
        }
    }

    // ==========================================
    // BUILD IN EDIT MODE
    // ==========================================

    [ContextMenu("Build Training Completion Screen")]
    public void BuildCompletionScreen()
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
                "Attach GuidedTrainingCompletion to " +
                "MaterialLossTrainingCanvas."
            );
            return;
        }

        if (trainingSummary == null ||
            trainingSummary.summaryPanel == null)
        {
            Debug.LogError(
                "Build GuidedTrainingSummary first."
            );
            return;
        }

        Transform feedback =
            trainingSummary.summaryPanel.transform.Find(
                "FeedbackScreen"
            );

        if (feedback == null)
        {
            Debug.LogError(
                "FeedbackScreen was not found."
            );
            return;
        }

#if UNITY_EDITOR
        Undo.RegisterFullObjectHierarchyUndo(
            gameObject,
            "Build Training Completion Screen"
        );
#endif

        // Add a dedicated completion button
        // to the existing feedback screen.
        finishButton = MakeButton(
            feedback,
            "FinishTrainingButton",
            "COMPLETE TRAINING",
            "FINISH",
            0, -257, 600, 49,
            blue, Color.white
        );

        Transform existing = transform.Find(
            "GuidedTrainingCompletionPanel"
        );

        if (existing == null)
        {
            GameObject obj = new GameObject(
                "GuidedTrainingCompletionPanel",
                typeof(RectTransform),
                typeof(Image)
            );

            obj.transform.SetParent(transform, false);
            completionPanel = obj;
        }
        else
        {
            completionPanel = existing.gameObject;
        }

        Transform root = completionPanel.transform;

        SetRect(root, 0, 0, 820, 680);

        Image background =
            completionPanel.GetComponent<Image>();

        background.color = Color.white;
        background.raycastTarget = true;

        MakeText(
            root,
            "PageTitle",
            "TRAINING COMPLETE",
            29, navy, FontStyles.Bold,
            0, 273, 730, 48,
            TextAlignmentOptions.Left
        );

        MakePanel(
            root,
            "GoldDivider",
            new Color32(211, 167, 71, 255),
            0, 234, 730, 3
        );

        Transform congratulations = MakePanel(
            root,
            "CongratulationsBackground",
            darkBlue,
            0, 52, 710, 240
        );

        MakeText(
            congratulations,
            "SuccessIcon",
            "✓",
            62, green, FontStyles.Bold,
            0, 78, 110, 75,
            TextAlignmentOptions.Center
        );

        MakeText(
            congratulations,
            "CongratulationsTitle",
            "CONGRATULATIONS!",
            31, Color.white, FontStyles.Bold,
            0, 12, 670, 48,
            TextAlignmentOptions.Center
        );

        MakeText(
            congratulations,
            "CongratulationsMessage",
            "You have successfully completed the " +
            "Pipeline Inspection Training.",
            21, Color.white, FontStyles.Normal,
            0, -49, 650, 58,
            TextAlignmentOptions.Center
        );

        statusText = MakeText(
            root,
            "StatusText",
            "Guided training completion recorded.",
            19, navy, FontStyles.Normal,
            0, -118, 710, 53,
            TextAlignmentOptions.Center
        );

        MakeButton(
            root,
            "MainMenuButton",
            "MAIN MENU",
            "MENU",
            -247, -239, 220, 58,
            paleBlue, navy
        );

        MakeButton(
            root,
            "StartTestingButton",
            "START TESTING",
            "TEST",
            0, -239, 245, 58,
            blue, Color.white
        );

        MakeButton(
            root,
            "ExitVRButton",
            "EXIT VR",
            "EXIT",
            247, -239, 220, 58,
            navy, Color.white
        );

        completionPanel.transform.SetAsLastSibling();

#if UNITY_EDITOR
        if (copyEZPZInteractions)
            ConfigureEZPZInteractions();
#endif

        completionPanel.SetActive(false);

#if UNITY_EDITOR
        EditorUtility.SetDirty(this);
        EditorSceneManager.MarkSceneDirty(
            gameObject.scene
        );
#endif

        Debug.Log(
            "Training Completion Screen built."
        );
    }

    // ==========================================
    // UI CREATION
    // ==========================================

    private Transform MakePanel(
        Transform parent,
        string name,
        Color color,
        float x, float y,
        float width, float height)
    {
        Transform child = parent.Find(name);

        if (child == null)
        {
            GameObject obj = new GameObject(
                name,
                typeof(RectTransform),
                typeof(Image)
            );

            obj.transform.SetParent(parent, false);
            child = obj.transform;
        }

        SetRect(child, x, y, width, height);

        Image image = child.GetComponent<Image>();
        image.color = color;
        image.raycastTarget = false;

        return child;
    }

    private TMP_Text MakeText(
        Transform parent,
        string name,
        string content,
        int size,
        Color color,
        FontStyles style,
        float x, float y,
        float width, float height,
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

            obj.transform.SetParent(parent, false);
            child = obj.transform;
        }

        SetRect(child, x, y, width, height);

        TMP_Text text = child.GetComponent<TMP_Text>();

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
        string action,
        float x, float y,
        float width, float height,
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

            obj.transform.SetParent(parent, false);
            child = obj.transform;
        }

        SetRect(child, x, y, width, height);

        Image image = child.GetComponent<Image>();
        image.color = background;
        image.raycastTarget = true;

        Button button = child.GetComponent<Button>();
        button.targetGraphic = image;
        button.transition = Selectable.Transition.None;

        BoxCollider collider =
            child.GetComponent<BoxCollider>();

        collider.center = Vector3.zero;
        collider.size = new Vector3(
            width, height, 4f
        );

        TMP_Text label = MakeText(
            child,
            "ButtonText",
            caption,
            19, foreground, FontStyles.Bold,
            0, 0, width - 12, height - 8,
            TextAlignmentOptions.Center
        );

        TrainingCompletionButton key =
            child.GetComponent<TrainingCompletionButton>();

        if (key == null)
        {
            key = child.gameObject.AddComponent<
                TrainingCompletionButton
            >();
        }

        key.controller = this;
        key.action = action;

        return button;
    }

    private void SetRect(
        Transform target,
        float x, float y,
        float width, float height)
    {
        RectTransform rect = target as RectTransform;

        rect.anchorMin = new Vector2(.5f, .5f);
        rect.anchorMax = new Vector2(.5f, .5f);
        rect.pivot = new Vector2(.5f, .5f);

        rect.anchoredPosition3D =
            new Vector3(x, y, 0);

        rect.sizeDelta =
            new Vector2(width, height);

        rect.localScale = Vector3.one;
        rect.localRotation = Quaternion.identity;
    }

    // ==========================================
    // CONNECT UNITY UI
    // ==========================================

    private void ConnectButtons()
    {
        FindReferences();

        if (completionPanel != null)
        {
            ConnectUnder(completionPanel.transform);
        }

        if (finishButton != null)
        {
            ConnectUnder(finishButton.transform);
        }
    }

    private void ConnectUnder(Transform root)
    {
        TrainingCompletionButton[] controls =
            root.GetComponentsInChildren<
                TrainingCompletionButton
            >(true);

        foreach (TrainingCompletionButton control in controls)
        {
            control.controller = this;

            Button button =
                control.GetComponent<Button>();

            if (button == null)
                continue;

            button.onClick.RemoveListener(
                control.Activate
            );

            button.onClick.AddListener(
                control.Activate
            );
        }
    }

    public void HandleAction(string action)
    {
        switch (action)
        {
            case "FINISH":
                OpenCompletionScreen();
                break;

            case "MENU":
                LoadScene(mainMenuSceneName);
                break;

            case "TEST":
                LoadScene(testingModeSceneName);
                break;

            case "EXIT":
                ExitExperience();
                break;
        }
    }

    // ==========================================
    // COMPLETION VALIDATION
    // ==========================================

    private bool IsFeedbackScreenVisible()
    {
        if (trainingSummary == null ||
            trainingSummary.summaryPanel == null)
            return false;

        Transform feedback =
            trainingSummary.summaryPanel.transform.Find(
                "FeedbackScreen"
            );

        return feedback != null &&
            feedback.gameObject.activeInHierarchy;
    }

    private bool AllRequiredActivitiesComplete()
    {
        bool reportDone =
            reportingTraining != null &&
            reportingTraining.IsReportSubmitted();

        bool communicationDone =
            engineerTraining != null &&
            engineerTraining.HasCompletedCommunication() &&
            engineerTraining.HasRecordedLocation() &&
            engineerTraining.HasPreservedEvidence() &&
            engineerTraining.HasUpdatedStatus();

        // The earlier stages are followed sequentially
        // in the existing guided training flow.
        return reportDone && communicationDone;
    }

    public void OpenCompletionScreen()
    {
        FindReferences();

        if (completionPanel == null)
        {
            Debug.LogError(
                "Build Training Completion Screen first."
            );
            return;
        }

        if (!IsFeedbackScreenVisible())
        {
            Debug.LogWarning(
                "Open the Performance Feedback screen first."
            );
            return;
        }

        if (!AllRequiredActivitiesComplete())
        {
            Debug.LogWarning(
                "The required reporting and " +
                "engineer communication tasks " +
                "are not yet complete."
            );
            return;
        }

        completed = true;

        // Save a local completion flag.
        PlayerPrefs.SetInt(completionSaveKey, 1);
        PlayerPrefs.Save();

        if (trainingSummary != null &&
            trainingSummary.summaryPanel != null)
        {
            trainingSummary.summaryPanel.SetActive(false);
        }

        completionPanel.SetActive(true);
        completionPanel.transform.SetAsLastSibling();

        if (statusText != null)
        {
            statusText.text =
                "Guided training completion recorded " +
                "for PIPE_0493.";
        }

        Debug.Log(
            "Guided Pipeline Inspection Training completed."
        );
    }

    // ==========================================
    // SCENE NAVIGATION
    // ==========================================

    private void LoadScene(string sceneName)
    {
        if (string.IsNullOrWhiteSpace(sceneName))
        {
            Debug.LogWarning(
                "Assign the destination scene name " +
                "in the GuidedTrainingCompletion Inspector."
            );
            return;
        }

        if (!Application.CanStreamedLevelBeLoaded(sceneName))
        {
            Debug.LogError(
                "Scene is not available in Build Profiles: " +
                sceneName
            );
            return;
        }

        SceneManager.LoadScene(sceneName);
    }

    public void ExitExperience()
    {
#if UNITY_EDITOR
        Debug.Log(
            "Exit VR requested. " +
            "Application.Quit runs in a built application."
        );
#else
        Application.Quit();
#endif
    }

    public bool IsTrainingCompleted()
    {
        return completed;
    }

    // ==========================================
    // EDIT MODE PREVIEW
    // ==========================================

    [ContextMenu("Preview Completion Screen")]
    public void PreviewCompletionScreen()
    {
        if (Application.isPlaying)
            return;

        FindReferences();

        if (completionPanel == null)
            return;

        completionPanel.SetActive(true);
        completionPanel.transform.SetAsLastSibling();

#if UNITY_EDITOR
        EditorSceneManager.MarkSceneDirty(gameObject.scene);
#endif
    }

    [ContextMenu("Hide Completion Preview")]
    public void HideCompletionPreview()
    {
        if (Application.isPlaying)
            return;

        if (completionPanel != null)
            completionPanel.SetActive(false);

#if UNITY_EDITOR
        EditorSceneManager.MarkSceneDirty(gameObject.scene);
#endif
    }

    // ==========================================
    // EZPZ COMPONENT COPYING
    // ==========================================

#if UNITY_EDITOR

    [ContextMenu("Connect Completion EZPZ Interactions")]
    public void ConfigureEZPZInteractions()
    {
        if (Application.isPlaying)
            return;

        if (calculatorKeyTemplate == null)
        {
            Debug.LogWarning(
                "Assign Calculator Key Template first."
            );
            return;
        }

        Component donor = null;

        foreach (Component component in
            calculatorKeyTemplate.GetComponents<Component>())
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
                "Calculator key has no InteractableGeneral."
            );
            return;
        }

        FindReferences();

        if (completionPanel == null)
            return;

        int connected = 0;
        int unresolved = 0;

        TrainingCompletionButton[] keys =
            GetComponentsInChildren<
                TrainingCompletionButton
            >(true);

        foreach (TrainingCompletionButton key in keys)
        {
            // Only configure this activity's buttons.
            if (!key.transform.IsChildOf(
                    completionPanel.transform) &&
                (finishButton == null ||
                 key.gameObject != finishButton.gameObject))
            {
                continue;
            }

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
                ComponentUtility.CopyComponent(donor);

                if (!ComponentUtility.PasteComponentAsNew(
                    key.gameObject))
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
                        "m_PersistentCalls.m_Calls"))
                    continue;

                for (int i = 0; i < iterator.arraySize; i++)
                {
                    SerializedProperty call =
                        iterator.GetArrayElementAtIndex(i);

                    SerializedProperty target =
                        call.FindPropertyRelative("m_Target");

                    SerializedProperty method =
                        call.FindPropertyRelative("m_MethodName");

                    if (target == null || method == null)
                        continue;

                    if (target.objectReferenceValue ==
                        calculatorKeyTemplate)
                    {
                        target.objectReferenceValue = key;
                        method.stringValue = "Activate";
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
            "Completion EZPZ callbacks connected: " +
            connected
        );

        if (unresolved > 0)
        {
            Debug.LogWarning(
                unresolved +
                " EZPZ callbacks require verification."
            );
        }

        EditorSceneManager.MarkSceneDirty(
            gameObject.scene
        );
    }

#endif
}
