
using UnityEngine;
using UnityEngine.InputSystem;

public class WhiteboardMouseInput : MonoBehaviour
{
    [Header("References")]
    public Camera playerCamera;
    public WhiteboardDrawing whiteboard;

    [Header("Interaction")]
    public float maxDrawingDistance = 5f;
    public bool useScreenCenter = true;

    private bool wasDrawing;

    void Update()
    {
        if (playerCamera == null || whiteboard == null)
            return;

        // New Unity Input System
        if (Mouse.current == null)
            return;

        bool mouseHeld =
            Mouse.current.leftButton.isPressed;

        if (!mouseHeld)
        {
            StopDrawing();
            return;
        }

        Ray ray;

        if (useScreenCenter)
        {
            ray = playerCamera.ViewportPointToRay(
                new Vector3(0.5f, 0.5f, 0f)
            );
        }
        else
        {
            Vector2 mousePosition =
                Mouse.current.position.ReadValue();

            ray = playerCamera.ScreenPointToRay(
                mousePosition
            );
        }

        if (Physics.Raycast(
            ray,
            out RaycastHit hit,
            maxDrawingDistance))
        {
            if (hit.collider.gameObject ==
                whiteboard.gameObject)
            {
                if (!wasDrawing)
                {
                    whiteboard.BeginStroke();
                    wasDrawing = true;
                }

                whiteboard.DrawAtUV(
                    hit.textureCoord
                );

                return;
            }
        }

        StopDrawing();
    }

    private void StopDrawing()
    {
        if (wasDrawing && whiteboard != null)
            whiteboard.EndStroke();

        wasDrawing = false;
    }

    void OnDisable()
    {
        StopDrawing();
    }
}
