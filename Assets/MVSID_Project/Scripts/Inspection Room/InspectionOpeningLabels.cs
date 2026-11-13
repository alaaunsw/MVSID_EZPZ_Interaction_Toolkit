
using UnityEngine;
using UnityEngine.UI;
using TMPro;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public class InspectionOpeningLabels : MonoBehaviour
{
    [Header("Physical Pipe Meshes")]
    public Transform pipe0043;
    public Transform pipe0493;
    public Transform pipe0007;

    [Header("Label Appearance")]
    public float heightAbovePipe = 0.9f;
    public float labelScale = 0.0025f;
    public bool faceCamera = true;

    private readonly Color navy =
        new Color32(23, 60, 106, 255);

    private readonly Color paleBlue =
        new Color32(244, 248, 253, 255);

    // No Start() generation.
    // Labels are built and saved in Edit Mode.

    [ContextMenu("Build Opening Labels")]
    public void BuildOpeningLabels()
    {
        CreateLabel("PIPE_0043", pipe0043);
        CreateLabel("PIPE_0493", pipe0493);
        CreateLabel("PIPE_0007", pipe0007);

        MarkSceneChanged();

        Debug.Log("Opening labels created in the scene.");
    }

    private void CreateLabel(string pipeID, Transform pipe)
    {
        if (pipe == null)
        {
            Debug.LogWarning(
                "Missing physical pipe for " + pipeID
            );
            return;
        }

        // Prevent duplicate labels.
        if (transform.Find(pipeID + "_OpeningLabel") != null)
        {
            Debug.Log(pipeID + " label already exists.");
            return;
        }

        Renderer pipeRenderer =
            pipe.GetComponent<Renderer>();

        if (pipeRenderer == null)
        {
            Debug.LogWarning(
                "No Mesh Renderer found on " + pipeID
            );
            return;
        }

        Bounds bounds = pipeRenderer.bounds;

        Vector3 markerPosition = new Vector3(
            bounds.center.x,
            bounds.max.y + 0.03f,
            bounds.center.z
        );

        GameObject root = new GameObject(
            pipeID + "_OpeningLabel",
            typeof(RectTransform),
            typeof(Canvas)
        );

        root.transform.SetParent(transform, false);

        RectTransform rootRect =
            root.GetComponent<RectTransform>();

        rootRect.sizeDelta = new Vector2(220, 900);
        rootRect.position = markerPosition;
        rootRect.localScale =
            Vector3.one * labelScale;

        Canvas canvas = root.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;

        Camera camera = Camera.main;

        if (camera != null)
            rootRect.rotation = camera.transform.rotation;

        float lineHeight =
            heightAbovePipe / Mathf.Max(labelScale, 0.0001f);

        // Connector: white vertical line.
        CreateImage(
            root.transform,
            "ConnectorLine",
            new Vector2(0, lineHeight * 0.5f),
            new Vector2(4, lineHeight),
            Color.white,
            null
        );

        // Label background.
        Image background = CreateImage(
            root.transform,
            "LabelBackground",
            new Vector2(0, lineHeight + 29),
            new Vector2(194, 56),
            paleBlue,
            GetBuiltinSprite("UI/Skin/UISprite.psd")
        );

        if (background.sprite != null)
            background.type = Image.Type.Sliced;

        Outline outline =
            background.gameObject.AddComponent<Outline>();

        outline.effectColor = navy;
        outline.effectDistance =
            new Vector2(1.2f, -1.2f);

        // Pipe ID.
        CreateText(
            root.transform,
            "PipeIDText",
            pipeID,
            new Vector2(0, lineHeight + 29),
            new Vector2(185, 49)
        );

        // White outer circle.
        Sprite circleSprite =
            GetBuiltinSprite("UI/Skin/Knob.psd");

        CreateImage(
            root.transform,
            "ConnectorCircleOuter",
            Vector2.zero,
            new Vector2(29, 29),
            Color.white,
            circleSprite
        );

        // Blue centre.
        CreateImage(
            root.transform,
            "ConnectorCircleInner",
            Vector2.zero,
            new Vector2(19, 19),
            navy,
            circleSprite
        );
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

        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;

        Image image = obj.GetComponent<Image>();
        image.sprite = sprite;
        image.color = color;
        image.raycastTarget = false;

        return image;
    }

    private void CreateText(
        Transform parent,
        string name,
        string content,
        Vector2 position,
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

        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = size;
        rect.anchoredPosition = position;

        TextMeshProUGUI text =
            obj.GetComponent<TextMeshProUGUI>();

        text.text = content;
        text.fontSize = 25;
        text.fontStyle = FontStyles.Bold;
        text.color = navy;
        text.alignment = TextAlignmentOptions.Center;
        text.raycastTarget = false;
    }

    private Sprite GetBuiltinSprite(string path)
    {
#if UNITY_EDITOR
        return AssetDatabase.GetBuiltinExtraResource<Sprite>(path);
#else
        return null;
#endif
    }

    [ContextMenu("Show Opening Labels")]
    public void ShowLabels()
    {
        SetLabelsVisible(true);
    }

    [ContextMenu("Hide Opening Labels")]
    public void HideLabels()
    {
        SetLabelsVisible(false);
    }

    private void SetLabelsVisible(bool visible)
    {
        string[] ids =
        {
            "PIPE_0043",
            "PIPE_0493",
            "PIPE_0007"
        };

        foreach (string id in ids)
        {
            Transform label =
                transform.Find(id + "_OpeningLabel");

            if (label != null)
                label.gameObject.SetActive(visible);
        }

        MarkSceneChanged();
    }

    private void LateUpdate()
    {
        if (!Application.isPlaying || !faceCamera)
            return;

        Camera cam = Camera.main;
        if (cam == null)
            return;

        foreach (Transform child in transform)
        {
            if (child.name.EndsWith("_OpeningLabel"))
                child.rotation = cam.transform.rotation;
        }
    }

    private void MarkSceneChanged()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
            EditorSceneManager.MarkSceneDirty(
                gameObject.scene
            );
#endif
    }
}
