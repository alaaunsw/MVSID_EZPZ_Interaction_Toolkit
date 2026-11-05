
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class VRSelectionPanelUI : MonoBehaviour
{
    [Header("Selection Panel")]
    public GameObject selectionPanel;

#if UNITY_EDITOR
    [ContextMenu("Build Selection Panel in Editor")]
    private void BuildSelectionPanel()
    {
        if (Application.isPlaying) return;

        if (selectionPanel == null)
        {
            Debug.LogError("Assign SelectionPanel first.");
            return;
        }

        RectTransform panel =
            selectionPanel.GetComponent<RectTransform>();

        panel.anchorMin = panel.anchorMax =
            new Vector2(0.5f, 0.5f);

        panel.pivot = new Vector2(0.5f, 0.5f);
        panel.anchoredPosition3D = Vector3.zero;
        panel.sizeDelta = new Vector2(850, 420);
        panel.localScale = Vector3.one;

        Image background = selectionPanel.GetComponent<Image>();

        if (background == null)
            background = selectionPanel.AddComponent<Image>();

        background.color = new Color32(14, 30, 65, 255);
        background.raycastTarget = false;

        AddText("Header", "VR CONTROLS TRAINING", 14,
            new Vector2(-245, 175), new Vector2(300, 30),
            new Color32(65, 215, 240, 255));

        AddText("Progress", "01 / 04", 14,
            new Vector2(340, 175), new Vector2(100, 30),
            Color.white);

        AddText("ActivityTitle", "ACTIVITY 1 — SELECT", 36,
            new Vector2(0, 105), new Vector2(750, 55),
            Color.white);

        AddText("Description",
            "Use your right controller to select the button.", 21,
            new Vector2(0, 48), new Vector2(750, 45),
            Color.white);

        GameObject taskBox = AddBox("TaskBox",
            selectionPanel.transform,
            new Vector2(0, -45),
            new Vector2(780, 105),
            new Color32(34, 67, 105, 255));

        AddText("TaskText",
            "Point at START TRAINING and press the highlighted trigger.",
            19, Vector2.zero, new Vector2(740, 75),
            Color.white, taskBox.transform);

        GameObject buttonObject = AddBox("StartTrainingButton",
            selectionPanel.transform,
            new Vector2(0, -155),
            new Vector2(780, 60),
            new Color32(25, 116, 209, 255));

        Button button = buttonObject.GetComponent<Button>();

        if (button == null)
            button = buttonObject.AddComponent<Button>();

        button.targetGraphic = buttonObject.GetComponent<Image>();

        AddText("ButtonLabel", "START TRAINING", 22,
            Vector2.zero, new Vector2(750, 55),
            Color.white, buttonObject.transform);

        UnityEditor.SceneManagement.EditorSceneManager
            .MarkSceneDirty(gameObject.scene);

        Debug.Log("Selection Panel created in the Hierarchy.");
    }

    private GameObject AddBox(
        string name,
        Transform parent,
        Vector2 position,
        Vector2 size,
        Color color)
    {
        Transform existing = parent.Find(name);

        GameObject obj;

        if (existing != null)
            obj = existing.gameObject;
        else
        {
            obj = new GameObject(name,
                typeof(RectTransform), typeof(Image));

            obj.transform.SetParent(parent, false);
        }

        RectTransform rect = obj.GetComponent<RectTransform>();

        rect.anchorMin = rect.anchorMax =
            new Vector2(0.5f, 0.5f);

        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        rect.localScale = Vector3.one;

        Image image = obj.GetComponent<Image>();
        image.color = color;

        return obj;
    }

    private void AddText(
        string name,
        string content,
        float fontSize,
        Vector2 position,
        Vector2 size,
        Color color,
        Transform parent = null)
    {
        if (parent == null)
            parent = selectionPanel.transform;

        Transform existing = parent.Find(name);

        GameObject obj;

        if (existing != null)
            obj = existing.gameObject;
        else
        {
            obj = new GameObject(name,
                typeof(RectTransform), typeof(TextMeshProUGUI));

            obj.transform.SetParent(parent, false);
        }

        RectTransform rect = obj.GetComponent<RectTransform>();

        rect.anchorMin = rect.anchorMax =
            new Vector2(0.5f, 0.5f);

        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        rect.localScale = Vector3.one;

        TextMeshProUGUI text =
            obj.GetComponent<TextMeshProUGUI>();

        text.text = content;
        text.fontSize = fontSize;
        text.color = color;
        text.alignment = TextAlignmentOptions.Center;
        text.enableWordWrapping = true;
        text.raycastTarget = false;
    }
#endif
}
