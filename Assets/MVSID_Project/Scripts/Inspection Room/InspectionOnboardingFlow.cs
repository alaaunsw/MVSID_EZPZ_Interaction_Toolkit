
using UnityEngine;

public class InspectionOnboardingFlow : MonoBehaviour
{
    [Header("Onboarding Panels")]
    public GameObject welcomePanel;
    public GameObject taskOverviewPanel;

    private void Start()
    {
        if (welcomePanel != null)
            welcomePanel.SetActive(true);

        if (taskOverviewPanel != null)
            taskOverviewPanel.SetActive(false);
    }

    public void ShowTaskOverview()
    {
        if (taskOverviewPanel == null)
        {
            Debug.LogWarning(
                "Task Overview Panel is not assigned."
            );
            return;
        }

        if (welcomePanel != null)
            welcomePanel.SetActive(false);

        taskOverviewPanel.SetActive(true);

        Debug.Log("Task Overview opened.");
    }

    public void BeginInspection()
    {
        if (welcomePanel != null)
            welcomePanel.SetActive(false);

        if (taskOverviewPanel != null)
            taskOverviewPanel.SetActive(false);

        Debug.Log("Pipeline inspection started.");
    }
}
