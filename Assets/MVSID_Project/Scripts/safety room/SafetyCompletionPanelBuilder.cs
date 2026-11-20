
using UnityEngine;
using UnityEngine.UI;
using TMPro;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
#endif

public class SafetyCompletionPanelBuilder : MonoBehaviour
{
    [Header("Existing PPE References")]
    public GameObject ppeCompletionCanvas;
    public GameObject enterFacilityButtonTemplate;

    [Header("Scene Transition")]
    public SafetyToInspectionTransition sceneTransition;

    [Header("Trainee Account")]
    public SafetyTraineeAccountManager accountManager;

    [Header("Generated Objects")]
    public GameObject completionCanvas;
    public GameObject completionPanel;
    public Button enterFacilityButton;

    [Header("Panel Settings")]
    public Vector2 panelSize = new Vector2(850f, 220f);
    public float canvasScale = 0.002f;

    public Vector3 positionOffset = Vector3.zero;
    public Vector3 rotationOffset = Vector3.zero;

    [Header("Startup")]
    public bool hidePanelOnStart = true;

    [Header("Safety Training Status")]
    [SerializeField] private bool safetyTrainingCompleted;
    [SerializeField] private bool safetyCompletionRecorded;
    [SerializeField] private string safetyTraineeId = "";

    private bool enteringFacility;

    private readonly Color navy =
        new Color32(18, 54, 74, 255);

    private readonly Color blue =
        new Color32(32, 92, 153, 255);

    private readonly Color green =
        new Color32(57, 212, 119, 255);

    // =====================================
    // STARTUP
    // =====================================

    private void Awake()
    {
        safetyTrainingCompleted = false;
        safetyCompletionRecorded = false;
        safetyTraineeId = "";
        enteringFacility = false;

        FindAccountManager();

        if (completionCanvas != null &&
            hidePanelOnStart)
        {
            completionCanvas.SetActive(false);
        }

        if (enterFacilityButtonTemplate != null)
        {
            enterFacilityButtonTemplate.SetActive(false);
        }

        if (enterFacilityButton != null)
        {
            enterFacilityButton.onClick.RemoveListener(
                EnterFacility
            );

            enterFacilityButton.onClick.AddListener(
                EnterFacility
            );
        }
    }

    // =====================================
    // FIND ACCOUNT MANAGER
    // =====================================

    private void FindAccountManager()
    {
        if (SafetyTraineeAccountManager.Instance != null)
        {
            accountManager =
                SafetyTraineeAccountManager.Instance;
        }
        else if (accountManager == null)
        {
            accountManager =
                FindFirstObjectByType<
                    SafetyTraineeAccountManager
                >();
        }
    }

    // =====================================
    // BUILD SIMPLE COMPLETION PANEL
    // =====================================

