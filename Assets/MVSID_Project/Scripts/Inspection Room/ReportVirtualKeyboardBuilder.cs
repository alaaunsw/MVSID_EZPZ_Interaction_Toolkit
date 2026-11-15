
using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;
using UnityEditor.SceneManagement;
#endif

public class ReportVirtualKeyboardBuilder : MonoBehaviour
{
    [Header("Existing Reporting UI")]
    public TMP_InputField reportInput;
    public GameObject reportingPanel;

    [Header("Generated Keyboard")]
    public GameObject keyboardPanel;

    [Header("EZPZ Interaction Template")]
    [Tooltip(
        "Drag an existing working calculator key " +
        "that has InteractableGeneral attached."
    )]
    public MaterialLossCalculatorKey calculatorKeyTemplate;

    [Header("Keyboard Options")]
    public bool startWithCapitals = true;
    public int maximumCharacters = 300;

    [Header("Editor Settings")]
    public bool copyEZPZFromCalculator = true;

    private TMP_Text previewText;
    private TMP_Text statusText;
    private Button openButton;

    private bool capitals;
    private bool keyboardOpen;

    private int lastOpenFrame = -1;

    private readonly Color navy =
        new Color32(26, 73, 135, 255);

    private readonly Color dark =
        new Color32(43, 55, 72, 255);

    private readonly Color lightBlue =
        new Color32(226, 237, 249, 255);

    private readonly Color white = Color.white;

    // =====================================
    // UNITY
    // =====================================

    private void Start()
    {
        FindReferences();

        capitals = startWithCapitals;

        if (reportInput != null)
        {
            reportInput.DeactivateInputField();
            reportInput.readOnly = true;
        }

        ConnectRuntimeButtons();

        keyboardOpen = false;

        if (keyboardPanel != null)
            keyboardPanel.SetActive(false);

        UpdateKeyLabels();
        UpdateOpenButtonVisibility();
    }

    private void Update()
    {
        UpdateOpenButtonVisibility();
    }

    private void UpdateOpenButtonVisibility()
    {
        if (openButton == null || reportInput == null)
            return;

        // Hide OPEN KEYBOARD when the reporting
        // input is hidden during report preview
        // or report submission.
        bool shouldShow =
            reportInput.gameObject.activeInHierarchy;

        if (openButton.gameObject.activeSelf != shouldShow)
            openButton.gameObject.SetActive(shouldShow);
    }

    // =====================================
    // FIND REFERENCES
    // =====================================

    private void FindReferences()
    {
        if (reportingPanel == null)
        {
            Transform found =
                transform.Find(
                    "GuidedInspectionReportPanel"
                );

            if (found != null)
                reportingPanel = found.gameObject;
        }

        if (reportingPanel != null &&
            reportInput == null)
        {
            Transform found =
                reportingPanel.transform.Find(
                    "ReportInput"
                );

            if (found != null)
            {
                reportInput =
                    found.GetComponent<TMP_InputField>();
            }
        }

        if (keyboardPanel == null)
        {
            Transform found =
                transform.Find(
                    "ReportVirtualKeyboardPanel"
                );

            if (found != null)
                keyboardPanel = found.gameObject;
        }

        if (keyboardPanel != null)
        {
            Transform preview =
                keyboardPanel.transform.Find(
                    "PreviewBackground/KeyboardPreview"
                );

            Transform status =
                keyboardPanel.transform.Find(
                    "KeyboardStatus"
                );

            if (preview != null)
            {
                previewText =
                    preview.GetComponent<TMP_Text>();
            }

            if (status != null)
            {
                statusText =
                    status.GetComponent<TMP_Text>();
            }
        }

        if (reportingPanel != null)
        {
            Transform found =
                reportingPanel.transform.Find(
                    "OpenReportKeyboardButton"
                );

            if (found != null)
            {
                openButton =
                    found.GetComponent<Button>();
            }
        }
    }

    // =====================================
    // EDIT MODE BUILDER
    // =====================================

