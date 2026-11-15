
using UnityEngine;

public class EngineerCommunicationButton : MonoBehaviour
{
    public GuidedEngineerCommunication training;
    public string action = "PRIMARY";

    private int lastActivationFrame = -1;

    public void Activate()
    {
        if (lastActivationFrame == Time.frameCount)
            return;

        lastActivationFrame = Time.frameCount;

        if (training != null)
            training.HandleAction(action);
    }
}
