
using UnityEngine;
using UnityEngine.Events;
using TMPro;

public class TestingPipelineMeasurements : MonoBehaviour
{
    [Header("Task 5 References")]
    public Animator pressureGaugeAnimator;

    [Header("Instrument Models - NEW")]
    public GameObject pressureGaugeModel;
    public GameObject temperatureGaugeModel;

    [Header("Pressure Reading")]
    public GameObject pressureDisplay;
    public TMP_Text pressureText;

    [Header("Temperature Reading")]
    public GameObject temperatureDisplay;
    public TMP_Text temperatureText;

    [Header("Simulated Measurements")]
    public string pressureReading = "150 psi";
    public string temperatureReading = "6.1 °C";

    [Header("Assessment Interface")]
    public GameObject readingsReportPanel;

    [Header("Animation")]
    public string pressureAnimationState = "Scene";
    public bool startPressureAnimationOnInteraction = true;

    [Header("Instrument Progress")]
    [SerializeField] private bool taskStarted;
    [SerializeField] private bool pressureInspected;
    [SerializeField] private bool temperatureInspected;

    [Header("Task Events")]
    public UnityEvent onTaskStarted;
    public UnityEvent onPressureInspected;
    public UnityEvent onTemperatureInspected;
    public UnityEvent onBothInstrumentsInspected;

    public bool PressureInspected => pressureInspected;
    public bool TemperatureInspected => temperatureInspected;

    private bool readyEventSent;

    // ======================================
    // INITIAL SETUP
    // ======================================

    private void Start()
    {
        taskStarted = false;
        pressureInspected = false;
        temperatureInspected = false;
        readyEventSent = false;

        // Prevent the pressure gauge animation
        // from playing before Task 5.
        if (pressureGaugeAnimator != null)
        {
            pressureGaugeAnimator.enabled = false;
        }

        // Hide both instrument readings.
        HideReadings();

        // Hide both complete instrument models.
        HideInstrumentModels();

        // Keep the assessment hidden initially.
        if (readingsReportPanel != null)
        {
            readingsReportPanel.SetActive(false);
        }

        Debug.Log(
            "[TESTING] Task 5 prepared. " +
            "Both instrument models and readings are hidden."
        );
    }

    // ======================================
    // HIDE INSTRUMENT MODELS
    // ======================================

    private void HideInstrumentModels()
    {
        if (pressureGaugeModel != null)
        {
            pressureGaugeModel.SetActive(false);
        }

        if (temperatureGaugeModel != null)
        {
            temperatureGaugeModel.SetActive(false);
        }
    }

    // ======================================
    // SHOW INSTRUMENT MODELS
    // ======================================

    private void ShowInstrumentModels()
    {
        if (pressureGaugeModel != null)
        {
            pressureGaugeModel.SetActive(true);
        }
        else
        {
            Debug.LogWarning(
                "[TESTING] Pressure Gauge Model not assigned."
            );
        }

        if (temperatureGaugeModel != null)
        {
            temperatureGaugeModel.SetActive(true);
        }
        else
        {
            Debug.LogWarning(
                "[TESTING] Temperature Gauge Model not assigned."
            );
        }
    }

    // ======================================
    // HIDE INSTRUMENT READINGS
    // ======================================

    private void HideReadings()
    {
        if (pressureDisplay != null)
        {
            pressureDisplay.SetActive(false);
        }

        if (temperatureDisplay != null)
        {
            temperatureDisplay.SetActive(false);
        }
    }

    // ======================================
    // START MEASUREMENT TASK
    // ======================================

    public void StartMeasurementTask()
    {
        if (taskStarted)
            return;

        taskStarted = true;

        // Reveal both instruments.
        ShowInstrumentModels();

        // Keep readings hidden until touched.
        HideReadings();

        Debug.Log(
            "[TESTING] Task 5 started. " +
            "Pressure and temperature gauges are now visible."
        );

        // Invokes the engineer typing dialogue
        // connected to On Task Started.
        onTaskStarted?.Invoke();
    }

    // ======================================
    // PRESSURE GAUGE INTERACTION
    // ======================================

    public void InspectPressureGauge()
    {
        if (!taskStarted)
        {
            Debug.Log(
                "[TESTING] Complete the engineer report first."
            );
            return;
        }

        // Only count the first interaction.
        if (pressureInspected)
            return;

        pressureInspected = true;

        // Start the existing pressure gauge animation.
        if (pressureGaugeAnimator != null &&
            startPressureAnimationOnInteraction)
        {
            pressureGaugeAnimator.enabled = true;

            pressureGaugeAnimator.Play(
                pressureAnimationState,
                0,
                0f
            );

            pressureGaugeAnimator.Update(0f);
        }

        // Update the pressure reading.
        if (pressureText != null)
        {
            pressureText.text = pressureReading;
        }

        // Reveal pressure value.
        if (pressureDisplay != null)
        {
            pressureDisplay.SetActive(true);
        }

        Debug.Log(
            "[TESTING] Pressure gauge inspected. " +
            "Reading: " + pressureReading
        );

        onPressureInspected?.Invoke();

        CheckCompletion();
    }

    // ======================================
    // TEMPERATURE GAUGE INTERACTION
    // ======================================

    public void InspectTemperatureInstrument()
    {
        if (!taskStarted)
        {
            Debug.Log(
                "[TESTING] Complete the engineer report first."
            );
            return;
        }

        // Only count the first interaction.
        if (temperatureInspected)
            return;

        temperatureInspected = true;

        // Update the temperature reading.
        if (temperatureText != null)
        {
            temperatureText.text = temperatureReading;
        }

        // Reveal temperature value.
        if (temperatureDisplay != null)
        {
            temperatureDisplay.SetActive(true);
        }

        Debug.Log(
            "[TESTING] Temperature gauge inspected. " +
            "Reading: " + temperatureReading
        );

        onTemperatureInspected?.Invoke();

        CheckCompletion();
    }

    // ======================================
    // CHECK BOTH INSTRUMENTS
    // ======================================

    private void CheckCompletion()
    {
        if (!pressureInspected ||
            !temperatureInspected ||
            readyEventSent)
        {
            return;
        }

        readyEventSent = true;

        Debug.Log(
            "[TESTING] Both instruments inspected. " +
            "Opening the instrument readings report."
        );

        // Connected through the Inspector to:
        // TestingInstrumentReadingsReport.ShowReport()
        onBothInstrumentsInspected?.Invoke();
    }

    // ======================================
    // OPTIONAL MANUAL REPORT OPENING
    // ======================================

    public void OpenReadingsReport()
    {
        if (!pressureInspected ||
            !temperatureInspected)
        {
            return;
        }

        if (readingsReportPanel != null)
        {
            readingsReportPanel.SetActive(true);
        }
    }

    // ======================================
    // RESET TASK 5
    // ======================================

    public void ResetMeasurements()
    {
        taskStarted = false;
        pressureInspected = false;
        temperatureInspected = false;
        readyEventSent = false;

        // Stop the gauge animation.
        if (pressureGaugeAnimator != null)
        {
            pressureGaugeAnimator.enabled = false;
        }

        // Hide both readings.
        HideReadings();

        // Hide both models.
        HideInstrumentModels();

        // Hide the assessment if assigned.
        if (readingsReportPanel != null)
        {
            readingsReportPanel.SetActive(false);
        }

        Debug.Log(
            "[TESTING] Task 5 reset. " +
            "Both instruments are hidden."
        );
    }
}
