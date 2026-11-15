
using UnityEngine;
using TMPro;

public class ReportTypingMode : MonoBehaviour
{
    [Header("Report Input")]
    public TMP_InputField reportInput;

    [Header("Disable While Typing")]
    public Behaviour[] keyboardControlScripts;

    private bool typing;

    private void Start()
    {
        if (reportInput == null)
            return;

        reportInput.onSelect.AddListener(
            OnStartTyping
        );

        reportInput.onDeselect.AddListener(
            OnStopTyping
        );
    }

    private void OnDisable()
    {
        SetTyping(false);
    }

    private void OnDestroy()
    {
        if (reportInput == null)
            return;

        reportInput.onSelect.RemoveListener(
            OnStartTyping
        );

        reportInput.onDeselect.RemoveListener(
            OnStopTyping
        );
    }

    private void OnStartTyping(string value)
    {
        SetTyping(true);
    }

    private void OnStopTyping(string value)
    {
        SetTyping(false);
    }

    private void SetTyping(bool active)
    {
        if (typing == active)
            return;

        typing = active;

        foreach (Behaviour script in keyboardControlScripts)
        {
            if (script != null)
                script.enabled = !active;
        }

        Debug.Log(
            active
            ? "Report typing mode enabled."
            : "Report typing mode disabled."
        );
    }
}
