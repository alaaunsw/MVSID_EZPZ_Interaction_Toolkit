
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

        HideAllPoints();
    }

    // ==========================================
    // HIDE ALL PPE INFORMATION POINTS
    // ==========================================

    public void HideAllPoints()
    {
        if (ppeInfoPoints == null)
            return;

        foreach (GameObject point in ppeInfoPoints)
        {
            if (point == null)
                continue;

            if (transform.IsChildOf(point.transform))
                continue;

            if (assessmentManager != null &&
                assessmentManager.transform.IsChildOf(
                    point.transform))
            {
                continue;
            }

            point.SetActive(false);
        }
    }

    // ==========================================
    // CHECK ASSESSMENT COMPLETION
    // ==========================================

    private bool IsAssessmentCompleted()
    {
        return assessmentManager != null &&
            (
                assessmentManager.AreAllQuestionsCompleted() ||
                assessmentManager.IsAssessmentSubmitted()
            );
    }

    // ==========================================
    // PLAYER ENTERS BLUE ZONE
    // ==========================================

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        // Never reactivate completed questions.

        if (IsAssessmentCompleted())
        {
            return;
        }

        BeginExploration();
    }

    // Supports a trainee who is already
    // standing in the zone when login completes.

    private void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (IsAssessmentCompleted())
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
        // Prevent activation after assessment.

        if (IsAssessmentCompleted())
        {
            return;
        }

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

        if (ppeInfoPoints == null)
        {
            Debug.LogWarning(
                "PPE information points are not assigned."
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
