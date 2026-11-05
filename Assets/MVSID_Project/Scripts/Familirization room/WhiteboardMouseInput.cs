
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

    private bool wasPainting;
    private bool wasErasing;

    void Update()
    {
        if (playerCamera == null || whiteboard == null ||
            Mouse.current == null)
        {
            StopPainting();
            return;
        }

        bool drawing = Mouse.current.leftButton.isPressed;
        bool erasing = Mouse.current.rightButton.isPressed && !drawing;

        if (!drawing && !erasing)
        {
            StopPainting();
            return;
        }

        if (wasPainting && wasErasing != erasing)
            whiteboard.EndStroke();

        Ray ray;

        if (useScreenCenter)
        {
            ray = playerCamera.ViewportPointToRay(
                new Vector3(0.5f, 0.5f, 0f)
            );
        }
        else
        {
            ray = playerCamera.ScreenPointToRay(
                Mouse.current.position.ReadValue()
            );
        }

        if (Physics.Raycast(
            ray,
            out RaycastHit hit,
            maxDrawingDistance))
        {
            if (hit.collider.gameObject == whiteboard.gameObject)
            {
                if (!wasPainting || wasErasing != erasing)
                    whiteboard.BeginStroke();

                if (erasing)
                    whiteboard.EraseAtUV(hit.textureCoord);
                else
                    whiteboard.DrawAtUV(hit.textureCoord);

                wasPainting = true;
                wasErasing = erasing;
                return;
            }
        }

        StopPainting();
    }

    private void StopPainting()
    {
        if (wasPainting && whiteboard != null)
            whiteboard.EndStroke();

        wasPainting = false;
        wasErasing = false;
    }

    void OnDisable()
    {
        StopPainting();
    }
}
