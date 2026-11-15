
using UnityEngine;

public class MaterialLossCalculatorKey : MonoBehaviour
{
    public MaterialLossCalculator calculator;
    public string key = "7";

    private int lastActivationFrame = -1;

    public void Activate()
    {
        if (lastActivationFrame == Time.frameCount)
            return;

        lastActivationFrame = Time.frameCount;

        if (calculator != null)
            calculator.Press(key);
    }
}
