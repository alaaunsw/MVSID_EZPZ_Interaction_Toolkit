
using UnityEngine;
using UnityEngine.UI;
using TMPro;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;
using UnityEditor.SceneManagement;
#endif

public class InspectionFlowButtonsBuilder : MonoBehaviour
{
    [Header("Existing Inspection Flow")]
    public PipelineInspectionFlowGate flowGate;

    [Header("Existing Calculator Key")]
    public MaterialLossCalculatorKey calculatorKeyTemplate;

    [Header("Generated Panels")]
    public GameObject inspectPipePanel;
    public GameObject finishInspectionPanel;

    [Header("Build Options")]
    public bool copyEZPZInteractions = true;

    private readonly Color navy =
        new Color32(26, 73, 135, 255);

    private readonly Color dark =
        new Color32(43, 55, 72, 255);

    private readonly Color paleBlue =
        new Color32(235, 244, 253, 255);

    private readonly Color gold =
        new Color32(211, 167, 71, 255);

    // =========================================
    // UNITY
    // =========================================

    private void Start()
    {
        FindReferences();
        ConnectButtons();

        if (inspectPipePanel != null)
            inspectPipePanel.SetActive(false);

        if (finishInspectionPanel != null)
            finishInspectionPanel.SetActive(false);
    }

    private void FindReferences()
    {
        if (flowGate == null)
        {
            flowGate =
                FindFirstObjectByType<
                    PipelineInspectionFlowGate
                >(
                    FindObjectsInactive.Include
                );
        }

        if (inspectPipePanel == null)
        {
            Transform found =
                transform.Find("InspectPipePanel");

            if (found != null)
                inspectPipePanel = found.gameObject;
        }

        if (finishInspectionPanel == null)
        {
            Transform found =
                transform.Find("FinishInspectionPanel");

            if (found != null)
                finishInspectionPanel = found.gameObject;
        }
    }

    // =========================================
    // BUILD IN EDIT MODE
    // =========================================

    [ContextMenu("Build Inspection Flow Buttons")]
    public void BuildInspectionButtons()
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
                "Attach InspectionFlowButtonsBuilder " +
                "to MaterialLossTrainingCanvas, " +
                "which must be a World Space Canvas."
            );
            return;
        }

        if (flowGate == null)
        {
            Debug.LogError(
                "Assign SafeInspectionZone's " +
                "PipelineInspectionFlowGate component."
            );
            return;
        }

        // -------------------------------------
        // INSPECT PIPE PANEL
        // -------------------------------------

        inspectPipePanel = CreatePanel(
            "InspectPipePanel",
            "VISUAL PIPE INSPECTION",
            "You have reached the inspection zone.",
            "Select INSPECT PIPE to examine " +
            "the affected pipeline.",
            "INSPECT PIPE",
            InspectionFlowButton.ButtonAction.InspectPipe
        );

        // -------------------------------------
        // FINISH INSPECTION PANEL
        // -------------------------------------

        finishInspectionPanel = CreatePanel(
            "FinishInspectionPanel",
            "PIPE INSPECTION",
            "Examine PIPE_0493.",
            "Inspect the pipe and its corroded " +
            "support area. When finished, select " +
            "FINISH INSPECTION to continue.",
            "FINISH INSPECTION",
            InspectionFlowButton.ButtonAction.FinishInspection
        );

        // -------------------------------------
        // LINK TO EXISTING FLOW GATE
        // -------------------------------------

#if UNITY_EDITOR
        Undo.RecordObject(
            flowGate,
            "Assign Inspection Flow Panels"
        );
#endif

        flowGate.inspectPipeButton =
            inspectPipePanel;

        flowGate.finishInspectionButton =
            finishInspectionPanel;

        // Place new panels in front of other
        // children of the training Canvas.
        inspectPipePanel.transform.SetAsLastSibling();
        finishInspectionPanel.transform.SetAsLastSibling();

#if UNITY_EDITOR
        if (copyEZPZInteractions)
            ConfigureEZPZInteractions();
#endif

        // Save panels hidden.
        // FlowGate controls visibility in Play Mode.
        inspectPipePanel.SetActive(false);
        finishInspectionPanel.SetActive(false);

#if UNITY_EDITOR
        EditorUtility.SetDirty(this);
        EditorUtility.SetDirty(flowGate);

        EditorSceneManager.MarkSceneDirty(
            gameObject.scene
        );
