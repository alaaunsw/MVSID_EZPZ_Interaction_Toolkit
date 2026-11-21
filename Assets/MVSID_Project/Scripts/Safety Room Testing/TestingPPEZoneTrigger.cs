
using UnityEngine;

public class TestingPPEZoneTrigger : MonoBehaviour
{
    [Header("Testing PPE Assessment")]
    public TestingPPEAssessmentManager assessmentManager;

    [Header("Existing PPE Interaction Points")]
    public GameObject[] ppeInfoPoints;

    [Header("Testing Status")]
    [SerializeField] private bool explorationStarted;

    private SafetyTraineeAccountManager accountManager;
    private SafetyTraineeProgressManager progressManager;

    // ==========================================
    // START
    // ==========================================

    private void Start()
    {
        explorationStarted = false;

        // Hide equipment points until the
        // trainee enters the assessment zone.

        foreach (GameObject point in ppeInfoPoints)
        {
            if (point != null)
            {
                point.SetActive(false);
            }
        }
    }

    // ==========================================
    // PLAYER ENTERS BLUE ZONE
    // ==========================================

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        BeginExploration();
    }

    // Also supports a trainee who is already
    // standing in the zone when login completes.

    private void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (!explorationStarted)
        {
            BeginExploration();
        }
    }

    // ==========================================
    // VERIFY ACCESS AND ACTIVATE POINTS
    // ==========================================

    public void BeginExploration()
    {
        if (explorationStarted)
            return;

        accountManager =
            SafetyTraineeAccountManager.Instance;

        if (accountManager == null ||
            !accountManager.IsLoggedIn)
        {
            return;
        }

        progressManager =
            accountManager.GetComponent<
                SafetyTraineeProgressManager>();

        if (progressManager == null)
        {
            Debug.LogWarning(
                "Testing progress manager not found."
            );
            return;
        }

        progressManager.RefreshProgress();

        if (!progressManager.CanAccessTesting)
        {
            Debug.LogWarning(
                "Complete the required training " +
                "sessions before PPE testing."
            );
            return;
        }

        if (assessmentManager == null)
        {
            Debug.LogError(
                "Testing PPE Assessment Manager " +
                "is not assigned."
            );
            return;
        }

        // ======================================
        // ACTIVATE ALL PPE INTERACTION POINTS
        // ======================================

        explorationStarted = true;

        int activated = 0;

        foreach (GameObject point in ppeInfoPoints)
        {
            if (point == null)
                continue;

            point.SetActive(true);
            activated++;
        }

        Debug.Log(
            "Independent PPE exploration started for: " +
            accountManager.CurrentUsername
        );

        Debug.Log(
            "PPE interaction points activated: " +
            activated
        );

        Debug.Log(
            "Explore the equipment and identify " +
            "what each item is used for."
        );
    }
}
