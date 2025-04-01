using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FMOD.Studio;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class ToolbarHandler : MonoBehaviour
{
    [Header("Navigation Tracking")] 
    public int slotNo;
    private int _activeConsumable;

    [Header("Item Tracking")] [SerializeField]
    private List<GameObject> slots;

    [SerializeField] private DataHolder dataHolder;

    [Header("UI References")] [SerializeField]
    private GameObject grid;

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
    private EventInstance _cycleInstance;
    private HorseFacts _horseFacts;
    private int[] _lastEquippedItems;

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
        _horseFacts = GetComponent<HorseFacts>();
        //UpdateActiveConsumables();
        //UpdateSlots();
        UpdateToolbar();
    }
    

    // triggered when a player clicks on an item when browsing the inventory menu
    public void InvItemSelected(Consumable consumable)
    {
        AddToToolbar(consumable);
        _menuHandler.ToggleEquip();
    }
    
    // adds consumable to equip menu slot, if the consumable exists in a slot already it removes it from that slot
    public void AddToToolbar(Consumable consumable)
    {
        for (var i = 0; i < dataHolder.equippedConsumables.Length; i++)
        {
            if (dataHolder.equippedConsumables[i] != consumable.itemID) continue;
            
            dataHolder.equippedConsumables[i] = 0;
            break;
        }
        
        var index = -1;

        if (dataHolder.isAutoEquipEnabled && !_characterMovement.uiOpen)
        {
            for (var i = 0; i < dataHolder.equippedConsumables.Length; i++)
            {
                if (dataHolder.equippedConsumables[i] > 0) continue;
                
                index = i;
                break;
            }
        }
        else
        {
            index = slotNo;
        }

        if (index >= 0 && index < dataHolder.equippedConsumables.Length)
        {
            dataHolder.equippedConsumables[index] = consumable.itemID;
        }

        UpdateToolbar();
    }

    
    // updates the text and images of slots
    private void UpdateSlotUI(GameObject slot, Consumable consumable)
    {
        foreach (var img in slot.GetComponentsInChildren<Image>())
        {
            if (img.name != "Image") continue;
            img.sprite = consumable?.uiIcon;
            img.enabled = consumable != null;
        }

        foreach (var text in slot.GetComponentsInChildren<TextMeshProUGUI>())
        {
            if (text.name != "title") continue;
            text.text = consumable != null ? consumable.title : "";
        }
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
        if (dataHolder.equippedConsumables == null || _activeConsumable < 0 || _activeConsumable >= dataHolder.equippedConsumables.Length) return;
        Debug.Log(_activeConsumable);

        var itemID = dataHolder.equippedConsumables[_activeConsumable];
        if (itemID <= 0) return;

        var consumable = _inventoryStore.FindConsumable(itemID);
        if (consumable == null) return;

        var itemIndex = dataHolder.savedItems.IndexOf(itemID);
        
        if (itemIndex >= 0)
        {
            dataHolder.savedItemCounts[itemIndex]--;
            
            if (dataHolder.savedItemCounts[itemIndex] <= 0)
            {
                dataHolder.savedItems.RemoveAt(itemIndex);
                dataHolder.savedItemCounts.RemoveAt(itemIndex);
                dataHolder.equippedConsumables[_activeConsumable] = 0;
            }
        }
        
        _inventoryStore.UpdateItemsHeld(consumable);
        UseItemEffect(consumable);
        //UpdateSlots();
        UpdateToolbar();
    }

    // updates the number of the specified item id held in dataholder
    public void UseItemEffect(Consumable consumable)
    {
        switch (consumable.consumableEffect)
        {
            case ConsumableEffect.None:
                Debug.LogWarning("Item has no effect assigned.");
                break;
            case ConsumableEffect.Heal: // Heals player by a percentage of their maximum health
                var newHealth = (float)_characterAttack.maxHealth / 100 * consumable.effectAmount;
                _characterAttack.TakeDamagePlayer((int)-newHealth, 0);
                break;
            case ConsumableEffect.GiveCurrency: // gives the player money
                AudioManager.Instance.PlayOneShot(FMODEvents.Instance.CurrencyPickup, transform.position);
                _currencyManager.UpdateCurrency((int)consumable.effectAmount);
                break;
            case ConsumableEffect.Poison: //  attacks have a chance to proc poison on enemies
                AudioManager.Instance.PlayOneShot(FMODEvents.Instance.ItemActivate, transform.position);
                StartCoroutine(ActivateStatusEffect(consumable));
                _playerStatus.AddNewStatus(consumable);
                break;
            case ConsumableEffect.Ice: // attacks have a chance to freeze enemies for a time
                AudioManager.Instance.PlayOneShot(FMODEvents.Instance.ItemActivate, transform.position);
                StartCoroutine(ActivateStatusEffect(consumable));
                _playerStatus.AddNewStatus(consumable);
                break;
            case ConsumableEffect.DamageBuff: // provides the player a non-stackable attack buff
                AudioManager.Instance.PlayOneShot(FMODEvents.Instance.ItemActivate, transform.position);
                StartCoroutine(ActivateAtkBuff(consumable));
                break;
            case ConsumableEffect.Invincibility: // gives player up to 3 hits without taking damage
                if (_characterAttack.isInvincible >= 3) return;
                AudioManager.Instance.PlayOneShot(FMODEvents.Instance.ItemActivate, transform.position);
                _characterAttack.isInvincible += (int)consumable.effectAmount;
                consumable.statusText = _characterAttack.isInvincible.ToString();
                _playerStatus.AddNewStatus(consumable);
                break;
            case ConsumableEffect.RouletteHeal: // Has a chance to either heal or harm the player (cannot kill them)
                AudioManager.Instance.PlayOneShot(FMODEvents.Instance.ItemActivate, transform.position);
                Debug.LogWarning("Roulette heal has no effect.");
                break;
            case ConsumableEffect.HorseFact: // shows a fact about a horse
                _inventoryStore.TriggerNotification(consumable.uiIcon, _horseFacts.HorseFact(), true);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private IEnumerator ActivateAtkBuff(Consumable consumable)
    {
        var atkIncrease =
            (int)((float)_characterAttack.baseAtk / 100 * consumable.effectAmount); // converts percentage to value

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

    // cycles through the toolbar and triggers sound effect when direction changed
    private void CycleToolbar(int direction)
    {
        var count = dataHolder.equippedConsumables.Length;
        if (count == 0 || dataHolder.equippedConsumables.All(id => id <= 0)) return;

        _cycleInstance = AudioManager.Instance.CreateEventInstance(FMODEvents.Instance.CycleItem);

        var startIndex = _activeConsumable;

        do { _activeConsumable = (_activeConsumable + direction + count) % count; }
        while (dataHolder.equippedConsumables[_activeConsumable] <= 0 && _activeConsumable != startIndex);

        AudioManager.Instance.SetEventParameter(_cycleInstance, "Cycle Direction", direction > 0 ? 0 : 1);

        _cycleInstance.start();
        _cycleInstance.release();

        UpdateCurrentTool();
    }

    // updates the sprite image, text and if image should be enabled or disabled (to prevent a white box appearing)
    private void UpdateCurrentTool()
    {
        if (dataHolder.equippedConsumables == null || dataHolder.equippedConsumables.Length == 0)
        {
            toolbarImg.enabled = false;
            toolbarTxt.text = "-";
            return;
        }

        var itemID = dataHolder.equippedConsumables[_activeConsumable];

        if (itemID <= 0)
        {
            toolbarImg.enabled = false;
            toolbarTxt.text = "-";
            return;
        }

        var consumable = _inventoryStore.FindConsumable(itemID);
        if (consumable == null)
        {
            toolbarImg.enabled = false;
            toolbarTxt.text = "-";
            return;
        }

        toolbarImg.enabled = true;
        toolbarImg.sprite = consumable.uiIcon;

        var count = 0;
        if (dataHolder.savedItems.Contains(itemID))
        {
            count = dataHolder.savedItemCounts[dataHolder.savedItems.IndexOf(itemID)];
        }

        toolbarTxt.text = count.ToString();
    }


    // removes all items from EquippedConsumables list and adds items from indexholders in slots if not null
    public void UpdateToolbar()
    {
        if (_lastEquippedItems == null || _lastEquippedItems.Length != slots.Count) _lastEquippedItems = new int[slots.Count];

        for (var i = 0; i < slots.Count; i++)
        {
            var itemID = dataHolder.equippedConsumables.ElementAtOrDefault(i);

            if (_lastEquippedItems[i] == itemID) continue;
            
            var consumable = _inventoryStore.FindConsumable(itemID);
            UpdateSlotUI(slots[i], consumable);
            _lastEquippedItems[i] = itemID;
        }
        
        if (_activeConsumable < 0 || _activeConsumable >= dataHolder.equippedConsumables.Length || dataHolder.equippedConsumables[_activeConsumable] <= 0)
        {
            _activeConsumable = Array.FindIndex(dataHolder.equippedConsumables, id => id > 0);
            
            if (_activeConsumable == -1)
            {
                _activeConsumable = 0;
            }
        }

        UpdateCurrentTool();
    }

    // if the player inputs button west then remove the item from a slot if a slot is currently selected
    public void RemoveFromToolbar(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        if (!_characterMovement.uiOpen) return;

        for (var i = 0; i < slots.Count; i++)
        {
            if (EventSystem.current.currentSelectedGameObject != slots[i]) continue;

            dataHolder.equippedConsumables[i] = 0;
            _lastEquippedItems[i] = 0;
            UpdateSlotUI(slots[i], null);
            break;
        }

        UpdateCurrentTool();
    }

    // checks if a new inventory item is selected while the information panel is open, if so it updates
    // the information to match currently selected item by checking index holder
    private void Update()
    {
        if (!isInfoOpen) return;

        var selected = EventSystem.current.currentSelectedGameObject;
        if (selected == _lastSelected) return;

        _lastSelected = selected;
        if (selected != null) UpdateInfo(selected);
    }

    // updates the info ui text 
    private void UpdateInfo(GameObject selectedObj)
    {
        var index = slots.IndexOf(selectedObj);
        if (index < 0 || index >= dataHolder.equippedConsumables.Length) return;

        var itemID = dataHolder.equippedConsumables[index];
        var consumable = _inventoryStore.FindConsumable(itemID);
        if (consumable == null) return;

        infoTxt.text = consumable.description;
        infoTitle.text = consumable.title;

        var count = 0;

        if (itemID > 0)
        {
            var savedIndex = dataHolder.savedItems.IndexOf(itemID);
            
            if (savedIndex >= 0 && savedIndex < dataHolder.savedItemCounts.Count)
            {
                count = dataHolder.savedItemCounts[savedIndex];
            }
        }

        numHeldTxt.text = count.ToString();
    }

}