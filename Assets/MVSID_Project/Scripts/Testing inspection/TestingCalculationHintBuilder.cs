
using UnityEngine;
using UnityEngine.UI;
using TMPro;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public class TestingCalculationHintBuilder : MonoBehaviour
{
    [Header("Existing Assessment Canvas")]
    public RectTransform assessmentPanel;

    [Header("Hint Button Positions")]
    public Vector2 materialLossButtonPosition =
        new Vector2(340, 140);

    public Vector2 percentageButtonPosition =
        new Vector2(340, 0);

    public Vector2 conditionButtonPosition =
        new Vector2(340, -130);

    [Header("Hint Panel Position")]
    public Vector2 hintPanelPosition =
        new Vector2(720, 0);

    public Vector2 hintPanelSize =
        new Vector2(560, 300);

    [Header("Appearance")]
    public Color buttonColor =
        new Color32(65, 95, 115, 255);

    public Color panelColor =
        new Color32(30, 48, 62, 255);

    public Color headingColor =
        new Color32(255, 215, 70, 255);

    [Header("Generated Hint Objects")]
    public GameObject hintRoot;

    public GameObject materialLossHintButton;
    public GameObject percentageHintButton;
    public GameObject conditionHintButton;

    public GameObject materialLossHintPanel;
    public GameObject percentageHintPanel;
    public GameObject conditionHintPanel;

    // ==========================================
    // BUILD ALL THREE HINTS
    // ==========================================

    [ContextMenu("Build Assessment Hints")]
    public void BuildAssessmentHints()
    {
        if (Application.isPlaying)
        {
            Debug.LogWarning(
                "Stop Play Mode before building."
            );

            return;
        }

        if (assessmentPanel == null)
        {
            Debug.LogError(
                "Assign the assessment panel first."
            );

            return;
        }

#if UNITY_EDITOR
        Undo.RecordObject(
            this,
            "Build Assessment Hints"
        );
#endif

        if (hintRoot != null)
        {
            if (hintRoot == gameObject ||
                assessmentPanel.IsChildOf(
                    hintRoot.transform))
            {
                Debug.LogError(
                    "Hint Root must be a separate child."
                );

                return;
            }

#if UNITY_EDITOR
            Undo.DestroyObjectImmediate(hintRoot);
#else
            DestroyImmediate(hintRoot);
#endif
        }

        // Create a separate parent for the hints.

        hintRoot = new GameObject(
            "AssessmentHints",
            typeof(RectTransform)
        );

        hintRoot.transform.SetParent(
            assessmentPanel,
            false
        );

        RectTransform rootRect =
            hintRoot.GetComponent<RectTransform>();

        rootRect.anchorMin = Vector2.zero;
        rootRect.anchorMax = Vector2.one;
        rootRect.offsetMin = Vector2.zero;
        rootRect.offsetMax = Vector2.zero;

#if UNITY_EDITOR
        Undo.RegisterCreatedObjectUndo(
            hintRoot,
            "Create Assessment Hints"
        );
#endif

        // ======================================
        // HINT BUTTONS
        // ======================================

        materialLossHintButton = CreateButton(
            "MaterialLossHintButton",
            "HINT",
            materialLossButtonPosition
        );

        percentageHintButton = CreateButton(
            "PercentageHintButton",
            "HINT",
            percentageButtonPosition
        );

        conditionHintButton = CreateButton(
            "ConditionHintButton",
            "HINT",
            conditionButtonPosition
        );

        // ======================================
        // MATERIAL LOSS FORMULA
        // ======================================

        materialLossHintPanel = CreateHintPanel(
            "MaterialLossHintPanel",

            "MATERIAL LOSS (mm)",

            "Material loss =\n\n" +
            "Reference wall thickness\n" +
            "- Remaining wall thickness\n\n" +
            "Enter your result in millimetres (mm)."
        );

        // ======================================
        // PERCENTAGE FORMULA
        // ======================================

        percentageHintPanel = CreateHintPanel(
            "PercentageHintPanel",

            "MATERIAL LOSS (%)",

            "Material loss percentage =\n\n" +
            "(Material loss / Reference wall thickness)" +
            "\n\nx 100\n\n" +
            "Enter your result as a percentage."
        );

        // ======================================
        // CONDITION CLASSIFICATION
        // ======================================

        conditionHintPanel = CreateHintPanel(
            "ConditionHintPanel",

            "CONDITION CLASSIFICATION",

            "<color=#78E89A>NORMAL</color>\n" +
            "Material loss: 2 mm or less\n\n" +

            "<color=#FFD75A>MODERATE</color>\n" +
            "Above 2 mm and up to 5 mm\n\n" +

            "<color=#FF7777>CRITICAL</color>\n" +
            "Material loss: above 5 mm"
        );

        HideAllHints();

#if UNITY_EDITOR
        EditorUtility.SetDirty(this);

        EditorSceneManager.MarkSceneDirty(
            gameObject.scene
        );
#endif

        Debug.Log(
            "[TESTING] Three assessment hints created."
        );
    }

    // ==========================================
    // CREATE HINT BUTTON
    // ==========================================

    private GameObject CreateButton(
        string objectName,
        string label,
        Vector2 position)
    {
        GameObject buttonObject = new GameObject(
            objectName,

            typeof(RectTransform),
            typeof(Image),
            typeof(Button),
            typeof(BoxCollider)
        );

        buttonObject.transform.SetParent(
            hintRoot.transform,
            false
        );

        RectTransform rect =
            buttonObject.GetComponent<RectTransform>();

        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);

        rect.sizeDelta = new Vector2(110, 45);
        rect.anchoredPosition = position;

        Image image =
            buttonObject.GetComponent<Image>();

        image.color = buttonColor;

        Button button =
            buttonObject.GetComponent<Button>();

        button.targetGraphic = image;

        BoxCollider box =
            buttonObject.GetComponent<BoxCollider>();

        box.size = new Vector3(110, 45, 12);

        CreateText(
            buttonObject.transform,
            "ButtonLabel",
            label,
            22,
            headingColor,
            new Vector2(110, 45),
            Vector2.zero,
            FontStyles.Bold
        );

        return buttonObject;
    }

    // ==========================================
    // CREATE FORMULA PANEL
    // ==========================================

    private GameObject CreateHintPanel(
        string objectName,
        string heading,
        string description)
    {
        GameObject panel = new GameObject(
            objectName,
            typeof(RectTransform),
            typeof(Image)
        );

        panel.transform.SetParent(
            hintRoot.transform,
            false
        );

        RectTransform rect =
            panel.GetComponent<RectTransform>();

        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);

        rect.sizeDelta = hintPanelSize;
        rect.anchoredPosition = hintPanelPosition;

        Image background =
            panel.GetComponent<Image>();

        background.color = panelColor;
        background.raycastTarget = false;

        // Heading.

        CreateText(
            panel.transform,
            "HintTitle",
            heading,
            28,
            headingColor,
            new Vector2(
                hintPanelSize.x - 30,
                50
            ),
            new Vector2(
                0,
                hintPanelSize.y / 2 - 35
            ),
            FontStyles.Bold
        );

        // Formula or condition information.

        CreateText(
            panel.transform,
            "HintDescription",
            description,
            23,
            Color.white,
            new Vector2(
                hintPanelSize.x - 40,
                hintPanelSize.y - 80
            ),
            new Vector2(0, -25),
            FontStyles.Normal
        );

        panel.SetActive(false);

        return panel;
    }

    // ==========================================
    // CREATE TEXT
    // ==========================================

    private void CreateText(
        Transform parent,
        string objectName,
        string content,
        float fontSize,
        Color color,
        Vector2 size,
        Vector2 position,
        FontStyles style)
    {
        GameObject textObject = new GameObject(
            objectName,
            typeof(RectTransform)
        );

        textObject.transform.SetParent(
            parent,
            false
        );

        TextMeshProUGUI text =
            textObject.AddComponent<TextMeshProUGUI>();

        text.text = content;
        text.fontSize = fontSize;
        text.color = color;
        text.fontStyle = style;

        text.alignment =
            TextAlignmentOptions.Center;

        text.enableAutoSizing = true;
        text.fontSizeMin = 16;
        text.fontSizeMax = fontSize;

        text.richText = true;
        text.raycastTarget = false;

        text.rectTransform.sizeDelta = size;

        text.rectTransform.anchoredPosition =
            position;
    }

    // ==========================================
    // RUNTIME SETUP
    // ==========================================

    private void Awake()
    {
        HideAllHints();

        ConnectButton(
            materialLossHintButton,
            ToggleMaterialLossHint
        );

        ConnectButton(
            percentageHintButton,
            TogglePercentageHint
        );

        ConnectButton(
            conditionHintButton,
            ToggleConditionHint
        );
    }

    private void ConnectButton(
        GameObject buttonObject,
        UnityEngine.Events.UnityAction action)
    {
        if (buttonObject == null)
            return;

        Button button =
            buttonObject.GetComponent<Button>();

        if (button != null)
        {
            button.onClick.AddListener(action);
        }
    }

    // ==========================================
    // EZPZ INTERACTION FUNCTIONS
    // ==========================================

    public void ToggleMaterialLossHint()
    {
        ToggleHint(materialLossHintPanel);
    }

    public void TogglePercentageHint()
    {
        ToggleHint(percentageHintPanel);
    }

    public void ToggleConditionHint()
    {
        ToggleHint(conditionHintPanel);
    }

    // ==========================================
    // SHOW ONLY ONE HINT AT A TIME
    // ==========================================

    private void ToggleHint(GameObject panel)
    {
        if (panel == null)
            return;

        bool shouldOpen = !panel.activeSelf;

        HideAllHints();

        panel.SetActive(shouldOpen);
    }

    // ==========================================
    // HIDE ALL FORMULA PANELS
    // ==========================================

    public void HideAllHints()
    {
        if (materialLossHintPanel != null)
            materialLossHintPanel.SetActive(false);

        if (percentageHintPanel != null)
            percentageHintPanel.SetActive(false);

        if (conditionHintPanel != null)
            conditionHintPanel.SetActive(false);
    }

    private void OnDisable()
    {
        HideAllHints();
    }
}
