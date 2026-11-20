
using UnityEngine;

public static class TraineeSafetySession
{
    private static string verifiedTraineeId = "";

    private static bool safetyCompleted;

    public static bool SafetyCompleted =>
        safetyCompleted;

    public static string VerifiedTraineeId =>
        verifiedTraineeId;

    // Reset when starting the application,
    // including when Unity domain reload is disabled.

    [RuntimeInitializeOnLoadMethod(
        RuntimeInitializeLoadType.SubsystemRegistration
    )]
    private static void ResetAtStartup()
    {
        Clear();
    }

    public static void MarkCompleted(string traineeId)
    {
        if (!Application.isPlaying)
            return;

        SafetyTraineeAccountManager account =
            SafetyTraineeAccountManager.Instance;

        if (account == null ||
            !account.IsLoggedIn ||
            string.IsNullOrEmpty(traineeId) ||
            account.CurrentTraineeId != traineeId)
        {
            Debug.LogWarning(
                "Cannot verify safety without " +
                "a valid trainee account."
            );

            return;
        }

        verifiedTraineeId = traineeId;
        safetyCompleted = true;

        Debug.Log(
            "Trainee safety session verified."
        );
    }

    public static bool IsCompletedFor(string traineeId)
    {
        if (string.IsNullOrEmpty(traineeId))
            return false;

        SafetyTraineeAccountManager account =
            SafetyTraineeAccountManager.Instance;

        if (account == null ||
            !account.IsLoggedIn)
        {
            return false;
        }

        return safetyCompleted &&
               verifiedTraineeId == traineeId &&
               account.CurrentTraineeId == traineeId;
    }

    public static void Clear()
    {
        verifiedTraineeId = "";
        safetyCompleted = false;
    }
}
