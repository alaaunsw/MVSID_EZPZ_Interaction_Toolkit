
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class VRGrabPanelUI : MonoBehaviour
{
    [Header("Grab Panel")]
    public GameObject grabPanel;

#if UNITY_EDITOR
    [ContextMenu("Build Grab Panel in Editor")]
    private void BuildGrabPanel()
    {
        if (Application.isPlaying)
            return;

        if (grabPanel == null)
        {
            Debug.LogError("Assign GrabPanel first.");
            return;
        }

        RectTransform panel = grabPanel.GetComponent<RectTransform>();

        panel.anchorMin = panel.anchorMax = new Vector2(0.5f, 0.5f);
        panel.pivot = new Vector2(0.5f, 0.5f);
        panel.anchoredPosition3D = Vector3.zero;
        panel.sizeDelta = new Vector2(850, 420);
        panel.localScale = Vector3.one;

        Image background = grabPanel.GetComponent<Image>();

        if (background == null)
            background = grabPanel.AddComponent<Image>();

        background.color = new Color32(14, 30, 65, 255);
        background.raycastTarget = false;

        // Header
        AddText(
            "Header",
            "VR CONTROLS TRAINING",
            16,
            new Vector2(-245, 175),
            new Vector2(300, 30),
            new Color32(65, 215, 240, 255),
            TextAlignmentOptions.Left
        );

        // Progress
        AddText(
            "Progress",
            "03 / 04",
            16,
            new Vector2(340, 175),
            new Vector2(100, 30),
            Color.white
        );

        // Main title
        AddText(
            "ActivityTitle",
            "ACTIVITY 3 — GRAB",
            38,
            new Vector2(0, 95),
            new Vector2(780, 60),
            Color.white
        );

        // Description
        AddText(
            "Description",
            "Use your controller to pick up and move an object.",
            23,
            new Vector2(0, 32),
            new Vector2(780, 45),
            Color.white
        );

        // Task box
        GameObject taskBox = AddBox(
            "TaskBox",
            grabPanel.transform,
            new Vector2(0, -83),
            new Vector2(780, 135),
            new Color32(34, 67, 105, 255)
        );

        // Task heading
        AddText(
            "TaskHeading",
            "YOUR TASK",
            17,
            new Vector2(0, 38),
            new Vector2(720, 30),
            new Color32(65, 215, 240, 255),
            TextAlignmentOptions.Center,
            taskBox.transform
        );

        // Task instructions
        AddText(
            "TaskText",
            "Approach the highlighted object. Press and hold the grip button to grab it, then release to drop it.",
            20,
            new Vector2(0, -13),
            new Vector2(710, 65),
            Color.white,
            TextAlignmentOptions.Center,
            taskBox.transform
        );

        UnityEditor.SceneManagement.EditorSceneManager
            .MarkSceneDirty(gameObject.scene);

        Debug.Log("Grab Panel updated successfully.");
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
            obj = new GameObject(
                name,
                typeof(RectTransform),
                typeof(Image)
            );

            obj.transform.SetParent(parent, false);
        }

        RectTransform rect = obj.GetComponent<RectTransform>();

        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        rect.localScale = Vector3.one;

        Image image = obj.GetComponent<Image>();
        image.color = color;
        image.raycastTarget = false;

        return obj;
    }

    private void AddText(
        string name,
        string content,
        float fontSize,
        Vector2 position,
        Vector2 size,
        Color color,
        TextAlignmentOptions alignment = TextAlignmentOptions.Center,
        Transform parent = null)
    {
        if (parent == null)
            parent = grabPanel.transform;

        Transform existing = parent.Find(name);

        GameObject obj;

        if (existing != null)
            obj = existing.gameObject;
        else
        {
            obj = new GameObject(
                name,
                typeof(RectTransform),
                typeof(TextMeshProUGUI)
            );

            obj.transform.SetParent(parent, false);
        }

        RectTransform rect = obj.GetComponent<RectTransform>();

        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        rect.localScale = Vector3.one;

        TextMeshProUGUI text =
            obj.GetComponent<TextMeshProUGUI>();

        if (text == null)
            text = obj.AddComponent<TextMeshProUGUI>();

        text.text = content;
        text.fontSize = fontSize;
        text.color = color;
        text.alignment = alignment;
        text.enableWordWrapping = true;
        text.raycastTarget = false;
        text.enableAutoSizing = false;
        text.overflowMode = TextOverflowModes.Overflow;
    }
#endif
}
