
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class VRSelectionPanelUI : MonoBehaviour
{
    [Header("Selection Panel")]
    public GameObject selectionPanel;

    private void Awake()
    {
        if (selectionPanel == null)
        {
            Debug.LogError("Assign SelectionPanel in Inspector.");
            return;
        }

        BuildPanel();
    }

    private void BuildPanel()
    {
        // Remove only previously generated content.
        Transform previous = selectionPanel.transform.Find("GeneratedContent");
        if (previous != null)
        {
            Destroy(previous.gameObject);
        }

        GameObject content = new GameObject(
            "GeneratedContent", typeof(RectTransform));

        content.transform.SetParent(selectionPanel.transform, false);

        RectTransform root = content.GetComponent<RectTransform>();
        root.anchorMin = Vector2.zero;
        root.anchorMax = Vector2.one;
        root.offsetMin = Vector2.zero;
        root.offsetMax = Vector2.zero;

        AddText(content.transform, "Header",
            "VR CONTROLS TRAINING", 14,
            new Vector2(-245, 175), new Vector2(300, 30),
            new Color32(65, 215, 240, 255));

        AddText(content.transform, "Progress",
            "01 / 04", 14,
            new Vector2(340, 175), new Vector2(100, 30),
            Color.white);

        AddText(content.transform, "Title",
            "ACTIVITY 1 — SELECT", 36,
            new Vector2(0, 105), new Vector2(750, 55),
            Color.white);

        AddText(content.transform, "Description",
            "Use your right controller to select the button.",
            21, new Vector2(0, 48), new Vector2(750, 45),
            Color.white);

        GameObject task = new GameObject(
            "TaskBox", typeof(RectTransform), typeof(Image));

        task.transform.SetParent(content.transform, false);

        RectTransform taskRect = task.GetComponent<RectTransform>();
        taskRect.sizeDelta = new Vector2(780, 105);
        taskRect.anchoredPosition = new Vector2(0, -45);

        task.GetComponent<Image>().color =
            new Color32(34, 67, 105, 255);

        AddText(task.transform, "TaskText",
            "Point at START TRAINING and press the highlighted trigger.",
            19, Vector2.zero, new Vector2(740, 75),
            Color.white);

        GameObject buttonObject = new GameObject(
            "StartTrainingButton",
            typeof(RectTransform),
            typeof(Image),
            typeof(Button));

        buttonObject.transform.SetParent(content.transform, false);

        RectTransform buttonRect =
            buttonObject.GetComponent<RectTransform>();

        buttonRect.sizeDelta = new Vector2(780, 60);
        buttonRect.anchoredPosition = new Vector2(0, -155);

        Image buttonImage = buttonObject.GetComponent<Image>();
        buttonImage.color = new Color32(25, 116, 209, 255);

        Button button = buttonObject.GetComponent<Button>();
        button.targetGraphic = buttonImage;

        AddText(buttonObject.transform, "ButtonLabel",
            "START TRAINING", 22,
            Vector2.zero, new Vector2(750, 55), Color.white);
    }

    private void AddText(
        Transform parent, string objectName, string value,
        float fontSize, Vector2 position, Vector2 size,
        Color color)
    {
        GameObject obj = new GameObject(
            objectName, typeof(RectTransform),
            typeof(TextMeshProUGUI));

        obj.transform.SetParent(parent, false);

        RectTransform rect = obj.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;

        TextMeshProUGUI text = obj.GetComponent<TextMeshProUGUI>();
        text.text = value;
        text.fontSize = fontSize;
        text.color = color;
        text.alignment = TextAlignmentOptions.Center;
        text.enableWordWrapping = true;
        text.raycastTarget = false;
    }
}
