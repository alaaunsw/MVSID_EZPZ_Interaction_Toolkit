
using UnityEngine;
using UnityEngine.Events;

public class PipelineInspectionFlowGate : MonoBehaviour
{
    [Header("Player")]
    public Transform player;

    [Header("Welcome")]
    public GameObject welcomePanel;

    [Header("Inspection Zone")]
    public Collider inspectionZone;

    [Header("Inspection Buttons")]
    public GameObject inspectPipeButton;
    public GameObject finishInspectionButton;

    [Header("Existing Rotation Inspection")]
    public GameObject pipeRotationPanel;

    [Header("Training Panels")]
    public GameObject trainingProgressPanel;
    public GameObject materialLossTrainingPanel;

    [Header("Events")]
    public UnityEvent onInspectionStarted;
    public UnityEvent onInspectionCompleted;
    public UnityEvent onTrainingStarted;

    private bool welcomeCompleted;
    private bool insideZone;
    private bool inspectionStarted;
    private bool inspectionCompleted;

    private void Start()
    {
        // Initial activity state
        welcomeCompleted = false;
        insideZone = false;
        inspectionStarted = false;
        inspectionCompleted = false;

        SetVisible(inspectPipeButton, false);
        SetVisible(finishInspectionButton, false);
        SetVisible(pipeRotationPanel, false);

        SetVisible(trainingProgressPanel, false);
        SetVisible(materialLossTrainingPanel, false);
    }

    // ======================================
    // WELCOME PANEL NEXT BUTTON
    // ======================================

    public void ContinueFromWelcome()
    {
        welcomeCompleted = true;

        SetVisible(welcomePanel, false);

        Debug.Log(
            "Welcome completed. Approach the pipe zone."
        );

        CheckPlayerInsideZone();
        UpdateInspectionButtons();
    }

    // ======================================
    // ZONE DETECTION
    // ======================================

    private void OnTriggerEnter(Collider other)
    {
        if (!IsPlayer(other))
            return;

        insideZone = true;
        UpdateInspectionButtons();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!IsPlayer(other))
            return;

        insideZone = false;
        UpdateInspectionButtons();
    }

    private bool IsPlayer(Collider other)
    {
        if (player != null)
        {
            return other.transform == player ||
                   other.transform.IsChildOf(player);
        }

        return other.CompareTag("Player") ||
               other.transform.root.CompareTag("Player");
    }

    private void CheckPlayerInsideZone()
    {
        if (player == null || inspectionZone == null)
            return;

        // Allows the trainee to press NEXT while
        // already standing inside the inspection zone.
        Vector3 nearest =
            inspectionZone.ClosestPoint(player.position);

        insideZone =
            Vector3.Distance(
                player.position,
                nearest
            ) < 0.5f;
    }

    private void UpdateInspectionButtons()
    {
        bool canInspect =
            welcomeCompleted &&
            insideZone &&
            !inspectionStarted &&
            !inspectionCompleted;

        SetVisible(
            inspectPipeButton,
            canInspect
        );
    }

    // ======================================
    // START PIPE INSPECTION
    // ======================================

    public void InspectPipe()
    {
        if (!welcomeCompleted ||
            !insideZone ||
            inspectionStarted ||
            inspectionCompleted)
        {
            return;
        }

        inspectionStarted = true;

        SetVisible(inspectPipeButton, false);

        SetVisible(pipeRotationPanel, true);
        SetVisible(finishInspectionButton, true);

        onInspectionStarted?.Invoke();

        Debug.Log(
            "Pipe visual inspection started."
        );
    }

    // ======================================
    // COMPLETE PIPE INSPECTION
    // ======================================

    public void FinishInspection()
    {
        if (!inspectionStarted ||
            inspectionCompleted)
        {
            return;
        }

        inspectionCompleted = true;
        inspectionStarted = false;

        SetVisible(pipeRotationPanel, false);
        SetVisible(finishInspectionButton, false);

        onInspectionCompleted?.Invoke();

        StartGuidedTraining();
    }

    // ======================================
    // START THE NEXT TRAINING ACTIVITIES
    // ======================================

    private void StartGuidedTraining()
    {
        SetVisible(trainingProgressPanel, true);
        SetVisible(materialLossTrainingPanel, true);

        onTrainingStarted?.Invoke();

        Debug.Log(
            "Visual inspection complete. " +
            "Guided training activities started."
        );
    }

    // ======================================
    // HELPER
    // ======================================

    private void SetVisible(
        GameObject target,
        bool visible)
    {
        if (target != null)
            target.SetActive(visible);
    }

    public bool IsInspectionCompleted()
    {
        return inspectionCompleted;
    }
}
