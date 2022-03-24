using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Press menu, opens menu, click button
/// </summary>
public class WristMenu : MonoBehaviour
{
    public GameObject wristUI;
    public bool active = false;
    public InputActionReference toggleReference = null;
    
    void Start()
    {
        toggleReference.action.performed += MenuPressed;
    }
    private void OnDestroy()
    {
        toggleReference.action.performed -= MenuPressed;
    }

    /// <summary>
    /// Wrapper
    /// </summary>
    /// <param name="context"></param>
    public void MenuPressed(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            DisplayWristUI();
        }
    }
    
    public void DisplayWristUI()
    {
        active = !active;
        wristUI.SetActive(active);
    }
}
