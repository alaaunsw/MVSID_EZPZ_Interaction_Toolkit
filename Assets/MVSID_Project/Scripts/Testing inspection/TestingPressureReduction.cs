
using System.Collections;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class TestingPressureReduction : MonoBehaviour
{
    [Header("Existing Systems")]
    public TestingInspectionDialogue dialogue;
    public TestingPipelineMeasurements measurements;

    [Header("Red Valve")]
    public Transform valveWheel;

    public Vector3 valveRotationAxis =
        Vector3.forward;

    public float valveTurnDegrees = 180f;

    [Header("Existing Pressure Gauge")]
    public Animator pressureGaugeAnimator;

    public Transform pressureNeedle;

    public Vector3 needleRotationAxis =
        Vector3.forward;

    public float needleReductionDegrees = -18f;

    [Header("Pressure Display")]
    public GameObject pressureDisplay;
    public TMP_Text pressureText;

    [Header("Simulated Vent Effect")]
    public ParticleSystem ventParticles;
    public AudioSource ventAudio;

    [Header("Animation Settings")]
    public float reductionDuration = 3f;

    [Header("Results")]
    public float initialPressure = 150f;
    public float targetPressure = 120f;

    // ======================================
    // NEW: ASSESSMENT SETTINGS
    // ======================================

    [Header("Assessment Settings")]

    public float expectedFinalPressure = 120f;

    [Min(0f)]
    public float pressureTolerance = 0.01f;

    // ======================================
    // EXISTING TASK PROGRESSION
    // ======================================

    [Header("After Reduction")]
    public UnityEvent onReductionCompleted;

    // ======================================
    // RECORDED ASSESSMENT RESULTS
    // ======================================

    [Header("Assessment Status")]

    [SerializeField]
    private bool taskStarted;

    [SerializeField]
    private bool reducingPressure;

    [SerializeField]
    private bool reductionCompleted;

    [SerializeField]
    private bool valveOperated;

    [SerializeField]
    private float achievedPressure;

    [SerializeField]
    private bool operationSuccessful;

    [SerializeField]
    private bool assessmentRecorded;

    private Quaternion initialValveRotation;

    private Quaternion initialNeedleRotation;

    private Coroutine activeReduction;

    // ======================================
    // PUBLIC ASSESSMENT RESULTS
    // ======================================

    public bool IsReductionCompleted
        => reductionCompleted;

    public bool IsTaskCompleted
        => reductionCompleted;

    public bool IsValveOperated
        => valveOperated;

    public float AchievedPressure
        => achievedPressure;

    public bool IsOperationSuccessful
        => reductionCompleted &&
           operationSuccessful;

    public bool IsAssessmentRecorded
        => assessmentRecorded;

    // ======================================
    // INITIALISATION
    // ======================================

    private void Start()
    {
        taskStarted = false;

        reducingPressure = false;

        reductionCompleted = false;

        valveOperated = false;

        operationSuccessful = false;

        assessmentRecorded = false;

        achievedPressure = initialPressure;

        if (valveWheel != null)
        {
            initialValveRotation =
                valveWheel.localRotation;
        }

        if (ventParticles != null)
        {
            ventParticles.Stop(
                true,
                ParticleSystemStopBehavior
                    .StopEmittingAndClear
            );
        }

        if (ventAudio != null)
        {
            ventAudio.playOnAwake = false;

            ventAudio.Stop();
        }
    }

    // ======================================
    // START TASK 6
    // ======================================

    // Called after Task 5 submission.

    public void StartPressureReductionTask()
    {
        if (taskStarted)
            return;

        taskStarted = true;

        reducingPressure = false;

        reductionCompleted = false;

        valveOperated = false;

        operationSuccessful = false;

        assessmentRecorded = false;

        achievedPressure = initialPressure;

        if (pressureText != null)
        {
            pressureText.text =
                initialPressure + " psi";
        }

        if (pressureDisplay != null)
        {
            pressureDisplay.SetActive(true);
        }

        // Preserve existing engineer instructions.

        if (dialogue != null)
        {
            dialogue.ShowMessage(
                "INSPECTION ENGINEER",

                "Your instrument readings have been recorded. " +
                "Now operate the highlighted pressure-control " +
                "valve to simulate reducing the pipeline " +
                "pressure to 120 psi. Observe the pressure gauge."
            );
        }

        Debug.Log(
            "[TESTING] Task 6: Pressure reduction started."
        );
    }

    // ======================================
    // OPERATE THE RED VALVE
    // ======================================

    // Connected to the red valve's
    // EZPZ On Primary Interact().

    public void OperateValve()
    {
        if (!taskStarted ||
            reducingPressure ||
            reductionCompleted)
        {
            return;
        }

        if (valveWheel == null ||
            pressureNeedle == null)
        {
            Debug.LogError(
                "[TESTING] Valve Wheel or Pressure Needle " +
                "is not assigned."
            );

            return;
        }

        // Record actual valve interaction.

        valveOperated = true;

        activeReduction =
            StartCoroutine(
                AnimatePressureReduction()
            );
    }

    // ======================================
    // EXISTING PRESSURE REDUCTION ANIMATION
    // ======================================

    private IEnumerator AnimatePressureReduction()
    {
        reducingPressure = true;

        // Stop the existing Animator from
        // overriding our needle rotation.

        if (pressureGaugeAnimator != null)
        {
            pressureGaugeAnimator.enabled = false;
        }

        initialValveRotation =
            valveWheel.localRotation;

        initialNeedleRotation =
            pressureNeedle.localRotation;

        // Existing simulated gas release.

        if (ventParticles != null)
        {
            ventParticles.Play();
        }

        // Existing vent audio.

        if (ventAudio != null)
        {
            ventAudio.Play();
        }

        float elapsed = 0f;

        float duration =
            Mathf.Max(0.1f, reductionDuration);

        Vector3 valveAxis =
            valveRotationAxis.sqrMagnitude > 0.001f

                ? valveRotationAxis.normalized

                : Vector3.forward;

        Vector3 needleAxis =
            needleRotationAxis.sqrMagnitude > 0.001f

                ? needleRotationAxis.normalized

                : Vector3.forward;

        Quaternion valveEnd =
            initialValveRotation *
            Quaternion.AngleAxis(
                valveTurnDegrees,
                valveAxis
            );

        Quaternion needleEnd =
            initialNeedleRotation *
            Quaternion.AngleAxis(
                needleReductionDegrees,
                needleAxis
            );

        // ======================================
        // EXISTING VALVE AND GAUGE ANIMATION
        // ======================================

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float t =
                Mathf.Clamp01(
                    elapsed / duration
                );

            float smooth =
                Mathf.SmoothStep(
                    0f,
                    1f,
                    t
                );

            // Rotate the valve wheel.

            valveWheel.localRotation =
                Quaternion.Slerp(
                    initialValveRotation,
                    valveEnd,
                    smooth
                );

            // Move the pressure gauge needle.

            pressureNeedle.localRotation =
                Quaternion.Slerp(
                    initialNeedleRotation,
                    needleEnd,
                    smooth
                );

            // Update the simulated pressure.

            float currentPressure =
                Mathf.Lerp(
                    initialPressure,
                    targetPressure,
                    smooth
                );

            achievedPressure = currentPressure;

            if (pressureText != null)
            {
                pressureText.text =
                    currentPressure.ToString("0") +
                    " psi";
            }

            yield return null;
        }

        // ======================================
        // FINAL ANIMATION STATE
        // ======================================

        valveWheel.localRotation = valveEnd;

        pressureNeedle.localRotation = needleEnd;

        achievedPressure = targetPressure;

        if (pressureText != null)
        {
            pressureText.text =
                targetPressure + " psi";
        }

        // Stop simulated gas release.

        if (ventParticles != null)
        {
            ventParticles.Stop(
                true,

                ParticleSystemStopBehavior
                    .StopEmitting
            );
        }

        if (ventAudio != null)
        {
            ventAudio.Stop();
        }

        reducingPressure = false;

        reductionCompleted = true;

        activeReduction = null;

        // ======================================
        // NEW: EVALUATE COMPLETED OPERATION
        // ======================================

        EvaluateOperation();

        // Record Task 6 before activating Task 7.

        SaveAssessmentResults();

        Debug.Log(
            "[TESTING] Task 6 completed. Pressure reduced to " +
            achievedPressure +
            " psi."
        );

        // ======================================
        // EXISTING ENGINEER INSTRUCTIONS
        // ======================================

        // Request the updated pressure reading.
        // No immediate assessment feedback.

        if (dialogue != null)
        {
            dialogue.ShowMessage(
                "INSPECTION ENGINEER",

                "The pressure-control operation is complete. " +
                "Observe the updated pressure gauge and " +
                "submit your current pressure reading."
            );
        }

        // Activate Task 7.

        onReductionCompleted?.Invoke();
    }

    // ======================================
    // NEW: EVALUATE SIMULATED OPERATION
    // ======================================

    private void EvaluateOperation()
    {
        const float precisionAllowance = 0.00001f;

        bool correctFinalPressure =
            Mathf.Abs(
                achievedPressure -
                expectedFinalPressure
            )
            <= pressureTolerance +
               precisionAllowance;

        operationSuccessful =
            valveOperated &&
            reductionCompleted &&
            correctFinalPressure;
    }

    // ======================================
    // NEW: SAVE CENTRAL ASSESSMENT RESULTS
    // ======================================

    public void SaveAssessmentResults()
    {
        if (!reductionCompleted ||
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
                "Pressure reduction results retained locally."
            );

            return;
        }

        TestingAssessmentManager.TaskRecord task =
            manager.FindTask("INSPECTION_06");

        if (task == null)
        {
            Debug.LogWarning(
                "[ASSESSMENT] Task INSPECTION_06 " +
                "was not registered."
            );

            return;
        }

        if (task.completed)
        {
            Debug.LogWarning(
                "[ASSESSMENT] Task 6 already recorded."
            );

            return;
        }

        // ======================================
        // RECORD ACTUAL SIMULATION OUTCOME
        // ======================================

        string traineeResult =
            "Valve operated: " +
            (valveOperated ? "Yes" : "No") +
            "\nInitial pressure: " +
            FormatNumber(initialPressure) +
            " psi" +
            "\nFinal simulated pressure: " +
            FormatNumber(achievedPressure) +
            " psi";

        string expectedResult =
            "Operate the designated pressure-control " +
            "valve and complete the simulated reduction " +
            "to " +
            FormatNumber(expectedFinalPressure) +
            " psi.";

        string improvement =
            operationSuccessful

                ? ""

                : "Review the assigned valve operation " +
                  "and the target pressure in the " +
                  "simulated inspection procedure.";

        manager.RecordAnswer(
            "INSPECTION_06",

            "Complete the simulated pressure " +
            "reduction operation.",

            traineeResult,

            expectedResult,

            operationSuccessful,

            improvement
        );

        // ======================================
        // RECORD TASK COMPLETION
        // ======================================

        task.contributesToScore = true;

        manager.RecordTaskResult(
            "INSPECTION_06",

            operationSuccessful ? 1 : 0,

            1,

            operationSuccessful ? 0 : 1,

            true,

            improvement
        );

        assessmentRecorded = true;

        Debug.Log(
            "[ASSESSMENT] Task 6 pressure " +
            "reduction operation recorded."
        );
    }

    // ======================================
    // FORMAT NUMERICAL RESULTS
    // ======================================

    private string FormatNumber(float value)
    {
        return value.ToString(
            "0.##",
            CultureInfo.InvariantCulture
        );
    }

    // ======================================
    // EXISTING CLEANUP
    // ======================================

    private void OnDisable()
    {
        if (activeReduction != null)
        {
            StopCoroutine(activeReduction);

            activeReduction = null;
        }

        if (ventParticles != null)
        {
            ventParticles.Stop(
                true,

                ParticleSystemStopBehavior
                    .StopEmittingAndClear
            );
        }

        if (ventAudio != null)
        {
            ventAudio.Stop();
        }
    }
}
