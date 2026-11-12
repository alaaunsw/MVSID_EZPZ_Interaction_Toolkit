
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InspectionWelcomeBuilder : MonoBehaviour
{
    [Header("Generated Panel")]
    public GameObject welcomePanel;

    [Header("Position")]
    public float distanceFromPlayer = 2f;
    public float leftOffset = -0.65f;
    public float verticalOffset = 0f;

    private readonly Color navy =
        new Color32(12, 49, 95, 255);

    private readonly Color gold =
        new Color32(238, 184, 64, 255);

    [ContextMenu("Build Inspection Welcome Panel")]
    public void BuildWelcomePanel()
    {
        if (transform.Find("InspectionWelcomePanel") != null)
        {
            Debug.LogWarning(
                "Welcome panel already exists."
            );
            return;
        }

        GameObject canvasObject = gameObject;
        Canvas canvas = GetComponent<Canvas>();

        if (canvas == null)
        {
            Debug.LogError(
                "Attach this script to InspectionWelcomeCanvas."
            );
            return;
        }

        canvas.renderMode = RenderMode.WorldSpace;

        RectTransform canvasRect =
            GetComponent<RectTransform>();

        canvasRect.sizeDelta = new Vector2(850, 650);
        canvasRect.localScale = Vector3.one * 0.003f;

        // White panel with blue border.
        welcomePanel = CreateImage(
            canvasObject.transform,
            "InspectionWelcomePanel",
            Vector2.zero,
            new Vector2(700, 570),
            Color.white
        );

        Outline outline =
            welcomePanel.AddComponent<Outline>();

        outline.effectColor = navy;
        outline.effectDistance =
            new Vector2(3, -3);

        // Navy number badge.
        GameObject badge = CreateImage(
            welcomePanel.transform,
            "StepNumberBadge",
            new Vector2(-255, 215),
            new Vector2(94, 94),
            navy
        );

        TextMeshProUGUI number = CreateText(
            badge.transform,
            "NumberText",
            "1",
            Vector2.zero,
            new Vector2(80, 80),
            48,
            Color.white,
            true
        );

        number.alignment =
            TextAlignmentOptions.Center;

        // Main title.
        CreateText(
            welcomePanel.transform,
            "WelcomeTitle",
            "HYPOTHETICAL INTERNAL\n" +
            "OIL PIPELINE INSPECTION",
            new Vector2(80, 215),
            new Vector2(440, 110),
            33,
            navy,
            true
        );

        // Gold horizontal divider.
        CreateImage(
            welcomePanel.transform,
            "GoldDivider",
            new Vector2(0, 135),
            new Vector2(590, 4),
            gold
        );

        // Introduction text.
        CreateText(
            welcomePanel.transform,
            "IntroductionText",
            "Welcome to the internal inspection\n" +
            "simulation of an oil pipeline.",
            new Vector2(0, 55),
            new Vector2(585, 100),
            29,
            navy,
            false
        );

        // Training description.
        CreateText(
            welcomePanel.transform,
            "DescriptionText",
            "In this training, you will learn how to\n" +
            "inspect pipeline sections for signs of\n" +
            "corrosion and how to report issues.",
            new Vector2(0, -85),
            new Vector2(585, 140),
            27,
            navy,
            false
        );

        // Next button.
        GameObject nextButton = CreateImage(
            welcomePanel.transform,
            "NextButton",
            new Vector2(0, -225),
            new Vector2(590, 76),
            navy
        );

        nextButton.GetComponent<Image>()
            .raycastTarget = true;

        nextButton.AddComponent<Button>();

        TextMeshProUGUI nextText = CreateText(
            nextButton.transform,
            "NextText",
            "NEXT   \u25B6",
            Vector2.zero,
            new Vector2(540, 65),
            34,
            Color.white,
            true
        );

        nextText.alignment =
            TextAlignmentOptions.Center;

        Debug.Log(
            "Inspection Welcome Panel created successfully."
        );
    }

    private GameObject CreateImage(
        Transform parent,
        string name,
        Vector2 position,
        Vector2 size,
        Color color)
    {
        GameObject obj = new GameObject(
            name,
            typeof(RectTransform),
            typeof(Image)
        );

        obj.transform.SetParent(parent, false);

        RectTransform rect =
            obj.GetComponent<RectTransform>();

        rect.sizeDelta = size;
        rect.anchoredPosition = position;

        Image image = obj.GetComponent<Image>();
        image.color = color;
        image.raycastTarget = false;

        return obj;
    }

    private TextMeshProUGUI CreateText(
        Transform parent,
        string name,
        string content,
        Vector2 position,
        Vector2 size,
        float fontSize,
        Color color,
        bool bold)
    {
        GameObject obj = new GameObject(
            name,
            typeof(RectTransform),
            typeof(TextMeshProUGUI)
        );

        obj.transform.SetParent(parent, false);

        RectTransform rect =
            obj.GetComponent<RectTransform>();

        rect.sizeDelta = size;
        rect.anchoredPosition = position;

        TextMeshProUGUI text =
            obj.GetComponent<TextMeshProUGUI>();

        text.text = content;
        text.fontSize = fontSize;
        text.color = color;

        text.fontStyle = bold
            ? FontStyles.Bold
            : FontStyles.Normal;

        text.alignment =
            TextAlignmentOptions.Left;

        text.verticalAlignment =
            VerticalAlignmentOptions.Middle;

        text.enableWordWrapping = true;
        text.raycastTarget = false;

        return text;
    }
}