    [ContextMenu("Build Safety Completion Panel")]
    public void BuildCompletionPanel()
    {
        if (Application.isPlaying)
        {
            Debug.LogWarning(
                "Stop Play Mode before building."
            );
            return;
        }

        if (ppeCompletionCanvas == null)
        {
            Debug.LogError(
                "Assign PPECompletionCanvas."
            );
            return;
        }

        if (sceneTransition == null &&
            enterFacilityButtonTemplate != null)
        {
            sceneTransition =
                enterFacilityButtonTemplate.GetComponent<
                    SafetyToInspectionTransition
                >();
        }

        if (sceneTransition == null)
        {
            Debug.LogError(
                "Assign SafetyToInspectionTransition."
            );
            return;
        }

        // CREATE WORLD SPACE CANVAS

        Transform existing = transform.Find(
            "SafetyTrainingCompletionCanvas"
        );

        if (existing == null)
        {
            completionCanvas = new GameObject(
                "SafetyTrainingCompletionCanvas",
                typeof(RectTransform),
                typeof(Canvas),
                typeof(CanvasScaler),
                typeof(GraphicRaycaster)
            );

            completionCanvas.transform.SetParent(
                transform, false
            );

#if UNITY_EDITOR
            Undo.RegisterCreatedObjectUndo(
                completionCanvas,
                "Create Safety Completion Canvas"
            );
#endif
        }
        else
        {
            completionCanvas = existing.gameObject;
        }

        Canvas canvas =
            completionCanvas.GetComponent<Canvas>();

        canvas.renderMode = RenderMode.WorldSpace;
        canvas.sortingOrder = 100;

        CanvasScaler scaler =
            completionCanvas.GetComponent<CanvasScaler>();

        scaler.dynamicPixelsPerUnit = 10f;

        RectTransform canvasRect =
            completionCanvas.GetComponent<RectTransform>();

        panelSize = new Vector2(850f, 220f);

        canvasRect.sizeDelta = panelSize;

        canvasRect.position =
            ppeCompletionCanvas.transform.position +
            positionOffset;

        canvasRect.rotation =
            ppeCompletionCanvas.transform.rotation *
            Quaternion.Euler(rotationOffset);

        canvasRect.localScale =
            Vector3.one * canvasScale;

        // CREATE BACKGROUND PANEL

        completionPanel = MakeRectangle(
            completionCanvas.transform,
            "SafetyCompletionPanel",
            navy,
            0, 0,
            panelSize.x,
            panelSize.y
        ).gameObject;

        Transform root = completionPanel.transform;

        RemoveOldElements(root);

        // GREEN COMPLETION TICK

        MakeText(
            root,
            "SuccessTick",
            "✓",
            42,
            green,
            FontStyles.Bold,
            0, 70,
            90, 44
        );

        // COMPLETION TITLE

        MakeText(
            root,
            "CompletionTitle",
            "SAFETY TRAINING COMPLETED",
            30,
            Color.white,
            FontStyles.Bold,
            0, 14,
            800, 48
        );

        // ENTER INSPECTION FACILITY BUTTON

        Transform buttonTransform = MakeRectangle(
            root,
            "EnterFacilityButton",
            blue,
            0, -57,
            440, 65
        );

        GameObject buttonObject =
            buttonTransform.gameObject;

        Image buttonImage =
            buttonObject.GetComponent<Image>();

        buttonImage.raycastTarget = true;

        enterFacilityButton =
            buttonObject.GetComponent<Button>();

        if (enterFacilityButton == null)
        {
            enterFacilityButton =
                buttonObject.AddComponent<Button>();
        }

        enterFacilityButton.targetGraphic =
            buttonImage;

        enterFacilityButton.interactable = true;

        enterFacilityButton.transition =
            Selectable.Transition.ColorTint;

        ColorBlock colors =
            enterFacilityButton.colors;

        colors.normalColor = Color.white;

        colors.highlightedColor =
            new Color32(190, 220, 255, 255);

        colors.pressedColor =
            new Color32(145, 190, 235, 255);

        enterFacilityButton.colors = colors;

        // BOX COLLIDER FOR EZPZ

        BoxCollider box =
            buttonObject.GetComponent<BoxCollider>();

        if (box == null)
        {
            box =
                buttonObject.AddComponent<BoxCollider>();
        }

        box.center = Vector3.zero;

        box.size =
            new Vector3(440f, 65f, 4f);

        box.enabled = true;

        MakeText(
            buttonTransform,
            "ButtonText",
            "ENTER INSPECTION FACILITY",
            21,
            Color.white,
            FontStyles.Bold,
            0, 0,
            420, 55
        );

        // CONNECT INTERACTIONS

#if UNITY_EDITOR

        bool alreadyConnected = false;

        for (int i = 0;
             i < enterFacilityButton.onClick
                 .GetPersistentEventCount();
             i++)
        {
            if (enterFacilityButton.onClick
                    .GetPersistentTarget(i) == this &&
                enterFacilityButton.onClick
                    .GetPersistentMethodName(i) ==
                nameof(EnterFacility))
            {
                alreadyConnected = true;
                break;
            }
        }

        if (!alreadyConnected)
        {
            UnityEventTools.AddPersistentListener(
                enterFacilityButton.onClick,
                EnterFacility
            );
        }

        ConfigureEZPZInteraction();

#endif

        // Hide the duplicated template.

        if (enterFacilityButtonTemplate != null)
        {
            enterFacilityButtonTemplate.SetActive(false);
        }

        completionCanvas.SetActive(false);

#if UNITY_EDITOR

        EditorUtility.SetDirty(this);

        EditorSceneManager.MarkSceneDirty(
            gameObject.scene
        );

#endif

        Debug.Log(
            "Simple Safety Completion Panel built."
        );
    }

    // =====================================
    // REMOVE PREVIOUS DETAILED ELEMENTS
    // =====================================

    private void RemoveOldElements(Transform root)
    {
        string[] oldElements =
        {
            "RoomLabel",
            "Divider",
            "CompletionDescription",
            "CompletedTask_0",
            "CompletedTask_1",
            "CompletedTask_2",
            "NextActivityDescription"
        };

        foreach (string objectName in oldElements)
        {
            Transform found =
                root.Find(objectName);

            if (found == null)
                continue;

#if UNITY_EDITOR

            if (!Application.isPlaying)
            {
                Undo.DestroyObjectImmediate(
                    found.gameObject
                );
            }
            else

#endif

            {
                Destroy(found.gameObject);
            }
        }
    }

