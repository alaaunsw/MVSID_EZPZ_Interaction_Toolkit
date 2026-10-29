using UnityEngine;
using UnityEngine.UI;

public class TutorialButtonHover : MonoBehaviour
{
    public Image buttonImage;

    public Color normalColor = new Color32(25, 115, 205, 255);
    public Color hoverColor = new Color32(40, 180, 240, 255);

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