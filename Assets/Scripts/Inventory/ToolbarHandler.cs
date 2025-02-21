using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;
using Vector2 = UnityEngine.Vector2;

public class ToolbarHandler : MonoBehaviour
{
    public int slotNo;
    [SerializeField] private GameObject[] slots;
    [SerializeField] private Consumable[] activeConsumables = new Consumable[5]; // keeps track of which consumable is in what slot
    [SerializeField] private List<Consumable> equippedConsumables; // allows flexibility for cycling consumables
    [SerializeField] private int _activeConsumable = 0;
    
    private InventoryStore _inventoryStore;
    private GameObject _player;
    private CharacterAttack _characterAttack;
    private MenuHandler _menuHandler;
    [SerializeField] private Image toolbarImg;
    [SerializeField] private TextMeshProUGUI toolbarTxt;

    private void Start()
    {
        _inventoryStore = GetComponent<InventoryStore>();
        _player = GameObject.FindGameObjectWithTag("Player");
        _characterAttack = _player.GetComponentInChildren<CharacterAttack>();
        _menuHandler = GetComponent<MenuHandler>();
    }

    private void AddToToolbar(Sprite newSprite, string txt, Consumable consumable)
    {
        slots[slotNo].GetComponentInChildren<TextMeshProUGUI>().text = ""; // set amount held
        activeConsumables[slotNo] = consumable; // update what consumables are equipped

        // find image and set to consumable set sprite and consumable set title, then enable the image
        foreach (var s in slots[slotNo].GetComponentsInChildren<Image>()) 
        {
            if (s.name == "Image")
            {
                s.sprite = newSprite;
                foreach (var t in s.GetComponentsInChildren<TextMeshProUGUI>())
                {
                    if (t.name == "title")
                    {
                        t.text = txt;
                    }
                }

                s.enabled = true;
            }
        }

        /*
        if (equippedConsumables[_activeConsumable])
        {
            toolbarImg.sprite = newSprite;
            toolbarTxt.text = txt;
            toolbarImg.enabled = true;
            _activeConsumable = equippedConsumables.IndexOf(consumable);
        }
        */

        UpdateToolBar();
    }

    // triggered when a player clicks on an item when browsing the inventory menu
    public void InvItemSelected(IndexHolder indexHolder)
    {
        // gets the consumable script on gameobject, gets the image and title, calls method to add inv item to toolbar
        var consumable = _inventoryStore.items[indexHolder.InventoryIndex].GetComponent<Consumable>();
        var s = consumable.uiIcon;
        var t = consumable.title;

        AddToToolbar(s, t, consumable);
        
        _menuHandler.ToggleEquip(); // once item is added go back to equip menu (slots gameobject)
    }

    // triggers when a direction on the dpad is pressed
    public void SlotItemActivated(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        // prevents player from using items when navigating the ui as dpad can be used aswell as thumbstick
        if (_player.GetComponent<CharacterMovement>().uiOpen) return;

        var dir = context.ReadValue<Vector2>();

        // do something based on which direction is pressed
        switch (dir.x, dir.y)
        {
            case (0, 1): // up (0)
                // use equipped consumable
                CheckItemEffect();
                break;
            case (1, 0): // right (1)
                // switch to consumable +1
                CycleToolbar(1);
                break;
            case (0, -1): // down (2)
                // do nothing for now
                break;
            case (-1, 0): // left (3)
                CycleToolbar(-1);
                break;
        }
    }

    // checks a consumables effect and triggers it based on which consumable is active (num = slot active)
    private void CheckItemEffect()
    {
        if (equippedConsumables.Count <= 0 || equippedConsumables[_activeConsumable] == null) return;
        var effect = equippedConsumables[_activeConsumable].consumableEffect;

        switch (effect)
        {
            case ConsumableEffect.None:
                Debug.Log("Item has no effect assigned.");
                break;
            case ConsumableEffect.Heal: // Heals player by 50% of their maximum health
                if (_characterAttack.currentHealth + (_characterAttack.maxHealth / 2) >= _characterAttack.maxHealth)
                {
                    _characterAttack.currentHealth = _characterAttack.maxHealth;
                }
                else if (_characterAttack.currentHealth + (_characterAttack.maxHealth / 2) < _characterAttack.maxHealth)
                {
                    _characterAttack.currentHealth += _characterAttack.maxHealth / 2; 
                }
                
                _characterAttack.TakeDamagePlayer(0); // to update ui
                break;
        }
    }

    private void CycleToolbar(int direction) // -1 = left, 1 = right
    { 
        // (equippedConsumables.Any(t => t == null)) return; 
        if (equippedConsumables.Count == 0) return; // if no consumables are equipped do nothing
        
        if (_activeConsumable + direction > equippedConsumables.Count - 1)
        {
            _activeConsumable = 0;
            Debug.Log("Index higher than list length, resetting to 0.");
        }
        else if (_activeConsumable + direction < 0)
        {
            _activeConsumable = equippedConsumables.Count - 1; 
            Debug.Log("Index is lower than 0, resetting to list length.");
        }
        else
        {
            if (_activeConsumable + direction > equippedConsumables.Count - 1) return;
            
            _activeConsumable += direction;
            Debug.Log("Moving to next index");
        }
        
        UpdateCurrentTool();
    }

    private void UpdateCurrentTool()
    {
        if (equippedConsumables.Count == 0)
        {
            toolbarImg.enabled = false;
            toolbarImg.sprite = null;
            toolbarTxt.text = "";
        }
        else
        {
            var con = equippedConsumables[_activeConsumable];
            toolbarImg.enabled = true;
            toolbarImg.sprite = con.uiIcon;
            toolbarTxt.text = con.title;
        }
    }

    // removes all items from EquippedConsumables list and adds items from ActiveConsumables if not null
    private void UpdateToolBar() 
    {
        equippedConsumables.Clear();

        foreach (var ac in activeConsumables)
        {
            if (ac != null)
            {
                equippedConsumables.Add(ac);
            }
        }

        _activeConsumable = 0;
        UpdateCurrentTool();
    }
}