    // =====================================
    // CREATE RECTANGLE
    // =====================================

    private Transform MakeRectangle(
        Transform parent,
        string objectName,
        Color color,
        float x,
        float y,
        float width,
        float height)
    {
        Transform found =
            parent.Find(objectName);

        if (found == null)
        {
            GameObject obj = new GameObject(
                objectName,
                typeof(RectTransform),
                typeof(Image)
            );

            obj.transform.SetParent(
                parent, false
            );

            found = obj.transform;
        }

        SetRect(
            found,
            x, y,
            width, height
        );

        Image image =
            found.GetComponent<Image>();

        image.color = color;
        image.raycastTarget = false;

        return found;
    }

    // =====================================
    // CREATE TEXT
    // =====================================

    private TMP_Text MakeText(
        Transform parent,
        string objectName,
        string textValue,
        float fontSize,
        Color color,
        FontStyles style,
        float x,
        float y,
        float width,
        float height)
    {
        Transform found =
            parent.Find(objectName);

        if (found == null)
        {
            GameObject obj = new GameObject(
                objectName,
                typeof(RectTransform),
                typeof(TextMeshProUGUI)
            );

            obj.transform.SetParent(
                parent, false
            );

            found = obj.transform;
        }

        SetRect(
            found,
            x, y,
            width, height
        );

        TMP_Text text =
            found.GetComponent<TMP_Text>();

        text.text = textValue;
        text.fontSize = fontSize;
        text.color = color;
        text.fontStyle = style;

        text.alignment =
            TextAlignmentOptions.Center;

        text.verticalAlignment =
            VerticalAlignmentOptions.Middle;

        text.raycastTarget = false;
        text.enableWordWrapping = false;

        return text;
    }

    // =====================================
    // RECT TRANSFORM
    // =====================================

    private void SetRect(
        Transform target,
        float x,
        float y,
        float width,
        float height)
    {
        RectTransform rect =
            target.GetComponent<RectTransform>();

        rect.anchorMin =
            new Vector2(0.5f, 0.5f);

        rect.anchorMax =
            new Vector2(0.5f, 0.5f);

        rect.pivot =
            new Vector2(0.5f, 0.5f);

        rect.anchoredPosition3D =
            new Vector3(x, y, 0f);

        rect.sizeDelta =
            new Vector2(width, height);

        rect.localScale = Vector3.one;
        rect.localRotation = Quaternion.identity;
    }

    // =====================================
    // SHOW AFTER PPE TRAINING COMPLETION
    // =====================================

    public void ShowCompletion()
    {
        if (!Application.isPlaying)
            return;

        if (completionCanvas == null)
        {
            Debug.LogError(
                "Build Safety Completion Panel first."
            );
            return;
        }

        FindAccountManager();

        if (accountManager == null ||
            !accountManager.IsLoggedIn)
        {
            Debug.LogWarning(
                "Safety completion requires " +
                "a logged-in trainee."
            );
            return;
        }

        // PPE activity has successfully completed.

        safetyTrainingCompleted = true;

        safetyTraineeId =
            accountManager.CurrentTraineeId;

        completionCanvas.SetActive(true);

        Debug.Log(
            "Safety training completed by: " +
            accountManager.CurrentUsername
        );

        Debug.Log(
            "Enter Inspection Facility is available."
        );
    }

    // =====================================
    // HIDE PANEL
    // =====================================

    public void HideCompletion()
    {
        safetyTrainingCompleted = false;
        safetyCompletionRecorded = false;

        safetyTraineeId = "";
        enteringFacility = false;

        if (completionCanvas != null)
        {
            completionCanvas.SetActive(false);
        }
    }

    // =====================================
    // VALIDATE TRAINEE SAFETY COMPLETION
    // =====================================

    public bool HasCompletedSafetyTraining()
    {
        FindAccountManager();

        if (!safetyTrainingCompleted)
            return false;

        if (accountManager == null ||
            !accountManager.IsLoggedIn)
        {
            return false;
        }

        if (string.IsNullOrEmpty(safetyTraineeId))
            return false;

        return accountManager.CurrentTraineeId ==
               safetyTraineeId;
    }

    // =====================================
    // RECORD TRAINEE SAFETY COMPLETION
    // =====================================

