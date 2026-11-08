
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;

public class SafetyIdentificationPanelBuilder : MonoBehaviour
{
    [Header("Navigation")]
    public GameObject mainMenuPanel;

    [Header("Account System")]
    public SafetyTraineeAccountManager accountManager;

    [Header("Mode Selection")]
    public SafetyModeSelectionPanel modeSelection;

    [Header("Runtime")]
    public TMP_InputField traineeNameInput;
    public TMP_InputField traineePasswordInput;
    public TextMeshProUGUI statusText;

    [Header("Account Mode")]
    public bool isSignUp = true;

    [Header("Design Colours")]
    public Color outerColour =
        new Color32(123, 156, 173, 255);

    public Color buttonColour =
        new Color32(24, 73, 102, 255);

    public Color actionButtonColour =
        new Color32(90, 154, 190, 255);

    private GameObject identificationPanel;
    private static Sprite roundedSprite;

    private enum ScreenMode
    {
        ChooseAccount,
        SignUp,
        Login
    }

    private ScreenMode currentScreen =
        ScreenMode.ChooseAccount;

    private void Awake()
    {
        FindReferences();

        if (accountManager == null)
            accountManager =
                FindFirstObjectByType<SafetyTraineeAccountManager>();

        if (modeSelection == null)
            modeSelection =
                GetComponent<SafetyModeSelectionPanel>();

        ConnectExistingButtons();
        ShowAccountSelection();
    }

    // ==========================================
    // FIND EXISTING OBJECTS
    // ==========================================

    private void FindReferences()
    {
        Transform panel =
            transform.Find("IdentificationPanel");

        if (panel == null)
            return;

        identificationPanel = panel.gameObject;

        Transform name =
            panel.Find("TraineeNameInput");

        Transform password =
            panel.Find("TraineePasswordInput");

        Transform status =
            panel.Find("StatusText");

        if (name != null)
            traineeNameInput =
                name.GetComponent<TMP_InputField>();

        if (password != null)
            traineePasswordInput =
                password.GetComponent<TMP_InputField>();

        if (status != null)
            statusText =
                status.GetComponent<TextMeshProUGUI>();
    }

    // ==========================================
    // BUILD IDENTIFICATION PANEL
    // ==========================================

    [ContextMenu("Build Identification Panel")]
    public void BuildIdentificationPanel()
    {
        if (transform.Find("IdentificationPanel") != null)
        {
            Debug.LogWarning(
                "IdentificationPanel already exists. " +
                "Use Upgrade Identification Panel."
            );
            return;
        }

        GameObject panel =
            CreateUI("IdentificationPanel", transform);

        SetRect(panel, 850, 540, 0, 0);

        Image background =
            panel.AddComponent<Image>();

        background.color = outerColour;

        CreateText(
            "IdentificationTitle",
            panel.transform,
            "TRAINEE IDENTIFICATION",
            43, 790, 65, 0, 200
        );

        CreateText(
            "Instructions",
            panel.transform,
            "Do you already have an account?",
            26, 760, 50, 0, 110
        );

        CreateButton(
            "SignUpButton",
            panel.transform,
            "SIGN UP",
            -180, -15, 280, 70
        );

        CreateButton(
            "LoginButton",
            panel.transform,
            "LOG IN",
            180, -15, 280, 70
        );

        traineeNameInput = CreateInputField(
            "TraineeNameInput",
            panel.transform,
            "Enter username",
            40,
            false
        );

        traineePasswordInput = CreateInputField(
            "TraineePasswordInput",
            panel.transform,
            "Enter password",
            -50,
            true
        );

        CreateButton(
            "ContinueButton",
            panel.transform,
            "CONTINUE",
            0, -135, 320, 60
        );

        CreateButton(
            "BackButton",
            panel.transform,
            "BACK",
            0, -200, 200, 50
        );

        statusText = CreateText(
            "StatusText",
            panel.transform,
            "",
            18, 750, 35, 0, -245
        );

        identificationPanel = panel;

        ConnectExistingButtons();
        ShowAccountSelection();

        panel.SetActive(false);

        Debug.Log(
            "Identification Panel created successfully."
        );
    }

    // ==========================================
    // UPGRADE EXISTING PANEL
    // ==========================================

