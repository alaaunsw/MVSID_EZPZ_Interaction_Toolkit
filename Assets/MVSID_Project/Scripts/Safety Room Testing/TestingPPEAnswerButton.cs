
using UnityEngine;

public class TestingPPEAnswerButton : MonoBehaviour
{
    [Header("Testing PPE Assessment")]
    public TestingPPEAssessmentManager assessmentManager;

    [Header("Answer Selection")]
    [Range(0, 3)]
    public int answerIndex;

    public void Activate()
    {
        if (assessmentManager == null)
        {
            Debug.LogError(
                "Testing PPE Assessment Manager is not assigned."
            );
            return;
        }

        Debug.Log(
            "PPE answer button selected: " +
            (answerIndex + 1)
        );

        assessmentManager.SelectAnswer(answerIndex);
    }
}
