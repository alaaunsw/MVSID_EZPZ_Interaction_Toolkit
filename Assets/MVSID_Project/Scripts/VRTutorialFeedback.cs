
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
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

    [Header("Navigation Buttons")]
    public GameObject safetyRoomButton;
    public GameObject redoTutorialButton;

    [Header("Trainee Self-Assessment")]
    public Toggle selectConfidence;
    public Toggle navigateConfidence;
    public Toggle grabConfidence;
    public Toggle writeConfidence;

    [Header("Card Appearance")]
    public float activityFontSize = 28f;
    public float statusFontSize = 21f;

    // Actual activity completion tracking.
    private bool selected;
    private bool navigated;
    private bool grabbed;
    private bool written;

    private readonly Color pendingColor = Color.white;

    private void Start()
    {
        FormatCard(selectText);
        FormatCard(navigateText);
        FormatCard(grabText);
        FormatCard(writeText);

        ResetConfidence();

        if (welcomeContent != null)
            welcomeContent.SetActive(false);
    }

    // Called by the existing tutorial activities.
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
    
#if UNITY_EDITOR
[ContextMenu("Preview Feedback Panel")]
private void PreviewFeedbackPanel()
{
    if (Application.isPlaying)
    {
        ShowFeedback();
    }
}
#endif


    // Opens the feedback panel after the tutorial.
    public void ShowFeedback()
    {
        if (welcomeContent != null)
            welcomeContent.SetActive(true);

        if (feedbackTitle != null)
        {
            feedbackTitle.gameObject.SetActive(true);
            feedbackTitle.text =
                "VR TUTORIAL - YOUR PROGRESS";

            feedbackTitle.fontSize = 34f;
            feedbackTitle.alignment =
                TextAlignmentOptions.Center;
        }

        UpdateCard(selectText, "SELECT", selected);
        UpdateCard(navigateText, "NAVIGATE", navigated);
        UpdateCard(grabText, "GRAB", grabbed);
        UpdateCard(writeText, "WRITE", written);

        // Begin self-assessment with no boxes selected.
        ResetConfidence();

        Debug.Log("Tutorial feedback displayed.");
    }

    // Connect this to each Toggle's On Value Changed.
    public void UpdateConfidenceButtons()
    {
        bool allConfident =
            selectConfidence != null &&
            selectConfidence.isOn &&
            navigateConfidence != null &&
            navigateConfidence.isOn &&
            grabConfidence != null &&
            grabConfidence.isOn &&
            writeConfidence != null &&
            writeConfidence.isOn;

        if (safetyRoomButton != null)
            safetyRoomButton.SetActive(allConfident);

        if (redoTutorialButton != null)
            redoTutorialButton.SetActive(!allConfident);
    }

    private void ResetConfidence()
    {
        if (selectConfidence != null)
            selectConfidence.SetIsOnWithoutNotify(false);

        if (navigateConfidence != null)
            navigateConfidence.SetIsOnWithoutNotify(false);

        if (grabConfidence != null)
            grabConfidence.SetIsOnWithoutNotify(false);

        if (writeConfidence != null)
            writeConfidence.SetIsOnWithoutNotify(false);

        UpdateConfidenceButtons();
    }

    // Connect this to the Redo Tutorial button.
    public void RedoTutorial()
    {
        Debug.Log("Restarting VR Familiarisation Tutorial.");

        Scene currentScene = SceneManager.GetActiveScene();

        SceneManager.LoadScene(currentScene.buildIndex);
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

        // Use text symbols supported by standard TMP fonts.
        string symbol = completed
            ? "[OK]"
            : "[ ]";

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