    [ContextMenu("Upgrade Identification Panel For Login")]
    public void UpgradeIdentificationPanelForLogin()
    {
        FindReferences();

        if (identificationPanel == null)
        {
            Debug.LogWarning(
                "IdentificationPanel not found."
            );
            return;
        }

        Transform panel =
            identificationPanel.transform;

        if (panel.Find("BackButton") == null)
        {
            CreateButton(
                "BackButton",
                panel,
                "BACK",
                0, -200, 200, 50
            );
        }

        if (traineeNameInput == null ||
            traineePasswordInput == null)
        {
            Debug.LogWarning(
                "Username or password field missing."
            );
            return;
        }

        traineePasswordInput.contentType =
            TMP_InputField.ContentType.Password;

        traineePasswordInput.asteriskChar = '*';
        traineePasswordInput.ForceLabelUpdate();

        ConnectExistingButtons();
        ShowAccountSelection();

        Debug.Log(
            "Identification Panel upgraded successfully."
        );
    }

    // ==========================================
    // SCREEN 1 - ACCOUNT SELECTION
    // ==========================================

    public void ShowAccountSelection()
    {
        FindReferences();

        if (identificationPanel == null)
            return;

        currentScreen = ScreenMode.ChooseAccount;

        SetRect(
            identificationPanel,
            850, 260, 0, 0
        );

        ApplyPanelBackground();

        SetVisible("SignUpButton", true);
        SetVisible("LoginButton", true);

        SetVisible("TraineeNameInput", false);
        SetVisible("TraineePasswordInput", false);
        SetVisible("ContinueButton", false);
        SetVisible("BackButton", false);
        SetVisible("StatusText", false);

        SetTitle("TRAINEE IDENTIFICATION");

        SetInstructions(
            "Do you already have an account?"
        );

        StyleText(
            "IdentificationTitle", 45, true
        );

        StyleText(
            "Instructions", 25, false
        );

        MoveUI("IdentificationTitle", 0, 73);
        MoveUI("Instructions", 0, 12);

        StyleButton(
            "SignUpButton",
            -205, -72,
            390, 75,
            buttonColour
        );

        StyleButton(
            "LoginButton",
            205, -72,
            390, 75,
            buttonColour
        );

        ClearInputs();
    }

    // ==========================================
    // SIGN UP SCREEN
    // ==========================================

    public void SelectSignUp()
    {
        isSignUp = true;
        currentScreen = ScreenMode.SignUp;

        ShowCredentialsForm();

        SetTitle(
            "CREATE TRAINEE ACCOUNT"
        );

        SetInstructions(
            "Choose a username and password."
        );

        SetButtonLabel(
            "ContinueButton",
            "CREATE ACCOUNT"
        );

        SetStatus(
            "Enter your new account details."
        );

        Debug.Log("Sign Up screen opened.");
    }

    // ==========================================
    // LOG IN SCREEN
    // ==========================================

    public void SelectLogin()
    {
        isSignUp = false;
        currentScreen = ScreenMode.Login;

        ShowCredentialsForm();

        SetTitle("WELCOME BACK");

        SetInstructions(
            "Log in to continue your training."
        );

        SetButtonLabel(
            "ContinueButton",
            "LOG IN"
        );

        SetStatus(
            "Enter your existing account details."
        );

        Debug.Log("Log In screen opened.");
    }

    // ==========================================
    // USERNAME AND PASSWORD FORM
    // ==========================================

    private void ShowCredentialsForm()
    {
        if (identificationPanel == null)
            return;

        SetRect(
            identificationPanel,
            850, 540, 0, 0
        );

        ApplyPanelBackground();

        SetVisible("SignUpButton", false);
        SetVisible("LoginButton", false);

        SetVisible("TraineeNameInput", true);
        SetVisible("TraineePasswordInput", true);
        SetVisible("ContinueButton", true);
        SetVisible("BackButton", true);
        SetVisible("StatusText", true);

        StyleText(
            "IdentificationTitle", 42, true
        );

        StyleText(
            "Instructions", 25, false
        );

        MoveUI("IdentificationTitle", 0, 194);
        MoveUI("Instructions", 0, 132);

        MoveUI("TraineeNameInput", 0, 38);
        MoveUI("TraineePasswordInput", 0, -52);

        StyleButton(
            "ContinueButton",
            0, -137,
            390, 65,
            actionButtonColour
        );

        StyleButton(
            "BackButton",
            0, -204,
            200, 50,
            buttonColour
        );

        MoveUI("StatusText", 0, -245);

        ClearInputs();
    }

    // ==========================================
    // BACK BUTTON
    // ==========================================

    public void BackToAccountSelection()
    {
        ShowAccountSelection();

        Debug.Log(
            "Returned to account selection."
        );
    }

