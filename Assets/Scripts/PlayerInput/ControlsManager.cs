using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class ControlsManager : MonoBehaviour
{
    private GameObject _player;
    private CharacterMovement _characterMovement;
    private GameObject _uiManager;
    private MenuHandler _menuHandler;
    [SerializeField] private GameObject diedScreen;
    [SerializeField] private DataHolder dataHolder;
    
    public Dictionary<ButtonType, Sprite> XboxSprites;
    public Dictionary<ButtonType, Sprite> PlaystationSprites;
    public Dictionary<ButtonType, string> KeyboardStrings;

    [Header("Playstation Sprites")] 
    [SerializeField] private Sprite circle;
    [SerializeField] private Sprite psX;
    [SerializeField] private Sprite triangle;
    [SerializeField] private Sprite square;
    [SerializeField] private Sprite L1, L2, L3;
    [SerializeField] private Sprite R1, R2, R3;
    [SerializeField] private Sprite create;

    [Header("XBOX Sprites")] 
    [SerializeField] private Sprite xboxX;
    [SerializeField] private Sprite Y, A, B;
    [SerializeField] private Sprite RB, LB, RT, LT;
    [SerializeField] private Sprite select;
    
    [Header("Generic Sprites")]
    [SerializeField] private Sprite menuImg;
    [SerializeField] private Sprite dPadUp, dPadDown, dPadLeft, dPadRight;
    [SerializeField] private Sprite lThumbstick, rThumbstick;
    [SerializeField] private Sprite lThumbstickDown, rThumbstickDown;

    private string keyboardInteractBack = "F";
    private string keyboardInteractSelect = "Space";
    
    private void Awake()
    {
        _uiManager = GameObject.FindWithTag("UIManager");
        _menuHandler = _uiManager.GetComponent<MenuHandler>();
        InitializeDictionaries();
    }

    private void Start()
    {
        if (SceneManager.GetActiveScene().name == "StartScreen") return;
        _player = GameObject.Find("PlayerCharacter");
        _characterMovement = _player.GetComponent<CharacterMovement>();
    }

    public enum ControlScheme
    {
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

        CheckControl();
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
            else if (deviceName.Contains("dualshock") || deviceName.Contains("dualsense") ||
                     deviceName.Contains("playstation"))
            {
                dataHolder.currentControl = ControlScheme.Playstation;
            }
            else
            {
                dataHolder.currentControl = ControlScheme.Xbox;
            }
        }

        if (Keyboard.current != null && !dataHolder.isGamepad)
        {
            dataHolder.currentControl = ControlScheme.Keyboard;
        }
    }
    
     private void InitializeDictionaries()
    {
        XboxSprites = new Dictionary<ButtonType, Sprite>
        {
            { ButtonType.Move, lThumbstick },
            { ButtonType.LightAttack, xboxX },
            { ButtonType.MediumAttack, Y },
            { ButtonType.HeavyAttack, B },
            { ButtonType.Jump, A },
            { ButtonType.Dash, RB },
            { ButtonType.CrouchL, lThumbstickDown },
            { ButtonType.CrouchR, rThumbstickDown },
            { ButtonType.Pause, menuImg },
            { ButtonType.Back, B },
            { ButtonType.CycleToolbarLeft, dPadLeft },
            { ButtonType.CycleToolbarRight, dPadRight },
            { ButtonType.UseItem, dPadUp },
            { ButtonType.RemoveItemFromToolbar, xboxX },
            { ButtonType.QuickOpenInventory, select },
            { ButtonType.Interact, RT },
            { ButtonType.LockOn, LT },
            { ButtonType.SwitchLockOnTarget, rThumbstick },
            { ButtonType.ProgressDialogue, A },
            { ButtonType.UISelect, A },
        };

        PlaystationSprites = new Dictionary<ButtonType, Sprite>
        {
            { ButtonType.Move, lThumbstick },
            { ButtonType.LightAttack, square },
            { ButtonType.MediumAttack, triangle },
            { ButtonType.HeavyAttack, circle },
            { ButtonType.Jump, psX },
            { ButtonType.Dash, R1 },
            { ButtonType.CrouchL, lThumbstickDown },
            { ButtonType.CrouchR, rThumbstickDown },
            { ButtonType.Pause, menuImg },
            { ButtonType.Back, circle },
            { ButtonType.CycleToolbarLeft, dPadLeft },
            { ButtonType.CycleToolbarRight, dPadRight },
            { ButtonType.UseItem, dPadUp },
            { ButtonType.RemoveItemFromToolbar, square },
            { ButtonType.QuickOpenInventory, create },
            { ButtonType.Interact, R2 },
            { ButtonType.LockOn, L1 },
            { ButtonType.SwitchLockOnTarget, rThumbstick },
            { ButtonType.ProgressDialogue, psX },
            { ButtonType.UISelect, psX },
        };
        
        KeyboardStrings = new Dictionary<ButtonType, string>
        {
            { ButtonType.Move, "A/D" },
            { ButtonType.LightAttack, "E / Left Click" },
            { ButtonType.MediumAttack, "W / Right Click" },
            { ButtonType.HeavyAttack, "Q / Middle Mouse Button" },
            { ButtonType.Jump, "SPACE" },
            { ButtonType.Dash, "SHIFT" },
            { ButtonType.CrouchL, "CTRL" },
            { ButtonType.CrouchR, "CTRL" },
            { ButtonType.Pause, "ESC" },
            { ButtonType.Back, "ESC" },
            { ButtonType.CycleToolbarLeft, "TAB" },
            { ButtonType.CycleToolbarRight, "TAB" },
            { ButtonType.UseItem, "1" },
            { ButtonType.RemoveItemFromToolbar, "E" },
            { ButtonType.QuickOpenInventory, "I" },
            { ButtonType.Interact, "F" },
            { ButtonType.LockOn, "R" },
            { ButtonType.SwitchLockOnTarget, "Z/X" },
            { ButtonType.ProgressDialogue, "SPACE" },
            { ButtonType.UISelect, "Enter / Left Click" },
        };
    }
    
    public enum ButtonType
    { 
        Move,
        LightAttack,
        MediumAttack,
        HeavyAttack,
        Jump,
        Dash,
        CrouchL,
        CrouchR,
        Pause,
        Back,
        CycleToolbarLeft,
        CycleToolbarRight,
        UseItem,
        RemoveItemFromToolbar,
        QuickOpenInventory,
        Interact,
        LockOn,
        SwitchLockOnTarget,
        ProgressDialogue,
        UISelect
    }
}