
using UnityEngine;

public class SafetyKeyboardFollow : MonoBehaviour
{
    [Header("Camera")]
    public Transform targetCamera;

    [Header("Keyboard Placement")]
    public float distance = 1.5f;
    public float verticalOffset = -0.45f;

    [Header("Follow Behaviour")]
    public float positionFollowSpeed = 4f;
    public float recenterAngle = 45f;
    public float rotationFollowSpeed = 2f;

    private Vector3 stableForward;
    private bool initialized;

    private void OnEnable()
    {
        initialized = false;
    }

    private void LateUpdate()
    {
        if (targetCamera == null) return;

        Vector3 cameraForward = targetCamera.forward;
        cameraForward.y = 0f;

        if (cameraForward.sqrMagnitude < 0.001f)
            return;

        cameraForward.Normalize();

        if (!initialized)
        {
            stableForward = cameraForward;
            initialized = true;
        }

        // Keep direction stable during small head movements.
        if (Vector3.Angle(stableForward, cameraForward) > recenterAngle)
        {
            stableForward = Vector3.Slerp(
                stableForward,
                cameraForward,
                Mathf.Clamp01(rotationFollowSpeed * Time.deltaTime)
            ).normalized;
        }

        Vector3 destination =
            targetCamera.position +
            stableForward * distance +
            Vector3.up * verticalOffset;

        transform.position = Vector3.Lerp(
            transform.position,
            destination,
            Mathf.Clamp01(positionFollowSpeed * Time.deltaTime)
        );

        // Keep the keyboard upright and show its front face.
transform.rotation =
    Quaternion.LookRotation(stableForward, Vector3.up)
    * Quaternion.Euler(0f, 180f, 0f);
    }
}