    [ContextMenu("Build Report Virtual Keyboard")]
    public void BuildKeyboard()
    {
        if (Application.isPlaying)
        {
            Debug.LogWarning(
                "Stop Play Mode before building."
            );
            return;
        }

        FindReferences();

        if (reportingPanel == null ||
            reportInput == null)
        {
            Debug.LogError(
                "GuidedInspectionReportPanel or " +
                "ReportInput is missing."
            );
            return;
        }

        Canvas canvas = GetComponent<Canvas>();

        if (canvas == null ||
            canvas.renderMode != RenderMode.WorldSpace)
        {
            Debug.LogError(
                "Attach the builder to the " +
                "existing World Space training Canvas."
            );
            return;
        }

#if UNITY_EDITOR
        Undo.RegisterFullObjectHierarchyUndo(
            gameObject,
            "Build Report Virtual Keyboard"
        );
#endif

        Transform existing =
            transform.Find(
                "ReportVirtualKeyboardPanel"
            );

        if (existing == null)
        {
            GameObject obj = new GameObject(
                "ReportVirtualKeyboardPanel",
                typeof(RectTransform),
                typeof(Image)
            );

            obj.transform.SetParent(
                transform,
                false
            );

            keyboardPanel = obj;
        }
        else
        {
            keyboardPanel = existing.gameObject;
        }

        SetRect(
            keyboardPanel.transform,
            0, 0,
            800, 660
        );

        Image background =
            keyboardPanel.GetComponent<Image>();

        background.color = white;
        background.raycastTarget = true;

        // Header
        MakeText(
            keyboardPanel.transform,
            "KeyboardTitle",
            "REPORT KEYBOARD",
            29, navy, FontStyles.Bold,
            0, 285, 730, 45
        );

        MakeText(
            keyboardPanel.transform,
            "KeyboardSubtitle",
            "Select keys to enter your report.",
            18, dark, FontStyles.Normal,
            0, 246, 730, 32
        );

        // Text preview
        Transform previewRoot = MakeRectangle(
            keyboardPanel.transform,
            "PreviewBackground",
            lightBlue,
            0, 182, 735, 75
        );

        previewText = MakeText(
            previewRoot,
            "KeyboardPreview",
            "Your report entry will appear here.",
            21, navy, FontStyles.Normal,
            0, 0, 700, 68
        );

        previewText.enableWordWrapping = true;
        previewText.richText = false;

        // Key rows
        CreateKeyRow(
            "1234567890",
            "Number",
            111
        );

        CreateKeyRow(
            "QWERTYUIOP",
            "Top",
            43
        );

        CreateKeyRow(
            "ASDFGHJKL",
            "Middle",
            -25
        );

        CreateKeyRow(
            "ZXCVBNM",
            "Bottom",
            -93
        );

        CreateKeyRow(
            ".,-_/:%@()",
            "Symbols",
            -161
        );

        // Special keys
        CreateKey(
            "Key_CAPS",
            "CAPS",
            "CAPS",
            -313, -235,
            110, 55,
            lightBlue, navy
        );

        CreateKey(
            "Key_SPACE",
            "SPACE",
            "SPACE",
            -110, -235,
            255, 55,
            lightBlue, navy
        );

        CreateKey(
            "Key_BACK",
            "BACK",
            "BACK",
            100, -235,
            135, 55,
            lightBlue, navy
        );

        CreateKey(
            "Key_CLEAR",
            "CLEAR",
            "CLEAR",
            231, -235,
            105, 55,
            lightBlue, navy
        );

        CreateKey(
            "Key_DONE",
            "DONE",
            "DONE",
            339, -235,
            94, 55,
            navy, white
        );

        statusText = MakeText(
            keyboardPanel.transform,
            "KeyboardStatus",
            "Press DONE when finished.",
            17, dark, FontStyles.Normal,
            0, -295, 730, 30
        );

        // Reposition the original hint so
        // OPEN KEYBOARD has its own area.
        Transform hintText =
            reportingPanel.transform.Find(
                "HintText"
            );

        if (hintText != null)
        {
            SetRect(
                hintText,
                0, -184,
                345, 57
            );
        }

        openButton = MakeButton(
            reportingPanel.transform,
            "OpenReportKeyboardButton",
            "OPEN KEYBOARD",
            275, -184,
            185, 47,
            navy, white
        );

        FindReferences();

#if UNITY_EDITOR
        if (copyEZPZFromCalculator)
            ConfigureEZPZInteractionsInEditor();
#endif

        keyboardPanel.transform.SetAsLastSibling();
        keyboardPanel.SetActive(false);

#if UNITY_EDITOR
        EditorUtility.SetDirty(this);
        EditorSceneManager.MarkSceneDirty(
            gameObject.scene
        );
#endif

        Debug.Log(
            "Report virtual keyboard built. " +
            "Save the scene with Ctrl+S."
        );
    }

