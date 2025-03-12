using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CheckControls : MonoBehaviour
{
    private bool _isGamepad;
    [SerializeField] private GameObject xboximg, psimg, keyimg;

    private void Update()
    {
        if (Gamepad.current != null && Gamepad.current.wasUpdatedThisFrame)
        {
            _isGamepad = true;
        }
        else if (Keyboard.current != null && Keyboard.current.wasUpdatedThisFrame)
        {
            _isGamepad = false;
        }
        
        if (Gamepad.current != null && _isGamepad)
        {
            var deviceName = Gamepad.current.name.ToLower();

            if (deviceName.Contains("xbox"))
            {
                xboximg.SetActive(true);
                psimg.SetActive(false);
            }
            else if (deviceName.Contains("dualshock") || deviceName.Contains("dualsense") || deviceName.Contains("playstation"))
            {
                psimg.SetActive(true);
                xboximg.SetActive(false);
            }
            else
            {
                xboximg.SetActive(true);
                psimg.SetActive(false);
            }

            keyimg.SetActive(false);
        }
        if (Keyboard.current != null && !_isGamepad)
        {
            keyimg.SetActive(true);
            xboximg.SetActive(false);
            psimg.SetActive(false);
        }
    }
}
