
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SafetyWelcomePanelBuilder : MonoBehaviour
{
    [Header("Onboarding Navigation")]
    public GameObject identificationPanel;

    [Header("Welcome Panel Colours")]
    public Color outerColour = new Color32(126, 152, 166, 235);
    public Color cardColour = new Color32(24, 73, 102, 255);
    public Color buttonColour = new Color32(90, 154, 190, 255);
    public Color textColour = Color.white;

    private Button launchButton;
    private static Sprite roundedSprite;

    private void Awake()
    {
        ApplyWelcomeDesign();
        ConnectButton();
    }

    [ContextMenu("Build Welcome Panel")]
    public void BuildWelcomePanel()
    {
        Transform panel = transform.Find("WelcomePanel");

        if (panel == null)
        {
            Debug.LogWarning(
                "WelcomePanel not found. Keep your existing " +
                "WelcomePanel and its EZPZ interaction components."
            );
            return;
        }

        ApplyWelcomeDesign();
        ConnectButton();

        Debug.Log("Welcome Panel design updated.");
    }

    [ContextMenu("Apply Navy Welcome Design")]
    public void ApplyWelcomeDesign()
    {
        Transform panel = transform.Find("WelcomePanel");

        if (panel == null)
        {
            Debug.LogWarning("WelcomePanel not found.");
            return;
        }

        // Main outer background
        RectTransform panelRect =
            panel.GetComponent<RectTransform>();

        SetRect(panelRect, 850, 420, 0, 0);

        Image outerImage = panel.GetComponent<Image>();

        if (outerImage == null)
            outerImage = panel.gameObject.AddComponent<Image>();

        ApplyRoundedImage(outerImage, outerColour);
        outerImage.raycastTarget = false;

        // Dark inner card
        Transform innerCard = panel.Find("InnerCard");

        if (innerCard == null)
        {
            GameObject cardObject = CreateUI(
                "InnerCard",
                panel
            );

            innerCard = cardObject.transform;
        }

        RectTransform cardRect =
            innerCard.GetComponent<RectTransform>();

        SetRect(cardRect, 765, 290, 0, -20);

        Image cardImage = innerCard.GetComponent<Image>();

        if (cardImage == null)
            cardImage = innerCard.gameObject.AddComponent<Image>();

        ApplyRoundedImage(cardImage, cardColour);
        cardImage.raycastTarget = false;

        // Keep the card behind all text and buttons.
        innerCard.SetAsFirstSibling();

        // Small heading above the navy card
        SetupText(
            panel,
            "AreaLabel",
            "SAFETY EQUIPMENT AREA",
            20,
            700, 35,
            0, 160,
            FontStyles.Normal
        );

        // Main title
        SetupText(
            panel,
            "WelcomeTitle",
            "WELCOME TO THE EXPERIENCE",
            35,
            710, 60,
            0, 65,
            FontStyles.Bold
        );

        // Subtitle
        SetupText(
            panel,
            "WelcomeDescription",
            "Virtual Reality Pipeline Inspection Training",
            24,
            700, 45,
            0, -10,
            FontStyles.Normal
        );

        // Hide the extra instruction sentence.
        Transform instructions =
            panel.Find("WelcomeInstructions");

        if (instructions != null)
            instructions.gameObject.SetActive(false);

        // Restyle the EXISTING launch button.
        Transform launch =
            panel.Find("LaunchExperienceButton");

        if (launch != null)
        {
            RectTransform buttonRect =
                launch.GetComponent<RectTransform>();

            SetRect(buttonRect, 330, 75, 0, -100);

            Image buttonImage =
                launch.GetComponent<Image>();

            if (buttonImage == null)
                buttonImage =
                    launch.gameObject.AddComponent<Image>();

            ApplyRoundedImage(
                buttonImage,
                buttonColour
            );

            Button button =
                launch.GetComponent<Button>();

            if (button != null)
                button.targetGraphic = buttonImage;

            // Preserve the EZPZ InteractableGeneral
            // and its existing interaction events.
            BoxCollider collider =
                launch.GetComponent<BoxCollider>();

            if (collider != null)
            {
                collider.center = Vector3.zero;
                collider.size =
                    new Vector3(330, 75, 10);
            }

            SetupText(
                launch,
                "ButtonText",
                "LAUNCH EXPERIENCE",
                26,
                320, 70,
                0, 0,
                FontStyles.Bold
            );

            launch.SetAsLastSibling();
        }

        Debug.Log("Navy Welcome Panel design applied.");
    }

    private GameObject CreateUI(
        string objectName,
        Transform parent)
    {
        GameObject obj = new GameObject(
            objectName,
            typeof(RectTransform)
        );

        obj.transform.SetParent(parent, false);
        obj.layer = LayerMask.NameToLayer("UI");

        return obj;
    }

    private void SetRect(
        RectTransform rect,
        float width,
        float height,
        float x,
        float y)
    {
        if (rect == null)
            return;

        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);

        rect.sizeDelta = new Vector2(width, height);
        rect.anchoredPosition = new Vector2(x, y);

        rect.localRotation = Quaternion.identity;
        rect.localScale = Vector3.one;
    }

    private void SetupText(
        Transform parent,
        string objectName,
        string content,
        float fontSize,
        float width,
        float height,
        float x,
        float y,
        FontStyles style)
    {
        Transform existing = parent.Find(objectName);

        GameObject obj;

        if (existing != null)
        {
            obj = existing.gameObject;
        }
        else
        {
            obj = CreateUI(objectName, parent);
        }

        obj.SetActive(true);

        RectTransform rect =
            obj.GetComponent<RectTransform>();

        SetRect(rect, width, height, x, y);

        TextMeshProUGUI text =
            obj.GetComponent<TextMeshProUGUI>();

        if (text == null)
            text = obj.AddComponent<TextMeshProUGUI>();

        text.text = content;
        text.fontSize = fontSize;
        text.fontStyle = style;
        text.color = textColour;
        text.alignment = TextAlignmentOptions.Center;
        text.enableAutoSizing = false;
        text.enableWordWrapping = true;
        text.raycastTarget = false;
    }

    private void ApplyRoundedImage(
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

        texture.filterMode = FilterMode.Bilinear;
        texture.wrapMode = TextureWrapMode.Clamp;

        Color[] pixels = new Color[size * size];

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
                    Mathf.Min(Mathf.Max(q.x, q.y), 0f)
                    + Vector2.Max(q, Vector2.zero).magnitude
                    - radius;

                float alpha = Mathf.Clamp01(
                    0.5f - distance
                );

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

    private void ConnectButton()
    {
        Transform buttonTransform = transform.Find(
            "WelcomePanel/LaunchExperienceButton"
        );

        if (buttonTransform == null)
            return;

        launchButton =
            buttonTransform.GetComponent<Button>();

        if (launchButton != null)
        {
            launchButton.onClick.RemoveListener(
                LaunchExperience
            );

            launchButton.onClick.AddListener(
                LaunchExperience
            );
        }
    }

    public void LaunchExperience()
    {
        if (identificationPanel == null)
        {
            Debug.LogWarning(
                "Identification panel is not assigned."
            );
            return;
        }

        identificationPanel.SetActive(true);

        Transform welcome =
            transform.Find("WelcomePanel");

        if (welcome != null)
            welcome.gameObject.SetActive(false);

        Debug.Log("Trainee Identification opened.");
    }
}
