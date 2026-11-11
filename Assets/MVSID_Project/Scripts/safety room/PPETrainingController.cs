
using UnityEngine;

public class PPETrainingController : MonoBehaviour
{
    [Header("PPE Interaction Points")]
    public GameObject[] ppeInfoPoints;

    [Header("Training Start Zone")]
    public GameObject trainingStartZone;

    private bool trainingSelected = false;
    private bool trainingStarted = false;
    private int currentPPEIndex = 0;

    private void Start()
    {
        HideAllPoints();

        if (trainingStartZone != null)
            trainingStartZone.SetActive(false);
    }

    private void HideAllPoints()
    {
        foreach (GameObject point in ppeInfoPoints)
        {
            if (point != null)
                point.SetActive(false);
        }
    }

    // Called after selecting Training Mode.
    public void BeginTraining()
    {
        trainingSelected = true;
        trainingStarted = false;
        currentPPEIndex = 0;

        HideAllPoints();

        if (trainingStartZone != null)
            trainingStartZone.SetActive(true);

        Debug.Log("Walk to the PPE training zone.");
    }

    // Called when the trainee enters the circle.
    public void EnterTrainingZone()
    {
        if (!trainingSelected || trainingStarted)
            return;

        trainingStarted = true;

        // Keep the circle visible.
        ShowCurrentPPE();

        Debug.Log("PPE training started.");
    }

    private void ShowCurrentPPE()
    {
        HideAllPoints();

        if (currentPPEIndex < ppeInfoPoints.Length)
        {
            GameObject point =
                ppeInfoPoints[currentPPEIndex];

            if (point != null)
            {
                point.SetActive(true);
                Debug.Log("Learn about: " + point.name);
            }
        }
        else
        {
            Debug.Log(
                "All PPE equipment completed. Assessment is next."
            );
        }
    }

    // Connect this to a NEXT EQUIPMENT button.
    public void NextEquipment()
    {
        if (!trainingStarted)
            return;

        // Close the current animation before advancing.
        if (currentPPEIndex < ppeInfoPoints.Length)
        {
            GameObject currentPoint =
                ppeInfoPoints[currentPPEIndex];

            if (currentPoint != null)
            {
                SafetyPPEInfoPoint info =
                    currentPoint.GetComponent<SafetyPPEInfoPoint>();

                if (info != null)
                    info.CloseInformation();
            }
        }

        currentPPEIndex++;
        ShowCurrentPPE();
    }
}
