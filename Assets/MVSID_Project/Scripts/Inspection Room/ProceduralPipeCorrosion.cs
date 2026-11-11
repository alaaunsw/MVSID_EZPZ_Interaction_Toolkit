
using UnityEngine;
using UnityEngine.Rendering;

public class ProceduralPipeCorrosion : MonoBehaviour
{
    public enum PipeAxis { X, Y, Z }
    public enum CorrosionLevel { Normal, Moderate, Critical }

    [Header("Pipe Geometry")]
    public PipeAxis pipeAxis = PipeAxis.X;
    public float pipeRadius = 0.35f;
    public Vector3 localPipeCenter = Vector3.zero;

    [Header("Corrosion Position")]
    public float positionAlongPipe = 0f;
    public float patchLength = 0.8f;
    public float centerAngle = 180f;
    public float angularCoverage = 100f;
    public float surfaceOffset = 0.005f;

    [Header("Corrosion Appearance")]
    public CorrosionLevel corrosionLevel = CorrosionLevel.Moderate;
    public int randomSeed = 493;

    [Header("Generated Object")]
    public GameObject corrosionObject;

    private void Start()
    {
        if (Application.isPlaying)
            GenerateCorrosion();
    }

    [ContextMenu("Generate Corrosion")]
    public void GenerateCorrosion()
    {
        RemoveCorrosion();

        if (corrosionLevel == CorrosionLevel.Normal)
        {
            Debug.Log("Normal pipe: no visible corrosion generated.");
            return;
        }

        float intensity = corrosionLevel == CorrosionLevel.Critical
            ? 0.9f : 0.55f;

        Mesh mesh = CreatePatchMesh();

        Texture2D texture = CreateRustTexture(intensity);

        Shader shader =
            Shader.Find("Universal Render Pipeline/Lit");

        if (shader == null)
        {
            Debug.LogError("URP Lit shader not found.");
            return;
        }

        Material material = new Material(shader);
        material.name = "GeneratedRustMaterial";

        material.SetTexture("_BaseMap", texture);
        material.SetColor("_BaseColor", Color.white);
        material.SetFloat("_Metallic", 0f);
        material.SetFloat("_Smoothness", 0.12f);

        // Enable transparent rendering in URP.
        material.SetFloat("_Surface", 1f);
        material.SetFloat("_Blend", 0f);
        material.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
        material.SetFloat(
            "_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
        material.SetFloat("_ZWrite", 0f);
        material.renderQueue = (int)RenderQueue.Transparent;
        material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");

        corrosionObject = new GameObject("Generated_Corrosion");

        corrosionObject.transform.SetParent(transform, false);
        corrosionObject.transform.localPosition = Vector3.zero;
        corrosionObject.transform.localRotation = Quaternion.identity;
        corrosionObject.transform.localScale = Vector3.one;

        MeshFilter filter =
            corrosionObject.AddComponent<MeshFilter>();

        MeshRenderer renderer =
            corrosionObject.AddComponent<MeshRenderer>();

        filter.sharedMesh = mesh;
        renderer.sharedMaterial = material;
        renderer.shadowCastingMode = ShadowCastingMode.Off;

        Debug.Log("Procedural corrosion generated.");
    }

    private Mesh CreatePatchMesh()
    {
        int lengthSegments = 36;
        int angleSegments = 28;

        Vector3[] vertices = new Vector3[
            (lengthSegments + 1) * (angleSegments + 1)];

        Vector3[] normals = new Vector3[vertices.Length];
        Vector2[] uv = new Vector2[vertices.Length];

        int[] triangles =
            new int[lengthSegments * angleSegments * 6];

        Vector3 axis, radial1, radial2;

        if (pipeAxis == PipeAxis.X)
        {
            axis = Vector3.right;
            radial1 = Vector3.up;
            radial2 = Vector3.forward;
        }
        else if (pipeAxis == PipeAxis.Y)
        {
            axis = Vector3.up;
            radial1 = Vector3.forward;
            radial2 = Vector3.right;
        }
        else
        {
            axis = Vector3.forward;
            radial1 = Vector3.right;
            radial2 = Vector3.up;
        }

        float radius = Mathf.Max(0.001f,
            pipeRadius + surfaceOffset);

        int index = 0;

        for (int i = 0; i <= lengthSegments; i++)
        {
            float u = (float)i / lengthSegments;

            float longitudinal =
                positionAlongPipe + (u - 0.5f) * patchLength;

            for (int j = 0; j <= angleSegments; j++)
            {
                float v = (float)j / angleSegments;

                float angle =
                    (centerAngle +
                    (v - 0.5f) * angularCoverage) *
                    Mathf.Deg2Rad;

                Vector3 radial =
                    radial1 * Mathf.Cos(angle) +
                    radial2 * Mathf.Sin(angle);

                vertices[index] =
                    localPipeCenter +
                    axis * longitudinal +
                    radial * radius;

                normals[index] = radial;
                uv[index] = new Vector2(u, v);

                index++;
            }
        }

        int t = 0;
        int stride = angleSegments + 1;

        for (int i = 0; i < lengthSegments; i++)
        {
            for (int j = 0; j < angleSegments; j++)
            {
                int a = i * stride + j;
                int b = (i + 1) * stride + j;
                int c = a + 1;
                int d = b + 1;

                triangles[t++] = a;
                triangles[t++] = c;
                triangles[t++] = b;

                triangles[t++] = b;
                triangles[t++] = c;
                triangles[t++] = d;
            }
        }

        Mesh mesh = new Mesh();
        mesh.name = "ProceduralRustPatch";
        mesh.vertices = vertices;
        mesh.normals = normals;
        mesh.uv = uv;
        mesh.triangles = triangles;
        mesh.RecalculateBounds();

        return mesh;
    }

    private Texture2D CreateRustTexture(float intensity)
    {
        int width = 256;
        int height = 128;

        Texture2D texture =
            new Texture2D(width, height, TextureFormat.RGBA32, false);

        texture.wrapMode = TextureWrapMode.Clamp;
        texture.filterMode = FilterMode.Bilinear;

        Color darkRust = new Color(0.19f, 0.075f, 0.035f);
        Color orangeRust = new Color(0.65f, 0.23f, 0.065f);
        Color lightRust = new Color(0.83f, 0.39f, 0.13f);

        float seedX = randomSeed * 0.317f;
        float seedY = randomSeed * 0.191f;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float u = (float)x / (width - 1);
                float v = (float)y / (height - 1);

                float large = Mathf.PerlinNoise(
                    u * 6f + seedX,
                    v * 6f + seedY);

                float medium = Mathf.PerlinNoise(
                    u * 23f + seedX,
                    v * 23f + seedY);

                float fine = Mathf.PerlinNoise(
                    u * 70f + seedX,
                    v * 70f + seedY);

                float noise =
                    large * 0.55f +
                    medium * 0.32f +
                    fine * 0.13f;

                float edgeX =
                    Mathf.SmoothStep(0f, 1f,
                        Mathf.Min(u, 1f - u) * 5f);

                float edgeY =
                    Mathf.SmoothStep(0f, 1f,
                        Mathf.Min(v, 1f - v) * 5f);

                float edge = edgeX * edgeY;

                float threshold =
                    Mathf.Lerp(0.57f, 0.39f, intensity);

                float alpha =
                    Mathf.SmoothStep(
                        threshold - 0.10f,
                        threshold + 0.10f,
                        noise);

                alpha *= edge;

                Color rust = Color.Lerp(
                    darkRust, orangeRust,
                    Mathf.Clamp01(noise * 1.5f));

                rust = Color.Lerp(
                    rust, lightRust,
                    Mathf.Clamp01((fine - 0.55f) * 1.5f));

                rust.a = alpha;

                texture.SetPixel(x, y, rust);
            }
        }

        texture.Apply();
        return texture;
    }

    [ContextMenu("Remove Corrosion")]
    public void RemoveCorrosion()
    {
        Transform old = transform.Find("Generated_Corrosion");

        if (old != null)
        {
            if (Application.isPlaying)
                Destroy(old.gameObject);
            else
                DestroyImmediate(old.gameObject);
        }

        corrosionObject = null;
    }
}
