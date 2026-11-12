
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PipeFloatingInfoCard : MonoBehaviour
{
    [Header("Dataset Connection")]
    public PipeInspectionTarget pipeTarget;

    [Header("Generated UI")]
    public Canvas cardCanvas;

    [Header("Display")]
    public bool faceCamera = true;

    [Range(0.001f, 0.008f)]
    public float cardScale = 0.003f;

    private TMP_Text titleText;
    private TMP_Text[] values = new TMP_Text[6];

    private readonly Color navy =
        new Color32(26, 73, 135, 255);

    private readonly Color headerBlue =
        new Color32(208, 228, 252, 255);

    private readonly Color bodyBlue =
        new Color32(245, 249, 255, 255);

    private readonly Color labelColor =
        new Color32(65, 77, 94, 255);

    private readonly Color valueColor =
        new Color32(48, 58, 75, 255);

    private readonly Color dividerColor =
        new Color32(212, 225, 242, 255);

    private void Awake()
    {
        if (cardCanvas != null)
            cardCanvas.gameObject.SetActive(false);
    }

    [ContextMenu("Build Final Reference Card")]
    public void BuildCard()
    {
        if (transform.Find("InspectionCardCanvas") != null)
        {
            Debug.LogWarning(
                "Delete the existing InspectionCardCanvas first."
            );
            return;
        }

        GameObject canvasObject = new GameObject(
            "InspectionCardCanvas",
            typeof(RectTransform),
            typeof(Canvas),
            typeof(CanvasScaler)
        );

        canvasObject.transform.SetParent(transform, false);

        cardCanvas = canvasObject.GetComponent<Canvas>();
        cardCanvas.renderMode = RenderMode.WorldSpace;

        RectTransform canvasRect =
            canvasObject.GetComponent<RectTransform>();

        canvasRect.sizeDelta = new Vector2(540, 560);
        canvasRect.localPosition = Vector3.zero;
        canvasRect.localRotation = Quaternion.identity;
        canvasRect.localScale = Vector3.one * cardScale;

        CanvasScaler scaler =
            canvasObject.GetComponent<CanvasScaler>();

        scaler.dynamicPixelsPerUnit = 20f;

        Transform root = canvasObject.transform;

        // Background shadow
        MakeShape(
            root, "CardShadow",
            new Vector2(4, 66),
            new Vector2(490, 360),
            new Color(0f, 0f, 0f, 0.24f),
            15f
        );

        // Rounded navy border
        MakeShape(
            root, "CardBorder",
            new Vector2(0, 70),
            new Vector2(488, 356),
            navy, 16f
        );

        // Pale-blue main background
        Transform card = MakeShape(
            root, "CardBackground",
            new Vector2(0, 70),
            new Vector2(482, 350),
            bodyBlue, 13f
        ).transform;

        // Header band
        MakeShape(
            card, "HeaderBackground",
            new Vector2(0, 140),
            new Vector2(478, 58),
            headerBlue, 11f
        );

        // Header title - left aligned
        titleText = MakeText(
            card, "PipeTitle",
            "PIPE_0493",
            new Vector2(0, 140),
            new Vector2(440, 46),
            30, navy, true, false
        );

        // Header divider
        MakeShape(
            card, "HeaderDivider",
            new Vector2(0, 108),
            new Vector2(478, 2),
            dividerColor, 0f
        );

        string[] labels =
        {
            "Pipe Size (Diameter)",
            "Wall Thickness (Original)",
            "Thickness Loss",
            "Operating Pressure",
            "Operating Temperature",
            "Condition"
        };

        // Six neatly arranged rows
        for (int i = 0; i < labels.Length; i++)
        {
            float y = 82f - i * 43f;

            MakeText(
                card, "Label" + (i + 1),
                labels[i],
                new Vector2(-103, y),
                new Vector2(255, 34),
                20, labelColor, false, false
            );

            values[i] = MakeText(
                card, "Value" + (i + 1),
                "-",
                new Vector2(163, y),
                new Vector2(118, 34),
                20, valueColor, false, true
            );

            if (i < labels.Length - 1)
            {
                MakeShape(
                    card, "RowDivider" + (i + 1),
                    new Vector2(0, y - 21.5f),
                    new Vector2(460, 1.4f),
                    dividerColor, 0f
                );
            }
        }

        // White line from bottom of card
        MakeShape(
            root, "ConnectorLine",
            new Vector2(0, -150),
            new Vector2(5, 90),
            Color.white, 0f
        );

        // White circular ring
        MakeShape(
            root, "ConnectorCircleOuter",
            new Vector2(0, -203),
            new Vector2(32, 32),
            Color.white, 16f
        );

        // Navy centre
        MakeShape(
            root, "ConnectorCircleInner",
            new Vector2(0, -203),
            new Vector2(21, 21),
            navy, 10.5f
        );

        CacheUI();
        UpdateCard();

        cardCanvas.gameObject.SetActive(false);

        Debug.Log("Final pipe information card created.");
    }

    private PipeCardShape MakeShape(
        Transform parent,
        string name,
        Vector2 position,
        Vector2 size,
        Color color,
        float radius)
    {
        GameObject obj = new GameObject(
            name, typeof(RectTransform)
        );

        obj.transform.SetParent(parent, false);

        RectTransform rt =
            obj.GetComponent<RectTransform>();

        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = position;
        rt.sizeDelta = size;

        PipeCardShape shape =
            obj.AddComponent<PipeCardShape>();

        shape.color = color;
        shape.cornerRadius = radius;
        shape.raycastTarget = false;

        return shape;
    }

    private TMP_Text MakeText(
        Transform parent,
        string name,
        string content,
        Vector2 position,
        Vector2 size,
        float fontSize,
        Color color,
        bool bold,
        bool rightAligned)
    {
        GameObject obj = new GameObject(
            name,
            typeof(RectTransform),
            typeof(TextMeshProUGUI)
        );

        obj.transform.SetParent(parent, false);

        RectTransform rt =
            obj.GetComponent<RectTransform>();

        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = size;
        rt.anchoredPosition = position;

        TMP_Text text =
            obj.GetComponent<TextMeshProUGUI>();

        text.text = content;
        text.fontSize = fontSize;
        text.fontStyle =
            bold ? FontStyles.Bold : FontStyles.Normal;

        text.color = color;
        text.alignment = rightAligned
            ? TextAlignmentOptions.Right
            : TextAlignmentOptions.Left;

        text.verticalAlignment =
            VerticalAlignmentOptions.Middle;

        text.enableWordWrapping = false;
        text.enableAutoSizing = true;
        text.fontSizeMin = 13;
        text.fontSizeMax = fontSize;
        text.raycastTarget = false;

        return text;
    }

    private void CacheUI()
    {
        if (cardCanvas == null)
            return;

        Transform card =
            cardCanvas.transform.Find("CardBackground");

        if (card == null)
            return;

        Transform title = card.Find("PipeTitle");

        titleText = title != null
            ? title.GetComponent<TMP_Text>()
            : null;

        for (int i = 0; i < 6; i++)
        {
            Transform value =
                card.Find("Value" + (i + 1));

            values[i] = value != null
                ? value.GetComponent<TMP_Text>()
                : null;
        }
    }

    public void UpdateCard()
    {
        if (pipeTarget == null)
            return;

        PipeInspectionData data =
            pipeTarget.GetInspectionData();

        if (data == null)
            return;

        CacheUI();

        if (titleText != null)
            titleText.text = data.pipeID;

        if (values[0] != null)
            values[0].text =
                data.pipeSizeMM.ToString("F1") + " mm";

        if (values[1] != null)
            values[1].text =
                data.referenceThicknessMM.ToString("F2") + " mm";

        if (values[2] != null)
            values[2].text =
                data.MaterialLossMM.ToString("F2") + " mm";

        if (values[3] != null)
            values[3].text =
                data.maximumPressurePSI.ToString("F0") + " psi";

        if (values[4] != null)
            values[4].text =
                data.temperatureC.ToString("F1") + "\u00B0C";

        if (values[5] != null)
            values[5].text =
                data.condition.ToString();
    }

    public void ShowCard()
    {
        if (cardCanvas == null)
            return;

        UpdateCard();
        cardCanvas.gameObject.SetActive(true);
    }

    public void HideCard()
    {
        if (cardCanvas != null)
            cardCanvas.gameObject.SetActive(false);
    }

    private void LateUpdate()
    {
        if (!faceCamera ||
            cardCanvas == null ||
            !cardCanvas.gameObject.activeSelf)
            return;

        Camera cam = Camera.main;

        if (cam != null)
        {
            cardCanvas.transform.rotation =
                cam.transform.rotation;
        }
    }
}

