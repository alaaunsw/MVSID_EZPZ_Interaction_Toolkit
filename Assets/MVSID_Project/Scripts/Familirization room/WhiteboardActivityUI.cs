
using UnityEngine;

public class WhiteboardActivityUI : MonoBehaviour
{
    [Header("Writing Instruction")]
    public GameObject writeHereText;

    [Header("Tutorial Panel")]
    public GameObject writePanel;

    [Header("Whiteboard Buttons")]
    public GameObject eraseBoardButton;
    public GameObject checkFeedbackButton;

    public void OnFirstDrawing()
    {
        // Hide the pulsing writing instruction
        if (writeHereText != null)
            writeHereText.SetActive(false);

        // Hide the original writing panel
        if (writePanel != null)
            writePanel.SetActive(false);

        // Show the whiteboard buttons
        if (eraseBoardButton != null)
            eraseBoardButton.SetActive(true);

        if (checkFeedbackButton != null)
            checkFeedbackButton.SetActive(true);

        Debug.Log("Writing started. Whiteboard buttons activated.");
    }
}
