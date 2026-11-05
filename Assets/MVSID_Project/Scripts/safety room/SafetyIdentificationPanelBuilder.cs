
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SafetyIdentificationPanelBuilder : MonoBehaviour
{
    [Header("Navigation")]
    public GameObject mainMenuPanel;

    [Header("Runtime")]
    public TMP_InputField traineeNameInput;
    public TextMeshProUGUI statusText;

    private GameObject identificationPanel;
    private bool isSignUp = true;

    private const string SavedNameKey = "SafetyTraineeName";

    [ContextMenu("Build Identification Panel")]
    public void BuildIdentificationPanel()
    {
        if (transform.Find("IdentificationPanel") != null)
        {
            Debug.LogWarning(
                "IdentificationPanel already exists. Build cancelled.");
            return;
        }

        GameObject panel = CreateUI(
            "IdentificationPanel", transform);

        SetRect(panel, 850, 420, 0, 0);

        Image background = panel.AddComponent<Image>();
        background.color = new Color32(110, 143, 161, 245);

        CreateText(
            "IdentificationTitle", panel.transform,
            "TRAINEE IDENTIFICATION",
            43, 790, 65, 0, 150);

        CreateText(
            "Instructions", panel.transform,
            "Select Sign Up or Log In to continue.",
            26, 760, 50, 0, 95);

        CreateButton(
            "SignUpButton", panel.transform,
            "SIGN UP", -180, 28, 280, 60);

        CreateButton(
            "LoginButton", panel.transform,
            "LOG IN", 180, 28, 280, 60);

        // Input field background
        GameObject inputObject = CreateUI(
            "TraineeNameInput", panel.transform);

        SetRect(inputObject, 570, 65, 0, -55);

        Image inputBackground =
            inputObject.AddComponent<Image>();

        inputBackground.color =
            new Color32(220, 232, 239, 255);

        TMP_InputField input =
            inputObject.AddComponent<TMP_InputField>();

        input.lineType =
            TMP_InputField.LineType.SingleLine;

        input.characterLimit = 32;

        // Text area
        GameObject textArea = CreateUI(
            "Text Area", inputObject.transform);

        SetRect(textArea, 540, 60, 0, 0);

        RectMask2D mask = textArea.AddComponent<RectMask2D>();

        GameObject textObject = CreateUI(
            "Text", textArea.transform);

        SetRect(textObject, 520, 55, 0, 0);

        TextMeshProUGUI inputText =
            textObject.AddComponent<TextMeshProUGUI>();

        inputText.fontSize = 30;
        inputText.color = new Color32(35, 55, 67, 255);
        inputText.alignment = TextAlignmentOptions.MidlineLeft;
        inputText.raycastTarget = false;

        GameObject placeholderObject = CreateUI(
            "Placeholder", textArea.transform);

        SetRect(placeholderObject, 520, 55, 0, 0);

        TextMeshProUGUI placeholder =
            placeholderObject.AddComponent<TextMeshProUGUI>();

        placeholder.text = "Enter trainee name";
        placeholder.fontSize = 26;
        placeholder.fontStyle = FontStyles.Italic;
        placeholder.color =
            new Color32(110, 125, 135, 255);
        placeholder.alignment =
            TextAlignmentOptions.MidlineLeft;
        placeholder.raycastTarget = false;

        input.textViewport =
            textArea.GetComponent<RectTransform>();

        input.textComponent = inputText;
        input.placeholder = placeholder;

        traineeNameInput = input;

        CreateButton(
            "ContinueButton", panel.transform,
            "CONTINUE", 0, -137, 320, 60);

        statusText = CreateText(
            "StatusText", panel.transform,
            "New trainee? Select Sign Up.",
            20, 750, 40, 0, -190);

        // Initially hidden.
        panel.SetActive(false);

        identificationPanel = panel;

        Debug.Log(
            "Identification Panel created successfully.");
    }

    private void Awake()
    {
        identificationPanel =
            transform.Find("IdentificationPanel")?.gameObject;

        if (identificationPanel == null)
            return;

        traineeNameInput = identificationPanel
            .GetComponentInChildren<TMP_InputField>(true);

        Transform status = identificationPanel
            .transform.Find("StatusText");

        if (status != null)
            statusText = status.GetComponent<TextMeshProUGUI>();

        ConnectButton("SignUpButton", SelectSignUp);
        ConnectButton("LoginButton", SelectLogin);
        ConnectButton("ContinueButton", Continue);
    }

    private void ConnectButton(
        string buttonName, UnityEngine.Events.UnityAction action)
    {
        Transform target = identificationPanel
            .transform.Find(buttonName);

        if (target == null)
            return;

        Button button = target.GetComponent<Button>();

        if (button != null)
        {
            button.onClick.AddListener(action);
        }
    }

    public void SelectSignUp()
    {
        isSignUp = true;

        if (traineeNameInput != null)
            traineeNameInput.text = "";

        SetStatus("Enter your name to create a trainee profile.");
    }

    public void SelectLogin()
    {
        isSignUp = false;

        string savedName =
            PlayerPrefs.GetString(SavedNameKey, "");

        if (traineeNameInput != null)
            traineeNameInput.text = savedName;

        if (string.IsNullOrEmpty(savedName))
            SetStatus("No local profile found. Select Sign Up.");
        else
            SetStatus("Existing trainee profile loaded.");
    }

    public void Continue()
    {
        if (traineeNameInput == null)
        {
            Debug.LogWarning("Trainee input not assigned.");
            return;
        }

        string name = traineeNameInput.text.Trim();

        if (string.IsNullOrEmpty(name))
        {
            SetStatus("Please enter your trainee name.");
            return;
        }

        if (isSignUp)
        {
            PlayerPrefs.SetString(SavedNameKey, name);
            PlayerPrefs.Save();
        }
        else
        {
            string savedName =
                PlayerPrefs.GetString(SavedNameKey, "");

            if (name != savedName ||
                string.IsNullOrEmpty(savedName))
            {
                SetStatus("Profile not found. Select Sign Up.");
                return;
            }
        }

        Debug.Log("Trainee identified: " + name);

        if (mainMenuPanel != null)
        {
            mainMenuPanel.SetActive(true);
            identificationPanel.SetActive(false);
        }
        else
        {
            SetStatus("Profile ready. Main Menu not connected yet.");
            Debug.Log(
                "Identification complete. Main Menu not assigned.");
        }
    }

    private void SetStatus(string message)
    {
        if (statusText != null)
            statusText.text = message;
    }

    private GameObject CreateUI(
        string name, Transform parent)
    {
        GameObject obj = new GameObject(
            name, typeof(RectTransform));

        obj.transform.SetParent(parent, false);
        obj.layer = LayerMask.NameToLayer("UI");

        return obj;
    }

    private void SetRect(
        GameObject obj, float width, float height,
        float x, float y)
    {
        RectTransform rect =
            obj.GetComponent<RectTransform>();

        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);

        rect.sizeDelta = new Vector2(width, height);
        rect.anchoredPosition = new Vector2(x, y);
        rect.localScale = Vector3.one;
        rect.localRotation = Quaternion.identity;
    }

    private TextMeshProUGUI CreateText(
        string name, Transform parent,
        string content, float fontSize,
        float width, float height, float x, float y)
    {
        GameObject obj = CreateUI(name, parent);
        SetRect(obj, width, height, x, y);

        TextMeshProUGUI text =
            obj.AddComponent<TextMeshProUGUI>();

        text.text = content;
        text.fontSize = fontSize;
        text.color = Color.white;
        text.alignment = TextAlignmentOptions.Center;
        text.raycastTarget = false;

        return text;
    }

    private void CreateButton(
        string name, Transform parent,
        string label, float x, float y,
        float width, float height)
    {
        GameObject buttonObject =
            CreateUI(name, parent);

        SetRect(buttonObject, width, height, x, y);

        Image image =
            buttonObject.AddComponent<Image>();

        image.color =
            new Color32(79, 116, 134, 255);

        Button button =
            buttonObject.AddComponent<Button>();

        button.targetGraphic = image;

        BoxCollider collider =
            buttonObject.AddComponent<BoxCollider>();

        collider.size =
            new Vector3(width, height, 10);

        CreateText(
            "ButtonText", buttonObject.transform,
            label, 29,
            width - 10, height, 0, 0);
    }
}
