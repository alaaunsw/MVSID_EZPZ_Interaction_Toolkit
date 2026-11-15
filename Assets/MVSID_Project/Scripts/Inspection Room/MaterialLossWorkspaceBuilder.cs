
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MaterialLossWorkspaceBuilder : MonoBehaviour
{
    [Header("Existing Components")]
    public GuidedMaterialLossTraining training;
    public MaterialLossCalculator calculator;

    [Header("World Space")]
    public float canvasScale = 0.0026f;

    [Header("Approved Steps 1 and 2")]
    public Vector2 measurementSize =
        new Vector2(820, 680);

    [Header("Compact Steps 3 and 4")]
    public Vector2 calculationSize =
        new Vector2(820, 680);

    [Header("Calculator Layout")]
    public Vector2 calculatorSize =
        new Vector2(580, 900);

    [Header("Calculator Key Interaction")]
    public float keyColliderDepth = 4f;

    [Header("Editor Calculator Preview")]
    [Range(3, 4)]
    public int editorCalculatorPreviewStep = 3;

    [Tooltip(
        "When enabled, Play Mode preserves the calculator " +
        "positions and colliders saved in Edit Mode."
    )]
    public bool useSavedCalculatorLayout = false;

    [SerializeField, HideInInspector]
    private bool workspaceBuilt;

    private RectTransform panel;
    private Transform content;
    private Transform visuals;

    private TMP_Text learningHeading;
    private TMP_Text pipeIDText;
    private TMP_Text helpText;
    private Transform helpBox;

    private Button openCalculatorButton;
    private Button closeCalculatorButton;

    private int lastStep = -1;
    private int lastOpenFrame = -1;
    private int editorStepOverride = 0;

    private readonly Color navy =
        new Color32(26, 73, 135, 255);

    private readonly Color dark =
        new Color32(43, 55, 72, 255);

    private readonly Color grey =
        new Color32(243, 245, 248, 255);

    private readonly Color paleBlue =
        new Color32(235, 244, 253, 255);

    // =========================================
    // UNITY LIFECYCLE
    // =========================================

    private void Awake()
    {
        FindReferences();
    }

    private void Start()
    {
        if (!FindReferences())
            return;

        FindCalculator();
        FindPresentationElements();
        ConnectExistingUI();
        BindCalculatorButtons();

        if (workspaceBuilt)
            lastStep = -1;
    }

    private void Update()
    {
        if (!Application.isPlaying ||
            !workspaceBuilt ||
            training == null ||
            training.stepText == null)
        {
            return;
        }

        int step = GetStep();

        if (step == lastStep)
            return;

        ApplyLayout(step);
        lastStep = step;
    }

    // =========================================
    // BUILD WORKSPACE
    // =========================================

    [ContextMenu("Build Clean Material Loss Workspace")]
    public void BuildWorkspace()
    {
        if (Application.isPlaying)
        {
            Debug.LogWarning(
                "Stop Play Mode before building."
            );
            return;
        }

        if (!FindReferences())
            return;

        Canvas canvas =
            GetComponentInParent<Canvas>();

        if (canvas == null ||
            canvas.renderMode != RenderMode.WorldSpace)
        {
            Debug.LogError(
                "Material Loss Canvas must use World Space."
            );
            return;
        }

        RectTransform canvasRect =
            canvas.GetComponent<RectTransform>();

        canvasRect.sizeDelta =
            new Vector2(900, 1050);

        canvasRect.localScale =
            Vector3.one * canvasScale;

        SetRect(
            transform,
            0, 0,
            measurementSize.x,
            measurementSize.y
        );

        Image background =
            GetComponent<Image>();

        if (background != null)
        {
            background.color = Color.white;
            background.raycastTarget = false;
        }

        RectTransform contentRect =
            content.GetComponent<RectTransform>();

        contentRect.anchorMin = Vector2.zero;
        contentRect.anchorMax = Vector2.one;
        contentRect.offsetMin = Vector2.zero;
        contentRect.offsetMax = Vector2.zero;

        ConnectExistingUI();

        visuals =
            content.Find("WorkspaceVisuals");

        BuildPresentationElements();

        FindCalculator();
        BuildCalculatorButtons();
        BuildCalculatorPresentation();

        workspaceBuilt = true;
        lastStep = -1;

        ApplyLayout(1);

        Debug.Log(
            "Material Loss Workspace built successfully."
        );
    }

    // =========================================
    // EDITOR CALCULATOR PREVIEW
    // =========================================

    [ContextMenu("Apply Calculator Layout in Editor")]
    public void ApplyCalculatorLayoutInEditor()
    {
        if (Application.isPlaying)
        {
            Debug.LogWarning(
                "Stop Play Mode before applying " +
                "the calculator layout."
            );
            return;
        }

        if (!FindReferences())
            return;

        FindCalculator();
        FindPresentationElements();
        ConnectExistingUI();

        if (calculator == null)
        {
            Debug.LogError(
                "CalculatorPanel was not found."
            );
            return;
        }

#if UNITY_EDITOR
        UnityEditor.Undo.RegisterFullObjectHierarchyUndo(
            calculator.gameObject,
            "Apply Calculator Layout in Editor"
        );

        UnityEditor.Undo.RecordObject(
            this,
            "Save Calculator Layout Settings"
        );
#endif

        // Preview the formula and values of
        // Step 3 or Step 4 without entering Play Mode.
        editorStepOverride =
            Mathf.Clamp(editorCalculatorPreviewStep, 3, 4);

        try
        {
            // Show the existing calculator.
            calculator.gameObject.SetActive(true);

            // Use the same layout method as Play Mode.
            // Force the layout only for this editor command.
            PositionCalculatorOverlay(true);

            Canvas.ForceUpdateCanvases();

            // IMPORTANT:
            // From now on, Play Mode will use the
            // positions saved in this scene.
            useSavedCalculatorLayout = true;
        }
        finally
        {
            editorStepOverride = 0;
        }

#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);

        UnityEditor.EditorUtility.SetDirty(
            calculator.gameObject
        );

        UnityEditor.SceneManagement.EditorSceneManager
            .MarkSceneDirty(gameObject.scene);
#endif

        Debug.Log(
            "Calculator layout saved in Edit Mode. " +
            "You can now move individual keys and " +
            "adjust their colliders in the Inspector."
        );
    }

    // =========================================
    // REFERENCES
    // =========================================

    private bool FindReferences()
    {
        if (training == null)
        {
            training =
                GetComponent<GuidedMaterialLossTraining>();
        }

        panel =
            GetComponent<RectTransform>();

        content =
            transform.Find("TrainingContent");

        if (training == null ||
            panel == null ||
            content == null)
        {
            Debug.LogError(
                "Training controller or " +
                "TrainingContent missing."
            );

            return false;
        }

        return true;
    }

    private void ConnectExistingUI()
    {
        AssignText(
            ref training.stepText,
            "StepText"
        );

        AssignText(
            ref training.instructionText,
            "InstructionText"
        );

        AssignText(
            ref training.inputLabel,
            "InputLabel"
        );

        AssignText(
            ref training.feedbackText,
            "FeedbackText"
        );

        Transform input =
            content.Find("AnswerInput");

        if (input != null)
        {
            training.answerInput =
                input.GetComponent<TMP_InputField>();
        }

        Transform check =
            content.Find("CheckAnswerButton");

        if (check != null)
        {
            training.checkAnswerButton =
                check.gameObject;
        }

        Transform next =
            FindNextButton();

        if (next != null)
        {
            training.nextStepButton =
                next.gameObject;

            Transform label =
                next.Find("ButtonText");

            if (label != null)
            {
                training.nextStepButtonText =
                    label.GetComponent<TMP_Text>();
            }
        }
    }

    private void AssignText(
        ref TMP_Text field,
        string name)
    {
        Transform found =
            content.Find(name);

        if (found != null)
        {
            field =
                found.GetComponent<TMP_Text>();
        }
    }

    private Transform FindNextButton()
    {
        Transform found =
            content.Find("CheckAnswNextStepButton");

        if (found == null)
        {
            found =
                content.Find("NextStepButton");
        }

        return found;
    }

    private void FindCalculator()
    {
        if (calculator == null)
        {
            Transform found =
                content.Find("CalculatorPanel");

            if (found == null &&
                transform.parent != null)
            {
                found =
                    transform.parent.Find(
                        "CalculatorPanel"
                    );
            }

            if (found != null)
            {
                calculator =
                    found.GetComponent<MaterialLossCalculator>();
            }
        }

        if (calculator == null)
        {
            Debug.LogWarning(
                "CalculatorPanel not assigned."
            );
            return;
        }

        if (calculator.transform.parent != content)
        {
            calculator.transform.SetParent(
                content,
                true
            );
        }

        calculator.training = training;

        training.calculatorPanel =
            calculator.gameObject;
    }

    // =========================================
    // PRESENTATION ELEMENTS
    // =========================================

    private void BuildPresentationElements()
    {
        learningHeading = MakeText(
            content,
            "LearningHeading",
            32,
            dark,
            FontStyles.Bold
        );

        pipeIDText = MakeText(
            content,
            "LearningPipeID",
            19,
            dark,
            FontStyles.Normal
        );

        helpBox = MakePanel(
            content,
            "LearningHelpBox",
            grey
        );

        helpText = MakeText(
            helpBox,
            "LearningHelpText",
            22,
            dark,
            FontStyles.Normal
        );

        helpBox.SetAsFirstSibling();

        if (visuals == null)
            return;

        Transform formula =
            visuals.Find(
                "FormulaBackground/FormulaText"
            );

        if (formula != null)
        {
            training.formulaText =
                formula.GetComponent<TMP_Text>();
        }

        AssignVisualText(
            ref training.originalValueText,
            "OriginalThicknessCard/OriginalThicknessValue"
        );

        AssignVisualText(
            ref training.remainingValueText,
            "RemainingThicknessCard/RemainingThicknessValue"
        );

        AssignVisualText(
            ref training.thicknessLossValueText,
            "ThicknessLossCard/ThicknessLossValue"
        );
    }

    private void AssignVisualText(
        ref TMP_Text field,
        string path)
    {
        if (visuals == null)
            return;

        Transform found =
            visuals.Find(path);

        if (found != null)
        {
            field =
                found.GetComponent<TMP_Text>();
        }
    }

    private void FindPresentationElements()
    {
        learningHeading =
            FindText(
                content,
                "LearningHeading"
            );

        pipeIDText =
            FindText(
                content,
                "LearningPipeID"
            );

        helpBox =
            content.Find("LearningHelpBox");

        if (helpBox != null)
        {
            helpText =
                FindText(
                    helpBox,
                    "LearningHelpText"
                );
        }

        visuals =
            content.Find("WorkspaceVisuals");

        Transform open =
            content.Find("OpenCalculatorButton");

        if (open != null)
        {
            openCalculatorButton =
                open.GetComponent<Button>();
        }

        if (calculator != null)
        {
            Transform close =
                calculator.transform.Find(
                    "CloseCalculatorButton"
                );

            if (close != null)
            {
                closeCalculatorButton =
                    close.GetComponent<Button>();
            }
        }
    }

    // =========================================
    // CALCULATOR CONTROLS
    // =========================================

    private void BuildCalculatorButtons()
    {
        openCalculatorButton = MakeButton(
            content,
            "OpenCalculatorButton",
            "OPEN CALCULATOR",
            paleBlue,
            navy
        );

        if (calculator == null)
            return;

        closeCalculatorButton = MakeButton(
            calculator.transform,
            "CloseCalculatorButton",
            "CLOSE",
            navy,
            Color.white
        );
    }

    private Button MakeButton(
        Transform parent,
        string name,
        string caption,
        Color backgroundColor,
        Color textColor)
    {
        Transform found =
            parent.Find(name);

        if (found == null)
        {
            GameObject obj = new GameObject(
                name,
                typeof(RectTransform),
                typeof(Image),
                typeof(Button),
                typeof(BoxCollider)
            );

            obj.transform.SetParent(
                parent,
                false
            );

            found = obj.transform;
        }

        Image image =
            found.GetComponent<Image>();

        Button button =
            found.GetComponent<Button>();

        if (image != null)
        {
            image.color = backgroundColor;
            image.raycastTarget = true;
        }

        if (button != null)
        {
            button.targetGraphic = image;
        }

        TMP_Text label = MakeText(
            found,
            "ButtonText",
            22,
            textColor,
            FontStyles.Bold
        );

        if (label != null)
        {
            label.text = caption;

            label.alignment =
                TextAlignmentOptions.Center;

            SetRect(
                label.transform,
                0, 0, 300, 42
            );
        }

        return button;
    }

    private void BindCalculatorButtons()
    {
        if (openCalculatorButton != null)
        {
            openCalculatorButton.onClick.RemoveListener(
                OpenCalculator
            );

            openCalculatorButton.onClick.AddListener(
                OpenCalculator
            );
        }

        if (closeCalculatorButton != null)
        {
            closeCalculatorButton.onClick.RemoveListener(
                CloseCalculator
            );

            closeCalculatorButton.onClick.AddListener(
                CloseCalculator
            );
        }
    }

    // =========================================
    // OPEN / CLOSE CALCULATOR
    // =========================================

    public void OpenCalculator()
    {
        if (calculator == null)
            return;

        int step = GetStep();

        if (step != 3 && step != 4)
            return;

        if (lastOpenFrame == Time.frameCount)
            return;

        lastOpenFrame = Time.frameCount;

        if (calculator.gameObject.activeSelf)
            return;

        calculator.gameObject.SetActive(true);

        calculator.Clear();

        // If the layout has been saved in Edit Mode,
        // do not move any keys or resize their colliders.
        PositionCalculatorOverlay(false);
    }

    public void CloseCalculator()
    {
        if (calculator != null)
        {
            calculator.gameObject.SetActive(false);
        }
    }

    public void CalculatorResultUsed()
    {
        CloseCalculator();
    }

    // =========================================
    // VERIFIED VALUES AND FORMULA
    // =========================================

    private void BuildCalculatorPresentation()
    {
        if (calculator == null)
            return;

        Transform root =
            calculator.transform;

        Transform old =
            root.Find("VerifiedCalculatorValues");

        if (old != null)
        {
            old.gameObject.SetActive(false);
        }

        Transform valuesPanel = MakePanel(
            root,
            "VerifiedValuesPanel",
            grey
        );

        SetRect(
            valuesPanel,
            0, 238, 510, 114
        );

        MakeValueCard(
            valuesPanel,
            "OriginalValueCard",
            -132
        );

        MakeValueCard(
            valuesPanel,
            "SecondValueCard",
            132
        );

        Transform formulaPanel = MakePanel(
            root,
            "CalculatorFormulaPanel",
            paleBlue
        );

        SetRect(
            formulaPanel,
            0, 123, 510, 92
        );

        TMP_Text formula = MakeText(
            formulaPanel,
            "CalculatorFormulaText",
            20,
            navy,
            FontStyles.Normal
        );

        if (formula != null)
        {
            formula.alignment =
                TextAlignmentOptions.Center;

            SetRect(
                formula.transform,
                0, 0, 482, 84
            );
        }
    }

    private void MakeValueCard(
        Transform parent,
        string name,
        float x)
    {
        Transform card = MakePanel(
            parent,
            name,
            Color.white
        );

        SetRect(
            card,
            x, 0, 245, 99
        );

        TMP_Text heading = MakeText(
            card,
            "Heading",
            16,
            dark,
            FontStyles.Normal
        );

        if (heading != null)
        {
            heading.alignment =
                TextAlignmentOptions.Center;

            SetRect(
                heading.transform,
                0, 23, 232, 30
            );
        }

        TMP_Text value = MakeText(
            card,
            "Value",
            30,
            navy,
            FontStyles.Bold
        );

        if (value != null)
        {
            value.alignment =
                TextAlignmentOptions.Center;

            SetRect(
                value.transform,
                0, -19, 232, 44
            );
        }
    }

    private void UpdateCalculatorValues()
    {
        if (calculator == null ||
            training == null)
        {
            return;
        }

        PipeInspectionData data =
            training.pipeTarget != null
            ? training.pipeTarget.GetInspectionData()
            : null;

        if (data == null)
        {
            Debug.LogWarning(
                "Calculator pipe data unavailable."
            );
            return;
        }

        int step = GetStep();

        float original =
            data.referenceThicknessMM;

        float remaining =
            data.remainingThicknessMM;

        float loss =
            original - remaining;

        Transform root =
            calculator.transform;

        Transform valuesPanel =
            root.Find("VerifiedValuesPanel");

        if (valuesPanel != null)
        {
            SetCardValues(
                valuesPanel.Find("OriginalValueCard"),
                "ORIGINAL THICKNESS",
                original.ToString("F2") + " mm"
            );

            SetCardValues(
                valuesPanel.Find("SecondValueCard"),
                step == 3
                    ? "REMAINING THICKNESS"
                    : "THICKNESS LOSS",
                (step == 3 ? remaining : loss)
                    .ToString("F2") + " mm"
            );
        }

        Transform formulaPanel =
            root.Find("CalculatorFormulaPanel");

        TMP_Text formula =
            FindText(
                formulaPanel,
                "CalculatorFormulaText"
            );

        if (formula != null)
        {
            formula.text = step == 3
                ? "<b>FORMULA</b>\n" +
                  "Thickness Loss = Original - Remaining"
                : "<b>FORMULA</b>\n" +
                  "Loss (%) = (Thickness Loss / Original) x 100";
        }
    }

    private void SetCardValues(
        Transform card,
        string heading,
        string value)
    {
        if (card == null)
            return;

        TMP_Text headingText =
            FindText(card, "Heading");

        TMP_Text valueText =
            FindText(card, "Value");

        if (headingText != null)
            headingText.text = heading;

        if (valueText != null)
            valueText.text = value;
    }

    // =========================================
    // CALCULATOR LAYOUT
    // =========================================

    private void PositionCalculatorOverlay(
        bool forceLayout)
    {
        if (calculator == null)
            return;

        Transform root =
            calculator.transform;

        RectTransform rect =
            calculator.GetComponent<RectTransform>();

        if (rect == null)
            return;

        // Always show the calculator in front.
        root.SetAsLastSibling();

        // Refresh Step 3 / Step 4 values,
        // without modifying button positions.
        UpdateCalculatorValues();

        Transform old =
            root.Find("VerifiedCalculatorValues");

        if (old != null)
            old.gameObject.SetActive(false);

        Transform valuesPanel =
            root.Find("VerifiedValuesPanel");

        if (valuesPanel != null)
            valuesPanel.gameObject.SetActive(true);

        Transform formulaPanel =
            root.Find("CalculatorFormulaPanel");

        if (formulaPanel != null)
            formulaPanel.gameObject.SetActive(true);

        // IMPORTANT:
        // Once the editor layout is saved,
        // Play Mode does not overwrite it.
        if (useSavedCalculatorLayout && !forceLayout)
        {
            return;
        }

        // -------------------------------------
        // CALCULATOR PANEL
        // -------------------------------------

        rect.anchorMin =
            new Vector2(0.5f, 0.5f);

        rect.anchorMax =
            new Vector2(0.5f, 0.5f);

        rect.pivot =
            new Vector2(0.5f, 0.5f);

        rect.sizeDelta = calculatorSize;

        rect.anchoredPosition3D =
            Vector3.zero;

        rect.localScale = Vector3.one;
        rect.localRotation = Quaternion.identity;

        // -------------------------------------
        // TITLE
        // -------------------------------------

        Place(
            root.Find("CalculatorTitle"),
            -45, 405, 385, 40, 26,
            TextAlignmentOptions.Center
        );

        // -------------------------------------
        // CLOSE BUTTON
        // -------------------------------------

        if (closeCalculatorButton == null)
        {
            Transform close =
                root.Find("CloseCalculatorButton");

            if (close != null)
            {
                closeCalculatorButton =
                    close.GetComponent<Button>();
            }
        }

        if (closeCalculatorButton != null)
        {
            Transform close =
                closeCalculatorButton.transform;

            SetRect(
                close,
                220, 405, 105, 42
            );

            ResizeCollider(
                close,
                105, 42
            );

            TMP_Text label =
                close.GetComponentInChildren<TMP_Text>(
                    true
                );

            if (label != null)
            {
                label.text = "CLOSE";
                label.fontSize = 16;

                label.alignment =
                    TextAlignmentOptions.Center;

                SetRect(
                    label.transform,
                    0, 0, 97, 35
                );

                label.raycastTarget = false;
            }

            close.SetAsLastSibling();
        }

        // -------------------------------------
        // VERIFIED VALUES
        // -------------------------------------

        if (valuesPanel != null)
        {
            SetRect(
                valuesPanel,
                0, 295, 510, 114
            );

            DisableDecorativeRaycasts(
                valuesPanel
            );
        }

        // -------------------------------------
        // FORMULA
        // -------------------------------------

        if (formulaPanel != null)
        {
            SetRect(
                formulaPanel,
                0, 185, 510, 92
            );

            DisableDecorativeRaycasts(
                formulaPanel
            );
        }

        // -------------------------------------
        // EXPRESSION
        // -------------------------------------

        Transform expression =
            root.Find("ExpressionText");

        Place(
            expression,
            0, 113, 490, 26, 18,
            TextAlignmentOptions.Right
        );

        if (expression != null)
        {
            TMP_Text expressionText =
                expression.GetComponent<TMP_Text>();

            if (expressionText != null)
                expressionText.raycastTarget = false;
        }

        // -------------------------------------
        // CALCULATOR DISPLAY
        // -------------------------------------

        Transform display =
            root.Find("DisplayBackground");

        SetRect(
            display,
            0, 61, 510, 65
        );

        if (display != null)
        {
            Image image =
                display.GetComponent<Image>();

            if (image != null)
                image.raycastTarget = false;
        }

        if (calculator.displayText != null)
        {
            SetRect(
                calculator.displayText.transform,
                0, 0, 470, 56
            );

            calculator.displayText.raycastTarget = false;
        }

        // -------------------------------------
        // EXISTING CALCULATOR KEYPAD
        // -------------------------------------

        // Reuse existing keys.
        // Do not create new buttons or events.

        string[,] keys =
        {
            { "7", "8", "9", "/" },
            { "4", "5", "6", "*" },
            { "1", "2", "3", "-" },
            { "C", "0", ".", "+" },
            { "BACK", "=", "USE", "" }
        };

        for (int row = 0; row < 5; row++)
        {
            for (int col = 0; col < 4; col++)
            {
                string key = keys[row, col];

                if (string.IsNullOrEmpty(key))
                    continue;

                Transform keyObject =
                    root.Find("CalculatorKey_" + key);

                if (keyObject == null)
                {
                    Debug.LogWarning(
                        "Missing calculator key: " + key
                    );
                    continue;
                }

                float x = row == 4
                    ? -130 + col * 130
                    : -195 + col * 130;

                float y = -47 - row * 67;

                SetRect(
                    keyObject,
                    x, y, 117, 54
                );

                ResizeCollider(
                    keyObject,
                    117, 54
                );

                Image keyImage =
                    keyObject.GetComponent<Image>();

                if (keyImage != null)
                    keyImage.raycastTarget = true;

                TMP_Text[] labels =
                    keyObject.GetComponentsInChildren<TMP_Text>(
                        true
                    );

                foreach (TMP_Text label in labels)
                {
                    label.raycastTarget = false;
                }
            }
        }

        // -------------------------------------
        // INSTRUCTIONS
        // -------------------------------------

        Transform hint =
            root.Find("CalculatorHint");

        Place(
            hint,
            0, -380, 510, 32, 17,
            TextAlignmentOptions.Center
        );

        if (hint != null)
        {
            TMP_Text hintText =
                hint.GetComponent<TMP_Text>();

            if (hintText != null)
            {
                hintText.text =
                    "Press = to calculate, then " +
                    "USE to transfer.";

                hintText.alignment =
                    TextAlignmentOptions.Center;

                hintText.raycastTarget = false;
            }
        }
    }

    // =========================================
    // DECORATIVE UI RAYCAST SETTINGS
    // =========================================

    private void DisableDecorativeRaycasts(
        Transform root)
    {
        if (root == null)
            return;

        Graphic[] graphics =
            root.GetComponentsInChildren<Graphic>(
                true
            );

        foreach (Graphic graphic in graphics)
        {
            graphic.raycastTarget = false;
        }
    }

    // =========================================
    // STEP MANAGEMENT
    // =========================================

    private int GetStep()
    {
        // Editor preview of Step 3 or Step 4.
        if (editorStepOverride == 3 ||
            editorStepOverride == 4)
        {
            return editorStepOverride;
        }

        if (training == null ||
            training.stepText == null)
        {
            return 1;
        }

        string heading =
            training.stepText.text;

        if (heading.Contains("STEP 1"))
            return 1;

        if (heading.Contains("STEP 2"))
            return 2;

        if (heading.Contains("STEP 3"))
            return 3;

        if (heading.Contains("STEP 4"))
            return 4;

        if (heading.Contains("COMPLETED"))
            return 5;

        return 1;
    }

    private void ApplyLayout(int step)
    {
        if (!FindReferences())
            return;

        FindPresentationElements();

        if (step == 1 || step == 2)
        {
            LayoutMeasurementStep(step);
        }
        else if (step == 3 || step == 4)
        {
            LayoutCalculationStep(step);
        }
        else
        {
            LayoutCompletion();
        }
    }

    // =========================================
    // APPROVED STEPS 1 AND 2
    // =========================================

    private void LayoutMeasurementStep(int step)
    {
        panel.sizeDelta = measurementSize;

        if (visuals != null)
            visuals.gameObject.SetActive(false);

        if (calculator != null)
            calculator.gameObject.SetActive(false);

        if (openCalculatorButton != null)
            openCalculatorButton.gameObject.SetActive(false);

        Transform oldHint =
            content.Find("StepHintBackground");

        if (oldHint != null)
            oldHint.gameObject.SetActive(false);

        Transform oldBackground =
            content.Find("InstructionBackground");

        if (oldBackground != null)
            oldBackground.gameObject.SetActive(false);

        Place(
            content.Find("TitleText"),
            -103, 279, 570, 43, 26,
            TextAlignmentOptions.Left
        );

        Place(
            content.Find("GoldDivider"),
            0, 229, 730, 3
        );

        if (pipeIDText != null)
        {
            pipeIDText.gameObject.SetActive(true);

            Place(
                pipeIDText.transform,
                -292, 240, 160, 30, 18,
                TextAlignmentOptions.Left
            );

            PipeInspectionData data =
                training.pipeTarget != null
                ? training.pipeTarget.GetInspectionData()
                : null;

            pipeIDText.text =
                data != null ? data.pipeID : "PIPE";
        }

        Place(
            content.Find("StepText"),
            296, 279, 170, 35, 20,
            TextAlignmentOptions.Right
        );

        if (learningHeading != null)
        {
            learningHeading.gameObject.SetActive(true);

            learningHeading.text = step == 1
                ? "Find the Original Wall Thickness"
                : "Find the Remaining Wall Thickness";

            Place(
                learningHeading.transform,
                0, 154, 730, 56, 32,
                TextAlignmentOptions.Left
            );
        }

        if (training.instructionText != null)
        {
            training.instructionText.text = step == 1
                ? "Open the pipe information card and " +
                  "locate the original wall thickness."
                : "Open the pipe information card and " +
                  "locate the remaining wall thickness.";
        }

        Place(
            content.Find("InstructionText"),
            0, 97, 730, 55, 22,
            TextAlignmentOptions.Left
        );

        if (helpBox != null)
        {
            helpBox.gameObject.SetActive(true);

            SetRect(
                helpBox,
                0, 12, 730, 81
            );
        }

        if (helpText != null)
        {
            helpText.text = step == 1
                ? "<b>WHERE TO LOOK</b>\n" +
                  "Wall Thickness (Original)"
                : "<b>WHERE TO LOOK</b>\n" +
                  "Remaining Thickness";

            Place(
                helpText.transform,
                0, 0, 683, 74, 22,
                TextAlignmentOptions.Left
            );
        }

        Place(
            content.Find("InputLabel"),
            0, -88, 730, 40, 22,
            TextAlignmentOptions.Left
        );

        Place(
            content.Find("AnswerInput"),
            0, -139, 730, 58
        );

        Place(
            content.Find("FeedbackText"),
            0, -208, 730, 55, 20,
            TextAlignmentOptions.Center
        );

        PlaceButtons(
            -280, 730, 57
        );

        ResizeCollider(
            content.Find("AnswerInput"),
            730, 58
        );

        FormatInputText();
    }

    // =========================================
    // COMPACT STEPS 3 AND 4
    // =========================================

    private void LayoutCalculationStep(int step)
    {
        panel.sizeDelta = calculationSize;

        if (visuals != null)
        {
            visuals.gameObject.SetActive(true);

            foreach (Transform child in visuals)
            {
                child.gameObject.SetActive(
                    child.name == "FormulaBackground"
                );
            }
        }

        if (calculator != null)
            calculator.gameObject.SetActive(false);

        if (helpBox != null)
            helpBox.gameObject.SetActive(false);

        Transform oldHint =
            content.Find("StepHintBackground");

        if (oldHint != null)
            oldHint.gameObject.SetActive(false);

        Transform oldBackground =
            content.Find("InstructionBackground");

        if (oldBackground != null)
            oldBackground.gameObject.SetActive(false);

        Place(
            content.Find("TitleText"),
            -103, 279, 570, 43, 26,
            TextAlignmentOptions.Left
        );

        Place(
            content.Find("StepText"),
            296, 279, 170, 35, 20,
            TextAlignmentOptions.Right
        );

        Place(
            content.Find("GoldDivider"),
            0, 229, 730, 3
        );

        if (pipeIDText != null)
        {
            pipeIDText.gameObject.SetActive(true);

            Place(
                pipeIDText.transform,
                -292, 240, 160, 30, 18,
                TextAlignmentOptions.Left
            );

            PipeInspectionData data =
                training.pipeTarget != null
                ? training.pipeTarget.GetInspectionData()
                : null;

            if (data != null)
                pipeIDText.text = data.pipeID;
        }

        if (learningHeading != null)
        {
            learningHeading.gameObject.SetActive(true);

            learningHeading.text = step == 3
                ? "Calculate Thickness Loss"
                : "Calculate Material Loss (%)";

            Place(
                learningHeading.transform,
                0, 154, 730, 56, 32,
                TextAlignmentOptions.Left
            );
        }

        if (training.instructionText != null)
        {
            training.instructionText.text =
                "Use the formula and your verified " +
                "values in the calculator.";
        }

        Place(
            content.Find("InstructionText"),
            0, 97, 730, 55, 22,
            TextAlignmentOptions.Left
        );

        Transform formula =
            visuals != null
            ? visuals.Find("FormulaBackground")
            : null;

        if (formula != null)
        {
            formula.gameObject.SetActive(true);

            SetRect(
                formula,
                0, 9, 730, 89
            );

            if (training.formulaText != null)
            {
                training.formulaText.text =
                    step == 3
                    ? "FORMULA\n" +
                      "Thickness Loss = Original Thickness " +
                      "- Remaining Thickness"
                    : "FORMULA\n" +
                      "Loss (%) = (Thickness Loss / " +
                      "Original Thickness) x 100";
            }

            Place(
                formula.Find("FormulaText"),
                0, 0, 690, 81, 20,
                TextAlignmentOptions.Center
            );
        }

        if (openCalculatorButton != null)
        {
            openCalculatorButton.gameObject.SetActive(true);

            SetRect(
                openCalculatorButton.transform,
                0, -86, 730, 52
            );

            ResizeCollider(
                openCalculatorButton.transform,
                730, 52
            );

            TMP_Text label =
                openCalculatorButton
                .GetComponentInChildren<TMP_Text>(true);

            if (label != null)
            {
                SetRect(
                    label.transform,
                    0, 0, 690, 46
                );
            }
        }

        Place(
            content.Find("InputLabel"),
            0, -146, 730, 34, 22,
            TextAlignmentOptions.Left
        );

        Place(
            content.Find("AnswerInput"),
            0, -200, 730, 57
        );

        Place(
            content.Find("FeedbackText"),
            0, -254, 730, 44, 20,
            TextAlignmentOptions.Center
        );

        PlaceButtons(
            -305, 730, 55
        );

        ResizeCollider(
            content.Find("AnswerInput"),
            730, 57
        );

        FormatInputText();
    }

    // =========================================
    // COMPLETION
    // =========================================

    private void LayoutCompletion()
    {
        LayoutMeasurementStep(2);

        if (learningHeading != null)
        {
            learningHeading.text =
                "Calculation Completed";
        }

        if (helpBox != null)
            helpBox.gameObject.SetActive(true);

        if (helpText != null)
        {
            helpText.text =
                "All four steps completed.\n" +
                "Review your final result.";
        }

        if (training.answerInput != null)
        {
            training.answerInput.interactable = false;
        }

        if (training.checkAnswerButton != null)
        {
            training.checkAnswerButton.SetActive(false);
        }

        if (training.nextStepButton != null)
        {
            training.nextStepButton.SetActive(false);
        }
    }

    // =========================================
    // UI HELPERS
    // =========================================

    private Transform MakePanel(
        Transform parent,
        string name,
        Color color)
    {
        Transform found =
            parent.Find(name);

        if (found == null)
        {
            GameObject obj = new GameObject(
                name,
                typeof(RectTransform),
                typeof(Image)
            );

            obj.transform.SetParent(
                parent,
                false
            );

            found = obj.transform;
        }

        Image image =
            found.GetComponent<Image>();

        if (image != null)
        {
            image.color = color;
            image.raycastTarget = false;
        }

        return found;
    }

    private TMP_Text MakeText(
        Transform parent,
        string name,
        float size,
        Color color,
        FontStyles style)
    {
        Transform found =
            parent.Find(name);

        if (found == null)
        {
            GameObject obj = new GameObject(
                name,
                typeof(RectTransform),
                typeof(TextMeshProUGUI)
            );

            obj.transform.SetParent(
                parent,
                false
            );

            found = obj.transform;
        }

        TMP_Text text =
            found.GetComponent<TMP_Text>();

        if (text == null)
            return null;

        text.fontSize = size;
        text.fontStyle = style;
        text.color = color;

        text.alignment =
            TextAlignmentOptions.Left;

        text.verticalAlignment =
            VerticalAlignmentOptions.Middle;

        text.enableWordWrapping = true;
        text.enableAutoSizing = false;
        text.raycastTarget = false;

        return text;
    }

    private TMP_Text FindText(
        Transform parent,
        string name)
    {
        if (parent == null)
            return null;

        Transform found =
            parent.Find(name);

        return found != null
            ? found.GetComponent<TMP_Text>()
            : null;
    }

    private void PlaceButtons(
        float y,
        float width,
        float height)
    {
        Transform check =
            content.Find("CheckAnswerButton");

        Transform next =
            FindNextButton();

        SetRect(
            check,
            0, y, width, height
        );

        SetRect(
            next,
            0, y, width, height
        );

        ResizeCollider(
            check,
            width, height
        );

        ResizeCollider(
            next,
            width, height
        );
    }

    private void SetRect(
        Transform target,
        float x,
        float y,
        float width,
        float height)
    {
        if (target == null)
            return;

        RectTransform rect =
            target as RectTransform;

        if (rect == null)
            return;

        rect.anchorMin =
            new Vector2(0.5f, 0.5f);

        rect.anchorMax =
            new Vector2(0.5f, 0.5f);

        rect.pivot =
            new Vector2(0.5f, 0.5f);

        rect.anchoredPosition3D =
            new Vector3(x, y, 0f);

        rect.sizeDelta =
            new Vector2(width, height);

        rect.localScale = Vector3.one;
        rect.localRotation = Quaternion.identity;
    }

    private void Place(
        Transform target,
        float x,
        float y,
        float width,
        float height,
        float fontSize = 0,
        TextAlignmentOptions alignment =
            TextAlignmentOptions.Center)
    {
        if (target == null)
            return;

        SetRect(
            target,
            x, y, width, height
        );

        TMP_Text text =
            target.GetComponent<TMP_Text>();

        if (text == null)
            return;

        if (fontSize > 0)
        {
            text.fontSize = fontSize;
            text.enableAutoSizing = false;
        }

        text.alignment = alignment;
        text.enableWordWrapping = true;
    }

    // =========================================
    // BOX COLLIDER ALIGNMENT
    // =========================================

    private void ResizeCollider(
        Transform target,
        float width,
        float height)
    {
        if (target == null)
            return;

        BoxCollider box =
            target.GetComponent<BoxCollider>();

        if (box == null)
            return;

        box.center = Vector3.zero;

        box.size = new Vector3(
            width,
            height,
            Mathf.Max(0.1f, keyColliderDepth)
        );
    }

    private void FormatInputText()
    {
        if (training.answerInput == null)
            return;

        TMP_Text text =
            training.answerInput.textComponent;

        if (text != null)
        {
            text.fontSize = 26;

            text.alignment =
                TextAlignmentOptions.Center;
        }

        TMP_Text placeholder =
            training.answerInput.placeholder
                as TMP_Text;

        if (placeholder != null)
        {
            placeholder.fontSize = 21;

            placeholder.alignment =
                TextAlignmentOptions.Center;
        }
    }
}