    // ==========================================
    // ACCOUNT REGISTRATION AND LOGIN
    // ==========================================

    public void Continue()
    {
        if (currentScreen == ScreenMode.ChooseAccount)
            return;

        FindReferences();

        if (accountManager == null)
        {
            accountManager =
                FindFirstObjectByType<SafetyTraineeAccountManager>();
        }

        if (accountManager == null)
        {
            SetStatus(
                "Account Manager is not connected."
            );

            Debug.LogError(
                "SafetyTraineeAccountManager not found."
            );

            return;
        }

        if (traineeNameInput == null ||
            traineePasswordInput == null)
        {
            SetStatus(
                "Username or password field missing."
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

        string message;
        bool success;

        // SIGN UP: Register a new trainee.
        if (currentScreen == ScreenMode.SignUp)
        {
            success = accountManager.Register(
                username,
                password,
                out message
            );
        }
        // LOG IN: Verify an existing trainee.
        else
        {
            success = accountManager.Login(
                username,
                password,
                out message
            );
        }

        SetStatus(message);

        // Incorrect credentials cannot proceed.
        if (!success)
        {
            Debug.LogWarning(
                "Trainee authentication failed: " +
                message
            );
            return;
        }

        Debug.Log(
            "Trainee authentication successful: " +
            accountManager.CurrentUsername
        );

        // Clear the password after authentication.
        traineePasswordInput.SetTextWithoutNotify("");
        traineePasswordInput.ForceLabelUpdate();

        SafetyKeyboardInputBridge keyboardBridge =
            GetComponent<SafetyKeyboardInputBridge>();

        if (keyboardBridge != null)
        {
            keyboardBridge.ResetKeyboardInputs();
        }

        SetStatus(
            "Welcome, " +
            accountManager.CurrentUsername +
            "!"
        );

        // ======================================
        // OPEN TRAINING / TESTING MODE SELECTION
        // ======================================

        if (modeSelection == null)
        {
            modeSelection =
                GetComponent<SafetyModeSelectionPanel>();
        }

        if (modeSelection == null)
        {
            Debug.LogError(
                "SafetyModeSelectionPanel is not connected."
            );

            SetStatus(
                "Mode Selection Panel is not connected."
            );

            return;
        }

        modeSelection.OpenModeSelection();

        Debug.Log(
            "Mode Selection opened for trainee: " +
            accountManager.CurrentUsername
        );
    }

    // ==========================================
    // PANEL BACKGROUND
    // ==========================================

    private void ApplyPanelBackground()
    {
        if (identificationPanel == null)
            return;

        Image image =
            identificationPanel.GetComponent<Image>();

        if (image == null)
            image =
                identificationPanel.AddComponent<Image>();

        ApplyRoundedImage(
            image,
            outerColour
        );
    }

    // ==========================================
    // BUTTON STYLING
    // ==========================================

    private void StyleButton(
        string buttonName,
        float x,
        float y,
        float width,
        float height,
        Color colour)
    {
        if (identificationPanel == null)
            return;

        Transform target =
            identificationPanel.transform.Find(
                buttonName
            );

        if (target == null)
            return;

        RectTransform rect =
            target.GetComponent<RectTransform>();

        if (rect != null)
        {
            rect.anchorMin =
                new Vector2(0.5f, 0.5f);

            rect.anchorMax =
                new Vector2(0.5f, 0.5f);

            rect.pivot =
                new Vector2(0.5f, 0.5f);

            rect.anchoredPosition =
                new Vector2(x, y);

            rect.sizeDelta =
                new Vector2(width, height);
        }

        Image image =
            target.GetComponent<Image>();

        if (image == null)
            image =
                target.gameObject.AddComponent<Image>();

        ApplyRoundedImage(image, colour);

        Button button =
            target.GetComponent<Button>();

        if (button != null)
            button.targetGraphic = image;

        BoxCollider collider =
            target.GetComponent<BoxCollider>();

        if (collider != null)
        {
            collider.center = Vector3.zero;

            collider.size = new Vector3(
                width,
                height,
                10
            );
        }

        Transform label =
            target.Find("ButtonText");

        if (label != null)
        {
            RectTransform labelRect =
                label.GetComponent<RectTransform>();

            if (labelRect != null)
            {
                labelRect.sizeDelta =
                    new Vector2(width - 20, height);

                labelRect.anchoredPosition =
                    Vector2.zero;
            }

            TextMeshProUGUI text =
                label.GetComponent<TextMeshProUGUI>();

            if (text != null)
            {
                text.fontSize = 26;
                text.fontStyle = FontStyles.Bold;
                text.color = Color.white;

                text.alignment =
                    TextAlignmentOptions.Center;

                text.raycastTarget = false;
            }
        }

        // Existing EZPZ InteractableGeneral and
        // On Primary Interact events are preserved.
    }

    // ==========================================
    // ROUNDED CORNERS
    // ==========================================

    private static void ApplyRoundedImage(
        Image image,
        Color colour)
    {
        if (roundedSprite == null)
            roundedSprite = CreateRoundedSprite();

        image.sprite = roundedSprite;
        image.type = Image.Type.Sliced;
        image.color = colour;
    }

    private static Sprite CreateRoundedSprite()
    {
        const int size = 64;
        const float radius = 15f;

        Texture2D texture = new Texture2D(
            size,
            size,
            TextureFormat.RGBA32,
            false
        );

        texture.filterMode =
            FilterMode.Bilinear;

        texture.wrapMode =
            TextureWrapMode.Clamp;

        Color[] pixels =
            new Color[size * size];

        float half = size * 0.5f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                Vector2 p = new Vector2(
                    x + 0.5f - half,
                    y + 0.5f - half
                );

                Vector2 q = new Vector2(
                    Mathf.Abs(p.x) - half + radius,
                    Mathf.Abs(p.y) - half + radius
                );

                float distance =
                    Mathf.Min(
                        Mathf.Max(q.x, q.y),
                        0f
                    )
                    + Vector2.Max(
                        q,
                        Vector2.zero
                    ).magnitude
                    - radius;

                float alpha =
                    Mathf.Clamp01(0.5f - distance);

                pixels[y * size + x] =
                    new Color(1f, 1f, 1f, alpha);
            }
        }

        texture.SetPixels(pixels);
        texture.Apply();

        return Sprite.Create(
            texture,
            new Rect(0, 0, size, size),
            new Vector2(0.5f, 0.5f),
            100f,
            0,
            SpriteMeshType.FullRect,
            new Vector4(16, 16, 16, 16)
        );
    }

