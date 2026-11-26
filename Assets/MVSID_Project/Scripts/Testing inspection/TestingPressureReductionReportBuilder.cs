
using TMPro;
using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Events;
#endif

public class TestingPressureReductionReportBuilder : MonoBehaviour
{
    [Header("Existing System")]
    public TestingPressureReductionReport reportController;

    [Header("Canvas")]
    public RectTransform canvasParent;

    [Header("Panel Settings")]
    public Vector2 panelSize = new Vector2(850f, 420f);

    [Header("Panel Colours")]
    public Color backgroundColor =
        new Color32(15, 15, 15, 240);

    public Color titleColor =
        new Color32(238, 236, 220, 255);

    public Color yellowColor =
        new Color32(245, 220, 45, 255);

    public Color inputColor =
        new Color32(48, 48, 48, 255);

    public Color buttonColor =
        new Color32(65, 65, 65, 255);

    public Color placeholderColor =
        new Color32(195, 188, 160, 255);

    [ContextMenu("Build Pressure Reduction Report Panel")]
    public void BuildPressureReductionReportPanel()
    {
        if (Application.isPlaying)
        {
            Debug.LogWarning("Exit Play Mode before building.");
            return;
        }

        if (reportController == null)
        {
            reportController =
                GetComponent<TestingPressureReductionReport>();
        }

        if (canvasParent == null)
        {
            canvasParent = GetComponent<RectTransform>();
        }

        if (reportController == null || canvasParent == null)
        {
            Debug.LogError(
                "Assign Report Controller and Canvas Parent."
            );
            return;
        }

        string panelName =
            "TestingPressureReductionReportPanel";

        // Remove the previously generated panel.
        Transform existing =
            canvasParent.Find(panelName);

        if (existing != null)
        {
#if UNITY_EDITOR
            Undo.DestroyObjectImmediate(existing.gameObject);
#else
            DestroyImmediate(existing.gameObject);
#endif
        }

        // Main background.
        GameObject panel = new GameObject(
            panelName,
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image)
        );

        RectTransform panelRect =
            panel.GetComponent<RectTransform>();

        panelRect.SetParent(canvasParent, false);

        SetRect(
            panelRect,
            panelSize,
            Vector2.zero
        );

        panelRect.localScale = new Vector3(2.4f, 2.4f, 2.4f);
        panelRect.localRotation = Quaternion.identity;

        Image background = panel.GetComponent<Image>();
        background.color = backgroundColor;

        // Title.
        CreateText(
            "Title",
            panelRect,
            "PRESSURE REDUCTION REPORT",
            36,
            titleColor,
            new Vector2(800, 50),
            new Vector2(0, 167),
            FontStyles.Bold
        );

        // Yellow task subtitle.
        CreateText(
            "Subtitle",
            panelRect,
            "TASK 07 - REPORT UPDATED PRESSURE",
            24,
            yellowColor,
            new Vector2(800, 40),
            new Vector2(0, 122),
            FontStyles.Bold
        );

        // Description.
        CreateText(
            "Description",
            panelRect,
            "Enter the updated pressure reading " +
            "observed on PIPE_0493.",
            22,
            titleColor,
            new Vector2(750, 65),
            new Vector2(0, 65),
            FontStyles.Normal
        );

        // Pressure label.
        CreateText(
            "PressureLabel",
            panelRect,
            "PRESSURE READING (psi)",
            25,
            titleColor,
            new Vector2(650, 40),
            new Vector2(0, -5),
            FontStyles.Bold
        );

        // Pressure input.
        TMP_InputField pressureInput =
            CreatePressureInput(
                panelRect,
                new Vector2(520, 60),
                new Vector2(0, -58)
            );

        // Submit button.
        Button submitButton = CreateSubmitButton(
            panelRect,
            new Vector2(370, 60),
            new Vector2(0, -145)
        );

        // Automatically assign report references.
        reportController.reportPanel = panel;
        reportController.pressureInput = pressureInput;

        if (reportController.dialogue == null)
        {
            reportController.dialogue =
                GetComponent<TestingInspectionDialogue>();
        }

        if (reportController.pressureReduction == null)
        {
            reportController.pressureReduction =
                GetComponent<TestingPressureReduction>();
        }

#if UNITY_EDITOR
        // Save the submit event permanently in Inspector.
        UnityEventTools.AddPersistentListener(
            submitButton.onClick,
            reportController.SubmitReport
        );
#else
        submitButton.onClick.AddListener(
            reportController.SubmitReport
        );
#endif

        // Keep hidden until Task 6 is completed.
        panel.SetActive(false);

#if UNITY_EDITOR
        Undo.RegisterCreatedObjectUndo(
            panel,
            "Build Pressure Reduction Report"
        );

        EditorUtility.SetDirty(reportController);
        EditorUtility.SetDirty(this);

        UnityEditor.SceneManagement.EditorSceneManager
            .MarkSceneDirty(gameObject.scene);
#endif

