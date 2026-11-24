using System.Collections;
using TMPro;
using UnityEngine;

public class TestingHUDMessageSystem : MonoBehaviour
{
    [Header("HUD Text")]
    public TextMeshProUGUI objectiveText;
    public TextMeshProUGUI hintText;

    [Header("Typing Effect")]
    public float typingSpeed = 0.02f;

    [Header("Optional Audio")]
    public AudioSource typingAudioSource;
    public AudioClip typingClip;
    public AudioSource notificationAudioSource;
    public AudioClip newObjectiveClip;

    private Coroutine objectiveRoutine;
    private Coroutine hintRoutine;

    private void Start()
    {
        if (hintText != null)
            hintText.text = "";
    }

    // ==========================
    // OBJECTIVE
    // ==========================

    public void SetObjective(string message, bool typewriter = true)
    {
        if (objectiveText == null)
            return;

        if (objectiveRoutine != null)
            StopCoroutine(objectiveRoutine);

        if (notificationAudioSource != null && newObjectiveClip != null)
            notificationAudioSource.PlayOneShot(newObjectiveClip);

        if (typewriter)
            objectiveRoutine = StartCoroutine(TypeText(objectiveText, message));
        else
            objectiveText.text = message;
    }

    // ==========================
    // HINT
    // ==========================

    public void ShowHint(string message, float duration = 4f, bool typewriter = true)
    {
        if (hintText == null)
            return;

        if (hintRoutine != null)
            StopCoroutine(hintRoutine);

        hintRoutine = StartCoroutine(ShowHintRoutine(message, duration, typewriter));
    }

    public void ClearHint()
    {
        if (hintRoutine != null)
            StopCoroutine(hintRoutine);

        if (hintText != null)
            hintText.text = "";
    }

    private IEnumerator ShowHintRoutine(string message, float duration, bool typewriter)
    {
        if (typewriter)
            yield return StartCoroutine(TypeText(hintText, message));
        else
            hintText.text = message;

        yield return new WaitForSeconds(duration);

        hintText.text = "";
    }

    // ==========================
    // TYPE TEXT
    // ==========================

    private IEnumerator TypeText(TextMeshProUGUI target, string fullText)
    {
        if (target == null)
            yield break;

        target.text = "";

        for (int i = 0; i < fullText.Length; i++)
        {
            target.text += fullText[i];

            if (typingAudioSource != null && typingClip != null)
                typingAudioSource.PlayOneShot(typingClip, 0.15f);

            yield return new WaitForSeconds(typingSpeed);
        }
    }
}