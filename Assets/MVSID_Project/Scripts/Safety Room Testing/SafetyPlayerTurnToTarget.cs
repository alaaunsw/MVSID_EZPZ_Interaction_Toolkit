
using UnityEngine;

public class SafetyPlayerTurnToTarget : MonoBehaviour
{
    [Header("Player References")]
    public Transform desktopPlayerRig;
    public Transform xrPlayerRig;

    [Header("Destination")]
    public Transform target;

    [Header("VR Settings")]
    public bool allowAutomaticVRTurn = false;

    [Header("Status")]
    [SerializeField] private bool hasTurned;

    public void TurnToTarget()
    {
        if (hasTurned || target == null)
            return;

        Transform activeRig = null;

        // Find the currently active player.

        if (xrPlayerRig != null &&
            xrPlayerRig.gameObject.activeInHierarchy)
        {
            if (!allowAutomaticVRTurn)
                return;

            activeRig = xrPlayerRig;
        }
        else if (desktopPlayerRig != null &&
                 desktopPlayerRig.gameObject.activeInHierarchy)
        {
            activeRig = desktopPlayerRig;
        }

        if (activeRig == null)
        {
            Debug.LogWarning(
                "[SAFETY] No active player rig found."
            );

            return;
        }

        Camera playerCamera =
            activeRig.GetComponentInChildren<Camera>();

        if (playerCamera == null)
            return;

        Vector3 direction =
            target.position -
            playerCamera.transform.position;

        direction.y = 0f;

        Vector3 currentForward =
            playerCamera.transform.forward;

        currentForward.y = 0f;

        if (direction.sqrMagnitude < 0.001f ||
            currentForward.sqrMagnitude < 0.001f)
        {
            return;
        }

        float turnAngle = Vector3.SignedAngle(
            currentForward,
            direction,
            Vector3.up
        );

        // Rotate around the player's camera
        // without changing their horizontal location.

        activeRig.RotateAround(
            playerCamera.transform.position,
            Vector3.up,
            turnAngle
        );

        hasTurned = true;

        Debug.Log(
            "[SAFETY] Player turned towards mannequin."
        );
    }

    public void ResetTurn()
    {
        hasTurned = false;
    }
}
