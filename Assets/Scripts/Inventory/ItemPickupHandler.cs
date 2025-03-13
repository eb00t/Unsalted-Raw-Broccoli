using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class ItemPickupHandler : MonoBehaviour
{
    private Transform _player;
    private CharacterMovement _characterMovement;
    private GameObject _prompt;
    private RectTransform _rectTransform;
    private TextMeshProUGUI _text, _controlTxt, _diedText;
    public bool isPlrNearShop;
    private bool _isGamepad;
    public int itemCount;
    private ControlsManager _controlsManager;
    [SerializeField] private DataHolder dataHolder;

    [Header("Image References")] 
    [SerializeField] private Sprite square; // for ps controller
    [SerializeField] private Sprite triangle;
    [SerializeField] private Sprite circle;
    [SerializeField] private Sprite x;
    private Image _ctrlImg, _diedImg;
    
    private void Start()
    {
        _player = GameObject.FindGameObjectWithTag("Player").transform;
        _characterMovement = _player.GetComponent<CharacterMovement>();
        _prompt = GameObject.FindGameObjectWithTag("Prompt");
        _rectTransform = _prompt.GetComponent<RectTransform>();
        _controlsManager = GameObject.FindGameObjectWithTag("UIManager").GetComponent<ControlsManager>();

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

        foreach (var img in _prompt.GetComponentsInChildren<Image>(true))
        {
            if (img.gameObject.name == "CtrlImg")
            {
                _ctrlImg = img;
            }
        }

        _controlsManager.CheckControl();
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
                TogglePrompt("Pick Up Item", true, "F", "B", "circle");
                break;
            case > 1:
                TogglePrompt("Pick Up Items", true, "F", "B", "circle");
                break;
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
            _controlsManager.CheckControl();

            switch (dataHolder.currentControl)
            {
                case ControlsManager.ControlScheme.None:
                case ControlsManager.ControlScheme.Xbox:
                    _controlTxt.text = ctrlXbox;
                    _ctrlImg.enabled = false;
                    break;
                case ControlsManager.ControlScheme.Playstation:
                    _ctrlImg.enabled = true;
                    
                    switch (ctrlPS)
                    {
                        case "square":
                            _ctrlImg.sprite = square;
                            break;
                        case "circle":
                            _ctrlImg.sprite = circle;
                            break;
                        case "x":
                            _ctrlImg.sprite = x;
                            break;
                        case "triangle":
                            _ctrlImg.sprite = triangle;
                            break;
                    }
                    
                    _controlTxt.text = "";
                    break;
                case ControlsManager.ControlScheme.Keyboard:
                    _ctrlImg.enabled = false;
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
}
