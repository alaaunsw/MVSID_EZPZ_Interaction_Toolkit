
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

public static class NavigationPanelGenerator
{
    [MenuItem("Tools/VR Tutorial/Create Navigation Panel")]
    public static void CreatePanel()
    {
        GameObject canvas = GameObject.Find("VR_Tutorial_Canvas");

        if (canvas == null)
        {
            Debug.LogError("VR_Tutorial_Canvas not found!");
            return;
        }

        Transform existing = canvas.transform.Find("NavigationPanel");

        if (existing != null)
        {
            Debug.Log("NavigationPanel already exists.");
            Selection.activeGameObject = existing.gameObject;
            return;
        }

        // Main panel
        GameObject panel = new GameObject(
            "NavigationPanel",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image)
        );

        panel.transform.SetParent(canvas.transform, false);

        RectTransform rect = panel.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(850, 420);
        rect.anchoredPosition = Vector2.zero;

        Image bg = panel.GetComponent<Image>();
        bg.color = new Color32(12, 27, 55, 255);
        bg.raycastTarget = false;

        // Header
        CreateText(panel.transform, "Header",
            "VR CONTROLS TRAINING",
            new Vector2(-225, 170),
            new Vector2(350, 45),
            24, new Color32(110, 220, 255, 255));

        CreateText(panel.transform, "Progress",
            "01 / 04",
            new Vector2(330, 170),
            new Vector2(130, 45),
            24, Color.white);

        // Activity heading
        CreateText(panel.transform, "ActivityTitle",
            "NAVIGATION",
            new Vector2(0, 90),
            new Vector2(750, 65),
            46, Color.white);

        // Main instruction
        CreateText(panel.transform, "MainInstruction",
            "Use the left joystick to move",
            new Vector2(0, 20),
            new Vector2(750, 55),
            32, Color.white);

        // Task box
        GameObject taskBox = new GameObject(
            "TaskBox",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image)
        );

        taskBox.transform.SetParent(panel.transform, false);

        RectTransform taskRect = taskBox.GetComponent<RectTransform>();
        taskRect.sizeDelta = new Vector2(760, 115);
        taskRect.anchoredPosition = new Vector2(0, -105);

        Image taskBg = taskBox.GetComponent<Image>();
        taskBg.color = new Color32(28, 51, 84, 255);
        taskBg.raycastTarget = false;

        CreateText(taskBox.transform, "TaskLabel",
            "YOUR TASK",
            new Vector2(0, 30),
            new Vector2(700, 35),
            23, new Color32(110, 220, 255, 255));

        CreateText(taskBox.transform, "TaskInstruction",
            "Move the joystick forward, backward, left and right.",
            new Vector2(0, -20),
            new Vector2(700, 55),
            24, Color.white);

        // Footer
        CreateText(panel.transform, "Status",
            "STEP 1 OF 4  |  PRACTISE MOVEMENT",
            new Vector2(0, -180),
            new Vector2(750, 40),
            21, new Color32(170, 195, 225, 255));

        panel.SetActive(false);

        Undo.RegisterCreatedObjectUndo(panel, "Create Navigation Panel");
        Selection.activeGameObject = panel;

        Debug.Log("Navigation panel created successfully!");
    }

    private static void CreateText(
        Transform parent,
        string name,
        string content,
        Vector2 position,
        Vector2 size,
        float fontSize,
        Color color)
    {
        GameObject obj = new GameObject(
            name,
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(TextMeshProUGUI)
        );

        obj.transform.SetParent(parent, false);

        RectTransform rect = obj.GetComponent<RectTransform>();
        rect.sizeDelta = size;
        rect.anchoredPosition = position;

        TextMeshProUGUI text = obj.GetComponent<TextMeshProUGUI>();

        text.text = content;
        text.fontSize = fontSize;
        text.color = color;
        text.alignment = TextAlignmentOptions.Center;
        text.enableAutoSizing = false;
        text.raycastTarget = false;
    }
}
