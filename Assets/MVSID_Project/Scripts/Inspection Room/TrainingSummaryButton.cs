
using UnityEngine;

public class TrainingSummaryButton : MonoBehaviour
{
    public GuidedTrainingSummary controller;
    public string action = "NEXT";

    private int lastFrame = -1;

    public void Activate()
    {
        if (lastFrame == Time.frameCount)
            return;

        lastFrame = Time.frameCount;

        if (controller != null)
            controller.HandleAction(action);
    }
}
