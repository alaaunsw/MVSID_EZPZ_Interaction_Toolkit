
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public class TestingPressureGaugeFaceFix : MonoBehaviour
{
    [Header("Original Gauge Texture")]
    public Texture2D gaugeTexture;

    [Header("Dial Area in Texture Atlas")]
    public Rect dialUV = new Rect(
        0.10f, 0.25f, 0.16f, 0.28f
    );

    [Header("Dial Placement")]
    public Vector3 localPosition = Vector3.zero;
    public Vector3 localRotation = Vector3.zero;

    [Min(0.001f)]
    public float dialRadius = 0.10f;

    [Header("Appearance")]
    public Color dialTint = Color.white;

    [Header("Generated Dial")]
    public GameObject generatedDial;

    [ContextMenu("Create / Update Gauge Face")]
    public void CreateGaugeFace()
    {
        if (gaugeTexture == null)
        {
            Debug.LogError(
                "Assign ModularPipes_Small_BaseColor texture first."
            );
            return;
        }

        if (generatedDial == null)
        {
            generatedDial = new GameObject(
                "Testing_VisiblePressureGaugeFace"
            );

            generatedDial.transform.SetParent(
                transform, false
            );

            generatedDial.AddComponent<MeshFilter>();
            generatedDial.AddComponent<MeshRenderer>();

#if UNITY_EDITOR
            if (!Application.isPlaying)
                Undo.RegisterCreatedObjectUndo(
                    generatedDial,
                    "Create Pressure Gauge Face"
                );
#endif
        }

        generatedDial.transform.localPosition = localPosition;
        generatedDial.transform.localEulerAngles = localRotation;
        generatedDial.transform.localScale = Vector3.one;

        MeshFilter filter =
            generatedDial.GetComponent<MeshFilter>();

        MeshRenderer renderer =
            generatedDial.GetComponent<MeshRenderer>();

        Mesh mesh = new Mesh();
        mesh.name = "Pressure Gauge Circular Dial";

        const int segments = 64;

        Vector3[] vertices =
            new Vector3[segments + 1];

        Vector2[] uvs =
            new Vector2[segments + 1];

        int[] triangles =
            new int[segments * 3];

        vertices[0] = Vector3.zero;

        uvs[0] = new Vector2(
            dialUV.x + dialUV.width * 0.5f,
            dialUV.y + dialUV.height * 0.5f
        );

        for (int i = 0; i < segments; i++)
        {
            float angle =
                i * Mathf.PI * 2f / segments;

            float x = Mathf.Cos(angle);
            float y = Mathf.Sin(angle);

            vertices[i + 1] =
                new Vector3(
                    x * dialRadius,
                    y * dialRadius,
                    0f
                );

            uvs[i + 1] = new Vector2(
                dialUV.x +
                    (x * 0.5f + 0.5f) * dialUV.width,
                dialUV.y +
                    (y * 0.5f + 0.5f) * dialUV.height
            );

            int next = (i + 1) % segments;

            triangles[i * 3] = 0;
            triangles[i * 3 + 1] = i + 1;
            triangles[i * 3 + 2] = next + 1;
        }

        mesh.vertices = vertices;
        mesh.uv = uvs;
        mesh.triangles = triangles;

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        if (filter.sharedMesh != null &&
            filter.sharedMesh.name ==
            "Pressure Gauge Circular Dial")
        {
            if (Application.isPlaying)
                Destroy(filter.sharedMesh);
#if UNITY_EDITOR
            else
                DestroyImmediate(filter.sharedMesh);
#endif
        }

        filter.sharedMesh = mesh;

        Shader shader = Shader.Find("Unlit/Texture");

        if (shader == null)
        {
            Debug.LogError(
                "Unlit/Texture shader not found."
            );
            return;
        }

        Material material = renderer.sharedMaterial;

        if (material == null ||
            material.name != "Testing_GaugeFace_Material")
        {
            material = new Material(shader);
            material.name = "Testing_GaugeFace_Material";
        }

        material.mainTexture = gaugeTexture;
        material.color = dialTint;

        renderer.sharedMaterial = material;

#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            EditorUtility.SetDirty(this);
            EditorSceneManager.MarkSceneDirty(gameObject.scene);
        }
#endif

        Debug.Log(
            "[TESTING] Pressure gauge face created."
        );
    }

    [ContextMenu("Remove Generated Gauge Face")]
    public void RemoveGaugeFace()
    {
        if (generatedDial == null)
            return;

#if UNITY_EDITOR
        if (!Application.isPlaying)
            Undo.DestroyObjectImmediate(generatedDial);
        else
            Destroy(generatedDial);
#else
        Destroy(generatedDial);
#endif

        generatedDial = null;
    }
}
