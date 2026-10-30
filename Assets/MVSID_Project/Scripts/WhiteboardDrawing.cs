
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]
public class WhiteboardDrawing : MonoBehaviour
{
    [Header("Drawing Settings")]
    public int textureSize = 1024;
    public int brushSize = 5;
    public Color inkColor = Color.black;
    public Color backgroundColor = Color.white;

    private Texture2D drawingTexture;
    private Material drawingMaterial;
    private MeshRenderer boardRenderer;

    private Vector2? previousUV;

    void Start()
    {
        boardRenderer = GetComponent<MeshRenderer>();

        // Create a separate material so the original is unchanged.
        drawingMaterial = new Material(
            boardRenderer.sharedMaterial
        );

        boardRenderer.material = drawingMaterial;

        // Create the writable texture.
        drawingTexture = new Texture2D(
            textureSize,
            textureSize,
            TextureFormat.RGBA32,
            false
        );

        drawingTexture.wrapMode = TextureWrapMode.Clamp;
        drawingTexture.filterMode = FilterMode.Bilinear;

        Color[] pixels = new Color[
            textureSize * textureSize
        ];

        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = backgroundColor;

        drawingTexture.SetPixels(pixels);
        drawingTexture.Apply();

        // URP Lit material.
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
        if (drawingTexture == null)
            return;

        if (previousUV.HasValue)
        {
            DrawLine(previousUV.Value, uv);
        }
        else
        {
            DrawBrush(uv);
        }

        previousUV = uv;
        drawingTexture.Apply();
    }

    private void DrawLine(Vector2 start, Vector2 end)
    {
        float distance = Vector2.Distance(start, end);

        int steps = Mathf.Max(
            1,
            Mathf.CeilToInt(
                distance * textureSize * 2
            )
        );

        for (int i = 0; i <= steps; i++)
        {
            float t = (float)i / steps;
            Vector2 uv = Vector2.Lerp(start, end, t);

            DrawBrush(uv);
        }
    }

    private void DrawBrush(Vector2 uv)
    {
        int x = Mathf.RoundToInt(
            uv.x * (textureSize - 1)
        );

        int y = Mathf.RoundToInt(
            uv.y * (textureSize - 1)
        );

        for (int dx = -brushSize; dx <= brushSize; dx++)
        {
            for (int dy = -brushSize; dy <= brushSize; dy++)
            {
                if (dx * dx + dy * dy >
                    brushSize * brushSize)
                    continue;

                int px = x + dx;
                int py = y + dy;

                if (px < 0 || px >= textureSize ||
                    py < 0 || py >= textureSize)
                    continue;

                drawingTexture.SetPixel(
                    px,
                    py,
                    inkColor
                );
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

