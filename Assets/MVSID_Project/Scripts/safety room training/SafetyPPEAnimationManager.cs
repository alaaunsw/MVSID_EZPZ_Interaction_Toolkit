
using UnityEngine;

public class SafetyPPEAnimationManager : MonoBehaviour
{
    public static SafetyPPEAnimationManager Instance { get; private set; }

    private SafetyPPEInfoPoint currentOpenPoint;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning(
                "Duplicate SafetyPPEAnimationManager found."
            );
            enabled = false;
            return;
        }

        Instance = this;
    }

    public void Open(SafetyPPEInfoPoint point)
    {
        if (point == null)
            return;

        // Close the previously opened animation.
        if (currentOpenPoint != null &&
            currentOpenPoint != point)
        {
            currentOpenPoint.CloseInformation();
        }

        currentOpenPoint = point;
    }

    public void NotifyClosed(SafetyPPEInfoPoint point)
    {
        if (currentOpenPoint == point)
            currentOpenPoint = null;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}
