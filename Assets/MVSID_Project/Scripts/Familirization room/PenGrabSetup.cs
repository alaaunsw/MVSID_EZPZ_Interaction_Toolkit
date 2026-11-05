
using UnityEngine;

public class PenGrabSetup : MonoBehaviour
{
    [Header("Marker References")]
    public Transform penTip;
    public Transform penGrabPoint;

    [Header("Grab Position")]
    [Tooltip("Offset from the pen tip in its local coordinates.")]
    public Vector3 grabOffset = new Vector3(0f, 0f, -0.08f);

    [Header("Grab Rotation")]
    public Vector3 grabRotation = Vector3.zero;

    [ContextMenu("Update Pen Grab Point")]
    public void UpdateGrabPoint()
    {
        if (penTip == null || penGrabPoint == null)
        {
            Debug.LogWarning(
                "Assign Pen Tip and Pen Grab Point first."
            );
            return;
        }

        // Position the grab point relative to the pen tip.
        penGrabPoint.position =
            penTip.TransformPoint(grabOffset);

        // Apply the desired grip orientation.
        penGrabPoint.rotation =
            penTip.rotation * Quaternion.Euler(grabRotation);

        Debug.Log("Pen grab point updated.");
    }
}

