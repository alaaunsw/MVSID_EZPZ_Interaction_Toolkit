
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MaterialLossPanelBuilder : MonoBehaviour
{
    [Header("Panel Settings")]
    public Vector2 panelSize = new Vector2(520, 560);

    [Header("Generated References")]
    public Button checkAnswerButton;

    private GuidedMaterialLossTraining training;

    private readonly Color navy =
        new Color32(25, 65, 115, 255);

    private readonly Color lightBlue =
        new Color32(235, 244, 253, 255);

    private readonly Color darkText =
        new Color32(49, 67, 87, 255);

    private readonly Color gold =
        new Color32(221, 177, 71, 255);

    private void Awake()
    {
        ConnectButton();
    }

    [ContextMenu("Build Material Loss Training Panel")]
    public void BuildPanel()
    {
        RectTransform panel =
            GetComponent<RectTransform>();

        if (panel == null)
        {
            Debug.LogError(
                "Attach this script to a UI Panel."
            );
            return;
        }

        if (transform.Find("TrainingContent") != null)
        {
            Debug.LogWarning(
                "TrainingContent already exists. " +
                "Delete it before rebuilding."
            );
            return;
        }

        panel.anchorMin = new Vector2(0f, 0.5f);
        panel.anchorMax = new Vector2(0f, 0.5f);
        panel.pivot = new Vector2(0.5f, 0.5f);

        panel.sizeDelta = panelSize;
        panel.anchoredPosition = new Vector2(285f, 0f);

        Image panelImage = GetComponent<Image>();

        if (panelImage == null)
            panelImage = gameObject.AddComponent<Image>();

        panelImage.color = Color.white;
        panelImage.raycastTarget = true;

        GameObject content = new GameObject(
            "TrainingContent",
            typeof(RectTransform)
        );

        content.transform.SetParent(transform, false);

        RectTransform contentRect =
            content.GetComponent<RectTransform>();

        contentRect.anchorMin = Vector2.zero;
        contentRect.anchorMax = Vector2.one;
        contentRect.offsetMin = Vector2.zero;
        contentRect.offsetMax = Vector2.zero;

        Transform root = content.transform;

        // Title
        TMP_Text title = CreateText(
            root,
            "TitleText",
            "CALCULATE MATERIAL LOSS",
            new Vector2(0, 225),
            new Vector2(460, 50),
            27,
            navy,
            true
        );

        // Gold divider
        CreateImage(
            root,
            "GoldDivider",
            new Vector2(0, 193),
            new Vector2(450, 4),
            gold
        );

        // Step heading
        TMP_Text stepText = CreateText(
            root,
            "StepText",
            "STEP 1 OF 4",
            new Vector2(0, 155),
            new Vector2(450, 40),
            23,
            navy,
            true
        );

        // Instruction background
        CreateImage(
            root,
            "InstructionBackground",
            new Vector2(0, 65),
            new Vector2(450, 120),
            lightBlue
        );

        // Instruction
        TMP_Text instructionText = CreateText(
            root,
            "InstructionText",
            "Open the PIPE_0493 information card. " +
            "Find the original wall thickness " +
            "and enter its value in millimetres.",
            new Vector2(0, 65),
            new Vector2(410, 100),
            20,
            darkText,
            false
        );

        // Input label
        CreateText(
            root,
            "InputLabel",
            "ORIGINAL WALL THICKNESS (mm)",
            new Vector2(0, -35),
            new Vector2(440, 35),
            18,
            navy,
            true
        );

        // Editable answer input
        TMP_InputField input = CreateInputField(
            root,
            "AnswerInput",
            new Vector2(0, -88),
            new Vector2(400, 55)
        );

        // Feedback
        TMP_Text feedbackText = CreateText(
            root,
            "FeedbackText",
            "Enter your answer to continue.",
            new Vector2(0, -152),
            new Vector2(440, 65),
            19,
            darkText,
            false
        );

        // Answer button
        checkAnswerButton = CreateButton(
            root,
            "CheckAnswerButton",
            "CHECK ANSWER",
            new Vector2(0, -223),
            new Vector2(250, 55)
        );

        // Connect generated UI to training script
        training = GetComponent<GuidedMaterialLossTraining>();

        if (training == null)
        {
            training =
                gameObject.AddComponent<GuidedMaterialLossTraining>();
        }

        training.stepText = stepText;
        training.instructionText = instructionText;
        training.answerInput = input;
        training.feedbackText = feedbackText;

        Debug.Log(
            "Material Loss Training Panel created successfully."
        );
    }

    private TMP_Text CreateText(
        Transform parent,
        string objectName,
        string message,
        Vector2 position,
        Vector2 size,
        int fontSize,
        Color color,
        bool bold)
    {
        GameObject obj = new GameObject(
            objectName,
            typeof(RectTransform),
            typeof(TextMeshProUGUI)
        );

        obj.transform.SetParent(parent, false);

        RectTransform rect =
            obj.GetComponent<RectTransform>();

        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = size;
        rect.anchoredPosition = position;

        TMP_Text tmp = obj.GetComponent<TMP_Text>();

        tmp.text = message;
        tmp.fontSize = fontSize;
        tmp.color = color;

        tmp.alignment =
            TextAlignmentOptions.Center;

        tmp.verticalAlignment =
            VerticalAlignmentOptions.Middle;

        tmp.enableWordWrapping = true;
        tmp.raycastTarget = false;

        if (bold)
            tmp.fontStyle = FontStyles.Bold;

        return tmp;
    }

    private Image CreateImage(
        Transform parent,
        string objectName,
        Vector2 position,
        Vector2 size,
        Color color)
    {
        GameObject obj = new GameObject(
            objectName,
            typeof(RectTransform),
            typeof(Image)
        );

        obj.transform.SetParent(parent, false);

        RectTransform rect =
            obj.GetComponent<RectTransform>();

        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = size;
        rect.anchoredPosition = position;

        Image image = obj.GetComponent<Image>();
        image.color = color;
        image.raycastTarget = false;

        return image;
    }

    private TMP_InputField CreateInputField(
        Transform parent,
        string objectName,
        Vector2 position,
        Vector2 size)
    {
        GameObject obj = new GameObject(
            objectName,
            typeof(RectTransform),
            typeof(Image),
            typeof(TMP_InputField)
        );

        obj.transform.SetParent(parent, false);

        RectTransform rect =
            obj.GetComponent<RectTransform>();

        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = size;
        rect.anchoredPosition = position;

        Image background = obj.GetComponent<Image>();
        background.color =
            new Color32(240, 246, 253, 255);

        // Text viewport
        GameObject viewport = new GameObject(
            "Text Area",
            typeof(RectTransform),
            typeof(RectMask2D)
        );

        viewport.transform.SetParent(obj.transform, false);

        RectTransform viewportRect =
            viewport.GetComponent<RectTransform>();

        viewportRect.anchorMin = Vector2.zero;
        viewportRect.anchorMax = Vector2.one;
        viewportRect.offsetMin = new Vector2(15, 5);
        viewportRect.offsetMax = new Vector2(-15, -5);

        // Input text
        TMP_Text inputText = CreateText(
            viewport.transform,
            "Text",
            "",
            Vector2.zero,
            new Vector2(size.x - 40, 40),
            24,
            darkText,
            false
        );

        // Placeholder
        TMP_Text placeholder = CreateText(
            viewport.transform,
            "Placeholder",
            "Enter thickness in mm",
            Vector2.zero,
            new Vector2(size.x - 40, 40),
            20,
            Color.gray,
            false
        );

        TMP_InputField field =
            obj.GetComponent<TMP_InputField>();

        field.targetGraphic = background;
        field.textViewport = viewportRect;
        field.textComponent = (TextMeshProUGUI)inputText;
        field.placeholder = (TextMeshProUGUI)placeholder;

        field.contentType =
            TMP_InputField.ContentType.DecimalNumber;

        field.lineType =
            TMP_InputField.LineType.SingleLine;

        field.characterLimit = 12;

        return field;
    }

    private Button CreateButton(
        Transform parent,
        string objectName,
        string label,
        Vector2 position,
        Vector2 size)
    {
        GameObject obj = new GameObject(
            objectName,
            typeof(RectTransform),
            typeof(Image),
            typeof(Button)
        );

        obj.transform.SetParent(parent, false);

        RectTransform rect =
            obj.GetComponent<RectTransform>();

        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = size;
        rect.anchoredPosition = position;

        Image image = obj.GetComponent<Image>();
        image.color = navy;

        Button button = obj.GetComponent<Button>();
        button.targetGraphic = image;

        CreateText(
            obj.transform,
            "ButtonText",
            label,
            Vector2.zero,
            new Vector2(size.x - 20, size.y - 10),
            20,
            Color.white,
            true
        );

        return button;
    }

    private void ConnectButton()
    {
        training = GetComponent<GuidedMaterialLossTraining>();

        if (checkAnswerButton == null)
        {
            Transform found = transform.Find(
                "TrainingContent/CheckAnswerButton"
            );

            if (found != null)
                checkAnswerButton =
                    found.GetComponent<Button>();
        }

        if (training != null && checkAnswerButton != null)
        {
            checkAnswerButton.onClick.RemoveListener(
                training.CheckAnswer
            );

            checkAnswerButton.onClick.AddListener(
                training.CheckAnswer
            );
        }
    }
}
