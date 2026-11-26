
using UnityEngine;
using TMPro;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public class SafetyZoneDirectionIndicator : MonoBehaviour
{
    [Header("Blue Zone")]
    public Transform blueZone;

    [Header("Navigation Indicator")]
    public GameObject indicatorRoot;
    public Transform arrow;
    public TextMeshPro instructionText;

    [Header("Player Camera")]
    public Camera playerCamera;

    [Header("Instruction")]
    [TextArea(2, 4)]
    public string instruction =
        "START HERE\nWalk towards the blue zone";

    [Header("Position and Size")]
    public float heightAboveZone = 1.5f;
    public float indicatorScale = 1f;

    [Header("Arrow Animation")]
    public float floatingHeight = 0.15f;
    public float floatingSpeed = 2.5f;

    [Header("Appearance")]
    public Color arrowColor = Color.yellow;
    public Color textColor = Color.yellow;

    public bool facePlayer = true;

    private Vector3 arrowStartPosition;
    private bool indicatorVisible;

    // =====================================
    // AUTOMATICALLY BUILD INDICATOR
    // =====================================

    [ContextMenu("Build Safety Zone Indicator")]
    public void BuildIndicator()
    {
        if (Application.isPlaying)
        {
            Debug.LogWarning(
                "Stop Play Mode before building."
            );

            return;
        }

#if UNITY_EDITOR
        Undo.RecordObject(
            this,
            "Build Safety Zone Indicator"
        );
#endif

        if (indicatorRoot != null)
        {
            if (indicatorRoot == gameObject ||
                transform.IsChildOf(indicatorRoot.transform))
            {
                Debug.LogError(
                    "Indicator Root must be a separate child."
                );

                return;
            }

#if UNITY_EDITOR
            Undo.DestroyObjectImmediate(indicatorRoot);
#else
            DestroyImmediate(indicatorRoot);
#endif
        }

        // =====================================
        // CREATE INDICATOR ROOT
        // =====================================

        indicatorRoot =
            new GameObject("SafetyZoneIndicator");

        indicatorRoot.transform.SetParent(
            transform,
            false
        );

#if UNITY_EDITOR
        Undo.RegisterCreatedObjectUndo(
            indicatorRoot,
            "Create Safety Zone Indicator"
        );
#endif

        indicatorRoot.transform.localScale =
            Vector3.one * indicatorScale;

        // =====================================
        // CREATE ARROW
        // =====================================

        GameObject arrowObject =
            new GameObject("Arrow");

        arrowObject.transform.SetParent(
            indicatorRoot.transform,
            false
        );

        arrowObject.transform.localPosition =
            Vector3.zero;

        arrow = arrowObject.transform;

        // Create a solid arrow mesh.
        // No special font or imported model required.

        MeshFilter meshFilter =
            arrowObject.AddComponent<MeshFilter>();

        MeshRenderer meshRenderer =
            arrowObject.AddComponent<MeshRenderer>();

        meshFilter.sharedMesh = CreateArrowMesh();

        // =====================================
        // ARROW MATERIAL
        // =====================================

        Shader shader =
            Shader.Find("Universal Render Pipeline/Unlit");

        if (shader == null)
        {
            shader = Shader.Find("Unlit/Color");
        }

        if (shader != null)
        {
            Material material =
                new Material(shader);

            material.name =
                "SafetyZoneArrow_Yellow";

            if (material.HasProperty("_BaseColor"))
            {
                material.SetColor(
                    "_BaseColor",
                    arrowColor
                );
            }

            if (material.HasProperty("_Color"))
            {
                material.SetColor(
                    "_Color",
                    arrowColor
                );
            }

            meshRenderer.sharedMaterial = material;
        }

        // =====================================
        // CREATE INSTRUCTION TEXT
        // =====================================

        GameObject textObject =
            new GameObject("InstructionText");

        textObject.transform.SetParent(
            indicatorRoot.transform,
            false
        );

        textObject.transform.localPosition =
            new Vector3(0f, 0.55f, 0f);

        textObject.transform.localScale =
            Vector3.one * 0.1f;

        instructionText =
            textObject.AddComponent<TextMeshPro>();

        instructionText.text = instruction;

        instructionText.fontSize = 4f;

        instructionText.color = textColor;

        instructionText.fontStyle =
            FontStyles.Bold;

        instructionText.alignment =
            TextAlignmentOptions.Center;

        instructionText.rectTransform.sizeDelta =
            new Vector2(18f, 5f);

        instructionText.enableAutoSizing = true;

        instructionText.fontSizeMin = 2f;
        instructionText.fontSizeMax = 4f;

        // =====================================
        // POSITION ABOVE BLUE ZONE
        // =====================================

        PositionIndicator();

        arrowStartPosition =
            arrow.localPosition;

        // Keep visible in Edit Mode
        // so the design can be adjusted.

        indicatorRoot.SetActive(true);

#if UNITY_EDITOR
        EditorUtility.SetDirty(this);

        EditorSceneManager.MarkSceneDirty(
            gameObject.scene
        );
#endif

        Debug.Log(
            "[SAFETY] Direction indicator created."
        );
    }

    // =====================================
    // GENERATE SOLID DOWNWARD ARROW
    // =====================================

    private Mesh CreateArrowMesh()
    {
        Mesh mesh = new Mesh();

        mesh.name = "SafetyZoneArrow";

        Vector3[] vertices =
        {
            // Arrow shaft

            new Vector3(-0.10f, 0.30f, 0f),
            new Vector3( 0.10f, 0.30f, 0f),
            new Vector3( 0.10f, 0.00f, 0f),
            new Vector3(-0.10f, 0.00f, 0f),

            // Arrow head

            new Vector3(-0.25f, 0.00f, 0f),
            new Vector3( 0.25f, 0.00f, 0f),
            new Vector3( 0.00f,-0.30f, 0f)
        };

        // Both sides of the arrow are visible.

        int[] triangles =
        {
            0, 1, 2,
            0, 2, 3,
            4, 5, 6,

            2, 1, 0,
            3, 2, 0,
            6, 5, 4
        };

        mesh.vertices = vertices;
        mesh.triangles = triangles;

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        return mesh;
    }

    // =====================================
    // POSITION INDICATOR
    // =====================================

    [ContextMenu("Update Indicator Position")]
    public void PositionIndicator()
    {
        if (indicatorRoot == null)
            return;

        Vector3 basePosition =
            blueZone != null

            ? blueZone.position

            : transform.position;

        indicatorRoot.transform.position =
            basePosition +
            Vector3.up * heightAboveZone;

        indicatorRoot.transform.localScale =
            Vector3.one * indicatorScale;
    }

    // =====================================
    // RUNTIME INITIALISATION
    // =====================================

    private void Awake()
    {
        PositionIndicator();

        if (arrow != null)
        {
            arrowStartPosition =
                arrow.localPosition;
        }

        // Hidden until successful login.

        HideIndicator();
    }

    // =====================================
    // ANIMATION AND PLAYER FACING
    // =====================================

    private void LateUpdate()
    {
        if (!indicatorVisible ||
            indicatorRoot == null)
        {
            return;
        }

        // Floating arrow animation.

        if (arrow != null)
        {
            float movement =
                Mathf.Sin(
                    Time.unscaledTime * floatingSpeed
                ) * floatingHeight;

            arrow.localPosition =
                arrowStartPosition +
                Vector3.up * movement;
        }

        // Keep the entire indicator
        // upright and facing the trainee.

        if (facePlayer)
        {
            if (playerCamera == null ||
                !playerCamera.isActiveAndEnabled)
            {
                playerCamera = Camera.main;
            }

            if (playerCamera != null)
            {
                Vector3 direction =
                    indicatorRoot.transform.position -
                    playerCamera.transform.position;

                direction.y = 0f;

                if (direction.sqrMagnitude > 0.001f)
                {
                    indicatorRoot.transform.rotation =
                        Quaternion.LookRotation(
                            direction,
                            Vector3.up
                        );
                }
            }
        }
    }

    // =====================================
    // SHOW AFTER LOGIN
    // =====================================

    public void ShowIndicator()
    {
        if (indicatorRoot == null)
        {
            Debug.LogWarning(
                "[SAFETY] Build the indicator first."
            );

            return;
        }

        PositionIndicator();

        indicatorVisible = true;

        if (instructionText != null)
        {
            instructionText.text = instruction;
            instructionText.color = textColor;
        }

        indicatorRoot.SetActive(true);

        Debug.Log(
            "[SAFETY] Navigation indicator displayed."
        );
    }

    // =====================================
    // HIDE WHEN ENTERING BLUE ZONE
    // =====================================

    public void HideIndicator()
    {
        indicatorVisible = false;

        if (indicatorRoot != null &&
            indicatorRoot != gameObject &&
            !transform.IsChildOf(
                indicatorRoot.transform))
        {
            indicatorRoot.SetActive(false);
        }
    }
}
