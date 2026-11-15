
using UnityEngine;

public class TrainingCompletionButton : MonoBehaviour
{
    public GuidedTrainingCompletion controller;
    public string action = "FINISH";

    private int lastActivationFrame = -1;

    public void Activate()
    {
        if (lastActivationFrame == Time.frameCount)
            return;

        lastActivationFrame = Time.frameCount;

        if (controller != null)
            controller.HandleAction(action);
    }
}