    private bool RecordSafetyCompletion()
    {
        if (!HasCompletedSafetyTraining())
        {
            Debug.LogWarning(
                "Safety training has not been completed " +
                "by the current trainee."
            );

            return false;
        }

        if (safetyCompletionRecorded &&
            TraineeSafetySession.IsCompletedFor(
                accountManager.CurrentTraineeId
            ))
        {
            return true;
        }

        // Store current runtime safety verification.
        // This is separate from inspection sessions.

        TraineeSafetySession.MarkCompleted(
            accountManager.CurrentTraineeId
        );

        safetyCompletionRecorded = true;

        Debug.Log(
            "Safety training verification recorded for: " +
            accountManager.CurrentUsername
        );

        return true;
    }

    // =====================================
    // LOAD INSPECTION TRAINING SCENE
    // =====================================

    public void EnterFacility()
    {
        if (!Application.isPlaying)
            return;

        if (enteringFacility)
            return;

        if (!HasCompletedSafetyTraining())
        {
            Debug.LogWarning(
                "Complete PPE training first."
            );

            return;
        }

        if (sceneTransition == null)
        {
            Debug.LogError(
                "SafetyToInspectionTransition is missing."
            );

            return;
        }

        if (!RecordSafetyCompletion())
            return;

        enteringFacility = true;

        Debug.Log(
            "Safety requirements verified. " +
            "Entering inspection facility."
        );

        try
        {
            sceneTransition.EnterTrainingMode();
        }
        finally
        {
            // The transition script handles duplicate
            // scene loading. Allow retry if loading fails.

            enteringFacility = false;
        }
    }

    // =====================================
    // EDIT MODE PREVIEW
    // =====================================

    [ContextMenu("Preview Safety Completion Panel")]
    public void PreviewCompletion()
    {
        if (Application.isPlaying)
            return;

        if (completionCanvas != null)
        {
            completionCanvas.SetActive(true);
        }

#if UNITY_EDITOR

        EditorSceneManager.MarkSceneDirty(
            gameObject.scene
        );

#endif
    }

    [ContextMenu("Hide Safety Completion Preview")]
    public void HidePreview()
    {
        if (Application.isPlaying)
            return;

        if (completionCanvas != null)
        {
            completionCanvas.SetActive(false);
        }

#if UNITY_EDITOR

        EditorSceneManager.MarkSceneDirty(
            gameObject.scene
        );

#endif
    }

    // =====================================
    // COPY EZPZ INTERACTION
    // =====================================

#if UNITY_EDITOR

    [ContextMenu("Connect Safety Completion EZPZ")]
    public void ConfigureEZPZInteraction()
    {
        if (Application.isPlaying)
            return;

        if (enterFacilityButtonTemplate == null ||
            enterFacilityButton == null)
        {
            Debug.LogWarning(
                "Assign the existing EZPZ button template."
            );

            return;
        }

        Component donor = null;

        foreach (Component component in
            enterFacilityButtonTemplate
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
                "Template has no InteractableGeneral."
            );

            return;
        }

        GameObject targetButton =
            enterFacilityButton.gameObject;

        targetButton.layer =
            enterFacilityButtonTemplate.layer;

        Component interaction = null;

        foreach (Component component in
            targetButton.GetComponents<Component>())
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
                targetButton))
            {
                Debug.LogWarning(
                    "Could not copy EZPZ interaction."
                );

                return;
            }

            foreach (Component component in
                targetButton.GetComponents<Component>())
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
            return;

        SerializedObject serialized =
            new SerializedObject(interaction);

        SerializedProperty iterator =
            serialized.GetIterator();

        bool connected = false;

        while (iterator.Next(true))
        {
            if (!iterator.isArray ||
                !iterator.propertyPath.EndsWith(
                    "m_PersistentCalls.m_Calls"))
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
                        sceneTransition &&
                    method.stringValue ==
                        "EnterTrainingMode")
                {
                    target.objectReferenceValue = this;

                    method.stringValue =
                        nameof(EnterFacility);

                    SerializedProperty typeName =
                        call.FindPropertyRelative(
                            "m_TargetAssemblyTypeName"
                        );

                    if (typeName != null)
                    {
                        typeName.stringValue =
                            GetType().AssemblyQualifiedName;
                    }

                    connected = true;
                }

                if (target.objectReferenceValue == this &&
                    method.stringValue ==
                        nameof(EnterFacility))
                {
                    connected = true;
                }
            }
        }

        serialized.ApplyModifiedProperties();

        EditorUtility.SetDirty(interaction);

        if (connected)
        {
            Debug.Log(
                "Safety completion EZPZ connected."
            );
        }
        else
        {
            Debug.LogWarning(
                "Check On Primary Interact manually. " +
                "Connect SafetyCompletionPanelBuilder " +
                "to EnterFacility()."
            );
        }

        EditorSceneManager.MarkSceneDirty(
            gameObject.scene
        );
    }

#endif
}
