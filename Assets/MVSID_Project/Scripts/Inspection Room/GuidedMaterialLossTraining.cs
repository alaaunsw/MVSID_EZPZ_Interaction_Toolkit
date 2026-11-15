
using UnityEngine;
using TMPro;
using System.Globalization;

public class GuidedMaterialLossTraining : MonoBehaviour
{
    [Header("Pipe Data")]
    public PipeInspectionTarget pipeTarget;

    [Header("Existing Training UI")]
    public TMP_Text stepText;
    public TMP_Text instructionText;
    public TMP_Text inputLabel;
    public TMP_InputField answerInput;
    public TMP_Text feedbackText;

    [Header("Verified Measurements")]
    public TMP_Text originalValueText;
    public TMP_Text remainingValueText;
    public TMP_Text thicknessLossValueText;

    [Header("Formula Display")]
    public TMP_Text formulaText;

    [Header("Training Buttons")]
    public GameObject checkAnswerButton;
    public GameObject nextStepButton;
    public TMP_Text nextStepButtonText;

    [Header("Optional Calculator")]
    public GameObject calculatorPanel;

    [Header("Answer Settings")]
    public float tolerance = 0.015f;

    [Header("Progress")]
    [SerializeField]
    private int currentStep = 1;

    private bool currentStepComplete = false;
    private bool trainingComplete = false;

    private bool originalVerified = false;
    private bool remainingVerified = false;
    private bool thicknessLossVerified = false;

    private float verifiedOriginal;
    private float verifiedRemaining;
    private float verifiedThicknessLoss;
    private float verifiedPercentage;

    private readonly Color normalColor =
        new Color32(65, 77, 94, 255);

    private readonly Color correctColor =
        new Color32(32, 145, 78, 255);

    private readonly Color incorrectColor =
        new Color32(190, 55, 55, 255);

    // Automatically connect existing generated UI.
    private void Awake()
    {
        Transform content =
            transform.Find("TrainingContent");

        if (content == null)
        {
            Debug.LogWarning(
                "GuidedMaterialLossTraining: " +
                "TrainingContent not found."
            );

            return;
        }

        // Step heading
        if (stepText == null)
        {
            Transform found =
                content.Find("StepText");

            if (found != null)
                stepText =
                    found.GetComponent<TMP_Text>();
        }

        // Instructions
        if (instructionText == null)
        {
            Transform found =
                content.Find("InstructionText");

            if (found != null)
                instructionText =
                    found.GetComponent<TMP_Text>();
        }

        // IMPORTANT: Fixes Step 2 and Step 3 label.
        if (inputLabel == null)
        {
            Transform found =
                content.Find("InputLabel");

            if (found != null)
                inputLabel =
                    found.GetComponent<TMP_Text>();
        }

        // Answer input
        if (answerInput == null)
        {
            Transform found =
                content.Find("AnswerInput");

            if (found != null)
                answerInput =
                    found.GetComponent<TMP_InputField>();
        }

        // Feedback
        if (feedbackText == null)
        {
            Transform found =
                content.Find("FeedbackText");

            if (found != null)
                feedbackText =
                    found.GetComponent<TMP_Text>();
        }

        // CHECK ANSWER button
        if (checkAnswerButton == null)
        {
            Transform found =
                content.Find("CheckAnswerButton");

            if (found != null)
                checkAnswerButton = found.gameObject;
        }

        // NEXT STEP button
        if (nextStepButton == null)
        {
            Transform found =
                content.Find(
                    "CheckAnswNextStepButton"
                );

            if (found == null)
            {
                found = content.Find("NextStepButton");
            }

            if (found != null)
                nextStepButton = found.gameObject;
        }

        // NEXT STEP button label
        if (nextStepButtonText == null &&
            nextStepButton != null)
        {
            Transform found =
                nextStepButton.transform.Find("ButtonText");

            if (found != null)
                nextStepButtonText =
                    found.GetComponent<TMP_Text>();
        }

        // Optional generated workspace elements.
        if (formulaText == null)
        {
            Transform found =
                content.Find("FormulaText");

            if (found != null)
                formulaText =
                    found.GetComponent<TMP_Text>();
        }

        if (originalValueText == null)
        {
            Transform found =
                content.Find("OriginalThicknessValue");

            if (found != null)
                originalValueText =
                    found.GetComponent<TMP_Text>();
        }

        if (remainingValueText == null)
        {
            Transform found =
                content.Find("RemainingThicknessValue");

            if (found != null)
                remainingValueText =
                    found.GetComponent<TMP_Text>();
        }

        if (thicknessLossValueText == null)
        {
            Transform found =
                content.Find("ThicknessLossValue");

            if (found != null)
                thicknessLossValueText =
                    found.GetComponent<TMP_Text>();
        }

        // Calculator may be a sibling of this panel.
        if (calculatorPanel == null &&
            transform.parent != null)
        {
            Transform found =
                transform.parent.Find("CalculatorPanel");

            if (found != null)
                calculatorPanel = found.gameObject;
        }
    }