    // ==========================================
    // VISIBILITY AND TEXT HELPERS
    // ==========================================

    private void SetVisible(
        string objectName,
        bool visible)
    {
        if (identificationPanel == null)
            return;

        Transform child =
            identificationPanel.transform.Find(
                objectName
            );

        if (child != null)
            child.gameObject.SetActive(visible);
    }

    private void SetTitle(string value)
    {
        SetText(
            "IdentificationTitle",
            value
        );
    }

    private void SetInstructions(string value)
    {
        SetText(
            "Instructions",
            value
        );
    }

    private void SetText(
        string objectName,
        string value)
    {
        if (identificationPanel == null)
            return;

        Transform child =
            identificationPanel.transform.Find(
                objectName
            );

        if (child == null)
            return;

        TextMeshProUGUI text =
            child.GetComponent<TextMeshProUGUI>();

        if (text != null)
            text.text = value;
    }

    private void StyleText(
        string objectName,
        float fontSize,
        bool bold)
    {
        if (identificationPanel == null)
            return;

        Transform child =
            identificationPanel.transform.Find(
                objectName
            );

        if (child == null)
            return;

        TextMeshProUGUI text =
            child.GetComponent<TextMeshProUGUI>();

        if (text == null)
            return;

        text.fontSize = fontSize;

        text.fontStyle =
            bold ? FontStyles.Bold : FontStyles.Normal;

        text.color = Color.white;

        text.alignment =
            TextAlignmentOptions.Center;
    }

    private void SetButtonLabel(
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

        Transform label =
            button.Find("ButtonText");

        if (label == null)
            return;

        TextMeshProUGUI text =
            label.GetComponent<TextMeshProUGUI>();

        if (text != null)
            text.text = value;
    }

    private void MoveUI(
        string objectName,
        float x,
        float y)
    {
        if (identificationPanel == null)
            return;

        Transform child =
            identificationPanel.transform.Find(
                objectName
            );

        if (child == null)
            return;

        RectTransform rect =
            child.GetComponent<RectTransform>();

        if (rect != null)
        {
            rect.anchoredPosition =
                new Vector2(x, y);
        }
    }

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

    private void SetStatus(string message)
    {
        if (statusText != null)
            statusText.text = message;
    }

    // ==========================================
    // BUTTON CONNECTIONS
    // ==========================================

