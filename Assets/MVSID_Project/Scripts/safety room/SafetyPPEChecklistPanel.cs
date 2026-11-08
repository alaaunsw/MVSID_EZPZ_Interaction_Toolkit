
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

public class SafetyPPEChecklistPanel : MonoBehaviour
{
    [Header("Panels")]
    public GameObject modeSelectionPanel;
    public GameObject ppeChecklistPanel;

    [Header("Selection")]
    public string selectedMode = "";

    [Header("Next Stage")]
    public GameObject nextPanel;

    private readonly bool[] confirmed = new bool[4];

    private readonly string[] itemNames =
    {
        "Safety helmet",
        "Safety glasses",
        "Protective gloves",
        "Safety boots"
    };

    private readonly string[] buttonNames =
    {
        "HelmetButton",
        "GlassesButton",
        "GlovesButton",
        "BootsButton"
    };

    private void Awake()
    {
        FindPanel();
        ConnectButtons();
    }

    [ContextMenu("Build PPE Checklist Panel")]
    public void BuildPPEChecklistPanel()
    {
        Transform existing = transform.Find("PPEChecklistPanel");

        if (existing != null)
        {
            ppeChecklistPanel = existing.gameObject;
            Debug.Log("PPE Checklist Panel already exists.");
            return;
        }

        GameObject panel = new GameObject(
            "PPEChecklistPanel",
            typeof(RectTransform),
            typeof(Image)
        );

        panel.transform.SetParent(transform, false);
        panel.layer = LayerMask.NameToLayer("UI");

        SetRect(panel, 850, 600, 0, 0);

        panel.GetComponent<Image>().color =
            new Color32(123, 156, 173, 255);

        CreateText(
            panel.transform,
            "PPETitle",
            "PPE SAFETY CHECKLIST",
            43, 0, 240, 790, 65
        );

        CreateText(
            panel.transform,
            "PPEInstructions",
            "Confirm your safety equipment before entering.",
            24, 0, 185, 790, 50
        );

        CreateButton(
            panel.transform,
            "HelmetButton",
            "[ ]  Safety helmet",
            0, 105, 690, 65
        );

        CreateButton(
            panel.transform,
            "GlassesButton",
            "[ ]  Safety glasses",
            0, 25, 690, 65
        );

        CreateButton(
            panel.transform,
            "GlovesButton",
            "[ ]  Protective gloves",
            0, -55, 690, 65
        );

        CreateButton(
            panel.transform,
            "BootsButton",
            "[ ]  Safety boots",
            0, -135, 690, 65
        );

        CreateButton(
            panel.transform,
            "ContinueButton",
            "CONTINUE",
            0, -220, 350, 65
        );

        CreateText(
            panel.transform,
            "PPEStatus",
            "Confirm all four PPE items.",
            19, 0, -270, 760, 40
        );

        ppeChecklistPanel = panel;

        ConnectButtons();
        ResetChecklist();

        panel.SetActive(false);

        Debug.Log("PPE Checklist Panel created.");
    }

    private void FindPanel()
    {
        if (ppeChecklistPanel != null)
            return;

        Transform found = transform.Find("PPEChecklistPanel");

        if (found != null)
            ppeChecklistPanel = found.gameObject;
    }

    private void ConnectButtons()
    {
        FindPanel();

        if (ppeChecklistPanel == null)
            return;

        ConnectButton("HelmetButton", ToggleHelmet);
        ConnectButton("GlassesButton", ToggleGlasses);
        ConnectButton("GlovesButton", ToggleGloves);
        ConnectButton("BootsButton", ToggleBoots);
        ConnectButton("ContinueButton", ContinueToFacility);
    }

    private void ConnectButton(
        string name,
        UnityAction action)
    {
        Transform child = ppeChecklistPanel.transform.Find(name);

        if (child == null)
            return;

        Button button = child.GetComponent<Button>();

        if (button != null)
        {
            button.onClick.RemoveListener(action);
            button.onClick.AddListener(action);
        }
    }

    // ==========================================
    // OPEN PPE CHECKLIST
    // ==========================================

    public void OpenPPEChecklist(string mode)
    {
        FindPanel();

        if (ppeChecklistPanel == null)
        {
            Debug.LogError("PPE Checklist Panel missing.");
            return;
        }

        if (mode != "Training Mode" && mode != "Testing Mode")
        {
            Debug.LogWarning("Invalid training mode.");
            return;
        }

        selectedMode = mode;

        ResetChecklist();

        if (modeSelectionPanel != null)
            modeSelectionPanel.SetActive(false);

        ppeChecklistPanel.SetActive(true);

        Debug.Log("PPE Checklist opened for " + mode);
    }

    // ==========================================
    // CHECKLIST INTERACTIONS
    // ==========================================

    public void ToggleHelmet()
    {
        ToggleItem(0);
    }

    public void ToggleGlasses()
    {
        ToggleItem(1);
    }

