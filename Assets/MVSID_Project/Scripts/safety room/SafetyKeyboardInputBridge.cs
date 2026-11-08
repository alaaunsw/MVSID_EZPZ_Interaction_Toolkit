
using UnityEngine;
using TMPro;

public class SafetyKeyboardInputBridge : MonoBehaviour
{
    [Header("EZPZ Keyboard")]
    public TextUtility keyboardOutput;

    [Header("Identification Panel")]
    public TMP_InputField traineeNameInput;
    public TMP_InputField traineePasswordInput;

    [Header("Settings")]
    public int maxCharacters = 32;
    public int maxPasswordCharacters = 64;

    [Header("Keyboard Visibility")]
    public GameObject keyboardRig;

    [Header("Panel Following")]
    public SafetyPanelFollow panelFollow;

    [Header("Keyboard Following")]
    public SafetyKeyboardFollow keyboardFollow;

    private enum InputMode
    {
        None,
        Username,
        Password
    }

    private InputMode activeInput = InputMode.None;

    private bool keyboardOpen = false;

    private string savedUsername = "";
    private string savedPassword = "";

    private bool originalWorldDisplayEnabled;
    private bool originalUIDisplayEnabled;

    private void Awake()
    {
        if (keyboardOutput != null)
        {
            if (keyboardOutput.textDisplay != null)
                originalWorldDisplayEnabled =
                    keyboardOutput.textDisplay.enabled;

            if (keyboardOutput.textDisplayPUGUI != null)
                originalUIDisplayEnabled =
                    keyboardOutput.textDisplayPUGUI.enabled;
        }

        if (traineePasswordInput != null)
        {
            traineePasswordInput.contentType =
                TMP_InputField.ContentType.Password;

            traineePasswordInput.asteriskChar = '*';
            traineePasswordInput.ForceLabelUpdate();
        }
    }

    private void LateUpdate()
    {
        if (!keyboardOpen || keyboardOutput == null)
            return;

        string currentText = keyboardOutput.buffer ?? "";

        if (activeInput == InputMode.Username)
        {
            currentText = LimitText(
                currentText,
                maxCharacters
            );

            savedUsername = currentText;

            UpdateField(
                traineeNameInput,
                savedUsername
            );
        }
        else if (activeInput == InputMode.Password)
        {
            currentText = LimitText(
                currentText,
                maxPasswordCharacters
            );

            savedPassword = currentText;

            UpdateField(
                traineePasswordInput,
                savedPassword
            );
        }

        if (keyboardOutput.buffer != currentText)
            keyboardOutput.buffer = currentText;
    }

    // Existing username-opening action.
    public void OpenKeyboard()
    {
        OpenUsernameKeyboard();
    }

    public void OpenUsernameKeyboard()
    {
        OpenForInput(InputMode.Username);
    }

    public void OpenPasswordKeyboard()
    {
        OpenForInput(InputMode.Password);
    }

    private void OpenForInput(InputMode mode)
    {
        if (keyboardOutput == null || keyboardRig == null)
        {
            Debug.LogWarning(
                "Keyboard Output or Keyboard Rig is missing."
            );
            return;
        }

        // Preserve edits before changing fields.
        SaveActiveInput();

        // Read the current field contents.
        if (mode == InputMode.Username &&
            traineeNameInput != null)
        {
            savedUsername = traineeNameInput.text;
        }

        if (mode == InputMode.Password &&
            traineePasswordInput != null)
        {
            savedPassword = traineePasswordInput.text;
        }

        activeInput = mode;

        if (mode == InputMode.Username)
        {
            keyboardOutput.buffer = savedUsername;
        }
        else
        {
            keyboardOutput.buffer = savedPassword;
        }

        // Pause the panel while typing.
        if (panelFollow != null)
            panelFollow.PauseFollowing();

        keyboardRig.SetActive(true);
        keyboardOpen = true;

        // Do not show password characters in
        // the EZPZ keyboard's output display.
        SetKeyboardOutputVisibility(
            mode != InputMode.Password
        );

        if (mode == InputMode.Username)
            Debug.Log("Username keyboard opened.");
        else
            Debug.Log("Password keyboard opened.");
    }

    private void SaveActiveInput()
    {
        if (!keyboardOpen || keyboardOutput == null)
            return;

        string value = keyboardOutput.buffer ?? "";

        if (activeInput == InputMode.Username)
        {
            savedUsername = LimitText(
                value,
                maxCharacters
            );

            UpdateField(
                traineeNameInput,
                savedUsername
            );
        }
        else if (activeInput == InputMode.Password)
        {
            savedPassword = LimitText(
                value,
                maxPasswordCharacters
            );

            UpdateField(
                traineePasswordInput,
                savedPassword
            );
        }
    }

    public void FinishTyping()
    {
        SaveActiveInput();

        keyboardOpen = false;
        activeInput = InputMode.None;

        // Clear the shared EZPZ text buffer.
        if (keyboardOutput != null)
        {
            keyboardOutput.buffer = "";
            UpdateKeyboardDisplay("");
        }

        RestoreKeyboardOutputVisibility();

        if (keyboardRig != null)
            keyboardRig.SetActive(false);

        // Restore panel following.
        if (panelFollow != null)
            panelFollow.ResumeFollowing();

        Debug.Log("Trainee finished typing.");
    }

    public void ResetKeyboardInputs()
    {
        keyboardOpen = false;
        activeInput = InputMode.None;

        savedUsername = "";
        savedPassword = "";

        if (keyboardOutput != null)
        {
            keyboardOutput.buffer = "";
            UpdateKeyboardDisplay("");
        }

        UpdateField(traineeNameInput, "");
        UpdateField(traineePasswordInput, "");

        RestoreKeyboardOutputVisibility();

        if (keyboardRig != null)
            keyboardRig.SetActive(false);

        if (panelFollow != null)
            panelFollow.ResumeFollowing();
    }

    private string LimitText(string value, int limit)
    {
        if (string.IsNullOrEmpty(value))
            return "";

        if (limit > 0 && value.Length > limit)
            return value.Substring(0, limit);

        return value;
    }

    private void UpdateField(
        TMP_InputField field,
        string value)
    {
        if (field == null)
            return;

        if (field.text != value)
        {
            field.SetTextWithoutNotify(value);
            field.ForceLabelUpdate();
        }
    }

    private void UpdateKeyboardDisplay(string value)
    {
        if (keyboardOutput == null)
            return;

        if (keyboardOutput.textDisplay != null)
            keyboardOutput.textDisplay.text = value;

        if (keyboardOutput.textDisplayPUGUI != null)
            keyboardOutput.textDisplayPUGUI.text = value;
    }

    private void SetKeyboardOutputVisibility(bool visible)
    {
        if (keyboardOutput == null)
            return;

        if (keyboardOutput.textDisplay != null)
        {
            keyboardOutput.textDisplay.enabled =
                visible && originalWorldDisplayEnabled;
        }

        if (keyboardOutput.textDisplayPUGUI != null)
        {
            keyboardOutput.textDisplayPUGUI.enabled =
                visible && originalUIDisplayEnabled;
        }
    }

    private void RestoreKeyboardOutputVisibility()
    {
        if (keyboardOutput == null)
            return;

        if (keyboardOutput.textDisplay != null)
        {
            keyboardOutput.textDisplay.enabled =
                originalWorldDisplayEnabled;
        }

        if (keyboardOutput.textDisplayPUGUI != null)
        {
            keyboardOutput.textDisplayPUGUI.enabled =
                originalUIDisplayEnabled;
        }
    }
}
