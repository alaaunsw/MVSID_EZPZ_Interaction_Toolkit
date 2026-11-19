
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Reflection;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;
using UnityEditor.SceneManagement;
#endif

public class CollapsibleTrainingProgress : MonoBehaviour
{
    [Header("Existing Checklist")]
    public GameObject checklistPanel;

    [Header("Generated Buttons")]
    public GameObject progressButton;
    public GameObject closeButton;

    [Header("EZPZ Template")]
    public MaterialLossCalculatorKey calculatorKeyTemplate;
    public bool copyEZPZInteractions = true;

    [Header("Button Position")]
    public Vector2 progressButtonOffset =
        new Vector2(-25f, -25f);

    [Header("Desktop Mouse")]
    public bool automaticallyUnlockMouse = true;

    [Tooltip(
        "Optional: assign Starter Assets Inputs " +
        "from EZPZ Player Flat Screen WASD."
    )]
    public MonoBehaviour starterAssetsInputs;

    [Header("Startup")]
    public bool hideChecklistAtStart = true;
    public bool hideProgressButtonAtStart = true;

    private bool isExpanded;
    private bool progressAvailable;
    private bool cursorManaged;

    private CursorLockMode previousCursorLock;
    private bool previousCursorVisible;

    private bool previousCursorLockedSetting;
    private bool previousLookSetting;
    private bool savedStarterSettings;

    private FieldInfo cursorLockedField;
    private FieldInfo cursorLookField;

    private readonly Color navy =
        new Color32(26, 73, 135, 255);

    private readonly Color paleBlue =
        new Color32(226, 237, 249, 255);

    // =====================================
    // START
    // =====================================

    private void Start()
    {
        FindReferences();
        ConnectButtons();
        PrepareCanvas();

        isExpanded = false;
        progressAvailable = false;

        if (hideChecklistAtStart &&
            checklistPanel != null)
        {
            checklistPanel.SetActive(false);
        }

        if (hideProgressButtonAtStart &&
            progressButton != null)
        {
            progressButton.SetActive(false);
        }

        Debug.Log(
            "Collapsible Training Progress ready."
        );
    }

    private void LateUpdate()
    {
        // Recognise when PipelineInspectionFlowGate
        // enables the compact button.

        if (progressButton != null &&
            progressButton.activeSelf &&
            !progressAvailable)
        {
            progressAvailable = true;
        }

        if (progressAvailable &&
            automaticallyUnlockMouse)
        {
            UnlockMouse();
        }
    }

    private void OnDisable()
    {
        RestoreMouse();
    }

    private void OnDestroy()
    {
        RestoreMouse();
    }

    // =====================================
    // FIND REFERENCES
    // =====================================

    private void FindReferences()
    {
        if (checklistPanel == null)
        {
            Transform[] objects =
                GetComponentsInChildren<Transform>(
                    true
                );

            foreach (Transform item in objects)
            {
                if (item.name == "ChecklistPanel")
                {
                    checklistPanel = item.gameObject;
                    break;
                }
            }
        }

        if (checklistPanel == null)
            return;

        Transform parent =
            checklistPanel.transform.parent;

        if (parent != null &&
            progressButton == null)
        {
            Transform found =
                parent.Find("ProgressToggleButton");

            if (found != null)
                progressButton = found.gameObject;
        }

        if (closeButton == null)
        {
            Transform found =
                checklistPanel.transform.Find(
                    "CloseProgressButton"
                );

            if (found != null)
                closeButton = found.gameObject;
        }
    }

    // =====================================
    // PREPARE UI CANVAS
    // =====================================

