
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

public class SafetyModeSelectionPanel : MonoBehaviour
{
    [Header("Panels")]
    public GameObject identificationPanel;
    public GameObject modeSelectionPanel;

    [Header("PPE Safety Checklist")]
    public SafetyPPEChecklistPanel ppeChecklist;

    [Header("Current Selection")]
    public string selectedMode = "";

    // ==========================================
    // INITIALISATION
    // ==========================================

    private void Awake()
    {
        FindPanel();

        if (ppeChecklist == null)
        {
            ppeChecklist =
                GetComponent<SafetyPPEChecklistPanel>();
        }

        UpdateModeButtonLabels();
        ConnectExistingButtons();
    }

    private void FindPanel()
    {
        if (modeSelectionPanel != null)
            return;

        Transform found =
            transform.Find("ModeSelectionPanel");

        if (found != null)
            modeSelectionPanel = found.gameObject;
    }

    // ==========================================
    // BUILD MODE SELECTION PANEL
    // ==========================================

    [ContextMenu("Build Mode Selection Panel")]
    public void BuildModeSelectionPanel()
    {
        Transform existing =
            transform.Find("ModeSelectionPanel");

        if (existing != null)
        {
            modeSelectionPanel = existing.gameObject;

            UpdateModeButtonLabels();
            ConnectExistingButtons();

            Debug.Log(
                "Existing Mode Selection Panel updated."
            );

            return;
        }

        GameObject panel = new GameObject(
            "ModeSelectionPanel",
            typeof(RectTransform),
            typeof(Image)
        );

        panel.transform.SetParent(transform, false);

        RectTransform rect =
            panel.GetComponent<RectTransform>();

        rect.anchorMin =
            new Vector2(0.5f, 0.5f);

        rect.anchorMax =
            new Vector2(0.5f, 0.5f);

        rect.pivot =
            new Vector2(0.5f, 0.5f);

        rect.sizeDelta =
            new Vector2(850, 420);

        rect.anchoredPosition = Vector2.zero;
        rect.localScale = Vector3.one;

        Image background =
            panel.GetComponent<Image>();

        background.color =
            new Color32(123, 156, 173, 255);

        // TITLE
        CreateText(
            panel.transform,
            "ModeTitle",
            "SELECT YOUR MODE",
            43,
            0, 140,
            790, 65
        );

        // INSTRUCTIONS
        CreateText(
            panel.transform,
            "ModeInstructions",
            "Choose a mode to continue.",
            26,
            0, 75,
            750, 50
        );

        // TRAINING MODE
        CreateButton(
            panel.transform,
            "TrainingButton",
            "TRAINING MODE",
            -205, -35,
            390, 90
        );

        // TESTING MODE
        CreateButton(
            panel.transform,
            "TestingButton",
            "TESTING MODE",
            205, -35,
            390, 90
        );

        // STATUS
        CreateText(
            panel.transform,
            "ModeStatus",
            "",
            22,
            0, -145,
            750, 50
        );

        modeSelectionPanel = panel;

        UpdateModeButtonLabels();
        ConnectExistingButtons();

        modeSelectionPanel.SetActive(false);

        Debug.Log(
            "Mode Selection Panel created successfully."
        );
    }

    // ==========================================
    // UPDATE EXISTING BUTTON LABELS
    // ==========================================

    private void UpdateModeButtonLabels()
    {
        SetButtonLabel(
            "TrainingButton",
            "TRAINING MODE"
        );

        SetButtonLabel(
            "TestingButton",
            "TESTING MODE"
        );
    }

    private void SetButtonLabel(
        string buttonName,
        string newLabel)
    {
        if (modeSelectionPanel == null)
            return;

        Transform button =
            modeSelectionPanel.transform.Find(
                buttonName
            );

        if (button == null)
            return;

        Transform textTransform =
            button.Find("ButtonText");

        if (textTransform == null)
            return;

        TextMeshProUGUI text =
            textTransform.GetComponent<TextMeshProUGUI>();

        if (text != null)
            text.text = newLabel;
    }

    // ==========================================
    // CONNECT EXISTING BUTTONS
    // ==========================================

    private void ConnectExistingButtons()
    {
        ConnectButton(
            "TrainingButton",
            ChooseTraining
        );

        ConnectButton(
            "TestingButton",
            ChooseTesting
        );
    }

    private void ConnectButton(
        string buttonName,
        UnityAction action)
    {
        if (modeSelectionPanel == null)
            return;

        Transform target =
            modeSelectionPanel.transform.Find(
                buttonName
            );

        if (target == null)
            return;

        Button button =
            target.GetComponent<Button>();

        if (button != null)
        {
            button.onClick.RemoveListener(action);
            button.onClick.AddListener(action);
        }
    }

    // ==========================================
    // OPEN MODE SELECTION
    // ==========================================

    public void OpenModeSelection()
    {
        FindPanel();

        if (modeSelectionPanel == null)
        {
            Debug.LogError(
                "Mode Selection Panel is missing."
            );
            return;
        }

        if (identificationPanel != null)
        {
            identificationPanel.SetActive(false);
        }

        UpdateModeButtonLabels();

        modeSelectionPanel.SetActive(true);

        selectedMode = "";

        SetStatus("");

        Debug.Log(
            "Mode Selection Panel opened."
        );
    }

