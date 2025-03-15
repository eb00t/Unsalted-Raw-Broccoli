using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
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
    private Image _ctrlImg, _diedImg;
    private UpdateButton _updateButton;
    
    private void Start()
    {
        _player = GameObject.FindGameObjectWithTag("Player").transform;
        _characterMovement = _player.GetComponent<CharacterMovement>();
        _prompt = GameObject.FindGameObjectWithTag("Prompt");
        _updateButton = _prompt.GetComponent<UpdateButton>();
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
    }
    
    private void Update()
    {
        if (SceneManager.GetActiveScene().name == "Tutorial") return;
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
                TogglePrompt("", false, ControlsManager.ButtonType.ButtonEast);
                break;
            case 1:
                TogglePrompt("Pick Up Item", true, ControlsManager.ButtonType.ButtonEast);
                break;
            case > 1:
                TogglePrompt("Pick Up Items", true, ControlsManager.ButtonType.ButtonEast);
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
    
     public void TogglePrompt(string prompt, bool toggle, ControlsManager.ButtonType button)
    {
        if (toggle)
        {
            _rectTransform.anchoredPosition = new Vector3(0, 100, 0);
            _text.text = prompt;
            _updateButton.button = button;
        }
        else
        {
            _rectTransform.anchoredPosition = new Vector3(0, -100, 0);  
            _text.text = "";
        }
    }
}
