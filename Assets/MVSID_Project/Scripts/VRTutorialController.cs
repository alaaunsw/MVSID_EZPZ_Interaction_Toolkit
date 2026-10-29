
using UnityEngine;
using TMPro;

public class VRTutorialController : MonoBehaviour
{
    [Header("Tutorial UI")]
    public TextMeshProUGUI titleText;
    public GameObject welcomeContent;
    public GameObject navigationInstructions;

    [Header("Navigation Panel")]
    public GameObject navigationPanel;
    public TextMeshProUGUI navigationStatus;
    public GameObject continueButton;

    [Header("3D Controller Demonstration")]
    public GameObject controllerTutorialDisplay;

    [Header("Movement Tracking")]
    public NavigationTrainingTracker navigationTracker;

    private bool navigationCompleted;

    public void StartTraining()
    {
        navigationCompleted = false;

        if (welcomeContent != null)
            welcomeContent.SetActive(false);

        if (navigationInstructions != null)
            navigationInstructions.SetActive(false);

        if (titleText != null)
            titleText.gameObject.SetActive(false);

        if (navigationPanel != null)
            navigationPanel.SetActive(true);

        if (controllerTutorialDisplay != null)
            controllerTutorialDisplay.SetActive(true);

        if (navigationStatus != null)
            navigationStatus.text = "STEP 1 OF 4 | PRACTISE MOVEMENT";

        if (continueButton != null)
            continueButton.SetActive(false);

        if (navigationTracker != null)
            navigationTracker.BeginTracking();

        Debug.Log("Navigation training started!");
    }

    public void CompleteNavigation()
    {
        if (navigationCompleted)
            return;

        navigationCompleted = true;

        if (navigationStatus != null)
        {
            navigationStatus.text =
                "ACTIVITY 1 COMPLETE! WELL DONE.";
        }

        if (continueButton != null)
            continueButton.SetActive(true);

        Debug.Log("Activity 1 completed. Continue is available.");
    }

    public void ContinueToActivity2()
    {
        Debug.Log("Continue to Activity 2 selected.");

        // Activity 2 will be connected next.
    }
}
