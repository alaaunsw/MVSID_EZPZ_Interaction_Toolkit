
using UnityEngine;

public class NavigationZoneOutline : MonoBehaviour
{
    [Header("Outline Appearance")]
    public Material outlineMaterial;

    public float zoneWidth = 2f;
    public float zoneLength = 2f;
    public float borderThickness = 0.06f;
    public float borderHeight = 0.04f;
    public float heightOffset = 0.03f;

    [Header("Pulse Settings")]
    public float pulseSpeed = 2f;

    [Range(0f, 0.3f)]
    public float pulseAmount = 0.08f;

    private Transform outlineRoot;
    private Vector3 originalScale;

#if UNITY_EDITOR
    [ContextMenu("Build Zone Outline in Editor")]
    private void BuildOutline()
    {
        if (Application.isPlaying)
            return;

        Transform existing = transform.Find("ZoneOutline");

        if (existing != null)
            DestroyImmediate(existing.gameObject);

        GameObject root = new GameObject("ZoneOutline");
        root.transform.SetParent(transform, false);

        outlineRoot = root.transform;
        outlineRoot.localPosition = Vector3.zero;
        outlineRoot.localRotation = Quaternion.identity;
        outlineRoot.localScale = Vector3.one;

        float halfWidth = zoneWidth / 2f;
        float halfLength = zoneLength / 2f;

        CreateBorder(
            "TopBorder",
            new Vector3(0, heightOffset, halfLength),
            new Vector3(
                zoneWidth,
                borderHeight,
                borderThickness));

        CreateBorder(
            "BottomBorder",
            new Vector3(0, heightOffset, -halfLength),
            new Vector3(
                zoneWidth,
                borderHeight,
                borderThickness));

        CreateBorder(
            "LeftBorder",
            new Vector3(-halfWidth, heightOffset, 0),
            new Vector3(
                borderThickness,
                borderHeight,
                zoneLength));

        CreateBorder(
            "RightBorder",
            new Vector3(halfWidth, heightOffset, 0),
            new Vector3(
                borderThickness,
                borderHeight,
                zoneLength));

        // Hide the solid cube but preserve its trigger collider.
        MeshRenderer parentRenderer =
            GetComponent<MeshRenderer>();

        if (parentRenderer != null)
            parentRenderer.enabled = false;

        UnityEditor.SceneManagement.EditorSceneManager
            .MarkSceneDirty(gameObject.scene);

        Debug.Log("Navigation zone outline created.");
    }

    private void CreateBorder(
        string borderName,
        Vector3 position,
        Vector3 scale)
    {
        GameObject border =
            GameObject.CreatePrimitive(PrimitiveType.Cube);

        border.name = borderName;
        border.transform.SetParent(outlineRoot, false);
        border.transform.localPosition = position;
        border.transform.localRotation = Quaternion.identity;
        border.transform.localScale = scale;

        MeshRenderer renderer =
            border.GetComponent<MeshRenderer>();

        if (outlineMaterial != null)
            renderer.sharedMaterial = outlineMaterial;

        Collider collider = border.GetComponent<Collider>();

        if (collider != null)
            DestroyImmediate(collider);
    }
#endif

    private void Awake()
    {
        outlineRoot = transform.Find("ZoneOutline");

        if (outlineRoot != null)
            originalScale = outlineRoot.localScale;
    }

    private void Update()
    {
        if (outlineRoot == null)
            return;

        float pulse = 1f +
            Mathf.Sin(Time.time * pulseSpeed * Mathf.PI * 2f)
            * pulseAmount;

        outlineRoot.localScale = originalScale * pulse;
    }

    private void OnDisable()
    {
        if (outlineRoot != null)
            outlineRoot.localScale = originalScale;
    }
}

