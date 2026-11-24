
using UnityEngine;

public class TestingSafetyZoneDialogueTrigger : MonoBehaviour
{
    [Header("Supervisor Dialogue")]
    public TestingInspectionDialogue dialogue;

    [Header("Pipeline Labels")]
    public InspectionOpeningLabels openingLabels;

    private bool hasTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (hasTriggered)
            return;

        if (!other.CompareTag("Player"))
            return;

        hasTriggered = true;

        Debug.Log(
            "[TESTING] Trainee entered the safety inspection zone."
        );

        // Display the three existing pipeline labels.
        if (openingLabels != null)
        {
            openingLabels.ShowLabels();

            Debug.Log(
                "[TESTING] Pipeline labels displayed."
            );
        }
        else
        {
            Debug.LogWarning(
                "[TESTING] Opening Labels is not assigned."
            );
        }

        // Display the next supervisor instruction.
        if (dialogue != null)
        {
            dialogue.ShowSafetyZoneMessage();
        }
    }
}
