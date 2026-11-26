
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class TestingPPEMatchingController : MonoBehaviour
{
    [Header("Puzzle Progress")]
    public int totalEquipment = 10;

    [Header("Optional Audio")]
    public AudioSource feedbackAudio;
    public AudioClip correctSound;
    public AudioClip wrongSound;

    private Transform panel;
    private TextMeshProUGUI feedbackText;
    private TextMeshProUGUI progressText;

    private readonly bool[] matched = new bool[10];

    private int selectedImage = -1;
    private int matchCount = 0;
    private int mistakes = 0;

    // ==========================================
    // ASSESSMENT RECORDS
    // ==========================================

    private readonly List<string> matchingMistakeDetails =
        new List<string>();

    public IReadOnlyList<string> MatchingMistakeDetails
        => matchingMistakeDetails;

    private readonly string[] equipmentNames =
    {
        "Safety Helmet",
        "Safety Glasses",
        "Safety Gloves",
        "Safety Vest",
        "Safety Boots",
        "Coveralls",
        "Ear Muffs",
        "Gas Mask",
        "Face Shield",
        "Ear Plugs"
    };

    // Existing shuffled equipment names.

    private readonly int[] nameEquipmentIDs =
    {
        8, 4, 6, 0, 5, 9, 2, 7, 3, 1
    };

    // ==========================================
    // PUBLIC ASSESSMENT RESULTS
    // ==========================================

    public bool IsComplete => matchCount == 10;

    public int MatchCount => matchCount;

    public int Mistakes => mistakes;

    public int TotalAttempts => matchCount + mistakes;

    public float MatchingAccuracy
    {
        get
        {
            if (TotalAttempts == 0)
                return 0f;

            return 100f * matchCount / TotalAttempts;
        }
    }

    // ==========================================
    // INITIALISATION
    // ==========================================

    private void Start()
    {
        Transform canvas =
            transform.Find("TestingPPEPuzzleCanvas");

        if (canvas == null)
        {
            Debug.LogError(
                "TestingPPEPuzzleCanvas was not found."
            );

            return;
        }

        panel = canvas.Find("PuzzlePanel");

        if (panel == null)
        {
            Debug.LogError(
                "PuzzlePanel was not found."
            );

            return;
        }

        feedbackText = panel.Find("FeedbackText")
            ?.GetComponent<TextMeshProUGUI>();

        progressText = panel.Find("ProgressText")
            ?.GetComponent<TextMeshProUGUI>();

        TestingPPEPuzzlePanelBuilder builder =
            GetComponent<TestingPPEPuzzlePanelBuilder>();

        for (int i = 0; i < 10; i++)
        {
            Transform imageButton =
                panel.Find("PPEImage_" + i);

            Transform nameButton =
                panel.Find("PPEName_" + i);

            if (imageButton == null ||
                nameButton == null)
            {
                Debug.LogError(
                    "Missing puzzle button at index: " + i
                );

                continue;
            }

            // Preserve existing PPE image assignments.

            if (builder != null &&
                builder.ppeImages != null &&
                i < builder.ppeImages.Length)
            {
                Transform picture =
                    imageButton.Find("EquipmentImage");

                if (picture != null &&
                    builder.ppeImages[i] != null)
                {
                    Image image =
                        picture.GetComponent<Image>();

                    if (image != null)
                    {
                        image.sprite =
                            builder.ppeImages[i];

                        image.color = Color.white;
                        image.preserveAspect = true;
                        image.raycastTarget = false;
                    }
                }
            }

            SetupButton(imageButton, true, i);
            SetupButton(nameButton, false, i);
        }

        UpdateProgress();

        ShowFeedback(
            "Select a PPE image, then match its name.",
            Color.white
        );
    }

    // ==========================================
    // EXISTING DESKTOP AND EZPZ INTERACTIONS
    // ==========================================

    private void SetupButton(
        Transform buttonTransform,
        bool isImage,
        int index)
    {
        Button button =
            buttonTransform.GetComponent<Button>();

        if (button == null)
        {
            Debug.LogError(
                "Button component missing on " +
                buttonTransform.name
            );

            return;
        }

        TestingPPEMatchButton relay =
            buttonTransform.GetComponent<
                TestingPPEMatchButton
            >();

        if (relay == null)
        {
            relay = buttonTransform.gameObject
                .AddComponent<TestingPPEMatchButton>();
        }

        relay.controller = this;
        relay.isImage = isImage;
        relay.index = index;

        // Preserve existing desktop interaction.

        button.onClick.RemoveListener(relay.Activate);
        button.onClick.AddListener(relay.Activate);
    }

    // ==========================================
    // SELECT PPE IMAGE
    // ==========================================

    public void SelectImage(int id)
    {
        if (IsComplete ||
            id < 0 ||
            id >= 10)
        {
            return;
        }

        if (matched[id])
            return;

        selectedImage = id;

        ShowFeedback(
            "Image selected. Now choose its name.",
            Color.white
        );
    }

    // ==========================================
    // SELECT EQUIPMENT NAME
    // ==========================================

    public void SelectName(int nameIndex)
    {
        if (IsComplete ||
            nameIndex < 0 ||
            nameIndex >= 10)
        {
            return;
        }

        int equipmentID =
            nameEquipmentIDs[nameIndex];

        if (matched[equipmentID])
            return;

        if (selectedImage == -1)
        {
            ShowFeedback(
                "Select an equipment image first.",
                Color.white
            );

            return;
        }

        int imageID = selectedImage;

        bool isCorrect =
            imageID == equipmentID;

        // ======================================
        // RECORD SUCCESSFUL MATCH
        // ======================================

        if (isCorrect)
        {
            matched[equipmentID] = true;

            matchCount++;

            LockMatchedButton(
                "PPEImage_" + equipmentID
            );

            LockMatchedButton(
                "PPEName_" + nameIndex
            );

            // No explicit correctness feedback.

            ShowFeedback(
                "Selection recorded. Continue matching.",
                Color.white
            );
        }

        // ======================================
        // RECORD INCORRECT ATTEMPT
        // ======================================

        else
        {
            mistakes++;

            string incorrectMatch =
                equipmentNames[imageID] +
                " was incorrectly matched with " +
                equipmentNames[equipmentID] + ".";

            matchingMistakeDetails.Add(
                incorrectMatch
            );

            // Record internally.
            // Do not display incorrect answer feedback.

            Debug.Log(
                "[PPE Matching Mistake] " +
                incorrectMatch
            );

            ShowFeedback(
                "Selection recorded. Continue matching.",
                Color.white
            );
        }

        // ======================================
        // RECORD INDIVIDUAL MATCHING ATTEMPT
        // ======================================

        TestingAssessmentManager manager =
            TestingAssessmentManager.Instance;

        if (manager != null)
        {
            manager.RecordAnswer(
                "SAFETY_02",

                "Matching attempt " +
                TotalAttempts +
                ": Identify " +
                equipmentNames[imageID],

                equipmentNames[equipmentID],

                equipmentNames[imageID],

                isCorrect,

                isCorrect
                    ? ""
                    : "Review the appearance and purpose of " +
                      equipmentNames[imageID] + "."
            );
        }

        selectedImage = -1;

        UpdateProgress();

        // Update central assessment after every attempt.

        SaveAssessmentResults();

        // ======================================
        // CHALLENGE COMPLETED
        // ======================================

        if (IsComplete)
        {
            ShowFeedback(
                "CHALLENGE 1 COMPLETE! " +
                "Proceed to the next activity.",
                Color.white
            );

            Debug.Log(
                "[ASSESSMENT] PPE Matching completed."
            );

            Debug.Log(
                "[ASSESSMENT] Correct matches: " +
                matchCount +
                " | Incorrect attempts: " +
                mistakes +
                " | Accuracy: " +
                MatchingAccuracy.ToString("F1") +
                "%"
            );
        }
    }

    // ==========================================
    // SAVE CENTRAL ASSESSMENT RESULTS
    // ==========================================

    public void SaveAssessmentResults()
    {
        TestingAssessmentManager manager =
            TestingAssessmentManager.Instance;

        if (manager == null)
        {
            Debug.LogWarning(
                "[ASSESSMENT] Central assessment " +
                "manager was not found."
            );

            return;
        }

        manager.RecordTaskResult(
            "SAFETY_02",

            matchCount,

            IsComplete
                ? TotalAttempts
                : 10 + mistakes,

            mistakes,

            IsComplete,

            mistakes == 0
                ? ""
                : "Review the appearance and purpose " +
                  "of incorrectly matched PPE items.",

            matchingMistakeDetails
        );
    }

    // ==========================================
    // LOCK COMPLETED MATCH
    // ==========================================

    private void LockMatchedButton(
        string objectName)
    {
        if (panel == null)
            return;

        Transform target =
            panel.Find(objectName);

        if (target == null)
            return;

        Button button =
            target.GetComponent<Button>();

        if (button != null)
        {
            button.interactable = false;
        }

        Image background =
            target.GetComponent<Image>();

        if (background != null)
        {
            // Neutral grey for completed items.
            // No green correctness indicator.

            background.color =
                new Color32(95, 105, 110, 255);
        }
    }

    // ==========================================
    // UPDATE TRAINEE PROGRESS
    // ==========================================

    private void UpdateProgress()
    {
        if (progressText == null)
            return;

        // Display completion progress only.
        // Hide mistakes until final assessment.

        progressText.text =
            "MATCHED: " +
            matchCount +
            " / 10";
    }

    // ==========================================
    // NEUTRAL STATUS MESSAGE
    // ==========================================

    private void ShowFeedback(
        string message,
        Color color)
    {
        if (feedbackText != null)
        {
            feedbackText.text = message;
            feedbackText.color = color;
        }

        Debug.Log(
            "[PPE Matching] " + message
        );
    }
}
