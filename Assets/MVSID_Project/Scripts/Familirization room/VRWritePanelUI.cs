
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class VRWritePanelUI : MonoBehaviour
{
    [Header("Write Panel")]
    public GameObject writePanel;

    [ContextMenu("Build Write Panel in Editor")]
    public void BuildWritePanel()
    {
        if (Application.isPlaying || writePanel == null)
            return;

        // Remove only previously generated content.
        Transform previous = writePanel.transform.Find("GeneratedWriteContent");

        if (previous != null)
            DestroyImmediate(previous.gameObject);

        GameObject content = new GameObject(
            "GeneratedWriteContent",
            typeof(RectTransform)
        );

        content.transform.SetParent(writePanel.transform, false);

        RectTransform root = content.GetComponent<RectTransform>();
        root.anchorMin = Vector2.zero;
        root.anchorMax = Vector2.one;
        root.offsetMin = Vector2.zero;
        root.offsetMax = Vector2.zero;

        AddText(content.transform, "Header",
            "VR CONTROLS TRAINING", 14,
            new Vector2(-245, 175),
            new Vector2(300, 30),
            new Color32(79, 224, 239, 255));

        AddText(content.transform, "Progress",
            "04 / 04", 14,
            new Vector2(340, 175),
            new Vector2(100, 30),
            Color.white);

        AddText(content.transform, "ActivityTitle",
            "ACTIVITY 4 — WRITE", 36,
            new Vector2(0, 105),
            new Vector2(750, 55),
            Color.white);

        AddText(content.transform, "Description",
            "Use your right controller to write on the whiteboard.",
            21, new Vector2(0, 45),
            new Vector2(750, 55),
            Color.white);

        GameObject taskBox = new GameObject(
            "TaskBox",
            typeof(RectTransform),
            typeof(Image)
        );

        taskBox.transform.SetParent(content.transform, false);

        RectTransform taskRect = taskBox.GetComponent<RectTransform>();
        taskRect.anchorMin = new Vector2(0.5f, 0.5f);
        taskRect.anchorMax = new Vector2(0.5f, 0.5f);
        taskRect.sizeDelta = new Vector2(780, 110);
        taskRect.anchoredPosition = new Vector2(0, -55);

        taskBox.GetComponent<Image>().color =
            new Color32(34, 67, 105, 255);

        AddText(taskBox.transform, "TaskInstruction",
            "Point at the whiteboard, hold the highlighted trigger, and move your controller to draw.",
            20, Vector2.zero,
            new Vector2(730, 90),
            Color.white);

        AddText(content.transform, "TriggerLabel",
            "RIGHT INDEX TRIGGER (R2)",
            19, new Vector2(0, -160),
            new Vector2(600, 45),
            new Color32(79, 224, 239, 255));

        Debug.Log("Write Panel created in Edit Mode.");
    }

    private void AddText(
        Transform parent,
        string objectName,
        string value,
        float fontSize,
        Vector2 position,
        Vector2 size,
        Color color)
    {
        GameObject obj = new GameObject(
            objectName,
            typeof(RectTransform),
            typeof(TextMeshProUGUI)
        );

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

