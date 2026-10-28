
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using TMPro;

public static class VRTutorialPanelGenerator
{
    [MenuItem("Tools/VR Tutorial/Create Panel")]
    public static void CreatePanel()
    {
        GameObject canvasObject = new GameObject(
            "VR_Tutorial_Canvas",
            typeof(RectTransform),
            typeof(Canvas),
            typeof(CanvasScaler),
            typeof(GraphicRaycaster)
        );

        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;

        RectTransform canvasRect =
            canvasObject.GetComponent<RectTransform>();

        canvasRect.sizeDelta = new Vector2(1000, 650);
        canvasRect.localScale = Vector3.one * 0.001f;

        Camera cam = Camera.main;

        if (cam != null)
        {
            canvasObject.transform.position =
                cam.transform.position +
                cam.transform.forward * 2f;

            canvasObject.transform.rotation =
                cam.transform.rotation;
        }
        else
        {
            canvasObject.transform.position =
                new Vector3(0, 1.5f, 2f);
        }

        CreateImage(canvasObject.transform,
            "Background",
            new Vector2(1000, 650),
            Vector2.zero,
            new Color(0.04f, 0.12f, 0.23f));

        CreateText(canvasObject.transform,
            "Title",
            "VR CONTROLS - PRACTISE THE BASIC SKILLS",
            new Vector2(0, 260),
            42);

        string[] activities =
        {
            "1. NAVIGATE\nUse the joystick to move",
            "2. SELECT\nPoint and select a button",
            "3. GRAB\nPick up a virtual object",
            "4. WRITE\nUse the marker on the board"
        };

        for (int i = 0; i < activities.Length; i++)
        {
            float x = (i % 2 == 0) ? -240 : 240;
            float y = (i < 2) ? 95 : -85;

            GameObject card = CreateImage(
                canvasObject.transform,
                "Activity_" + (i + 1),
                new Vector2(440, 150),
                new Vector2(x, y),
                new Color(0.10f, 0.29f, 0.48f)
            );

            CreateText(card.transform,
                "Instruction",
                activities[i],
                Vector2.zero,
                25);
        }

        GameObject buttonObject = CreateImage(
            canvasObject.transform,
            "Start_Button",
            new Vector2(350, 65),
            new Vector2(0, -260),
            new Color(0.12f, 0.46f, 0.78f)
        );

        Button button = buttonObject.AddComponent<Button>();

        CreateText(buttonObject.transform,
            "Button_Text",
            "START TRAINING",
            Vector2.zero,
            28);

        Selection.activeGameObject = canvasObject;
        EditorSceneManager.MarkSceneDirty(
            canvasObject.scene
        );

        Debug.Log("VR Tutorial Panel created successfully!");
    }

    static GameObject CreateImage(
        Transform parent,
        string name,
        Vector2 size,
        Vector2 position,
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

        obj.GetComponent<Image>().color = color;

        return obj;
    }

    static void CreateText(
        Transform parent,
        string name,
        string content,
        Vector2 position,
        float fontSize)
    {
        GameObject obj = new GameObject(
            name,
            typeof(RectTransform),
            typeof(TextMeshProUGUI)
        );

        obj.transform.SetParent(parent, false);

        RectTransform rect =
            obj.GetComponent<RectTransform>();

        rect.sizeDelta = new Vector2(900, 120);
        rect.anchoredPosition = position;

        TextMeshProUGUI text =
            obj.GetComponent<TextMeshProUGUI>();

        text.text = content;
        text.fontSize = fontSize;
        text.color = Color.white;
        text.alignment = TextAlignmentOptions.Center;
        text.raycastTarget = false;
    }
}
