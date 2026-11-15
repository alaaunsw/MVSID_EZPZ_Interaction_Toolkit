
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MaterialLossCalculatorBuilder : MonoBehaviour
{
    [Header("Existing Working Training")]
    public GuidedMaterialLossTraining training;

    [Header("EZPZ Calculator Button Template")]
    public MaterialLossCalculatorKey keyTemplate;

    [Header("Optional Layout")]
    public RectTransform existingTrainingPanel;
    public Vector2 calculatorPosition =
        new Vector2(275, 0);

    private readonly Color navy =
        new Color32(25, 65, 115, 255);

    private readonly Color light =
        new Color32(243, 247, 251, 255);

    private readonly Color dark =
        new Color32(48, 58, 75, 255);

    [ContextMenu("Build EZPZ Calculator")]
    public void Build()
    {
        if (training == null || keyTemplate == null)
        {
            Debug.LogError(
                "Assign Training and Key Template first."
            );
            return;
        }

        if (transform.Find("CalculatorPanel") != null)
        {
            Debug.LogWarning(
                "CalculatorPanel already exists. " +
                "Delete that generated panel before rebuilding."
            );
            return;
        }

        GameObject panel = new GameObject(
            "CalculatorPanel",
            typeof(RectTransform),
            typeof(Image)
        );

        panel.transform.SetParent(transform, false);

        RectTransform panelRect =
            panel.GetComponent<RectTransform>();

        panelRect.anchorMin =
            new Vector2(0.5f, 0.5f);

        panelRect.anchorMax =
            new Vector2(0.5f, 0.5f);

        panelRect.sizeDelta =
            new Vector2(470, 560);

        panelRect.anchoredPosition =
            calculatorPosition;

        Image background =
            panel.GetComponent<Image>();

        background.color = light;
        background.raycastTarget = false;

        MaterialLossCalculator calculator =
            panel.AddComponent<MaterialLossCalculator>();

        calculator.training = training;

        Transform root = panel.transform;

        MakeText(
            root,
            "CalculatorTitle",
            "CALCULATOR",
            new Vector2(0, 245),
            new Vector2(430, 40),
            25,
            navy
        );

        calculator.expressionText = MakeText(
            root,
            "ExpressionText",
            "",
            new Vector2(0, 192),
            new Vector2(420, 28),
            17,
            dark
        );

        GameObject displayBG = new GameObject(
            "DisplayBackground",
            typeof(RectTransform),
            typeof(Image)
        );

        displayBG.transform.SetParent(root, false);

        RectTransform displayRect =
            displayBG.GetComponent<RectTransform>();

        displayRect.sizeDelta =
            new Vector2(430, 64);

        displayRect.anchoredPosition =
            new Vector2(0, 143);

        Image displayImage =
            displayBG.GetComponent<Image>();

        displayImage.color = Color.white;
        displayImage.raycastTarget = false;

        calculator.displayText = MakeText(
            displayBG.transform,
            "CalculatorDisplay",
            "0",
            Vector2.zero,
            new Vector2(400, 54),
            31,
            dark
        );

        calculator.displayText.alignment =
            TextAlignmentOptions.Right;

        string[,] keys =
        {
            { "7", "8", "9", "/" },
            { "4", "5", "6", "*" },
            { "1", "2", "3", "-" },
            { "C", "0", ".", "+" },
            { "BACK", "=", "USE", "" }
        };

        for (int row = 0; row < 5; row++)
        {
            for (int col = 0; col < 4; col++)
            {
                string key = keys[row, col];

                if (string.IsNullOrEmpty(key))
                    continue;

                MaterialLossCalculatorKey clone =
                    Instantiate(
                        keyTemplate,
                        root
                    );

                clone.gameObject.name =
                    "CalculatorKey_" + key;

                clone.gameObject.SetActive(true);

                clone.calculator = calculator;
                clone.key = key;

                RectTransform rect =
                    clone.GetComponent<RectTransform>();

                if (rect == null)
                {
                    Debug.LogError(
                        "Key template needs RectTransform."
                    );
                    continue;
                }

                rect.anchorMin =
                    new Vector2(0.5f, 0.5f);

                rect.anchorMax =
                    new Vector2(0.5f, 0.5f);

                rect.pivot =
                    new Vector2(0.5f, 0.5f);

                rect.localScale = Vector3.one;
                rect.localRotation =
                    Quaternion.identity;

                rect.sizeDelta =
                    new Vector2(98, 48);

                rect.anchoredPosition =
                    new Vector2(
                        -159 + col * 106,
                        65 - row * 65
                    );

                string label = key;

                if (key == "/")
                    label = "÷";

                if (key == "*")
                    label = "×";

                if (key == "-")
                    label = "−";

                if (key == "BACK")
                    label = "DEL";

                if (key == "USE")
                    label = "USE";

                TMP_Text text =
                    clone.GetComponentInChildren<TMP_Text>(
                        true
                    );

                if (text != null)
                {
                    text.text = label;
                    text.fontSize =
                        key == "USE" ? 16 : 24;

                    text.alignment =
                        TextAlignmentOptions.Center;
                }

                Image keyImage =
                    clone.GetComponent<Image>();

                if (keyImage != null)
                {
                    keyImage.color =
                        key == "=" || key == "USE"
                        ? navy
                        : new Color32(
                            210, 225, 241, 255
                        );
                }

                if (text != null)
                {
                    text.color =
                        key == "=" || key == "USE"
                        ? Color.white
                        : dark;
                }

                BoxCollider box =
                    clone.GetComponent<BoxCollider>();

                if (box != null)
                {
                    box.center = Vector3.zero;
                    box.size =
                        new Vector3(98, 48, 2);
                }
            }
        }

        MakeText(
            root,
            "CalculatorHint",
            "Enter the values yourself. " +
            "Press =, then USE to transfer the result.",
            new Vector2(0, -244),
            new Vector2(435, 45),
            16,
            dark
        );

        training.calculatorPanel = panel;

        // Put the existing training panel on the
        // left and the calculator on the right.
        if (existingTrainingPanel != null)
        {
            existingTrainingPanel.anchoredPosition =
                new Vector2(-255, 0);
        }

        panel.SetActive(false);

        Debug.Log(
            "EZPZ Material Loss Calculator created."
        );
    }

    private TMP_Text MakeText(
        Transform parent,
        string objectName,
        string content,
        Vector2 position,
        Vector2 size,
        float fontSize,
        Color color)
    {
        GameObject obj = new GameObject(
            objectName,
            typeof(RectTransform),
            typeof(TextMeshProUGUI)
        );

        obj.transform.SetParent(parent, false);

        RectTransform rect =
            obj.GetComponent<RectTransform>();

        rect.anchorMin =
            new Vector2(0.5f, 0.5f);

        rect.anchorMax =
            new Vector2(0.5f, 0.5f);

        rect.sizeDelta = size;
        rect.anchoredPosition = position;

        TMP_Text text =
            obj.GetComponent<TMP_Text>();

        text.text = content;
        text.fontSize = fontSize;
        text.color = color;
        text.alignment =
            TextAlignmentOptions.Center;

        text.verticalAlignment =
            VerticalAlignmentOptions.Middle;

        text.enableWordWrapping = true;
        text.raycastTarget = false;

        return text;
    }
}
