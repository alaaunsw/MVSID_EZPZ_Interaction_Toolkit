
using UnityEngine;
using UnityEngine.SceneManagement;

public class SafetyToInspectionTransition : MonoBehaviour
{
    [Header("Destination Scenes")]
    public string trainingSceneName = "Training Environment";
    public string testingSceneName = "Testing Environment";

    private bool isLoading;

    // ==========================================
    // ENTER TRAINING MODE
    // ==========================================

    public void EnterTrainingMode()
    {
        LoadScene(trainingSceneName);
    }

    // ==========================================
    // ENTER TESTING MODE
    // ==========================================

    public void EnterTestingMode()
    {
        LoadScene(testingSceneName);
    }

    // ==========================================
    // LOAD SELECTED SCENE
    // ==========================================

    private void LoadScene(string sceneName)
    {
        if (isLoading)
            return;

        if (string.IsNullOrWhiteSpace(sceneName))
        {
            Debug.LogWarning(
                "Scene name is not assigned."
            );

            return;
        }

        if (!Application.CanStreamedLevelBeLoaded(sceneName))
        {
            Debug.LogError(
                "Scene is missing from Build Profiles: " +
                sceneName
            );

            return;
        }

        isLoading = true;

        Debug.Log(
            "Entering: " + sceneName
        );

        SceneManager.LoadScene(sceneName);
    }
}
