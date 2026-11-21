
using System;
using System.Collections.Generic;
using UnityEngine;

public class SafetyTraineeProgressManager : MonoBehaviour
{
    [Header("Account System")]
    public SafetyTraineeAccountManager accountManager;

    [Header("Training Pass Requirements")]
    [Range(0, 100)]
    public float minimumPassScore = 80f;

    [Min(1)]
    public int requiredTrainingSessions = 1;

    [Header("Current Trainee Progress")]
    [SerializeField] private bool trainingPassed;
    [SerializeField] private float bestTrainingScore;
    [SerializeField] private int trainingAttempts;

    [SerializeField] private int successfulTrainingSessions;
    [SerializeField] private int remainingTrainingSessions;

    [SerializeField] private string loadedTraineeId = "";
    [SerializeField] private string lastTrainingCompletionDate = "";

    private List<string> recordedSessionIds =
        new List<string>();

    [Serializable]
    private class ProgressData
    {
        public string traineeId;
        public bool trainingPassed;
        public float bestTrainingScore;
        public int trainingAttempts;

        public int successfulTrainingSessions;
        public string lastTrainingCompletionDate;

        public List<string> recordedSessionIds =
            new List<string>();
    }

    // ==========================================
    // PUBLIC PROGRESS PROPERTIES
    // ==========================================

    public bool TrainingPassed
    {
        get
        {
            LoadCurrentTraineeProgress();
            UpdateTrainingEligibility();

            return trainingPassed;
        }
    }

    public bool CanAccessTesting
    {
        get
        {
            return TrainingPassed;
        }
    }

    public int RequiredTrainingSessions
    {
        get
        {
            return Mathf.Max(
                1, requiredTrainingSessions
            );
        }
    }

    public int SuccessfulTrainingSessions
    {
        get
        {
            LoadCurrentTraineeProgress();

            return successfulTrainingSessions;
        }
    }

    public int RemainingTrainingSessions
    {
        get
        {
            LoadCurrentTraineeProgress();

            return Mathf.Max(
                0,
                RequiredTrainingSessions -
                successfulTrainingSessions
            );
        }
    }

    public int TrainingAttempts
    {
        get
        {
            LoadCurrentTraineeProgress();

            return trainingAttempts;
        }
    }

    public float BestTrainingScore
    {
        get
        {
            LoadCurrentTraineeProgress();

            return bestTrainingScore;
        }
    }

    public string LastTrainingCompletionDate
    {
        get
        {
            LoadCurrentTraineeProgress();

            return lastTrainingCompletionDate;
        }
    }

    // ==========================================
    // UNITY
    // ==========================================

    private void Awake()
    {
        FindAccountManager();
    }

    private void Start()
    {
        LoadCurrentTraineeProgress();
        UpdateTrainingEligibility();
    }

    private void OnValidate()
    {
        requiredTrainingSessions =
            Mathf.Max(
                1, requiredTrainingSessions
            );

        UpdateTrainingEligibility();
    }

    private void FindAccountManager()
    {
        if (accountManager == null)
        {
            accountManager =
                FindFirstObjectByType<
                    SafetyTraineeAccountManager
                >();
        }
    }

    // ==========================================
    // LOAD CURRENT TRAINEE PROGRESS
    // ==========================================

    public void LoadCurrentTraineeProgress()
    {
        FindAccountManager();

        if (accountManager == null ||
            !accountManager.IsLoggedIn)
        {
            ClearCurrentProgress();

            return;
        }

        string traineeId =
            accountManager.CurrentTraineeId;

        if (string.IsNullOrEmpty(traineeId))
        {
            ClearCurrentProgress();

            return;
        }

        if (loadedTraineeId == traineeId)
        {
            UpdateTrainingEligibility();

            return;
        }

        ClearCurrentProgress();

        loadedTraineeId = traineeId;

        string key =
            GetProgressKey(traineeId);

        if (!PlayerPrefs.HasKey(key))
        {
            UpdateTrainingEligibility();

            return;
        }

        try
        {
            string json =
                PlayerPrefs.GetString(key);

            ProgressData data =
                JsonUtility.FromJson<
                    ProgressData
                >(json);

            if (data == null ||
                data.traineeId != traineeId)
            {
                return;
            }

            bestTrainingScore =
                data.bestTrainingScore;

            trainingAttempts =
                Mathf.Max(
                    0, data.trainingAttempts
                );

            successfulTrainingSessions =
                Mathf.Max(
                    0,
                    data.successfulTrainingSessions
                );

            lastTrainingCompletionDate =
                data.lastTrainingCompletionDate ?? "";

            recordedSessionIds =
                data.recordedSessionIds != null
                ? new List<string>(
                    data.recordedSessionIds
                )
                : new List<string>();

            UpdateTrainingEligibility();

            Debug.Log(
                "Trainee progress loaded: " +
                accountManager.CurrentUsername +
                " | Successful sessions: " +
                successfulTrainingSessions +
                " / " +
                RequiredTrainingSessions
            );
        }
        catch (Exception error)
        {
            Debug.LogError(
                "Could not load trainee progress: " +
                error.Message
            );
        }
    }

