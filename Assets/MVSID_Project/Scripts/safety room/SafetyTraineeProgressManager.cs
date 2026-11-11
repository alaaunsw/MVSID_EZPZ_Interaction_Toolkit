
using System;
using UnityEngine;

public class SafetyTraineeProgressManager : MonoBehaviour
{
    [Header("Account System")]
    public SafetyTraineeAccountManager accountManager;

    [Header("Training Pass Requirements")]
    [Range(0, 100)]
    public float minimumPassScore = 80f;

    [Header("Current Trainee Progress")]
    [SerializeField] private bool trainingPassed;
    [SerializeField] private float bestTrainingScore;
    [SerializeField] private int trainingAttempts;
    [SerializeField] private string loadedTraineeId = "";

    [Serializable]
    private class ProgressData
    {
        public string traineeId;
        public bool trainingPassed;
        public float bestTrainingScore;
        public int trainingAttempts;
    }

    public bool TrainingPassed
    {
        get
        {
            LoadCurrentTraineeProgress();
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

    private void Awake()
    {
        if (accountManager == null)
        {
            accountManager =
                FindFirstObjectByType<SafetyTraineeAccountManager>();
        }
    }

    // ==========================================
    // LOAD THE CURRENT USER'S PROGRESS
    // ==========================================

    public void LoadCurrentTraineeProgress()
    {
        if (accountManager == null)
        {
            accountManager =
                FindFirstObjectByType<SafetyTraineeAccountManager>();
        }

        if (accountManager == null ||
            !accountManager.IsLoggedIn)
        {
            ClearCurrentProgress();
            return;
        }

        string traineeId = accountManager.CurrentTraineeId;

        // Avoid reloading the same account unnecessarily.
        if (loadedTraineeId == traineeId)
            return;

        ClearCurrentProgress();

        loadedTraineeId = traineeId;

        string key = GetProgressKey(traineeId);

        if (!PlayerPrefs.HasKey(key))
            return;

        try
        {
            string json = PlayerPrefs.GetString(key);

            ProgressData data =
                JsonUtility.FromJson<ProgressData>(json);

            if (data == null || data.traineeId != traineeId)
                return;

            trainingPassed = data.trainingPassed;
            bestTrainingScore = data.bestTrainingScore;
            trainingAttempts = data.trainingAttempts;
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
    // RECORD AN ACTUAL TRAINING RESULT
    // ==========================================

    public void RecordTrainingResult(
        float score,
        bool criticalSafetyPassed)
    {
        LoadCurrentTraineeProgress();

        if (accountManager == null ||
            !accountManager.IsLoggedIn)
        {
            Debug.LogWarning(
                "Cannot record progress: no trainee logged in."
            );
            return;
        }

        if (float.IsNaN(score) ||
            float.IsInfinity(score) ||
            score < 0f || score > 100f)
        {
            Debug.LogWarning("Invalid training score.");
            return;
        }

        trainingAttempts++;

        bestTrainingScore =
            Mathf.Max(bestTrainingScore, score);

        bool passedThisAttempt =
            score >= minimumPassScore &&
            criticalSafetyPassed;

        // Once unlocked, Testing Mode remains available.
        if (passedThisAttempt)
            trainingPassed = true;

        SaveCurrentProgress();

        Debug.Log(
            "Training result saved for " +
            accountManager.CurrentUsername +
            " | Score: " + score +
            " | Safety passed: " + criticalSafetyPassed +
            " | Testing unlocked: " + trainingPassed
        );
    }

    // ==========================================
    // SAVE CURRENT USER'S PROGRESS
    // ==========================================

    private void SaveCurrentProgress()
    {
        if (string.IsNullOrEmpty(loadedTraineeId))
            return;

        ProgressData data = new ProgressData
        {
            traineeId = loadedTraineeId,
            trainingPassed = trainingPassed,
            bestTrainingScore = bestTrainingScore,
            trainingAttempts = trainingAttempts
        };

        string json = JsonUtility.ToJson(data);

        PlayerPrefs.SetString(
            GetProgressKey(loadedTraineeId),
            json
        );

        PlayerPrefs.Save();
    }

    private string GetProgressKey(string traineeId)
    {
        return "TraineeProgress_" + traineeId;
    }

    private void ClearCurrentProgress()
    {
        trainingPassed = false;
        bestTrainingScore = 0f;
        trainingAttempts = 0;
        loadedTraineeId = "";
    }

    public void RefreshProgress()
    {
        ClearCurrentProgress();
        LoadCurrentTraineeProgress();
    }
}
