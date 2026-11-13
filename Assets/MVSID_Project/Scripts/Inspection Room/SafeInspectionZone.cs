
using UnityEngine;
using TMPro;

public class SafeInspectionZone : MonoBehaviour
{
    [Header("Zone Dimensions")]
    public float zoneWidth = 7f;
    public float zoneDepth = 2.2f;

    [Header("Floor Appearance")]
    public float borderWidth = 0.18f;
    public float floorOffset = 0.035f;
    public int stripesPerLongSide = 24;
    public int stripesPerShortSide = 8;

    [Header("Trainee Detection")]
    public bool traineeInside = false;

    private readonly Color safetyYellow =
        new Color32(255, 194, 28, 255);

    private readonly Color safetyBlack =
        new Color32(30, 33, 37, 255);

    [ContextMenu("Build Safe Inspection Zone")]
    public void BuildZone()
    {
        if (transform.Find("ZoneVisuals") != null)
        {
            Debug.LogWarning(
                "Zone already built. Delete ZoneVisuals to rebuild."
            );
            return;
        }

        GameObject visuals =
            new GameObject("ZoneVisuals");

        visuals.transform.SetParent(transform, false);

        // Create the striped front and back borders.
        CreateBorder(
            visuals.transform,
            true,
            -zoneDepth / 2f,
            zoneWidth,
            stripesPerLongSide
        );

        CreateBorder(
            visuals.transform,
            true,
            zoneDepth / 2f,
            zoneWidth,
            stripesPerLongSide
        );

        // Create the left and right borders.
        CreateBorder(
            visuals.transform,
            false,
            -zoneWidth / 2f,
            zoneDepth,
            stripesPerShortSide
        );

        CreateBorder(
            visuals.transform,
            false,
            zoneWidth / 2f,
            zoneDepth,
            stripesPerShortSide
        );

        CreateFloorText(visuals.transform);
        SetupTrigger();

        Debug.Log("Safe Inspection Zone created.");
    }

    private void CreateBorder(
        Transform parent,
        bool horizontal,
        float fixedPosition,
        float totalLength,
        int segments)
    {
        segments = Mathf.Max(2, segments);

        float segmentLength = totalLength / segments;

        for (int i = 0; i < segments; i++)
        {
            bool yellow = i % 2 == 0;

            GameObject stripe =
                GameObject.CreatePrimitive(
                    PrimitiveType.Cube
                );

            stripe.name = yellow
                ? "YellowStripe"
                : "BlackStripe";

            stripe.transform.SetParent(parent, false);

            float position =
                -totalLength / 2f +
                (i + 0.5f) * segmentLength;

            if (horizontal)
            {
                stripe.transform.localPosition =
                    new Vector3(
                        position,
                        floorOffset,
                        fixedPosition
                    );

                stripe.transform.localScale =
                    new Vector3(
                        segmentLength,
                        0.012f,
                        borderWidth
                    );
            }
            else
            {
                stripe.transform.localPosition =
                    new Vector3(
                        fixedPosition,
                        floorOffset,
                        position
                    );

                stripe.transform.localScale =
                    new Vector3(
                        borderWidth,
                        0.012f,
                        segmentLength
                    );
            }

            // Remove collisions from visual stripes.
            Collider stripeCollider =
                stripe.GetComponent<Collider>();

            if (stripeCollider != null)
            {
                if (Application.isPlaying)
                    Destroy(stripeCollider);
                else
                    DestroyImmediate(stripeCollider);
            }

            Renderer renderer =
                stripe.GetComponent<Renderer>();

            renderer.sharedMaterial =
                CreateMaterial(
                    yellow ? safetyYellow : safetyBlack
                );
        }
    }

    private Material CreateMaterial(Color color)
    {
        Shader shader =
            Shader.Find("Universal Render Pipeline/Unlit");

        if (shader == null)
            shader = Shader.Find("Unlit/Color");

        Material material = new Material(shader);

        material.color = color;

        return material;
    }

    private void CreateFloorText(Transform parent)
    {
        GameObject textObject =
            new GameObject("SafeInspectionZoneText");

        textObject.transform.SetParent(parent, false);

        TextMeshPro text =
            textObject.AddComponent<TextMeshPro>();

        text.text = "SAFE INSPECTION ZONE";
        text.fontSize = 3.5f;
        text.color = safetyYellow;
        text.fontStyle = FontStyles.Bold;
        text.alignment = TextAlignmentOptions.Center;

        RectTransform rect =
            textObject.GetComponent<RectTransform>();

        rect.sizeDelta = new Vector2(12f, 2f);

        // Lay the text flat, facing upwards.
        textObject.transform.localRotation =
            Quaternion.Euler(-90f, 0f, 0f);

        textObject.transform.localPosition =
            new Vector3(0f, floorOffset + 0.015f, 0f);

        textObject.transform.localScale =
            Vector3.one * 0.28f;
    }

    private void SetupTrigger()
    {
        BoxCollider trigger =
            GetComponent<BoxCollider>();

        if (trigger == null)
            trigger = gameObject.AddComponent<BoxCollider>();

        trigger.isTrigger = true;

        trigger.center =
            new Vector3(0f, 0.9f, 0f);

        trigger.size =
            new Vector3(
                zoneWidth,
                1.8f,
                zoneDepth
            );

        Rigidbody rb = GetComponent<Rigidbody>();

        if (rb == null)
            rb = gameObject.AddComponent<Rigidbody>();

        rb.isKinematic = true;
        rb.useGravity = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsTrainee(other))
            return;

        traineeInside = true;

        Debug.Log(
            "Trainee entered SAFE INSPECTION ZONE."
        );
    }

    private void OnTriggerExit(Collider other)
    {
        if (!IsTrainee(other))
            return;

        traineeInside = false;

        Debug.Log(
            "Trainee left SAFE INSPECTION ZONE."
        );
    }

    private bool IsTrainee(Collider other)
    {
        // Finds colliders belonging to a player rig.
        Transform current = other.transform;

        while (current != null)
        {
            if (current.CompareTag("Player"))
                return true;

            if (current.name.Contains("EZPZ Player"))
                return true;

            current = current.parent;
        }

        return false;
    }
}
