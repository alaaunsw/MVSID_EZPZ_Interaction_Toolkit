
using UnityEngine;

public class TestingPPEPanelLookAt : MonoBehaviour
{
    [Header("Player Camera")]
    public Transform playerCamera;

    [Header("Rotation Settings")]
    public bool rotateOnlyY = true;

    [Min(0f)]
    public float rotationSpeed = 8f;

    [Header("Optional Position Following")]
    public bool followPlayer = false;

    [Min(0.1f)]
    public float followDistance = 2f;

    public float verticalOffset = -0.05f;

    [Min(0f)]
    public float followSpeed = 4f;

    private void LateUpdate()
    {
        if (playerCamera == null)
        {
            Camera mainCamera = Camera.main;

            if (mainCamera == null)
                return;

            playerCamera = mainCamera.transform;
        }

        UpdatePanelPosition();
        UpdatePanelRotation();
    }

    private void UpdatePanelPosition()
    {
        if (!followPlayer)
            return;

        Vector3 forward = playerCamera.forward;

        // Keep the panel in front of the trainee,
        // even when looking up or down.
        forward.y = 0f;

        if (forward.sqrMagnitude < 0.001f)
            return;

        forward.Normalize();

        Vector3 targetPosition =
            playerCamera.position +
            forward * followDistance +
            Vector3.up * verticalOffset;

        transform.position = Vector3.Lerp(
            transform.position,
            targetPosition,
            Time.deltaTime * followSpeed
        );
    }

    private void UpdatePanelRotation()
    {
        Vector3 direction =
            transform.position - playerCamera.position;

        if (rotateOnlyY)
        {
            direction.y = 0f;
        }

        if (direction.sqrMagnitude < 0.001f)
            return;

        // For a normal Unity World Space Canvas,
        // this faces its front toward the camera.
        Quaternion targetRotation =
            Quaternion.LookRotation(direction);

        transform.rotation =
            Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                Time.deltaTime * rotationSpeed
            );
    }
}
