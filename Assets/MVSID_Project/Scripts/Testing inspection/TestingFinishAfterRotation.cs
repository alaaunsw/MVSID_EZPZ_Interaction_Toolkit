
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class TestingFinishAfterRotation : MonoBehaviour
{
    [Header("Existing Close-Up Pipe")]
    public Transform viewerMesh;
    public Camera playerCamera;

    [Header("Supervisor Dialogue")]
    public TestingInspectionDialogue dialogue;

    [Header("Visual Inspection Assessment")]
    public TestingVisualInspectionAssessment visualAssessment;

    [Header("Rotation Requirement")]
    [Range(1f, 90f)]
    public float requiredRotationDegrees = 20f;

    [Header("Floating Text Appearance")]
    public string buttonText = "FINISH INSPECTION";

    public Color normalColor =
        new Color32(255, 213, 79, 255);

    public Color hoverColor =
        new Color32(255, 245, 157, 255);

    public float fontSize = 4f;
    public float textScale = 0.08f;

    [Header("Text Position")]
    public float horizontalOffset = 0.8f;
    public float verticalOffset = 0.5f;

    private TextMeshPro floatingText;
    private GameObject floatingObject;
    private Material privateTextMaterial;

    private Collider viewerCollider;
    private BoxCollider finishTextCollider;

    private bool dragging;
    private bool finishAvailable;
    private bool inspectionFinished;

    private Quaternion rotationAtStart;

    private void Start()
    {
        if (playerCamera == null)
            playerCamera = Camera.main;

        if (viewerMesh == null)
        {
            Debug.LogError(
                "[TESTING] Viewer Mesh is missing."
            );
            return;
        }

        viewerCollider =
            viewerMesh.GetComponent<Collider>();

        if (viewerCollider == null)
        {
            Debug.LogError(
                "[TESTING] Viewer Mesh needs a Collider."
            );
            return;
        }

        CreateFloatingText();
    }

    private void OnEnable()
    {
        dragging = false;
        finishAvailable = false;
        inspectionFinished = false;

        if (floatingObject != null)
            floatingObject.SetActive(false);
    }

    // ========================================
    // CREATE FLOATING FINISH TEXT
    // ========================================

    private void CreateFloatingText()
    {
        floatingObject = new GameObject(
            "Testing_FinishInspection_FloatingText"
        );

        // Keep floating text outside the rotating model.
        floatingObject.transform.SetParent(null);

        floatingText =
            floatingObject.AddComponent<TextMeshPro>();

        floatingText.text = buttonText;
        floatingText.fontSize = fontSize;
        floatingText.alignment =
            TextAlignmentOptions.Center;

        floatingText.enableWordWrapping = false;
        floatingText.color = normalColor;

        // Create a private material instance.
        // Never change the shared TMP material.
        if (floatingText.fontSharedMaterial != null)
        {
            privateTextMaterial = new Material(
                floatingText.fontSharedMaterial
            );

            floatingText.fontSharedMaterial =
                privateTextMaterial;
        }

        floatingObject.transform.localScale =
            Vector3.one * textScale;

        floatingText.ForceMeshUpdate();

        // Collider for desktop and EZPZ interaction.
        finishTextCollider =
            floatingObject.AddComponent<BoxCollider>();

        Bounds textBounds = floatingText.bounds;

        finishTextCollider.center = textBounds.center;

        finishTextCollider.size = new Vector3(
            Mathf.Max(1f, textBounds.size.x + 0.8f),
            Mathf.Max(0.6f, textBounds.size.y + 0.5f),
            0.5f
        );

        finishTextCollider.isTrigger = false;

        // EZPZ interaction.
        InteractableGeneral interaction =
            floatingObject.AddComponent<InteractableGeneral>();

        interaction.onHoverEnter.AddListener(
            SetHoverColour
        );

        interaction.onHoverExit.AddListener(
            SetNormalColour
        );

        interaction.onPrimaryInteract.AddListener(
            FinishInspection
        );

        UpdateFloatingPosition();

        // Hidden until required rotation is completed.
        floatingObject.SetActive(false);

        Debug.Log(
            "[TESTING] Finish text created and hidden."
        );
    }

    // ========================================
    // DESKTOP HOVER AND LEFT MOUSE CLICK
    // ========================================

    private void Update()
    {
        if (!finishAvailable ||
            inspectionFinished ||
            finishTextCollider == null ||
            playerCamera == null ||
            Mouse.current == null)
        {
            return;
        }

        Mouse mouse = Mouse.current;

        Ray ray = playerCamera.ScreenPointToRay(
            mouse.position.ReadValue()
        );

        bool isHovering =
            finishTextCollider.Raycast(
                ray,
                out RaycastHit hit,
                100f
            );

        if (isHovering)
        {
            SetHoverColour();

            if (mouse.leftButton.wasPressedThisFrame)
            {
                FinishInspection();
            }
        }
        else
        {
            SetNormalColour();
        }
    }

    // ========================================
    // DETECT RIGHT MOUSE ROTATION
    // ========================================

    private void LateUpdate()
    {
        if (viewerMesh == null ||
            viewerCollider == null ||
            playerCamera == null ||
            inspectionFinished)
        {
            return;
        }

        if (finishAvailable)
            return;

        if (Mouse.current == null)
            return;

        Mouse mouse = Mouse.current;

        if (mouse.rightButton.wasPressedThisFrame)
        {
            Ray ray =
                playerCamera.ScreenPointToRay(
                    mouse.position.ReadValue()
                );

            if (viewerCollider.Raycast(
                ray,
                out RaycastHit hit,
                100f))
            {
                dragging = true;

                rotationAtStart = transform.rotation;

                Debug.Log(
                    "[TESTING] Right mouse rotation started."
                );
            }
        }

        // Reveal finish text only after actual rotation.
        if (dragging && mouse.rightButton.isPressed)
        {
            float rotatedDegrees =
                Quaternion.Angle(
                    rotationAtStart,
                    transform.rotation
                );

            if (rotatedDegrees >= requiredRotationDegrees)
            {
                RevealFinishText();
            }
        }

        if (mouse.rightButton.wasReleasedThisFrame)
        {
            dragging = false;
        }
    }

    // ========================================
    // POSITION FLOATING TEXT
    // ========================================

    private void UpdateFloatingPosition()
    {
        if (floatingObject == null ||
            playerCamera == null ||
            viewerMesh == null)
        {
            return;
        }

        Renderer meshRenderer =
            viewerMesh.GetComponent<Renderer>();

        if (meshRenderer == null)
        {
            meshRenderer =
                viewerMesh.GetComponentInChildren<Renderer>();
        }

        Vector3 center =
            meshRenderer != null
                ? meshRenderer.bounds.center
                : viewerMesh.position;

        floatingObject.transform.position =
            center +
            playerCamera.transform.right * horizontalOffset +
            Vector3.up * verticalOffset;

        // Face the player.
        floatingObject.transform.rotation =
            playerCamera.transform.rotation;
    }

    // ========================================
    // REVEAL AFTER ROTATING
    // ========================================

    private void RevealFinishText()
    {
        if (finishAvailable || floatingObject == null)
            return;

        finishAvailable = true;
        dragging = false;

        UpdateFloatingPosition();
        SetNormalColour();

        floatingObject.SetActive(true);

        Debug.Log(
            "[TESTING] Required rotation completed. " +
            "FINISH INSPECTION is now visible."
        );
    }

    // ========================================
    // HOVER COLOURS
    // ========================================

    public void SetHoverColour()
    {
        if (floatingText != null)
            floatingText.color = hoverColor;
    }

    public void SetNormalColour()
    {
        if (floatingText != null)
            floatingText.color = normalColor;
    }

    // ========================================
    // FINISH INSPECTION
    // ========================================

    public void FinishInspection()
    {
        if (!finishAvailable || inspectionFinished)
            return;

        inspectionFinished = true;

        Debug.Log(
            "[TESTING] Visual inspection completed."
        );

        // Hide the floating yellow text.
        if (floatingObject != null)
            floatingObject.SetActive(false);

        // Supervisor asks the trainee what they observed.
        if (dialogue != null)
        {
            dialogue.ShowVisualInspectionQuestion();
        }
        else
        {
            Debug.LogWarning(
                "[TESTING] Supervisor dialogue is not assigned."
            );
        }

        // NEW: Open the floating neon multiple-choice question.
        if (visualAssessment != null)
        {
            visualAssessment.ShowQuestion();

            Debug.Log(
                "[TESTING] Visual inspection assessment opened."
            );
        }
        else
        {
            Debug.LogWarning(
                "[TESTING] Visual Assessment is not assigned."
            );
        }

        // Close the close-up pipeline model.
        gameObject.SetActive(false);
    }

    // ========================================
    // CLEANUP
    // ========================================

    private void OnDisable()
    {
        dragging = false;

        if (floatingObject != null)
            floatingObject.SetActive(false);
    }

    private void OnDestroy()
    {
        if (floatingObject != null)
            Destroy(floatingObject);

        if (privateTextMaterial != null)
            Destroy(privateTextMaterial);
    }
}
