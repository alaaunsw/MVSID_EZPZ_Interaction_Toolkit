
using UnityEngine;

public class InspectionOnboardingFlow : MonoBehaviour
{
    [Header("Onboarding Panels")]
    public GameObject welcomePanel;
    public GameObject taskOverviewPanel;

    [Header("Opening Pipe Labels")]
    public InspectionOpeningLabels openingLabels;

    [Header("Persistent Task Checklist")]
    public InspectionTaskChecklist taskChecklist;

    private void Start()
    {
        AlignTaskOverview();

        // Show Welcome Panel first
        if (welcomePanel != null)
            welcomePanel.SetActive(true);

        // Hide Task Overview initially
        if (taskOverviewPanel != null)
            taskOverviewPanel.SetActive(false);

        // Hide opening pipe labels initially
        if (openingLabels != null)
            openingLabels.HideLabels();

        // Hide persistent checklist initially
        if (taskChecklist != null)
            taskChecklist.HideHUD();
    }

    private void AlignTaskOverview()
    {
        if (welcomePanel == null ||
            taskOverviewPanel == null)
            return;

        RectTransform welcome =
            welcomePanel.GetComponent<RectTransform>();

        RectTransform overview =
            taskOverviewPanel.GetComponent<RectTransform>();

        if (welcome == null || overview == null)
            return;

        // Both panels should share the same parent Canvas.
        if (welcome.parent != overview.parent)
        {
            Debug.LogWarning(
                "Welcome and Task Overview panels " +
                "must have the same parent Canvas."
            );
            return;
        }

        // Copy the Welcome Panel layout.
        overview.anchorMin = welcome.anchorMin;
        overview.anchorMax = welcome.anchorMax;
        overview.pivot = welcome.pivot;

        overview.sizeDelta = welcome.sizeDelta;

        overview.anchoredPosition3D =
            welcome.anchoredPosition3D;

        overview.localRotation = welcome.localRotation;
        overview.localScale = welcome.localScale;
    }

    // STEP 1 -> STEP 2
    public void ShowTaskOverview()
    {
        if (taskOverviewPanel == null)
        {
            Debug.LogWarning(
                "Task Overview Panel is not assigned."
            );
            return;
        }

        AlignTaskOverview();

        if (welcomePanel != null)
            welcomePanel.SetActive(false);

        taskOverviewPanel.SetActive(true);

        // Show the three pipe labels
        if (openingLabels != null)
            openingLabels.ShowLabels();

        // Checklist remains hidden during overview
        if (taskChecklist != null)
            taskChecklist.HideHUD();

        Debug.Log("Task Overview opened.");
    }

    // STEP 2 -> BEGIN INSPECTION
    public void BeginInspection()
    {
        // Hide Welcome Panel
        if (welcomePanel != null)
            welcomePanel.SetActive(false);

        // Hide Task Overview Panel
        if (taskOverviewPanel != null)
            taskOverviewPanel.SetActive(false);

        // Hide opening pipe labels
        if (openingLabels != null)
            openingLabels.HideLabels();

        // Show persistent task checklist
        if (taskChecklist != null)
        {
            taskChecklist.ResetTasks();
            taskChecklist.ShowHUD();
        }
        else
        {
            Debug.LogWarning(
                "Inspection Task Checklist is not assigned."
            );
        }

        Debug.Log("Pipeline inspection started.");
    }
}
