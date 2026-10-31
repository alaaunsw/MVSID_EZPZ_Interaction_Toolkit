
using UnityEngine;
using TMPro;

public class VRTutorialFeedback : MonoBehaviour
{
    [Header("Existing Feedback UI")]
    public GameObject welcomeContent;
    public TextMeshProUGUI feedbackTitle;

    [Header("Activity Card Text")]
    public TextMeshProUGUI selectText;
    public TextMeshProUGUI navigateText;
    public TextMeshProUGUI grabText;
    public TextMeshProUGUI writeText;

    [Header("Continue Button")]
    public GameObject safetyRoomButton;

    [Header("Card Appearance")]
    public float activityFontSize = 28f;
    public float statusFontSize = 21f;

    private bool selected;
    private bool navigated;
    private bool grabbed;
    private bool written;

    private readonly Color completedColor =
        new Color32(145, 233, 188, 255);

    private readonly Color pendingColor = Color.white;

    private void Start()
    {
        // Prepare card formatting without opening feedback.
        FormatCard(selectText);
        FormatCard(navigateText);
        FormatCard(grabText);
        FormatCard(writeText);

        if (safetyRoomButton != null)
            safetyRoomButton.SetActive(false);
    }

    public void CompleteSelect()
    {
        selected = true;
    }

    public void CompleteNavigate()
    {
        navigated = true;
    }

    public void CompleteGrab()
    {
        grabbed = true;
    }

    public void CompleteWrite()
    {
        written = true;
    }

    public void ShowFeedback()
    {
        if (welcomeContent != null)
            welcomeContent.SetActive(true);

        if (feedbackTitle != null)
        {
            feedbackTitle.gameObject.SetActive(true);
            feedbackTitle.text =
                "VR TUTORIAL — YOUR PROGRESS";

            feedbackTitle.fontSize = 34;
            feedbackTitle.alignment =
                TextAlignmentOptions.Center;
        }

        UpdateCard(selectText, "SELECT", selected);
        UpdateCard(navigateText, "NAVIGATE", navigated);
        UpdateCard(grabText, "GRAB", grabbed);
        UpdateCard(writeText, "WRITE", written);

        bool allCompleted =
            selected && navigated && grabbed && written;

        if (safetyRoomButton != null)
            safetyRoomButton.SetActive(allCompleted);

        Debug.Log("Tutorial feedback displayed.");
    }

    private void FormatCard(TextMeshProUGUI text)
    {
        if (text == null)
            return;

        text.enableAutoSizing = false;
        text.fontSize = activityFontSize;
        text.alignment = TextAlignmentOptions.Center;
        text.enableWordWrapping = true;
        text.raycastTarget = false;

        RectTransform rect =
            text.GetComponent<RectTransform>();

        if (rect != null)
        {
            rect.anchorMin = new Vector2(0, 0);
            rect.anchorMax = new Vector2(1, 1);
            rect.offsetMin = new Vector2(10, 5);
            rect.offsetMax = new Vector2(-10, -5);
        }
    }

    private void UpdateCard(
        TextMeshProUGUI text,
        string activity,
        bool completed)
    {
        if (text == null)
            return;

        FormatCard(text);

        string status = completed
            ? "COMPLETED"
            : "NOT COMPLETED";

        string symbol = completed
            ? "\u2713"
            : "\u25CB";

        string colorHex = completed
            ? "#91E9BC"
            : "#FFFFFF";

        text.text =
            $"<size={activityFontSize}>{activity}</size>\n" +
            $"<size={statusFontSize}><color={colorHex}>" +
            $"{symbol} {status}</color></size>";

        text.color = pendingColor;
    }
}
