
using UnityEngine;
using UnityEngine.InputSystem;

public class PipeInspectionRotation : MonoBehaviour
{
    [Header("Inspection Model")]
    public Transform viewerMesh;

    [Header("Camera")]
    public Camera playerCamera;

    [Header("Rotation")]
    public bool allowRotation = true;
    public float rotationSpeed = 0.3f;

    private Collider modelCollider;
    private bool isDragging;

    private void Start()
    {
        if (playerCamera == null)
            playerCamera = Camera.main;

        if (viewerMesh == null)
        {
            Debug.LogError("Viewer Mesh not assigned.");
            return;
        }

        modelCollider = viewerMesh.GetComponent<Collider>();

        if (modelCollider == null)
        {
            Debug.LogError(
                "Viewer Mesh needs a Box Collider."
            );
        }
    }

    private void Update()
    {
        if (!allowRotation ||
            viewerMesh == null ||
            playerCamera == null ||
            modelCollider == null ||
            Mouse.current == null)
            return;

        Mouse mouse = Mouse.current;

        // Start rotation when right-clicking the model.
        if (mouse.rightButton.wasPressedThisFrame)
        {
            Ray ray = playerCamera.ScreenPointToRay(
                mouse.position.ReadValue()
            );

            if (modelCollider.Raycast(
                ray,
                out RaycastHit hit,
                100f))
            {
                isDragging = true;
                Debug.Log("Pipe rotation started.");
            }
        }

        // Stop rotating when button released.
        if (mouse.rightButton.wasReleasedThisFrame)
        {
            isDragging = false;
        }

        if (!isDragging || !mouse.rightButton.isPressed)
            return;

        Vector2 movement = mouse.delta.ReadValue();

        float horizontal =
            movement.x * rotationSpeed;

        float vertical =
            movement.y * rotationSpeed;

        Renderer meshRenderer =
            viewerMesh.GetComponent<Renderer>();

        Vector3 center = meshRenderer != null
            ? meshRenderer.bounds.center
            : viewerMesh.position;

        // Rotate the entire inspection model.
        transform.RotateAround(
            center,
            Vector3.up,
            -horizontal
        );

        transform.RotateAround(
            center,
            playerCamera.transform.right,
            vertical
        );
    }
}