    private void Start()
    {
        ShowStepOne();
    }

    public void ShowStepOne()
    {
        currentStep = 1;
        currentStepComplete = false;
        trainingComplete = false;

        originalVerified = false;
        remainingVerified = false;
        thicknessLossVerified = false;

        verifiedOriginal = 0f;
        verifiedRemaining = 0f;
        verifiedThicknessLoss = 0f;
        verifiedPercentage = 0f;

        if (answerInput != null)
            answerInput.gameObject.SetActive(true);

        ShowCurrentStep();
    }

    private PipeInspectionData GetData()
    {
        if (pipeTarget == null)
        {
            Debug.LogError(
                "GuidedMaterialLossTraining: " +
                "Pipe Target not assigned."
            );

            return null;
        }

        PipeInspectionData data =
            pipeTarget.GetInspectionData();

        if (data == null)
        {
            Debug.LogError(
                "GuidedMaterialLossTraining: " +
                "Pipe data not found."
            );
        }

        return data;
    }

    private void ShowCurrentStep()
    {
        currentStepComplete = false;

        PipeInspectionData data = GetData();

        if (data == null)
            return;

        string pipeID = data.pipeID;

        if (stepText != null)
        {
            stepText.text =
                "STEP " + currentStep + " OF 4";
        }

        if (answerInput != null)
        {
            answerInput.SetTextWithoutNotify("");
            answerInput.interactable = true;
        }

        // Show CHECK ANSWER.
        if (checkAnswerButton != null)
            checkAnswerButton.SetActive(true);

        // Hide NEXT STEP.
        if (nextStepButton != null)
            nextStepButton.SetActive(false);

        // Calculator is only needed for Steps 3 and 4.
        if (calculatorPanel != null)
        {
            calculatorPanel.SetActive(
                currentStep == 3 || currentStep == 4
            );

            // Clear previous calculator entries.
            if (calculatorPanel.activeSelf)
            {
                MaterialLossCalculator calculator =
                    calculatorPanel.GetComponent<
                        MaterialLossCalculator
                    >();

                if (calculator != null)
                    calculator.Clear();
            }
        }

        UpdateVerifiedMeasurements();

        switch (currentStep)
        {
            case 1:
                SetInstruction(
                    "Open the " + pipeID +
                    " information card.\n\n" +
                    "Find the ORIGINAL WALL THICKNESS " +
                    "and enter its value in millimetres."
                );

                SetInputLabel(
                    "ORIGINAL WALL THICKNESS (mm)"
                );

                SetFormula(
                    "Read the original thickness " +
                    "from the pipe information card."
                );

                break;

            case 2:
                SetInstruction(
                    "Return to the " + pipeID +
                    " information card.\n\n" +
                    "Find the REMAINING WALL THICKNESS " +
                    "and enter its value in millimetres."
                );

                SetInputLabel(
                    "REMAINING WALL THICKNESS (mm)"
                );

                SetFormula(
                    "Read the remaining thickness " +
                    "from the pipe information card."
                );

                break;

            case 3:
                SetInstruction(
                    "Calculate the thickness lost " +
                    "due to corrosion.\n\n" +
                    "Look at your verified measurements " +
                    "and use the calculator."
                );

                SetInputLabel(
                    "THICKNESS LOSS (mm)"
                );

                // Formula only: no numeric answers.
                SetFormula(
                    "THICKNESS LOSS FORMULA\n\n" +
                    "Thickness Loss = Original Thickness " +
                    "- Remaining Thickness"
                );

                break;

            case 4:
                SetInstruction(
                    "Calculate the percentage of " +
                    "original wall thickness lost.\n\n" +
                    "Use your verified measurements " +
                    "and the calculator.\n\n" +
                    "Round your answer to 2 decimal places."
                );

                SetInputLabel(
                    "MATERIAL LOSS (%)"
                );

                // Formula only: no numeric answers.
                SetFormula(
                    "MATERIAL LOSS FORMULA\n\n" +
                    "Material Loss (%) = " +
                    "(Thickness Loss / Original Thickness) " +
                    "x 100"
                );

                break;
        }

        SetFeedback(
            "Enter your answer to continue.",
            normalColor
        );
    }

