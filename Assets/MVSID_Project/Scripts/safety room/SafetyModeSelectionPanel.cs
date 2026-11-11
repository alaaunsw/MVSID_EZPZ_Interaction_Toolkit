
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

    [Header("Trainee Progress")]
    public SafetyTraineeProgressManager progressManager;

    [Header("Current Selection")]
    public string selectedMode = "";

    private readonly Color normalButtonColour =
        new Color32(24, 73, 102, 255);

    private readonly Color lockedButtonColour =
        new Color32(88, 107, 120, 255);

    // ==========================================
    // INITIALISATION
    // ==========================================

    private void Awake()
    {
        FindPanel();
        FindManagers();

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

    private void FindManagers()
    {
        if (ppeChecklist == null)
        {
            ppeChecklist =
                GetComponent<SafetyPPEChecklistPanel>();
        }

        if (progressManager == null)
        {
            progressManager =
                FindFirstObjectByType<SafetyTraineeProgressManager>();
        }
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

        int uiLayer = LayerMask.NameToLayer("UI");

        if (uiLayer >= 0)
            panel.layer = uiLayer;

        RectTransform rect =
            panel.GetComponent<RectTransform>();

        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);

        rect.sizeDelta = new Vector2(850, 420);
        rect.anchoredPosition = Vector2.zero;
        rect.localScale = Vector3.one;

        Image background =
            panel.GetComponent<Image>();

        background.color =
            new Color32(123, 156, 173, 255);

        CreateText(
            panel.transform,
            "ModeTitle",
            "SELECT YOUR MODE",
            43,
            0, 140,
            790, 65
        );

        CreateText(
            panel.transform,
            "ModeInstructions",
            "Choose a mode to continue.",
            26,
            0, 75,
            750, 50
        );

        CreateButton(
            panel.transform,
            "TrainingButton",
            "TRAINING MODE",
            -205, -35,
            390, 90
        );

        CreateButton(
            panel.transform,
            "TestingButton",
            "TESTING MODE",
            205, -35,
            390, 90
        );

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
    // TRAINEE PROGRESS AND ACCESS
    // ==========================================

    private bool IsTestingUnlocked()
    {
        FindManagers();

        if (progressManager == null)
            return false;

        return progressManager.CanAccessTesting;
    }

    private void UpdateModeButtonLabels()
    {
        SetButtonLabel(
            "TrainingButton",
            "TRAINING MODE"
        );

        if (IsTestingUnlocked())
        {
            SetButtonLabel(
                "TestingButton",
                "TESTING MODE"
            );
        }
        else
        {
            SetButtonLabel(
                "TestingButton",
                "TESTING MODE - LOCKED"
            );
        }

        UpdateTestingButtonAppearance();
    }

    private void UpdateTestingButtonAppearance()
    {
        if (modeSelectionPanel == null)
            return;

        Transform buttonTransform =
            modeSelectionPanel.transform.Find(
                "TestingButton"
            );

        if (buttonTransform == null)
            return;

        bool unlocked = IsTestingUnlocked();

        Image image =
            buttonTransform.GetComponent<Image>();

        if (image != null)
        {
            image.color = unlocked
                ? normalButtonColour
                : lockedButtonColour;
        }

        // Keep the button interactable so the trainee
        // receives an explanation when pressing it.
        // Access is enforced inside ChooseTesting().
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
        FindManagers();

        if (modeSelectionPanel == null)
        {
            Debug.LogError(
                "Mode Selection Panel is missing."
            );
            return;
        }

        if (progressManager == null)
        {
            Debug.LogWarning(
                "Trainee Progress Manager is missing. " +
                "Testing Mode will remain locked."
            );
        }
        else
        {
            progressManager.RefreshProgress();
        }

        UpdateModeButtonLabels();

        if (identificationPanel != null)
        {
            identificationPanel.SetActive(false);
        }

        modeSelectionPanel.SetActive(true);

        selectedMode = "";

        if (IsTestingUnlocked())
        {
            SetStatus(
                "Training completed. Both modes are available."
            );
        }
        else
        {
            SetStatus(
                "Complete Training Mode to unlock Testing Mode."
            );
        }

        Debug.Log(
            "Mode Selection opened. Testing unlocked: " +
            IsTestingUnlocked()
        );
    }

    // ==========================================
    // TRAINING MODE
    // ==========================================

  
public void ChooseTraining()
{
    selectedMode = "Training Mode";

    Debug.Log("Training Mode started.");

    // Hide the mode-selection interface.
    if (modeSelectionPanel != null)
        modeSelectionPanel.SetActive(false);

    // Hide the old PPE checklist.
    if (ppeChecklist != null &&
        ppeChecklist.ppeChecklistPanel != null)
    {
        ppeChecklist.ppeChecklistPanel.SetActive(false);
    }

    
    // NEW: Activate the PPE training start zone
    FindFirstObjectByType<PPETrainingController>()?.BeginTraining();

    // Do not open the PPE checklist.
    // The player can now explore the PPE equipment.
}


    // ==========================================
    // TESTING MODE - ACCESS RESTRICTION
    // ==========================================

    public void ChooseTesting()
    {
        FindManagers();

        if (progressManager == null)
        {
            selectedMode = "";

            SetStatus(
                "Trainee Progress Manager is not connected."
            );

            Debug.LogError(
                "SafetyTraineeProgressManager missing."
            );

            return;
        }

        // Check the logged-in trainee's saved progress.
        progressManager.LoadCurrentTraineeProgress();

        if (!progressManager.CanAccessTesting)
        {
            selectedMode = "";

            SetStatus(
                "TESTING MODE LOCKED - Pass Training Mode first."
            );

            UpdateModeButtonLabels();

            Debug.LogWarning(
                "Testing Mode denied: training not passed."
            );

            // Do NOT open PPE Checklist.
            return;
        }

        selectedMode = "Testing Mode";

        SetStatus(
            "Testing Mode unlocked."
        );

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
        FindManagers();

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

        // Additional guard against bypassing
        // testing access through this method.
        if (selectedMode == "Testing Mode" &&
            !IsTestingUnlocked())
        {
            selectedMode = "";

            SetStatus(
                "TESTING MODE LOCKED - Pass Training Mode first."
            );

            return;
        }

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

        image.color = normalButtonColour;

        Button button =
            obj.GetComponent<Button>();

        button.targetGraphic = image;

        BoxCollider collider =
            obj.AddComponent<BoxCollider>();

        collider.size =
            new Vector3(width, height, 10);

        // Preserve EZPZ Interaction Toolkit.
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
