
using UnityEngine;
using UnityEngine.UI;

public class PPECompletionTickBuilder : MonoBehaviour
{
    [Header("Generated Tick")]
    public GameObject greenTick;

    [Header("Appearance")]
    public Color tickColor = new Color32(80, 255, 40, 255);

    [ContextMenu("Build Green Tick")]
    public void BuildTick()
    {
        Transform existing = transform.Find("GreenTick");

        if (existing != null)
        {
            if (Application.isPlaying)
            {
                Debug.LogWarning("Build the tick in Edit Mode.");
                return;
            }

            DestroyImmediate(existing.gameObject);
        }

        greenTick = new GameObject(
            "GreenTick",
            typeof(RectTransform)
        );

        greenTick.transform.SetParent(transform, false);

        RectTransform root =
            greenTick.GetComponent<RectTransform>();

        root.anchorMin = new Vector2(0.5f, 0.5f);
        root.anchorMax = new Vector2(0.5f, 0.5f);
        root.pivot = new Vector2(0.5f, 0.5f);
        root.anchoredPosition = Vector2.zero;
        root.sizeDelta = new Vector2(350, 250);
        root.localScale = Vector3.one;

        CreateStroke(
            "ShortStroke",
            greenTick.transform,
            new Vector2(-72, -5),
            new Vector2(120, 40),
            -45f
        );

        CreateStroke(
            "LongStroke",
            greenTick.transform,
            new Vector2(42, 30),
            new Vector2(225, 40),
            40f
        );

        Debug.Log("Large green completion tick created.");
    }

    private void CreateStroke(
        string objectName,
        Transform parent,
        Vector2 position,
        Vector2 size,
        float rotation)
    {
        GameObject obj = new GameObject(
            objectName,
            typeof(RectTransform),
            typeof(Image)
        );

        obj.transform.SetParent(parent, false);

        RectTransform rect =
            obj.GetComponent<RectTransform>();

        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        rect.localRotation =
            Quaternion.Euler(0, 0, rotation);

        Image image = obj.GetComponent<Image>();
        image.color = tickColor;
        image.raycastTarget = false;
    }
}
