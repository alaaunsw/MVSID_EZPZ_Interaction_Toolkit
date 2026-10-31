
using UnityEngine;

public class WhiteboardActivityUI : MonoBehaviour
{
    [Header("Tutorial Panels")]
    public GameObject writePanel;
    public GameObject whiteboardHighlight;
    public GameObject whiteboardOptionsPanel;

    public void OnFirstDrawing()
    {
        if (writePanel != null)
            writePanel.SetActive(false);

        if (whiteboardHighlight != null)
            whiteboardHighlight.SetActive(false);

        if (whiteboardOptionsPanel != null)
            whiteboardOptionsPanel.SetActive(true);

        Debug.Log("Whiteboard options are now visible.");
    }
}

