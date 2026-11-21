
using UnityEngine;

public class SafetyPanelFollow : MonoBehaviour
{
    [Header("Camera")]
    public Transform targetCamera;

    [Header("Position")]
    public float followDistance = 2f;
    public float verticalOffset = 0f;
    public float followSpeed = 4f;

    [Header("Rotation")]
    public bool facePlayer = true;

    [Header("Interaction Lock")]
    public bool pauseFollowing = false;

    void LateUpdate()
    {
        // Keep the panel stationary while the trainee is typing.
        if (pauseFollowing || targetCamera == null)
            return;

        Vector3 forward = targetCamera.forward;
        forward.y = 0f;

        if (forward.sqrMagnitude < 0.001f)
            return;

        forward.Normalize();

        Vector3 targetPosition =
            targetCamera.position +
            forward * followDistance +
            Vector3.up * verticalOffset;

        transform.position = Vector3.Lerp(
            transform.position,
            targetPosition,
            Mathf.Clamp01(followSpeed * Time.deltaTime)
        );

        if (facePlayer)
        {
            transform.rotation =
                Quaternion.LookRotation(forward, Vector3.up);
        }
    }

    // Freeze the panel at its current position and rotation.
    public void PauseFollowing()
    {
        pauseFollowing = true;
    }

    // Allow the panel to follow the player again.
    public void ResumeFollowing()
    {
        pauseFollowing = false;
    }
}
