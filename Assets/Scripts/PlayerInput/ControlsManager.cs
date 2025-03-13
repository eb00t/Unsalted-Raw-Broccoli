using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class ControlsManager : MonoBehaviour
{
    [SerializeField] private DataHolder dataHolder;
    
    public enum ControlScheme
    {
        None,
        Xbox,
        Playstation,
        Keyboard
    }

    private void Update()
    {
        if (Gamepad.current != null && Gamepad.current.wasUpdatedThisFrame)
        {
            dataHolder.isGamepad = true;
        }
        else if (Keyboard.current != null && Keyboard.current.wasUpdatedThisFrame)
        {
            dataHolder.isGamepad = false;
        }
    }

    public void CheckControl()
    {
        if (dataHolder.forceControlScheme) return;
        
        if (Gamepad.current != null && dataHolder.isGamepad)
        {
            var deviceName = Gamepad.current.name.ToLower();

            if (deviceName.Contains("xbox"))
            {
                dataHolder.currentControl = ControlScheme.Xbox;
            }
            else if (deviceName.Contains("dualshock") || deviceName.Contains("dualsense") || deviceName.Contains("playstation"))
            {
                dataHolder.currentControl = ControlScheme.Playstation;
            }
            else
            {
                dataHolder.currentControl = ControlScheme.None;
                Debug.Log("Using another gamepad: " + deviceName);
            }
        }
        if (Keyboard.current != null && !dataHolder.isGamepad)
        {
            dataHolder.currentControl = ControlScheme.Keyboard;
        }
    }
}