    // ==========================================
    // CHECK ACCOUNT
    // ==========================================

    private bool HasValidAccount()
    {
        LoadCurrentTraineeProgress();

        if (accountManager == null ||
            !accountManager.IsLoggedIn ||
            string.IsNullOrEmpty(loadedTraineeId))
        {
            Debug.LogWarning(
                "Cannot record training progress: " +
                "no trainee logged in."
            );

            return false;
        }

        return true;
    }

    // ==========================================
    // RECORD LEGACY SCORED TRAINING RESULT
    // ==========================================

    public void RecordTrainingResult(
        float score,
        bool criticalSafetyPassed)
    {
        // Retain compatibility with existing scripts.
        // New training integrations should use a
        // persistent, unique session ID.

        string sessionId =
            Guid.NewGuid().ToString("N");

        RecordTrainingResult(
            score,
            criticalSafetyPassed,
            sessionId
        );
    }

    // ==========================================
    // RECORD SCORED TRAINING WITH SESSION ID
    // ==========================================

    public bool RecordTrainingResult(
        float score,
        bool criticalSafetyPassed,
        string sessionId)
    {
        if (!Application.isPlaying)
            return false;

        if (!HasValidAccount())
            return false;

        if (float.IsNaN(score) ||
            float.IsInfinity(score) ||
            score < 0f ||
            score > 100f)
        {
            Debug.LogWarning(
                "Invalid training score."
            );

            return false;
        }

        if (!IsNewSession(sessionId))
            return false;

        recordedSessionIds.Add(sessionId);

        trainingAttempts++;

        bestTrainingScore =
            Mathf.Max(
                bestTrainingScore, score
            );

        bool passedThisAttempt =
            score >= minimumPassScore &&
            criticalSafetyPassed;

        if (passedThisAttempt)
        {
            successfulTrainingSessions++;

            lastTrainingCompletionDate =
                DateTime.Now.ToString(
                    "yyyy-MM-dd HH:mm:ss"
                );
        }

        UpdateTrainingEligibility();

        SaveCurrentProgress();

        Debug.Log(
            "Training result saved for " +
            accountManager.CurrentUsername +
            " | Score: " + score +
            " | Safety passed: " +
            criticalSafetyPassed +
            " | Attempts: " +
            trainingAttempts +
            " | Successful sessions: " +
            successfulTrainingSessions +
            " / " +
            RequiredTrainingSessions +
            " | Testing unlocked: " +
            trainingPassed
        );

        return true;
    }

    // ==========================================
    // RECORD VERIFIED GUIDED TRAINING COMPLETION
    // ==========================================

    public bool RecordGuidedTrainingCompletion(
        string sessionId,
        bool allTasksCompleted,
        bool criticalSafetyPassed)
    {
        if (!Application.isPlaying)
            return false;

        if (!HasValidAccount())
            return false;

        if (!allTasksCompleted)
        {
            Debug.LogWarning(
                "Training cannot be recorded. " +
                "All six checklist tasks must be completed."
            );

            return false;
        }

        if (!criticalSafetyPassed)
        {
            Debug.LogWarning(
                "Training cannot be recorded. " +
                "Safety requirements are incomplete."
            );

            return false;
        }

        if (!IsNewSession(sessionId))
            return false;

        // Record this unique training session.

        recordedSessionIds.Add(sessionId);

        trainingAttempts++;

        successfulTrainingSessions++;

        lastTrainingCompletionDate =
            DateTime.Now.ToString(
                "yyyy-MM-dd HH:mm:ss"
            );

        // Checklist completion is not a measured
        // performance score. The existing best
        // training score is therefore unchanged.

        UpdateTrainingEligibility();

        SaveCurrentProgress();

        Debug.Log(
            "Guided training successfully recorded."
        );

        Debug.Log(
            "Trainee: " +
            accountManager.CurrentUsername
        );

        Debug.Log(
            "Successful training sessions: " +
            successfulTrainingSessions +
            " / " +
            RequiredTrainingSessions
        );

        Debug.Log(
            "Remaining training sessions: " +
            remainingTrainingSessions
        );

        if (trainingPassed)
        {
            Debug.Log(
                "Independent Testing Mode unlocked."
            );
        }
        else
        {
            Debug.Log(
                "Independent Testing remains locked."
            );
        }

        return true;
    }

    // ==========================================
    // PREVENT DUPLICATE SESSION RECORDS
    // ==========================================

