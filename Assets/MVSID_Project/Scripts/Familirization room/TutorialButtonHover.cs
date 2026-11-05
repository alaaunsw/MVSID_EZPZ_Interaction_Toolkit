
using UnityEngine;
using UnityEngine.UI;

public class TutorialButtonHover : MonoBehaviour
{
    [Header("Button Appearance")]
    public Image buttonImage;

    public Color normalColor = new Color32(25, 116, 209, 255);
    public Color hoverColor = new Color32(65, 215, 240, 255);

    public void HoverEnter()
    {
        if (buttonImage != null)
            buttonImage.color = hoverColor;
    }

    public void HoverExit()
    {
        if (buttonImage != null)
            buttonImage.color = normalColor;
    }
}
