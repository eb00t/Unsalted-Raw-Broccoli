using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using Vector2 = UnityEngine.Vector2;


// TODO: Item type that gets used as soon as its picked up
public class ToolbarHandler : MonoBehaviour
{
    [Header("Navigation Tracking")]
    public int slotNo;
    private int _activeConsumable;
    
    [Header("Item Tracking")]
    [SerializeField] private List<GameObject> slots;
    [SerializeField] private List<Consumable> equippedConsumables; // allows flexibility for cycling consumables
    
    [Header("UI References")]
    [SerializeField] private GameObject grid;
    [SerializeField] private Image toolbarImg, infoImg;
    [SerializeField] private TextMeshProUGUI toolbarTxt, infoTxt, numHeldTxt;
    
    private InventoryStore _inventoryStore;
    private GameObject _player;
    private CharacterAttack _characterAttack;
    private MenuHandler _menuHandler;
    private CharacterMovement _characterMovement;
    private GameObject _lastSelected;
    public bool isInfoOpen;

    private void Start()
    {
        _inventoryStore = GetComponent<InventoryStore>();
        _player = GameObject.FindGameObjectWithTag("Player");
        _characterAttack = _player.GetComponentInChildren<CharacterAttack>();
        _characterMovement = _player.GetComponent<CharacterMovement>();
        _menuHandler = GetComponent<MenuHandler>();
    }
    
    // triggered when a player clicks on an item when browsing the inventory menu
    public void InvItemSelected(IndexHolder indexHolder)
    {
        // gets the consumable script on gameobject, gets the image and title, calls method to add inv item to toolbar
        var consumable = indexHolder.consumable;

        AddToToolbar(consumable.uiIcon, consumable.title, consumable);
        
        _menuHandler.ToggleEquip(); // once item is added go back to equip menu (slots gameobject)
    }

    // adds consumable to equip menu slot
    private void AddToToolbar(Sprite newSprite, string txt, Consumable consumable)
    {
        foreach (var slot in slots)
        {
            if (slot.GetComponent<IndexHolder>().consumable == null) continue;
            
            if (slot.GetComponent<IndexHolder>().consumable.title == consumable.title)
            {
                slot.GetComponent<IndexHolder>().consumable = null;
            }
        }
        
        slots[slotNo].GetComponentInChildren<TextMeshProUGUI>().text = ""; // set amount held
        slots[slotNo].GetComponent<IndexHolder>().consumable = consumable; // update what consumables are equipped

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

        UpdateActiveConsumables();
    }

    // triggers when a direction on the dpad is pressed
    public void SlotItemActivated(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        // prevents player from using items when navigating the ui as dpad can be used aswell as thumbstick
        if (_characterMovement.uiOpen) return;

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

        _inventoryStore.UpdateItemsHeld(equippedConsumables[_activeConsumable]);

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
        
        UpdateCurrentTool();
    }

    private void CycleToolbar(int direction) // -1 = left, 1 = right
    { 
        // (equippedConsumables.Any(t => t == null)) return; 
        if (equippedConsumables.Count == 0) return; // if no consumables are equipped do nothing
        
        if (_activeConsumable + direction > equippedConsumables.Count - 1)
        {
            _activeConsumable = 0;
            //Debug.LogWarning("Index higher than list length, resetting to 0.");
        }
        else if (_activeConsumable + direction < 0)
        {
            _activeConsumable = equippedConsumables.Count - 1; 
            //Debug.LogWarning("Index is lower than 0, resetting to list length.");
        }
        else
        {
            if (_activeConsumable + direction > equippedConsumables.Count - 1) return;
            
            _activeConsumable += direction;
            //Debug.Log("Moving to next index");
        }
        
        UpdateCurrentTool();
    }

    // updates the sprite image, text and if image should be enabled or disabled (to prevent a white box appearing)
    private void UpdateCurrentTool()
    {
        if (equippedConsumables.Count == 0)
        {
            toolbarImg.enabled = false;
            toolbarImg.sprite = null;
            toolbarTxt.text = "-";
        }
        else
        {
            var con = equippedConsumables[_activeConsumable];
            toolbarImg.enabled = true;
            toolbarImg.sprite = con.uiIcon;

            foreach (var b in grid.GetComponentsInChildren<IndexHolder>())
            {
                if (b.consumable == con)
                {
                    toolbarTxt.text = b.numHeld.ToString();
                }
            }
        }
    }

    // removes all items from EquippedConsumables list and adds items from indexholders in slots if not null
    private void UpdateToolBar() 
    {
        equippedConsumables.Clear();

        foreach (var ac in slots)
        {
            var consumable = ac.GetComponent<IndexHolder>().consumable;
            
            if (consumable != null)
            {
                equippedConsumables.Add(consumable);
            }
        }

        _activeConsumable = 0;
        UpdateCurrentTool();
    }

    public void UpdateActiveConsumables()
    {
        // checks what consumables are in the toolbar (held in children of slots gameobject),
        // then 
        foreach (var ac in slots)
        {
            var consumable = ac.GetComponent<IndexHolder>().consumable;
            
            // compares what is in the toolbar to what items are held in inventory
            if (_inventoryStore.items.Count > 0 & consumable != null)
            {
                var isInInventory = false;
                
                foreach (var i in _inventoryStore.items)
                {
                    if (i.GetComponent<Consumable>().title == consumable.title)
                    {
                        // if the item is in the inventory then stop loop
                        isInInventory = true;
                        break;
                    }
                }
                
                // the current slot is not processed if item is in inventory, moves onto next slot
                if (isInInventory)
                {
                    continue;
                }
            }
            
            // if the checked consumable is not in the inventory then remove the consumable from the toolbar
            foreach (var s in ac.GetComponentsInChildren<Image>())
            {
                if (s.name == "Image")
                {
                    s.sprite = null;
                    s.enabled = false;
                }
            }

            ac.GetComponent<IndexHolder>().consumable = null;
        }
        
        UpdateToolBar();
    }

    public void RemoveFromToolbar(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        if (!_characterMovement.uiOpen) return;

        foreach (var slot in slots)
        {
            if (EventSystem.current.currentSelectedGameObject == slot)
            {
                slot.GetComponent<IndexHolder>().consumable = null;
            }
        }
        
        UpdateActiveConsumables();
    }

    private void Update()
    {
        if (!isInfoOpen) return;
        if (EventSystem.current.currentSelectedGameObject == _lastSelected) return;
        _lastSelected = EventSystem.current.currentSelectedGameObject;
        UpdateInfo();
    }

    private void UpdateInfo()
    {
        if (EventSystem.current.currentSelectedGameObject == null) return;
        
        var indexHolder = EventSystem.current.currentSelectedGameObject.GetComponentInChildren<IndexHolder>();

        if (indexHolder.consumable == null) return;
        
        infoTxt.text = indexHolder.consumable.description;
        numHeldTxt.text = indexHolder.numHeld.ToString();
        infoImg.sprite = indexHolder.consumable.uiIcon;
    }
}