    // =====================================
    // CREATE ROWS
    // =====================================

    private void CreateKeyRow(
        string characters,
        string prefix,
        float y)
    {
        float spacing = 72f;

        float startX =
            -(characters.Length - 1) *
            spacing / 2f;

        for (int i = 0; i < characters.Length; i++)
        {
            string character =
                characters[i].ToString();

            CreateKey(
                "Key_" + prefix + "_" + i,
                character,
                character,
                startX + i * spacing,
                y,
                64, 53,
                lightBlue, navy
            );
        }
    }

    private void CreateKey(
        string name,
        string caption,
        string value,
        float x,
        float y,
        float width,
        float height,
        Color background,
        Color foreground)
    {
        Button button = MakeButton(
            keyboardPanel.transform,
            name,
            caption,
            x, y,
            width, height,
            background,
            foreground
        );

        ReportVirtualKeyboardKey key =
            button.GetComponent<
                ReportVirtualKeyboardKey
            >();

        if (key == null)
        {
            key = button.gameObject.AddComponent<
                ReportVirtualKeyboardKey
            >();
        }

        key.keyboard = this;
        key.keyValue = value;
    }

    // =====================================
    // BUTTON FACTORY
    // =====================================

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

        if (collider == null)
        {
            collider =
                child.gameObject.AddComponent<
                    BoxCollider
                >();
        }

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
            19,
            foreground,
            FontStyles.Bold,
            0, 0,
            width - 10,
            height - 8
        );

        label.alignment =
            TextAlignmentOptions.Center;

        return button;
    }

    // =====================================
    // TEXT FACTORY
    // =====================================

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
        float height)
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

        text.text = content;
        text.fontSize = size;
        text.fontStyle = style;
        text.color = color;

        text.alignment =
            TextAlignmentOptions.Center;

        text.verticalAlignment =
            VerticalAlignmentOptions.Middle;

        text.enableWordWrapping = false;
        text.raycastTarget = false;

        return text;
    }

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

        image.color = color;
        image.raycastTarget = false;

        return child;
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
        rect.localRotation = Quaternion.identity;
    }

    // =====================================
    // AUTOMATIC EZPZ SETUP
    // =====================================

