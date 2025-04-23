using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PassiveItemHandler : MonoBehaviour
{
    [Header("Item Tracking")] private int _itemSwapIndex;

    [Header("References")] [SerializeField]
    private PassiveDataBase itemDatabase;

    [SerializeField] private DataHolder dataHolder;
    [SerializeField] private GameObject itemPrefab;
    [SerializeField] private Transform itemContainer;
    [SerializeField] private GameObject infoPopup;
    [SerializeField] private TextMeshProUGUI infoTitle;
    [SerializeField] private TextMeshProUGUI infoDesc;
    [SerializeField] private GameObject companionCamera;
    private GameObject _player;
    private CharacterAttack _characterAttack;
    private ItemPickupHandler _itemPickupHandler;

    private void Start()
    {
        _player = GameObject.FindGameObjectWithTag("Player");
        _characterAttack = _player.GetComponentInChildren<CharacterAttack>();
        _itemPickupHandler = _player.GetComponent<ItemPickupHandler>();
        ClearPassiveGUI();
        LoadPassives();
    }

    private void TriggerInfoPopup(PermanentPassiveItem passiveItem)
    {
        infoPopup.SetActive(true);
        infoTitle.text = passiveItem.title;
        infoDesc.text = passiveItem.description;
        _itemPickupHandler.TogglePrompt("Close", true, ControlsManager.ButtonType.Back, "", null);
    }

    // checks each slot of passive items, if a free slot is found then equip the new passive there
    // if all slots are full get the item that has been equipped the longest
    // item swap index is increased to keep track of which item is the oldest
    public void AddNewPassive(PermanentPassiveItem passiveItem) // fix this to make sure duplicates are always caught
    {
        for (var i = 0; i < dataHolder.permanentPassiveItems.Length; i++)
        {
            if (dataHolder.permanentPassiveItems[i] == 0 || dataHolder.permanentPassiveItems[i] == passiveItem.itemID)
            {
                dataHolder.permanentPassiveItems[i] = passiveItem.itemID;
                ClearPassiveGUI();
                LoadPassives();
                TriggerInfoPopup(passiveItem);
                return; // stops method early if a free slot is found
            }
        }
        
        dataHolder.permanentPassiveItems[_itemSwapIndex] = passiveItem.itemID;
        _itemSwapIndex++;
        
        ClearPassiveGUI();
        LoadPassives();
        
        if (_itemSwapIndex == dataHolder.permanentPassiveItems.Length - 1)
        {
            _itemSwapIndex = 0;
        }
    }

    public void RemovePassive(PermanentPassiveItem passiveItem)
    {
        for (var i = 0; i < dataHolder.permanentPassiveItems.Length; i++)
        {
            if (dataHolder.permanentPassiveItems[i] == passiveItem.itemID)
            {
                dataHolder.permanentPassiveItems[i] = 0;
                break;
            }
        }
        
        ClearPassiveGUI();
        LoadPassives();
    }

    // destroy all ui elements in passive item container ui object before refreshing
    private void ClearPassiveGUI()
    {
        foreach (var index in itemContainer.GetComponentsInChildren<IndexHolder>())
        {
            foreach (var passiveItem in itemDatabase.allPassives)
            {
                ResetPassiveToDefault(passiveItem);
            }

            Destroy(index.gameObject);
        }
    }

    // add image of passive to ui element based on what passives are in dataholder
    private void LoadPassives()
    {
        foreach (var itemID in dataHolder.permanentPassiveItems)
        {
            var item = FindPassive(itemID);
            if (item == null) continue;
            
            var newPassive = Instantiate(itemPrefab, itemContainer);
            var indexHolder = newPassive.GetComponent<IndexHolder>();
            indexHolder.permanentPassiveItem = item;
            ActivatePassiveEffect(item);

            foreach (var img in newPassive.GetComponentsInChildren<Image>())
            {
                if (img.name == "Icon")
                {
                    img.sprite = item.uiIcon;
                }
            }
        }
    }

    private void ResetPassiveToDefault(PermanentPassiveItem passiveItem)
    {
        switch (passiveItem.passiveEffect)
        {
            case PassiveEffect.None:
                break;
            case PassiveEffect.DefenseIncrease:
                dataHolder.playerDefense = 0;
                break;
            case PassiveEffect.AttackIncrease:
                dataHolder.playerBaseAttack = 10;
                break;
            case PassiveEffect.HpChanceOnKill:
                dataHolder.hpChanceOnKill = false;
                break;
            case PassiveEffect.SurviveLethalHit:
                dataHolder.surviveLethalHit = false;
                break;
            case PassiveEffect.PassiveEnergyRegen:
                dataHolder.passiveEnergyRegen = false;
                break;
            case PassiveEffect.Companion:
                companionCamera.SetActive(false);
                break;
            case PassiveEffect.LuckIncrease:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    // depending on the passives effect change things for the player (i.e. if the effect is more health then give more health)
    private void ActivatePassiveEffect(PermanentPassiveItem passiveItem)
    {
        switch (passiveItem.passiveEffect)
        {
            case PassiveEffect.None:
                Debug.LogWarning("Permanent passive item has no effect set.");
                break;
            case PassiveEffect.DefenseIncrease: // increases player defense (negates damage by percentage)
                dataHolder.playerDefense = (int)passiveItem.effectAmount;
                break;
            case PassiveEffect.PassiveEnergyRegen: // energy regenerates slowly over time
                dataHolder.passiveEnergyRegen = true;
                break;
            case PassiveEffect.Companion:
                companionCamera.SetActive(true);
                break;
            case PassiveEffect.AttackIncrease: // increases base attack by percentage
                var newAttack = (float)dataHolder.playerBaseAttack / 100 * passiveItem.effectAmount;
                dataHolder.playerBaseAttack += (int)newAttack;
                break;
            case PassiveEffect.HpChanceOnKill:
                dataHolder.hpChanceOnKill = true;
                break;
            case PassiveEffect.SurviveLethalHit: // allows player to survive death once
                dataHolder.surviveLethalHit = true;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    // gets the PermanentPassiveItem script based on itemID
    public PermanentPassiveItem FindPassive(int itemID)
    {
        return itemDatabase.allPassives.FirstOrDefault(item => item.itemID == itemID);
    }
}
