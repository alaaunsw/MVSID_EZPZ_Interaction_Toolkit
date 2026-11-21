
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TestingIdentificationPanel : MonoBehaviour
{
    [Header("Existing Identification Panel")]
    public GameObject identificationPanel;

    [Header("Existing Login Inputs")]
    public TMP_InputField traineeNameInput;
    public TMP_InputField traineePasswordInput;
    public TextMeshProUGUI statusText;

    [Header("Account and Training Progress")]
    public SafetyTraineeAccountManager accountManager;
    public SafetyTraineeProgressManager progressManager;

    [Header("Existing Buttons")]
    public Button continueButton;
    public Button backButton;

    [Header("Login Status")]
    [SerializeField] private bool testingUnlocked;
    [SerializeField] private int successfulTrainingSessions;
    [SerializeField] private int requiredTrainingSessions;

    private bool loginCompleted;

    // ==========================================
    // INITIALISATION
    // ==========================================

    private void Awake()
    {
        FindReferences();
        ConnectButtons();
        PrepareLoginPanel();
    }

    // ==========================================
    // FIND EXISTING REFERENCES
    // ==========================================

    private void FindReferences()
    {
        if (identificationPanel == null)
        {
            Transform found =
                transform.Find("IdentificationPanel");

            if (found != null)
            {
                identificationPanel = found.gameObject;
            }
        }

        if (identificationPanel != null)
        {
            Transform root =
                identificationPanel.transform;

            if (traineeNameInput == null)
            {
                Transform found =
                    root.Find("TraineeNameInput");

                if (found != null)
                {
                    traineeNameInput =
                        found.GetComponent<TMP_InputField>();
                }
            }

            if (traineePasswordInput == null)
            {
                Transform found =
                    root.Find("TraineePasswordInput");

                if (found != null)
                {
                    traineePasswordInput =
                        found.GetComponent<TMP_InputField>();
                }
            }

            if (statusText == null)
            {
                Transform found =
                    root.Find("StatusText");

                if (found != null)
                {
                    statusText =
                        found.GetComponent<TextMeshProUGUI>();
                }
            }

            if (continueButton == null)
            {
                Transform found =
                    root.Find("ContinueButton");

                if (found != null)
                {
                    continueButton =
                        found.GetComponent<Button>();
                }
            }

            if (backButton == null)
            {
                Transform found =
                    root.Find("BackButton");

                if (found != null)
                {
                    backButton =
                        found.GetComponent<Button>();
                }
            }
        }

        // Use the current persistent account manager.

        if (SafetyTraineeAccountManager.Instance != null)
        {
            accountManager =
                SafetyTraineeAccountManager.Instance;
        }

        if (accountManager == null)
        {
            accountManager =
                FindFirstObjectByType<
                    SafetyTraineeAccountManager
                >();
        }

        // Use the progress manager belonging to
        // the same account manager.

        if (accountManager != null)
        {
            SafetyTraineeProgressManager manager =
                accountManager.GetComponent<
                    SafetyTraineeProgressManager
                >();

            if (manager != null)
            {
                progressManager = manager;
            }
        }
    }

    // ==========================================
    // CONNECT EXISTING UNITY UI BUTTONS
    // ==========================================

    private void ConnectButtons()
    {
        if (continueButton != null)
        {
            continueButton.onClick.RemoveListener(
                Continue
            );

            continueButton.onClick.AddListener(
                Continue
            );
        }

        if (backButton != null)
        {
            backButton.onClick.RemoveListener(
                BackToAccountSelection
            );

            backButton.onClick.AddListener(
                BackToAccountSelection
            );
        }
    }

    // ==========================================
    // PREPARE TESTING LOGIN PANEL
    // ==========================================

    private void PrepareLoginPanel()
    {
        if (identificationPanel == null)
        {
            Debug.LogError(
                "Testing Identification Panel is missing."
            );

            return;
        }

        // Testing Mode is login-only.
        // New account registration is unavailable.

        SetChildVisible("SignUpButton", false);
        SetChildVisible("LoginButton", false);

        // Show existing login fields.

        SetChildVisible("TraineeNameInput", true);
        SetChildVisible("TraineePasswordInput", true);
        SetChildVisible("ContinueButton", true);
        SetChildVisible("StatusText", true);

        // No account-selection step is required.

        SetChildVisible("BackButton", false);

        RectTransform rect =
            identificationPanel.GetComponent<RectTransform>();

        if (rect != null)
        {
            rect.sizeDelta =
                new Vector2(850, 540);
        }

        SetPanelText(
            "IdentificationTitle",
            "RETURNING TRAINEE LOGIN"
        );

        SetPanelText(
            "Instructions",
            "Enter your existing username and password."
        );

        SetButtonText(
            "ContinueButton",
            "LOG IN"
        );

        // Keep password characters hidden.

        if (traineePasswordInput != null)
        {
            traineePasswordInput.contentType =
                TMP_InputField.ContentType.Password;

            traineePasswordInput.asteriskChar = '*';

            traineePasswordInput.ForceLabelUpdate();
        }

        ClearInputs();

        SetStatus(
            "Log in to verify your training eligibility."
        );

        testingUnlocked = false;
        loginCompleted = false;
        successfulTrainingSessions = 0;
        requiredTrainingSessions = 0;
    }

    // ==========================================
    // OPEN EXISTING LOGIN PANEL
    // ==========================================

    public void ShowLogin()
    {
        PrepareLoginPanel();

        if (identificationPanel != null)
        {
            identificationPanel.SetActive(true);
        }
    }

    // Compatibility with existing buttons.

    public void SelectLogin()
    {
        ShowLogin();
    }

    public void ShowAccountSelection()
    {
        ShowLogin();
    }

    public void BackToAccountSelection()
    {
        ShowLogin();
    }

    // ==========================================
    // DISABLE SIGN UP IN TESTING MODE
    // ==========================================

    public void SelectSignUp()
    {
        Debug.LogWarning(
            "Sign Up is disabled in Testing Mode."
        );

        SetStatus(
            "Testing Mode is available " +
            "to existing trainees only."
        );
    }

    // ==========================================
    // LOGIN AND ELIGIBILITY CHECK
    // ==========================================

    public void Continue()
    {
        if (!Application.isPlaying)
            return;

        if (loginCompleted)
        {
            Debug.Log(
                "Testing login has already been completed."
            );

            return;
        }

        FindReferences();

        // ======================================
        // CHECK ACCOUNT MANAGER
        // ======================================

        if (accountManager == null)
        {
            SetStatus(
                "Account Manager is not connected."
            );

            Debug.LogError(
                "SafetyTraineeAccountManager missing."
            );

            return;
        }

        // ======================================
        // CHECK INPUT FIELDS
        // ======================================

        if (traineeNameInput == null ||
            traineePasswordInput == null)
        {
            SetStatus(
                "Username or password input is missing."
            );

            return;
        }

        string username =
            traineeNameInput.text.Trim();

        string password =
            traineePasswordInput.text;

        if (string.IsNullOrWhiteSpace(username))
        {
            SetStatus(
                "Please enter your username."
            );

            return;
        }

        if (string.IsNullOrEmpty(password))
        {
            SetStatus(
                "Please enter your password."
            );

            return;
        }

        // ======================================
        // AUTHENTICATE EXISTING TRAINEE
        // ======================================

        string message;

        bool success = accountManager.Login(
            username,
            password,
            out message
        );

        // Clear the password after authentication.

        traineePasswordInput.SetTextWithoutNotify("");

        traineePasswordInput.ForceLabelUpdate();

        if (!success)
        {
            SetStatus(message);

            Debug.LogWarning(
                "Testing Mode login failed."
            );

            return;
        }

        Debug.Log(
            "Testing Mode login successful for: " +
            accountManager.CurrentUsername
        );

        // ======================================
        // LOAD INDIVIDUAL TRAINING PROGRESS
        // ======================================

        FindReferences();

        if (progressManager == null)
        {
            SetStatus(
                "Training progress manager not found."
            );

            Debug.LogError(
                "SafetyTraineeProgressManager missing."
            );

            return;
        }

        progressManager.RefreshProgress();

        successfulTrainingSessions =
            progressManager.SuccessfulTrainingSessions;

        requiredTrainingSessions =
            progressManager.RequiredTrainingSessions;

        testingUnlocked =
            progressManager.CanAccessTesting;

        // ======================================
        // CHECK TESTING ELIGIBILITY
        // ======================================

        if (!testingUnlocked)
        {
            SetStatus(
                progressManager.GetTestingEligibilityMessage()
            );

            Debug.LogWarning(
                "Testing locked. Successful sessions: " +
                successfulTrainingSessions +
                " / " +
                requiredTrainingSessions
            );

            // Keep the login panel visible.
            // Do not permit PPE exploration.

            return;
        }

        // ======================================
        // SUCCESSFUL LOGIN AND VERIFICATION
        // ======================================

        loginCompleted = true;

        SetStatus(
            "Welcome, " +
            accountManager.CurrentUsername +
            "!\nTraining verified: " +
            successfulTrainingSessions +
            " / " +
            requiredTrainingSessions +
            "\nIndependent PPE assessment is next."
        );

        Debug.Log(
            "Testing eligibility verified for: " +
            accountManager.CurrentUsername
        );

        // ======================================
        // NEW: CLOSE THE LOGIN PANEL
        // ======================================

        // The trainee can now move freely toward
        // the blue zone and explore equipment.

        if (identificationPanel != null)
        {
            identificationPanel.SetActive(false);
        }

        Debug.Log(
            "Login panel closed. " +
            "Trainee can begin PPE exploration."
        );

        // Do not open the old Training Mode
        // selection panel or PPE training videos.
        //
        // Do not load Testing Environment yet.
        // The independent PPE assessment must
        // be completed first.
    }

    // ==========================================
    // UI VISIBILITY HELPERS
    // ==========================================

    private void SetChildVisible(
        string childName,
        bool visible)
    {
        if (identificationPanel == null)
            return;

        Transform child =
            identificationPanel.transform.Find(
                childName
            );

        if (child != null)
        {
            child.gameObject.SetActive(visible);
        }
    }

    // ==========================================
    // UI TEXT HELPERS
    // ==========================================

    private void SetPanelText(
        string childName,
        string value)
    {
        if (identificationPanel == null)
            return;

        Transform child =
            identificationPanel.transform.Find(
                childName
            );

        if (child == null)
            return;

        TMP_Text text =
            child.GetComponent<TMP_Text>();

        if (text != null)
        {
            text.text = value;
        }
    }

    private void SetButtonText(
        string buttonName,
        string value)
    {
        if (identificationPanel == null)
            return;

        Transform button =
            identificationPanel.transform.Find(
                buttonName
            );

        if (button == null)
            return;

        TMP_Text text =
            button.GetComponentInChildren<TMP_Text>(true);

        if (text != null)
        {
            text.text = value;
        }
    }

    private void SetStatus(string message)
    {
        if (statusText != null)
        {
            statusText.text = message;
        }
    }

    // ==========================================
    // CLEAR LOGIN INPUTS
    // ==========================================

    private void ClearInputs()
    {
        if (traineeNameInput != null)
        {
            traineeNameInput.SetTextWithoutNotify("");

            traineeNameInput.ForceLabelUpdate();
        }

        if (traineePasswordInput != null)
        {
            traineePasswordInput.SetTextWithoutNotify("");

            traineePasswordInput.ForceLabelUpdate();
        }
    }
}
