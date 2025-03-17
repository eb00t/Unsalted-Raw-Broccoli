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
    
    [Header("Item Tracking")]
    [SerializeField] private List<GameObject> slots;
    [SerializeField] private DataHolder dataHolder;

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
    private EventInstance _cycleInstance;

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
        UpdateActiveConsumables();
        UpdateSlots();
        UpdateToolBar();
    }
    
    private void UpdateSlots()
    {
        for (var i = 0; i < slots.Count; i++)
        {
            var itemID = dataHolder.equippedConsumables.Count > i ? dataHolder.equippedConsumables[i] : 0;
            var consumable = _inventoryStore.FindConsumable(itemID);

            var slot = slots[i].GetComponent<IndexHolder>();
            if (consumable != null)
            {
                slot.consumable = consumable;

                var index = dataHolder.savedItems.IndexOf(consumable.itemID);
                if (index >= 0)
                {
                    slot.numHeld = dataHolder.savedItemCounts[index];
                }
                else
                {
                    slot.numHeld = 0;
                }

                foreach (var img in slot.GetComponentsInChildren<Image>())
                {
                    if (img.name != "Image") continue;
                    img.sprite = consumable.uiIcon;
                    img.enabled = true;
                }

                foreach (var text in slot.GetComponentsInChildren<TextMeshProUGUI>())
                {
                    if (text.name != "title") continue;
                    text.text = consumable.title;
                }
            }
            else
            {
                slot.consumable = null;
                slot.numHeld = 0;

                foreach (var img in slot.GetComponentsInChildren<Image>())
                {
                    if (img.name == "Image")
                    {
                        img.sprite = null;
                        img.enabled = false;
                    }
                }

                foreach (var text in slot.GetComponentsInChildren<TextMeshProUGUI>())
                {
                    if (text.name == "title")
                    {
                        text.text = "";
                    }
                }
            }
        }
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
        foreach (var slot in slots)
        {
            var indexHolder = slot.GetComponent<IndexHolder>();
            
            if (indexHolder.consumable != null && indexHolder.consumable.itemID == consumable.itemID)
            {
                foreach (var img in slot.GetComponentsInChildren<Image>())
                {
                    if (img.name == "Image")
                    {
                        img.sprite = null;
                        img.enabled = false;
                    }
                }

                indexHolder.consumable = null;
            }
        }
        
        if (dataHolder.isAutoEquipEnabled && !_characterMovement.uiOpen)
        {
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
            slots[slotNo].GetComponent<IndexHolder>().consumable = consumable;
        }
        
        foreach (var slot in slots)
        {
            var con = slot.GetComponent<IndexHolder>().consumable;
            if (con == null) continue;

            foreach (var img in slot.GetComponentsInChildren<Image>())
            {
                if (img.name == "Image")
                {
                    img.sprite = con.uiIcon;
                    img.enabled = true;
                }
            }

            foreach (var txt in slot.GetComponentsInChildren<TextMeshProUGUI>())
            {
                if (txt.name == "title")
                {
                    txt.text = con.title;
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
        if (dataHolder.equippedConsumables.Count == 0) return;

        var itemID = dataHolder.equippedConsumables[_activeConsumable];
        if (itemID == 0) return;

        var consumable = _inventoryStore.FindConsumable(itemID);
        if (consumable == null) return;

        UseItemEffect(consumable);

        var itemIndex = dataHolder.savedItems.IndexOf(itemID);
        if (itemIndex >= 0)
        {
            dataHolder.savedItemCounts[itemIndex] -= 1;
        }

        if (dataHolder.savedItemCounts[itemIndex] <= 0)
        {
            dataHolder.equippedConsumables.RemoveAt(_activeConsumable);
            dataHolder.savedItems.RemoveAt(itemIndex);
            dataHolder.savedItemCounts.RemoveAt(itemIndex);

            if (_activeConsumable >= dataHolder.equippedConsumables.Count)
            {
                _activeConsumable = dataHolder.equippedConsumables.Count - 1;
            }

            if (_activeConsumable < 0)
            {
                toolbarImg.enabled = false;
                toolbarImg.sprite = null;
                toolbarTxt.text = "-";
            }
        }

        UpdateSlots();
        UpdateToolBar();
        _inventoryStore.UpdateItemsHeld(consumable);
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

    // cycles through the toolbar
    private void CycleToolbar(int direction)
    { 
        // (equippedConsumables.Any(t => t == null)) return; 
        if (dataHolder.equippedConsumables.Count == 0) return; // if no consumables are equipped do nothing

        _cycleInstance = AudioManager.Instance.CreateEventInstance(FMODEvents.Instance.CycleItem);
        
        if (_activeConsumable + direction > dataHolder.equippedConsumables.Count - 1)
        {
            _activeConsumable = 0;
            AudioManager.Instance.SetEventParameter(_cycleInstance, "Cycle Direction", 0);
            //Debug.LogWarning("Index higher than list length, resetting to 0.");
        }
        else if (_activeConsumable + direction < 0)
        {
            _activeConsumable = dataHolder.equippedConsumables.Count - 1; 
            AudioManager.Instance.SetEventParameter(_cycleInstance, "Cycle Direction", 1);
            //Debug.LogWarning("Index is lower than 0, resetting to list length.");
        }
        else
        {
            _activeConsumable += direction;
            //Debug.Log("Moving to next index");
            
            if (direction == 1)
            {
                AudioManager.Instance.SetEventParameter(_cycleInstance, "Cycle Direction", 0);
            }
            else
            {
                AudioManager.Instance.SetEventParameter(_cycleInstance, "Cycle Direction", 1);
            }
        }
        _cycleInstance.start();
        _cycleInstance.release();
        UpdateCurrentTool();
    }

    // updates the sprite image, text and if image should be enabled or disabled (to prevent a white box appearing)
    private void UpdateCurrentTool()
    {
        if (dataHolder.equippedConsumables.Count == 0)
        {
            toolbarImg.enabled = false;
            toolbarImg.sprite = null;
            toolbarTxt.text = "-";
        }
        else
        {
            var itemID = dataHolder.equippedConsumables[_activeConsumable];
            var consumable = _inventoryStore.FindConsumable(itemID);

            toolbarImg.enabled = true;
            toolbarImg.sprite = consumable.uiIcon;

            foreach (var b in grid.GetComponentsInChildren<IndexHolder>())
            {
                if (b.consumable == consumable)
                {
                    toolbarTxt.text = b.numHeld.ToString();
                }
            }
        }
    }

    // removes all items from EquippedConsumables list and adds items from indexholders in slots if not null
    private void UpdateToolBar() 
    {
        dataHolder.equippedConsumables.Clear();

        foreach (var ac in slots)
        {
            var consumable = ac.GetComponent<IndexHolder>().consumable;
            
            if (consumable != null)
            {
                dataHolder.equippedConsumables.Add(consumable.itemID);
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
            if (dataHolder.savedItems.Count > 0 && consumable != null)
            {
                var isInInventory = dataHolder.savedItems.Contains(consumable.itemID);
                if (!isInInventory)
                {
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
            }
        }
        
        foreach (var ac in slots)
        {
            var consumable = ac.GetComponent<IndexHolder>().consumable;

            if (consumable != null && !dataHolder.equippedConsumables.Contains(consumable.itemID))
            {
                dataHolder.equippedConsumables.Add(consumable.itemID);
            }
        }


        _activeConsumable = 0;
        UpdateCurrentTool();
    }


    public void RemoveFromToolbar(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        if (!_characterMovement.uiOpen) return;

        foreach (var slot in slots)
        {
            if (EventSystem.current.currentSelectedGameObject == slot)
            {
                var consumable = slot.GetComponent<IndexHolder>().consumable;
                if (consumable != null)
                {
                    dataHolder.equippedConsumables.Remove(consumable.itemID);
                    
                    slot.GetComponent<IndexHolder>().consumable = null;
                    
                    foreach (var img in slot.GetComponentsInChildren<Image>())
                    {
                        if (img.name == "Image")
                        {
                            img.sprite = null;
                            img.enabled = false;
                        }
                    }
                }
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
