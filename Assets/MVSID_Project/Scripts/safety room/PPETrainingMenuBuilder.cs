
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PPETrainingMenuBuilder : MonoBehaviour
{
    [Header("Mannequin Controller")]
    public PPEMannequinClothingController clothingController;

    [Header("Panel")]
    public GameObject trainingPanel;

    private TextMeshProUGUI feedbackText;

    private readonly Color outerBlue =
        new Color32(110, 143, 161, 245);

    private readonly Color navy =
        new Color32(24, 62, 87, 255);

    private readonly Color buttonBlue =
        new Color32(38, 103, 143, 255);

    [ContextMenu("Build PPE Training Menu")]
    public void BuildMenu()
    {
        Transform oldPanel =
            transform.Find("PPETrainingPanel");

        if (oldPanel != null)
        {
            if (Application.isPlaying)
                Destroy(oldPanel.gameObject);
            else
                DestroyImmediate(oldPanel.gameObject);
        }

        GameObject panel = CreatePanel(
            "PPETrainingPanel",
            transform,
            new Vector2(850, 420),
            outerBlue
        );

        trainingPanel = panel;

        GameObject inner = CreatePanel(
            "InnerPanel",
            panel.transform,
            new Vector2(795, 365),
            navy
        );

        CreateText(
            inner.transform,
            "Title",
            "PPE EQUIPMENT TRAINING",
            39,
            new Vector2(0, 135),
            new Vector2(760, 65)
        );

        CreateText(
            inner.transform,
            "Instructions",
            "Select equipment to dress the mannequin.",
            25,
            new Vector2(0, 65),
            new Vector2(740, 55)
        );

        CreateButton(
            inner.transform,
            "HelmetButton",
            "SAFETY HELMET",
            new Vector2(0, -20),
            new Vector2(390, 75)
        );

        feedbackText = CreateText(
            inner.transform,
            "FeedbackText",
            "Select the safety helmet to begin.",
            22,
            new Vector2(0, -125),
            new Vector2(750, 85)
        );

        panel.SetActive(false);
    }

    private void Awake()
    {
        FindReferences();

        if (trainingPanel != null)
            trainingPanel.SetActive(false);

        Button helmetButton = null;

        if (trainingPanel != null)
        {
            Transform found =
                trainingPanel.transform.Find(
                    "InnerPanel/HelmetButton"
                );

            if (found != null)
                helmetButton = found.GetComponent<Button>();
        }

        if (helmetButton != null)
        {
            helmetButton.onClick.AddListener(SelectHelmet);
        }
    }

    private void FindReferences()
    {
        if (trainingPanel == null)
        {
            Transform found =
                transform.Find("PPETrainingPanel");

            if (found != null)
                trainingPanel = found.gameObject;
        }

        if (trainingPanel != null)
        {
            feedbackText =
                trainingPanel.GetComponentInChildren<
                    TextMeshProUGUI>(true);

            Transform found =
                trainingPanel.transform.Find(
                    "InnerPanel/FeedbackText"
                );

            if (found != null)
                feedbackText =
                    found.GetComponent<TextMeshProUGUI>();
        }
    }

    public void ShowMenu()
    {
        FindReferences();

        if (trainingPanel == null)
        {
            Debug.LogWarning(
                "Build the PPE Training Menu first."
            );
            return;
        }

        if (feedbackText != null)
        {
            feedbackText.color = Color.white;
            feedbackText.text =
                "Select the safety helmet to begin.";
        }

        trainingPanel.SetActive(true);
    }

    public void HideMenu()
    {
        if (trainingPanel != null)
            trainingPanel.SetActive(false);
    }

    public void SelectHelmet()
    {
        if (clothingController == null)
        {
            Debug.LogWarning(
                "Mannequin Clothing Controller is missing."
            );
            return;
        }

        clothingController.SelectHelmet();

        if (clothingController.helmetOnMannequin == null)
            return;

        if (feedbackText != null)
        {
            feedbackText.color =
                new Color32(140, 255, 170, 255);

            feedbackText.text =
                "✓ CORRECT!\n" +
                "The safety helmet helps protect the head " +
                "against specified impact and falling-object hazards.";
        }
    }

    private GameObject CreatePanel(
        string objectName,
        Transform parent,
        Vector2 size,
        Color colour)
    {
        GameObject obj = new GameObject(
            objectName,
            typeof(RectTransform),
            typeof(Image)
        );

        obj.transform.SetParent(parent, false);

        RectTransform rect =
            obj.GetComponent<RectTransform>();

        rect.anchorMin =
            new Vector2(0.5f, 0.5f);

        rect.anchorMax =
            new Vector2(0.5f, 0.5f);

        rect.pivot =
            new Vector2(0.5f, 0.5f);

        rect.sizeDelta = size;
        rect.anchoredPosition = Vector2.zero;
        rect.localScale = Vector3.one;

        Image image = obj.GetComponent<Image>();
        image.color = colour;

        return obj;
    }

    private TextMeshProUGUI CreateText(
        Transform parent,
        string objectName,
        string content,
        float fontSize,
        Vector2 position,
        Vector2 size)
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
        rect.localScale = Vector3.one;

        TextMeshProUGUI text =
            obj.GetComponent<TextMeshProUGUI>();

        text.text = content;
        text.fontSize = fontSize;
        text.color = Color.white;

        text.alignment =
            TextAlignmentOptions.Center;

        text.enableWordWrapping = true;
        text.raycastTarget = false;

        return text;
    }

    private void CreateButton(
        Transform parent,
        string objectName,
        string label,
        Vector2 position,
        Vector2 size)
    {
        GameObject obj = CreatePanel(
            objectName,
            parent,
            size,
            buttonBlue
        );

        RectTransform rect =
            obj.GetComponent<RectTransform>();

        rect.anchoredPosition = position;

        Button button = obj.AddComponent<Button>();

        button.targetGraphic =
            obj.GetComponent<Image>();

        BoxCollider collider =
            obj.AddComponent<BoxCollider>();

        collider.size =
            new Vector3(size.x, size.y, 10);

        // EZPZ interaction component.
        obj.AddComponent<InteractableGeneral>();

        CreateText(
            obj.transform,
            "ButtonText",
            label,
            28,
            Vector2.zero,
            new Vector2(size.x - 20, size.y)
        );
    }
}