    private void PrepareCanvas()
    {
        if (checklistPanel == null)
            return;

        Canvas canvas =
            checklistPanel.GetComponentInParent<Canvas>(
                true
            );

        if (canvas == null)
        {
            Debug.LogWarning(
                "Checklist has no Canvas."
            );
            return;
        }

        // The existing checklist uses
        // Screen Space - Overlay.
        // Don't convert it to World Space.

        GraphicRaycaster raycaster =
            canvas.GetComponent<GraphicRaycaster>();

        if (raycaster == null)
        {
            raycaster =
                canvas.gameObject.AddComponent<
                    GraphicRaycaster
                >();
        }

        raycaster.enabled = true;

        // Keep progress UI above normal
        // inspection UI canvases.
        canvas.overrideSorting = true;
        canvas.sortingOrder = 500;

        if (EventSystem.current == null)
        {
            Debug.LogWarning(
                "No EventSystem found. " +
                "Use the existing UI_EventSystem."
            );
        }
        else if (
            EventSystem.current.currentInputModule == null
        )
        {
            Debug.LogWarning(
                "EventSystem has no active UI Input Module."
            );
        }
    }

    // =====================================
    // BUILD BUTTONS IN EDIT MODE
    // =====================================

    [ContextMenu("Build Collapsible Progress Buttons")]
    public void BuildButtons()
    {
        if (Application.isPlaying)
        {
            Debug.LogWarning(
                "Stop Play Mode before building."
            );
            return;
        }

        FindReferences();

        if (checklistPanel == null)
        {
            Debug.LogError(
                "Assign ChecklistPanel in the Inspector."
            );
            return;
        }

        RectTransform checklistRect =
            checklistPanel.GetComponent<RectTransform>();

        if (checklistRect == null)
        {
            Debug.LogError(
                "ChecklistPanel must have a RectTransform."
            );
            return;
        }

        Transform buttonParent =
            checklistPanel.transform.parent;

        if (buttonParent == null)
        {
            Debug.LogError(
                "ChecklistPanel has no UI parent."
            );
            return;
        }

        Canvas canvas =
            checklistPanel.GetComponentInParent<Canvas>(
                true
            );

        if (canvas == null)
        {
            Debug.LogError(
                "No Canvas found for ChecklistPanel."
            );
            return;
        }

#if UNITY_EDITOR
        Undo.RegisterFullObjectHierarchyUndo(
            buttonParent.gameObject,
            "Build Collapsible Progress Buttons"
        );
#endif

        // =====================================
        // SMALL PROGRESS BUTTON
        // =====================================

        progressButton = MakeButton(
            buttonParent,
            "ProgressToggleButton",
            "PROGRESS",
            210f,
            55f,
            navy,
            Color.white,
            TrainingProgressButton.Action.Open
        );

        // Position relative to the screen,
        // not relative to the pipeline.
        RectTransform progressRect =
            progressButton.GetComponent<RectTransform>();

        progressRect.anchorMin =
            new Vector2(1f, 1f);

        progressRect.anchorMax =
            new Vector2(1f, 1f);

        progressRect.pivot =
            new Vector2(1f, 1f);

        progressRect.anchoredPosition =
            progressButtonOffset;

        progressRect.localRotation =
            Quaternion.identity;

        progressRect.localScale =
            Vector3.one;

        // =====================================
        // CLOSE BUTTON
        // =====================================

        closeButton = MakeButton(
            checklistPanel.transform,
            "CloseProgressButton",
            "CLOSE",
            100f,
            38f,
            paleBlue,
            navy,
            TrainingProgressButton.Action.Close
        );

        RectTransform closeRect =
            closeButton.GetComponent<RectTransform>();

        closeRect.anchorMin =
            new Vector2(1f, 1f);

        closeRect.anchorMax =
            new Vector2(1f, 1f);

        closeRect.pivot =
            new Vector2(1f, 1f);

        closeRect.anchoredPosition =
            new Vector2(-10f, -8f);

        closeRect.localRotation =
            Quaternion.identity;

        closeRect.localScale =
            Vector3.one;

        closeButton.transform.SetAsLastSibling();
        progressButton.transform.SetAsLastSibling();

        // =====================================
        // EZPZ
        // =====================================

#if UNITY_EDITOR
        if (copyEZPZInteractions)
        {
            ConfigureEZPZInteractions();
        }
#endif

        // Save hidden.
        // Flow Gate activates ProgressToggleButton
        // after visual inspection.

        checklistPanel.SetActive(false);
        progressButton.SetActive(false);

        isExpanded = false;
        progressAvailable = false;

#if UNITY_EDITOR
        EditorUtility.SetDirty(this);

        EditorSceneManager.MarkSceneDirty(
            gameObject.scene
        );
#endif

        Debug.Log(
            "Collapsible progress buttons generated. " +
            "Assign ProgressToggleButton to Flow Gate."
        );
    }

