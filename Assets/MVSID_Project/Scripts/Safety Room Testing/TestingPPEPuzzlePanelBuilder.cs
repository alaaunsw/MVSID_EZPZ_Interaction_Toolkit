
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TestingPPEPuzzlePanelBuilder : MonoBehaviour
{
    [Header("Puzzle Images - Assign Sprites")]
    public Sprite[] ppeImages = new Sprite[10];

    [Header("UI Colours")]
    public Color panelColor =
        new Color32(120, 150, 166, 255);

    public Color buttonColor =
        new Color32(36, 80, 107, 255);

    public Color scenarioColor =
        new Color32(255, 174, 26, 255);

    [ContextMenu("Build PPE Puzzle Panel")]
    public void BuildPuzzlePanel()
    {
        Transform existing = transform.Find("TestingPPEPuzzleCanvas");

        if (existing != null)
        {
            Debug.LogWarning(
                "Puzzle canvas already exists. Remove it before rebuilding."
            );
            return;
        }

        GameObject canvasObject = new GameObject(
            "TestingPPEPuzzleCanvas",
            typeof(RectTransform),
            typeof(Canvas),
            typeof(CanvasScaler),
            typeof(GraphicRaycaster)
        );

        canvasObject.transform.SetParent(transform, false);

        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;

        RectTransform canvasRect =
            canvasObject.GetComponent<RectTransform>();

        canvasRect.sizeDelta = new Vector2(1200, 850);
        canvasRect.localScale = Vector3.one * 0.0025f;

        GameObject panel = CreateUI(
            "PuzzlePanel",
            canvasObject.transform,
            new Vector2(1200, 850),
            Vector2.zero
        );

        panel.AddComponent<Image>().color = panelColor;

        CreateText(
            "Title",
            panel.transform,
            "PPE SELECTION PUZZLE",
            40,
            new Vector2(1100, 65),
            new Vector2(0, 365),
            Color.white
        );

        GameObject scenarioHeader = CreateUI(
            "ScenarioHeader",
            panel.transform,
            new Vector2(1100, 85),
            new Vector2(0, 285)
        );

        scenarioHeader.AddComponent<Image>().color =
            scenarioColor;

        CreateText(
            "ScenarioText",
            scenarioHeader.transform,
            "SCENARIO 1: ROUTINE PIPELINE INSPECTION",
            31,
            new Vector2(1050, 80),
            Vector2.zero,
            new Color32(25, 35, 45, 255)
        );

        CreateText(
            "TaskText",
            panel.transform,
            "CHALLENGE 1: Match each PPE picture to its correct name.",
            25,
            new Vector2(1100, 55),
            new Vector2(0, 210),
            Color.white
        );

        CreateText(
            "ImagesHeading",
            panel.transform,
            "EQUIPMENT IMAGES",
            25,
            new Vector2(520, 50),
            new Vector2(-285, 155),
            Color.white
        );

        CreateText(
            "NamesHeading",
            panel.transform,
            "EQUIPMENT NAMES",
            25,
            new Vector2(520, 50),
            new Vector2(285, 155),
            Color.white
        );

        string[] shuffledNames =
        {
            "Face Shield",
            "Safety Boots",
            "Ear Muffs",
            "Safety Helmet",
            "Coveralls",
            "Ear Plugs",
            "Safety Gloves",
            "Gas Mask",
            "Safety Vest",
            "Safety Glasses"
        };

        for (int i = 0; i < 10; i++)
        {
            int row = i / 2;
            int column = i % 2;

            float y = 95 - row * 73;

            float imageX = -425 + column * 275;
            float nameX = 145 + column * 275;

            GameObject imageButton = CreateButton(
                "PPEImage_" + i,
                panel.transform,
                new Vector2(245, 62),
                new Vector2(imageX, y)
            );

            GameObject picture = CreateUI(
                "EquipmentImage",
                imageButton.transform,
                new Vector2(54, 54),
                Vector2.zero
            );

            Image pictureImage = picture.AddComponent<Image>();

            if (i < ppeImages.Length &&
                ppeImages[i] != null)
            {
                pictureImage.sprite = ppeImages[i];
                pictureImage.preserveAspect = true;
            }
            else
            {
                pictureImage.color =
                    new Color(1, 1, 1, 0);
            }

            GameObject nameButton = CreateButton(
                "PPEName_" + i,
                panel.transform,
                new Vector2(245, 62),
                new Vector2(nameX, y)
            );

            CreateText(
                "NameText",
                nameButton.transform,
                shuffledNames[i],
                19,
                new Vector2(235, 55),
                Vector2.zero,
                Color.white
            );
        }

        CreateText(
            "FeedbackText",
            panel.transform,
            "Select an image, then match it to the correct name.",
            23,
            new Vector2(1050, 45),
            new Vector2(0, -290),
            Color.white
        );

        CreateText(
            "ProgressText",
            panel.transform,
            "MATCHED: 0 / 10",
            25,
            new Vector2(800, 50),
            new Vector2(0, -345),
            Color.white
        );

        Debug.Log("PPE Puzzle panel created.");
    }

    private GameObject CreateUI(
        string objectName,
        Transform parent,
        Vector2 size,
        Vector2 position)
    {
        GameObject obj = new GameObject(
            objectName,
            typeof(RectTransform)
        );

        obj.transform.SetParent(parent, false);

        RectTransform rect =
            obj.GetComponent<RectTransform>();

        rect.sizeDelta = size;
        rect.anchoredPosition = position;

        return obj;
    }

    private GameObject CreateButton(
        string objectName,
        Transform parent,
        Vector2 size,
        Vector2 position)
    {
        GameObject obj = CreateUI(
            objectName,
            parent,
            size,
            position
        );

        Image image = obj.AddComponent<Image>();
        image.color = buttonColor;

        obj.AddComponent<Button>();

        return obj;
    }

    private TextMeshProUGUI CreateText(
        string objectName,
        Transform parent,
        string content,
        float fontSize,
        Vector2 size,
        Vector2 position,
        Color color)
    {
        GameObject obj = CreateUI(
            objectName,
            parent,
            size,
            position
        );

        TextMeshProUGUI text =
            obj.AddComponent<TextMeshProUGUI>();

        text.text = content;
        text.fontSize = fontSize;
        text.color = color;
        text.alignment =
            TextAlignmentOptions.Center;

        text.enableWordWrapping = true;

        return text;
    }
}