#if UNITY_EDITOR

    [ContextMenu("Connect Keyboard EZPZ Interactions")]
    public void ConfigureEZPZInteractionsInEditor()
    {
        if (Application.isPlaying)
        {
            Debug.LogWarning(
                "Stop Play Mode to configure EZPZ."
            );
            return;
        }

        FindReferences();

        if (keyboardPanel == null)
        {
            Debug.LogError(
                "Build the keyboard first."
            );
            return;
        }

        MaterialLossCalculatorKey donor =
            FindCalculatorTemplate();

        if (donor == null)
        {
            Debug.LogWarning(
                "No calculator key template found. " +
                "Assign a working calculator key " +
                "in the Inspector."
            );
            return;
        }

        Component donorInteraction =
            FindEZPZComponent(donor.gameObject);

        if (donorInteraction == null)
        {
            Debug.LogWarning(
                "The selected calculator key has " +
                "no InteractableGeneral component."
            );
            return;
        }

        int configured = 0;
        int missingBindings = 0;

        ReportVirtualKeyboardKey[] keys =
            keyboardPanel.GetComponentsInChildren<
                ReportVirtualKeyboardKey
            >(true);

        foreach (ReportVirtualKeyboardKey key in keys)
        {
            Component interaction =
                CopyInteractionComponent(
                    donorInteraction,
                    key.gameObject
                );

            if (interaction == null)
                continue;

            bool wired = RedirectPersistentEvents(
                interaction,
                donor,
                key,
                "Press"
            );

            if (wired)
                configured++;
            else
                missingBindings++;
        }

        if (openButton != null)
        {
            Component interaction =
                CopyInteractionComponent(
                    donorInteraction,
                    openButton.gameObject
                );

            if (interaction != null)
            {
                bool wired = RedirectPersistentEvents(
                    interaction,
                    donor,
                    this,
                    "OpenKeyboard"
                );

                if (wired)
                    configured++;
                else
                    missingBindings++;
            }
        }

        if (missingBindings > 0)
        {
            Debug.LogWarning(
                "EZPZ components copied, but " +
                missingBindings +
                " objects had no compatible " +
                "calculator callback to redirect. " +
                "Check their On Primary Interact " +
                "events before VR testing."
            );
        }

        Debug.Log(
            "EZPZ setup completed. " +
            configured +
            " interaction callbacks configured."
        );

        EditorSceneManager.MarkSceneDirty(
            gameObject.scene
        );
    }

    private MaterialLossCalculatorKey FindCalculatorTemplate()
    {
        if (calculatorKeyTemplate != null)
            return calculatorKeyTemplate;

        MaterialLossCalculatorKey[] candidates =
            FindObjectsByType<
                MaterialLossCalculatorKey
            >(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None
            );

        foreach (MaterialLossCalculatorKey candidate
            in candidates)
        {
            if (candidate.gameObject.scene !=
                gameObject.scene)
            {
                continue;
            }

            if (FindEZPZComponent(
                candidate.gameObject
            ) != null)
            {
                calculatorKeyTemplate = candidate;
                return candidate;
            }
        }

        return null;
    }

    private Component FindEZPZComponent(
        GameObject target)
    {
        Component[] components =
            target.GetComponents<Component>();

        foreach (Component component in components)
        {
            if (component == null)
                continue;

            if (component.GetType().Name ==
                "InteractableGeneral")
            {
                return component;
            }
        }

        return null;
    }

    private Component CopyInteractionComponent(
        Component source,
        GameObject destination)
    {
        if (source == null ||
            destination == null)
            return null;

        Component existing =
            FindEZPZComponent(destination);

        if (existing != null)
            return existing;

        ComponentUtility.CopyComponent(source);

        bool pasted =
            ComponentUtility.PasteComponentAsNew(
                destination
            );

        if (!pasted)
        {
            Debug.LogWarning(
                "Could not copy EZPZ component to " +
                destination.name
            );
            return null;
        }

        return FindEZPZComponent(destination);
    }

    private bool RedirectPersistentEvents(
        Component interaction,
        MaterialLossCalculatorKey originalKey,
        UnityEngine.Object newTarget,
        string newMethod)
    {
        SerializedObject serialized =
            new SerializedObject(interaction);

        SerializedProperty iterator =
            serialized.GetIterator();

        bool redirected = false;

        // UnityEvents store persistent calls in
        // m_PersistentCalls.m_Calls.
        // Redirect callbacks that previously
        // targeted MaterialLossCalculatorKey.
        while (iterator.Next(true))
        {
            if (!iterator.isArray ||
                !iterator.propertyPath.EndsWith(
                    "m_PersistentCalls.m_Calls"
                ))
            {
                continue;
            }

            for (int i = 0; i < iterator.arraySize; i++)
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

                if (target == null || method == null)
                    continue;

                if (target.objectReferenceValue !=
                    originalKey)
                {
                    continue;
                }

                target.objectReferenceValue =
                    newTarget;

                method.stringValue = newMethod;

                redirected = true;
            }
        }

        if (redirected)
        {
            serialized.ApplyModifiedProperties();

            EditorUtility.SetDirty(interaction);
        }

        return redirected;
    }