    private void UpdateVerifiedMeasurements()
    {
        if (originalValueText != null)
        {
            originalValueText.text =
                originalVerified
                ? "Original Thickness\n" +
                  verifiedOriginal.ToString("F2") + " mm"
                : "Original Thickness\nNot verified";
        }

        if (remainingValueText != null)
        {
            remainingValueText.text =
                remainingVerified
                ? "Remaining Thickness\n" +
                  verifiedRemaining.ToString("F2") + " mm"
                : "Remaining Thickness\nNot verified";
        }

        if (thicknessLossValueText != null)
        {
            thicknessLossValueText.text =
                thicknessLossVerified
                ? "Thickness Loss\n" +
                  verifiedThicknessLoss.ToString("F2") + " mm"
                : "Thickness Loss\nNot calculated";
        }
    }

    public void CheckAnswer()
    {
        if (trainingComplete || currentStepComplete)
            return;

        PipeInspectionData data = GetData();

        if (data == null)
            return;

        if (answerInput == null)
        {
            Debug.LogError(
                "GuidedMaterialLossTraining: " +
                "AnswerInput not assigned."
            );

            return;
        }

        string input =
            answerInput.text.Trim().Replace(",", ".");

        float traineeAnswer;

        bool valid = float.TryParse(
            input,
            NumberStyles.Float,
            CultureInfo.InvariantCulture,
            out traineeAnswer
        );

        if (!valid ||
            float.IsNaN(traineeAnswer) ||
            float.IsInfinity(traineeAnswer))
        {
            SetFeedback(
                "Please enter a valid number.",
                incorrectColor
            );

            return;
        }

        float original =
            data.referenceThicknessMM;

        float remaining =
            data.remainingThicknessMM;

        if (original <= 0f ||
            remaining < 0f ||
            remaining > original)
        {
            SetFeedback(
                "The pipe thickness measurements are invalid.",
                incorrectColor
            );

            return;
        }

        float thicknessLoss =
            original - remaining;

        float percentageLoss =
            (thicknessLoss / original) * 100f;

        float correctAnswer;
        string unit = "mm";
        string hint;

        switch (currentStep)
        {
            case 1:
                correctAnswer = original;
                hint =
                    "Check Original Wall Thickness " +
                    "on the pipe information card.";
                break;

            case 2:
                correctAnswer = remaining;
                hint =
                    "Check Remaining Wall Thickness " +
                    "on the pipe information card.";
                break;

            case 3:
                correctAnswer = thicknessLoss;
                hint =
                    "Use Original Thickness minus " +
                    "Remaining Thickness.";
                break;

            case 4:
                correctAnswer = percentageLoss;
                unit = "%";
                hint =
                    "Divide Thickness Loss by Original " +
                    "Thickness, then multiply by 100.";
                break;

            default:
                return;
        }

        float difference =
            Mathf.Abs(traineeAnswer - correctAnswer);

        if (difference <= tolerance)
        {
            currentStepComplete = true;

            // Save verified trainee answers.
            switch (currentStep)
            {
                case 1:
                    verifiedOriginal = traineeAnswer;
                    originalVerified = true;
                    break;

                case 2:
                    verifiedRemaining = traineeAnswer;
                    remainingVerified = true;
                    break;

                case 3:
                    verifiedThicknessLoss = traineeAnswer;
                    thicknessLossVerified = true;
                    break;

                case 4:
                    verifiedPercentage = traineeAnswer;
                    break;
            }

            UpdateVerifiedMeasurements();

            SetFeedback(
                "Correct! " +
                correctAnswer.ToString("F2") +
                " " + unit,
                correctColor
            );

            if (answerInput != null)
                answerInput.interactable = false;

            // Hide CHECK ANSWER.
            if (checkAnswerButton != null)
                checkAnswerButton.SetActive(false);

            // Show NEXT STEP.
            if (nextStepButton != null)
                nextStepButton.SetActive(true);

            if (nextStepButtonText != null)
            {
                nextStepButtonText.text =
                    currentStep == 4
                    ? "FINISH"
                    : "NEXT STEP";
            }

            Debug.Log(
                "Material loss training Step " +
                currentStep + " completed."
            );
        }
        else
        {
            SetFeedback(
                "Not quite correct. " + hint,
                incorrectColor
            );
        }
    }