    // =====================================
    // MAKE BUTTON
    // =====================================

    private GameObject MakeButton(
        Transform parent,
        string objectName,
        string caption,
        float width,
        float height,
        Color background,
        Color foreground,
        TrainingProgressButton.Action action)
    {
        Transform existing =
            parent.Find(objectName);

        GameObject obj;

        if (existing == null)
        {
            obj = new GameObject(
                objectName,
                typeof(RectTransform),
                typeof(Image),
                typeof(Button),
                typeof(BoxCollider)
            );

            obj.transform.SetParent(
                parent,
                false
            );
        }
        else
        {
            obj = existing.gameObject;
        }

        RectTransform rect =
            obj.GetComponent<RectTransform>();

        rect.sizeDelta =
            new Vector2(width, height);

        rect.localScale = Vector3.one;
        rect.localRotation = Quaternion.identity;

        // IMAGE

        Image image =
            obj.GetComponent<Image>();

        if (image == null)
            image = obj.AddComponent<Image>();

        image.color = background;
        image.raycastTarget = true;
        image.enabled = true;

        // UNITY BUTTON

        Button button =
            obj.GetComponent<Button>();

        if (button == null)
            button = obj.AddComponent<Button>();

        button.enabled = true;
        button.interactable = true;
        button.targetGraphic = image;

        button.transition =
            Selectable.Transition.ColorTint;

        ColorBlock colors = button.colors;

        colors.normalColor = Color.white;

        colors.highlightedColor =
            new Color32(190, 216, 245, 255);

        colors.pressedColor =
            new Color32(140, 180, 226, 255);

        colors.selectedColor = Color.white;
        colors.colorMultiplier = 1f;

        button.colors = colors;

        // BOX COLLIDER FOR EZPZ

        BoxCollider collider =
            obj.GetComponent<BoxCollider>();

        if (collider == null)
            collider = obj.AddComponent<BoxCollider>();

        collider.enabled = true;
        collider.isTrigger = false;
        collider.center = Vector3.zero;

        collider.size =
            new Vector3(
                width,
                height,
                4f
            );

        // TEXT

        Transform textObject =
            obj.transform.Find("ButtonText");

        if (textObject == null)
        {
            GameObject labelObject = new GameObject(
                "ButtonText",
                typeof(RectTransform),
                typeof(TextMeshProUGUI)
            );

            labelObject.transform.SetParent(
                obj.transform,
                false
            );

            textObject = labelObject.transform;
        }

        RectTransform textRect =
            textObject.GetComponent<RectTransform>();

        textRect.anchorMin =
            new Vector2(0f, 0f);

        textRect.anchorMax =
            new Vector2(1f, 1f);

        textRect.offsetMin =
            new Vector2(5f, 3f);

        textRect.offsetMax =
            new Vector2(-5f, -3f);

        textRect.localScale = Vector3.one;
        textRect.localRotation = Quaternion.identity;

        TMP_Text label =
            textObject.GetComponent<TMP_Text>();

        label.text = caption;

        label.fontSize =
            action == TrainingProgressButton.Action.Close
            ? 16f
            : 20f;

        label.fontStyle = FontStyles.Bold;
        label.color = foreground;

        label.alignment =
            TextAlignmentOptions.Center;

        label.verticalAlignment =
            VerticalAlignmentOptions.Middle;

        label.raycastTarget = false;

        // CONTROLLER

        TrainingProgressButton key =
            obj.GetComponent<TrainingProgressButton>();

        if (key == null)
        {
            key =
                obj.AddComponent<TrainingProgressButton>();
        }

        key.controller = this;
        key.action = action;

        // Also save a Unity UI event in Edit Mode.
        // This does not depend on copying EZPZ.

#if UNITY_EDITOR
        SerializedObject serializedButton =
            new SerializedObject(button);

        // The runtime listener is still added
        // in Start() below.
#endif

        return obj;
    }

