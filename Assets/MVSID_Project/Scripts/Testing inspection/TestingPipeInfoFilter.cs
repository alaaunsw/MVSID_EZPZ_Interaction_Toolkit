
using UnityEngine;

public class TestingPipeInfoFilter : MonoBehaviour
{
    [Header("Existing Pipe Information Card")]
    public PipeFloatingInfoCard infoCard;

    // Call after the existing ShowCard() event.
    public void HideRestrictedFields()
    {
        if (infoCard == null ||
            infoCard.cardCanvas == null)
        {
            Debug.LogWarning(
                "[TESTING] Pipe information card is not assigned."
            );
            return;
        }

        Transform card = infoCard.cardCanvas.transform
            .Find("CardBackground");

        if (card == null)
        {
            Debug.LogWarning(
                "[TESTING] CardBackground not found."
            );
            return;
        }

        // Row 4: Operating Pressure
        // Row 5: Operating Temperature
        // Row 6: Condition
        for (int i = 4; i <= 6; i++)
        {
            HideChild(card, "Label" + i);
            HideChild(card, "Value" + i);
        }

        // Hide dividers for the removed rows.
        HideChild(card, "RowDivider3");
        HideChild(card, "RowDivider4");
        HideChild(card, "RowDivider5");

        Debug.Log(
            "[TESTING] Pressure, temperature and condition hidden."
        );
    }

    private void HideChild(
        Transform parent,
        string childName)
    {
        Transform child = parent.Find(childName);

        if (child != null)
            child.gameObject.SetActive(false);
    }
}
