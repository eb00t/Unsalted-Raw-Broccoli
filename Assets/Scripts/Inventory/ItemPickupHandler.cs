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
    private TextMeshProUGUI _text;
    public bool isPlrNearShop;
    
    private void Start()
    {
        _player = GameObject.FindGameObjectWithTag("Player").transform;
        _characterMovement = _player.GetComponent<CharacterMovement>();
        _prompt = GameObject.FindGameObjectWithTag("Prompt");
        _rectTransform = _prompt.GetComponent<RectTransform>();
        _text = _prompt.GetComponentInChildren<TextMeshProUGUI>();
    }
    
    private void Update()
    {
        if (_characterMovement.uiOpen) return;
        if (isPlrNearShop) return;
        
        var itemCount = 0;
        
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
                _rectTransform.anchoredPosition = new Vector3(0, -200, 0);
                _text.text = "";
                break;
            case 1:
                _rectTransform.anchoredPosition = new Vector3(0, 200, 0);
                _text.text = "Pick up item [O] / Backspace";
                break;
            case > 1:
                _rectTransform.anchoredPosition = new Vector3(0, 200, 0);
                _text.text = "Pick up items [O] / Backspace";
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
    
    /*
     public void TogglePrompt(string prompt, bool toggle)
    {
        _rectTransform.anchoredPosition = new Vector3(0, -200, 0);
        _text.text = prompt;
    }
     */
}
