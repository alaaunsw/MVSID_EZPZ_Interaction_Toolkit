
using UnityEngine;
using TMPro;

public class VRTutorialController : MonoBehaviour
{
    [Header("Tutorial UI")]
    public TextMeshProUGUI titleText;
    public GameObject welcomeContent;
    public GameObject navigationInstructions;

    [Header("New Navigation Panel")]
    public GameObject navigationPanel;

    [Header("3D Controller Demonstration")]
    public GameObject controllerTutorialDisplay;

    public void StartTraining()
    {
        if (welcomeContent != null)
            welcomeContent.SetActive(false);

        // Hide the old navigation instructions
        if (navigationInstructions != null)
            navigationInstructions.SetActive(false);

        // Hide the old title to avoid duplicate headings
        if (titleText != null)
            titleText.gameObject.SetActive(false);

        // Show the redesigned panel
        if (navigationPanel != null)
            navigationPanel.SetActive(true);

        // Show the floating controller
        if (controllerTutorialDisplay != null)
            controllerTutorialDisplay.SetActive(true);

        Debug.Log("Navigation training started!");
    }
}
