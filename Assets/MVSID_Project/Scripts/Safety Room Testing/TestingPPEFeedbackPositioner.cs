
using UnityEngine;

public class TestingPPEFeedbackPositioner : MonoBehaviour
{
    [Header("Feedback Symbols")]
    public RectTransform correctTick;
    public RectTransform wrongAnswerX;

    [Header("Position on Question Panel")]
    public Vector2 feedbackPosition =
        new Vector2(0f, -240f);

    [Header("Symbol Size")]
    public Vector2 feedbackSize =
        new Vector2(100f, 100f);

    [Header("Panel Depth")]
    public float localZ = -0.02f;

    private void Start()
    {
        PositionFeedback();
    }

    [ContextMenu("Position Feedback Symbols")]
    public void PositionFeedback()
    {
        PositionSymbol(correctTick);
        PositionSymbol(wrongAnswerX);
    }

    private void PositionSymbol(RectTransform symbol)
    {
        if (symbol == null)
            return;

        RectTransform panel =
            transform as RectTransform;

        if (panel == null)
            return;

        if (symbol.parent != panel)
        {
            Debug.LogWarning(
                symbol.name +
                " must be a child of TestingPPEQuestionPanel."
            );

            return;
        }

        symbol.anchorMin =
            new Vector2(0.5f, 0.5f);

        symbol.anchorMax =
            new Vector2(0.5f, 0.5f);

        symbol.pivot =
            new Vector2(0.5f, 0.5f);

        symbol.anchoredPosition3D =
            new Vector3(
                feedbackPosition.x,
                feedbackPosition.y,
                localZ
            );

        symbol.sizeDelta =
            feedbackSize;

        symbol.localRotation =
            Quaternion.identity;

        symbol.localScale =
            Vector3.one;
    }
}
