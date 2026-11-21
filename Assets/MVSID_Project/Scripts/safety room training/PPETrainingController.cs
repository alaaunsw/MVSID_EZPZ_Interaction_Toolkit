
using System.Collections;
using UnityEngine;

public class PPETrainingController : MonoBehaviour
{
    [Header("PPE Learning Points")]
    public GameObject[] ppeInfoPoints;

    [Header("Training Start Zone")]
    public GameObject trainingStartZone;

    [Header("Mannequin Training")]
    public PPETrainingDualMenuBuilder mannequinMenu;

    [Header("Completion Feedback")]
    public GameObject greenTick;
    public AudioSource successSound;
    public float feedbackDuration = 2f;

    [Header("Old Next Button - Optional")]
    public GameObject nextEquipmentButton;

    private bool trainingSelected;
    private bool trainingStarted;
    private bool completingVideo;

    private int currentPPEIndex;

    private void Start()
    {
        HideAllPoints();

        if (trainingStartZone != null)
            trainingStartZone.SetActive(false);

        HideFeedback();

        if (nextEquipmentButton != null)
            nextEquipmentButton.SetActive(false);

        if (mannequinMenu != null)
            mannequinMenu.HideMenus();
    }

    private void HideAllPoints()
    {
        foreach (GameObject point in ppeInfoPoints)
        {
            if (point != null)
                point.SetActive(false);
        }
    }

    private void HideFeedback()
    {
        if (greenTick != null)
            greenTick.SetActive(false);
    }

    // =====================================
    // SELECT TRAINING MODE
    // =====================================

    public void BeginTraining()
    {
        StopAllCoroutines();

        trainingSelected = true;
        trainingStarted = false;
        completingVideo = false;
        currentPPEIndex = 0;

        HideFeedback();

        foreach (GameObject point in ppeInfoPoints)
        {
            if (point == null)
                continue;

            SafetyPPEInfoPoint info =
                point.GetComponent<SafetyPPEInfoPoint>();

            if (info != null)
                info.CloseInformation();
        }

        HideAllPoints();

        if (nextEquipmentButton != null)
            nextEquipmentButton.SetActive(false);

        if (mannequinMenu != null)
            mannequinMenu.HideMenus();

        if (trainingStartZone != null)
            trainingStartZone.SetActive(true);

        Debug.Log(
            "Training selected. Enter the blue circle."
        );
    }

    // =====================================
    // ENTER TRAINING AREA
    // =====================================

    public void EnterTrainingZone()
    {
        if (!trainingSelected || trainingStarted)
            return;

        trainingStarted = true;

        ShowCurrentPPE();

        Debug.Log("PPE video learning started.");
    }

    // =====================================
    // SHOW ONE PPE POINT
    // =====================================

    private void ShowCurrentPPE()
    {
        HideAllPoints();
        HideFeedback();

        completingVideo = false;

        if (currentPPEIndex >= ppeInfoPoints.Length)
        {
            StartMannequinTraining();
            return;
        }

        GameObject point =
            ppeInfoPoints[currentPPEIndex];

        if (point == null)
        {
            Debug.LogWarning(
                "Missing PPE point at index " +
                currentPPEIndex
            );
            return;
        }

        point.SetActive(true);

        Debug.Log(
            "PPE " + (currentPPEIndex + 1) +
            "/" + ppeInfoPoints.Length +
            ": " + point.name
        );
    }

    // =====================================
    // CALLED WHEN VIDEO FINISHES
    // =====================================

    public void NotifyVideoFinished(
        SafetyPPEInfoPoint infoPoint)
    {
        if (!trainingStarted ||
            completingVideo ||
            infoPoint == null)
            return;

        if (currentPPEIndex >= ppeInfoPoints.Length)
            return;

        GameObject expectedPoint =
            ppeInfoPoints[currentPPEIndex];

        if (expectedPoint != infoPoint.gameObject)
            return;

        completingVideo = true;

        StartCoroutine(
            CompleteCurrentPPE(infoPoint)
        );
    }

    private IEnumerator CompleteCurrentPPE(
        SafetyPPEInfoPoint infoPoint)
    {
        Debug.Log(
            "PPE completed: " + infoPoint.name
        );

        // Show green tick.
        if (greenTick != null)
            greenTick.SetActive(true);

        // Play success sound.
        if (successSound != null)
            successSound.Play();

        // Allow trainee to see success feedback.
        yield return new WaitForSeconds(
            Mathf.Max(0f, feedbackDuration)
        );

        // Close the completed video.
        if (infoPoint != null)
            infoPoint.CloseInformation();

        currentPPEIndex++;

        // Automatically activate next PPE point.
        ShowCurrentPPE();
    }

    // Keep old EZPZ button references harmless.
    public void NextEquipment()
    {
        Debug.Log(
            "Automatic progression is enabled. " +
            "Watch the current PPE video to its end."
        );
    }

    // =====================================
    // START MANNEQUIN ACTIVITY
    // =====================================

    private void StartMannequinTraining()
    {
        HideAllPoints();
        HideFeedback();

        if (nextEquipmentButton != null)
            nextEquipmentButton.SetActive(false);

        Debug.Log(
            "All PPE videos completed. " +
            "Starting mannequin activity."
        );

        if (mannequinMenu != null)
        {
            mannequinMenu.ResetTraining();
            mannequinMenu.ShowMenus();
        }
        else
        {
            Debug.LogWarning(
                "Mannequin menu not assigned."
            );
        }
    }
}
