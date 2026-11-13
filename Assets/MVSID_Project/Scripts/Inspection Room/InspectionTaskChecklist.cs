
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InspectionTaskChecklist : MonoBehaviour
{
    [Header("Generated HUD")]
    public Canvas hudCanvas;

    [Header("Build Options")]
    public bool buildOnStart = true;
    public bool showOnStart = false;

    [Header("Panel Position")]
    public Vector2 panelSize = new Vector2(340, 420);
    public Vector2 panelOffset = new Vector2(-18, -18);

    [Header("Training Tasks")]
    public string[] tasks =
    {
        "Inspect pipeline sections",
        "Read pipe information",
        "Identify corrosion",
        "Record inspection findings",
        "Review engineering assessment",
        "Receive feedback"
    };

    private bool[] completed;
    private TMP_Text[] taskTexts;
    private TMP_Text[] numberTexts;
    private Image[] rowImages;
    private GameObject[] tickObjects;

    private Image progressFill;
    private TMP_Text progressText;

    private readonly Color navy =
        new Color32(20, 55, 99, 255);

    private readonly Color blue =
        new Color32(36, 89, 155, 255);

    private readonly Color green =
        new Color32(36, 160, 82, 255);

    private readonly Color pale =
        new Color32(247, 250, 254, 255);

    private readonly Color current =
        new Color32(221, 236, 252, 255);

    private readonly Color done =
        new Color32(231, 248, 235, 255);

    private void Start()
    {
        if (hudCanvas == null && buildOnStart)
            BuildChecklistHUD();

        CacheReferences();
        ResetTasks();

        if (showOnStart)
            ShowHUD();
        else
            HideHUD();
    }

    [ContextMenu("Build Training Progress HUD")]
    public void BuildChecklistHUD()
    {
        if (transform.Find("InspectionTaskChecklistCanvas") != null)
        {
            Debug.LogWarning(
                "Delete the old checklist Canvas before rebuilding."
            );
            return;
        }

        GameObject root = new GameObject(
            "InspectionTaskChecklistCanvas",
            typeof(RectTransform),
            typeof(Canvas),
            typeof(CanvasScaler)
        );

        root.transform.SetParent(transform, false);

        hudCanvas = root.GetComponent<Canvas>();
        hudCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        hudCanvas.sortingOrder = 200;

        CanvasScaler scaler = root.GetComponent<CanvasScaler>();
        scaler.uiScaleMode =
            CanvasScaler.ScaleMode.ConstantPixelSize;

        scaler.scaleFactor = 1f;

        RectTransform canvasRect =
            root.GetComponent<RectTransform>();

        canvasRect.anchorMin = Vector2.zero;
        canvasRect.anchorMax = Vector2.one;
        canvasRect.offsetMin = Vector2.zero;
        canvasRect.offsetMax = Vector2.zero;

        // Main panel
        Image panel = MakeImage(
            root.transform,
            "ChecklistPanel",
            pale,
            panelSize,
            panelOffset
        );

        RectTransform panelRect = panel.rectTransform;
        panelRect.anchorMin = Vector2.one;
        panelRect.anchorMax = Vector2.one;
        panelRect.pivot = Vector2.one;

        Outline outline =
            panel.gameObject.AddComponent<Outline>();

        outline.effectColor = blue;
        outline.effectDistance = new Vector2(2, -2);

        // Navy header
        MakeImage(
            panel.transform,
            "HeaderBar",
            navy,
            new Vector2(panelSize.x, 48),
            new Vector2(0, -24)
        );

        MakeText(
            panel.transform,
            "HeaderText",
            "TRAINING PROGRESS",
            19,
            Color.white,
            new Vector2(panelSize.x - 24, 34),
            new Vector2(0, -24),
            true,
            TextAlignmentOptions.Center
        );

        // Progress bar background
        Image progressBackground = MakeImage(
            panel.transform,
            "ProgressBackground",
            new Color32(198, 212, 229, 255),
            new Vector2(panelSize.x - 32, 12),
            new Vector2(0, -70)
        );

        progressFill = MakeImage(
            progressBackground.transform,
            "ProgressFill",
            green,
            new Vector2(0, 12),
            Vector2.zero
        );

        RectTransform fillRect =
            progressFill.rectTransform;

        fillRect.anchorMin = new Vector2(0, 0.5f);
        fillRect.anchorMax = new Vector2(0, 0.5f);
        fillRect.pivot = new Vector2(0, 0.5f);

        progressText = MakeText(
            panel.transform,
            "ProgressText",
            "0 / 6 completed",
            14,
            navy,
            new Vector2(panelSize.x - 30, 24),
            new Vector2(0, -92),
            true,
            TextAlignmentOptions.Center
        );

        // Task rows
        for (int i = 0; i < tasks.Length; i++)
        {
            float y = -132 - i * 46f;

            Image row = MakeImage(
                panel.transform,
                "TaskRow_" + i,
                pale,
                new Vector2(panelSize.x - 20, 42),
                new Vector2(0, y)
            );

            // Task number
            MakeText(
                row.transform,
                "Number",
                (i + 1).ToString(),
                17,
                blue,
                new Vector2(28, 30),
                new Vector2(-panelSize.x / 2 + 38, 0),
                true,
                TextAlignmentOptions.Center
            );

            // Green tick drawn with two Image strokes.
            GameObject tick = new GameObject(
                "GreenTick",
                typeof(RectTransform)
            );

            tick.transform.SetParent(row.transform, false);

            RectTransform tickRect =
                tick.GetComponent<RectTransform>();

            tickRect.sizeDelta = new Vector2(30, 30);

            tickRect.anchoredPosition =
                new Vector2(-panelSize.x / 2 + 38, 0);

            Image shortStroke = MakeImage(
                tick.transform,
                "ShortStroke",
                green,
                new Vector2(5, 11),
                new Vector2(-6, -3)
            );

            shortStroke.rectTransform.localRotation =
                Quaternion.Euler(0, 0, 45);

            Image longStroke = MakeImage(
                tick.transform,
                "LongStroke",
                green,
                new Vector2(5, 20),
                new Vector2(3, 1)
            );

            longStroke.rectTransform.localRotation =
                Quaternion.Euler(0, 0, -45);

            MakeText(
                row.transform,
                "TaskText",
                tasks[i],
                16,
                navy,
                new Vector2(panelSize.x - 85, 34),
                new Vector2(20, 0),
                false,
                TextAlignmentOptions.Left
            );
        }

        CacheReferences();
        ResetTasks();
        hudCanvas.gameObject.SetActive(false);

        Debug.Log("Training Progress HUD created.");
    }

    private Image MakeImage(
        Transform parent,
        string name,
        Color color,
        Vector2 size,
        Vector2 position)
    {
        GameObject obj = new GameObject(
            name,
            typeof(RectTransform),
            typeof(Image)
        );

        obj.transform.SetParent(parent, false);

        RectTransform rect =
            obj.GetComponent<RectTransform>();

        rect.anchorMin = new Vector2(0.5f, 1f);
        rect.anchorMax = new Vector2(0.5f, 1f);
        rect.pivot = new Vector2(0.5f, 0.5f);

        rect.sizeDelta = size;
        rect.anchoredPosition = position;

        Image image = obj.GetComponent<Image>();
        image.color = color;
        image.raycastTarget = false;

        return image;
    }

    private TMP_Text MakeText(
        Transform parent,
        string name,
        string content,
        float size,
        Color color,
        Vector2 dimensions,
        Vector2 position,
        bool bold,
        TextAlignmentOptions alignment)
    {
        GameObject obj = new GameObject(
            name,
            typeof(RectTransform),
            typeof(TextMeshProUGUI)
        );

        obj.transform.SetParent(parent, false);

        RectTransform rect =
            obj.GetComponent<RectTransform>();

        rect.anchorMin = new Vector2(0.5f, 1f);
        rect.anchorMax = new Vector2(0.5f, 1f);
        rect.sizeDelta = dimensions;
        rect.anchoredPosition = position;

        TMP_Text text = obj.GetComponent<TMP_Text>();

        text.text = content;
        text.fontSize = size;
        text.fontStyle =
            bold ? FontStyles.Bold : FontStyles.Normal;

        text.color = color;
        text.alignment = alignment;
        text.verticalAlignment =
            VerticalAlignmentOptions.Middle;

        text.enableAutoSizing = true;
        text.fontSizeMin = 12;
        text.fontSizeMax = size;
        text.raycastTarget = false;

        return text;
    }

    private void CacheReferences()
    {
        if (hudCanvas == null)
            return;

        Transform panel =
            hudCanvas.transform.Find("ChecklistPanel");

        if (panel == null)
            return;

        Transform progress =
            panel.Find("ProgressBackground/ProgressFill");

        progressFill = progress != null
            ? progress.GetComponent<Image>()
            : null;

        Transform progressLabel =
            panel.Find("ProgressText");

        progressText = progressLabel != null
            ? progressLabel.GetComponent<TMP_Text>()
            : null;

        int count = tasks.Length;

        taskTexts = new TMP_Text[count];
        numberTexts = new TMP_Text[count];
        rowImages = new Image[count];
        tickObjects = new GameObject[count];

        for (int i = 0; i < count; i++)
        {
            Transform row = panel.Find("TaskRow_" + i);

            if (row == null)
                continue;

            rowImages[i] = row.GetComponent<Image>();

            Transform task = row.Find("TaskText");
            Transform number = row.Find("Number");
            Transform tick = row.Find("GreenTick");

            if (task != null)
                taskTexts[i] = task.GetComponent<TMP_Text>();

            if (number != null)
                numberTexts[i] = number.GetComponent<TMP_Text>();

            if (tick != null)
                tickObjects[i] = tick.gameObject;
        }
    }

    public void ResetTasks()
    {
        completed = new bool[tasks.Length];
        RefreshUI();
    }

    public void CompleteTaskByIndex(int index)
    {
        if (completed == null ||
            index < 0 ||
            index >= completed.Length)
            return;

        completed[index] = true;
        RefreshUI();

        Debug.Log("Task completed: " + tasks[index]);
    }

    public void CompleteCurrentTask()
    {
        if (completed == null)
            return;

        for (int i = 0; i < completed.Length; i++)
        {
            if (!completed[i])
            {
                CompleteTaskByIndex(i);
                return;
            }
        }
    }

    private void RefreshUI()
    {
        if (completed == null || rowImages == null)
            return;

        int doneCount = 0;
        int currentIndex = -1;

        for (int i = 0; i < completed.Length; i++)
        {
            if (completed[i])
                doneCount++;
            else if (currentIndex == -1)
                currentIndex = i;
        }

        for (int i = 0; i < completed.Length; i++)
        {
            if (rowImages[i] == null)
                continue;

            bool isDone = completed[i];
            bool isCurrent = i == currentIndex;

            rowImages[i].color = isDone
                ? done
                : isCurrent
                    ? current
                    : pale;

            if (tickObjects[i] != null)
                tickObjects[i].SetActive(isDone);

            if (numberTexts[i] != null)
                numberTexts[i].gameObject.SetActive(!isDone);

            if (taskTexts[i] != null)
            {
                taskTexts[i].color =
                    isDone ? green :
                    isCurrent ? navy :
                    new Color32(105, 119, 138, 255);

                taskTexts[i].fontStyle = isCurrent
                    ? FontStyles.Bold
                    : FontStyles.Normal;
            }
        }

        float percentage = completed.Length > 0
            ? (float)doneCount / completed.Length
            : 0f;

        if (progressFill != null)
        {
            progressFill.rectTransform.sizeDelta =
                new Vector2(
                    (panelSize.x - 32) * percentage,
                    12
                );
        }

        if (progressText != null)
        {
            progressText.text =
                doneCount + " / " + completed.Length +
                " completed  (" +
                Mathf.RoundToInt(percentage * 100) + "%)";
        }
    }

    public void ShowHUD()
    {
        if (hudCanvas != null)
            hudCanvas.gameObject.SetActive(true);
    }

    public void HideHUD()
    {
        if (hudCanvas != null)
            hudCanvas.gameObject.SetActive(false);
    }

    // EZPZ / Unity event functions
    public void CompleteTask0() { CompleteTaskByIndex(0); }
    public void CompleteTask1() { CompleteTaskByIndex(1); }
    public void CompleteTask2() { CompleteTaskByIndex(2); }
    public void CompleteTask3() { CompleteTaskByIndex(3); }
    public void CompleteTask4() { CompleteTaskByIndex(4); }
    public void CompleteTask5() { CompleteTaskByIndex(5); }

    [ContextMenu("TEST Complete Next Task")]
    private void TestCompleteNextTask()
    {
        if (completed == null)
        {
            CacheReferences();
            ResetTasks();
        }

        CompleteCurrentTask();
    }
}
