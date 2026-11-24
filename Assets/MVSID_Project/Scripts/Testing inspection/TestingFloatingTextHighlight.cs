
using UnityEngine;
using TMPro;

public class TestingFloatingTextHighlight : MonoBehaviour
{
    [Header("Floating Text")]
    public TMP_Text targetText;

    [Header("Colours")]
    public Color normalColor = new Color32(255, 213, 79, 255);
    public Color hoverColor = new Color32(255, 245, 157, 255);
    public Color pressedColor = Color.white;

    private void Awake()
    {
        if (targetText == null)
            targetText = GetComponentInChildren<TMP_Text>();
    }

    private void OnEnable()
    {
        SetNormal();
    }

    public void SetNormal()
    {
        if (targetText != null)
            targetText.color = normalColor;
    }

    public void SetHover()
    {
        if (targetText != null)
            targetText.color = hoverColor;
    }

    public void SetPressed()
    {
        if (targetText != null)
            targetText.color = pressedColor;
    }
}
