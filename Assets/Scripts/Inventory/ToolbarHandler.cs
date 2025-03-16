using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    [SerializeField] private Image toolbarImg;
    [SerializeField] private TextMeshProUGUI toolbarTxt, infoTxt, numHeldTxt, infoTitle;
    
    private InventoryStore _inventoryStore;
    private GameObject _player;
    private CharacterAttack _characterAttack;
    private MenuHandler _menuHandler;
    private CharacterMovement _characterMovement;
    private CurrencyManager _currencyManager;
    private PlayerStatus _playerStatus;
    private GameObject _lastSelected;
    public bool isInfoOpen;
    private List<int> _activeAtkBuffs;
    
    [SerializeField] private DataHolder dataHolder;

    private void Start()
    {
        _inventoryStore = GetComponent<InventoryStore>();
        _player = GameObject.FindGameObjectWithTag("Player");
        _characterAttack = _player.GetComponentInChildren<CharacterAttack>();
        _characterMovement = _player.GetComponent<CharacterMovement>();
        _menuHandler = GetComponent<MenuHandler>();
        _currencyManager = GetComponent<CurrencyManager>();
        _playerStatus = GetComponent<PlayerStatus>();
        _activeAtkBuffs = new List<int>();
    }

    // triggered when a player clicks on an item when browsing the inventory menu
    public void InvItemSelected(IndexHolder indexHolder)
    {
        // gets the consumable script on gameobject, gets the image and title, calls method to add inv item to toolbar
        var consumable = indexHolder.consumable;

        AddToToolbar(consumable);
        
        _menuHandler.ToggleEquip(); // once item is added go back to equip menu (slots gameobject)
    }

    // adds consumable to equip menu slot
    public void AddToToolbar(Consumable consumable)
    {
        // checks if any of the slots already contain the consumable being added and if so it removes it (makes moving items around easier)
        foreach (var slot in slots)
        {
            if (slot.GetComponent<IndexHolder>().consumable == null) continue;
            
            if (slot.GetComponent<IndexHolder>().consumable.title == consumable.title)
            {
                slot.GetComponent<IndexHolder>().consumable = null;
            }
        }
        
        if (dataHolder.isAutoEquipEnabled && !_characterMovement.uiOpen)
        {
            // adds consumable to the first free slot found due to auto equip
            foreach (var slot in slots)
            {
                if (slot.GetComponent<IndexHolder>().consumable == null)
                {
                    slot.GetComponent<IndexHolder>().consumable = consumable;
                    break;
                }
            }
        }
        else
        {
            // adds consumable to specific slot when player selects it
            slots[slotNo].GetComponent<IndexHolder>().consumable = consumable;
        }

        // checks each slots consumable and updates their image and title
        foreach (var s in slots) 
        {
            foreach (var img in s.GetComponentsInChildren<Image>())
            {
                var con = s.GetComponent<IndexHolder>().consumable;
                if (con == null) continue;
                
                if (img.name == "Image")
                {
                    img.sprite = con.uiIcon;
                    img.enabled = true;
                }

                foreach (var t in s.GetComponentsInChildren<TextMeshProUGUI>())
                {
                    if (t.name == "title")
                    {
                        t.text = con.title;
                    }
                }
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
                _menuHandler.ToggleEquip();
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
        
        UseItemEffect(equippedConsumables[_activeConsumable]);
        
        _inventoryStore.UpdateItemsHeld(equippedConsumables[_activeConsumable]);
        
        UpdateCurrentTool();
    }

    public void UseItemEffect(Consumable consumable)
    {
        switch (consumable.consumableEffect)
        {
            case ConsumableEffect.None:
                Debug.LogWarning("Item has no effect assigned.");
                break;
            case ConsumableEffect.Heal: // Heals player by a percentage of their maximum health
                var newHealth = (float)_characterAttack.maxHealth / 100 * consumable.effectAmount;
                _characterAttack.TakeDamagePlayer((int)-newHealth);
                break;
            case ConsumableEffect.GiveCurrency: // gives the player money
                _currencyManager.UpdateCurrency((int)consumable.effectAmount);
                break;
            case ConsumableEffect.Poison: //  attacks have a chance to proc poison on enemies
                StartCoroutine(ActivateStatusEffect(consumable));
                _playerStatus.AddNewStatus(consumable);
                break;
            case ConsumableEffect.Ice: // attacks have a chance to freeze enemies for a time
                StartCoroutine(ActivateStatusEffect(consumable));
                _playerStatus.AddNewStatus(consumable);
                break;
            case ConsumableEffect.DamageBuff: // provides the player a non-stackable attack buff
                StartCoroutine(ActivateAtkBuff(consumable));
                break;
            case ConsumableEffect.Invincibility: // gives player up to 3 hits without taking damage
                if (_characterAttack.isInvincible >= 3) return;
                _characterAttack.isInvincible += (int)consumable.effectAmount;
                consumable.statusText = _characterAttack.isInvincible.ToString();
                _playerStatus.AddNewStatus(consumable);
                break;
            case ConsumableEffect.RouletteHeal: // ?
                Debug.LogWarning("Roulette heal has no effect.");
                break;
            case ConsumableEffect.HorseFact: // enemy deaths in vicinity have a chance to show fact about a horse
                Debug.LogWarning("Horse fact has no effect.");
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private IEnumerator ActivateAtkBuff(Consumable consumable)
    {
        var atkIncrease = (int)((float)_characterAttack.baseAtk / 100 * consumable.effectAmount); // converts percentage to value

        if (_activeAtkBuffs.Count > 0)
        {
            if (_activeAtkBuffs.Contains(atkIncrease))
            {
                _playerStatus.AddNewStatus(consumable);
                yield break;
            }
        }
        
        _activeAtkBuffs.Add(atkIncrease);
        _characterAttack.charAtk = _characterAttack.baseAtk + _activeAtkBuffs.Sum();
        _playerStatus.AddNewStatus(consumable);
        
        yield return new WaitForSecondsRealtime(consumable.effectDuration);
        
        _activeAtkBuffs.Remove(atkIncrease);
        _characterAttack.charAtk = _activeAtkBuffs.Sum() + _characterAttack.baseAtk;
    }

    private IEnumerator ActivateStatusEffect(Consumable consumable)
    {
        AudioManager.Instance.PlayOneShot(FMODEvents.Instance.ItemActivate, transform.position);
        switch (consumable.consumableEffect)
        {
            case ConsumableEffect.Ice:
                _characterAttack.isIce = true;
                yield return new WaitForSecondsRealtime(consumable.effectDuration);
                _characterAttack.isIce = false;
                break;
            case ConsumableEffect.Poison:
                _characterAttack.isPoison = true;
                yield return new WaitForSecondsRealtime(consumable.effectDuration);
                _characterAttack.isPoison = false;
                break;
        }
    }

    private void CycleToolbar(int direction) // -1 = left, 1 = right
    { 
        // (equippedConsumables.Any(t => t == null)) return; 
        if (equippedConsumables.Count == 0) return; // if no consumables are equipped do nothing
        
        AudioManager.Instance.PlayOneShot(FMODEvents.Instance.CycleItem, transform.position);
        
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
        infoTitle.text = indexHolder.consumable.title;
        numHeldTxt.text = indexHolder.numHeld.ToString();
        //infoImg.sprite = indexHolder.consumable.uiIcon;
    }
}