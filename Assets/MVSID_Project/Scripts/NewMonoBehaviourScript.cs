
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TutorialWelcomeCountdown : MonoBehaviour
{
    [Header("Existing UI")]
    public GameObject welcomePanel;
    public GameObject activity1Panel;
    public TMP_Text welcomeText;
    public TMP_Text countdownText;

    [Header("Timing")]
    public int countdownSeconds = 5;

    private readonly Color navy =
        new Color32(14, 29, 62, 255);

    private readonly Color cyan =
        new Color32(67, 219, 239, 255);

    private readonly Color muted =
        new Color32(161, 187, 219, 255);

    private void Start()
    {
        if (!welcomePanel || !activity1Panel ||
            !welcomeText || !countdownText)
        {
            Debug.LogError("Missing welcome UI references.");
            return;
        }

        SetupWelcomeMenu();

        welcomePanel.SetActive(true);
        activity1Panel.SetActive(false);

        StartCoroutine(WelcomeSequence());
    }

    private void SetupWelcomeMenu()
    {
        RectTransform panel =
            welcomePanel.GetComponent<RectTransform>();

        panel.anchorMin = new Vector2(0.5f, 0.5f);
        panel.anchorMax = new Vector2(0.5f, 0.5f);
        panel.pivot = new Vector2(0.5f, 0.5f);
        panel.anchoredPosition = Vector2.zero;
        panel.sizeDelta = new Vector2(1000, 335);
        panel.localScale = Vector3.one;

        // Background
        Image background = welcomePanel.GetComponent<Image>();

        if (!background)
            background = welcomePanel.AddComponent<Image>();

        background.color = navy;
        background.raycastTarget = false;

        // Main welcome message
        welcomeText.text =
            "<size=44><b>WELCOME!</b></size>\n\n" +
            "This short tutorial will help you get familiar " +
            "with the VR controllers and practise the basic " +
            "skills you'll need during training.";

        StyleText(
            welcomeText,
            new Vector2(0, 55),
            new Vector2(940, 165),
            22,
            Color.white
        );

        // Countdown text
        StyleText(
            countdownText,
            new Vector2(0, -110),
            new Vector2(900, 130),
            22,
            muted
        );

        CreateLabel(
            "TopLeftLabel",
            "VR CONTROLS TRAINING",
            new Vector2(-410, 140),
            new Vector2(180, 30),
            13,
            cyan
        );

        CreateLabel(
            "TopRightLabel",
            "WELCOME",
            new Vector2(420, 140),
            new Vector2(100, 30),
            13,
            muted
        );
    }

    private void StyleText(
        TMP_Text text,
        Vector2 position,
        Vector2 size,
        float fontSize,
        Color color)
    {
        RectTransform rect = text.rectTransform;

        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        rect.localScale = Vector3.one;

        text.fontSize = fontSize;
        text.color = color;
        text.alignment = TextAlignmentOptions.Center;
        text.enableAutoSizing = false;
        text.enableWordWrapping = true;
        text.overflowMode = TextOverflowModes.Overflow;
        text.raycastTarget = false;
    }

    private void CreateLabel(
        string objectName,
        string content,
        Vector2 position,
        Vector2 size,
        float fontSize,
        Color color)
    {
        Transform existing =
            welcomePanel.transform.Find(objectName);

        GameObject labelObject;

        if (existing)
        {
            labelObject = existing.gameObject;
        }
        else
        {
            labelObject = new GameObject(
                objectName,
                typeof(RectTransform),
                typeof(TextMeshProUGUI)
            );

            labelObject.transform.SetParent(
                welcomePanel.transform, false
            );
        }

        TMP_Text label =
            labelObject.GetComponent<TMP_Text>();

        label.text = content;

        StyleText(label, position, size, fontSize, color);
    }

    private IEnumerator WelcomeSequence()
    {
        for (int i = countdownSeconds; i > 0; i--)
        {
            countdownText.text =
                "Your first activity begins in\n" +
                "<size=58><color=#43DBEF><b>" +
                i +
                "</b></color></size>\n" +
                "<size=17>seconds</size>";

            yield return new WaitForSeconds(1f);
        }

        welcomePanel.SetActive(false);
        activity1Panel.SetActive(true);

        Debug.Log("Welcome finished. Activity 1 started.");
    }
}
