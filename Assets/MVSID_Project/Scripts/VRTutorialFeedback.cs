
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class VRTutorialFeedback : MonoBehaviour
{
    [Header("Feedback Panel")]
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
    public GameObject checkProgressButton;

    [Header("Trainee Self-Assessment")]
    public Toggle selectConfidence;
    public Toggle navigateConfidence;
    public Toggle grabConfidence;
    public Toggle writeConfidence;

    [Header("Card Appearance")]
    public float activityFontSize = 28f;
    public float statusFontSize = 21f;

    private bool selected;
    private bool navigated;
    private bool grabbed;
    private bool written;

    private void Start()
    {
        if (welcomeContent != null)
            welcomeContent.SetActive(false);

        ResetConfidence();
    }

    public void CompleteSelect() => selected = true;
    public void CompleteNavigate() => navigated = true;
    public void CompleteGrab() => grabbed = true;
    public void CompleteWrite() => written = true;

   
public void ShowFeedback()
{
    if (welcomeContent != null)
        welcomeContent.SetActive(true);

    ResetConfidence();

    Debug.Log("Feedback self-assessment displayed.");
}

// Called whenever a confidence checkbox changes.
public void UpdateConfidenceButtons()
{
    UpdateCard(selectText, "SELECT",
        selectConfidence != null && selectConfidence.isOn);

    UpdateCard(navigateText, "NAVIGATE",
        navigateConfidence != null && navigateConfidence.isOn);

    UpdateCard(grabText, "GRAB",
        grabConfidence != null && grabConfidence.isOn);

    UpdateCard(writeText, "WRITE",
        writeConfidence != null && writeConfidence.isOn);

    // Any change returns the trainee to the checking stage.
    ShowCheckProgressButton();
}

private void ShowCheckProgressButton()
{
    if (checkProgressButton != null)
        checkProgressButton.SetActive(true);

    if (safetyRoomButton != null)
        safetyRoomButton.SetActive(false);

    if (redoTutorialButton != null)
        redoTutorialButton.SetActive(false);
}

// Called by CHECK MY PROGRESS.
public void CheckMyProgress()
{
    bool allConfident =
        selectConfidence != null && selectConfidence.isOn &&
        navigateConfidence != null && navigateConfidence.isOn &&
        grabConfidence != null && grabConfidence.isOn &&
        writeConfidence != null && writeConfidence.isOn;

    if (checkProgressButton != null)
        checkProgressButton.SetActive(false);

    if (safetyRoomButton != null)
        safetyRoomButton.SetActive(allConfident);

    if (redoTutorialButton != null)
        redoTutorialButton.SetActive(!allConfident);

    Debug.Log(allConfident
        ? "All skills confirmed. Continue to Safety Room."
        : "More practice requested. Redo Tutorial available.");
}

public void RedoTutorial()
{
    Debug.Log("Restarting VR Familiarisation Tutorial.");

    Scene currentScene = SceneManager.GetActiveScene();
    SceneManager.LoadScene(currentScene.buildIndex);
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

private void UpdateCard(
    TextMeshProUGUI text,
    string activity,
    bool confident)
{
    if (text == null)
        return;

    string status = confident
        ? "[OK] COMPLETED"
        : "NOT COMPLETED";

    string color = confident
        ? "#91E9BC"
        : "#FFFFFF";

    text.text =
        $"<size={activityFontSize}>{activity}</size>\n" +
        $"<size={statusFontSize}><color={color}>" +
        $"{status}</color></size>";
}


#if UNITY_EDITOR

    [ContextMenu("Build Feedback Panel in Editor")]
    private void BuildFeedbackPanelInEditor()
    {
        if (Application.isPlaying)
        {
            Debug.LogWarning("Exit Play mode first.");
            return;
        }

        // Prevent accidental duplicate generation.
        Transform existing =
            transform.Find("FeedbackPanel");

        if (existing != null)
        {
            Debug.LogWarning(
                "FeedbackPanel already exists. Edit it in the Hierarchy.");
            return;
        }

        if (safetyRoomButton == null)
        {
            Debug.LogError(
                "Assign your existing SafetyRoomButton before building.");
            return;
        }

        Undo.RegisterFullObjectHierarchyUndo(
            gameObject, "Build Feedback Panel");

        GameObject oldContent = welcomeContent;

        // Create panel under the object containing this script.
        GameObject panel = CreateUI(
            "FeedbackPanel", transform,
            new Vector2(1000, 650),
            Vector2.zero);

        Image panelImage = panel.AddComponent<Image>();
        panelImage.color = new Color32(13, 27, 58, 255);
        panelImage.raycastTarget = false;

        welcomeContent = panel;

        feedbackTitle = CreateText(
            "FeedbackTitle", panel.transform,
            "VR TUTORIAL - YOUR PROGRESS",
            new Vector2(900, 55),
            new Vector2(0, 265),
            36, Color.white);

        CreateText(
            "ConfidenceInstructions", panel.transform,
            "Tick the skills you feel confident performing independently.",
            new Vector2(920, 50),
            new Vector2(0, 205),
            24, new Color32(185, 211, 238, 255));

        CreateCard(
            panel.transform, "SelectCard",
            "SELECT", new Vector2(-240, 85),
            out selectText, out selectConfidence);

        CreateCard(
            panel.transform, "NavigateCard",
            "NAVIGATE", new Vector2(240, 85),
            out navigateText, out navigateConfidence);

        CreateCard(
            panel.transform, "GrabCard",
            "GRAB", new Vector2(-240, -100),
            out grabText, out grabConfidence);

        CreateCard(
            panel.transform, "WriteCard",
            "WRITE", new Vector2(240, -100),
            out writeText, out writeConfidence);

        // Preserve the existing Safety Room button
        // and its configured interaction events.
        Undo.SetTransformParent(
            safetyRoomButton.transform,
            panel.transform,
            "Move Safety Room Button");

        RectTransform safetyRect =
            safetyRoomButton.GetComponent<RectTransform>();

        if (safetyRect != null)
        {
            safetyRect.anchorMin = new Vector2(0.5f, 0.5f);
            safetyRect.anchorMax = new Vector2(0.5f, 0.5f);
            safetyRect.pivot = new Vector2(0.5f, 0.5f);
            safetyRect.anchoredPosition = new Vector2(0, -255);
            safetyRect.localScale = Vector3.one;
            safetyRect.localRotation = Quaternion.identity;
            safetyRect.sizeDelta = new Vector2(420, 65);
        }

        // Create a new Redo button at the same position.
        redoTutorialButton = CreateButton(
            "RedoTutorialButton",
            panel.transform,
            "REDO TUTORIAL",
            new Vector2(0, -255));

        // Standard Unity UI click event.
        Button redoButton =
            redoTutorialButton.GetComponent<Button>();

        redoButton.onClick.AddListener(RedoTutorial);

        // Wire the confidence Toggles.
        selectConfidence.onValueChanged.AddListener(
            delegate { UpdateConfidenceButtons(); });

        navigateConfidence.onValueChanged.AddListener(
            delegate { UpdateConfidenceButtons(); });

        grabConfidence.onValueChanged.AddListener(
            delegate { UpdateConfidenceButtons(); });

        writeConfidence.onValueChanged.AddListener(
            delegate { UpdateConfidenceButtons(); });

        // Disable the old feedback content, but keep it
        // in the Hierarchy as a backup.
        if (oldContent != null && oldContent != panel)
            oldContent.SetActive(false);

        // Show the new panel for editing.
        panel.SetActive(true);

        UpdateCard(selectText, "SELECT", false);
        UpdateCard(navigateText, "NAVIGATE", false);
        UpdateCard(grabText, "GRAB", false);
        UpdateCard(writeText, "WRITE", false);

        ResetConfidence();

        EditorUtility.SetDirty(this);
        EditorUtility.SetDirty(panel);
        UnityEditor.SceneManagement.EditorSceneManager
            .MarkSceneDirty(gameObject.scene);

        Selection.activeGameObject = panel;

        Debug.Log(
            "Editable FeedbackPanel created. Save the scene.");
    }

    [ContextMenu("Preview Feedback Panel in Editor")]
    private void PreviewFeedbackInEditor()
    {
        if (Application.isPlaying)
        {
            ShowFeedback();
            return;
        }

        if (welcomeContent == null)
            return;

        welcomeContent.SetActive(true);

        UpdateCard(selectText, "SELECT", false);
        UpdateCard(navigateText, "NAVIGATE", false);
        UpdateCard(grabText, "GRAB", false);
        UpdateCard(writeText, "WRITE", false);

        ResetConfidence();

        UnityEditor.SceneManagement.EditorSceneManager
            .MarkSceneDirty(gameObject.scene);
    }

    private GameObject CreateUI(
        string objectName,
        Transform parent,
        Vector2 size,
        Vector2 position)
    {
        GameObject obj = new GameObject(
            objectName, typeof(RectTransform));

        obj.transform.SetParent(parent, false);

        RectTransform rect =
            obj.GetComponent<RectTransform>();

        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = size;
        rect.anchoredPosition = position;
        rect.localScale = Vector3.one;

        return obj;
    }

    private TextMeshProUGUI CreateText(
        string objectName,
        Transform parent,
        string content,
        Vector2 size,
        Vector2 position,
        float fontSize,
        Color color)
    {
        GameObject obj =
            CreateUI(objectName, parent, size, position);

        TextMeshProUGUI text =
            obj.AddComponent<TextMeshProUGUI>();

        text.text = content;
        text.fontSize = fontSize;
        text.color = color;
        text.alignment = TextAlignmentOptions.Center;
        text.enableAutoSizing = false;
        text.raycastTarget = false;

        return text;
    }

    private void CreateCard(
        Transform parent,
        string cardName,
        string title,
        Vector2 position,
        out TextMeshProUGUI cardText,
        out Toggle confidenceToggle)
    {
        GameObject card = CreateUI(
            cardName, parent,
            new Vector2(440, 170), position);

        Image background = card.AddComponent<Image>();
        background.color = new Color32(30, 67, 110, 255);
        background.raycastTarget = false;

        cardText = CreateText(
            "ActivityStatus", card.transform,
            title + "\nNOT COMPLETED",
            new Vector2(400, 90),
            new Vector2(0, 25),
            activityFontSize, Color.white);

        GameObject toggleObject = CreateUI(
            title + "ConfidenceToggle",
            card.transform,
            new Vector2(370, 45),
            new Vector2(0, -57));

        confidenceToggle =
            toggleObject.AddComponent<Toggle>();

        confidenceToggle.isOn = false;

        GameObject backgroundObject = CreateUI(
            "Background", toggleObject.transform,
            new Vector2(32, 32),
            new Vector2(-160, 0));

        Image checkboxBackground =
            backgroundObject.AddComponent<Image>();

        checkboxBackground.color = Color.white;

        GameObject checkmarkObject = CreateUI(
            "Checkmark", backgroundObject.transform,
            new Vector2(22, 22),
            Vector2.zero);

        Image checkmark =
            checkmarkObject.AddComponent<Image>();

        checkmark.color = new Color32(25, 140, 195, 255);

        // Use a filled square as the checked indicator.
        // The Toggle controls its visibility.
        confidenceToggle.targetGraphic = checkboxBackground;
        confidenceToggle.graphic = checkmark;

        CreateText(
            "Label", toggleObject.transform,
            "I feel confident",
            new Vector2(310, 40),
            new Vector2(35, 0),
            22, Color.white);
    }

    private GameObject CreateButton(
        string objectName,
        Transform parent,
        string label,
        Vector2 position)
    {
        GameObject obj = CreateUI(
            objectName, parent,
            new Vector2(420, 65), position);

        Image image = obj.AddComponent<Image>();
        image.color = new Color32(31, 117, 202, 255);

        obj.AddComponent<Button>();

        CreateText(
            "Text (TMP)", obj.transform,
            label,
            new Vector2(400, 60),
            Vector2.zero,
            25, Color.white);

        return obj;
    }

#endif
}