    // =====================================
    // CONNECT BUTTONS
    // =====================================

    private void ConnectButtons()
    {
        ConnectButton(progressButton);
        ConnectButton(closeButton);
    }

    private void ConnectButton(GameObject obj)
    {
        if (obj == null)
            return;

        Button button =
            obj.GetComponent<Button>();

        TrainingProgressButton key =
            obj.GetComponent<TrainingProgressButton>();

        if (button == null || key == null)
            return;

        key.controller = this;

        button.onClick.RemoveListener(
            key.Activate
        );

        button.onClick.AddListener(
            key.Activate
        );
    }

    // =====================================
    // UNLOCK DESKTOP MOUSE
    // =====================================

    private void UnlockMouse()
    {
        if (!cursorManaged)
        {
            previousCursorLock =
                Cursor.lockState;

            previousCursorVisible =
                Cursor.visible;

            cursorManaged = true;

            SaveStarterInputSettings();
        }

        // Keep cursor available for UI.
        Cursor.lockState =
            CursorLockMode.None;

        Cursor.visible = true;

        // Prevent Starter Assets from immediately
        // relocking the cursor or processing
        // mouse look while interacting with UI.

        SetStarterBool(
            cursorLockedField,
            false
        );

        SetStarterBool(
            cursorLookField,
            false
        );
    }

    private void SaveStarterInputSettings()
    {
        if (starterAssetsInputs == null)
            return;

        System.Type type =
            starterAssetsInputs.GetType();

        BindingFlags flags =
            BindingFlags.Instance |
            BindingFlags.Public |
            BindingFlags.NonPublic;

        cursorLockedField =
            type.GetField(
                "cursorLocked",
                flags
            );

        cursorLookField =
            type.GetField(
                "cursorInputForLook",
                flags
            );

        savedStarterSettings = false;

        if (cursorLockedField != null &&
            cursorLockedField.FieldType ==
            typeof(bool))
        {
            previousCursorLockedSetting =
                (bool)cursorLockedField.GetValue(
                    starterAssetsInputs
                );

            savedStarterSettings = true;
        }

        if (cursorLookField != null &&
            cursorLookField.FieldType ==
            typeof(bool))
        {
            previousLookSetting =
                (bool)cursorLookField.GetValue(
                    starterAssetsInputs
                );

            savedStarterSettings = true;
        }
    }

    private void SetStarterBool(
        FieldInfo field,
        bool value)
    {
        if (starterAssetsInputs == null ||
            field == null ||
            field.FieldType != typeof(bool))
        {
            return;
        }

        field.SetValue(
            starterAssetsInputs,
            value
        );
    }

    private void RestoreMouse()
    {
        if (!cursorManaged)
            return;

        Cursor.lockState =
            previousCursorLock;

        Cursor.visible =
            previousCursorVisible;

        if (savedStarterSettings)
        {
            SetStarterBool(
                cursorLockedField,
                previousCursorLockedSetting
            );

            SetStarterBool(
                cursorLookField,
                previousLookSetting
            );
        }

        cursorManaged = false;
    }

    // =====================================
    // OPEN PROGRESS
    // =====================================

    public void OpenProgress()
    {
        FindReferences();

        if (checklistPanel == null)
        {
            Debug.LogWarning(
                "ChecklistPanel is missing."
            );
            return;
        }

        progressAvailable = true;
        isExpanded = true;

        checklistPanel.SetActive(true);
        checklistPanel.transform.SetAsLastSibling();

        if (progressButton != null)
            progressButton.SetActive(false);

        if (automaticallyUnlockMouse)
            UnlockMouse();

        Debug.Log(
            "Training progress opened."
        );
    }

    // =====================================
    // CLOSE PROGRESS
    // =====================================

    public void CloseProgress()
    {
        isExpanded = false;

        if (checklistPanel != null)
            checklistPanel.SetActive(false);

        if (progressButton != null &&
            progressAvailable)
        {
            progressButton.SetActive(true);
        }

        Debug.Log(
            "Training progress collapsed."
        );
    }

