
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

    [Header("Assessment Questions - 13 Items")]
    public EquipmentQuestion[] equipment =
        new EquipmentQuestion[13];

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

    [Header("Visual Answer Feedback")]
    public GameObject correctTick;
    public GameObject wrongAnswerX;

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

    [Header("Assessment Settings")]
    [Range(0f, 100f)]
    public float passingScore = 80f;

    // ==========================================
    // NEXT ACTIVITY
    // ==========================================

    [Header("Next Activity - PPE Matching Puzzle")]
    public GameObject matchingPuzzlePanel;

    public GameObject[] ppeInfoPoints =
        new GameObject[13];

    public bool automaticallySubmit = true;

    [SerializeField]
    private bool matchingPuzzleOpened;

    // ==========================================
    // ASSESSMENT STATE
    // ==========================================

    [Header("Assessment State")]

    [SerializeField]
    private int currentEquipment = -1;

    [SerializeField]
    private int answeredCount;

    [SerializeField]
    private int correctCount;

    [SerializeField]
    private float finalScore;

    [SerializeField]
    private bool assessmentSubmitted;

    [SerializeField]
    private bool assessmentPassed;

    [SerializeField]
    private bool showingAnswerFeedback;

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

        if (feedbackAudioSource != null)
        {
            feedbackAudioSource.playOnAwake = false;
            feedbackAudioSource.Stop();
        }

        // Keep matching puzzle hidden until
        // all questions have been answered.

        if (matchingPuzzlePanel != null)
        {
            if (!transform.IsChildOf(
                matchingPuzzlePanel.transform))
            {
                matchingPuzzlePanel.SetActive(false);
            }
            else
            {
                Debug.LogError(
                    "[TESTING] Matching puzzle panel " +
                    "must not contain the assessment manager."
                );
            }
        }

        matchingPuzzleOpened = false;

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

        // Display four answer options.

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
    // SELECT AND RECORD ANSWER
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

        if (answerIndex < 0 ||
            answerIndex > 3)
        {
            return;
        }

        if (!CheckEligibility())
            return;

        if (answers.ContainsKey(currentEquipment))
            return;

        int selectedEquipment =
            currentEquipment;

        EquipmentQuestion item =
            equipment[selectedEquipment];

        if (item == null ||
            item.options == null ||
            answerIndex >= item.options.Length ||
            item.correctAnswer < 0 ||
            item.correctAnswer >= item.options.Length)
        {
            Debug.LogError(
                "[TESTING] Equipment question is invalid."
            );

            return;
        }

        bool isCorrect =
            answerIndex == item.correctAnswer;

        // ======================================
        // STORE THE ACTUAL TRAINEE ANSWER
        // ======================================

        answers.Add(
            selectedEquipment,
            answerIndex
        );

        // ======================================
        // SAVE TO CENTRAL ASSESSMENT MANAGER
        // ======================================

        TestingAssessmentManager manager =
            TestingAssessmentManager.Instance;

        if (manager != null &&
            manager.SessionStarted &&
            !manager.SessionCompleted)
        {
            manager.RecordAnswer(
                "SAFETY_01",

                item.equipmentName + ": " +
                item.question,

                item.options[answerIndex],

                item.options[item.correctAnswer],

                isCorrect,

                isCorrect

                    ? ""

                    : "Review the purpose and correct use of " +
                      item.equipmentName + "."
            );
        }
        else
        {
            Debug.LogWarning(
                "[ASSESSMENT] No active central assessment " +
                "session. Answer retained locally."
            );
        }

        // Prevent repeated submissions.

        showingAnswerFeedback = true;

        SetAnswerButtonsEnabled(false);

        HideFeedbackSymbols();

        RestoreStatusAppearance();

        // ======================================
        // CORRECT ANSWER FEEDBACK
        // ======================================

        if (isCorrect)
        {
            if (correctTick != null)
            {
                correctTick.SetActive(true);
            }

            if (wrongAnswerX != null)
            {
                wrongAnswerX.SetActive(false);
            }

            if (statusText != null)
            {
                statusText.text = "CORRECT!";
                statusText.color = correctColor;
            }

            if (feedbackAudioSource != null &&
                correctSound != null)
            {
                feedbackAudioSource.Stop();

                feedbackAudioSource.PlayOneShot(
                    correctSound
                );
            }
        }

        // ======================================
        // INCORRECT ANSWER FEEDBACK
        // ======================================

        else
        {
            if (correctTick != null)
            {
                correctTick.SetActive(false);
            }

            if (wrongAnswerX != null)
            {
                wrongAnswerX.SetActive(true);
            }

            if (statusText != null)
            {
                statusText.text = "INCORRECT!";
                statusText.color = wrongColor;
            }

            if (feedbackAudioSource != null &&
                wrongSound != null)
            {
                feedbackAudioSource.Stop();

                feedbackAudioSource.PlayOneShot(
                    wrongSound
                );
            }
        }

        // ======================================
        // FEEDBACK TEXT APPEARANCE
        // ======================================

        if (statusText != null)
        {
            statusText.fontSize = 30;

            statusText.fontStyle =
                FontStyles.Bold;

            statusText.alignment =
                TextAlignmentOptions.Center;
        }

        Debug.Log(
            "[TESTING] Answer recorded for: " +
            item.equipmentName +
            " | Correct: " +
            isCorrect
        );

        UpdateProgress();

        // ======================================
        // CLOSE QUESTION AFTER FEEDBACK
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
    // CLOSE QUESTION PANEL
    // ==========================================

    private IEnumerator ShowFeedbackThenClose()
    {
        yield return new WaitForSecondsRealtime(
            Mathf.Max(0.2f, feedbackDuration)
        );

        HideFeedbackSymbols();

        currentEquipment = -1;

        showingAnswerFeedback = false;

        SetAnswerButtonsEnabled(true);

        RestoreStatusAppearance();

        feedbackRoutine = null;

        if (questionPanel != null)
        {
            questionPanel.SetActive(false);
        }

        // ======================================
        // CHECK IF ALL QUESTIONS ARE COMPLETED
        // ======================================

        if (equipment != null &&
            equipment.Length > 0 &&
            answers.Count == equipment.Length)
        {
            Debug.Log(
                "[TESTING] All equipment questions completed."
            );

            // NEW:
            // Hide every PPE information point
            // immediately after the final feedback.

            HideAllPPEInfoPoints();

            // Preserve automatic assessment submission.

            if (automaticallySubmit)
            {
                SubmitAssessment();
            }
        }
    }

    // ==========================================
    // NEW: HIDE ALL PPE INFORMATION POINTS
    // ==========================================

    public void HideAllPPEInfoPoints()
    {
        if (ppeInfoPoints == null)
        {
            Debug.LogWarning(
                "[TESTING] PPE information points " +
                "are not assigned."
            );

            return;
        }

        int hiddenCount = 0;

        foreach (GameObject point in ppeInfoPoints)
        {
            if (point == null)
                continue;

            // Protect the assessment manager.

            if (transform.IsChildOf(point.transform))
            {
                Debug.LogWarning(
                    "[TESTING] Cannot hide " +
                    point.name +
                    " because it contains the assessment manager."
                );

                continue;
            }

            // Protect the matching puzzle.

            if (matchingPuzzlePanel != null &&
                matchingPuzzlePanel.transform.IsChildOf(
                    point.transform))
            {
                Debug.LogWarning(
                    "[TESTING] Cannot hide " +
                    point.name +
                    " because it contains the matching puzzle."
                );

                continue;
            }

            // Disable only the information point.

            point.SetActive(false);

            hiddenCount++;
        }

        Debug.Log(
            "[TESTING] PPE information points hidden: " +
            hiddenCount
        );
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
    // RESTORE STATUS TEXT APPEARANCE
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
    // ENABLE OR DISABLE ANSWER BUTTONS
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
    // EXISTING EZPZ ANSWER METHODS
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

        if (assessmentSubmitted)
        {
            OpenMatchingPuzzle();

            return;
        }

        if (showingAnswerFeedback)
            return;

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

        // ======================================
        // CALCULATE SCORE INTERNALLY
        // ======================================

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

        // ======================================
        // CHECK CENTRAL ASSESSMENT SESSION
        // ======================================

        TestingAssessmentManager manager =
            TestingAssessmentManager.Instance;

        if (manager == null ||
            !manager.SessionStarted ||
            manager.SessionCompleted)
        {
            Debug.LogError(
                "[ASSESSMENT] No active testing session. " +
                "Answers retained locally. " +
                "Check trainee identification."
            );

            return;
        }

        TestingAssessmentManager.TaskRecord task =
            manager.FindTask("SAFETY_01");

        if (task == null)
        {
            Debug.LogError(
                "[ASSESSMENT] SAFETY_01 was not registered."
            );

            return;
        }

        // ======================================
        // SYNCHRONISE ALL ANSWERS
        // ======================================

        foreach (
            KeyValuePair<int, int> entry
            in answers
        )
        {
            EquipmentQuestion item =
                equipment[entry.Key];

            bool isCorrect =
                entry.Value == item.correctAnswer;

            manager.RecordAnswer(
                "SAFETY_01",

                item.equipmentName + ": " +
                item.question,

                item.options[entry.Value],

                item.options[item.correctAnswer],

                isCorrect,

                isCorrect

                    ? ""

                    : "Review the purpose and correct use of " +
                      item.equipmentName + "."
            );
        }

        // ======================================
        // SAVE FINAL PPE KNOWLEDGE RESULTS
        // ======================================

        manager.RecordTaskResult(
            "SAFETY_01",

            correctCount,

            equipment.Length,

            equipment.Length - correctCount,

            true,

            correctCount == equipment.Length

                ? ""

                : "Review the equipment questions " +
                  "answered incorrectly."
        );

        assessmentSubmitted = true;

        // Keep overall results internal.

        Debug.Log(
            "[ASSESSMENT] PPE Knowledge completed. " +
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

        // ======================================
        // AUTOMATICALLY OPEN MATCHING PUZZLE
        // ======================================

        // Progression is not restricted
        // by the trainee's score.

        OpenMatchingPuzzle();
    }

    // ==========================================
    // OPEN MATCHING PUZZLE
    // ==========================================

    public void OpenMatchingPuzzle()
    {
        if (!assessmentSubmitted ||
            matchingPuzzleOpened)
        {
            return;
        }

        if (matchingPuzzlePanel == null)
        {
            Debug.LogWarning(
                "[TESTING] Matching Puzzle Panel " +
                "is not assigned."
            );

            return;
        }

        if (matchingPuzzlePanel == questionPanel ||
            transform.IsChildOf(
                matchingPuzzlePanel.transform))
        {
            Debug.LogError(
                "[TESTING] Assign a separate " +
                "matching puzzle panel."
            );

            return;
        }

        // Check whether the puzzle's parent
        // is currently active.

        if (matchingPuzzlePanel.transform.parent != null &&
            !matchingPuzzlePanel.transform.parent
                .gameObject.activeInHierarchy)
        {
            Debug.LogWarning(
                "[TESTING] Matching puzzle has an " +
                "inactive parent GameObject."
            );

            return;
        }

        // Hide multiple-choice question panel.

        if (questionPanel != null)
        {
            questionPanel.SetActive(false);
        }

        HideFeedbackSymbols();

        // Ensure every information point
        // remains hidden before opening the puzzle.

        HideAllPPEInfoPoints();

        // Show the matching puzzle.

        matchingPuzzlePanel.SetActive(true);

        matchingPuzzleOpened =
            matchingPuzzlePanel.activeInHierarchy;

        if (!matchingPuzzleOpened)
        {
            Debug.LogWarning(
                "[TESTING] Matching puzzle could not open."
            );

            return;
        }

        Debug.Log(
            "[TESTING] PPE matching puzzle opened."
        );
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

    public bool IsMatchingPuzzleOpened()
    {
        return matchingPuzzleOpened;
    }

    // NEW:
    // Allows other scripts to check whether
    // every PPE question has been answered.

    public bool AreAllQuestionsCompleted()
    {
        return equipment != null &&
               equipment.Length > 0 &&
               answers.Count == equipment.Length;
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

        if (feedbackAudioSource != null &&
            Application.isPlaying)
        {
            feedbackAudioSource.Stop();
        }

        answers.Clear();

        currentEquipment = -1;

        answeredCount = 0;

        correctCount = 0;

        finalScore = 0f;

        assessmentSubmitted = false;

        assessmentPassed = false;

        showingAnswerFeedback = false;

        matchingPuzzleOpened = false;

        // Reset central assessment record.

        if (Application.isPlaying)
        {
            TestingAssessmentManager manager =
                TestingAssessmentManager.Instance;

            if (manager != null &&
                manager.SessionStarted &&
                !manager.SessionCompleted)
            {
                TestingAssessmentManager.TaskRecord task =
                    manager.FindTask("SAFETY_01");

                if (task != null)
                {
                    task.answers.Clear();

                    task.mistakeDetails.Clear();

                    task.correctAnswers = 0;

                    task.incorrectAttempts = 0;

                    task.totalAnswers =
                        GetTotalEquipmentCount();

                    task.completed = false;

                    task.improvement = "";
                }
            }
        }

        HideFeedbackSymbols();

        SetAnswerButtonsEnabled(true);

        if (questionPanel != null)
        {
            questionPanel.SetActive(false);
        }

        if (matchingPuzzlePanel != null &&
            !transform.IsChildOf(
                matchingPuzzlePanel.transform))
        {
            matchingPuzzlePanel.SetActive(false);
        }

        if (Application.isPlaying)
        {
            RestoreStatusAppearance();
        }

        UpdateProgress();

        Debug.Log(
            "[TESTING] PPE assessment reset."
        );
    }
}
