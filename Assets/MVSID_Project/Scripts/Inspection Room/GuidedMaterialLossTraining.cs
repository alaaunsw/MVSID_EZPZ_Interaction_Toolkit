
using UnityEngine;
using TMPro;
using System.Globalization;

public class GuidedMaterialLossTraining : MonoBehaviour
{
    [Header("Pipe Data")]
    public PipeInspectionTarget pipeTarget;

    [Header("Training UI")]
    public TMP_Text stepText;
    public TMP_Text instructionText;
    public TMP_Text inputLabel;
    public TMP_InputField answerInput;
    public TMP_Text feedbackText;

   [Header("Training Buttons")]
public GameObject checkAnswerButton;
public GameObject nextStepButton;
public TMP_Text nextStepButtonText;

    [Header("Answer Settings")]
    public float tolerance = 0.015f;

    [Header("Progress")]
    [SerializeField]
    private int currentStep = 1;

    private bool currentStepComplete = false;
    private bool trainingComplete = false;

    private readonly Color normalColor =
        new Color32(65, 77, 94, 255);

    private readonly Color correctColor =
        new Color32(32, 145, 78, 255);

    private readonly Color incorrectColor =
        new Color32(190, 55, 55, 255);

    private void Start()
    {
        ShowStepOne();
    }

    public void ShowStepOne()
    {
        currentStep = 1;
        trainingComplete = false;
        ShowCurrentStep();
    }

    private PipeInspectionData GetData()
    {
        if (pipeTarget == null)
        {
            Debug.LogError(
                "Pipe Target is not assigned."
            );
            return null;
        }

        return pipeTarget.GetInspectionData();
    }

    private void ShowCurrentStep()
    {
        currentStepComplete = false;

        PipeInspectionData data = GetData();

        if (data == null)
            return;

        string pipeID = data.pipeID;

        if (stepText != null)
            stepText.text =
                "STEP " + currentStep + " OF 4";

        if (answerInput != null)
        {
            answerInput.text = "";
            answerInput.interactable = true;
        }

       // Show CHECK ANSWER at the start of every step.
if (checkAnswerButton != null)
    checkAnswerButton.SetActive(true);

// Hide NEXT STEP until the answer is correct.
if (nextStepButton != null)
    nextStepButton.SetActive(false);

        switch (currentStep)
        {
            case 1:

                SetInstruction(
                    "Open the " + pipeID +
                    " information card. Find the " +
                    "ORIGINAL wall thickness and enter " +
                    "its value in millimetres."
                );

                SetInputLabel(
                    "ORIGINAL WALL THICKNESS (mm)"
                );

                break;

            case 2:

                SetInstruction(
                    "Return to the " + pipeID +
                    " information card. Find the " +
                    "REMAINING wall thickness and enter " +
                    "its value in millimetres."
                );

                SetInputLabel(
                    "REMAINING WALL THICKNESS (mm)"
                );

                break;

            case 3:

                SetInstruction(
                    "Now calculate the thickness lost " +
                    "due to corrosion.\n\n" +
                    "Thickness Loss = Original Thickness " +
                    "- Remaining Thickness\n\n" +
                    "Enter the result in millimetres."
                );

                SetInputLabel(
                    "THICKNESS LOSS (mm)"
                );

                break;

            case 4:

                SetInstruction(
                    "Calculate the percentage of wall " +
                    "thickness lost.\n\n" +
                    "Material Loss (%) = " +
                    "(Thickness Loss / Original Thickness) " +
                    "x 100\n\n" +
                    "Round your answer to 2 decimals."
                );

                SetInputLabel(
                    "MATERIAL LOSS (%)"
                );

                break;
        }

        SetFeedback(
            "Enter your answer to continue.",
            normalColor
        );
    }

    public void CheckAnswer()
    {
        if (trainingComplete ||
            currentStepComplete)
            return;

        PipeInspectionData data = GetData();

        if (data == null)
            return;

        if (answerInput == null)
        {
            Debug.LogError(
                "Answer Input is not assigned."
            );
            return;
        }

        string input =
            answerInput.text.Trim();

        input = input.Replace(",", ".");

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
                "The thickness measurements are invalid.",
                incorrectColor
            );
            return;
        }

        float thicknessLoss =
            original - remaining;

        float lossPercentage =
            (thicknessLoss / original) * 100f;

        float correctAnswer = 0f;

        string unit = "mm";

        switch (currentStep)
        {
            case 1:
                correctAnswer = original;
                break;

            case 2:
                correctAnswer = remaining;
                break;

            case 3:
                correctAnswer = thicknessLoss;
                break;

            case 4:
                correctAnswer = lossPercentage;
                unit = "%";
                break;
        }

        if (Mathf.Abs(
            traineeAnswer - correctAnswer
            ) <= tolerance)
        {
            currentStepComplete = true;
            // Hide CHECK ANSWER after a correct answer.
if (checkAnswerButton != null)
    checkAnswerButton.SetActive(false);

            SetFeedback(
                "✓ Correct! " +
                correctAnswer.ToString("F2") +
                " " + unit,
                correctColor
            );

            if (answerInput != null)
                answerInput.interactable = false;

            if (nextStepButton != null)
            {
                nextStepButton.SetActive(true);

                if (nextStepButtonText != null)
                {
                    nextStepButtonText.text =
                        currentStep == 4
                        ? "FINISH"
                        : "NEXT STEP";
                }
            }

            Debug.Log(
                "Material Loss Step " +
                currentStep + " completed."
            );
        }
        else
        {
            string hint = "";

            switch (currentStep)
            {
                case 1:
                    hint =
                        "Check the Original Wall Thickness " +
                        "on the information card.";
                    break;

                case 2:
                    hint =
                        "Check the Remaining Thickness " +
                        "on the information card.";
                    break;

                case 3:
                    hint =
                        "Subtract remaining thickness " +
                        "from original thickness.";
                    break;

                case 4:
                    hint =
                        "Divide thickness loss by original " +
                        "thickness, then multiply by 100.";
                    break;
            }

            SetFeedback(
                "Not quite correct. " + hint,
                incorrectColor
            );
        }
    }

    public void NextStep()
    {
        if (!currentStepComplete)
        {
            SetFeedback(
                "Complete this step before continuing.",
                incorrectColor
            );
            return;
        }

        if (currentStep < 4)
        {
            currentStep++;
            ShowCurrentStep();
        }
        else
        {
            trainingComplete = true;

            if (stepText != null)
                stepText.text =
                    "CALCULATION COMPLETED";

            SetInstruction(
                "Excellent work! You have successfully " +
                "retrieved the pipe measurements and " +
                "calculated its material loss."
            );

            SetInputLabel(
                "ALL FOUR STEPS COMPLETED"
            );

            if (nextStepButton != null)
                nextStepButton.SetActive(false);

            if (answerInput != null)
                answerInput.interactable = false;

            SetFeedback(
                "✓ Material loss training completed!",
                correctColor
            );

            Debug.Log(
                "Guided material loss training finished."
            );
        }
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
