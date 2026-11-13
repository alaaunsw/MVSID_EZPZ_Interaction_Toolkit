
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class EZPZCalculationInput : MonoBehaviour
{
    public TMP_InputField inputField;

    public void ActivateInput()
    {
        if (inputField == null)
        {
            Debug.LogWarning("Input field not assigned.");
            return;
        }

        if (EventSystem.current == null)
        {
            Debug.LogWarning("No active EventSystem found.");
            return;
        }

        inputField.interactable = true;

        EventSystem.current.SetSelectedGameObject(
            inputField.gameObject
        );

        inputField.Select();
        inputField.ActivateInputField();

        Debug.Log("Calculation input activated.");
    }
}
