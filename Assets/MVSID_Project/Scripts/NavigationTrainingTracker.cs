
using UnityEngine;
using UnityEngine.Events;

public class NavigationTrainingTracker : MonoBehaviour
{
    [Header("Player Movement")]
    public Transform playerTransform;
    public float requiredDistance = 3f;

    [Header("Completion Event")]
    public UnityEvent onNavigationCompleted;

    private Vector3 previousPosition;
    private float distanceTravelled;
    private bool tracking;
    private bool completed;

    public void BeginTracking()
    {
        if (playerTransform == null)
        {
            Debug.LogWarning("Player Transform is missing!");
            return;
        }

        previousPosition = playerTransform.position;
        distanceTravelled = 0f;
        completed = false;
        tracking = true;

        Debug.Log("Navigation tracking started.");
    }

    void Update()
    {
        if (!tracking || completed || playerTransform == null)
            return;

        Vector3 currentPosition = playerTransform.position;

        // Measure horizontal movement only.
        Vector3 previousFlat = new Vector3(
            previousPosition.x, 0f, previousPosition.z
        );

        Vector3 currentFlat = new Vector3(
            currentPosition.x, 0f, currentPosition.z
        );

        float movement = Vector3.Distance(
            previousFlat, currentFlat
        );

        distanceTravelled += movement;
        previousPosition = currentPosition;

        if (distanceTravelled >= requiredDistance)
        {
            completed = true;
            tracking = false;

            Debug.Log("Navigation activity completed!");
            onNavigationCompleted?.Invoke();
        }
    }
}
