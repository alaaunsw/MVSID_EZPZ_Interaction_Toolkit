
using UnityEngine;

public class InspectionFlowButton : MonoBehaviour
{
    public PipelineInspectionFlowGate flowGate;

    public enum ButtonAction
    {
        InspectPipe,
        FinishInspection
    }

    public ButtonAction action;

    private int lastActivationFrame = -1;

    public void Activate()
    {
        if (lastActivationFrame == Time.frameCount)
            return;

        lastActivationFrame = Time.frameCount;

        if (flowGate == null)
        {
            Debug.LogWarning(
                "Inspection Flow Gate is not assigned."
            );
            return;
        }

        switch (action)
        {
            case ButtonAction.InspectPipe:
                flowGate.InspectPipe();
                break;

            case ButtonAction.FinishInspection:
                flowGate.FinishInspection();
                break;
        }
    }
}
