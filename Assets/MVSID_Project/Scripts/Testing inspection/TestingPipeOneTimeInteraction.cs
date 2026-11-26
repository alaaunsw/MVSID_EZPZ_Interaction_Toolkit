
using UnityEngine;

public class TestingPipeOneTimeInteraction : MonoBehaviour
{
    [Header("Close-Up Inspection Model")]
    public GameObject inspectionModel;

    [Header("Supervisor Dialogue")]
    public TestingInspectionDialogue dialogue;

    [Header("Pipeline Identification")]
    public string pipelineID = "PIPE_0493";

    [Header("Interaction Status")]
    [SerializeField]
    private bool hasOpened = false;

    [SerializeField]
    private bool assessmentRecorded = false;

    [Header("Assessment Recording Settings")]
    [Min(0.1f)]
    public float retryInterval = 1f;

    private float nextRetryTime;

    public bool IsTaskCompleted
        => hasOpened;

    public bool IsAssessmentRecorded
        => assessmentRecorded;

    // ==========================================
    // INITIALISATION
    // ==========================================

    private void Awake()
    {
        hasOpened = false;

        assessmentRecorded = false;

        nextRetryTime = 0f;
    }

    // ==========================================
    // AUTOMATIC RECORDING RETRY
    // ==========================================

    private void Update()
    {
        if (!hasOpened || assessmentRecorded)
            return;

        if (Time.unscaledTime < nextRetryTime)
            return;

        nextRetryTime =
            Time.unscaledTime + retryInterval;

        TestingAssessmentManager manager =
            TestingAssessmentManager.Instance;

        if (manager == null ||
            !manager.SessionStarted ||
            manager.SessionCompleted)
        {
            return;
        }

        if (manager.FindTask("INSPECTION_01") == null)
            return;

        SaveAssessmentResults();
    }

    // ==========================================
    // OPEN PIPELINE INSPECTION
    // ==========================================

    public void OpenInspectionOnce()
    {
        // Do not reopen the inspection model.
        // However, allow assessment recording
        // to be retried if necessary.

        if (hasOpened)
        {
            if (!assessmentRecorded)
            {
                SaveAssessmentResults();
            }

            Debug.Log(
                "[TESTING] " +
                pipelineID +
                " inspection already opened."
            );

            return;
        }

        if (inspectionModel == null)
        {
            Debug.LogError(
                "[TESTING] Inspection Model is not assigned."
            );

            return;
        }

        hasOpened = true;

        // Open existing inspection model.

        inspectionModel.SetActive(true);

        // Record Task 1 immediately.

        SaveAssessmentResults();

        // Preserve existing dialogue interaction.

        if (dialogue != null)
        {
            dialogue.ShowRotationInstruction();
        }

        Debug.Log(
            "[TESTING] " +
            pipelineID +
            " close-up inspection opened."
        );
    }

    // ==========================================
    // SAVE CENTRAL ASSESSMENT RESULTS
    // ==========================================

    [ContextMenu("Save Assessment Results")]
    public void SaveAssessmentResults()
    {
        if (!Application.isPlaying)
            return;

        if (!hasOpened)
        {
            Debug.LogWarning(
                "[ASSESSMENT] Pipeline inspection " +
                "has not been opened."
            );

            return;
        }

        if (assessmentRecorded)
            return;

        TestingAssessmentManager manager =
            TestingAssessmentManager.Instance;

        // ======================================
        // VERIFY CENTRAL SESSION
        // ======================================

        if (manager == null)
        {
            Debug.LogWarning(
                "[ASSESSMENT] TestingAssessmentManager " +
                "was not found."
            );

            return;
        }

        if (!manager.SessionStarted)
        {
            Debug.LogWarning(
                "[ASSESSMENT] Testing session " +
                "has not started."
            );

            return;
        }

        if (manager.SessionCompleted)
        {
            Debug.LogWarning(
                "[ASSESSMENT] Testing session " +
                "is already completed."
            );

            return;
        }

        // ======================================
        // VERIFY TASK REGISTRATION
        // ======================================

        TestingAssessmentManager.TaskRecord task =
            manager.FindTask("INSPECTION_01");

        if (task == null)
        {
            Debug.LogWarning(
                "[ASSESSMENT] INSPECTION_01 " +
                "was not registered."
            );

            return;
        }

        // Avoid duplicate assessment records.

        if (task.completed)
        {
            assessmentRecorded = true;

            Debug.Log(
                "[ASSESSMENT] Task 1 already recorded."
            );

            return;
        }

        // ======================================
        // CHECK PIPELINE IDENTIFICATION
        // ======================================

        string submittedPipeline =
            pipelineID == null
                ? ""
                : pipelineID.Trim();

        bool isCorrect =
            submittedPipeline == "PIPE_0493";

        // ======================================
        // RECORD TRAINEE ANSWER
        // ======================================

        manager.RecordAnswer(
            "INSPECTION_01",

            "Locate and access the designated pipeline.",

            submittedPipeline,

            "PIPE_0493",

            isCorrect,

            isCorrect
                ? ""
                : "Review the pipeline identification " +
                  "information before beginning inspection."
        );

        // ======================================
        // RECORD TASK COMPLETION
        // ======================================

        manager.RecordTaskResult(
            "INSPECTION_01",

            isCorrect ? 1 : 0,

            1,

            isCorrect ? 0 : 1,

            true,

            isCorrect
                ? ""
                : "Review pipeline identification procedures."
        );

        // ======================================
        // VERIFY SUCCESSFUL RECORDING
        // ======================================

        task = manager.FindTask("INSPECTION_01");

        assessmentRecorded =
            task != null && task.completed;

        if (assessmentRecorded)
        {
            Debug.Log(
                "[ASSESSMENT] Task 1 successfully recorded: " +
                submittedPipeline
            );

            Debug.Log(
                "[ASSESSMENT] Completed tasks: " +
                manager.GetCompletedTaskCount() +
                " / " +
                manager.GetTotalTaskCount()
            );
        }
        else
        {
            Debug.LogWarning(
                "[ASSESSMENT] Task 1 recording failed. " +
                "The system will retry."
            );
        }
    }

    // ==========================================
    // INSPECT CURRENT RECORDING STATUS
    // ==========================================

    [ContextMenu("Check Task 1 Assessment Status")]
    public void CheckAssessmentStatus()
    {
        TestingAssessmentManager manager =
            TestingAssessmentManager.Instance;

        Debug.Log(
            "[TASK 1] Pipeline ID: " + pipelineID
        );

        Debug.Log(
            "[TASK 1] Has Opened: " + hasOpened
        );

        Debug.Log(
            "[TASK 1] Assessment Recorded: " +
            assessmentRecorded
        );

        if (manager == null)
        {
            Debug.LogWarning(
                "[TASK 1] Central manager not found."
            );

            return;
        }

        Debug.Log(
            "[TASK 1] Session Started: " +
            manager.SessionStarted
        );

        TestingAssessmentManager.TaskRecord task =
            manager.FindTask("INSPECTION_01");

        if (task == null)
        {
            Debug.LogWarning(
                "[TASK 1] Task registration missing."
            );

            return;
        }

        Debug.Log(
            "[TASK 1] Central task completed: " +
            task.completed
        );

        Debug.Log(
            "[TASK 1] Correct answers: " +
            task.correctAnswers +
            " / " +
            task.totalAnswers
        );
    }
}
