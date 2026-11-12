
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InspectionOpeningLabels : MonoBehaviour
{
    [Header("Physical Pipe Meshes")]
    public Transform pipe0043;
    public Transform pipe0493;
    public Transform pipe0007;

    [Header("Label Settings")]
    public float heightAbovePipe = 0.9f;
    public float labelScale = 0.0025f;
    public bool faceCamera = true;

    private GameObject[] labels = new GameObject[3];
    private Canvas[] labelCanvases = new Canvas[3];
    private Canvas[] markerCanvases = new Canvas[3];
    private LineRenderer[] lines = new LineRenderer[3];

    private Vector3[] markerPositions = new Vector3[3];

    private Sprite roundedSprite;
    private Sprite circleSprite;
    private Material lineMaterial;

    private readonly Color navy =
        new Color32(23, 60, 106, 255);

    private readonly Color paleBlue =
        new Color32(244, 248, 253, 255);

    private void Start()
    {
        roundedSprite = CreateSprite(64, 36, 9);
        circleSprite = CreateSprite(64, 64, 32);

        Shader shader = Shader.Find("Sprites/Default");

        if (shader != null)
            lineMaterial = new Material(shader);

        Transform[] pipes =
        {
            pipe0043,
            pipe0493,
            pipe0007
        };

        string[] ids =
        {
            "PIPE_0043",
            "PIPE_0493",
            "PIPE_0007"
        };

        for (int i = 0; i < 3; i++)
        {
            if (pipes[i] == null)
            {
                Debug.LogWarning(
                    "Opening label pipe missing: " + ids[i]
                );
                continue;
            }

            Renderer renderer =
                pipes[i].GetComponent<Renderer>();

            if (renderer == null)
            {
                Debug.LogWarning(
                    "Pipe has no Renderer: " + ids[i]
                );
                continue;
            }

            // Position marker near the upper pipe surface.
            Bounds bounds = renderer.bounds;

            markerPositions[i] = new Vector3(
                bounds.center.x,
                bounds.max.y + 0.03f,
                bounds.center.z
            );

            CreateLabel(i, ids[i]);
        }
    }

    private void CreateLabel(int index, string pipeID)
    {
        GameObject root =
            new GameObject(pipeID + "_OpeningLabel");

        root.transform.SetParent(transform, true);

        labels[index] = root;

        Vector3 labelPosition =
            markerPositions[index] +
            Vector3.up * heightAbovePipe;

        // Floating ID card.
        Canvas canvas = CreateCanvas(
            root.transform,
            "LabelCanvas",
            labelPosition,
            new Vector2(190, 54)
        );

        labelCanvases[index] = canvas;

        Image background = CreateImage(
            canvas.transform,
            "LabelBackground",
            Vector2.zero,
            new Vector2(190, 54),
            paleBlue,
            roundedSprite
        );

        Outline outline =
            background.gameObject.AddComponent<Outline>();

        outline.effectColor = navy;
        outline.effectDistance =
            new Vector2(1.5f, -1.5f);

        CreateText(
            canvas.transform,
            "PipeIDText",
            pipeID,
            new Vector2(190, 46)
        );

        // White connector line.
        GameObject lineObj =
            new GameObject("ConnectorLine");

        lineObj.transform.SetParent(root.transform, false);

        LineRenderer line =
            lineObj.AddComponent<LineRenderer>();

        lines[index] = line;

        line.useWorldSpace = true;
        line.positionCount = 2;
        line.startWidth = 0.012f;
        line.endWidth = 0.012f;
        line.startColor = Color.white;
        line.endColor = Color.white;
        line.numCapVertices = 6;

        if (lineMaterial != null)
            line.material = lineMaterial;

        // Circular marker positioned on the pipe.
        Canvas marker = CreateCanvas(
            root.transform,
            "MarkerCanvas",
            markerPositions[index],
            new Vector2(28, 28)
        );

        markerCanvases[index] = marker;

        CreateImage(
            marker.transform,
            "WhiteOuterCircle",
            Vector2.zero,
            new Vector2(28, 28),
            Color.white,
            circleSprite
        );

        CreateImage(
            marker.transform,
            "BlueInnerCircle",
            Vector2.zero,
            new Vector2(18, 18),
            navy,
            circleSprite
        );

        UpdateLine(index);
    }

    private Canvas CreateCanvas(
        Transform parent,
        string name,
        Vector3 position,
        Vector2 size)
    {
        GameObject obj = new GameObject(
            name,
            typeof(RectTransform),
            typeof(Canvas)
        );

        obj.transform.SetParent(parent, false);
        obj.transform.position = position;

        Canvas canvas = obj.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;

        RectTransform rect =
            obj.GetComponent<RectTransform>();

        rect.sizeDelta = size;
        rect.localScale = Vector3.one * labelScale;

        return canvas;
    }

    private Image CreateImage(
        Transform parent,
        string name,
        Vector2 position,
        Vector2 size,
        Color color,
        Sprite sprite)
    {
        GameObject obj = new GameObject(
            name,
            typeof(RectTransform),
            typeof(Image)
        );

        obj.transform.SetParent(parent, false);

        RectTransform rect =
            obj.GetComponent<RectTransform>();

        rect.sizeDelta = size;
        rect.anchoredPosition = position;

        Image img = obj.GetComponent<Image>();

        img.sprite = sprite;
        img.color = color;
        img.raycastTarget = false;

        return img;
    }

    private void CreateText(
        Transform parent,
        string name,
        string content,
        Vector2 size)
    {
        GameObject obj = new GameObject(
            name,
            typeof(RectTransform),
            typeof(TextMeshProUGUI)
        );

        obj.transform.SetParent(parent, false);

        RectTransform rect =
            obj.GetComponent<RectTransform>();

        rect.sizeDelta = size;
        rect.anchoredPosition = Vector2.zero;

        TextMeshProUGUI text =
            obj.GetComponent<TextMeshProUGUI>();

        text.text = content;
        text.fontSize = 24;
        text.fontStyle = FontStyles.Bold;
        text.color = navy;
        text.alignment = TextAlignmentOptions.Center;
        text.raycastTarget = false;
    }

    // Generate rounded/circular sprites without
    // requiring external image files.
    private Sprite CreateSprite(
        int width,
        int height,
        float radius)
    {
        Texture2D texture = new Texture2D(
            width, height, TextureFormat.RGBA32, false
        );

        texture.filterMode = FilterMode.Bilinear;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float dx = Mathf.Max(
                    Mathf.Abs(x + 0.5f - width / 2f) -
                    (width / 2f - radius), 0
                );

                float dy = Mathf.Max(
                    Mathf.Abs(y + 0.5f - height / 2f) -
                    (height / 2f - radius), 0
                );

                float distance = Mathf.Sqrt(dx * dx + dy * dy);

                float alpha = Mathf.Clamp01(
                    radius + 0.5f - distance
                );

                texture.SetPixel(
                    x, y,
                    new Color(1, 1, 1, alpha)
                );
            }
        }

        texture.Apply();

        return Sprite.Create(
            texture,
            new Rect(0, 0, width, height),
            new Vector2(0.5f, 0.5f),
            100f
        );
    }

    private void UpdateLine(int index)
    {
        if (lines[index] == null ||
            labelCanvases[index] == null)
            return;

        Vector3 labelBottom =
            labelCanvases[index].transform.position -
            Vector3.up * (54f * labelScale * 0.5f);

        lines[index].SetPosition(0, labelBottom);
        lines[index].SetPosition(
            1, markerPositions[index]
        );
    }

    private void LateUpdate()
    {
        Camera cam = Camera.main;

        if (cam == null)
            return;

        for (int i = 0; i < 3; i++)
        {
            if (labelCanvases[i] == null)
                continue;

            if (faceCamera)
            {
                labelCanvases[i].transform.rotation =
                    cam.transform.rotation;

                markerCanvases[i].transform.rotation =
                    cam.transform.rotation;
            }

            UpdateLine(i);
        }
    }

    public void ShowLabels()
    {
        SetLabelsVisible(true);
    }

    public void HideLabels()
    {
        SetLabelsVisible(false);
    }

    private void SetLabelsVisible(bool visible)
    {
        foreach (GameObject label in labels)
        {
            if (label != null)
                label.SetActive(visible);
        }
    }
}
