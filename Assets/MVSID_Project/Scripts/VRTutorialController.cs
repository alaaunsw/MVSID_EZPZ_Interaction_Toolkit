
using UnityEngine;
using TMPro;

public class VRTutorialController : MonoBehaviour
{
    public TextMeshProUGUI titleText;

    public void StartTraining()
    {
        if (titleText != null)
        {
            titleText.text = "STEP 1: NAVIGATE";
        }

        Debug.Log("VR Tutorial started!");
    }
}