#endif

    // =====================================
    // CONNECT UNITY BUTTON EVENTS
    // =====================================

    private void ConnectRuntimeButtons()
    {
        if (keyboardPanel != null)
        {
            ReportVirtualKeyboardKey[] keys =
                keyboardPanel.GetComponentsInChildren<
                    ReportVirtualKeyboardKey
                >(true);

            foreach (ReportVirtualKeyboardKey key in keys)
            {
                key.keyboard = this;

                Button button =
                    key.GetComponent<Button>();

                if (button == null)
                    continue;

                button.onClick.RemoveListener(
                    key.Press
                );

                button.onClick.AddListener(
                    key.Press
                );
            }
        }

        if (openButton != null)
        {
            openButton.onClick.RemoveListener(
                OpenKeyboard
            );

            openButton.onClick.AddListener(
                OpenKeyboard
            );
        }
    }

    // =====================================
    // OPEN KEYBOARD
    // =====================================

    public void OpenKeyboard()
    {
        if (lastOpenFrame == Time.frameCount)
            return;

        lastOpenFrame = Time.frameCount;

        FindReferences();

        if (keyboardPanel == null ||
            reportInput == null)
        {
            Debug.LogWarning(
                "Keyboard or report input missing."
            );
            return;
        }

        if (!reportInput.gameObject.activeInHierarchy)
            return;

        reportInput.DeactivateInputField();
        reportInput.readOnly = true;

        keyboardOpen = true;

        keyboardPanel.SetActive(true);
        keyboardPanel.transform.SetAsLastSibling();

        UpdatePreview();
        UpdateKeyLabels();

        Debug.Log("Report keyboard opened.");
    }

    // =====================================
    // HANDLE KEY PRESSES
    // =====================================

    public void PressKey(string value)
    {
        if (!keyboardOpen || reportInput == null)
            return;

        string current = reportInput.text ?? "";

        switch (value)
        {
            case "DONE":
                CloseKeyboard();
                return;

            case "CAPS":
                capitals = !capitals;
                UpdateKeyLabels();
                return;

            case "CLEAR":
                current = "";
                break;

            case "BACK":
                if (current.Length > 0)
                {
                    current = current.Substring(
                        0,
                        current.Length - 1
                    );
                }
                break;

            case "SPACE":
                if (current.Length < maximumCharacters)
                    current += " ";
                break;

            default:
                if (current.Length >= maximumCharacters)
                    return;

                if (value.Length == 1 &&
                    char.IsLetter(value[0]))
                {
                    current += capitals
                        ? value.ToUpperInvariant()
                        : value.ToLowerInvariant();
                }
                else
                {
                    current += value;
                }
                break;
        }

        if (current.Length > maximumCharacters)
        {
            current =
                current.Substring(
                    0,
                    maximumCharacters
                );
        }

        reportInput.SetTextWithoutNotify(current);
        reportInput.ForceLabelUpdate();

        UpdatePreview();
    }

    // =====================================
    // UPDATE KEYBOARD
    // =====================================

    private void UpdatePreview()
    {
        if (reportInput == null)
            return;

        string value = reportInput.text ?? "";

        if (previewText != null)
        {
            previewText.text =
                string.IsNullOrEmpty(value)
                ? "Your report entry will appear here."
                : value;
        }

        if (statusText != null)
        {
            statusText.text =
                value.Length +
                " / " +
                maximumCharacters +
                " characters";
        }
    }

    private void UpdateKeyLabels()
    {
        if (keyboardPanel == null)
            return;

        ReportVirtualKeyboardKey[] keys =
            keyboardPanel.GetComponentsInChildren<
                ReportVirtualKeyboardKey
            >(true);

        foreach (ReportVirtualKeyboardKey key in keys)
        {
            if (key.keyValue.Length != 1 ||
                !char.IsLetter(key.keyValue[0]))
            {
                continue;
            }

            TMP_Text label =
                key.GetComponentInChildren<
                    TMP_Text
                >(true);

            if (label != null)
            {
                label.text = capitals
                    ? key.keyValue.ToUpperInvariant()
                    : key.keyValue.ToLowerInvariant();
            }
        }
    }

    // =====================================
    // CLOSE KEYBOARD
    // =====================================

    public void CloseKeyboard()
    {
        keyboardOpen = false;

        if (reportInput != null)
        {
            reportInput.DeactivateInputField();
            reportInput.ForceLabelUpdate();
        }

        if (keyboardPanel != null)
            keyboardPanel.SetActive(false);

        Debug.Log(
            "Report keyboard closed. " +
            "Press CHECK ENTRY to continue."
        );
    }
}
