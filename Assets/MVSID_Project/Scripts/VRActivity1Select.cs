
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

public class VRActivity1Select : MonoBehaviour
{
    [Header("Existing UI")]
    public GameObject welcomePanel;
    public GameObject welcomeContent;
    public GameObject navigationPanel;

    [Header("Activity 1")]
    public float panelWidth = 850f;
    public float panelHeight = 420f;

    [Header("Button")]
    public UnityEvent onStartTraining;

    private GameObject activityPanel;

    private readonly Color navy = new Color32(14, 30, 65, 255);
    private readonly Color instructionBlue = new Color32(34, 67, 105, 255);
    private readonly Color buttonBlue = new Color32(25, 116, 209, 255);
    private readonly Color cyan = new Color32(65, 215, 240, 255);

    void Awake()
    {
        BuildPanel();
        activityPanel.SetActive(false);
    }

    public void ShowActivity1()
    {
        if (welcomePanel != null)
            welcomePanel.SetActive(false);

        if (welcomeContent != null)
            welcomeContent.SetActive(false);

        if (navigationPanel != null)
            navigationPanel.SetActive(false);

        activityPanel.SetActive(true);
    }

    public void StartTraining()
    {
        activityPanel.SetActive(false);

        if (navigationPanel != null)
            navigationPanel.SetActive(true);

        onStartTraining?.Invoke();
    }

    void BuildPanel()
    {
        Transform old = transform.Find("Generated_Activity1");
        if (old != null)
        {
            activityPanel = old.gameObject;
            return;
        }

        activityPanel = new GameObject(
            "Generated_Activity1",
            typeof(RectTransform),
            typeof(Image)
        );

        activityPanel.transform.SetParent(transform, false);

        RectTransform root = activityPanel.GetComponent<RectTransform>();
        root.anchorMin = root.anchorMax = new Vector2(.5f, .5f);
        root.pivot = new Vector2(.5f, .5f);
        root.sizeDelta = new Vector2(panelWidth, panelHeight);
        root.anchoredPosition = Vector2.zero;

        activityPanel.GetComponent<Image>().color = navy;

        CreateText("TopLeft", "VR CONTROLS TRAINING",
            13, cyan, new Vector2(20, -22),
            new Vector2(350, 25), TextAlignmentOptions.Left);

        CreateText("TopRight", "01 / 04",
            13, Color.white, new Vector2(-20, -22),
            new Vector2(100, 25), TextAlignmentOptions.Right,
            new Vector2(1, 1));

        CreateText("Title", "ACTIVITY 1 — SELECT",
            34, Color.white, new Vector2(0, -95),
            new Vector2(750, 50), TextAlignmentOptions.Center,
            new Vector2(.5f, 1));

        CreateText("Subtitle",
            "Use your right controller to select the button.",
            20, Color.white, new Vector2(0, -150),
            new Vector2(760, 40), TextAlignmentOptions.Center,
            new Vector2(.5f, 1));

        GameObject task = CreateBox(
            "InstructionBox",
            instructionBlue,
            new Vector2(0, -245),
            new Vector2(800, 115)
        );

        CreateText("PointerIcon", "⌖",
            30, cyan, new Vector2(0, -208),
            new Vector2(100, 40), TextAlignmentOptions.Center,
            new Vector2(.5f, 1));

        CreateText("TaskInstruction",
            "Point at START TRAINING and press the highlighted trigger.",
            19, Color.white, new Vector2(0, -270),
            new Vector2(740, 45), TextAlignmentOptions.Center,
            new Vector2(.5f, 1));

        GameObject buttonObj = CreateBox(
            "StartTrainingButton",
            buttonBlue,
            new Vector2(0, -373),
            new Vector2(800, 65)
        );

        Button button = buttonObj.AddComponent<Button>();
        button.targetGraphic = buttonObj.GetComponent<Image>();
        button.onClick.AddListener(StartTraining);

        CreateText("ButtonLabel", "START TRAINING",
            21, Color.white, Vector2.zero,
            new Vector2(760, 55), TextAlignmentOptions.Center,
            new Vector2(.5f, .5f), buttonObj.transform);
    }

    GameObject CreateBox(
        string name, Color color, Vector2 position, Vector2 size)
    {
        GameObject obj = new GameObject(
            name, typeof(RectTransform), typeof(Image));

        obj.transform.SetParent(activityPanel.transform, false);

        RectTransform rect = obj.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = new Vector2(.5f, 1);
        rect.pivot = new Vector2(.5f, .5f);
        rect.sizeDelta = size;
        rect.anchoredPosition = position;

        obj.GetComponent<Image>().color = color;
        return obj;
    }

    void CreateText(
        string name, string content, float size, Color color,
        Vector2 position, Vector2 dimensions,
        TextAlignmentOptions alignment,
        Vector2? anchor = null, Transform parent = null)
    {
        GameObject obj = new GameObject(
            name, typeof(RectTransform), typeof(TextMeshProUGUI));

        obj.transform.SetParent(
            parent == null ? activityPanel.transform : parent, false);

        RectTransform rect = obj.GetComponent<RectTransform>();

        Vector2 a = anchor ?? new Vector2(.5f, 1);
        rect.anchorMin = rect.anchorMax = a;
        rect.pivot = a;
        rect.sizeDelta = dimensions;
        rect.anchoredPosition = position;

        TextMeshProUGUI text = obj.GetComponent<TextMeshProUGUI>();
        text.text = content;
        text.fontSize = size;
        text.color = color;
        text.alignment = alignment;
        text.enableWordWrapping = true;
        text.raycastTarget = false;
    }
}
