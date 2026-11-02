
using UnityEngine;
using UnityEngine.UI;

public class EZPZConfidenceToggle : MonoBehaviour
{
    public Toggle confidenceToggle;

    public void ToggleConfidence()
    {
        if (confidenceToggle != null)
        {
            confidenceToggle.isOn = !confidenceToggle.isOn;
        }
    }
}

