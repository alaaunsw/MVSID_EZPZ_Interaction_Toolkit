
using UnityEngine;

public class ReportChoiceButton : MonoBehaviour
{
    public GuidedInspectionMultipleChoice reporting;

    // SELECT_0, SELECT_1, SELECT_2,
    // CHECK, NEXT, EDIT, SUBMIT
    public string action = "SELECT_0";

    private int lastActivationFrame = -1;

    public void Activate()
    {
        if (lastActivationFrame == Time.frameCount)
            return;

        lastActivationFrame = Time.frameCount;

        if (reporting != null)
            reporting.HandleAction(action);
    }
}
