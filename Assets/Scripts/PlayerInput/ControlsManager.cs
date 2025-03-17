using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class ControlsManager : MonoBehaviour
{
    private GameObject _player;
    private CharacterMovement _characterMovement;
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
    
    private void Awake()
    {
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

        if (SceneManager.GetActiveScene().name != "StartScreen")
        {
            if (_characterMovement.uiOpen && !diedScreen.activeSelf)
            {
                keyboardInteractBack = "Esc";
            }
            else
            {
                keyboardInteractBack = "F";
            }
        }
        else
        {
            keyboardInteractBack = "Esc";
        }
        
        if (KeyboardStrings.ContainsKey(ButtonType.ButtonEast))
        {
            KeyboardStrings[ButtonType.ButtonEast] = keyboardInteractBack;
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
                Debug.Log("Using another gamepad: " + deviceName);
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
            { ButtonType.ButtonNorth, Y },
            { ButtonType.ButtonEast, B },
            { ButtonType.ButtonSouth, A },
            { ButtonType.ButtonWest, xboxX },
            { ButtonType.LShoulder, LB },
            { ButtonType.RShoulder, RB },
            { ButtonType.LTrigger, LT },
            { ButtonType.RTrigger, RT },
            { ButtonType.DpadNorth, dPadUp },
            { ButtonType.DpadEast, dPadRight },
            { ButtonType.DpadSouth, dPadDown },
            { ButtonType.DpadWest, dPadLeft },
            { ButtonType.Start, menuImg },
            { ButtonType.Select, select },
            { ButtonType.LThumbstick, lThumbstick},
            { ButtonType.RThumbstick, rThumbstick},
            { ButtonType.LThumbstickDown, lThumbstickDown},
            { ButtonType.RThumbstickDown, rThumbstickDown}
        };

        PlaystationSprites = new Dictionary<ButtonType, Sprite>
        {
            { ButtonType.ButtonNorth, triangle },
            { ButtonType.ButtonEast, circle },
            { ButtonType.ButtonSouth, psX },
            { ButtonType.ButtonWest, square },
            { ButtonType.LShoulder, L1 },
            { ButtonType.RShoulder, R1 },
            { ButtonType.LTrigger, L2 },
            { ButtonType.RTrigger, R2 },
            { ButtonType.DpadNorth, dPadUp },
            { ButtonType.DpadEast, dPadRight },
            { ButtonType.DpadSouth, dPadDown },
            { ButtonType.DpadWest, dPadLeft },
            { ButtonType.Start, menuImg },
            { ButtonType.Select, create },
            { ButtonType.LThumbstick, lThumbstick},
            { ButtonType.RThumbstick, rThumbstick},
            { ButtonType.LThumbstickDown, L3},
            { ButtonType.RThumbstickDown, R3}
        };
        
        KeyboardStrings = new Dictionary<ButtonType, string>
        {
            { ButtonType.ButtonNorth, "Q" },
            { ButtonType.ButtonEast, "Esc" },
            { ButtonType.ButtonSouth, "Space" },
            { ButtonType.ButtonWest, "E" },
            { ButtonType.LShoulder, "-" },
            { ButtonType.RShoulder, "Shift" },
            { ButtonType.LTrigger, "-" },
            { ButtonType.RTrigger, "-" },
            { ButtonType.DpadNorth, "W" },
            { ButtonType.DpadEast, "S" },
            { ButtonType.DpadSouth, "I" },
            { ButtonType.DpadWest, "I" },
            { ButtonType.Start, "Esc" },
            { ButtonType.Select, "-" },
            { ButtonType.LThumbstick, "A/D"},
            { ButtonType.RThumbstick, "1/2"},
            { ButtonType.LThumbstickDown, "CTRL"},
            { ButtonType.RThumbstickDown, "R"}
        };
    }
    
    public enum ButtonType
    {
        LShoulder, RShoulder,
        LTrigger, RTrigger,
        LThumbstick, RThumbstick, LThumbstickDown, RThumbstickDown,
        DpadNorth, DpadEast, DpadSouth, DpadWest,
        ButtonNorth, ButtonEast, ButtonSouth, ButtonWest,
        Start, Select
    }
}