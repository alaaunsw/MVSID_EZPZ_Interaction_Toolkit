
using UnityEngine;

public class JoystickHighlightPulse : MonoBehaviour
{
    public float pulseSpeed = 2f;
    public float pulseAmount = 0.15f;

    private Vector3 originalScale;

    void Start()
    {
        originalScale = transform.localScale;
    }

    void Update()
    {
        float pulse = 1f + Mathf.Sin(Time.time * pulseSpeed * Mathf.PI * 2f) * pulseAmount;

        transform.localScale = originalScale * pulse;
    }
}
