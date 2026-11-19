
using UnityEngine;

public class TrainingProgressButton : MonoBehaviour
{
    public CollapsibleTrainingProgress controller;

    public enum Action
    {
        Open,
        Close
    }

    public Action action;

    private int lastFrame = -1;

    public void Activate()
    {
        if (lastFrame == Time.frameCount)
            return;

        lastFrame = Time.frameCount;

        if (controller == null)
            return;

        if (action == Action.Open)
            controller.OpenProgress();
        else
            controller.CloseProgress();
    }
}
