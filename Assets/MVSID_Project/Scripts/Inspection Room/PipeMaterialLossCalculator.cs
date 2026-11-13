
using UnityEngine;
using TMPro;

public class PipeMaterialLossCalculator : MonoBehaviour
{
    [Header("Central Inspection Database")]
    public PipeInspectionDatabase database;

    [Header("Selected Pipe")]
    public string selectedPipeID;

    [Header("Calculation UI")]
    public TMP_Text pipeIDText;
    public TMP_Text measurementsText;
    public TMP_InputField answerInput;
    public TMP_Text feedbackText;
    public TMP_Text progressText;

    [Header("Answer Settings")]
    public float tolerancePercent = 0.1f;

    private readonly string[] pipeIDs =
    {
        "PIPE_0043",
        "PIPE_0493",
        "PIPE_0007"
    };

    private bool[] completed = new bool[3];

    public void SelectPipe0043()
    {
        SelectPipe("PIPE_0043");
    }

    public void SelectPipe0493()
    {
        SelectPipe("PIPE_0493");
    }

    public void SelectPipe0007()
    {
        SelectPipe("PIPE_0007");
    }

    public void SelectPipe(string pipeID)
    {
        if (database == null)
        {
            Debug.LogError(
                "Pipe inspection database is not assigned."
            );
            return;
        }

        PipeInspectionData data =
            database.GetPipeData(pipeID);

        if (data == null)
        {
            Debug.LogWarning(
                "No dataset found for " + pipeID
            );
            return;
        }

        selectedPipeID = pipeID;

        if (pipeIDText != null)
            pipeIDText.text = pipeID;

        if (measurementsText != null)
        {
            measurementsText.text =
                "Original Thickness: " +
                data.referenceThicknessMM.ToString("F2") +
                " mm\n" +
                "Remaining Thickness: " +
                data.remainingThicknessMM.ToString("F2") +
                " mm";
        }

        if (answerInput != null)
            answerInput.text = "";

        if (feedbackText != null)
            feedbackText.text =
                "Calculate the material loss percentage.";

        Debug.Log("Selected pipe: " + pipeID);
    }

    public void CheckAnswer()
    {
        if (string.IsNullOrEmpty(selectedPipeID))
        {
            SetFeedback("Please select a pipe first.");
            return;
        }

        if (database == null)
            return;

        PipeInspectionData data =
            database.GetPipeData(selectedPipeID);

        if (data == null)
            return;

        if (!data.measurementsAvailable ||
            data.referenceThicknessMM <= 0f ||
            data.remainingThicknessMM < 0f ||
            data.remainingThicknessMM >
            data.referenceThicknessMM)
        {
            SetFeedback(
                "Valid thickness measurements are unavailable."
            );
            return;
        }

        if (answerInput == null)
        {
            Debug.LogWarning("Answer Input is missing.");
            return;
        }

        string userText = answerInput.text.Trim();

        float userAnswer;

        if (!float.TryParse(userText, out userAnswer) ||
            float.IsNaN(userAnswer) ||
            float.IsInfinity(userAnswer))
        {
            SetFeedback(
                "Please enter a valid percentage."
            );
            return;
        }

        float correctAnswer =
            data.MaterialLossPercent;

        float difference = Mathf.Abs(
            userAnswer - correctAnswer
        );

        if (difference <= tolerancePercent)
        {
            SetFeedback(
                "Correct! Material loss = " +
                correctAnswer.ToString("F2") + "%"
            );

            MarkPipeComplete(selectedPipeID);
        }
        else
        {
            SetFeedback(
                "Not quite correct. Check your calculation " +
                "and try again."
            );
        }
    }

    private void MarkPipeComplete(string pipeID)
    {
        for (int i = 0; i < pipeIDs.Length; i++)
        {
            if (pipeIDs[i] == pipeID)
            {
                completed[i] = true;
                break;
            }
        }

        UpdateProgress();
    }

    private void UpdateProgress()
    {
        int count = 0;

        foreach (bool done in completed)
        {
            if (done)
                count++;
        }

        if (progressText != null)
        {
            progressText.text =
                "Calculations completed: " +
                count + " / 3";
        }

        if (count == 3)
        {
            Debug.Log(
                "All three material loss calculations completed."
            );
        }
    }

    private void SetFeedback(string message)
    {
        if (feedbackText != null)
            feedbackText.text = message;

        Debug.Log(message);
    }

    public void ResetCalculations()
    {
        completed = new bool[3];
        selectedPipeID = "";

        if (answerInput != null)
            answerInput.text = "";

        UpdateProgress();
    }

    private void Start()
    {
        ResetCalculations();
    }
}