    // =====================================
    // FLOW GATE SUPPORT
    // =====================================

    public void EnableProgress()
    {
        FindReferences();

        progressAvailable = true;
        isExpanded = false;

        if (checklistPanel != null)
            checklistPanel.SetActive(false);

        if (progressButton != null)
            progressButton.SetActive(true);

        if (automaticallyUnlockMouse)
            UnlockMouse();
    }

    public void DisableProgress()
    {
        isExpanded = false;
        progressAvailable = false;

        if (checklistPanel != null)
            checklistPanel.SetActive(false);

        if (progressButton != null)
            progressButton.SetActive(false);

        RestoreMouse();
    }

    // =====================================
    // EDITOR PREVIEW
    // =====================================

    [ContextMenu("Preview Collapsed Progress")]
    public void PreviewCollapsed()
    {
        if (Application.isPlaying)
            return;

        FindReferences();

        if (checklistPanel != null)
            checklistPanel.SetActive(false);

        if (progressButton != null)
            progressButton.SetActive(true);

#if UNITY_EDITOR
        EditorSceneManager.MarkSceneDirty(
            gameObject.scene
        );
#endif
    }

    [ContextMenu("Preview Expanded Progress")]
    public void PreviewExpanded()
    {
        if (Application.isPlaying)
            return;

        FindReferences();

        if (checklistPanel != null)
            checklistPanel.SetActive(true);

        if (progressButton != null)
            progressButton.SetActive(false);

#if UNITY_EDITOR
        EditorSceneManager.MarkSceneDirty(
            gameObject.scene
        );
#endif
    }

    [ContextMenu("Hide Progress Preview")]
    public void HidePreview()
    {
        if (Application.isPlaying)
            return;

        FindReferences();

        if (checklistPanel != null)
            checklistPanel.SetActive(false);

        if (progressButton != null)
            progressButton.SetActive(false);

#if UNITY_EDITOR
        EditorSceneManager.MarkSceneDirty(
            gameObject.scene
        );
#endif
    }

    // =====================================
    // COPY EZPZ COMPONENT
    // =====================================

#if UNITY_EDITOR

    [ContextMenu("Connect Progress EZPZ Interactions")]
    public void ConfigureEZPZInteractions()
    {
        if (Application.isPlaying)
            return;

        FindReferences();

        if (calculatorKeyTemplate == null)
        {
            Debug.LogWarning(
                "Assign a working calculator key."
            );
            return;
        }

        Component donor = null;

        foreach (
            Component component in
            calculatorKeyTemplate.GetComponents<Component>()
        )
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
                "Calculator template has no " +
                "InteractableGeneral."
            );
            return;
        }

        GameObject[] targets =
        {
            progressButton,
            closeButton
        };

        int connected = 0;

        foreach (GameObject targetObject in targets)
        {
            if (targetObject == null)
                continue;

            Component interaction = null;

            foreach (
                Component component in
                targetObject.GetComponents<Component>()
            )
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
                    targetObject))
                {
                    continue;
                }

                foreach (
                    Component component in
                    targetObject.GetComponents<Component>()
                )
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
                continue;

            TrainingProgressButton key =
                targetObject.GetComponent<
                    TrainingProgressButton
                >();

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

                for (
                    int i = 0;
                    i < iterator.arraySize;
                    i++
                )
                {
                    SerializedProperty call =
                        iterator.GetArrayElementAtIndex(i);

                    SerializedProperty eventTarget =
                        call.FindPropertyRelative(
                            "m_Target"
                        );

                    SerializedProperty method =
                        call.FindPropertyRelative(
                            "m_MethodName"
                        );

                    if (eventTarget == null ||
                        method == null)
                    {
                        continue;
                    }

                    if (eventTarget.objectReferenceValue ==
                        calculatorKeyTemplate)
                    {
                        eventTarget.objectReferenceValue =
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
        }

        Debug.Log(
            "Progress EZPZ callbacks connected: " +
            connected + " / 2"
        );

        EditorSceneManager.MarkSceneDirty(
            gameObject.scene
        );
    }

#endif
}