    private bool IsNewSession(string sessionId)
    {
        if (string.IsNullOrWhiteSpace(sessionId))
        {
            Debug.LogWarning(
                "Training session ID is invalid."
            );

            return false;
        }

        if (recordedSessionIds.Contains(sessionId))
        {
            Debug.LogWarning(
                "This training session has already " +
                "been recorded."
            );

            return false;
        }

        return true;
    }

    // ==========================================
    // UPDATE TESTING MODE ELIGIBILITY
    // ==========================================

    private void UpdateTrainingEligibility()
    {
        int required =
            Mathf.Max(
                1, requiredTrainingSessions
            );

        remainingTrainingSessions =
            Mathf.Max(
                0,
                required -
                successfulTrainingSessions
            );

        trainingPassed =
            !string.IsNullOrEmpty(loadedTraineeId) &&
            successfulTrainingSessions >= required;
    }

    // ==========================================
    // GET TRAINING SESSION PROGRESS
    // ==========================================

    public int GetTrainingProgressPercentage()
    {
        LoadCurrentTraineeProgress();

        float progress =
            (float)successfulTrainingSessions /
            RequiredTrainingSessions;

        return Mathf.RoundToInt(
            Mathf.Clamp01(progress) * 100f
        );
    }

    public string GetTrainingProgressText()
    {
        LoadCurrentTraineeProgress();

        return
            successfulTrainingSessions +
            " / " +
            RequiredTrainingSessions +
            " training sessions completed";
    }

    public string GetTestingEligibilityMessage()
    {
        LoadCurrentTraineeProgress();

        if (string.IsNullOrEmpty(loadedTraineeId))
        {
            return "Log in to access training.";
        }

        if (trainingPassed)
        {
            return "Independent Testing is unlocked.";
        }

        int remaining =
            RemainingTrainingSessions;

        return
            "Complete " +
            remaining +
            " more guided training " +
            (remaining == 1 ? "session" : "sessions") +
            " to unlock Independent Testing.";
    }

    // ==========================================
    // SAVE CURRENT TRAINEE PROGRESS
    // ==========================================

    private void SaveCurrentProgress()
    {
        if (string.IsNullOrEmpty(loadedTraineeId))
            return;

        ProgressData data = new ProgressData
        {
            traineeId =
                loadedTraineeId,

            trainingPassed =
                trainingPassed,

            bestTrainingScore =
                bestTrainingScore,

            trainingAttempts =
                trainingAttempts,

            successfulTrainingSessions =
                successfulTrainingSessions,

            lastTrainingCompletionDate =
                lastTrainingCompletionDate,

            recordedSessionIds =
                new List<string>(
                    recordedSessionIds
                )
        };

        string json =
            JsonUtility.ToJson(data);

        PlayerPrefs.SetString(
            GetProgressKey(loadedTraineeId),
            json
        );

        PlayerPrefs.Save();
    }

    // ==========================================
    // INDIVIDUAL TRAINEE STORAGE KEY
    // ==========================================

    private string GetProgressKey(
        string traineeId)
    {
        return
            "TraineeProgress_" +
            traineeId;
    }

    // ==========================================
    // CLEAR LOADED PROGRESS
    // ==========================================

    private void ClearCurrentProgress()
    {
        trainingPassed = false;

        bestTrainingScore = 0f;

        trainingAttempts = 0;

        successfulTrainingSessions = 0;

        remainingTrainingSessions =
            RequiredTrainingSessions;

        loadedTraineeId = "";

        lastTrainingCompletionDate = "";

        recordedSessionIds =
            new List<string>();
    }

    // ==========================================
    // REFRESH CURRENT ACCOUNT
    // ==========================================

    public void RefreshProgress()
    {
        ClearCurrentProgress();

        LoadCurrentTraineeProgress();

        UpdateTrainingEligibility();
    }

    // ==========================================
    // DEBUG CURRENT TRAINEE PROGRESS
    // ==========================================

    [ContextMenu("Check Current Trainee Progress")]
    public void CheckCurrentTraineeProgress()
    {
        LoadCurrentTraineeProgress();

        Debug.Log(
            "Trainee ID: " +
            loadedTraineeId
        );

        Debug.Log(
            "Training attempts: " +
            trainingAttempts
        );

        Debug.Log(
            "Successful training sessions: " +
            successfulTrainingSessions
        );

        Debug.Log(
            "Required training sessions: " +
            RequiredTrainingSessions
        );

        Debug.Log(
            "Remaining training sessions: " +
            RemainingTrainingSessions
        );

        Debug.Log(
            "Best training score: " +
            bestTrainingScore
        );

        Debug.Log(
            "Testing access: " +
            CanAccessTesting
        );
    }
}
