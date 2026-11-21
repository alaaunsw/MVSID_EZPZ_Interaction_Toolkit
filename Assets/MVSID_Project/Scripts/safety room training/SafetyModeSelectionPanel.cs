
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
    public SafetyTraineeAccountManager accountManager;

    [Header("Current Selection")]
    public string selectedMode = "";

    [Header("Current Trainee Training Progress")]
    [SerializeField] private int successfulTrainingSessions;
    [SerializeField] private int requiredTrainingSessions;
    [SerializeField] private int remainingTrainingSessions;
    [SerializeField] private bool testingUnlocked;

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

    // ==========================================
    // FIND PERSISTENT TRAINEE MANAGERS
    // ==========================================

    private void FindManagers()
    {
        if (ppeChecklist == null)
        {
            ppeChecklist =
                GetComponent<SafetyPPEChecklistPanel>();
        }

        if (SafetyTraineeAccountManager.Instance != null)
        {
            accountManager =
                SafetyTraineeAccountManager.Instance;
        }
        else if (accountManager == null)
        {
            accountManager =
                FindFirstObjectByType<
                    SafetyTraineeAccountManager
                >();
        }

        // Prefer the persistent progress manager
        // attached to the account manager.

        if (accountManager != null)
        {
            SafetyTraineeProgressManager persistentManager =
                accountManager.GetComponent<
                    SafetyTraineeProgressManager
                >();

            if (persistentManager != null)
            {
                progressManager =
                    persistentManager;
            }
        }

        if (progressManager == null)
        {
            progressManager =
                FindFirstObjectByType<
                    SafetyTraineeProgressManager
                >();
        }
    }

    // ==========================================
    // CHECK LOGGED-IN TRAINEE
    // ==========================================

    private bool HasLoggedInTrainee()
    {
        FindManagers();

        return accountManager != null &&
               accountManager.IsLoggedIn;
    }

    // ==========================================
    // REFRESH SAVED TRAINING PROGRESS
    // ==========================================

    public void RefreshTrainingProgress()
    {
        FindManagers();

        if (progressManager == null)
        {
            successfulTrainingSessions = 0;
            requiredTrainingSessions = 1;
            remainingTrainingSessions = 1;
            testingUnlocked = false;

            return;
        }

        progressManager.RefreshProgress();

        requiredTrainingSessions =
            progressManager.RequiredTrainingSessions;

        if (!HasLoggedInTrainee())
        {
            successfulTrainingSessions = 0;

            remainingTrainingSessions =
                requiredTrainingSessions;

            testingUnlocked = false;

            return;
        }

        successfulTrainingSessions =
            progressManager.SuccessfulTrainingSessions;

        remainingTrainingSessions =
            progressManager.RemainingTrainingSessions;

        testingUnlocked =
            progressManager.CanAccessTesting;
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

        panel.transform.SetParent(
            transform, false
        );

        int uiLayer =
            LayerMask.NameToLayer("UI");

        if (uiLayer >= 0)
            panel.layer = uiLayer;

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

        rect.anchoredPosition =
            Vector2.zero;

        rect.localScale =
            Vector3.one;

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
    // TESTING MODE ELIGIBILITY
    // ==========================================

    private bool IsTestingUnlocked()
    {
        FindManagers();

        if (accountManager == null ||
            !accountManager.IsLoggedIn ||
            progressManager == null)
        {
            return false;
        }

        return progressManager.CanAccessTesting;
    }

    // ==========================================
    // UPDATE BUTTON LABELS
    // ==========================================

    private void UpdateModeButtonLabels()
    {
        SetButtonLabel(
            "TrainingButton",
            "TRAINING MODE"
        );

        testingUnlocked = IsTestingUnlocked();

        if (testingUnlocked)
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

    // ==========================================
    // UPDATE TESTING BUTTON APPEARANCE
    // ==========================================

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

        bool unlocked =
            IsTestingUnlocked();

        Image image =
            buttonTransform.GetComponent<Image>();

        if (image != null)
        {
            image.color = unlocked
                ? normalButtonColour
                : lockedButtonColour;
        }

        // Keep the existing button interactable.
        //
        // When locked, pressing it displays the
        // remaining training session requirement.
        //
        // Actual access is enforced in ChooseTesting().
    }

    // ==========================================
    // SET BUTTON LABEL
    // ==========================================

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
            textTransform.GetComponent<
                TextMeshProUGUI
            >();

        if (text != null)
        {
            text.text = newLabel;
        }
    }

    // ==========================================
    // DISPLAY TRAINEE SESSION PROGRESS
    // ==========================================

    private void UpdateTrainingProgressStatus()
    {
        RefreshTrainingProgress();

        if (progressManager == null)
        {
            SetStatus(
                "Trainee Progress Manager is unavailable."
            );

            return;
        }

        if (!HasLoggedInTrainee())
        {
            SetStatus(
                "Please log in to access training."
            );

            return;
        }

        string progressText =
            "Training sessions completed: " +
            successfulTrainingSessions +
            " / " +
            requiredTrainingSessions;

        if (testingUnlocked)
        {
            SetStatus(
                progressText +
                "\nIndependent Testing is unlocked."
            );
        }
        else
        {
            string remainingMessage =
                remainingTrainingSessions +
                " more training " +
                (
                    remainingTrainingSessions == 1
                    ? "session required."
                    : "sessions required."
                );

            SetStatus(
                progressText +
                "\n" +
                remainingMessage
            );
        }
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

        if (!HasLoggedInTrainee())
        {
            Debug.LogWarning(
                "Mode selection cannot open. " +
                "No trainee is currently logged in."
            );

            if (identificationPanel != null)
            {
                identificationPanel.SetActive(true);
            }

            modeSelectionPanel.SetActive(false);

            return;
        }

        // Load this trainee's saved training
        // repetitions from individual storage.

        RefreshTrainingProgress();

        UpdateModeButtonLabels();

        ConnectExistingButtons();

        if (identificationPanel != null)
        {
            identificationPanel.SetActive(false);
        }

        modeSelectionPanel.SetActive(true);

        selectedMode = "";

        UpdateTrainingProgressStatus();

        Debug.Log(
            "Mode Selection opened for: " +
            accountManager.CurrentUsername
        );

        Debug.Log(
            "Successful training sessions: " +
            successfulTrainingSessions +
            " / " +
            requiredTrainingSessions
        );

        Debug.Log(
            "Testing unlocked: " +
            testingUnlocked
        );
    }

    // ==========================================
    // TRAINING MODE
    // ==========================================

    public void ChooseTraining()
    {
        if (!Application.isPlaying)
            return;

        // Prevent duplicate activation from
        // Unity UI and EZPZ interaction events.

        if (!string.IsNullOrEmpty(selectedMode))
            return;

        FindManagers();

        if (!HasLoggedInTrainee())
        {
            SetStatus(
                "Please log in before starting training."
            );

            return;
        }

        PPETrainingController ppeController =
            FindFirstObjectByType<
                PPETrainingController
            >();

        if (ppeController == null)
        {
            SetStatus(
                "PPE Training Controller is unavailable."
            );

            Debug.LogError(
                "PPETrainingController could not be found."
            );

            return;
        }

        selectedMode = "Training Mode";

        Debug.Log(
            "Training Mode started for: " +
            accountManager.CurrentUsername
        );

        // Hide the existing mode selection panel.

        if (modeSelectionPanel != null)
        {
            modeSelectionPanel.SetActive(false);
        }

        // Hide the old PPE checklist.

        if (ppeChecklist != null &&
            ppeChecklist.ppeChecklistPanel != null)
        {
            ppeChecklist.ppeChecklistPanel
                .SetActive(false);
        }

        // Preserve the existing PPE training flow.

        ppeController.BeginTraining();
    }

    // ==========================================
    // TESTING MODE - ACCESS RESTRICTION
    // ==========================================

    public void ChooseTesting()
    {
        if (!Application.isPlaying)
            return;

        FindManagers();

        if (!HasLoggedInTrainee())
        {
            selectedMode = "";

            SetStatus(
                "Please log in before accessing Testing Mode."
            );

            return;
        }

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

        // Reload the current trainee's saved progress.

        RefreshTrainingProgress();

        // ==========================================
        // KEEP TESTING LOCKED IF TRAINING IS INCOMPLETE
        // ==========================================

        if (!testingUnlocked)
        {
            selectedMode = "";

            UpdateModeButtonLabels();

            string message =
                "Training sessions: " +
                successfulTrainingSessions +
                " / " +
                requiredTrainingSessions +
                "\nComplete " +
                remainingTrainingSessions +
                " more " +
                (
                    remainingTrainingSessions == 1
                    ? "session"
                    : "sessions"
                ) +
                " to unlock Testing.";

            SetStatus(message);

            Debug.LogWarning(
                "Testing Mode denied for: " +
                accountManager.CurrentUsername
            );

            Debug.Log(
                "Remaining training sessions: " +
                remainingTrainingSessions
            );

            return;
        }

        // Prevent duplicate button activation.

        if (!string.IsNullOrEmpty(selectedMode))
            return;

        // ==========================================
        // ALLOW TESTING MODE
        // ==========================================

        selectedMode = "Testing Mode";

        SetStatus(
            "Training sessions: " +
            successfulTrainingSessions +
            " / " +
            requiredTrainingSessions +
            "\nTesting Mode unlocked."
        );

        Debug.Log(
            "Testing Mode authorised for: " +
            accountManager.CurrentUsername
        );

        // Preserve the existing PPE checklist
        // connection for Testing Mode.

        OpenPPEChecklist();
    }

    // ==========================================
    // OPEN PPE CHECKLIST
    // ==========================================

    private void OpenPPEChecklist()
    {
        FindManagers();

        if (!HasLoggedInTrainee())
        {
            selectedMode = "";

            SetStatus(
                "A valid trainee login is required."
            );

            return;
        }

        // Additional access restriction.

        if (selectedMode == "Testing Mode" &&
            !IsTestingUnlocked())
        {
            selectedMode = "";

            UpdateTrainingProgressStatus();
            UpdateModeButtonLabels();

            return;
        }

        if (ppeChecklist == null)
        {
            selectedMode = "";

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
            selectedMode = "";

            SetStatus(
                "PPE Checklist Panel has not been built."
            );

            Debug.LogWarning(
                "Build PPE Checklist Panel first."
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
        {
            text.text = message;

            // Fit the two-line session progress
            // inside the existing ModeStatus area.

            text.enableAutoSizing = true;
            text.fontSizeMin = 14;
            text.fontSizeMax = 22;

            text.alignment =
                TextAlignmentOptions.Center;
        }
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

        obj.transform.SetParent(
            parent, false
        );

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

        obj.transform.SetParent(
            parent, false
        );

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
