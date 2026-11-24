
using System.Collections;
using TMPro;
using UnityEngine;

public class TestingInspectionDialogue : MonoBehaviour
{
    [Header("Bottom Screen Dialogue")]
    public TextMeshProUGUI dialogueText;

    [Header("Typing Effect")]
    [Range(0.01f, 0.2f)]
    public float characterDelay = 0.055f;

    public float readingTime = 6f;

    [Header("Typing Sound")]
    public AudioSource audioSource;
    public AudioClip typingSound;

    [Tooltip("Use false for sound synchronised with individual letters.")]
    public bool continuousTypingSound = false;

    [Range(0f, 1f)]
    public float typingVolume = 0.15f;

    [Header("First Mission")]
    public bool playOpeningOnStart = true;

    private Coroutine activeMessage;

    private void Start()
    {
        if (dialogueText != null)
        {
            dialogueText.text = "";
            dialogueText.maxVisibleCharacters = int.MaxValue;
        }

        if (audioSource != null)
        {
            audioSource.playOnAwake = false;
            audioSource.loop = false;
            audioSource.Stop();
        }

        if (playOpeningOnStart)
        {
            ShowOpeningMessage();
        }
    }

    public void ShowOpeningMessage()
    {
        ShowMessage(
            "SITE SUPERVISOR",
            "Day 1 - Oil Pipeline Inspection. " +
            "Your assignment is to inspect PIPE_0493. " +
            "Proceed to the designated safety inspection zone."
        );
    }

    public void ShowSafetyZoneMessage()
    {
        ShowMessage(
            "SITE SUPERVISOR",
            "You have reached the inspection area. " +
            "Locate PIPE_0493 and touch its information label " +
            "to review the inspection data."
        );
    }

    public void ShowPipeInformationMessage()
    {
        ShowMessage(
            "SITE SUPERVISOR",
            "You've reviewed the pipeline information. " +
            "Now touch PIPE_0493 to take a closer look " +
            "at its condition."
        );
    }

    public void ShowRotationInstruction()
    {
        ShowMessage(
            "SITE SUPERVISOR",
            "Take a closer look at the pipeline. " +
            "Hold the RIGHT MOUSE BUTTON on the close-up pipe " +
            "and move your mouse to rotate it. " +
            "Inspect the surface carefully for corrosion."
        );
    }

    // NEW: Appears after the trainee finishes
    // examining the close-up pipeline model.
    public void ShowVisualInspectionQuestion()
    {
        ShowMessage(
            "SITE SUPERVISOR",
            "You've completed the close-up inspection " +
            "of PIPE_0493. What did you observe? " +
            "Please submit your inspection findings."
        );
    }

    public void ShowCalculationMessage()
    {
        ShowMessage(
            "INSPECTION ENGINEER",
            "Use the available inspection data to calculate " +
            "material loss and determine the pipeline condition. " +
            "Submit your findings."
        );
    }

    public void ShowEngineerMessage()
    {
        ShowMessage(
            "INSPECTION ENGINEER",
            "Report received. Please check the current " +
            "pipeline temperature and pressure using " +
            "the available instruments."
        );
    }

    public void ShowMessage(string speaker, string message)
    {
        if (activeMessage != null)
        {
            StopCoroutine(activeMessage);
            activeMessage = null;
        }

        StopTypingAudio();

        if (dialogueText == null)
        {
            Debug.LogWarning(
                "TestingInspectionDialogue: Dialogue Text is not assigned."
            );
            return;
        }

        activeMessage = StartCoroutine(
            TypeMessage(speaker, message)
        );
    }

    private IEnumerator TypeMessage(
        string speaker,
        string message)
    {
        string prefix =
            "<color=#FFC247><b>" +
            speaker +
            ":</b></color> ";

        dialogueText.text = prefix + message;
        dialogueText.ForceMeshUpdate();

        int prefixCharacters = speaker.Length + 2;

        dialogueText.maxVisibleCharacters =
            prefixCharacters;

        if (continuousTypingSound)
        {
            StartContinuousTypingAudio();
        }

        for (int i = 0; i < message.Length; i++)
        {
            dialogueText.maxVisibleCharacters =
                prefixCharacters + i + 1;

            char currentCharacter = message[i];

            if (!continuousTypingSound &&
                !char.IsWhiteSpace(currentCharacter))
            {
                PlaySingleTypingClick();
            }

            if (i < message.Length - 1)
            {
                yield return new WaitForSeconds(
                    characterDelay
                );
            }
        }

        StopTypingAudio();

        dialogueText.maxVisibleCharacters =
            int.MaxValue;

        yield return new WaitForSeconds(readingTime);

        dialogueText.text = "";
        activeMessage = null;
    }

    private void StartContinuousTypingAudio()
    {
        if (audioSource == null || typingSound == null)
            return;

        audioSource.Stop();
        audioSource.clip = typingSound;
        audioSource.loop = true;
        audioSource.volume = typingVolume;
        audioSource.spatialBlend = 0f;
        audioSource.Play();
    }

    private void PlaySingleTypingClick()
    {
        if (audioSource == null || typingSound == null)
            return;

        audioSource.Stop();
        audioSource.clip = typingSound;
        audioSource.loop = false;
        audioSource.volume = typingVolume;
        audioSource.spatialBlend = 0f;
        audioSource.Play();
    }

    private void StopTypingAudio()
    {
        if (audioSource == null)
            return;

        audioSource.Stop();
        audioSource.loop = false;
    }

    private void OnDisable()
    {
        if (activeMessage != null)
        {
            StopCoroutine(activeMessage);
            activeMessage = null;
        }

        StopTypingAudio();
    }
}
