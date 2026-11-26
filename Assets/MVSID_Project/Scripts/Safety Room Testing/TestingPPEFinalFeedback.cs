
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class TestingPPEFinalFeedback : MonoBehaviour
{
    // ==========================================
    // EXISTING ASSESSMENT SYSTEMS
    // ==========================================

    [Header("Assessment Systems")]
    public TestingPPEMatchingController matching;

    public TestingPPETaskPuzzle mannequinPuzzle;

    public GameObject mannequinCanvas;

    // ==========================================
    // TESTING ENVIRONMENT
    // ==========================================

    [Header("Testing Environment")]
    public string testingSceneName =
        "Testing Environment";

    [Header("Completion Status")]
    [SerializeField]
    private bool safetyAssessmentCompleted;

    [SerializeField]
    private bool enteringTestingEnvironment;

    private GameObject resultsPanel;

    private Button enterTestingButton;

    // ==========================================
    // FIND EXISTING REFERENCES
    // ==========================================

    private bool FindReferences()
    {
        if (matching == null)
        {
            matching =
                GetComponent<
                    TestingPPEMatchingController
                >();
        }

        if (mannequinPuzzle == null)
        {
            mannequinPuzzle =
                GetComponent<
                    TestingPPETaskPuzzle
                >();
        }

        if (mannequinCanvas == null)
        {
            Transform found =
                transform.Find(
                    "TestingPPEMannequinCanvas"
                );

            if (found != null)
            {
                mannequinCanvas =
                    found.gameObject;
            }
        }

        if (matching == null ||
            mannequinPuzzle == null ||
            mannequinCanvas == null)
        {
            Debug.LogError(
                "[PPE Completion] Required references " +
                "are missing."
            );

            return false;
        }

        return true;
    }

    // ==========================================
    // VERIFY ALL SAFETY ACTIVITIES
    // ==========================================

    private bool CheckSafetyCompletion()
    {
        if (!FindReferences())
            return false;

        TestingAssessmentManager manager =
            TestingAssessmentManager.Instance;

        if (manager == null)
        {
            Debug.LogError(
                "[ASSESSMENT] Central assessment " +
                "manager was not found."
            );

            return false;
        }

        if (!manager.SessionStarted ||
            manager.SessionCompleted)
        {
            Debug.LogWarning(
                "[ASSESSMENT] No active testing session."
            );

            return false;
        }

        // ======================================
        // VERIFY EQUIPMENT KNOWLEDGE
        // ======================================

        TestingAssessmentManager.TaskRecord
            knowledgeTask =
                manager.FindTask("SAFETY_01");

        if (knowledgeTask == null ||
            !knowledgeTask.completed)
        {
            Debug.LogWarning(
                "[ASSESSMENT] Equipment knowledge " +
                "assessment is incomplete."
            );

            return false;
        }

        // ======================================
        // VERIFY PPE MATCHING
        // ======================================

        if (!matching.IsComplete)
        {
            Debug.LogWarning(
                "[ASSESSMENT] PPE matching " +
                "challenge is incomplete."
            );

            return false;
        }

        // ======================================
        // VERIFY BOTH MANNEQUIN SCENARIOS
        // ======================================

        if (!mannequinPuzzle.IsAssessmentComplete)
        {
            Debug.LogWarning(
                "[ASSESSMENT] Mannequin assessment " +
                "is incomplete."
            );

            return false;
        }

        // ======================================
        // SYNCHRONISE CENTRAL ASSESSMENT
        // ======================================

        matching.SaveAssessmentResults();

        mannequinPuzzle.SaveAssessmentResults();

        TestingAssessmentManager.TaskRecord
            matchingTask =
                manager.FindTask("SAFETY_02");

        TestingAssessmentManager.TaskRecord
            mannequinTask =
                manager.FindTask("SAFETY_03");

        if (matchingTask == null ||
            !matchingTask.completed ||
            mannequinTask == null ||
            !mannequinTask.completed)
        {
            Debug.LogError(
                "[ASSESSMENT] Safety results " +
                "could not be synchronised."
            );

            return false;
        }

        safetyAssessmentCompleted = true;

        return true;
    }

    // ==========================================
    // SHOW SAFETY COMPLETION PANEL
    // ==========================================

    public void ShowResults()
    {
        if (enteringTestingEnvironment)
            return;

        safetyAssessmentCompleted = false;

        if (!CheckSafetyCompletion())
            return;

        Transform parent =
            mannequinCanvas.transform.Find(
                "MannequinPanel"
            );

        if (parent == null)
        {
            Debug.LogError(
                "MannequinPanel not found."
            );

            return;
        }

        if (resultsPanel != null)
        {
            resultsPanel.SetActive(false);

            Destroy(resultsPanel);
        }

        // ======================================
        // CREATE COMPLETION PANEL
        // ======================================

        resultsPanel = new GameObject(
            "FinalFeedbackPanel",
            typeof(RectTransform),
            typeof(Image)
        );

        resultsPanel.transform.SetParent(
            parent,
            false
        );

        RectTransform rect =
            resultsPanel.GetComponent<
                RectTransform
            >();

        rect.anchorMin = Vector2.zero;

        rect.anchorMax = Vector2.one;

        rect.offsetMin = Vector2.zero;

        rect.offsetMax = Vector2.zero;

        Image background =
            resultsPanel.GetComponent<Image>();

        // Preserve the existing Safety Room colour.

        background.color =
            new Color32(
                120, 150, 166, 255
            );

        background.raycastTarget = true;

        // ======================================
        // MAIN TITLE
        // ======================================

        AddText(
            resultsPanel.transform,

            "Title",

            "SAFETY ASSESSMENT COMPLETE",

            38,

            0, 320,

            1000, 70,

            Color.white,

            true
        );

        AddText(
            resultsPanel.transform,

            "Description",

            "Your Safety Room assessment " +
            "has been recorded.",

            25,

            0, 245,

            960, 65,

            Color.white
        );

        // ======================================
        // RETRIEVE ACTUAL COMPLETION RESULTS
        // ======================================

        TestingAssessmentManager manager =
            TestingAssessmentManager.Instance;

        TestingAssessmentManager.TaskRecord
            knowledgeTask =
                manager.FindTask("SAFETY_01");

        int questionsAnswered =
            knowledgeTask.answers.Count;

        int totalQuestions =
            knowledgeTask.totalAnswers;

        // ======================================
        // EQUIPMENT KNOWLEDGE
        // ======================================

        CreateCompletionRow(

            "EquipmentKnowledgeRow",

            "EQUIPMENT KNOWLEDGE",

            questionsAnswered +
            " / " +
            totalQuestions +
            " QUESTIONS ANSWERED",

            125
        );

        // ======================================
        // PPE MATCHING
        // ======================================

        CreateCompletionRow(

            "MatchingRow",

            "PPE MATCHING CHALLENGE",

            matching.MatchCount +
            " / " +
            matching.totalEquipment +
            " ITEMS MATCHED",

            25
        );

        // ======================================
        // MANNEQUIN SCENARIOS
        // ======================================

        CreateCompletionRow(

            "MannequinRow",

            "MANNEQUIN PPE SCENARIOS",

            mannequinPuzzle.CompletedScenarios +
            " / " +
            mannequinPuzzle.TotalScenarios +
            " SCENARIOS COMPLETED",

            -75
        );

        // ======================================
        // NEXT STAGE INSTRUCTIONS
        // ======================================

        AddText(
            resultsPanel.transform,

            "NextStageHeading",

            "NEXT STAGE: PIPELINE INSPECTION",

            27,

            0, -180,

            970, 50,

            new Color32(
                255, 213, 100, 255
            ),

            true
        );

        AddText(
            resultsPanel.transform,

            "NextStageDescription",

            "Continue to the inspection facility.\n" +
            "Your complete assessment results will " +
            "be available at the end of the experience.",

            23,

            0, -245,

            940, 85,

            Color.white
        );

        // ======================================
        // ENTER TESTING ENVIRONMENT BUTTON
        // ======================================

        CreateEnterTestingButton();

        resultsPanel.transform.SetAsLastSibling();

        Debug.Log(
            "[ASSESSMENT] Safety Room assessment " +
            "completed and recorded."
        );

        // No score or mistake details
        // are displayed to the trainee.
    }

    // ==========================================
    // CREATE COMPLETION ROW
    // ==========================================

    private void CreateCompletionRow(
        string objectName,
        string heading,
        string description,
        float y)
    {
        GameObject rowObject = new GameObject(
            objectName,
            typeof(RectTransform),
            typeof(Image)
        );

        rowObject.transform.SetParent(
            resultsPanel.transform,
            false
        );

        RectTransform rect =
            rowObject.GetComponent<RectTransform>();

        rect.sizeDelta =
            new Vector2(960, 86);

        rect.anchoredPosition =
            new Vector2(0, y);

        Image background =
            rowObject.GetComponent<Image>();

        background.color =
            new Color32(
                73, 107, 126, 255
            );

        background.raycastTarget = false;

        // Activity title.

        TextMeshProUGUI title = AddText(
            rowObject.transform,

            "ActivityTitle",

            heading,

            25,

            -120, 16,

            660, 35,

            Color.white,

            true
        );

        title.alignment =
            TextAlignmentOptions.Left;

        // Activity completion progress.

        TextMeshProUGUI progress = AddText(
            rowObject.transform,

            "ActivityProgress",

            description,

            20,

            -120, -18,

            660, 30,

            new Color32(
                225, 235, 240, 255
            )
        );

        progress.alignment =
            TextAlignmentOptions.Left;

        // Completion status only.
        // Does not indicate accuracy or pass/fail.

        AddText(
            rowObject.transform,

            "CompletionStatus",

            "COMPLETED",

            22,

            340, 0,

            220, 45,

            Color.white,

            true
        );
    }

    // ==========================================
    // CREATE ENTER TESTING BUTTON
    // ==========================================

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
            resultsPanel.transform,
            false
        );

        RectTransform rect =
            buttonObject.GetComponent<
                RectTransform
            >();

        rect.sizeDelta =
            new Vector2(600, 65);

        rect.anchoredPosition =
            new Vector2(0, -345);

        Image background =
            buttonObject.GetComponent<Image>();

        background.color =
            new Color32(
                36, 80, 107, 255
            );

        background.raycastTarget = true;

        // Preserve collider-based interaction.

        BoxCollider boxCollider =
            buttonObject.GetComponent<
                BoxCollider
            >();

        boxCollider.center =
            Vector3.zero;

        boxCollider.size =
            new Vector3(
                600, 65, 5
            );

        enterTestingButton =
            buttonObject.GetComponent<Button>();

        enterTestingButton.targetGraphic =
            background;

        enterTestingButton.onClick.AddListener(
            EnterTestingEnvironment
        );

        ColorBlock colors =
            enterTestingButton.colors;

        colors.normalColor =
            Color.white;

        colors.highlightedColor =
            new Color32(
                200, 225, 240, 255
            );

        colors.pressedColor =
            new Color32(
                160, 195, 220, 255
            );

        enterTestingButton.colors = colors;

        // Button label.

        AddText(
            buttonObject.transform,

            "ButtonText",

            "ENTER TESTING ENVIRONMENT",

            25,

            0, 0,

            585, 60,

            Color.white,

            true
        );
    }

    // ==========================================
    // ENTER TESTING ENVIRONMENT
    // ==========================================

    public void EnterTestingEnvironment()
    {
        // Prevent duplicate scene transitions.

        if (enteringTestingEnvironment)
            return;

        if (resultsPanel == null ||
            !safetyAssessmentCompleted)
        {
            Debug.LogWarning(
                "Complete the Safety Room " +
                "assessment before entering."
            );

            return;
        }

        // Recheck all assessment results
        // before leaving the Safety Room.

        if (!CheckSafetyCompletion())
            return;

        if (string.IsNullOrWhiteSpace(
            testingSceneName))
        {
            Debug.LogError(
                "Testing Environment scene name " +
                "is empty."
            );

            return;
        }

        if (!Application.CanStreamedLevelBeLoaded(
            testingSceneName))
        {
            Debug.LogError(
                "Testing scene is not included " +
                "in the active Build Profile, " +
                "or its name is incorrect: " +
                testingSceneName
            );

            return;
        }

        TestingAssessmentManager manager =
            TestingAssessmentManager.Instance;

        if (manager == null)
            return;

        enteringTestingEnvironment = true;

        if (enterTestingButton != null)
        {
            enterTestingButton.interactable =
                false;
        }

        // ======================================
        // SAFETY ROOM RESULTS ARE PRESERVED
        // ======================================

        Debug.Log(
            "[ASSESSMENT] Safety Room results saved."
        );

        Debug.Log(
            "[ASSESSMENT] Trainee: " +
            manager.TraineeName
        );

        Debug.Log(
            "[ASSESSMENT] Completed tasks: " +
            manager.GetCompletedTaskCount() +
            " / " +
            manager.GetTotalTaskCount()
        );

        Debug.Log(
            "[ASSESSMENT] Current session time: " +
            manager.GetFormattedDuration()
        );

        // IMPORTANT:
        // Do not call FinishSession().
        // Do not reset the assessment manager.
        //
        // The same assessment session continues
        // inside the Testing Environment.

        Debug.Log(
            "Entering Testing Environment: " +
            testingSceneName
        );

        SceneManager.LoadScene(
            testingSceneName
        );
    }

    // ==========================================
    // TEXT CREATION HELPER
    // ==========================================

    private TextMeshProUGUI AddText(
        Transform parent,
        string objectName,
        string message,
        float fontSize,
        float x,
        float y,
        float width,
        float height,
        Color color,
        bool bold = false)
    {
        GameObject obj = new GameObject(

            objectName,

            typeof(RectTransform),

            typeof(TextMeshProUGUI)
        );

        obj.transform.SetParent(
            parent,
            false
        );

        RectTransform rect =
            obj.GetComponent<RectTransform>();

        rect.sizeDelta =
            new Vector2(
                width,
                height
            );

        rect.anchoredPosition =
            new Vector2(
                x,
                y
            );

        TextMeshProUGUI text =
            obj.GetComponent<
                TextMeshProUGUI
            >();

        text.text = message;

        text.fontSize = fontSize;

        text.color = color;

        text.alignment =
            TextAlignmentOptions.Center;

        text.fontStyle =
            bold
                ? FontStyles.Bold
                : FontStyles.Normal;

        text.enableWordWrapping = true;

        text.enableAutoSizing = true;

        text.fontSizeMin = 12;

        text.fontSizeMax = fontSize;

        text.raycastTarget = false;

        return text;
    }
}