    public void NextStep()
    {
        if (!currentStepComplete || trainingComplete)
            return;

        if (currentStep < 4)
        {
            currentStep++;
            ShowCurrentStep();
        }
        else
        {
            CompleteTraining();
        }
    }

    private void CompleteTraining()
    {
        trainingComplete = true;

        if (stepText != null)
            stepText.text = "CALCULATION COMPLETED";

        SetInstruction(
            "Excellent work!\n\n" +
            "You have successfully completed " +
            "all four material-loss calculation steps."
        );

        SetInputLabel("ACTIVITY COMPLETED");

        SetFormula(
            "FINAL MATERIAL LOSS\n\n" +
            verifiedPercentage.ToString("F2") + "%"
        );

        UpdateVerifiedMeasurements();

        if (calculatorPanel != null)
            calculatorPanel.SetActive(false);

        if (answerInput != null)
            answerInput.interactable = false;

        if (checkAnswerButton != null)
            checkAnswerButton.SetActive(false);

        if (nextStepButton != null)
            nextStepButton.SetActive(false);

        SetFeedback(
            "Material loss training completed!",
            correctColor
        );

        Debug.Log(
            "Guided material-loss activity completed."
        );
    }

    // Receives calculated result from the calculator.
    // The trainee must still press CHECK ANSWER.
    public void SetCalculatorResult(string result)
    {
        if (answerInput == null ||
            currentStepComplete ||
            trainingComplete)
            return;

        if (currentStep != 3 && currentStep != 4)
            return;

        answerInput.text = result;

        SetFeedback(
            "Calculator result entered. " +
            "Press CHECK ANSWER.",
            normalColor
        );
    }

    private void SetInstruction(string message)
    {
        if (instructionText != null)
            instructionText.text = message;
    }

    private void SetInputLabel(string message)
    {
        if (inputLabel != null)
            inputLabel.text = message;
    }

    private void SetFormula(string message)
    {
        if (formulaText != null)
            formulaText.text = message;
    }

    private void SetFeedback(
        string message,
        Color color)
    {
        if (feedbackText != null)
        {
            feedbackText.text = message;
            feedbackText.color = color;
        }
    }
}