#endif

        Debug.Log(
            "Inspection flow buttons created and " +
            "assigned to PipelineInspectionFlowGate."
        );
    }

    // =========================================
    // CREATE PANEL
    // =========================================

    private GameObject CreatePanel(
        string panelName,
        string title,
        string subtitle,
        string instruction,
        string buttonCaption,
        InspectionFlowButton.ButtonAction action)
    {
        Transform existing =
            transform.Find(panelName);

        GameObject panel;

        if (existing == null)
        {
            panel = new GameObject(
                panelName,
                typeof(RectTransform),
                typeof(Image)
            );

            panel.transform.SetParent(
                transform, false
            );
        }
        else
        {
            panel = existing.gameObject;
        }

        SetRect(
            panel.transform,
            0, 0, 820, 680
        );

        Image background =
            panel.GetComponent<Image>();

        if (background == null)
            background = panel.AddComponent<Image>();

        background.color = Color.white;
        background.raycastTarget = true;

        // Header
        MakeText(
            panel.transform,
            "PanelTitle",
            title,
            29, navy, FontStyles.Bold,
            0, 267, 720, 52,
            TextAlignmentOptions.Center
        );

        MakeRectangle(
            panel.transform,
            "GoldDivider",
            gold,
            0, 226, 715, 3
        );

        // Subtitle
        MakeText(
            panel.transform,
            "Subtitle",
            subtitle,
            27, dark, FontStyles.Bold,
            0, 146, 715, 70,
            TextAlignmentOptions.Center
        );

        // Instruction background
        Transform instructionBox =
            MakeRectangle(
                panel.transform,
                "InstructionBackground",
                paleBlue,
                0, 24, 720, 129
            );

        MakeText(
            instructionBox,
            "InstructionText",
            instruction,
            23, dark, FontStyles.Normal,
            0, 0, 665, 110,
            TextAlignmentOptions.Center
        );

        // Action button
        Button button = MakeButton(
            panel.transform,
            "ActionButton",
            buttonCaption,
            0, -122, 690, 73
        );

        InspectionFlowButton key =
            button.GetComponent<InspectionFlowButton>();

        if (key == null)
        {
            key = button.gameObject.AddComponent<
                InspectionFlowButton
            >();
        }

        key.flowGate = flowGate;
        key.action = action;

        // Bottom instruction
        MakeText(
            panel.transform,
            "HelpText",
            "Use your mouse or VR controller " +
            "to select the button.",
            18, dark, FontStyles.Normal,
            0, -229, 700, 40,
            TextAlignmentOptions.Center
        );

        return panel;
    }

    // =========================================
    // UI BUTTON
    // =========================================

    private Button MakeButton(
        Transform parent,
        string name,
        string caption,
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
            child,
            x, y,
            width, height
        );

        Image image =
            child.GetComponent<Image>();

        if (image == null)
            image = child.gameObject.AddComponent<Image>();

        image.color = navy;
        image.raycastTarget = true;

        Button button =
            child.GetComponent<Button>();

        if (button == null)
            button = child.gameObject.AddComponent<Button>();

        button.targetGraphic = image;
        button.interactable = true;

        // Keep button colours consistent
        // when interacting.
        button.transition =
            Selectable.Transition.None;

        BoxCollider collider =
            child.GetComponent<BoxCollider>();

        if (collider == null)
            collider = child.gameObject.AddComponent<BoxCollider>();

        collider.center = Vector3.zero;

        collider.size = new Vector3(
            width,
            height,
            4f
        );

        TMP_Text label = MakeText(
            child,
            "ButtonText",
            caption,
            25, Color.white, FontStyles.Bold,
            0, 0,
            width - 24,
            height - 10,
            TextAlignmentOptions.Center
        );

        label.raycastTarget = false;

        return button;
    }

    // =========================================
    // BACKGROUND
    // =========================================

    private Transform MakeRectangle(
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
        string textValue,
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
            child,
            x, y,
            width,
            height
        );

        TMP_Text text =
            child.GetComponent<TMP_Text>();

        text.text = textValue;
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
            new Vector3(x, y, 0);

        rect.sizeDelta =
            new Vector2(width, height);

        rect.localScale = Vector3.one;
        rect.localRotation = Quaternion.identity;
    }

    // =========================================
    // CONNECT DESKTOP BUTTONS
    // =========================================

    private void ConnectButtons()
    {
        ConnectPanel(inspectPipePanel);
        ConnectPanel(finishInspectionPanel);
    }

    private void ConnectPanel(GameObject panel)
    {
        if (panel == null)
            return;

        InspectionFlowButton[] keys =
            panel.GetComponentsInChildren<
                InspectionFlowButton
            >(true);

        foreach (InspectionFlowButton key in keys)
        {
            key.flowGate = flowGate;

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
    // COPY WORKING EZPZ INTERACTION
    // =========================================

#if UNITY_EDITOR

    [ContextMenu("Connect Inspection EZPZ Buttons")]
    public void ConfigureEZPZInteractions()
    {
        if (Application.isPlaying)
            return;

        FindReferences();

        if (calculatorKeyTemplate == null)
        {
            Debug.LogWarning(
                "Assign a working CalculatorKey_7 " +
                "to Calculator Key Template."
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
                "The calculator key template does " +
                "not contain InteractableGeneral."
            );
            return;
        }

        int connected = 0;
        int unresolved = 0;

        GameObject[] panels =
        {
            inspectPipePanel,
            finishInspectionPanel
        };

        foreach (GameObject panel in panels)
        {
            if (panel == null)
                continue;

            InspectionFlowButton key =
                panel.GetComponentInChildren<
                    InspectionFlowButton
                >(true);

            if (key == null)
                continue;

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

            // Copy the calculator's persistent
            // interaction callback, then redirect
            // it to this button's Activate().
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
            "Inspection EZPZ callbacks connected: " +
            connected
        );

        if (unresolved > 0)
        {
            Debug.LogWarning(
                unresolved +
                " inspection buttons need their " +
                "EZPZ Primary Interact events verified."
            );
        }

        EditorSceneManager.MarkSceneDirty(
            gameObject.scene
        );
    }

#endif
}
