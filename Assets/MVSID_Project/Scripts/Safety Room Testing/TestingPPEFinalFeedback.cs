
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Text;

public class TestingPPEFinalFeedback : MonoBehaviour
{
    [Header("Assessment Systems")]
    public TestingPPEMatchingController matching;
    public TestingPPETaskPuzzle mannequinPuzzle;
    public GameObject mannequinCanvas;

    [Header("Testing Environment")]
    public string testingSceneName = "Testing Environment";

    private GameObject resultsPanel;
    private Button enterTestingButton;

    public void ShowResults()
    {
        if (matching == null)
            matching =
                GetComponent<TestingPPEMatchingController>();

        if (mannequinPuzzle == null)
            mannequinPuzzle =
                GetComponent<TestingPPETaskPuzzle>();

        if (mannequinCanvas == null)
        {
            Transform found =
                transform.Find("TestingPPEMannequinCanvas");

            if (found != null)
                mannequinCanvas = found.gameObject;
        }

        if (matching == null ||
            mannequinPuzzle == null ||
            mannequinCanvas == null)
        {
            Debug.LogError(
                "PPE final feedback references are missing."
            );
            return;
        }

        Transform parent =
            mannequinCanvas.transform.Find("MannequinPanel");

        if (parent == null)
        {
            Debug.LogError("MannequinPanel not found.");
            return;
        }

        if (resultsPanel != null)
        {
            Destroy(resultsPanel);
        }

        resultsPanel = new GameObject(
            "FinalFeedbackPanel",
            typeof(RectTransform),
            typeof(Image)
        );

        resultsPanel.transform.SetParent(parent, false);

        RectTransform rect =
            resultsPanel.GetComponent<RectTransform>();

        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        Image background =
            resultsPanel.GetComponent<Image>();

        background.color =
            new Color32(120, 150, 166, 255);

        background.raycastTarget = true;

        int matchingErrors = matching.Mistakes;

        int mannequinErrors =
            mannequinPuzzle.MannequinMistakeDetails.Count;

        int totalErrors =
            matchingErrors + mannequinErrors;

        // Main title
        AddText(
            "Title",
            "PPE ASSESSMENT FEEDBACK",
            36, 0, 330, 1000, 60,
            Color.white
        );

        // Assessment summary
        AddText(
            "Summary",
            "PPE MATCHING: 10 / 10 COMPLETED\n" +
            "MANNEQUIN SCENARIOS: 2 / 2 COMPLETED\n" +
            "MATCHING MISTAKES: " + matchingErrors + "\n" +
            "MANNEQUIN MISTAKES: " + mannequinErrors + "\n" +
            "TOTAL INCORRECT ATTEMPTS: " + totalErrors,
            24, 0, 205, 980, 180,
            Color.white
        );

        // Mistake review heading
        AddText(
            "ReviewHeading",
            "EQUIPMENT TO REVIEW",
            27, 0, 80, 960, 45,
            new Color32(255, 205, 100, 255)
        );

        StringBuilder review =
            new StringBuilder();

        foreach (string detail in
                 matching.MatchingMistakeDetails)
        {
            review.AppendLine("• " + detail);
        }

        foreach (string detail in
                 mannequinPuzzle.MannequinMistakeDetails)
        {
            review.AppendLine("• " + detail);
        }

        if (review.Length == 0)
        {
            review.AppendLine(
                "No incorrect selections were recorded."
            );
        }

        // Scrollable area for detailed mistakes
        CreateMistakeScrollArea(
            review.ToString()
        );

        // Improvement heading
        AddText(
            "ImprovementHeading",
            "HOW TO IMPROVE",
            26, 0, -200, 960, 42,
            new Color32(255, 205, 100, 255)
        );

        string improvement;

        if (totalErrors == 0)
        {
            improvement =
                "Excellent work! You completed both challenges " +
                "without incorrect attempts. Continue practising " +
                "PPE recognition and task-specific hazard assessment.";
        }
        else
        {
            improvement =
                "Review the equipment listed above. Practise " +
                "identifying each PPE item by its appearance " +
                "and purpose. Before selecting PPE, identify " +
                "the workplace hazards and follow site requirements.";
        }

        AddText(
            "ImprovementText",
            improvement,
            20, 0, -265, 960, 85,
            Color.white
        );

        // Button to continue to inspection testing
        CreateEnterTestingButton();

        resultsPanel.transform.SetAsLastSibling();

        Debug.Log(
            "[PPE Final Feedback] Results panel displayed. " +
            "Total mistakes: " + totalErrors
        );
    }

