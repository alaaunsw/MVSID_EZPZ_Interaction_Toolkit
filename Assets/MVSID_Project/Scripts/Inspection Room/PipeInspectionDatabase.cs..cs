
using UnityEngine;

public class PipeInspectionDatabase : MonoBehaviour
{
    [Header("Three Pipe Inspection Records")]
    public PipeInspectionData pipe0043;
    public PipeInspectionData pipe0493;
    public PipeInspectionData pipe0007;

    public PipeInspectionData GetPipeData(string pipeID)
    {
        switch (pipeID)
        {
            case "PIPE_0043":
                return pipe0043;

            case "PIPE_0493":
                return pipe0493;

            case "PIPE_0007":
                return pipe0007;

            default:
                Debug.LogWarning(
                    "Pipe not found in database: " + pipeID
                );
                return null;
        }
    }

    public void CheckDatabase()
    {
        CheckRecord("PIPE_0043", pipe0043);
        CheckRecord("PIPE_0493", pipe0493);
        CheckRecord("PIPE_0007", pipe0007);
    }

    private void CheckRecord(
        string expectedID,
        PipeInspectionData data)
    {
        if (data == null)
        {
            Debug.LogWarning(
                "Missing dataset: " + expectedID
            );
            return;
        }

        if (data.pipeID != expectedID)
        {
            Debug.LogWarning(
                "Dataset ID mismatch for " + expectedID
            );
            return;
        }

        Debug.Log(
            "Loaded: " + data.pipeID +
            " | Condition: " + data.condition +
            " | " + data.MeasurementSummary()
        );
    }
}
