
using UnityEngine;

public class PPETrainingZoneTrigger : MonoBehaviour
{
    public PPETrainingController trainingController;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (trainingController != null)
            {
                trainingController.EnterTrainingZone();
            }
        }
    }
}