    public void ToggleGloves()
    {
        ToggleItem(2);
    }

    public void ToggleBoots()
    {
        ToggleItem(3);
    }

    private void ToggleItem(int index)
    {
        confirmed[index] = !confirmed[index];

        UpdateItem(index);
        UpdateContinueButton();
    }

    private void ResetChecklist()
    {
        for (int i = 0; i < confirmed.Length; i++)
        {
            confirmed[i] = false;
            UpdateItem(i);
        }

        UpdateContinueButton();
    }

    private void UpdateItem(int index)
    {
        if (ppeChecklistPanel == null)
            return;

        Transform button =
            ppeChecklistPanel.transform.Find(buttonNames[index]);

        if (button == null)
            return;

        Transform textObject = button.Find("ButtonText");

        if (textObject != null)
        {
            TextMeshProUGUI text =
                textObject.GetComponent<TextMeshProUGUI>();

            if (text != null)
            {
                text.text = (confirmed[index] ? "[X]  " : "[ ]  ")
                    + itemNames[index];
            }
        }

        Image image = button.GetComponent<Image>();

        if (image != null)
        {
            image.color = confirmed[index]
                ? new Color32(44, 115, 121, 255)
                : new Color32(24, 73, 102, 255);
        }
    }

    private bool AllConfirmed()
    {
        foreach (bool item in confirmed)
        {
            if (!item)
                return false;
        }

        return true;
    }

    private void UpdateContinueButton()
    {
        if (ppeChecklistPanel == null)
            return;

        bool ready = AllConfirmed();

        Transform button =
            ppeChecklistPanel.transform.Find("ContinueButton");

        if (button != null)
        {
            Image image = button.GetComponent<Image>();

            if (image != null)
            {
                image.color = ready
                    ? new Color32(90, 154, 190, 255)
                    : new Color32(85, 110, 125, 255);
            }
        }

        SetStatus(
            ready
                ? "All PPE items confirmed. You may continue."
                : "Confirm all four PPE items."
        );
    }

    // ==========================================
    // CONTINUE
    // ==========================================

    public void ContinueToFacility()
    {
        if (!AllConfirmed())
        {
            SetStatus("Please confirm all PPE items first.");
            return;
        }

        Debug.Log(
            "PPE Checklist completed for " + selectedMode
        );

        if (nextPanel != null)
        {
            ppeChecklistPanel.SetActive(false);
            nextPanel.SetActive(true);
        }
        else
        {
            SetStatus(
                "PPE complete. Facility entry is not connected yet."
            );
        }
    }

    private void SetStatus(string message)
    {
        if (ppeChecklistPanel == null)
            return;

        Transform status =
            ppeChecklistPanel.transform.Find("PPEStatus");

        if (status != null)
        {
            TextMeshProUGUI text =
                status.GetComponent<TextMeshProUGUI>();

            if (text != null)
                text.text = message;
        }
    }

    // ==========================================
    // UI HELPERS
    // ==========================================

    private void SetRect(
        GameObject obj,
        float width,
        float height,
        float x,
        float y)
    {
        RectTransform rect = obj.GetComponent<RectTransform>();

        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);

        rect.sizeDelta = new Vector2(width, height);
        rect.anchoredPosition = new Vector2(x, y);
        rect.localScale = Vector3.one;
        rect.localRotation = Quaternion.identity;
    }

    private void CreateText(
        Transform parent,
        string name,
        string content,
        float size,
        float x,
        float y,
        float width,
        float height)
    {
        GameObject obj = new GameObject(
            name,
            typeof(RectTransform),
            typeof(TextMeshProUGUI)
        );

        obj.transform.SetParent(parent, false);
        obj.layer = LayerMask.NameToLayer("UI");

        SetRect(obj, width, height, x, y);

        TextMeshProUGUI text =
            obj.GetComponent<TextMeshProUGUI>();

        text.text = content;
        text.fontSize = size;
        text.color = Color.white;
        text.alignment = TextAlignmentOptions.Center;
        text.raycastTarget = false;
    }

    private void CreateButton(
        Transform parent,
        string name,
        string label,
        float x,
        float y,
        float width,
        float height)
    {
        GameObject obj = new GameObject(
            name,
            typeof(RectTransform),
            typeof(Image),
            typeof(Button)
        );

        obj.transform.SetParent(parent, false);
        obj.layer = LayerMask.NameToLayer("UI");

        SetRect(obj, width, height, x, y);

        Image image = obj.GetComponent<Image>();
        image.color = new Color32(24, 73, 102, 255);

        Button button = obj.GetComponent<Button>();
        button.targetGraphic = image;

        BoxCollider collider = obj.AddComponent<BoxCollider>();
        collider.size = new Vector3(width, height, 10);

        // Keep the same EZPZ interaction system.
        obj.AddComponent<InteractableGeneral>();

        CreateText(
            obj.transform,
            "ButtonText",
            label,
            26,
            0, 0,
            width - 20,
            height
        );
    }
}