        Debug.Log(
            "[TESTING] Task 7 pressure reduction " +
            "report panel successfully created."
        );
    }

    private TMP_InputField CreatePressureInput(
        RectTransform parent,
        Vector2 size,
        Vector2 position)
    {
        GameObject inputObject = new GameObject(
            "PressureInput",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image),
            typeof(TMP_InputField)
        );

        RectTransform inputRect =
            inputObject.GetComponent<RectTransform>();

        inputRect.SetParent(parent, false);

        SetRect(inputRect, size, position);

        Image inputBackground =
            inputObject.GetComponent<Image>();

        inputBackground.color = inputColor;

        TMP_InputField input =
            inputObject.GetComponent<TMP_InputField>();

        // Text viewport.
        GameObject viewport = new GameObject(
            "Text Area",
            typeof(RectTransform),
            typeof(RectMask2D)
        );

        RectTransform viewportRect =
            viewport.GetComponent<RectTransform>();

        viewportRect.SetParent(inputRect, false);

        viewportRect.anchorMin = Vector2.zero;
        viewportRect.anchorMax = Vector2.one;

        viewportRect.offsetMin =
            new Vector2(15, 5);

        viewportRect.offsetMax =
            new Vector2(-15, -5);

        // Placeholder text.
        TextMeshProUGUI placeholder = CreateText(
            "Placeholder",
            viewportRect,
            "Enter updated pressure",
            23,
            placeholderColor,
            size,
            Vector2.zero,
            FontStyles.Normal
        );

        StretchRect(
            placeholder.rectTransform
        );

        placeholder.alignment =
            TextAlignmentOptions.MidlineLeft;

        // Entered pressure text.
        TextMeshProUGUI inputText = CreateText(
            "Text",
            viewportRect,
            "",
            25,
            titleColor,
            size,
            Vector2.zero,
            FontStyles.Normal
        );

        StretchRect(inputText.rectTransform);

        inputText.alignment =
            TextAlignmentOptions.MidlineLeft;

        input.textViewport = viewportRect;
        input.textComponent = inputText;
        input.placeholder = placeholder;
        input.targetGraphic = inputBackground;

        input.contentType =
            TMP_InputField.ContentType.DecimalNumber;

        input.lineType =
            TMP_InputField.LineType.SingleLine;

        input.characterLimit = 8;

        input.customCaretColor = true;
        input.caretColor = Color.white;

        input.text = "";

        return input;
    }

    private Button CreateSubmitButton(
        RectTransform parent,
        Vector2 size,
        Vector2 position)
    {
        GameObject buttonObject = new GameObject(
            "SubmitButton",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image),
            typeof(Button),
            typeof(BoxCollider)
        );

        RectTransform buttonRect =
            buttonObject.GetComponent<RectTransform>();

        buttonRect.SetParent(parent, false);

        SetRect(buttonRect, size, position);

        Image image =
            buttonObject.GetComponent<Image>();

        image.color = buttonColor;

        Button button =
            buttonObject.GetComponent<Button>();

        button.targetGraphic = image;

        ColorBlock colors = button.colors;

        colors.normalColor = Color.white;

        colors.highlightedColor =
            new Color32(190, 190, 190, 255);

        colors.pressedColor =
            new Color32(145, 145, 145, 255);

        button.colors = colors;

        CreateText(
            "ButtonText",
            buttonRect,
            "SUBMIT REPORT",
            27,
            titleColor,
            size,
            Vector2.zero,
            FontStyles.Bold
        );

        BoxCollider collider =
            buttonObject.GetComponent<BoxCollider>();

        collider.center = Vector3.zero;

        collider.size = new Vector3(
            size.x,
            size.y,
            5f
        );

        return button;
    }

    private TextMeshProUGUI CreateText(
        string objectName,
        RectTransform parent,
        string content,
        float fontSize,
        Color color,
        Vector2 size,
        Vector2 position,
        FontStyles style)
    {
        GameObject textObject = new GameObject(
            objectName,
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(TextMeshProUGUI)
        );

        RectTransform rect =
            textObject.GetComponent<RectTransform>();

        rect.SetParent(parent, false);

        SetRect(rect, size, position);

        TextMeshProUGUI text =
            textObject.GetComponent<TextMeshProUGUI>();

        text.text = content;
        text.fontSize = fontSize;
        text.color = color;
        text.fontStyle = style;

        text.alignment =
            TextAlignmentOptions.Center;

        text.raycastTarget = false;

        return text;
    }

    private void SetRect(
        RectTransform rect,
        Vector2 size,
        Vector2 position)
    {
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);

        rect.pivot = new Vector2(0.5f, 0.5f);

        rect.sizeDelta = size;
        rect.anchoredPosition = position;

        rect.localScale = Vector3.one;
        rect.localRotation = Quaternion.identity;
    }

    private void StretchRect(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }
}