    // ==========================================
    // TRAINING MODE
    // ==========================================

    public void ChooseTraining()
    {
        selectedMode = "Training Mode";

        Debug.Log(
            "Selected mode: Training Mode"
        );

        OpenPPEChecklist();
    }

    // ==========================================
    // TESTING MODE
    // ==========================================

    public void ChooseTesting()
    {
        selectedMode = "Testing Mode";

        Debug.Log(
            "Selected mode: Testing Mode"
        );

        OpenPPEChecklist();
    }

    // ==========================================
    // OPEN PPE CHECKLIST
    // ==========================================

    private void OpenPPEChecklist()
    {
        if (ppeChecklist == null)
        {
            ppeChecklist =
                GetComponent<SafetyPPEChecklistPanel>();
        }

        if (ppeChecklist == null)
        {
            SetStatus(
                "PPE Safety Checklist is not connected."
            );

            Debug.LogError(
                "SafetyPPEChecklistPanel component missing."
            );

            return;
        }

        if (ppeChecklist.ppeChecklistPanel == null)
        {
            Transform found =
                transform.Find("PPEChecklistPanel");

            if (found != null)
            {
                ppeChecklist.ppeChecklistPanel =
                    found.gameObject;
            }
        }

        if (ppeChecklist.ppeChecklistPanel == null)
        {
            SetStatus(
                "PPE Checklist Panel has not been built."
            );

            Debug.LogWarning(
                "Build PPE Checklist Panel first."
            );

            return;
        }

        // Preserve the chosen Training/Testing mode.
        ppeChecklist.OpenPPEChecklist(selectedMode);

        Debug.Log(
            "Opening PPE Checklist for: " +
            selectedMode
        );
    }

    // ==========================================
    // STATUS TEXT
    // ==========================================

    private void SetStatus(string message)
    {
        if (modeSelectionPanel == null)
            return;

        Transform label =
            modeSelectionPanel.transform.Find(
                "ModeStatus"
            );

        if (label == null)
            return;

        TextMeshProUGUI text =
            label.GetComponent<TextMeshProUGUI>();

        if (text != null)
            text.text = message;
    }

    // ==========================================
    // CREATE TEXT
    // ==========================================

    private void CreateText(
        Transform parent,
        string objectName,
        string content,
        float fontSize,
        float x,
        float y,
        float width,
        float height)
    {
        GameObject obj = new GameObject(
            objectName,
            typeof(RectTransform),
            typeof(TextMeshProUGUI)
        );

        obj.transform.SetParent(parent, false);

        int uiLayer =
            LayerMask.NameToLayer("UI");

        if (uiLayer >= 0)
            obj.layer = uiLayer;

        RectTransform rect =
            obj.GetComponent<RectTransform>();

        rect.anchorMin =
            new Vector2(0.5f, 0.5f);

        rect.anchorMax =
            new Vector2(0.5f, 0.5f);

        rect.pivot =
            new Vector2(0.5f, 0.5f);

        rect.sizeDelta =
            new Vector2(width, height);

        rect.anchoredPosition =
            new Vector2(x, y);

        TextMeshProUGUI text =
            obj.GetComponent<TextMeshProUGUI>();

        text.text = content;
        text.fontSize = fontSize;
        text.color = Color.white;

        text.alignment =
            TextAlignmentOptions.Center;

        text.raycastTarget = false;
    }

    // ==========================================
    // CREATE BUTTON
    // ==========================================

    private void CreateButton(
        Transform parent,
        string objectName,
        string label,
        float x,
        float y,
        float width,
        float height)
    {
        GameObject obj = new GameObject(
            objectName,
            typeof(RectTransform),
            typeof(Image),
            typeof(Button)
        );

        obj.transform.SetParent(parent, false);

        int uiLayer =
            LayerMask.NameToLayer("UI");

        if (uiLayer >= 0)
            obj.layer = uiLayer;

        RectTransform rect =
            obj.GetComponent<RectTransform>();

        rect.anchorMin =
            new Vector2(0.5f, 0.5f);

        rect.anchorMax =
            new Vector2(0.5f, 0.5f);

        rect.pivot =
            new Vector2(0.5f, 0.5f);

        rect.sizeDelta =
            new Vector2(width, height);

        rect.anchoredPosition =
            new Vector2(x, y);

        Image image =
            obj.GetComponent<Image>();

        image.color =
            new Color32(24, 73, 102, 255);

        Button button =
            obj.GetComponent<Button>();

        button.targetGraphic = image;

        // XR collider.
        BoxCollider collider =
            obj.AddComponent<BoxCollider>();

        collider.size =
            new Vector3(width, height, 10);

        // EZPZ Interaction Toolkit.
        obj.AddComponent<InteractableGeneral>();

        CreateText(
            obj.transform,
            "ButtonText",
            label,
            27,
            0, 0,
            width - 20,
            height
        );
    }
}