    private void ConnectExistingButtons()
    {
        if (identificationPanel == null)
            return;

        ConnectButton(
            "SignUpButton",
            SelectSignUp
        );

        ConnectButton(
            "LoginButton",
            SelectLogin
        );

        ConnectButton(
            "ContinueButton",
            Continue
        );

        ConnectButton(
            "BackButton",
            BackToAccountSelection
        );
    }

    private void ConnectButton(
        string buttonName,
        UnityAction action)
    {
        Transform target =
            identificationPanel.transform.Find(
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
    // UI CREATION HELPERS
    // ==========================================

    private GameObject CreateUI(
        string objectName,
        Transform parent)
    {
        GameObject obj = new GameObject(
            objectName,
            typeof(RectTransform)
        );

        obj.transform.SetParent(parent, false);

        int layer =
            LayerMask.NameToLayer("UI");

        if (layer >= 0)
            obj.layer = layer;

        return obj;
    }

    private void SetRect(
        GameObject obj,
        float width,
        float height,
        float x,
        float y)
    {
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

        rect.localScale =
            Vector3.one;

        rect.localRotation =
            Quaternion.identity;
    }

    private TextMeshProUGUI CreateText(
        string objectName,
        Transform parent,
        string content,
        float fontSize,
        float width,
        float height,
        float x,
        float y)
    {
        GameObject obj =
            CreateUI(objectName, parent);

        SetRect(
            obj, width, height, x, y
        );

        TextMeshProUGUI text =
            obj.AddComponent<TextMeshProUGUI>();

        text.text = content;
        text.fontSize = fontSize;
        text.color = Color.white;

        text.alignment =
            TextAlignmentOptions.Center;

        text.raycastTarget = false;

        return text;
    }

    private TMP_InputField CreateInputField(
        string objectName,
        Transform parent,
        string placeholderText,
        float y,
        bool isPassword)
    {
        GameObject obj =
            CreateUI(objectName, parent);

        SetRect(
            obj, 570, 65, 0, y
        );

        Image background =
            obj.AddComponent<Image>();

        background.color =
            new Color32(220, 232, 239, 255);

        TMP_InputField input =
            obj.AddComponent<TMP_InputField>();

        input.lineType =
            TMP_InputField.LineType.SingleLine;

        input.characterLimit =
            isPassword ? 64 : 32;

        GameObject area =
            CreateUI("Text Area", obj.transform);

        SetRect(
            area, 540, 60, 0, 0
        );

        area.AddComponent<RectMask2D>();

        GameObject textObj =
            CreateUI("Text", area.transform);

        SetRect(
            textObj, 520, 55, 0, 0
        );

        TextMeshProUGUI inputText =
            textObj.AddComponent<TextMeshProUGUI>();

        inputText.fontSize = 30;

        inputText.color =
            new Color32(35, 55, 67, 255);

        inputText.alignment =
            TextAlignmentOptions.MidlineLeft;

        inputText.raycastTarget = false;

        GameObject placeholderObj =
            CreateUI(
                "Placeholder",
                area.transform
            );

        SetRect(
            placeholderObj,
            520, 55, 0, 0
        );

        TextMeshProUGUI placeholder =
            placeholderObj.AddComponent<TextMeshProUGUI>();

        placeholder.text = placeholderText;
        placeholder.fontSize = 26;
        placeholder.fontStyle = FontStyles.Italic;

        placeholder.color =
            new Color32(110, 125, 135, 255);

        placeholder.alignment =
            TextAlignmentOptions.MidlineLeft;

        placeholder.raycastTarget = false;

        input.textViewport =
            area.GetComponent<RectTransform>();

        input.textComponent = inputText;
        input.placeholder = placeholder;

        if (isPassword)
        {
            input.contentType =
                TMP_InputField.ContentType.Password;

            input.asteriskChar = '*';
        }

        return input;
    }

    private void CreateButton(
        string objectName,
        Transform parent,
        string label,
        float x,
        float y,
        float width,
        float height)
    {
        GameObject obj =
            CreateUI(objectName, parent);

        SetRect(
            obj, width, height, x, y
        );

        Image image =
            obj.AddComponent<Image>();

        image.color = buttonColour;

        Button button =
            obj.AddComponent<Button>();

        button.targetGraphic = image;

        BoxCollider collider =
            obj.AddComponent<BoxCollider>();

        collider.size =
            new Vector3(width, height, 10);

        // Preserve compatibility with your EZPZ setup.
        obj.AddComponent<InteractableGeneral>();

        CreateText(
            "ButtonText",
            obj.transform,
            label,
            25,
            width - 10,
            height,
            0, 0
        );
    }
}
