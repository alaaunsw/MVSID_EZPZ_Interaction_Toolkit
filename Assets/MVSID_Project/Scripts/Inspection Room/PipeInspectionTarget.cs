
using UnityEngine;

public class PipeInspectionTarget : MonoBehaviour
{
    [Header("Pipe Identification")]
    public string pipeID = "PIPE_0493";

    [Header("Central Database")]
    public PipeInspectionDatabase database;

    public PipeInspectionData GetInspectionData()
    {
        if (database == null)
        {
            Debug.LogWarning("Database not assigned.");
            return null;
        }

        return database.GetPipeData(pipeID);
    }

    public void InspectPipe()
    {
        PipeInspectionData data = GetInspectionData();

        if (data == null)
            return;

        Debug.Log(
            "PIPE INSPECTION DATA RETRIEVED\n" +
            "Pipe ID: " + data.pipeID + "\n" +
            "Material: " + data.material + "\n" +
            "Grade: " + data.materialGrade + "\n" +
            "Condition: " + data.condition + "\n" +
            data.MeasurementSummary()
        );
    }

    [ContextMenu("Test Data Retrieval")]
    private void TestDataRetrieval()
    {
        InspectPipe();
    }
}
