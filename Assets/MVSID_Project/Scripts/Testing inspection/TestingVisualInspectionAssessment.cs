
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public class TestingVisualInspectionAssessment : MonoBehaviour
{
    [Header("Camera")]
    public Camera playerCamera;
    public float distanceFromPlayer = 1.6f;

    [Header("Colours")]
    public Color panelColor =
        new Color32(8, 8, 8, 225);

    public Color normalBorder =
        new Color32(130, 130, 130, 255);

    public Color hoverColor =
        new Color32(255, 213, 79, 255);

    public Color answerBackground =
        new Color32(30, 30, 30, 245);

    [Header("Editable Assessment")]
    [SerializeField]
    private GameObject assessmentRoot;

    [SerializeField]
    private Image[] answerBorders = new Image[4];

    [SerializeField]
    private Image[] letterBackgrounds = new Image[4];

    [SerializeField]
    private TextMeshProUGUI[] answerTexts =
        new TextMeshProUGUI[4];

    [SerializeField]
    private BoxCollider[] answerColliders =
        new BoxCollider[4];

    [SerializeField]
    private Button[] answerButtons = new Button[4];

    [SerializeField]
    private InteractableGeneral[] answerEZPZ =
        new InteractableGeneral[4];

    // ===================================
    // ASSESSMENT RESULTS
    // ===================================

    [Header("Assessment Results")]

    [SerializeField]
    private int selectedAnswer = -1;

    [SerializeField]
    private bool submitted = false;

    [SerializeField]
    private bool correctAnswerSelected;

    [SerializeField]
    private bool assessmentRecorded;

    [Header("After Submission")]
    public UnityEvent onAnswerSubmitted;

    private int hoveredIndex = -1;

    private const int correctAnswerIndex = 1;

    public int SelectedAnswer => selectedAnswer;

    public bool IsSubmitted => submitted;

    public bool IsCorrectAnswer
        => submitted && correctAnswerSelected;

    public bool IsAssessmentRecorded
        => assessmentRecorded;

    public string SubmittedAnswer
    {
        get
        {
            if (selectedAnswer < 0 ||
                selectedAnswer >= answers.Length)
            {
                return "";
            }

            return answers[selectedAnswer];
        }
    }

    public string ExpectedAnswer
        => answers[correctAnswerIndex];

    private readonly string[] answers =
    {
        "No visible damage",
        "Corrosion near the pipe support",
        "Cracking along the pipeline",
        "Active pipeline leakage"
    };

    // ===================================
    // BUILD IN UNITY EDIT MODE
    // ===================================

    [ContextMenu("Build Assessment")]
    public void BuildAssessment()
    {
        if (Application.isPlaying)
        {
            Debug.LogWarning(
                "Stop Play Mode before building."
            );

            return;
        }

        // Remove only the previously generated
        // assessment belonging to this controller.

        if (assessmentRoot != null)
        {
#if UNITY_EDITOR
            Undo.DestroyObjectImmediate(assessmentRoot);
#else
            DestroyImmediate(assessmentRoot);
#endif
        }

        assessmentRoot = new GameObject(
            "Testing_VisualInspection_Assessment",
            typeof(RectTransform),
            typeof(Canvas),
            typeof(GraphicRaycaster)
        );

#if UNITY_EDITOR
        Undo.RegisterCreatedObjectUndo(
            assessmentRoot,
            "Build Visual Inspection Assessment"
        );
#endif

        // Keep it at scene root for easy editing.

        assessmentRoot.transform.SetParent(null);

        Canvas canvas =
            assessmentRoot.GetComponent<Canvas>();

        canvas.renderMode = RenderMode.WorldSpace;

        canvas.worldCamera = playerCamera;

        canvas.overrideSorting = true;

        canvas.sortingOrder = 100;

        RectTransform root =
            assessmentRoot.GetComponent<RectTransform>();

        root.sizeDelta =
            new Vector2(1120, 580);

        root.localScale =
            Vector3.one * 0.0017f;

        // ===================================
        // MAIN BLACK BACKGROUND
        // ===================================

        RectTransform panel = CreateRect(
            "MainPanel",
            assessmentRoot.transform,
            Vector2.zero,
            new Vector2(1120, 580)
        );

        AddImage(panel, panelColor);

        CreateText(
            "AssessmentTitle",
            panel,
            "PIPELINE INSPECTION ASSESSMENT",
            new Vector2(0, 245),
            new Vector2(1050, 55),
            32,
            Color.white
        );

        // ===================================
        // LEFT QUESTION
        // ===================================

        RectTransform questionBorder = CreateRect(
            "QuestionBorder",
            panel,
            new Vector2(-275, -15),
            new Vector2(490, 420)
        );

        AddImage(
            questionBorder,
            normalBorder
        );

        RectTransform questionBackground = CreateRect(
            "QuestionBackground",
            questionBorder,
            Vector2.zero,
            new Vector2(482, 412)
        );

        AddImage(
            questionBackground,
            new Color32(22, 22, 22, 245)
        );

        CreateText(
            "QuestionNumber",
            questionBackground,
            "01  VISUAL INSPECTION",
            new Vector2(0, 135),
            new Vector2(445, 55),
            28,
            hoverColor
        );

        CreateText(
            "QuestionText",
            questionBackground,
            "What did you observe\non PIPE_0493?",
            new Vector2(0, 10),
            new Vector2(440, 185),
            42,
            Color.white
        );

        CreateText(
            "Instruction",
            questionBackground,
            "SELECT ONE ANSWER",
            new Vector2(0, -140),
            new Vector2(420, 50),
            24,
            Color.white
        );

        // ===================================
        // RIGHT ANSWERS
        // ===================================

        RectTransform column = CreateRect(
            "AnswerColumn",
            panel,
            new Vector2(275, -15),
            new Vector2(510, 410)
        );

        float[] positions =
        {
            150f,
            50f,
            -50f,
            -150f
        };

        for (int i = 0; i < 4; i++)
        {
            CreateAnswer(
                column,
                i,
                positions[i]
            );
        }

        // Put assessment in front of camera
        // for easier editing.

        PositionInFrontOfCamera();

        // Visible in Edit Mode for editing.

        assessmentRoot.SetActive(true);

#if UNITY_EDITOR
        EditorUtility.SetDirty(this);

        EditorSceneManager.MarkSceneDirty(
            gameObject.scene
        );
#endif

        Debug.Log(
            "[TESTING] Editable assessment built: " +
            "all four answers created."
        );
    }

    // ===================================
    // RECT TRANSFORM HELPER
    // ===================================

    private RectTransform CreateRect(
        string objectName,
        Transform parent,
        Vector2 position,
        Vector2 size)
    {
        GameObject obj = new GameObject(
            objectName,
            typeof(RectTransform)
        );

        obj.transform.SetParent(parent, false);

        RectTransform rect =
            obj.GetComponent<RectTransform>();

        rect.anchorMin =
            Vector2.one * 0.5f;

        rect.anchorMax =
            Vector2.one * 0.5f;

        rect.pivot =
            Vector2.one * 0.5f;

        rect.anchoredPosition3D =
            new Vector3(
                position.x,
                position.y,
                0f
            );

        rect.sizeDelta = size;

        rect.localRotation =
            Quaternion.identity;

        rect.localScale = Vector3.one;

        return rect;
    }

    // ===================================
    // IMAGE HELPER
    // ===================================

    private Image AddImage(
        RectTransform rect,
        Color imageColor,
        bool raycast = false)
    {
        Image image =
            rect.gameObject.AddComponent<Image>();

        image.color = imageColor;

        image.raycastTarget = raycast;

        return image;
    }

    // ===================================
    // TEXT HELPER
    // ===================================

    private TextMeshProUGUI CreateText(
        string objectName,
        Transform parent,
        string content,
        Vector2 position,
        Vector2 size,
        float fontSize,
        Color textColor)
    {
        RectTransform rect = CreateRect(
            objectName,
            parent,
            position,
            size
        );

        TextMeshProUGUI text =
            rect.gameObject.AddComponent<TextMeshProUGUI>();

        text.text = content;

        text.fontSize = fontSize;

        text.color = textColor;

        text.alignment =
            TextAlignmentOptions.Center;

        text.enableWordWrapping = true;

        text.raycastTarget = false;

        return text;
    }

    // ===================================
    // BUILD INDIVIDUAL ANSWER
    // ===================================

    private void CreateAnswer(
        Transform parent,
        int index,
        float yPosition)
    {
        RectTransform row = CreateRect(
            "Answer_" + (index + 1),
            parent,
            new Vector2(0, yPosition),
            new Vector2(500, 82)
        );

        // Outer grey border and click target.

        Image border = AddImage(
            row,
            normalBorder,
            true
        );

        RectTransform background = CreateRect(
            "AnswerBackground",
            row,
            Vector2.zero,
            new Vector2(492, 74)
        );

        AddImage(
            background,
            answerBackground
        );

        RectTransform letterBox = CreateRect(
            "LetterBox",
            row,
            new Vector2(-215, 0),
            new Vector2(54, 54)
        );

        Image letterImage = AddImage(
            letterBox,
            new Color32(85, 85, 85, 255)
        );

        CreateText(
            "LetterText",
            letterBox,
            ((char)('A' + index)).ToString(),
            Vector2.zero,
            new Vector2(50, 50),
            30,
            Color.white
        );

        TextMeshProUGUI description = CreateText(
            "AnswerText",
            row,
            answers[index],
            new Vector2(35, 0),
            new Vector2(385, 70),
            26,
            Color.white
        );

        description.alignment =
            TextAlignmentOptions.MidlineLeft;

        // ===================================
        // EXISTING PHYSICS INTERACTION
        // ===================================

        BoxCollider box =
            row.gameObject.AddComponent<BoxCollider>();

        box.center = Vector3.zero;

        box.size =
            new Vector3(500, 82, 8);

        box.isTrigger = false;

        Button button =
            row.gameObject.AddComponent<Button>();

        button.targetGraphic = border;

        button.transition =
            Selectable.Transition.None;

        // Existing EZPZ interaction component.

        InteractableGeneral ezpz =
            row.gameObject.AddComponent<
                InteractableGeneral
            >();

        answerBorders[index] = border;

        letterBackgrounds[index] = letterImage;

        answerTexts[index] = description;

        answerColliders[index] = box;

        answerButtons[index] = button;

        answerEZPZ[index] = ezpz;

        Debug.Log(
            "[TESTING] Built Answer_" +
            (index + 1)
        );
    }

    // ===================================
    // POSITION
    // ===================================

    private void PositionInFrontOfCamera()
    {
        if (assessmentRoot == null)
            return;

        if (playerCamera == null)
            playerCamera = Camera.main;

        if (playerCamera == null)
            return;

        Transform cam =
            playerCamera.transform;

        assessmentRoot.transform.position =
            cam.position +
            cam.forward * distanceFromPlayer;

        assessmentRoot.transform.rotation =
            cam.rotation;
    }

    // ===================================
    // CONNECT RUNTIME EVENTS
    // ===================================

    private void Awake()
    {
        if (!Application.isPlaying)
            return;

        selectedAnswer = -1;

        submitted = false;

        correctAnswerSelected = false;

        assessmentRecorded = false;

        if (assessmentRoot == null)
        {
            Debug.LogError(
                "[TESTING] Build Assessment in Edit Mode first."
            );

            return;
        }

        // Hide during gameplay until
        // visual inspection ends.

        assessmentRoot.SetActive(false);

        for (int i = 0; i < 4; i++)
        {
            int index = i;

            if (answerButtons[i] != null)
            {
                answerButtons[i].onClick.AddListener(
                    () => SubmitAnswer(index)
                );
            }

            if (answerEZPZ[i] != null)
            {
                answerEZPZ[i].onHoverEnter.AddListener(
                    () => SetHover(index)
                );

                answerEZPZ[i].onHoverExit.AddListener(
                    () => ClearHover(index)
                );

                answerEZPZ[i].onPrimaryInteract.AddListener(
                    () => SubmitAnswer(index)
                );
            }
        }
    }

    // ===================================
    // SHOW QUESTION
    // ===================================

    public void ShowQuestion()
    {
        if (assessmentRoot == null || submitted)
            return;

        PositionInFrontOfCamera();

        hoveredIndex = -1;

        RefreshAnswers();

        assessmentRoot.SetActive(true);

        Debug.Log(
            "[TESTING] Visual assessment opened."
        );
    }

    // ===================================
    // DESKTOP MOUSE INTERACTION
    // ===================================

    private void Update()
    {
        if (assessmentRoot == null ||
            !assessmentRoot.activeSelf ||
            submitted ||
            playerCamera == null ||
            Mouse.current == null)
        {
            return;
        }

        Vector2 position =
            Mouse.current.position.ReadValue();

        if (Cursor.lockState ==
            CursorLockMode.Locked)
        {
            position = new Vector2(
                Screen.width * 0.5f,
                Screen.height * 0.5f
            );
        }

        Ray ray =
            playerCamera.ScreenPointToRay(
                position
            );

        int detected = -1;

        for (int i = 0; i < 4; i++)
        {
            if (answerColliders[i] != null &&
                answerColliders[i].Raycast(
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
            hoveredIndex = detected;

            RefreshAnswers();
        }

        if (detected >= 0 &&
            Mouse.current.leftButton.wasPressedThisFrame)
        {
            SubmitAnswer(detected);
        }
    }

    // ===================================
    // HOVER
    // ===================================

    public void SetHover(int index)
    {
        if (submitted)
            return;

        hoveredIndex = index;

        RefreshAnswers();
    }

    public void ClearHover(int index)
    {
        if (submitted)
            return;

        if (hoveredIndex == index)
        {
            hoveredIndex = -1;

            RefreshAnswers();
        }
    }

    private void RefreshAnswers()
    {
        for (int i = 0; i < 4; i++)
        {
            bool hover =
                i == hoveredIndex;

            if (answerBorders[i] != null)
            {
                answerBorders[i].color =
                    hover
                        ? hoverColor
                        : normalBorder;
            }

            if (letterBackgrounds[i] != null)
            {
                letterBackgrounds[i].color =
                    hover
                        ? hoverColor
                        : new Color32(
                            85, 85, 85, 255
                        );
            }

            if (answerTexts[i] != null)
            {
                answerTexts[i].color =
                    hover
                        ? hoverColor
                        : Color.white;
            }
        }
    }

    // ===================================
    // SUBMIT ANSWER
    // ===================================

    public void SubmitAnswer(int index)
    {
        if (submitted ||
            assessmentRoot == null ||
            !assessmentRoot.activeSelf ||
            index < 0 ||
            index >= answers.Length)
        {
            return;
        }

        selectedAnswer = index;

        submitted = true;

        // ===================================
        // NEW: ASSESS ANSWER INTERNALLY
        // ===================================

        correctAnswerSelected =
            selectedAnswer == correctAnswerIndex;

        // Record the answer before starting
        // the next inspection activity.

        SaveAssessmentResults();

        Debug.Log(
            "[TESTING] Selected answer: " +
            ((char)('A' + index)) +
            " - " +
            answers[index]
        );

        // No immediate correct or incorrect
        // feedback is displayed to the trainee.

        assessmentRoot.SetActive(false);

        // Preserve existing task progression.

        onAnswerSubmitted?.Invoke();
    }

    // ===================================
    // SAVE CENTRAL ASSESSMENT RESULTS
    // ===================================

    public void SaveAssessmentResults()
    {
        if (!submitted ||
            assessmentRecorded)
        {
            return;
        }

        TestingAssessmentManager manager =
            TestingAssessmentManager.Instance;

        if (manager == null ||
            !manager.SessionStarted ||
            manager.SessionCompleted)
        {
            Debug.LogWarning(
                "[ASSESSMENT] No active testing session. " +
                "Visual inspection answer retained locally."
            );

            return;
        }

        TestingAssessmentManager.TaskRecord task =
            manager.FindTask("INSPECTION_02");

        if (task == null)
        {
            Debug.LogWarning(
                "[ASSESSMENT] Task INSPECTION_02 " +
                "was not registered."
            );

            return;
        }

        if (task.completed)
        {
            Debug.LogWarning(
                "[ASSESSMENT] Task 2 was already recorded."
            );

            return;
        }

        string improvement =
            correctAnswerSelected

                ? ""

                : "Review the visual characteristics " +
                  "of corrosion near pipe supports. " +
                  "Distinguish visible corrosion from " +
                  "cracking, active leakage and " +
                  "undamaged pipeline surfaces.";

        // Record the actual submitted answer.

        manager.RecordAnswer(
            "INSPECTION_02",

            "What did you observe on PIPE_0493?",

            SubmittedAnswer,

            ExpectedAnswer,

            correctAnswerSelected,

            improvement
        );

        // Record task completion and accuracy.

        manager.RecordTaskResult(
            "INSPECTION_02",

            correctAnswerSelected ? 1 : 0,

            1,

            correctAnswerSelected ? 0 : 1,

            true,

            improvement
        );

        assessmentRecorded = true;

        Debug.Log(
            "[ASSESSMENT] Task 2 visual inspection recorded."
        );
    }

    // ===================================
    // RESET ASSESSMENT
    // ===================================

    public void ResetAssessment()
    {
        selectedAnswer = -1;

        submitted = false;

        correctAnswerSelected = false;

        assessmentRecorded = false;

        hoveredIndex = -1;

        // Clear this task in the central manager
        // if an active testing session exists.

        if (Application.isPlaying)
        {
            TestingAssessmentManager manager =
                TestingAssessmentManager.Instance;

            if (manager != null &&
                manager.SessionStarted &&
                !manager.SessionCompleted)
            {
                TestingAssessmentManager.TaskRecord task =
                    manager.FindTask("INSPECTION_02");

                if (task != null)
                {
                    task.answers.Clear();

                    task.mistakeDetails.Clear();

                    task.correctAnswers = 0;

                    task.totalAnswers = 1;

                    task.incorrectAttempts = 0;

                    task.completed = false;

                    task.improvement = "";
                }
            }
        }

        RefreshAnswers();

        if (assessmentRoot != null)
        {
            assessmentRoot.SetActive(false);
        }
    }
}
