
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class TestingPPEAssessmentManager : MonoBehaviour
{
    [System.Serializable]
    public class EquipmentQuestion
    {
        public string equipmentName;

        [TextArea(2, 3)]
        public string question;

        public string[] options = new string[4];

        [Range(0, 3)]
        public int correctAnswer;
    }

    [Header("Assessment Questions")]
    public EquipmentQuestion[] equipment = new EquipmentQuestion[5];

    [Header("Question Panel")]
    public GameObject questionPanel;
    public TextMeshProUGUI equipmentTitle;
    public TextMeshProUGUI questionText;
    public TextMeshProUGUI progressText;
    public TextMeshProUGUI statusText;

    [Header("Answer Buttons")]
    public Button[] answerButtons = new Button[4];

    [Header("Answer Labels")]
    public TextMeshProUGUI[] answerLabels =
        new TextMeshProUGUI[4];

    [Header("Assessment State")]
    [SerializeField] private int currentEquipment = -1;
    [SerializeField] private int answeredCount;
    [SerializeField] private int correctCount;

    private readonly Dictionary<int, int> answers =
        new Dictionary<int, int>();

    private SafetyTraineeAccountManager accountManager;
    private SafetyTraineeProgressManager progressManager;

    private void Reset()
    {
        equipment = new EquipmentQuestion[]
        {
            MakeQuestion(
                "SAFETY HELMET",
                "What is the main purpose of this equipment?",
                "Protect the head from impacts",
                "Protect the hands from cuts",
                "Reduce exposure to noise",
                "Improve visibility",
                0
            ),

            MakeQuestion(
                "SAFETY GLASSES",
                "What is the main purpose of this equipment?",
                "Protect the feet",
                "Protect the eyes from particles",
                "Detect hazardous gas",
                "Protect against falling",
                1
            ),

            MakeQuestion(
                "HIGH-VIS VEST",
                "What is the main purpose of this equipment?",
                "Reduce noise",
                "Protect the hands",
                "Make the worker more visible",
                "Measure electrical voltage",
                2
            ),

            MakeQuestion(
                "SAFETY GLOVES",
                "What is the main purpose of this equipment?",
                "Protect the head",
                "Protect the eyes",
                "Detect hazardous gas",
                "Protect the hands from task hazards",
                3
            ),

            MakeQuestion(
                "SAFETY BOOTS",
                "What is the main purpose of this equipment?",
                "Protect the feet from workplace hazards",
                "Protect the face",
                "Reduce noise",
                "Improve visibility",
                0
            )
        };
    }

    private EquipmentQuestion MakeQuestion(
        string name,
        string question,
        string option0,
        string option1,
        string option2,
        string option3,
        int correctAnswer)
    {
        return new EquipmentQuestion
        {
            equipmentName = name,
            question = question,
            options = new string[]
            {
                option0,
                option1,
                option2,
                option3
            },
            correctAnswer = correctAnswer
        };
    }

    private void Start()
    {
        if (questionPanel != null)
            questionPanel.SetActive(false);

        for (int i = 0; i < answerButtons.Length; i++)
        {
            if (answerButtons[i] == null)
                continue;

            int index = i;

            answerButtons[i].onClick.AddListener(
                () => SelectAnswer(index)
            );
        }

        UpdateProgress();
    }

    private bool CheckEligibility()
    {
        accountManager =
            SafetyTraineeAccountManager.Instance;

        if (accountManager == null ||
            !accountManager.IsLoggedIn)
        {
            Debug.LogWarning(
                "A trainee must log in before PPE testing."
            );
            return false;
        }

        progressManager =
            accountManager.GetComponent<
                SafetyTraineeProgressManager>();

        if (progressManager == null)
        {
            Debug.LogError(
                "Training progress manager missing."
            );
            return false;
        }

        progressManager.RefreshProgress();

        if (!progressManager.CanAccessTesting)
        {
            Debug.LogWarning(
                "Trainee is not eligible for Testing Mode."
            );
            return false;
        }

        return true;
    }

    // ==========================================
    // OPEN QUESTION FOR SELECTED EQUIPMENT
    // ==========================================

    public void OpenQuestion(int index)
    {
        if (!CheckEligibility())
            return;

        if (equipment == null ||
            index < 0 ||
            index >= equipment.Length)
            return;

        if (answers.ContainsKey(index))
        {
            Debug.Log(
                "This equipment has already been answered."
            );
            return;
        }

        currentEquipment = index;

        EquipmentQuestion item = equipment[index];

        if (equipmentTitle != null)
            equipmentTitle.text = item.equipmentName;

        if (questionText != null)
            questionText.text = item.question;

        for (int i = 0; i < 4; i++)
        {
            if (answerLabels != null &&
                i < answerLabels.Length &&
                answerLabels[i] != null)
            {
                answerLabels[i].text =
                    item.options != null &&
                    i < item.options.Length
                    ? item.options[i]
                    : "";
            }
        }

        if (statusText != null)
            statusText.text = "Choose one answer.";

        if (questionPanel != null)
            questionPanel.SetActive(true);
    }

    // ==========================================
    // EQUIPMENT INTERACTION METHODS
    // ==========================================

    public void OpenHelmetQuestion()
    {
        OpenQuestion(0);
    }

    public void OpenGlassesQuestion()
    {
        OpenQuestion(1);
    }

    public void OpenVestQuestion()
    {
        OpenQuestion(2);
    }

    public void OpenGlovesQuestion()
    {
        OpenQuestion(3);
    }

    public void OpenBootsQuestion()
    {
        OpenQuestion(4);
    }

    // ==========================================
    // ANSWER SELECTION
    // ==========================================

    public void SelectAnswer(int answerIndex)
    {
        if (currentEquipment < 0)
            return;

        if (answerIndex < 0 || answerIndex > 3)
            return;

        if (answers.ContainsKey(currentEquipment))
            return;

        answers.Add(currentEquipment, answerIndex);

        UpdateProgress();

        Debug.Log(
            "PPE answer recorded for: " +
            equipment[currentEquipment].equipmentName
        );

        currentEquipment = -1;

        if (questionPanel != null)
            questionPanel.SetActive(false);

        if (answers.Count == equipment.Length)
        {
            Debug.Log(
                "All PPE questions answered. " +
                "Assessment is ready for submission."
            );
        }
    }

    // Separate methods for EZPZ events.

    public void AnswerA()
    {
        SelectAnswer(0);
    }

    public void AnswerB()
    {
        SelectAnswer(1);
    }

    public void AnswerC()
    {
        SelectAnswer(2);
    }

    public void AnswerD()
    {
        SelectAnswer(3);
    }

    // ==========================================
    // PROGRESS
    // ==========================================

    private void UpdateProgress()
    {
        answeredCount = answers.Count;

        if (progressText != null)
        {
            progressText.text =
                "EQUIPMENT IDENTIFIED: " +
                answeredCount + " / " +
                equipment.Length;
        }
    }

    // ==========================================
    // FINAL SUBMISSION
    // ==========================================

    public void SubmitAssessment()
    {
        if (!CheckEligibility())
            return;

        if (answers.Count != equipment.Length)
        {
            Debug.LogWarning(
                "Identify all equipment before submitting."
            );
            return;
        }

        correctCount = 0;

        foreach (KeyValuePair<int, int> entry in answers)
        {
            if (entry.Value ==
                equipment[entry.Key].correctAnswer)
            {
                correctCount++;
            }
        }

        float score = equipment.Length > 0
            ? 100f * correctCount / equipment.Length
            : 0f;

        Debug.Log(
            "PPE Assessment completed. Score: " +
            score.ToString("F0") + "%"
        );

        // Final result panel and pass/fail gate
        // will be connected separately.
        //
        // This script does not unlock the facility.
    }

    // ==========================================
    // RESET
    // ==========================================

    public void ResetAssessment()
    {
        answers.Clear();
        currentEquipment = -1;
        answeredCount = 0;
        correctCount = 0;

        if (questionPanel != null)
            questionPanel.SetActive(false);

        UpdateProgress();
    }
}
