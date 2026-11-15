
using UnityEngine;
using TMPro;
using System;
using System.Globalization;

public class MaterialLossCalculator : MonoBehaviour
{
    [Header("Training Connection")]
    public GuidedMaterialLossTraining training;

    [Header("Calculator Display")]
    public TMP_Text displayText;
    public TMP_Text expressionText;

    private string current = "";
    private string pendingOperator = "";
    private double storedValue;
    private bool hasStoredValue;
    private bool startNewNumber;
    private bool resultReady;

    private readonly CultureInfo culture =
        CultureInfo.InvariantCulture;

    private void Start()
    {
        Clear();
    }

    private string Format(double value)
    {
        return value.ToString("0.########", culture);
    }

    private void UpdateDisplay()
    {
        if (displayText != null)
        {
            displayText.text =
                string.IsNullOrEmpty(current)
                ? "0"
                : current;
        }

        if (expressionText != null)
        {
            expressionText.text =
                hasStoredValue
                ? Format(storedValue) + " " + pendingOperator
                : "";
        }
    }

    public void Clear()
    {
        current = "";
        pendingOperator = "";
        storedValue = 0;
        hasStoredValue = false;
        startNewNumber = false;
        resultReady = false;

        UpdateDisplay();
    }

    public void Press(string key)
    {
        if (key == "C")
        {
            Clear();
            return;
        }

        if (key == "USE")
        {
            UseResult();
            return;
        }

        if (key == "=")
        {
            Calculate();
            return;
        }

        if (key == "BACK")
        {
            if (startNewNumber)
                return;

            if (current.Length > 0)
            {
                current = current.Substring(
                    0, current.Length - 1
                );
            }

            resultReady = false;
            UpdateDisplay();
            return;
        }

        if (key == "+" || key == "-" ||
            key == "*" || key == "/")
        {
            SetOperator(key);
            return;
        }

        if (key == ".")
        {
            if (startNewNumber)
            {
                current = "";
                startNewNumber = false;
            }

            if (current.Contains("."))
                return;

            current =
                string.IsNullOrEmpty(current)
                ? "0."
                : current + ".";

            resultReady = false;
            UpdateDisplay();
            return;
        }

        if (key.Length == 1 &&
            char.IsDigit(key[0]))
        {
            if (startNewNumber)
            {
                current = "";
                startNewNumber = false;
            }

            if (current == "0")
                current = key;
            else if (current.Length < 14)
                current += key;

            resultReady = false;
            UpdateDisplay();
        }
    }

    private bool TryGetCurrent(out double value)
    {
        return double.TryParse(
            current,
            NumberStyles.Float,
            culture,
            out value
        );
    }

    private bool ApplyPending(double secondValue)
    {
        double result;

        switch (pendingOperator)
        {
            case "+":
                result = storedValue + secondValue;
                break;

            case "-":
                result = storedValue - secondValue;
                break;

            case "*":
                result = storedValue * secondValue;
                break;

            case "/":
                if (secondValue == 0)
                {
                    ShowError();
                    return false;
                }

                result = storedValue / secondValue;
                break;

            default:
                return false;
        }

        if (double.IsNaN(result) ||
            double.IsInfinity(result))
        {
            ShowError();
            return false;
        }

        storedValue = result;
        return true;
    }

    private void SetOperator(string key)
    {
        double value;

        if (TryGetCurrent(out value))
        {
            if (hasStoredValue &&
                !string.IsNullOrEmpty(pendingOperator) &&
                !startNewNumber)
            {
                if (!ApplyPending(value))
                    return;
            }
            else if (!hasStoredValue)
            {
                storedValue = value;
                hasStoredValue = true;
            }
        }
        else if (!hasStoredValue)
        {
            return;
        }

        pendingOperator = key;
        current = Format(storedValue);
        startNewNumber = true;
        resultReady = false;

        UpdateDisplay();
    }

    public void Calculate()
    {
        double value;

        if (!TryGetCurrent(out value))
            return;

        if (hasStoredValue &&
            !string.IsNullOrEmpty(pendingOperator))
        {
            if (startNewNumber)
                return;

            if (!ApplyPending(value))
                return;

            current = Format(storedValue);
        }

        pendingOperator = "";
        hasStoredValue = false;
        startNewNumber = true;
        resultReady = true;

        UpdateDisplay();
    }

    public void UseResult()
    {
        if (!resultReady)
        {
            Debug.Log("Press = before USE.");
            return;
        }

        double value;

        if (!TryGetCurrent(out value) ||
            double.IsNaN(value) ||
            double.IsInfinity(value))
        {
            return;
        }

        if (training == null)
        {
            Debug.LogWarning(
                "MaterialLossCalculator: training not assigned."
            );
            return;
        }

        string answer = Math.Round(
            value,
            2,
            MidpointRounding.AwayFromZero
        ).ToString("F2", culture);

        // Send the result into the existing answer field.
        // The trainee still presses CHECK ANSWER.
        training.SetCalculatorResult(answer);

        // Close calculator immediately after USE.
        MaterialLossWorkspaceBuilder workspace =
            GetComponentInParent<
                MaterialLossWorkspaceBuilder
            >();

        if (workspace != null)
        {
            workspace.CalculatorResultUsed();
        }

        Debug.Log(
            "Calculator USE transferred " + answer
        );
    }

    private void ShowError()
    {
        Clear();

        if (displayText != null)
            displayText.text = "ERROR";
    }
}
