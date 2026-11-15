
using UnityEngine;

public class ReportVirtualKeyboardKey : MonoBehaviour
{
    public ReportVirtualKeyboardBuilder keyboard;
    public string keyValue = "A";

    private int lastActivationFrame = -1;

    public void Press()
    {
        if (lastActivationFrame == Time.frameCount)
            return;

        lastActivationFrame = Time.frameCount;

        if (keyboard != null)
            keyboard.PressKey(keyValue);
    }
}
