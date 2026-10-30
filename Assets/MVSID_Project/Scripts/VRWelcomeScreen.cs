using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class VRWelcomeScreen : MonoBehaviour
{
    [Header("Existing Panels")]
    public GameObject welcomePanel;
    public GameObject activity1Panel;
    public GameObject controllerDisplay;

    [Header("Settings")]
    public int countdownSeconds = 5;

    private TMP_Text countdownNumber;

    void Start()
    {
        if (welcomePanel == null || activity1Panel == null)
        {
            Debug.LogError("Assign WelcomePanel and Activity 1 Panel.");
            return;
        }

        // Hide the activity until the countdown finishes.
        activity1Panel.SetActive(false);

        if (controllerDisplay != null)
            controllerDisplay.SetActive(false);

        welcomePanel.SetActive(true);
        BuildWelcomeScreen();

        StartCoroutine(WelcomeCountdown());
    }

    void BuildWelcomeScreen()
    {
        // Clear previous welcome content only.
        // Existing navigation and activity panels are untouched.
        for (int i = welcomePanel.transform.childCount - 1; i >= 0; i--)
        {
            Destroy(welcomePanel.transform.GetChild(i).gameObject);
        }

        RectTransform panel = welcomePanel.GetComponent<RectTransform>();
        panel.anchorMin = new Vector2(0.5f, 0.5f);
        panel.anchorMax = new Vector2(0.5f, 0.5f);
        panel.pivot = new Vector2(0.5f, 0.5f);
        panel.anchoredPosition3D = Vector3.zero;
        panel.sizeDelta = new Vector2(1000, 650);
        panel.localScale = Vector3.one;

        Image background = welcomePanel.GetComponent<Image>();
        if (background == null)
            background = welcomePanel.AddComponent<Image>();

        background.color = new Color32(13, 27, 58, 255);
        background.raycastTarget = false;

        // Small top labels
        AddText("TopLeft", "VR CONTROLS TRAINING",
            18, new Vector2(-370, 275),
            new Vector2(240, 35),
            new Color32(79, 224, 239, 255),
            TextAlignmentOptions.Left);

        AddText("TopRight", "WELCOME",
            18, new Vector2(385, 275),
            new Vector2(160, 35),
            new Color32(170, 190, 215, 255),
            TextAlignmentOptions.Right);

        // Main heading
        AddText("Heading", "WELCOME!",
            64, new Vector2(0, 135),
            new Vector2(850, 100),
            Color.white,
            TextAlignmentOptions.Center,
            true);

        // Description
        AddText("Description",
            "This short tutorial will help you get familiar with the VR controllers and practise the basic skills you'll need during training.",
            29, new Vector2(0, 35),
            new Vector2(930, 100),
            Color.white,
            TextAlignmentOptions.Center);

        // Countdown label
        AddText("CountdownLabel",
            "Your first activity begins in",
            25, new Vector2(0, -75),
            new Vector2(700, 45),
            new Color32(160, 185, 220, 255),
            TextAlignmentOptions.Center);

        // Countdown number
        countdownNumber = AddText("CountdownNumber",
            countdownSeconds.ToString(),
            85, new Vector2(0, -155),
            new Vector2(250, 100),
            new Color32(79, 224, 239, 255),
            TextAlignmentOptions.Center,
            true);

        AddText("SecondsLabel", "seconds",
            23, new Vector2(0, -215),
            new Vector2(250, 40),
            new Color32(160, 185, 220, 255),
            TextAlignmentOptions.Center);
    }

    TMP_Text AddText(
        string objectName,
        string content,
        float fontSize,
        Vector2 position,
        Vector2 size,
        Color color,
        TextAlignmentOptions alignment,
        bool bold = false)
    {
        GameObject obj = new GameObject(objectName,
            typeof(RectTransform));

        obj.transform.SetParent(welcomePanel.transform, false);

        RectTransform rect = obj.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;

        TextMeshProUGUI text = obj.AddComponent<TextMeshProUGUI>();
        text.text = content;
        text.fontSize = fontSize;
        text.color = color;
        text.alignment = alignment;
        text.enableWordWrapping = true;
        text.raycastTarget = false;

        if (bold)
            text.fontStyle = FontStyles.Bold;

        return text;
    }

    IEnumerator WelcomeCountdown()
    {
        for (int seconds = countdownSeconds; seconds > 0; seconds--)
        {
            countdownNumber.text = seconds.ToString();
            yield return new WaitForSeconds(1f);
        }

        welcomePanel.SetActive(false);
        activity1Panel.SetActive(true);

        Debug.Log("Welcome complete. Activity 1 started.");
    }
}