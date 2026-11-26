
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.InputSystem;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public class TestingCorrosionRiskVisualisation : MonoBehaviour
{
    [Header("Existing Systems")]
    public TestingInspectionDialogue dialogue;
    public TestingPressureReductionReport pressureReport;

    [Header("Camera")]
    public Camera playerCamera;
    public float distanceFromPlayer = 1.6f;

    [Header("Risk Visualisation Panel")]
    public GameObject riskPanel;
    public Button viewSimulationButton;

    [Header("Previous Panels")]
    public GameObject[] panelsToHide;

    [Header("Pipeline Visualisation")]
    public GameObject currentCorrosion;
    public GameObject severeCorrosion;

    [Header("Hypothetical Leak")]
    public ParticleSystem leakParticles;
    public AudioSource leakAudio;

    [Header("Stage Duration")]
    public float currentConditionDuration = 6f;
    public float deteriorationDuration = 6f;
    public float leakDuration = 6f;

    [Header("Assessment Results")]
    [SerializeField] private bool taskStarted;
    [SerializeField] private bool simulationRunning;
    [SerializeField] private bool taskCompleted;

    [SerializeField]
    private bool currentConditionCompleted;

    [SerializeField]
    private bool deteriorationCompleted;

    [SerializeField]
    private bool leakDemonstrationCompleted;

    [SerializeField]
    private bool assessmentRecorded;

    [SerializeField]
    private int completedStages;

    [SerializeField]
    private float simulationDuration;

    [Header("After Visualisation")]
    public UnityEvent onVisualisationCompleted;

    private Coroutine simulationCoroutine;

    private bool panelOpen;
    private bool cursorStateCaptured;
    private bool previousCursorVisible;
    private CursorLockMode previousCursorLock;

    private bool originalCorrosionState;
    private bool originalStateCaptured;

    private float simulationStartTime;

    // =====================================
    // PUBLIC ASSESSMENT RESULTS
    // =====================================

    public bool IsTaskCompleted
        => taskCompleted;

    public bool IsAssessmentRecorded
        => assessmentRecorded;

    public bool IsSimulationRunning
        => simulationRunning;

    public bool IsCurrentConditionCompleted
        => currentConditionCompleted;

    public bool IsDeteriorationCompleted
        => deteriorationCompleted;

    public bool IsLeakDemonstrationCompleted
        => leakDemonstrationCompleted;

    public int CompletedStages
        => completedStages;

    public int TotalStages => 3;

    public float SimulationDuration
        => simulationDuration;

    // =====================================
    // BUILD PANEL
    // =====================================

    [ContextMenu("Build Risk Visualisation Panel")]
    public void BuildPanel()
    {
        if (Application.isPlaying)
        {
            Debug.LogWarning(
                "Stop Play Mode before building."
            );
            return;
        }

#if UNITY_EDITOR
        Undo.RecordObject(
            this,
            "Build Risk Visualisation Panel"
        );

        if (riskPanel != null)
        {
            Undo.DestroyObjectImmediate(riskPanel);
        }
#endif

        riskPanel = new GameObject(
            "Testing_CorrosionRiskVisualisation_Form",
            typeof(RectTransform),
            typeof(Canvas),
            typeof(GraphicRaycaster)
        );

#if UNITY_EDITOR
        Undo.RegisterCreatedObjectUndo(
            riskPanel,
            "Build Risk Visualisation Panel"
        );
#endif

        Canvas canvas =
            riskPanel.GetComponent<Canvas>();

        canvas.renderMode = RenderMode.WorldSpace;
        canvas.worldCamera = playerCamera;
        canvas.overrideSorting = true;
        canvas.sortingOrder = 180;

        RectTransform root =
            riskPanel.GetComponent<RectTransform>();

        root.sizeDelta = new Vector2(950, 620);
        root.localScale = Vector3.one * 0.0017f;

        Color yellow =
            new Color32(255, 213, 79, 255);

        RectTransform background = CreateRect(
            "MainPanel",
            root,
            Vector2.zero,
            new Vector2(950, 620)
        );

        AddImage(
            background,
            new Color32(10, 10, 10, 235)
        );

        CreateText(
            "Title",
            background,
            "POTENTIAL CONSEQUENCES OF CORROSION",
            new Vector2(0, 245),
            new Vector2(900, 65),
            35,
            Color.white
        );

        CreateText(
            "TaskTitle",
            background,
            "TASK 08 - RISK VISUALISATION",
            new Vector2(0, 185),
            new Vector2(850, 50),
            29,
            yellow
        );

        CreateText(
            "Instructions",
            background,
            "Continued corrosion may cause further " +
            "wall thinning and eventually a loss " +
            "of containment.",
            new Vector2(0, 105),
            new Vector2(850, 100),
            27,
            Color.white
        );

        // THREE VISUALISATION STAGES

        CreateStage(
            background,
            "CurrentCorrosion",
            "CURRENT\nCORROSION",
            new Vector2(-300, -10)
        );

        CreateText(
            "Arrow1",
            background,
            "→",
            new Vector2(-150, -10),
            new Vector2(60, 80),
            40,
            yellow
        );

        CreateStage(
            background,
            "FurtherThinning",
            "FURTHER\nTHINNING",
            new Vector2(0, -10)
        );

        CreateText(
            "Arrow2",
            background,
            "→",
            new Vector2(150, -10),
            new Vector2(60, 80),
            40,
            yellow
        );

        CreateStage(
            background,
            "PossibleLeak",
            "POSSIBLE\nLEAK",
            new Vector2(300, -10)
        );

        CreateText(
            "Disclaimer",
            background,
            "Hypothetical demonstration only.\n" +
            "The inspected pipeline has no active leak.",
            new Vector2(0, -120),
            new Vector2(850, 85),
            24,
            new Color32(180, 180, 180, 255)
        );

        viewSimulationButton = CreateButton(
            background,
            new Vector2(0, -240)
        );

        if (dialogue == null)
        {
            dialogue =
                GetComponent<TestingInspectionDialogue>();
        }

        if (pressureReport == null)
        {
            pressureReport =
                GetComponent<TestingPressureReductionReport>();
        }

        PositionPanel();

        riskPanel.SetActive(true);

#if UNITY_EDITOR
        EditorUtility.SetDirty(this);

        EditorSceneManager.MarkSceneDirty(
            gameObject.scene
        );
#endif

        Debug.Log(
            "[TESTING] Task 8 risk panel built."
        );
    }

    // =====================================
    // UI RECT HELPER
    // =====================================

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

        rect.anchorMin = Vector2.one * 0.5f;
        rect.anchorMax = Vector2.one * 0.5f;
        rect.pivot = Vector2.one * 0.5f;
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        rect.localScale = Vector3.one;
        rect.localRotation = Quaternion.identity;

        return rect;
    }

    private Image AddImage(
        RectTransform rect,
        Color colour,
        bool raycast = false)
    {
        Image image =
            rect.gameObject.AddComponent<Image>();

        image.color = colour;
        image.raycastTarget = raycast;

        return image;
    }

    private TextMeshProUGUI CreateText(
        string objectName,
        Transform parent,
        string content,
        Vector2 position,
        Vector2 size,
        float fontSize,
        Color colour)
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
        text.color = colour;

        text.alignment =
            TextAlignmentOptions.Center;

        text.raycastTarget = false;

        return text;
    }

    private void CreateStage(
        Transform parent,
        string objectName,
        string content,
        Vector2 position)
    {
        RectTransform stage = CreateRect(
            objectName,
            parent,
            position,
            new Vector2(245, 100)
        );

        AddImage(
            stage,
            new Color32(50, 50, 50, 255)
        );

        CreateText(
            "StageText",
            stage,
            content,
            Vector2.zero,
            new Vector2(230, 90),
            26,
            Color.white
        );
    }

    // =====================================
    // CREATE SIMULATION BUTTON
    // =====================================

    private Button CreateButton(
        Transform parent,
        Vector2 position)
    {
        RectTransform rect = CreateRect(
            "ViewSimulationButton",
            parent,
            position,
            new Vector2(460, 75)
        );

        Image image = AddImage(
            rect,
            new Color32(65, 65, 65, 255),
            true
        );

        Button button =
            rect.gameObject.AddComponent<Button>();

        button.targetGraphic = image;

        ColorBlock colours = button.colors;

        colours.normalColor = Color.white;

        colours.highlightedColor =
            new Color32(255, 213, 79, 255);

        colours.pressedColor =
            new Color32(255, 235, 155, 255);

        button.colors = colours;

        CreateText(
            "ButtonText",
            rect,
            "VIEW SIMULATION",
            Vector2.zero,
            new Vector2(445, 65),
            30,
            Color.white
        );

        BoxCollider box =
            rect.gameObject.AddComponent<BoxCollider>();

        box.size = new Vector3(460, 75, 8);

        // Support EZPZ controller interaction.

        rect.gameObject.AddComponent<InteractableGeneral>();

        return button;
    }

    // =====================================
    // RUNTIME SETUP
    // =====================================

    private void Awake()
    {
        taskStarted = false;
        simulationRunning = false;
        taskCompleted = false;

        currentConditionCompleted = false;
        deteriorationCompleted = false;
        leakDemonstrationCompleted = false;

        assessmentRecorded = false;
        completedStages = 0;
        simulationDuration = 0f;

        if (riskPanel != null)
        {
            riskPanel.SetActive(false);
        }

        // Original corrosion visible.
        // Severe corrosion hidden initially.

        if (currentCorrosion != null)
        {
            currentCorrosion.SetActive(true);
        }

        if (severeCorrosion != null)
        {
            severeCorrosion.SetActive(false);
        }

        // Prevent leak particles from playing
        // before the hypothetical demonstration.

        if (leakParticles != null)
        {
            var main = leakParticles.main;
            main.playOnAwake = false;

            leakParticles.Stop(
                true,
                ParticleSystemStopBehavior
                    .StopEmittingAndClear
            );
        }

        // Prevent leak audio from playing early.

        if (leakAudio != null)
        {
            leakAudio.playOnAwake = false;
            leakAudio.Stop();
        }

        if (viewSimulationButton != null)
        {
            viewSimulationButton.onClick.AddListener(
                PlayRiskSimulation
            );

            InteractableGeneral ezpz =
                viewSimulationButton.GetComponent<
                    InteractableGeneral
                >();

            if (ezpz != null)
            {
                ezpz.onPrimaryInteract.AddListener(
                    PlayRiskSimulation
                );
            }
        }
    }

    // =====================================
    // POSITION PANEL
    // =====================================

    private void PositionPanel()
    {
        if (riskPanel == null)
            return;

        if (playerCamera == null)
        {
            playerCamera = Camera.main;
        }

        if (playerCamera == null)
            return;

        Transform cam = playerCamera.transform;

        riskPanel.transform.position =
            cam.position +
            cam.forward * distanceFromPlayer;

        riskPanel.transform.rotation =
            cam.rotation;

        Canvas canvas =
            riskPanel.GetComponent<Canvas>();

        if (canvas != null)
        {
            canvas.worldCamera = playerCamera;
        }
    }

    // =====================================
    // START TASK 8
    // =====================================

    public void StartRiskVisualisation()
    {
        if (taskStarted || taskCompleted)
            return;

        if (pressureReport == null ||
            !pressureReport.IsTaskCompleted)
        {
            Debug.LogWarning(
                "[TESTING] Complete Task 7 first."
            );
            return;
        }

        if (riskPanel == null)
        {
            Debug.LogError(
                "[TESTING] Build Task 8 panel first."
            );
            return;
        }

        taskStarted = true;

        if (panelsToHide != null)
        {
            foreach (GameObject panel in panelsToHide)
            {
                if (panel == null ||
                    panel == riskPanel ||
                    panel == gameObject)
                {
                    continue;
                }

                if (transform.IsChildOf(panel.transform) ||
                    riskPanel.transform.IsChildOf(
                        panel.transform))
                {
                    continue;
                }

                panel.SetActive(false);
            }
        }

        PositionPanel();

        previousCursorVisible = Cursor.visible;
        previousCursorLock = Cursor.lockState;
        cursorStateCaptured = true;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        panelOpen = true;
        riskPanel.SetActive(true);

        Debug.Log(
            "[TESTING] Task 8 started."
        );
    }

    // =====================================
    // DESKTOP MOUSE SUPPORT
    // =====================================

    private void Update()
    {
        if (!panelOpen ||
            playerCamera == null ||
            Mouse.current == null ||
            viewSimulationButton == null)
        {
            return;
        }

        if (!Mouse.current.leftButton.wasPressedThisFrame)
            return;

        Ray ray =
            playerCamera.ScreenPointToRay(
                Mouse.current.position.ReadValue()
            );

        BoxCollider box =
            viewSimulationButton.GetComponent<BoxCollider>();

        if (box != null &&
            box.Raycast(
                ray,
                out RaycastHit hit,
                100f))
        {
            PlayRiskSimulation();
        }
    }

    // =====================================
    // PLAY SIMULATION
    // =====================================

    public void PlayRiskSimulation()
    {
        if (!taskStarted ||
            !panelOpen ||
            simulationRunning ||
            taskCompleted)
        {
            return;
        }

        if (currentCorrosion == null ||
            severeCorrosion == null ||
            leakParticles == null)
        {
            Debug.LogWarning(
                "[TESTING] Assign Current Corrosion, " +
                "Severe Corrosion and Leak Particles."
            );
            return;
        }

        if (currentCorrosion == severeCorrosion)
        {
            Debug.LogError(
                "[TESTING] Current and Severe Corrosion " +
                "must be different GameObjects."
            );
            return;
        }

        if (!leakParticles.gameObject.activeInHierarchy)
        {
            Debug.LogWarning(
                "[TESTING] Enable the Leak Particles GameObject."
            );
            return;
        }

        if (leakAudio == null)
        {
            Debug.LogWarning(
                "[TESTING] Leak Audio is not assigned. " +
                "Simulation will continue without sound."
            );
        }
        else if (!leakAudio.isActiveAndEnabled)
        {
            Debug.LogWarning(
                "[TESTING] Leak Audio is disabled."
            );
        }

        simulationRunning = true;

        completedStages = 0;
        currentConditionCompleted = false;
        deteriorationCompleted = false;
        leakDemonstrationCompleted = false;

        simulationDuration = 0f;

        simulationStartTime =
            Time.realtimeSinceStartup;

        // Save original corrosion visibility.

        originalCorrosionState =
            currentCorrosion.activeSelf;

        originalStateCaptured = true;

        // Hide the introduction panel.

        ClosePanel();

        simulationCoroutine =
            StartCoroutine(RiskSequence());
    }

    // =====================================
    // THREE-STAGE RISK VISUALISATION
    // =====================================

    private IEnumerator RiskSequence()
    {
        // =====================================
        // STAGE 1 - CURRENT CORROSION
        // =====================================

        currentCorrosion.SetActive(true);
        severeCorrosion.SetActive(false);

        StopLeakEffects();

        if (dialogue != null)
        {
            dialogue.ShowMessage(
                "INSPECTION ENGINEER",

                "Observe the existing corrosion around " +
                "the pipeline support. Continued corrosion " +
                "may cause further material loss."
            );
        }

        Debug.Log(
            "[TESTING] Stage 1: Current corrosion visible."
        );

        yield return new WaitForSeconds(
            Mathf.Max(
                0.1f,
                currentConditionDuration
            )
        );

        currentConditionCompleted = true;

        UpdateCompletedStages();

        // =====================================
        // STAGE 2 - SEVERE DETERIORATION
        // =====================================

        // IMPORTANT NEW CHANGE:
        // Hide the original rust decal before
        // displaying the severe corrosion decal.

        currentCorrosion.SetActive(false);

        severeCorrosion.SetActive(true);

        // The dark holes are now visible.
        // No vapour or leak audio yet.

        StopLeakEffects();

        if (dialogue != null)
        {
            dialogue.ShowMessage(
                "INSPECTION ENGINEER",

                "This hypothetical deterioration illustrates " +
                "how continued corrosion may cause further " +
                "wall thinning, deep pitting and possible " +
                "perforation of the pipeline."
            );
        }

        Debug.Log(
            "[TESTING] Stage 2: Severe corrosion visible."
        );

        yield return new WaitForSeconds(
            Mathf.Max(
                0.1f,
                deteriorationDuration
            )
        );

        deteriorationCompleted = true;

        UpdateCompletedStages();

        // =====================================
        // STAGE 3 - HYPOTHETICAL LEAK
        // =====================================

        // Keep severe corrosion and dark holes visible.

        currentCorrosion.SetActive(false);
        severeCorrosion.SetActive(true);

        // Activate the simulated vapour.

        if (leakParticles != null)
        {
            leakParticles.Clear(true);
            leakParticles.Play(true);
        }

        // Play the leak sound.

        if (leakAudio != null &&
            leakAudio.isActiveAndEnabled)
        {
            leakAudio.Play();
        }

        if (dialogue != null)
        {
            dialogue.ShowMessage(
                "INSPECTION ENGINEER",

                "This hypothetical leak demonstrates " +
                "a possible loss of containment resulting " +
                "from continued pipeline deterioration. " +
                "Timely inspection and engineering " +
                "intervention are important."
            );
        }

        Debug.Log(
            "[TESTING] Stage 3: Hypothetical leak activated."
        );

        yield return new WaitForSeconds(
            Mathf.Max(
                0.1f,
                leakDuration
            )
        );

        leakDemonstrationCompleted = true;

        UpdateCompletedStages();

        // =====================================
        // RESTORE ORIGINAL PIPELINE
        // =====================================

        RestorePipeline();

        simulationDuration =
            Time.realtimeSinceStartup -
            simulationStartTime;

        simulationRunning = false;

        taskCompleted =
            completedStages == TotalStages;

        simulationCoroutine = null;

        // =====================================
        // SAVE ASSESSMENT RESULTS
        // =====================================

        SaveAssessmentResults();

        Debug.Log(
            "[TESTING] Task 8 visualisation completed."
        );

        // =====================================
        // FINAL ENGINEER MESSAGE
        // =====================================

        if (dialogue != null)
        {
            dialogue.ShowMessage(
                "INSPECTION ENGINEER",

                "The hypothetical demonstration is complete. " +
                "The original pipeline condition has been restored. " +
                "No active leak is present in the inspected " +
                "pipeline. Proceed to your final inspection report."
            );
        }

        // Automatically activate Task 9.

        onVisualisationCompleted?.Invoke();
    }

    // =====================================
    // COUNT COMPLETED STAGES
    // =====================================

    private void UpdateCompletedStages()
    {
        completedStages = 0;

        if (currentConditionCompleted)
            completedStages++;

        if (deteriorationCompleted)
            completedStages++;

        if (leakDemonstrationCompleted)
            completedStages++;
    }

    // =====================================
    // CENTRAL ASSESSMENT RECORDING
    // =====================================

    public void SaveAssessmentResults()
    {
        if (!taskCompleted ||
            assessmentRecorded ||
            completedStages != TotalStages)
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
                "Visualisation results retained locally."
            );
            return;
        }

        TestingAssessmentManager.TaskRecord task =
            manager.FindTask("INSPECTION_08");

        if (task == null)
        {
            Debug.LogWarning(
                "[ASSESSMENT] Task INSPECTION_08 " +
                "was not registered."
            );
            return;
        }

        if (task.completed)
        {
            assessmentRecorded = true;
            return;
        }

        // Completion-only activity.
        // Excluded from overall accuracy scoring.

        task.contributesToScore = false;

        manager.RecordAnswer(
            "INSPECTION_08",

            "Complete the current corrosion demonstration.",

            "Current corrosion demonstration completed.",

            "Complete stage 1.",

            currentConditionCompleted,

            ""
        );

        manager.RecordAnswer(
            "INSPECTION_08",

            "Complete the hypothetical deterioration demonstration.",

            "Further material loss demonstration completed.",

            "Complete stage 2.",

            deteriorationCompleted,

            ""
        );

        manager.RecordAnswer(
            "INSPECTION_08",

            "Complete the hypothetical loss of containment demonstration.",

            "Hypothetical leak demonstration completed. " +
            "Original pipeline condition restored.",

            "Complete stage 3.",

            leakDemonstrationCompleted,

            ""
        );

        manager.RecordTaskResult(
            "INSPECTION_08",

            completedStages,

            TotalStages,

            0,

            true,

            ""
        );

        assessmentRecorded = true;

        Debug.Log(
            "[ASSESSMENT] Task 8 recorded: " +
            completedStages + " / " +
            TotalStages + " stages completed."
        );

        Debug.Log(
            "[ASSESSMENT] Visualisation duration: " +
            simulationDuration.ToString("F1") +
            " seconds."
        );
    }

    // =====================================
    // STOP LEAK EFFECTS
    // =====================================

    private void StopLeakEffects()
    {
        if (leakParticles != null)
        {
            leakParticles.Stop(
                true,
                ParticleSystemStopBehavior
                    .StopEmittingAndClear
            );
        }

        if (leakAudio != null)
        {
            leakAudio.Stop();
        }
    }

    // =====================================
    // RESTORE ORIGINAL PIPELINE CONDITION
    // =====================================

    private void RestorePipeline()
    {
        StopLeakEffects();

        if (severeCorrosion != null)
        {
            severeCorrosion.SetActive(false);
        }

        if (currentCorrosion != null &&
            originalStateCaptured)
        {
            currentCorrosion.SetActive(
                originalCorrosionState
            );
        }

        originalStateCaptured = false;

        Debug.Log(
            "[TESTING] Original pipeline condition restored."
        );
    }

    // =====================================
    // CLOSE PANEL
    // =====================================

    private void ClosePanel()
    {
        panelOpen = false;

        if (riskPanel != null &&
            riskPanel.activeSelf)
        {
            riskPanel.SetActive(false);
        }

        if (cursorStateCaptured)
        {
            Cursor.lockState = previousCursorLock;
            Cursor.visible = previousCursorVisible;

            cursorStateCaptured = false;
        }
    }

    // =====================================
    // CLEANUP
    // =====================================

    private void OnDisable()
    {
        if (simulationCoroutine != null)
        {
            StopCoroutine(simulationCoroutine);

            simulationCoroutine = null;
        }

        if (!taskCompleted)
        {
            taskStarted = false;
        }

        simulationRunning = false;

        RestorePipeline();
        ClosePanel();
    }

    // =====================================
    // RESET TASK
    // =====================================

    public void ResetTask()
    {
        if (simulationCoroutine != null)
        {
            StopCoroutine(simulationCoroutine);
            simulationCoroutine = null;
        }

        taskStarted = false;
        simulationRunning = false;
        taskCompleted = false;

        currentConditionCompleted = false;
        deteriorationCompleted = false;
        leakDemonstrationCompleted = false;

        assessmentRecorded = false;

        completedStages = 0;
        simulationDuration = 0f;
        simulationStartTime = 0f;

        RestorePipeline();
        ClosePanel();

        if (Application.isPlaying)
        {
            TestingAssessmentManager manager =
                TestingAssessmentManager.Instance;

            if (manager != null &&
                manager.SessionStarted &&
                !manager.SessionCompleted)
            {
                TestingAssessmentManager.TaskRecord task =
                    manager.FindTask("INSPECTION_08");

                if (task != null)
                {
                    task.answers.Clear();
                    task.mistakeDetails.Clear();

                    task.correctAnswers = 0;
                    task.totalAnswers = TotalStages;
                    task.incorrectAttempts = 0;

                    task.completed = false;
                    task.contributesToScore = false;
                    task.improvement = "";
                }
            }
        }

        Debug.Log(
            "[ASSESSMENT] Risk visualisation reset."
        );
    }
}
