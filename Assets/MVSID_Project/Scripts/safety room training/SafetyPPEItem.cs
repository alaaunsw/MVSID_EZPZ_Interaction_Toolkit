
using UnityEngine;

public class SafetyPPEItem : MonoBehaviour
{
    public enum EquipmentCategory
    {
        Coverall,
        SafetyHelmet,
        SafetyGlasses,
        SafetyGloves,
        SafetyBoots,
        HearingProtection,
        GasDetector,
        Multimeter,
        Other
    }

    [Header("Equipment Information")]
    public EquipmentCategory category;

    public string equipmentName = "Safety Equipment";

    [Header("Inspection Requirements")]
    public bool requiredForInspection = false;

    [TextArea(2, 4)]
    public string correctFeedback =
        "Correct! This equipment is required.";

    [TextArea(2, 4)]
    public string incorrectFeedback =
        "This equipment is not required for this step.";

    [Header("Interaction State")]
    public bool hasBeenSelected = false;

    // Called by the EZPZ interaction event.
    public void SelectEquipment()
    {
        if (hasBeenSelected)
            return;

        hasBeenSelected = true;

        if (requiredForInspection)
        {
            Debug.Log(
                "CORRECT: " + equipmentName +
                " - " + correctFeedback
            );
        }
        else
        {
            Debug.Log(
                "NOT REQUIRED: " + equipmentName +
                " - " + incorrectFeedback
            );
        }
    }
}
