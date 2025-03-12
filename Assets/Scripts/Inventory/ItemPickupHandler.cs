using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class ItemPickupHandler : MonoBehaviour
{
    private Transform _player;
    private CharacterMovement _characterMovement;
    private GameObject _prompt;
    private RectTransform _rectTransform;
    private TextMeshProUGUI _text, _controlTxt;
    public bool isPlrNearShop, forceControlScheme;
    public ControlScheme currentControl;
    private bool _isGamepad;
    public int itemCount;
    public enum ControlScheme
    {
        None,
        Xbox,
        Playstation,
        Keyboard
    }
    
    private void Start()
    {
        _player = GameObject.FindGameObjectWithTag("Player").transform;
        _characterMovement = _player.GetComponent<CharacterMovement>();
        _prompt = GameObject.FindGameObjectWithTag("Prompt");
        _rectTransform = _prompt.GetComponent<RectTransform>();

        foreach (var txt in _prompt.GetComponentsInChildren<TextMeshProUGUI>())
        {
            switch (txt.name)
            {
                case "Txt":
                    _text = txt;
                    break;
                case "ControlTxt":
                    _controlTxt = txt;
                    break;
            }
        }
        
        CheckControl();
    }
    
    private void Update()
    {
        if (_characterMovement.uiOpen) return;
        if (isPlrNearShop) return;
        
        itemCount = 0;
        
        foreach (var item in GameObject.FindGameObjectsWithTag("Item"))
        {
            var ip = item.GetComponent<ItemPickup>();
            if (ip == null) continue;
            if (!ip.canPickup) continue;
            itemCount++;
        }
        
        switch (itemCount)
        {
            case 0:
                TogglePrompt("", false, "", "", "");
                break;
            case 1:
                TogglePrompt("Pick Up Item", true, "F", "B", "[]");
                break;
            case > 1:
                TogglePrompt("Pick Up Items", true, "F", "B", "[]");
                break;
        }
        
        if (Gamepad.current != null && Gamepad.current.wasUpdatedThisFrame)
        {
            _isGamepad = true;
        }
        else if (Keyboard.current != null && Keyboard.current.wasUpdatedThisFrame)
        {
            _isGamepad = false;
        }
    }

    public void PickUpItem(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        if (_characterMovement.uiOpen) return;
        
        foreach (var item in GameObject.FindGameObjectsWithTag("Item"))
        {
            var ip = item.GetComponent<ItemPickup>();
            
            if (ip == null) continue;
            if (ip.canPickup)
            {
                ip.AddItemToInventory();
            }
        }
    }
    
     public void TogglePrompt(string prompt, bool toggle, string ctrlKeyboard, string ctrlXbox, string ctrlPS)
    {
        if (toggle)
        {
            _rectTransform.anchoredPosition = new Vector3(0, 100, 0);
            _text.text = prompt;
            CheckControl();

            switch (currentControl)
            {
                case ControlScheme.None:
                case ControlScheme.Xbox:
                    _controlTxt.text = ctrlXbox;
                    break;
                case ControlScheme.Playstation:
                    _controlTxt.text = ctrlPS;
                    break;
                case ControlScheme.Keyboard:
                    _controlTxt.text = ctrlKeyboard;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        else
        {
            _rectTransform.anchoredPosition = new Vector3(0, -100, 0);  
            _text.text = "";
        }
    }

    public void CheckControl()
    {
        if (forceControlScheme) return;
        
        if (Gamepad.current != null && _isGamepad)
        {
            var deviceName = Gamepad.current.name.ToLower();

            if (deviceName.Contains("xbox"))
            {
                currentControl = ControlScheme.Xbox;
            }
            else if (deviceName.Contains("dualshock") || deviceName.Contains("dualsense") || deviceName.Contains("playstation"))
            {
                currentControl = ControlScheme.Playstation;
            }
            else
            {
                currentControl = ControlScheme.None;
                Debug.Log("Using another gamepad: " + deviceName);
            }
        }
        if (Keyboard.current != null && !_isGamepad)
        {
            currentControl = ControlScheme.Keyboard;
        }
    }
}
