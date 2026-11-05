
using UnityEngine;
using UnityEngine.XR;

public class XRWhiteboardDrawing : MonoBehaviour
{
    [Header("XR Controller")]
    public Transform rightController;
    public float maxDrawDistance = 5f;
    public float triggerThreshold = 0.5f;

    [Header("Whiteboard")]
    public WhiteboardDrawing whiteboardDrawing;
    public Collider drawingSurfaceCollider;

    private InputDevice rightHandDevice;
    private bool wasDrawing;

    void Update()
    {
        if (!rightHandDevice.isValid)
        {
            rightHandDevice = InputDevices.GetDeviceAtXRNode(
                XRNode.RightHand
            );
        }

        if (rightController == null ||
            whiteboardDrawing == null ||
            drawingSurfaceCollider == null)
        {
            return;
        }

        bool triggerPressed = false;

        if (rightHandDevice.isValid &&
            rightHandDevice.TryGetFeatureValue(
                CommonUsages.trigger,
                out float triggerValue))
        {
            triggerPressed = triggerValue >= triggerThreshold;
        }

        if (triggerPressed)
        {
            Debug.Log("XR TRIGGER DETECTED");

            Ray ray = new Ray(
                rightController.position,
                rightController.forward
            );

            if (drawingSurfaceCollider.Raycast(
                ray,
                out RaycastHit hit,
                maxDrawDistance))
            {
                Debug.Log(
                    "XR RAY HIT BACKBOARD: " + hit.textureCoord
                );

                if (!wasDrawing)
                {
                    whiteboardDrawing.BeginStroke();
                    wasDrawing = true;
                }

                whiteboardDrawing.DrawAtUV(hit.textureCoord);

                Debug.Log(
                    "XR DRAW HIT! UV: " + hit.textureCoord
                );
            }
            else
            {
                StopDrawing();
            }
        }
        else
        {
            StopDrawing();
        }
    }

    private void StopDrawing()
    {
        if (!wasDrawing)
            return;

        whiteboardDrawing.EndStroke();
        wasDrawing = false;
    }

    private void OnDisable()
    {
        StopDrawing();
    }
}
