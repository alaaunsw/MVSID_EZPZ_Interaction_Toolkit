
using UnityEngine;
using UnityEngine.Events;

public class NavigationTrainingTracker : MonoBehaviour
{
    [Header("Player Movement")]
    public Transform playerTransform;
    public float requiredDistance = 3f;

    [Header("Navigation Target Zone")]
    public GameObject navigationTargetZone;

    [Header("Old Continue Button")]
    public GameObject continueButton;

    [Header("Completion Event")]
    public UnityEvent onNavigationCompleted;

    private Vector3 previousPosition;
    private float distanceTravelled;
    private bool tracking;
    private bool completed;

    private void Start()
    {
        // Hide the zone until movement is completed.
        if (navigationTargetZone != null)
            navigationTargetZone.SetActive(false);

        // The Continue button is no longer needed.
        if (continueButton != null)
            continueButton.SetActive(false);
    }

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

        // Hide the target until the user has moved enough.
        if (navigationTargetZone != null)
            navigationTargetZone.SetActive(false);

        if (continueButton != null)
            continueButton.SetActive(false);

        Debug.Log("Navigation tracking started.");
    }

    private void Update()
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

            Debug.Log(
                "Movement completed! Navigation zone activated."
            );

            // Show the pulsing navigation zone.
            if (navigationTargetZone != null)
                navigationTargetZone.SetActive(true);

            // Keep the old Continue button hidden.
            if (continueButton != null)
                continueButton.SetActive(false);

            onNavigationCompleted?.Invoke();
        }
    }
}
