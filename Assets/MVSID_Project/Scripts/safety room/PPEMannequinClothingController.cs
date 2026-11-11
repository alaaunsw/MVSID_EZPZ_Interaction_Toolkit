
using UnityEngine;

public class PPEMannequinClothingController : MonoBehaviour
{
    [Header("Mannequin PPE Models")]
    public GameObject helmetOnMannequin;

    [Header("Success Feedback")]
    public AudioSource successSound;

    private void Start()
    {
        // Mannequin starts without a helmet.
        if (helmetOnMannequin != null)
            helmetOnMannequin.SetActive(false);
    }

    // Called when trainee selects SAFETY HELMET.
    public void SelectHelmet()
    {
        if (helmetOnMannequin == null)
        {
            Debug.LogWarning(
                "Helmet_OnMannequin is not assigned."
            );
            return;
        }

        helmetOnMannequin.SetActive(true);

        if (successSound != null)
            successSound.Play();

        Debug.Log(
            "✓ Correct! The safety helmet helps protect " +
            "the head from specified impact and falling-object hazards."
        );
    }
}
