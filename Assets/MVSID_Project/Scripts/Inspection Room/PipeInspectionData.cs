
using UnityEngine;

[CreateAssetMenu(
    fileName = "NewPipeInspectionData",
    menuName = "Pipeline Inspection/Pipe Data"
)]
public class PipeInspectionData : ScriptableObject
{
    public enum Condition
    {
        Normal,
        Moderate,
        Critical
    }

    [Header("Pipe Identification")]
    public string pipeID;

    [Header("Pipe Specifications")]
    public string material = "Carbon Steel";
    public string materialGrade = "ASTM A106 Grade B";

    public float pipeSizeMM;
    public float maximumPressurePSI;
    public float temperatureC;
    public float timeYears;

    [Header("Thickness Measurements")]
    public bool measurementsAvailable = true;

    public float referenceThicknessMM;
    public float remainingThicknessMM;

    [Header("Condition")]
    public Condition condition;

    [Header("Data Source")]
    public string dataSource = "Synthetic Training Dataset";

    public float MaterialLossMM
    {
        get
        {
            return Mathf.Max(
                0f,
                referenceThicknessMM - remainingThicknessMM
            );
        }
    }

    public float MaterialLossPercent
    {
        get
        {
            if (referenceThicknessMM <= 0f)
                return 0f;

            return MaterialLossMM / referenceThicknessMM * 100f;
        }
    }

    public string MeasurementSummary()
    {
        if (!measurementsAvailable)
            return "Measurements not available.";

        return
            "Original Thickness: " +
            referenceThicknessMM.ToString("F2") + " mm\n" +
            "Remaining Thickness: " +
            remainingThicknessMM.ToString("F2") + " mm\n" +
            "Material Loss: " +
            MaterialLossMM.ToString("F2") + " mm\n" +
            "Material Loss Percentage: " +
            MaterialLossPercent.ToString("F2") + "%";
    }
}
