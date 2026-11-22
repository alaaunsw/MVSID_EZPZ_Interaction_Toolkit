
using UnityEngine;

public class TestingPPEMatchButton : MonoBehaviour
{
    public TestingPPEMatchingController controller;

    public bool isImage;

    [Range(0, 9)]
    public int index;

    private int lastActivationFrame = -1;

    public void Activate()
    {
        if (lastActivationFrame == Time.frameCount)
            return;

        lastActivationFrame = Time.frameCount;

        if (controller == null)
            return;

        if (isImage)
            controller.SelectImage(index);
        else
            controller.SelectName(index);
    }
}
