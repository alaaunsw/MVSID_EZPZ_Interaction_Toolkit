
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]
public class WhiteboardDrawing : MonoBehaviour
{
    [Header("Drawing Settings")]
    public int textureSize = 1024;
    public int brushSize = 2;
    public int eraserSize = 15;

    public Color inkColor = Color.black;
    public Color backgroundColor = Color.white;

    private Texture2D drawingTexture;
    private Material drawingMaterial;
    private MeshRenderer boardRenderer;

    private Vector2? previousUV;

    void Start()
    {
        boardRenderer = GetComponent<MeshRenderer>();

        drawingMaterial = new Material(
            boardRenderer.sharedMaterial
        );

        boardRenderer.material = drawingMaterial;

        drawingTexture = new Texture2D(
            textureSize,
            textureSize,
            TextureFormat.RGBA32,
            false
        );

        drawingTexture.wrapMode = TextureWrapMode.Clamp;
        drawingTexture.filterMode = FilterMode.Bilinear;

        Color[] pixels = new Color[textureSize * textureSize];

        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = backgroundColor;

        drawingTexture.SetPixels(pixels);
        drawingTexture.Apply();

        drawingMaterial.SetTexture(
            "_BaseMap",
            drawingTexture
        );

        drawingMaterial.SetColor(
            "_BaseColor",
            Color.white
        );
    }

    public void BeginStroke()
    {
        previousUV = null;
    }

    public void EndStroke()
    {
        previousUV = null;
    }

    public void DrawAtUV(Vector2 uv)
    {
        PaintAtUV(uv, inkColor, brushSize);
    }

    public void EraseAtUV(Vector2 uv)
    {
        PaintAtUV(uv, backgroundColor, eraserSize);
    }

    private void PaintAtUV(Vector2 uv, Color color, int size)
    {
        if (drawingTexture == null)
            return;

        if (previousUV.HasValue)
        {
            float distance = Vector2.Distance(
                previousUV.Value, uv
            );

            int steps = Mathf.Max(
                1,
                Mathf.CeilToInt(distance * textureSize * 2)
            );

            for (int i = 0; i <= steps; i++)
            {
                float t = (float)i / steps;
                Vector2 point = Vector2.Lerp(
                    previousUV.Value, uv, t
                );

                PaintBrush(point, color, size);
            }
        }
        else
        {
            PaintBrush(uv, color, size);
        }

        previousUV = uv;
        drawingTexture.Apply();
    }

    private void PaintBrush(Vector2 uv, Color color, int size)
    {
        int x = Mathf.RoundToInt(
            uv.x * (textureSize - 1)
        );

        int y = Mathf.RoundToInt(
            uv.y * (textureSize - 1)
        );

        for (int dx = -size; dx <= size; dx++)
        {
            for (int dy = -size; dy <= size; dy++)
            {
                if (dx * dx + dy * dy > size * size)
                    continue;

                int px = x + dx;
                int py = y + dy;

                if (px < 0 || px >= textureSize ||
                    py < 0 || py >= textureSize)
                    continue;

                drawingTexture.SetPixel(px, py, color);
            }
        }
    }

    void OnDestroy()
    {
        if (drawingTexture != null)
            Destroy(drawingTexture);

        if (drawingMaterial != null)
            Destroy(drawingMaterial);
    }
}
