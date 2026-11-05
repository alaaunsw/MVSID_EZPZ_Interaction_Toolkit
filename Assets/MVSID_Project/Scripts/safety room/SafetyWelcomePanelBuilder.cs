
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SafetyWelcomePanelBuilder : MonoBehaviour
{
    [Header("Onboarding Navigation")]
    public GameObject identificationPanel;

    private Button launchButton;

    [ContextMenu("Build Welcome Panel")]
    public void BuildWelcomePanel()
    {
        Transform oldPanel = transform.Find("WelcomePanel");

        if (oldPanel != null)
        {
            Debug.LogWarning(
                "WelcomePanel already exists. Delete it first if you want to rebuild."
            );
            return;
        }

        // Main panel
        GameObject panel = CreateUI("WelcomePanel", transform);
        SetRect(panel, 850, 420, 0, 0);

        Image background = panel.AddComponent<Image>();
       background.color = new Color32(110, 143, 161, 235);

        // Title
        CreateText(
            "WelcomeTitle",
            panel.transform,
            "WELCOME TO THE EXPERIENCE",
            44,
            780, 65,
            0, 135,
            FontStyles.Bold
        );

        // Description
        CreateText(
            "WelcomeDescription",
            panel.transform,
            "Virtual Reality Pipeline Inspection Training",
            29,
            760, 55,
            0, 45,
            FontStyles.Normal
        );

        // Instructions
        CreateText(
            "WelcomeInstructions",
            panel.transform,
            "Follow the instructions to begin your experience.",
            24,
            740, 55,
            0, -25,
            FontStyles.Normal
        );

        // Launch button
        GameObject buttonObject =
            CreateUI("LaunchExperienceButton", panel.transform);

        SetRect(buttonObject, 330, 75, 0, -125);

        Image buttonImage = buttonObject.AddComponent<Image>();
       buttonImage.color = new Color32(79, 116, 134, 255);

        Button button = buttonObject.AddComponent<Button>();
        button.targetGraphic = buttonImage;

        // Collider for EZPZ ray interaction
        BoxCollider collider = buttonObject.AddComponent<BoxCollider>();
        collider.size = new Vector3(330, 75, 10);

        CreateText(
            "ButtonText",
            buttonObject.transform,
            "LAUNCH EXPERIENCE",
            29,
            320, 70,
            0, 0,
            FontStyles.Bold
        );

        Debug.Log("Safety Welcome Panel created successfully.");
    }

    private GameObject CreateUI(string name, Transform parent)
    {
        GameObject obj = new GameObject(
            name,
            typeof(RectTransform)
        );

        obj.transform.SetParent(parent, false);
        obj.layer = LayerMask.NameToLayer("UI");

        return obj;
    }

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
        string name,
        Transform parent,
        string content,
        float fontSize,
        float width,
        float height,
        float x,
        float y,
        FontStyles style)
    {
        GameObject obj = CreateUI(name, parent);

        SetRect(obj, width, height, x, y);

        TextMeshProUGUI text =
            obj.AddComponent<TextMeshProUGUI>();

        text.text = content;
        text.fontSize = fontSize;
        text.fontStyle = style;
        text.color = new Color32(242, 246, 248, 255);
        text.alignment = TextAlignmentOptions.Center;
        text.enableWordWrapping = true;
        text.raycastTarget = false;
    }

    private void Awake()
    {
        ConnectButton();
    }

    private void ConnectButton()
    {
        Transform buttonTransform = transform.Find(
            "WelcomePanel/LaunchExperienceButton"
        );

        if (buttonTransform == null)
            return;

        launchButton = buttonTransform.GetComponent<Button>();

        if (launchButton != null)
        {
            launchButton.onClick.RemoveListener(LaunchExperience);
            launchButton.onClick.AddListener(LaunchExperience);
        }
    }

    public void LaunchExperience()
    {
        if (identificationPanel == null)
        {
            Debug.Log(
                "Launch pressed. Identification panel is not assigned yet."
            );
            return;
        }

        identificationPanel.SetActive(true);

        Transform welcome = transform.Find("WelcomePanel");

        if (welcome != null)
            welcome.gameObject.SetActive(false);
    }
}