    private void CreateMistakeScrollArea(string content)
    {
        GameObject scrollObject = new GameObject(
            "MistakeScrollArea",
            typeof(RectTransform),
            typeof(Image),
            typeof(ScrollRect)
        );

        scrollObject.transform.SetParent(
            resultsPanel.transform, false
        );

        RectTransform scrollRect =
            scrollObject.GetComponent<RectTransform>();

        scrollRect.sizeDelta =
            new Vector2(970, 175);

        scrollRect.anchoredPosition =
            new Vector2(0, -65);

        Image scrollBackground =
            scrollObject.GetComponent<Image>();

        scrollBackground.color =
            new Color32(36, 80, 107, 255);

        GameObject viewport = new GameObject(
            "Viewport",
            typeof(RectTransform),
            typeof(RectMask2D)
        );

        viewport.transform.SetParent(
            scrollObject.transform, false
        );

        RectTransform viewportRect =
            viewport.GetComponent<RectTransform>();

        viewportRect.anchorMin = Vector2.zero;
        viewportRect.anchorMax = Vector2.one;
        viewportRect.offsetMin = new Vector2(12, 8);
        viewportRect.offsetMax = new Vector2(-12, -8);

        GameObject contentObject = new GameObject(
            "Content",
            typeof(RectTransform),
            typeof(TextMeshProUGUI)
        );

        contentObject.transform.SetParent(
            viewport.transform, false
        );

        RectTransform contentRect =
            contentObject.GetComponent<RectTransform>();

        contentRect.anchorMin = new Vector2(0, 1);
        contentRect.anchorMax = new Vector2(1, 1);
        contentRect.pivot = new Vector2(0.5f, 1);
        contentRect.anchoredPosition = Vector2.zero;

        TextMeshProUGUI text =
            contentObject.GetComponent<TextMeshProUGUI>();

        text.text = content;
        text.fontSize = 19;
        text.color = Color.white;
        text.alignment =
            TextAlignmentOptions.TopLeft;

        text.enableWordWrapping = true;
        text.raycastTarget = false;

        // Set the height according to the text.
        float preferredHeight =
            text.GetPreferredValues(
                content,
                920,
                Mathf.Infinity
            ).y;

        contentRect.sizeDelta = new Vector2(
            0,
            Mathf.Max(160, preferredHeight + 20)
        );

        ScrollRect scrolling =
            scrollObject.GetComponent<ScrollRect>();

        scrolling.viewport = viewportRect;
        scrolling.content = contentRect;
        scrolling.horizontal = false;
        scrolling.vertical = true;
        scrolling.movementType =
            ScrollRect.MovementType.Clamped;
        scrolling.scrollSensitivity = 25;
    }

    private void CreateEnterTestingButton()
    {
        GameObject buttonObject = new GameObject(
            "EnterTestingEnvironmentButton",
            typeof(RectTransform),
            typeof(Image),
            typeof(Button),
            typeof(BoxCollider)
        );

        buttonObject.transform.SetParent(
            resultsPanel.transform, false
        );

        RectTransform rect =
            buttonObject.GetComponent<RectTransform>();

        rect.sizeDelta = new Vector2(480, 58);
        rect.anchoredPosition =
            new Vector2(0, -345);

        Image background =
            buttonObject.GetComponent<Image>();

        background.color =
            new Color32(36, 80, 107, 255);

        BoxCollider boxCollider =
            buttonObject.GetComponent<BoxCollider>();

        boxCollider.center = Vector3.zero;
        boxCollider.size =
            new Vector3(480, 58, 5);

        enterTestingButton =
            buttonObject.GetComponent<Button>();

        enterTestingButton.onClick.AddListener(
            EnterTestingEnvironment
        );

        GameObject labelObject = new GameObject(
            "ButtonText",
            typeof(RectTransform),
            typeof(TextMeshProUGUI)
        );

        labelObject.transform.SetParent(
            buttonObject.transform, false
        );

        RectTransform labelRect =
            labelObject.GetComponent<RectTransform>();

        labelRect.sizeDelta =
            new Vector2(465, 52);

        TextMeshProUGUI label =
            labelObject.GetComponent<TextMeshProUGUI>();

        label.text = "ENTER TESTING ENVIRONMENT";
        label.fontSize = 23;
        label.color = Color.white;
        label.alignment =
            TextAlignmentOptions.Center;
        label.raycastTarget = false;
    }

    public void EnterTestingEnvironment()
    {
        if (resultsPanel == null)
        {
            Debug.LogWarning(
                "Complete the PPE assessment before entering."
            );
            return;
        }

        if (string.IsNullOrWhiteSpace(testingSceneName))
        {
            Debug.LogError(
                "Testing Environment scene name is empty."
            );
            return;
        }

        if (!Application.CanStreamedLevelBeLoaded(
                testingSceneName))
        {
            Debug.LogError(
                "Testing scene is not included in the " +
                "active Build Profile, or its name is wrong: " +
                testingSceneName
            );
            return;
        }

        Debug.Log(
            "Entering Testing Environment: " +
            testingSceneName
        );

        SceneManager.LoadScene(testingSceneName);
    }

    private void AddText(
        string objectName,
        string message,
        float fontSize,
        float x,
        float y,
        float width,
        float height,
        Color color)
    {
        GameObject obj = new GameObject(
            objectName,
            typeof(RectTransform),
            typeof(TextMeshProUGUI)
        );

        obj.transform.SetParent(
            resultsPanel.transform, false
        );

        RectTransform rect =
            obj.GetComponent<RectTransform>();

        rect.sizeDelta =
            new Vector2(width, height);

        rect.anchoredPosition =
            new Vector2(x, y);

        TextMeshProUGUI text =
            obj.GetComponent<TextMeshProUGUI>();

        text.text = message;
        text.fontSize = fontSize;
        text.color = color;
        text.alignment =
            TextAlignmentOptions.Center;

        text.enableWordWrapping = true;
        text.enableAutoSizing = true;
        text.fontSizeMin = 12;
        text.fontSizeMax = fontSize;
        text.raycastTarget = false;
    }
}
