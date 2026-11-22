
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

#if UNITY_EDITOR
using UnityEditor;
#endif

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

    // ==========================================
    // ASSESSMENT QUESTIONS
    // ==========================================

    [Header("Assessment Questions - 13 Items")]
    public EquipmentQuestion[] equipment =
        new EquipmentQuestion[13];

    // ==========================================
    // EXISTING QUESTION PANEL
    // ==========================================

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

    // ==========================================
    // NEW: VISUAL FEEDBACK
    // ==========================================

    [Header("Visual Answer Feedback")]
    public GameObject correctTick;
    public GameObject wrongAnswerX;

    // ==========================================
    // AUDIO FEEDBACK
    // ==========================================

    [Header("Audio Answer Feedback")]
    public AudioSource feedbackAudioSource;
    public AudioClip correctSound;
    public AudioClip wrongSound;

    [Min(0.2f)]
    public float feedbackDuration = 1.8f;

    public Color correctColor =
        new Color32(80, 255, 145, 255);

    public Color wrongColor =
        new Color32(255, 110, 110, 255);

    // ==========================================
    // ASSESSMENT SETTINGS
    // ==========================================

    [Header("Assessment Settings")]
    [Range(0f, 100f)]
    public float passingScore = 80f;

    // ==========================================
    // ASSESSMENT STATE
    // ==========================================

    [Header("Assessment State")]
    [SerializeField] private int currentEquipment = -1;
    [SerializeField] private int answeredCount;
    [SerializeField] private int correctCount;
    [SerializeField] private float finalScore;

    [SerializeField] private bool assessmentSubmitted;
    [SerializeField] private bool assessmentPassed;
    [SerializeField] private bool showingAnswerFeedback;

    private readonly Dictionary<int, int> answers =
        new Dictionary<int, int>();

    private SafetyTraineeAccountManager accountManager;
    private SafetyTraineeProgressManager progressManager;

    private Coroutine feedbackRoutine;

    private Color originalStatusColor = Color.white;
    private float originalStatusFontSize = 20f;
    private FontStyles originalStatusFontStyle =
        FontStyles.Normal;

    private TextAlignmentOptions originalStatusAlignment =
        TextAlignmentOptions.Center;

    // ==========================================
    // DEFAULT QUESTIONS
    // ==========================================

    private void Reset()
    {
        LoadDefaultQuestions();
    }

    [ContextMenu("Load 13 Default Equipment Questions")]
    public void LoadDefaultQuestions()
    {
        if (Application.isPlaying)
        {
            Debug.LogWarning(
                "Stop Play Mode before loading questions."
            );
            return;
        }

        equipment = new EquipmentQuestion[]
        {
            // 0 - SAFETY HELMET
            MakeQuestion(
                "SAFETY HELMET",
                "What is the main purpose of this equipment?",
                "Protect the head from impacts and falling objects",
                "Protect the hands from cuts",
                "Reduce exposure to excessive noise",
                "Improve worker visibility",
                0
            ),

            // 1 - SAFETY GLASSES
            MakeQuestion(
                "SAFETY GLASSES",
                "What is the main purpose of this equipment?",
                "Protect the feet from impacts",
                "Protect the eyes from specified particles and impacts",
                "Detect hazardous gases",
                "Prevent falls from height",
                1
            ),

            // 2 - FACE SHIELD
            MakeQuestion(
                "FACE SHIELD",
                "What is the main purpose of this equipment?",
                "Reduce excessive noise exposure",
                "Measure electrical voltage",
                "Protect the face from specified splashes and impacts",
                "Improve worker visibility",
                2
            ),

            // 3 - HEARING PROTECTION
            MakeQuestion(
                "HEARING PROTECTION",
                "What is the main purpose of ear muffs?",
                "Protect the hands from cuts",
                "Detect hazardous gases",
                "Protect the feet",
                "Reduce exposure to hazardous noise",
                3
            ),

            // 4 - EAR PLUGS
            MakeQuestion(
                "EAR PLUGS",
                "What is the main purpose of ear plugs?",
                "Reduce noise exposure when correctly fitted",
                "Protect the eyes from flying particles",
                "Measure electrical current",
                "Protect the head from impacts",
                0
            ),

            // 5 - SAFETY GLOVES
            MakeQuestion(
                "SAFETY GLOVES",
                "What is the main purpose of this equipment?",
                "Prevent falls from height",
                "Protect the hands from specified workplace hazards",
                "Detect gas leaks",
                "Reduce noise exposure",
                1
            ),

            // 6 - SAFETY SHOES
            MakeQuestion(
                "SAFETY SHOES",
                "What is the main purpose of safety footwear?",
                "Protect the face from splashes",
                "Improve worker visibility",
                "Protect the feet from specified workplace hazards",
                "Measure gas concentration",
                2
            ),

            // 7 - COVERALLS
            MakeQuestion(
                "COVERALLS",
                "What is the main purpose of protective coveralls?",
                "Measure electrical voltage",
                "Reduce workplace noise",
                "Prevent falls from ladders",
                "Provide body protection against specified hazards",
                3
            ),

            // 8 - SAFETY VEST
            MakeQuestion(
                "HIGH-VISIBILITY SAFETY VEST",
                "What is the main purpose of this equipment?",
                "Make the worker easier to see",
                "Protect the eyes from impacts",
                "Protect the hands from sharp objects",
                "Detect hazardous gases",
                0
            ),

            // 9 - SAFETY HARNESS
            MakeQuestion(
                "SAFETY HARNESS",
                "What is the main purpose of a safety harness?",
                "Protect against excessive noise",
                "Support fall protection when used with a suitable system",
                "Measure pipe wall thickness",
                "Protect the eyes from dust",
                1
            ),

            // 10 - GAS DETECTOR
            MakeQuestion(
                "GAS DETECTOR",
                "What is the main purpose of this instrument?",
                "Protect the hands from cuts",
                "Measure electrical resistance",
                "Detect specified hazardous gases",
                "Improve worker visibility",
                2
            ),

            // 11 - GAS MASK
            MakeQuestion(
                "GAS MASK / RESPIRATOR",
                "What is the main purpose of suitable respiratory protection?",
                "Protect the feet from impacts",
                "Prevent falls from height",
                "Measure gas concentration",
                "Reduce inhalation of specified airborne hazards",
                3
            ),

            // 12 - MULTIMETER
            MakeQuestion(
                "MULTIMETER",
                "What is the main purpose of this instrument?",
                "Measure electrical quantities such as voltage and resistance",
                "Protect the head from falling objects",
                "Detect hazardous gases",
                "Reduce noise exposure",
                0
            )
        };

        ResetAssessment();

#if UNITY_EDITOR
        EditorUtility.SetDirty(this);
#endif

        Debug.Log(
            "13 equipment assessment questions loaded."
        );
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

    // ==========================================
    // UNITY START
    // ==========================================

    private void Start()
    {
        if (statusText != null)
        {
            originalStatusColor =
                statusText.color;

            originalStatusFontSize =
                statusText.fontSize;

            originalStatusFontStyle =
                statusText.fontStyle;

            originalStatusAlignment =
                statusText.alignment;
        }

        HideFeedbackSymbols();

        if (questionPanel != null)
        {
            questionPanel.SetActive(false);
        }

        // The Question Panel Builder already
        // connects normal Unity UI buttons to
        // TestingPPEAnswerButton.Activate().
        //
        // We do not add another click listener
        // here, avoiding duplicate callbacks.

        UpdateProgress();
    }

    // ==========================================
    // VERIFY TRAINEE ELIGIBILITY
    // ==========================================

    private bool CheckEligibility()
    {
        accountManager =
            SafetyTraineeAccountManager.Instance;

        if (accountManager == null ||
            !accountManager.IsLoggedIn)
        {
            Debug.LogWarning(
                "Trainee must log in before PPE testing."
            );

            return false;
        }

        progressManager =
            accountManager.GetComponent<
                SafetyTraineeProgressManager
            >();

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
    // OPEN EQUIPMENT QUESTION
    // ==========================================

    public void OpenQuestion(int index)
    {
        if (!CheckEligibility())
            return;

        if (assessmentSubmitted ||
            showingAnswerFeedback)
        {
            return;
        }

        if (currentEquipment >= 0)
            return;

        if (equipment == null ||
            index < 0 ||
            index >= equipment.Length)
        {
            return;
        }

        if (equipment[index] == null)
            return;

        if (answers.ContainsKey(index))
        {
            Debug.Log(
                "Equipment already answered: " +
                equipment[index].equipmentName
            );

            return;
        }

        if (questionPanel == null)
        {
            Debug.LogError(
                "Testing PPE Question Panel is not assigned."
            );

            return;
        }

        currentEquipment = index;

        HideFeedbackSymbols();

        EquipmentQuestion item =
            equipment[index];

        if (equipmentTitle != null)
        {
            equipmentTitle.text =
                item.equipmentName;
        }

        if (questionText != null)
        {
            questionText.text =
                item.question;
        }

        // Update the four answer choices.

        for (int i = 0; i < 4; i++)
        {
            if (answerLabels != null &&
                i < answerLabels.Length &&
                answerLabels[i] != null)
            {
                string option =
                    item.options != null &&
                    i < item.options.Length
                    ? item.options[i]
                    : "";

                answerLabels[i].text =
                    ((char)('A' + i)) +
                    ". " +
                    option;
            }
        }

        RestoreStatusAppearance();

        if (statusText != null)
        {
            statusText.text =
                "Select one answer.";
        }

        SetAnswerButtonsEnabled(true);

        UpdateProgress();

        questionPanel.SetActive(true);

        Debug.Log(
            "Question opened: " +
            item.equipmentName
        );
    }

    // ==========================================
    // ALL 13 EQUIPMENT FUNCTIONS
    // ==========================================

    public void OpenHelmetQuestion()
    {
        OpenQuestion(0);
    }

    public void OpenGlassesQuestion()
    {
        OpenQuestion(1);
    }

    public void OpenFaceShieldQuestion()
    {
        OpenQuestion(2);
    }

    public void OpenHearingProtectionQuestion()
    {
        OpenQuestion(3);
    }

    public void OpenEarPlugsQuestion()
    {
        OpenQuestion(4);
    }

    public void OpenGlovesQuestion()
    {
        OpenQuestion(5);
    }

    public void OpenShoesQuestion()
    {
        OpenQuestion(6);
    }

    public void OpenBootsQuestion()
    {
        OpenQuestion(6);
    }

    public void OpenCoverallsQuestion()
    {
        OpenQuestion(7);
    }

    public void OpenVestQuestion()
    {
        OpenQuestion(8);
    }

    public void OpenHarnessQuestion()
    {
        OpenQuestion(9);
    }

    public void OpenGasDetectorQuestion()
    {
        OpenQuestion(10);
    }

    public void OpenGasMaskQuestion()
    {
        OpenQuestion(11);
    }

    public void OpenMultimeterQuestion()
    {
        OpenQuestion(12);
    }

    // ==========================================
    // RECORD ANSWER AND SHOW FEEDBACK
    // ==========================================

    public void SelectAnswer(int answerIndex)
    {
        if (showingAnswerFeedback ||
            assessmentSubmitted)
        {
            return;
        }

        if (currentEquipment < 0 ||
            equipment == null ||
            currentEquipment >= equipment.Length)
        {
            return;
        }

        if (answerIndex < 0 || answerIndex > 3)
            return;

        if (!CheckEligibility())
            return;

        if (answers.ContainsKey(currentEquipment))
            return;

        int selectedEquipment =
            currentEquipment;

        EquipmentQuestion item =
            equipment[selectedEquipment];

        bool isCorrect =
            answerIndex == item.correctAnswer;

        // Save the answer once.

        answers.Add(
            selectedEquipment,
            answerIndex
        );

        showingAnswerFeedback = true;

        // Prevent repeated answers.

        SetAnswerButtonsEnabled(false);

        // ======================================
        // SHOW GREEN TICK OR RED X
        // ======================================

        if (correctTick != null)
        {
            correctTick.SetActive(isCorrect);
        }

        if (wrongAnswerX != null)
        {
            wrongAnswerX.SetActive(!isCorrect);
        }

        // ======================================
        // TEXT FEEDBACK
        // ======================================

        if (statusText != null)
        {
            statusText.fontSize = 30;
            statusText.fontStyle =
                FontStyles.Bold;

            statusText.alignment =
                TextAlignmentOptions.Center;

            if (isCorrect)
            {
                statusText.text =
                    "CORRECT!";

                statusText.color =
                    correctColor;
            }
            else
            {
                statusText.text =
                    "INCORRECT";

                statusText.color =
                    wrongColor;
            }
        }

        // ======================================
        // PLAY CORRECT OR WRONG SOUND
        // ======================================

        if (feedbackAudioSource != null)
        {
            AudioClip clip =
                isCorrect
                ? correctSound
                : wrongSound;

            if (clip != null)
            {
                feedbackAudioSource.PlayOneShot(
                    clip
                );
            }
        }

        Debug.Log(
            item.equipmentName +
            " - " +
            (isCorrect ? "CORRECT" : "INCORRECT")
        );

        UpdateProgress();

        // ======================================
        // WAIT BEFORE CLOSING PANEL
        // ======================================

        if (feedbackRoutine != null)
        {
            StopCoroutine(feedbackRoutine);
        }

        feedbackRoutine =
            StartCoroutine(
                ShowFeedbackThenClose()
            );
    }

    // ==========================================
    // SHOW FEEDBACK FOR 1.8 SECONDS
    // ==========================================

    private IEnumerator ShowFeedbackThenClose()
    {
        yield return new WaitForSecondsRealtime(
            Mathf.Max(0.2f, feedbackDuration)
        );

        // Hide feedback before next question.

        HideFeedbackSymbols();

        if (questionPanel != null)
        {
            questionPanel.SetActive(false);
        }

        currentEquipment = -1;
        showingAnswerFeedback = false;

        SetAnswerButtonsEnabled(true);

        RestoreStatusAppearance();

        feedbackRoutine = null;

        if (equipment != null &&
            answers.Count == equipment.Length)
        {
            Debug.Log(
                "All 13 questions answered. " +
                "Assessment is ready for submission."
            );
        }
    }

    // ==========================================
    // HIDE TICK AND X
    // ==========================================

    private void HideFeedbackSymbols()
    {
        if (correctTick != null)
        {
            correctTick.SetActive(false);
        }

        if (wrongAnswerX != null)
        {
            wrongAnswerX.SetActive(false);
        }
    }

    // ==========================================
    // RESET STATUS TEXT
    // ==========================================

    private void RestoreStatusAppearance()
    {
        if (statusText == null)
            return;

        statusText.color =
            originalStatusColor;

        statusText.fontSize =
            originalStatusFontSize;

        statusText.fontStyle =
            originalStatusFontStyle;

        statusText.alignment =
            originalStatusAlignment;
    }

    // ==========================================
    // ENABLE / DISABLE ANSWER BUTTONS
    // ==========================================

    private void SetAnswerButtonsEnabled(bool enabled)
    {
        if (answerButtons == null)
            return;

        foreach (Button button in answerButtons)
        {
            if (button != null)
            {
                button.interactable = enabled;
            }
        }
    }

    // ==========================================
    // EZPZ ANSWER METHODS
    // ==========================================

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
    // ASSESSMENT PROGRESS
    // ==========================================

    private void UpdateProgress()
    {
        answeredCount = answers.Count;

        if (progressText != null)
        {
            progressText.text =
                "EQUIPMENT IDENTIFIED: " +
                answeredCount +
                " / " +
                (equipment != null
                    ? equipment.Length
                    : 0);
        }
    }

    // ==========================================
    // FINAL SUBMISSION
    // ==========================================

    public void SubmitAssessment()
    {
        if (!CheckEligibility())
            return;

        if (assessmentSubmitted ||
            showingAnswerFeedback)
        {
            return;
        }

        if (equipment == null ||
            equipment.Length == 0)
        {
            return;
        }

        if (answers.Count != equipment.Length)
        {
            Debug.LogWarning(
                "Answer all equipment questions first. " +
                answers.Count +
                " / " +
                equipment.Length
            );

            return;
        }

        correctCount = 0;

        foreach (
            KeyValuePair<int, int> entry
            in answers
        )
        {
            if (entry.Value ==
                equipment[entry.Key].correctAnswer)
            {
                correctCount++;
            }
        }

        finalScore =
            100f * correctCount /
            equipment.Length;

        assessmentPassed =
            finalScore >= passingScore;

        assessmentSubmitted = true;

        Debug.Log(
            "PPE Assessment Score: " +
            correctCount +
            " / " +
            equipment.Length +
            " (" +
            finalScore.ToString("F1") +
            "%)"
        );

        Debug.Log(
            assessmentPassed
            ? "PPE ASSESSMENT PASSED"
            : "PPE ASSESSMENT NOT PASSED"
        );

        // Final results UI and facility entry
        // are separate steps.
        //
        // This method does not automatically
        // load Testing Environment.
    }

    // ==========================================
    // PUBLIC ASSESSMENT STATUS
    // ==========================================

    public bool IsAssessmentSubmitted()
    {
        return assessmentSubmitted;
    }

    public bool HasPassedAssessment()
    {
        return assessmentSubmitted &&
               assessmentPassed &&
               CheckEligibility();
    }

    public float GetFinalScore()
    {
        return finalScore;
    }

    public int GetCorrectAnswerCount()
    {
        return correctCount;
    }

    public int GetAnsweredCount()
    {
        return answers.Count;
    }

    public int GetTotalEquipmentCount()
    {
        return equipment != null
            ? equipment.Length
            : 0;
    }

    // ==========================================
    // RESET ASSESSMENT
    // ==========================================

    public void ResetAssessment()
    {
        if (feedbackRoutine != null)
        {
            StopCoroutine(feedbackRoutine);
            feedbackRoutine = null;
        }

        answers.Clear();

        currentEquipment = -1;
        answeredCount = 0;
        correctCount = 0;
        finalScore = 0f;

        assessmentSubmitted = false;
        assessmentPassed = false;
        showingAnswerFeedback = false;

        HideFeedbackSymbols();

        SetAnswerButtonsEnabled(true);

        if (questionPanel != null)
        {
            questionPanel.SetActive(false);
        }

        RestoreStatusAppearance();

        UpdateProgress();

        Debug.Log(
            "PPE assessment reset."
        );
    }
}
