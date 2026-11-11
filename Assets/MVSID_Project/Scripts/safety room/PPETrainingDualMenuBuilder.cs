
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PPETrainingDualMenuBuilder : MonoBehaviour
{
    [System.Serializable]
    public class PPEItem
    {
        public string equipmentName;
        public Sprite icon;
        public GameObject mannequinModel;

        [TextArea(2, 4)]
        public string explanation;
    }

    [Header("Two Separate Canvases")]
    public Canvas informationCanvas;
    public Canvas selectionCanvas;

    [Header("PPE Equipment")]
    public PPEItem[] equipment = new PPEItem[5];

    [Header("Feedback Sound")]
    public AudioSource successSound;

    [Header("Starting Behaviour")]
    public bool hideMenusAtStart = true;

    private GameObject infoPanel;
    private GameObject selectionPanel;

    private TextMeshProUGUI stepText;
    private TextMeshProUGUI explanationText;
    private TextMeshProUGUI progressText;

    private Image progressFill;
    private Image[] selectionHighlights;

    private readonly HashSet<int> completed =
        new HashSet<int>();

    private readonly Color dark =
        new Color32(38, 43, 49, 245);

    private readonly Color card =
        new Color32(55, 62, 70, 245);

    private readonly Color tile =
        new Color32(76, 83, 92, 255);

    private readonly Color selected =
        new Color32(47, 112, 173, 255);

    private readonly Color green =
        new Color32(127, 220, 145, 255);

    private void Reset()
    {
        equipment = new PPEItem[]
        {
            MakeItem("SAFETY HELMET",
                "Helmets help protect against specified head impact and falling-object hazards."),
            MakeItem("SAFETY GLASSES",
                "Safety glasses help protect the eyes against specified particles and impact hazards."),
            MakeItem("HIGH-VIS VEST",
                "High-visibility clothing makes workers easier to see in appropriate conditions."),
            MakeItem("SAFETY GLOVES",
                "Task-appropriate gloves help protect hands against specified hazards."),
            MakeItem("SAFETY BOOTS",
                "Appropriate safety footwear helps protect feet against specified workplace hazards.")
        };
    }

    private PPEItem MakeItem(string name, string message)
    {
        return new PPEItem
        {
            equipmentName = name,
            explanation = message
        };
    }

    private void Start()
    {
        FindPanels();
        ConnectButtons();

        if (hideMenusAtStart)
            HideMenus();

        UpdateProgress();
    }

    // ==========================================
    // BUILD BOTH MENUS IN THE EDITOR
    // ==========================================

    [ContextMenu("Build Both PPE Menus")]
    public void BuildBothMenus()
    {
        if (informationCanvas == null ||
            selectionCanvas == null)
        {
            Debug.LogError(
                "Assign both Canvas references first.");
            return;
        }

        BuildInformationPanel();
        BuildSelectionPanel();

        FindPanels();

        if (Application.isPlaying)
            ConnectButtons();

        UpdateProgress();

        Debug.Log("Both PPE menus created.");
    }

    private void BuildInformationPanel()
    {
        RemoveOld(informationCanvas.transform,
            "TrainingInfoPanel");

        infoPanel = CreateBox(
            "TrainingInfoPanel",
            informationCanvas.transform,
            500, 600, 0, 0, dark
        );

        CreateText(
            "Heading", infoPanel.transform,
            "PPE TRAINING", 34,
            440, 48, 0, 245,
            Color.white
        );

        CreateText(
            "ObjectiveHeading", infoPanel.transform,
            "TRAINING OBJECTIVE", 23,
            440, 40, 0, 180,
            green
        );

        CreateText(
            "Objective", infoPanel.transform,
            "Understand how personal protective " +
            "equipment helps protect workers.",
            21, 440, 105, 0, 105,
            Color.white
        );

        CreateBox(
            "Divider",
            infoPanel.transform,
            430, 3, 0, 30, card
        );

        CreateText(
            "CurrentStepHeading", infoPanel.transform,
            "CURRENT STEP", 23,
            440, 40, 0, -10,
            green
        );

        stepText = CreateText(
            "CurrentStep", infoPanel.transform,
            "Select PPE to begin.",
            22, 440, 75, 0, -70,
            Color.white
        );

        explanationText = CreateText(
            "Explanation", infoPanel.transform,
            "Choose equipment from the selection bar.",
            19, 440, 110, 0, -165,
            Color.white
        );

        CreateBox(
            "ProgressBackground",
            infoPanel.transform,
            410, 12, 0, -245, card
        );

        GameObject fill = CreateBox(
            "ProgressFill",
            infoPanel.transform,
            410, 12, 0, -245, green
        );

        RectTransform fillRect =
            fill.GetComponent<RectTransform>();
fillRect.anchorMin = new Vector2(0.5f, 0.5f);
fillRect.anchorMax = new Vector2(0.5f, 0.5f);
        fillRect.pivot = new Vector2(0, 0.5f);
        fillRect.anchoredPosition = new Vector2(-205, -245);

        progressFill = fill.GetComponent<Image>();
        progressFill.fillAmount = 0;
        progressFill.type = Image.Type.Filled;
        progressFill.fillMethod = Image.FillMethod.Horizontal;

        progressText = CreateText(
            "ProgressText", infoPanel.transform,
            "0 / 5 COMPLETED", 17,
            420, 30, 0, -273,
            green
        );
    }

    private void BuildSelectionPanel()
    {
        RemoveOld(selectionCanvas.transform,
            "PPESelectionBar");

        selectionPanel = CreateBox(
            "PPESelectionBar",
            selectionCanvas.transform,
            1000, 260, 0, 0, dark
        );

        CreateText(
            "SelectionTitle",
            selectionPanel.transform,
            "SELECT THE PPE TO WEAR",
            28, 920, 44, 0, 90,
            Color.white
        );

        selectionHighlights = new Image[equipment.Length];

        int count = equipment.Length;

        if (count == 0)
            return;

        // Fit all equipment tiles inside the selection bar.
        float spacing = 12;
        float tileWidth =
            (920f - spacing * (count - 1)) / count;

        tileWidth = Mathf.Min(tileWidth, 165);

        float totalWidth =
            tileWidth * count + spacing * (count - 1);

        float firstX =
            -totalWidth / 2f + tileWidth / 2f;

        for (int i = 0; i < count; i++)
        {
            float x = firstX +
                i * (tileWidth + spacing);

            CreateEquipmentButton(
                i, x, -25, tileWidth
            );
        }
    }

    private void CreateEquipmentButton(
        int index, float x, float y, float width)
    {
        PPEItem item = equipment[index];

        GameObject obj = CreateBox(
            "PPEButton_" + index,
            selectionPanel.transform,
            width, 150, x, y, tile
        );

        selectionHighlights[index] =
            obj.GetComponent<Image>();

        Button button = obj.AddComponent<Button>();
        button.targetGraphic = obj.GetComponent<Image>();

        // Unity UI desktop interaction
        int capturedIndex = index;
        button.onClick.AddListener(
            () => SelectEquipment(capturedIndex)
        );

        // EZPZ interaction
        BoxCollider collider =
            obj.AddComponent<BoxCollider>();

        collider.size =
            new Vector3(width, 150, 10);

        obj.AddComponent<InteractableGeneral>();

        // Icon
        GameObject iconObj = CreateBox(
            "EquipmentIcon", obj.transform,
            90, 85, 0, 20, Color.clear
        );

        Image iconImage = iconObj.GetComponent<Image>();
        iconImage.preserveAspect = true;
        iconImage.raycastTarget = false;

        if (item.icon != null)
        {
            iconImage.sprite = item.icon;
            iconImage.color = Color.white;
        }
        else
        {
            // Placeholder until a Sprite is assigned.
            iconImage.enabled = false;

            CreateText(
                "IconPlaceholder",
                obj.transform,
                item.equipmentName.Substring(0, 1),
                48, 90, 85, 0, 20,
                Color.white
            );
        }

        CreateText(
            "EquipmentLabel", obj.transform,
            item.equipmentName,
            16, width - 8, 38, 0, -54,
            Color.white
        );
    }

    // ==========================================
    // PPE SELECTION
    // ==========================================

    public void SelectEquipment(int index)
    {
        if (index < 0 || index >= equipment.Length)
            return;

        PPEItem item = equipment[index];

        if (item.mannequinModel == null)
        {
            Debug.LogWarning(
                item.equipmentName +
                " has no mannequin model assigned.");
            return;
        }

        item.mannequinModel.SetActive(true);

        completed.Add(index);

        if (successSound != null)
            successSound.Play();

        if (stepText != null)
            stepText.text =
    "CORRECT! " + item.equipmentName + " SELECTED";

        if (explanationText != null)
            explanationText.text = item.explanation;

        if (selectionHighlights != null)
        {
            for (int i = 0;
                 i < selectionHighlights.Length; i++)
            {
                if (selectionHighlights[i] != null)
                    selectionHighlights[i].color =
                        completed.Contains(i)
                        ? selected
                        : tile;
            }
        }

        UpdateProgress();

        Debug.Log(
            "PPE selected: " + item.equipmentName
        );
    }

    // Public methods for EZPZ event connections.
    public void SelectHelmet() => SelectEquipment(0);
    public void SelectGlasses() => SelectEquipment(1);
    public void SelectVest() => SelectEquipment(2);
    public void SelectGloves() => SelectEquipment(3);
    public void SelectBoots() => SelectEquipment(4);

    // ==========================================
    // PROGRESS
    // ==========================================

    private void UpdateProgress()
    {
        int total = equipment.Length;
        int done = completed.Count;

        if (progressText != null)
            progressText.text =
                done + " / " + total + " COMPLETED";

        if (progressFill != null)
            progressFill.fillAmount =
                total > 0 ? (float)done / total : 0;
    }

    public void ShowMenus()
    {
        FindPanels();

        if (infoPanel != null)
            infoPanel.SetActive(true);

        if (selectionPanel != null)
            selectionPanel.SetActive(true);
    }

    public void HideMenus()
    {
        FindPanels();

        if (infoPanel != null)
            infoPanel.SetActive(false);

        if (selectionPanel != null)
            selectionPanel.SetActive(false);
    }

    public void ResetTraining()
    {
        completed.Clear();

        foreach (PPEItem item in equipment)
        {
            if (item.mannequinModel != null)
                item.mannequinModel.SetActive(false);
        }

        if (selectionHighlights != null)
        {
            foreach (Image image in selectionHighlights)
                if (image != null)
                    image.color = tile;
        }

        if (stepText != null)
            stepText.text = "Select PPE to begin.";

        if (explanationText != null)
            explanationText.text =
                "Choose equipment from the selection bar.";

        UpdateProgress();
    }

    // ==========================================
    // HELPERS
    // ==========================================

    private void FindPanels()
    {
        if (informationCanvas != null)
        {
            Transform t = informationCanvas.transform.Find(
                "TrainingInfoPanel");

            infoPanel = t != null ? t.gameObject : null;

            if (t != null)
            {
                stepText = FindText(t, "CurrentStep");
                explanationText = FindText(t, "Explanation");
                progressText = FindText(t, "ProgressText");

                Transform fill = t.Find("ProgressFill");
                progressFill = fill != null
                    ? fill.GetComponent<Image>()
                    : null;
            }
        }

        if (selectionCanvas != null)
        {
            Transform t = selectionCanvas.transform.Find(
                "PPESelectionBar");

            selectionPanel = t != null ? t.gameObject : null;
        }
    }

    private TextMeshProUGUI FindText(
        Transform parent, string name)
    {
        Transform t = parent.Find(name);
        return t != null
            ? t.GetComponent<TextMeshProUGUI>()
            : null;
    }

    private void ConnectButtons()
    {
        if (selectionPanel == null)
            return;

        selectionHighlights = new Image[equipment.Length];

        for (int i = 0; i < equipment.Length; i++)
        {
            int capturedIndex = i;

            Transform t = selectionPanel.transform.Find(
                "PPEButton_" + i);

            if (t == null)
                continue;

            selectionHighlights[i] =
                t.GetComponent<Image>();

            Button button = t.GetComponent<Button>();

            if (button != null)
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(
                    () => SelectEquipment(capturedIndex)
                );
            }
        }
    }

    private void RemoveOld(
        Transform parent, string objectName)
    {
        Transform old = parent.Find(objectName);

        if (old == null)
            return;

        if (Application.isPlaying)
            Destroy(old.gameObject);
        else
            DestroyImmediate(old.gameObject);
    }

    private GameObject CreateBox(
        string name,
        Transform parent,
        float width,
        float height,
        float x,
        float y,
        Color colour)
    {
        GameObject obj = new GameObject(
            name,
            typeof(RectTransform),
            typeof(Image)
        );

        obj.transform.SetParent(parent, false);
        obj.layer = LayerMask.NameToLayer("UI");

        RectTransform rect =
            obj.GetComponent<RectTransform>();

        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(width, height);
        rect.anchoredPosition = new Vector2(x, y);
        rect.localScale = Vector3.one;
        rect.localRotation = Quaternion.identity;

        Image image = obj.GetComponent<Image>();
        image.color = colour;

        return obj;
    }

    private TextMeshProUGUI CreateText(
        string name,
        Transform parent,
        string content,
        float size,
        float width,
        float height,
        float x,
        float y,
        Color colour)
    {
        GameObject obj = new GameObject(
            name,
            typeof(RectTransform),
            typeof(TextMeshProUGUI)
        );

        obj.transform.SetParent(parent, false);

        RectTransform rect =
            obj.GetComponent<RectTransform>();

        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);

        rect.sizeDelta = new Vector2(width, height);
        rect.anchoredPosition = new Vector2(x, y);
        rect.localScale = Vector3.one;

        TextMeshProUGUI txt =
            obj.GetComponent<TextMeshProUGUI>();

        txt.text = content;
        txt.fontSize = size;
        txt.color = colour;
        txt.alignment = TextAlignmentOptions.Center;
        txt.enableWordWrapping = true;
        txt.raycastTarget = false;

        return txt;
    }
}
