
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

    // NEW: Records exactly which PPE items
    // the trainee confused during matching.
    private readonly List<string> matchingMistakeDetails =
        new List<string>();

    public IReadOnlyList<string> MatchingMistakeDetails
        => matchingMistakeDetails;

    private readonly string[] equipmentNames =
    {
        "Safety Helmet",   // 0
        "Safety Glasses",  // 1
        "Safety Gloves",   // 2
        "Safety Vest",     // 3
        "Safety Boots",    // 4
        "Coveralls",       // 5
        "Ear Muffs",       // 6
        "Gas Mask",        // 7
        "Face Shield",     // 8
        "Ear Plugs"        // 9
    };

    // Matches the shuffled names in your
    // existing TestingPPEPuzzlePanelBuilder.
    private readonly int[] nameEquipmentIDs =
    {
        8, 4, 6, 0, 5, 9, 2, 7, 3, 1
    };

    public bool IsComplete => matchCount == 10;
    public int MatchCount => matchCount;
    public int Mistakes => mistakes;

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

            // Assign PPE images from the builder.
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
            "Select a PPE image, then match its correct name.",
            Color.white
        );
    }

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
            buttonTransform.GetComponent<TestingPPEMatchButton>();

        if (relay == null)
        {
            relay = buttonTransform.gameObject
                .AddComponent<TestingPPEMatchButton>();
        }

        relay.controller = this;
        relay.isImage = isImage;
        relay.index = index;

        // Preserve the existing desktop interaction.
        button.onClick.RemoveListener(relay.Activate);
        button.onClick.AddListener(relay.Activate);
    }

    public void SelectImage(int id)
    {
        if (IsComplete || id < 0 || id >= 10)
            return;

        if (matched[id])
            return;

        selectedImage = id;

        ShowFeedback(
            "Image selected. Now choose its correct name.",
            Color.white
        );
    }

    public void SelectName(int nameIndex)
    {
        if (IsComplete ||
            nameIndex < 0 ||
            nameIndex >= 10)
            return;

        int equipmentID =
            nameEquipmentIDs[nameIndex];

        if (matched[equipmentID])
            return;

        if (selectedImage == -1)
        {
            ShowFeedback(
                "Select an equipment image first.",
                Color.yellow
            );
            return;
        }

        if (selectedImage == equipmentID)
        {
            matched[equipmentID] = true;
            matchCount++;

            LockMatchedButton(
                "PPEImage_" + equipmentID
            );

            LockMatchedButton(
                "PPEName_" + nameIndex
            );

            ShowFeedback(
                "CORRECT! Equipment matched.",
                new Color32(90, 255, 130, 255)
            );

            PlaySound(correctSound);
        }
        else
        {
            mistakes++;

            // NEW: Save the actual incorrect pairing.
            string incorrectMatch =
                equipmentNames[selectedImage] +
                " was incorrectly matched with " +
                equipmentNames[equipmentID] + ".";

            matchingMistakeDetails.Add(
                incorrectMatch
            );

            Debug.Log(
                "[PPE Matching Mistake] " +
                incorrectMatch
            );

            ShowFeedback(
                "INCORRECT! Try another match.",
                new Color32(255, 105, 105, 255)
            );

            PlaySound(wrongSound);
        }

        selectedImage = -1;

        UpdateProgress();

        if (IsComplete)
        {
            ShowFeedback(
                "CHALLENGE 1 COMPLETE! All PPE matched.",
                new Color32(90, 255, 130, 255)
            );

            Debug.Log(
                "PPE Matching Complete. Mistakes: " +
                mistakes
            );
        }
    }

    private void LockMatchedButton(
        string objectName)
    {
        Transform target =
            panel.Find(objectName);

        if (target == null)
            return;

        Button button =
            target.GetComponent<Button>();

        if (button != null)
            button.interactable = false;

        Image background =
            target.GetComponent<Image>();

        if (background != null)
        {
            background.color =
                new Color32(45, 125, 95, 255);
        }
    }

    private void UpdateProgress()
    {
        if (progressText == null)
            return;

        progressText.text =
            "MATCHED: " + matchCount +
            " / 10  |  MISTAKES: " + mistakes;
    }

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

    private void PlaySound(AudioClip clip)
    {
        if (feedbackAudio != null &&
            clip != null)
        {
            feedbackAudio.PlayOneShot(clip);
        }
    }
}
