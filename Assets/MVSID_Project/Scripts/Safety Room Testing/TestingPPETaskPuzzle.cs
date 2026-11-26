
using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class TestingPPETaskPuzzle : MonoBehaviour
{
    // ==========================================
    // EXISTING MANNEQUIN EQUIPMENT
    // ==========================================

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

    // Retained to preserve Inspector assignments.

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip correctSound;
    public AudioClip wrongSound;

    // ==========================================
    // CURRENT SCENARIO STATE
    // ==========================================

    [Header("Assessment Progress")]
    [SerializeField] private int currentScenario = 0;
    [SerializeField] private int mistakes = 0;
    [SerializeField] private int totalMistakes = 0;

    [SerializeField] private int totalCorrectSelections;
    [SerializeField] private int completedScenarios;

    [SerializeField] private bool routineScenarioCompleted;
    [SerializeField] private bool highNoiseScenarioCompleted;

    private bool scenarioInitialised;

    private readonly HashSet<int> selected =
        new HashSet<int>();

    // ==========================================
    // SCENARIO ASSESSMENT RECORDS
    // ==========================================

    private readonly bool[] scenarioCompleted =
        new bool[2];

    private readonly int[] scenarioCorrectSelections =
        new int[2];

    private readonly int[] scenarioMistakes =
        new int[2];

    private readonly List<string> mannequinMistakeDetails =
        new List<string>();

    public IReadOnlyList<string> MannequinMistakeDetails
        => mannequinMistakeDetails;

    // ==========================================
    // EQUIPMENT NAMES
    // ==========================================

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

    // ==========================================
    // SCENARIO-SPECIFIC PPE REQUIREMENTS
    // ==========================================

    // Predefined requirements for the
    // hypothetical training facility.

    private readonly int[][] requiredEquipment =
    {
        // Scenario 1: Routine Inspection

        new int[]
        {
            0, 1, 2, 3, 4, 5
        },

        // Scenario 2: High-Noise Inspection

        new int[]
        {
            0, 1, 2, 3, 4, 5, 6
        }
    };

    private readonly string[] scenarioNames =
    {
        "Routine Pipeline Inspection",
        "High-Noise Pipeline Inspection"
    };

    // ==========================================
    // PUBLIC ASSESSMENT RESULTS
    // ==========================================

    public bool IsScenarioComplete
    {
        get;
        private set;
    }

    public int CurrentScenario => currentScenario;

    public int Mistakes => mistakes;

    public int CorrectSelections => selected.Count;

    public int TotalMistakes => totalMistakes;

    public int TotalCorrectSelections
        => totalCorrectSelections;

    public int CompletedScenarios
        => completedScenarios;

    public int TotalScenarios
        => requiredEquipment.Length;

    public bool IsAssessmentComplete
        => completedScenarios == TotalScenarios;

    public int TotalRequiredSelections
        => requiredEquipment[0].Length +
           requiredEquipment[1].Length;

    public int TotalAttempts
        => totalCorrectSelections + totalMistakes;

    public float SelectionAccuracy
    {
        get
        {
            if (TotalAttempts == 0)
                return 0f;

            return 100f *
                totalCorrectSelections /
                TotalAttempts;
        }
    }

    // ==========================================
    // INITIALISATION
    // ==========================================

    private void Start()
    {
        if (!scenarioInitialised)
        {
            StartScenario(0);
        }
    }

    // ==========================================
    // START SCENARIO
    // ==========================================

    public void StartScenario(int scenario)
    {
        if (scenario < 0 ||
            scenario >= requiredEquipment.Length)
        {
            return;
        }

        // Prevent restarting an already
        // completed scenario.

        if (scenarioCompleted[scenario])
        {
            Debug.Log(
                "[PPE Puzzle] Scenario already completed."
            );

            return;
        }

        // Prevent resetting the current scenario
        // by pressing the same button again.

        if (scenarioInitialised &&
            scenario == currentScenario)
        {
            return;
        }

        // Scenario 1 must be completed before
        // Scenario 2 becomes available.

        if (scenario == 1 &&
            !scenarioCompleted[0])
        {
            Debug.LogWarning(
                "[PPE Puzzle] Complete Scenario 1 first."
            );

            return;
        }

        currentScenario = scenario;

        scenarioInitialised = true;

        selected.Clear();

        mistakes = scenarioMistakes[scenario];

        IsScenarioComplete = false;

        HideAllEquipment();

        if (scenarioText != null)
        {
            scenarioText.text = scenario == 0

                ? "ROUTINE PIPELINE INSPECTION"

                : "HIGH-NOISE PIPELINE INSPECTION";
        }

        ShowFeedback(
            "Select the required PPE."
        );

        UpdateProgress();

        Debug.Log(
            "[PPE Puzzle] Started scenario: " +
            scenarioNames[currentScenario]
        );
    }

    // ==========================================
    // SELECT EQUIPMENT
    // ==========================================

    public void SelectEquipment(int equipmentID)
    {
        if (!scenarioInitialised ||
            IsScenarioComplete ||
            IsAssessmentComplete)
        {
            return;
        }

        if (equipmentID < 0 ||
            equipmentID >= equipmentNames.Length)
        {
            return;
        }

        // Prevent repeated correct selections.

        if (selected.Contains(equipmentID))
            return;

        bool correct = System.Array.Exists(
            requiredEquipment[currentScenario],
            id => id == equipmentID
        );

        // ======================================
        // RECORD REQUIRED PPE SELECTION
        // ======================================

        if (correct)
        {
            selected.Add(equipmentID);

            scenarioCorrectSelections[
                currentScenario
            ]++;

            totalCorrectSelections++;

            GameObject equipment =
                GetEquipment(equipmentID);

            if (equipment != null)
            {
                equipment.SetActive(true);
            }

            // Neutral acknowledgement.
            // No correctness message or sound.

            ShowFeedback(
                "Selection recorded. Continue."
            );
        }

        // ======================================
        // RECORD INCORRECT PPE SELECTION
        // ======================================

        else
        {
            mistakes++;

            scenarioMistakes[
                currentScenario
            ]++;

            totalMistakes++;

            string mistakeDetail =
                scenarioNames[currentScenario] +
                ": " +
                equipmentNames[equipmentID] +
                " was selected but was not required " +
                "for the specified task.";

            mannequinMistakeDetails.Add(
                mistakeDetail
            );

            // Internal record only.

            Debug.Log(
                "[PPE Mannequin Mistake] " +
                mistakeDetail
            );

            ShowFeedback(
                "Selection recorded. Continue."
            );
        }

        // ======================================
        // RECORD INDIVIDUAL ATTEMPT
        // ======================================

        TestingAssessmentManager manager =
            TestingAssessmentManager.Instance;

        if (manager != null)
        {
            manager.RecordAnswer(
                "SAFETY_03",

                scenarioNames[currentScenario] +
                " - Selection attempt " +
                TotalAttempts,

                equipmentNames[equipmentID],

                correct

                    ? equipmentNames[equipmentID] +
                      " is required for this scenario."

                    : equipmentNames[equipmentID] +
                      " is not required for this scenario. " +
                      "Required PPE: " +
                      GetRequiredEquipmentNames(
                          currentScenario
                      ),

                correct,

                correct

                    ? ""

                    : "Review task-specific hazards " +
                      "and the predefined PPE requirements."
            );
        }

        UpdateProgress();

        CheckCompletion();

        // Synchronise results after each attempt.

        SaveAssessmentResults();
    }

    // ==========================================
    // GET REQUIRED EQUIPMENT NAMES
    // ==========================================

    private string GetRequiredEquipmentNames(
        int scenario)
    {
        List<string> names =
            new List<string>();

        foreach (
            int equipmentID
            in requiredEquipment[scenario]
        )
        {
            names.Add(
                equipmentNames[equipmentID]
            );
        }

        return string.Join(", ", names);
    }

    // ==========================================
    // GET MANNEQUIN EQUIPMENT
    // ==========================================

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

    // ==========================================
    // HIDE ALL MANNEQUIN EQUIPMENT
    // ==========================================

    private void HideAllEquipment()
    {
        for (
            int i = 0;
            i < equipmentNames.Length;
            i++
        )
        {
            GameObject item =
                GetEquipment(i);

            if (item != null)
            {
                item.SetActive(false);
            }
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
        // Mistakes are hidden until final feedback.

        progressText.text =
            "EQUIPPED: " +
            selected.Count +
            " / " +
            requiredEquipment[currentScenario].Length;
    }

    // ==========================================
    // CHECK SCENARIO COMPLETION
    // ==========================================

    private void CheckCompletion()
    {
        if (IsScenarioComplete)
            return;

        if (selected.Count !=
            requiredEquipment[currentScenario].Length)
        {
            return;
        }

        IsScenarioComplete = true;

        // Record completion permanently
        // for the current testing session.

        if (!scenarioCompleted[currentScenario])
        {
            scenarioCompleted[currentScenario] =
                true;

            completedScenarios++;
        }

        routineScenarioCompleted =
            scenarioCompleted[0];

        highNoiseScenarioCompleted =
            scenarioCompleted[1];

        // ======================================
        // BOTH SCENARIOS COMPLETED
        // ======================================

        if (IsAssessmentComplete)
        {
            ShowFeedback(
                "CHALLENGE 2 COMPLETE! " +
                "Proceed to the next activity."
            );

            Debug.Log(
                "[ASSESSMENT] Mannequin assessment complete."
            );

            Debug.Log(
                "[ASSESSMENT] Scenarios completed: " +
                completedScenarios +
                " / " +
                TotalScenarios
            );

            Debug.Log(
                "[ASSESSMENT] Required PPE selected: " +
                totalCorrectSelections +
                " / " +
                TotalRequiredSelections
            );

            Debug.Log(
                "[ASSESSMENT] Total incorrect attempts: " +
                totalMistakes
            );

            Debug.Log(
                "[ASSESSMENT] Selection accuracy: " +
                SelectionAccuracy.ToString("F1") +
                "%"
            );
        }
        else
        {
            ShowFeedback(
                "SCENARIO COMPLETE! " +
                "Proceed to the next scenario."
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
            "SAFETY_03",

            totalCorrectSelections,

            IsAssessmentComplete

                ? TotalAttempts

                : TotalRequiredSelections +
                  totalMistakes,

            totalMistakes,

            IsAssessmentComplete,

            totalMistakes == 0

                ? ""

                : "Review task-specific hazards and " +
                  "select PPE according to the specified " +
                  "inspection requirements.",

            mannequinMistakeDetails
        );
    }

    // ==========================================
    // INDIVIDUAL SCENARIO RESULTS
    // ==========================================

    public bool GetScenarioCompleted(
        int scenario)
    {
        if (scenario < 0 ||
            scenario >= scenarioCompleted.Length)
        {
            return false;
        }

        return scenarioCompleted[scenario];
    }

    public int GetScenarioCorrectSelections(
        int scenario)
    {
        if (scenario < 0 ||
            scenario >= scenarioCorrectSelections.Length)
        {
            return 0;
        }

        return scenarioCorrectSelections[scenario];
    }

    public int GetScenarioMistakes(
        int scenario)
    {
        if (scenario < 0 ||
            scenario >= scenarioMistakes.Length)
        {
            return 0;
        }

        return scenarioMistakes[scenario];
    }

    public int GetScenarioRequiredSelections(
        int scenario)
    {
        if (scenario < 0 ||
            scenario >= requiredEquipment.Length)
        {
            return 0;
        }

        return requiredEquipment[scenario].Length;
    }

    // ==========================================
    // NEUTRAL FEEDBACK
    // ==========================================

    private void ShowFeedback(string message)
    {
        if (feedbackText != null)
        {
            feedbackText.text = message;

            feedbackText.color = Color.white;
        }

        Debug.Log(
            "[PPE Puzzle] " + message
        );
    }

    // ==========================================
    // EXISTING BUTTON METHODS
    // ==========================================

    public void StartRoutineScenario()
    {
        StartScenario(0);
    }

    public void StartHighNoiseScenario()
    {
        StartScenario(1);
    }

    // ==========================================
    // RESET COMPLETE MANNEQUIN ASSESSMENT
    // ==========================================

    public void ResetMannequinAssessment()
    {
        selected.Clear();

        mannequinMistakeDetails.Clear();

        currentScenario = 0;

        mistakes = 0;

        totalMistakes = 0;

        totalCorrectSelections = 0;

        completedScenarios = 0;

        routineScenarioCompleted = false;

        highNoiseScenarioCompleted = false;

        IsScenarioComplete = false;

        scenarioInitialised = false;

        System.Array.Clear(
            scenarioCompleted,
            0,
            scenarioCompleted.Length
        );

        System.Array.Clear(
            scenarioCorrectSelections,
            0,
            scenarioCorrectSelections.Length
        );

        System.Array.Clear(
            scenarioMistakes,
            0,
            scenarioMistakes.Length
        );

        // Clear the corresponding central task.

        TestingAssessmentManager manager =
            TestingAssessmentManager.Instance;

        if (manager != null &&
            manager.SessionStarted &&
            !manager.SessionCompleted)
        {
            TestingAssessmentManager.TaskRecord task =
                manager.FindTask("SAFETY_03");

            if (task != null)
            {
                task.answers.Clear();

                task.mistakeDetails.Clear();

                task.correctAnswers = 0;

                task.totalAnswers =
                    TotalRequiredSelections;

                task.incorrectAttempts = 0;

                task.completed = false;

                task.improvement = "";
            }
        }

        StartScenario(0);

        Debug.Log(
            "[ASSESSMENT] Mannequin assessment reset."
        );
    }
}
