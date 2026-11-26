
using System;
using System.Globalization;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public class TestingInspectionCalculator : MonoBehaviour
{
    [Header("Camera")]
    public Camera playerCamera;

    [Header("Calculator Placement")]
    public float distanceFromPlayer = 1.5f;
    public float horizontalOffset = 0.7f;
    public float verticalOffset = 0f;
    public float canvasScale = 0.0015f;

    [Header("Colours")]
    public Color panelColor =
        new Color32(10, 10, 10, 245);

    public Color keyColor =
        new Color32(55, 55, 55, 255);

    public Color highlightColor =
        new Color32(255, 213, 79, 255);

    [Header("Generated Objects")]
    public GameObject calculatorRoot;
    public TextMeshProUGUI displayText;

    [SerializeField]
    private GameObject[] keyObjects = new GameObject[19];

    [SerializeField]
    private string[] keyValues = new string[19];

    private static readonly string[] keys =
    {
        "7", "8", "9", "÷",
        "4", "5", "6", "×",
        "1", "2", "3", "−",
        "C", "0", ".", "+",
        "⌫", "=", "CLOSE"
    };

    private string display = "0";
    private double storedValue;
    private string pendingOperator = "";

    private bool startNewNumber;
    private bool calculatorOpen;
    private int hoveredIndex = -1;
    private int lastPressFrame = -1;

    // ==========================================
    // BUILD CALCULATOR IN EDIT MODE
    // ==========================================

    [ContextMenu("Build Calculator")]
    public void BuildCalculator()
    {
        if (Application.isPlaying)
        {
            Debug.LogWarning(
                "[TESTING] Stop Play Mode before building."
            );
            return;
        }

        // Rebuilding replaces the current calculator.
        // Use Reconnect Calculator Keys instead if
        // you already adjusted the calculator layout.

        if (calculatorRoot != null)
        {
#if UNITY_EDITOR
            Undo.DestroyObjectImmediate(calculatorRoot);
#else
            DestroyImmediate(calculatorRoot);
#endif
        }

        calculatorRoot = new GameObject(
            "Testing_Inspection_Calculator",
            typeof(RectTransform),
            typeof(Canvas),
            typeof(GraphicRaycaster)
        );

#if UNITY_EDITOR
        Undo.RegisterCreatedObjectUndo(
            calculatorRoot,
            "Build Testing Calculator"
        );
#endif

        Canvas canvas =
            calculatorRoot.GetComponent<Canvas>();

        canvas.renderMode = RenderMode.WorldSpace;
        canvas.worldCamera = playerCamera;
        canvas.overrideSorting = true;
        canvas.sortingOrder = 150;

        RectTransform root =
            calculatorRoot.GetComponent<RectTransform>();

        root.sizeDelta = new Vector2(540, 760);
        root.localScale = Vector3.one * canvasScale;

        RectTransform panel = MakeRect(
            "MainPanel",
            calculatorRoot.transform,
            Vector2.zero,
            new Vector2(540, 760)
        );

        MakeImage(panel, panelColor);

        MakeText(
            "Title",
            panel,
            "INSPECTION CALCULATOR",
            new Vector2(0, 320),
            new Vector2(500, 55),
            29,
            Color.white
        );

        RectTransform screen = MakeRect(
            "DisplayBackground",
            panel,
            new Vector2(0, 235),
            new Vector2(490, 100)
        );

        MakeImage(
            screen,
            new Color32(30, 30, 30, 255)
        );

        displayText = MakeText(
            "CalculatorDisplay",
            screen,
            "0",
            Vector2.zero,
            new Vector2(450, 80),
            48,
            Color.white
        );

        displayText.alignment =
            TextAlignmentOptions.MidlineRight;

        keyObjects = new GameObject[keys.Length];
        keyValues = new string[keys.Length];

        // Main keypad.
        for (int i = 0; i < 16; i++)
        {
            int row = i / 4;
            int col = i % 4;

            CreateKey(
                panel,
                i,
                keys[i],
                new Vector2(
                    -180 + col * 120,
                    115 - row * 110
                ),
                new Vector2(105, 90)
            );
        }

        CreateKey(
            panel, 16, "⌫",
            new Vector2(-180, -325),
            new Vector2(105, 75)
        );

        CreateKey(
            panel, 17, "=",
            new Vector2(-60, -325),
            new Vector2(105, 75)
        );

        CreateKey(
            panel, 18, "CLOSE",
            new Vector2(120, -325),
            new Vector2(215, 75)
        );

        PositionCalculator();
        calculatorRoot.SetActive(true);

        ReconnectCalculatorKeys();

#if UNITY_EDITOR
        EditorUtility.SetDirty(this);
        EditorSceneManager.MarkSceneDirty(
            gameObject.scene
        );
#endif

        Debug.Log(
            "[TESTING] Calculator built with 19 keys."
        );
    }

    // ==========================================
    // UI CREATION
    // ==========================================

    private RectTransform MakeRect(
        string name,
        Transform parent,
        Vector2 position,
        Vector2 size)
    {
        GameObject obj = new GameObject(
            name,
            typeof(RectTransform)
        );

        obj.transform.SetParent(parent, false);

        RectTransform rect =
            obj.GetComponent<RectTransform>();

        rect.anchorMin = Vector2.one * 0.5f;
        rect.anchorMax = Vector2.one * 0.5f;
        rect.pivot = Vector2.one * 0.5f;

        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        rect.localScale = Vector3.one;

        return rect;
    }

    private Image MakeImage(
        RectTransform rect,
        Color color,
        bool raycastTarget = false)
    {
        Image image =
            rect.gameObject.AddComponent<Image>();

        image.color = color;
        image.raycastTarget = raycastTarget;

        return image;
    }

    private TextMeshProUGUI MakeText(
        string name,
        Transform parent,
        string content,
        Vector2 position,
        Vector2 size,
        float fontSize,
        Color color)
    {
        RectTransform rect = MakeRect(
            name,
            parent,
            position,
            size
        );

        TextMeshProUGUI text =
            rect.gameObject.AddComponent<TextMeshProUGUI>();

        text.text = content;
        text.fontSize = fontSize;
        text.color = color;
        text.alignment = TextAlignmentOptions.Center;
        text.enableAutoSizing = false;
        text.raycastTarget = false;

        return text;
    }

    private void CreateKey(
        Transform parent,
        int index,
        string value,
        Vector2 position,
        Vector2 size)
    {
        RectTransform key = MakeRect(
            "Key_" + index + "_" + value,
            parent,
            position,
            size
        );

        // Only this key image receives UI pointer events.
        MakeImage(key, keyColor, true);

        MakeText(
            "KeyText",
            key,
            value,
            Vector2.zero,
            size - new Vector2(10, 10),
            value == "CLOSE" ? 25 : 38,
            Color.white
        );

        BoxCollider collider =
            key.gameObject.AddComponent<BoxCollider>();

        collider.center = Vector3.zero;
        collider.size = new Vector3(
            size.x,
            size.y,
            10f
        );

        collider.isTrigger = false;

        // EZPZ events are connected at runtime.
        key.gameObject.AddComponent<InteractableGeneral>();

        keyObjects[index] = key.gameObject;
        keyValues[index] = value;
    }

    // ==========================================
    // RECONNECT EXISTING KEYS
    // ==========================================

    [ContextMenu("Reconnect Calculator Keys")]
    public void ReconnectCalculatorKeys()
    {
        if (calculatorRoot == null)
        {
            Debug.LogError(
                "[TESTING] Calculator Root is not assigned."
            );
            return;
        }

        Transform panel =
            calculatorRoot.transform.Find("MainPanel");

        if (panel == null)
        {
            Debug.LogError(
                "[TESTING] MainPanel was not found."
            );
            return;
        }

        keyObjects = new GameObject[keys.Length];
        keyValues = new string[keys.Length];

        int found = 0;

        for (int i = 0; i < keys.Length; i++)
        {
            Transform key = panel.Find(
                "Key_" + i + "_" + keys[i]
            );

            // Allow the user to edit a key's label
            // without losing the reference.
            if (key == null)
            {
                string prefix = "Key_" + i + "_";

                foreach (Transform child in panel)
                {
                    if (child.name.StartsWith(prefix))
                    {
                        key = child;
                        break;
                    }
                }
            }

            if (key != null)
            {
                keyObjects[i] = key.gameObject;
                keyValues[i] = keys[i];
                found++;
            }
            else
            {
                Debug.LogWarning(
                    "[TESTING] Missing key index " + i +
                    ": " + keys[i]
                );
            }
        }

        Transform screen = calculatorRoot.transform.Find(
            "MainPanel/DisplayBackground/CalculatorDisplay"
        );

        if (screen != null)
        {
            displayText =
                screen.GetComponent<TextMeshProUGUI>();
        }

#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            EditorUtility.SetDirty(this);
            EditorSceneManager.MarkSceneDirty(
                gameObject.scene
            );
        }
#endif

        Debug.Log(
            "[TESTING] Calculator references connected: " +
            found + "/" + keys.Length
        );
    }

    // ==========================================
    // RUNTIME INITIALISATION
    // ==========================================

    private void Awake()
    {
        if (!Application.isPlaying)
            return;

        ReconnectCalculatorKeys();

        // Connect EZPZ events exactly once
        // for this runtime instance.
        for (int i = 0; i < keys.Length; i++)
        {
            if (keyObjects[i] == null)
                continue;

            InteractableGeneral ezpz =
                keyObjects[i].GetComponent<InteractableGeneral>();

            if (ezpz == null)
                continue;

            int index = i;

            ezpz.onPrimaryInteract.AddListener(
                () => PressKey(index)
            );

            ezpz.onHoverEnter.AddListener(
                () => SetKeyHighlight(index, true)
            );

            ezpz.onHoverExit.AddListener(
                () => SetKeyHighlight(index, false)
            );
        }

        // Calculator starts hidden.
        if (calculatorRoot != null)
            calculatorRoot.SetActive(false);

        calculatorOpen = false;
    }

    // ==========================================
    // POSITION CALCULATOR
    // ==========================================

    private void PositionCalculator()
    {
        if (calculatorRoot == null)
            return;

        if (playerCamera == null)
            playerCamera = Camera.main;

        if (playerCamera == null)
            return;

        Transform cam = playerCamera.transform;

        calculatorRoot.transform.position =
            cam.position +
            cam.forward * distanceFromPlayer +
            cam.right * horizontalOffset +
            cam.up * verticalOffset;

        calculatorRoot.transform.rotation =
            cam.rotation;

        Canvas canvas =
            calculatorRoot.GetComponent<Canvas>();

        if (canvas != null)
            canvas.worldCamera = playerCamera;
    }

    public void OpenCalculator()
    {
        if (calculatorRoot == null)
        {
            Debug.LogError(
                "[TESTING] Calculator Root is missing."
            );
            return;
        }

        PositionCalculator();

        calculatorRoot.SetActive(true);
        calculatorOpen = true;
        hoveredIndex = -1;

        UpdateDisplay();

        Debug.Log("[TESTING] Calculator opened.");
    }

    public void CloseCalculator()
    {
        calculatorOpen = false;
        hoveredIndex = -1;

        if (calculatorRoot != null)
            calculatorRoot.SetActive(false);
    }

    public void ToggleCalculator()
    {
        if (calculatorOpen)
            CloseCalculator();
        else
            OpenCalculator();
    }

    // ==========================================
    // DESKTOP MOUSE DETECTION
    // ==========================================

    private void Update()
    {
        if (!calculatorOpen ||
            Mouse.current == null ||
            playerCamera == null ||
            keyObjects == null)
            return;

        Vector2 position =
            Mouse.current.position.ReadValue();

        if (Cursor.lockState == CursorLockMode.Locked)
        {
            position = new Vector2(
                Screen.width * 0.5f,
                Screen.height * 0.5f
            );
        }

        Ray ray = playerCamera.ScreenPointToRay(position);
        int detected = -1;

        for (int i = 0; i < keyObjects.Length; i++)
        {
            if (keyObjects[i] == null ||
                !keyObjects[i].activeInHierarchy)
                continue;

            BoxCollider collider =
                keyObjects[i].GetComponent<BoxCollider>();

            if (collider != null &&
                collider.Raycast(
                    ray,
                    out RaycastHit hit,
                    100f))
            {
                detected = i;
                break;
            }
        }

        if (detected != hoveredIndex)
        {
            if (hoveredIndex >= 0)
                SetKeyHighlight(hoveredIndex, false);

            hoveredIndex = detected;

            if (hoveredIndex >= 0)
                SetKeyHighlight(hoveredIndex, true);
        }

        if (detected >= 0 &&
            Mouse.current.leftButton.wasPressedThisFrame)
        {
            PressKey(detected);
        }
    }

    private void SetKeyHighlight(
        int index,
        bool highlighted)
    {
        if (keyObjects == null ||
            index < 0 ||
            index >= keyObjects.Length ||
            keyObjects[index] == null)
            return;

        Image image =
            keyObjects[index].GetComponent<Image>();

        if (image != null)
        {
            image.color =
                highlighted ? highlightColor : keyColor;
        }
    }

    // ==========================================
    // CALCULATOR OPERATIONS
    // ==========================================

    public void PressKey(int index)
    {
        if (!calculatorOpen ||
            keyValues == null ||
            index < 0 ||
            index >= keyValues.Length)
            return;

        // Prevent desktop mouse + EZPZ
        // activating the same key twice.
        if (lastPressFrame == Time.frameCount)
            return;

        lastPressFrame = Time.frameCount;

        string key = keyValues[index];

        if (string.IsNullOrEmpty(key))
            return;

        if (key == "CLOSE")
        {
            CloseCalculator();
            return;
        }

        if (key == "C")
        {
            display = "0";
            storedValue = 0;
            pendingOperator = "";
            startNewNumber = false;
        }
        else if (key == "⌫")
        {
            if (display.Length > 1 && display != "Error")
            {
                display =
                    display.Substring(0, display.Length - 1);
            }
            else
            {
                display = "0";
            }
        }
        else if (key == ".")
        {
            if (startNewNumber || display == "Error")
            {
                display = "0.";
                startNewNumber = false;
            }
            else if (!display.Contains("."))
            {
                display += ".";
            }
        }
        else if (
            key == "+" ||
            key == "−" ||
            key == "×" ||
            key == "÷")
        {
            if (!TryReadDisplay(out double number))
                return;

            if (pendingOperator != "" && !startNewNumber)
            {
                if (!Calculate(number))
                {
                    UpdateDisplay();
                    return;
                }
            }
            else
            {
                storedValue = number;
            }

            pendingOperator = key;
            startNewNumber = true;
        }
        else if (key == "=")
        {
            if (pendingOperator != "" &&
                TryReadDisplay(out double number))
            {
                Calculate(number);
                pendingOperator = "";
                startNewNumber = true;
            }
        }
        else
        {
            if (startNewNumber ||
                display == "0" ||
                display == "Error")
            {
                display = key;
                startNewNumber = false;
            }
            else if (display.Length < 15)
            {
                display += key;
            }
        }

        UpdateDisplay();
    }

    private bool Calculate(double nextNumber)
    {
        double result = storedValue;

        switch (pendingOperator)
        {
            case "+":
                result += nextNumber;
                break;

            case "−":
                result -= nextNumber;
                break;

            case "×":
                result *= nextNumber;
                break;

            case "÷":
                if (nextNumber == 0)
                {
                    display = "Error";
                    pendingOperator = "";
                    return false;
                }

                result /= nextNumber;
                break;
        }

        if (double.IsNaN(result) ||
            double.IsInfinity(result))
        {
            display = "Error";
            return false;
        }

        storedValue = result;

        display = result.ToString(
            "G10",
            CultureInfo.InvariantCulture
        );

        return true;
    }

    private bool TryReadDisplay(out double value)
    {
        return double.TryParse(
            display,
            NumberStyles.Float,
            CultureInfo.InvariantCulture,
            out value
        );
    }

    private void UpdateDisplay()
    {
        if (displayText != null)
            displayText.text = display;
    }
}
