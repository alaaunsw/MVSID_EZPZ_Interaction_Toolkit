
using UnityEngine;
using TMPro;

public class SafetyKeyboardInputBridge : MonoBehaviour
{
    [Header("EZPZ Keyboard")]
    public TextUtility keyboardOutput;

    [Header("Identification Panel")]
    public TMP_InputField traineeNameInput;

    [Header("Settings")]
    public int maxCharacters = 32;

    private void LateUpdate()
    {
        if (keyboardOutput == null || traineeNameInput == null)
            return;

        // Read the characters entered using the EZPZ keyboard.
        string name = keyboardOutput.buffer ?? "";

        if (name.Length > maxCharacters)
            name = name.Substring(0, maxCharacters);

        // Display the same text inside the trainee name field.
        if (traineeNameInput.text != name)
        {
            traineeNameInput.SetTextWithoutNotify(name);
            traineeNameInput.ForceLabelUpdate();
        }
    }
    [Header("Keyboard Visibility")]
public GameObject keyboardRig;

public void OpenKeyboard()
{
    if (keyboardRig != null)
    {
        keyboardRig.SetActive(true);
        Debug.Log("Trainee keyboard opened.");
    }
}
}
