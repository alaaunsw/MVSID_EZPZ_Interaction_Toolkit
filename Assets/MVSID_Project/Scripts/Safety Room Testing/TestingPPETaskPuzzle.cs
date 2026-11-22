
using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class TestingPPETaskPuzzle : MonoBehaviour
{
    [Header("Mannequin Equipment")]
    public GameObject helmet;
    public GameObject glasses;
    public GameObject gloves;
    public GameObject vest;
    public GameObject boots;
    public GameObject coveralls;
    public GameObject earmuffs;
    public GameObject gasMask;
    public GameObject faceShield;
    public GameObject earplugs;

    [Header("Optional UI")]
    public TextMeshProUGUI scenarioText;
    public TextMeshProUGUI feedbackText;
    public TextMeshProUGUI progressText;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip correctSound;
    public AudioClip wrongSound;

    private int currentScenario = 0;
    private int mistakes = 0;
    private int totalMistakes = 0;

    private readonly HashSet<int> selected =
        new HashSet<int>();

    // NEW: Save detailed incorrect selections
    // across both mannequin scenarios.
    private readonly List<string> mannequinMistakeDetails =
        new List<string>();

    public IReadOnlyList<string> MannequinMistakeDetails
        => mannequinMistakeDetails;

    public int TotalMistakes => totalMistakes;

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

    // Scenario-specific PPE requirements.
    // These are predefined requirements for
    // the hypothetical training facility.
    private readonly int[][] requiredEquipment =
    {
        new int[] { 0, 1, 2, 3, 4, 5 },
        new int[] { 0, 1, 2, 3, 4, 5, 6 }
    };

    public bool IsScenarioComplete { get; private set; }
    public int CurrentScenario => currentScenario;
    public int Mistakes => mistakes;
    public int CorrectSelections => selected.Count;

    private void Start()
    {
        StartScenario(0);
    }

    public void StartScenario(int scenario)
    {
        if (scenario < 0 ||
            scenario >= requiredEquipment.Length)
        {
            return;
        }

        // Starting Scenario 1 resets the complete
        // mannequin assessment mistake history.
        if (scenario == 0)
        {
            mannequinMistakeDetails.Clear();
            totalMistakes = 0;
        }

        currentScenario = scenario;

        selected.Clear();

        // Reset only this scenario's mistake count.
        // The overall history remains available.
        mistakes = 0;

        IsScenarioComplete = false;

        HideAllEquipment();

        if (scenarioText != null)
        {
            scenarioText.text = scenario == 0
                ? "ROUTINE PIPELINE INSPECTION"
                : "HIGH-NOISE PIPELINE INSPECTION";
        }

        ShowFeedback("Select the required PPE.");
        UpdateProgress();
    }

    public void SelectEquipment(int equipmentID)
    {
        if (IsScenarioComplete)
            return;

        if (equipmentID < 0 || equipmentID > 9)
            return;

        if (selected.Contains(equipmentID))
            return;

        bool correct = System.Array.Exists(
            requiredEquipment[currentScenario],
            id => id == equipmentID
        );

        if (correct)
        {
            selected.Add(equipmentID);

            GameObject equipment =
                GetEquipment(equipmentID);

            if (equipment != null)
                equipment.SetActive(true);

            ShowFeedback("CORRECT! PPE added.");

            PlaySound(correctSound);
        }
        else
        {
            mistakes++;
            totalMistakes++;

            // NEW: Identify the incorrect PPE
            // and the scenario where it occurred.
            string scenarioName = currentScenario == 0
                ? "Routine Pipeline Inspection"
                : "High-Noise Pipeline Inspection";

            string mistakeDetail =
                scenarioName + ": " +
                equipmentNames[equipmentID] +
                " was selected but was not required " +
                "for the specified task.";

            mannequinMistakeDetails.Add(mistakeDetail);

            Debug.Log(
                "[PPE Mannequin Mistake] " +
                mistakeDetail
            );

            ShowFeedback(
                "INCORRECT. This item is not required."
            );

            PlaySound(wrongSound);
        }

        UpdateProgress();
        CheckCompletion();
    }

    private GameObject GetEquipment(int id)
    {
        switch (id)
        {
            case 0: return helmet;
            case 1: return glasses;
            case 2: return gloves;
            case 3: return vest;
            case 4: return boots;
            case 5: return coveralls;
            case 6: return earmuffs;
            case 7: return gasMask;
            case 8: return faceShield;
            case 9: return earplugs;
            default: return null;
        }
    }

    private void HideAllEquipment()
    {
        for (int i = 0; i < 10; i++)
        {
            GameObject item = GetEquipment(i);

            if (item != null)
                item.SetActive(false);
        }
    }

    private void UpdateProgress()
    {
        if (progressText == null)
            return;

        progressText.text =
            "EQUIPPED: " + selected.Count +
            " / " +
            requiredEquipment[currentScenario].Length +
            " | MISTAKES: " + mistakes;
    }

    private void CheckCompletion()
    {
        if (selected.Count !=
            requiredEquipment[currentScenario].Length)
        {
            return;
        }

        IsScenarioComplete = true;

        ShowFeedback(
            "SCENARIO COMPLETE! All required PPE selected."
        );
    }

    private void ShowFeedback(string message)
    {
        if (feedbackText != null)
            feedbackText.text = message;

        Debug.Log("[PPE Puzzle] " + message);
    }

    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
            audioSource.PlayOneShot(clip);
    }

    public void StartRoutineScenario()
    {
        StartScenario(0);
    }

    public void StartHighNoiseScenario()
    {
        StartScenario(1);
    }
}