// Custom graphic for rounded rectangles and circles.
// The triangle winding is corrected so that the
// shapes render properly on a World Space Canvas.
public class PipeCardShape : MaskableGraphic
{
    [Min(0f)]
    public float cornerRadius = 12f;

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();

        Rect r = rectTransform.rect;

        float radius = Mathf.Clamp(
            cornerRadius,
            0f,
            Mathf.Min(r.width, r.height) * 0.5f
        );

        // Use a standard rectangle when no rounding
        // is required.
        if (radius <= 0.001f)
        {
            UIVertex v = UIVertex.simpleVert;
            v.color = color;

            v.position = new Vector3(r.xMin, r.yMin, 0);
            vh.AddVert(v);

            v.position = new Vector3(r.xMin, r.yMax, 0);
            vh.AddVert(v);

            v.position = new Vector3(r.xMax, r.yMax, 0);
            vh.AddVert(v);

            v.position = new Vector3(r.xMax, r.yMin, 0);
            vh.AddVert(v);

            vh.AddTriangle(0, 1, 2);
            vh.AddTriangle(0, 2, 3);

            return;
        }

        const int segments = 12;

        Vector2[] corners =
        {
            new Vector2(r.xMax - radius, r.yMax - radius),
            new Vector2(r.xMin + radius, r.yMax - radius),
            new Vector2(r.xMin + radius, r.yMin + radius),
            new Vector2(r.xMax - radius, r.yMin + radius)
        };

        float[] angles = { 0f, 90f, 180f, 270f };

        UIVertex vertex = UIVertex.simpleVert;
        vertex.color = color;
        vertex.position = r.center;

        vh.AddVert(vertex);

        for (int corner = 0; corner < 4; corner++)
        {
            for (int i = 0; i <= segments; i++)
            {
                float angle =
                    (angles[corner] +
                     i * 90f / segments) *
                    Mathf.Deg2Rad;

                Vector2 p = corners[corner] +
                    new Vector2(
                        Mathf.Cos(angle),
                        Mathf.Sin(angle)
                    ) * radius;

                vertex.position = p;
                vh.AddVert(vertex);
            }
        }

        int count = 4 * (segments + 1);

        for (int i = 0; i < count; i++)
        {
            int current = i + 1;
            int next = ((i + 1) % count) + 1;

            // Correct winding for Unity UI rendering.
            vh.AddTriangle(0, next, current);
        }
    }
}
