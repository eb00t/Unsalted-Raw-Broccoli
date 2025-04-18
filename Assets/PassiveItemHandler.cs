using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class PassiveItemHandler : MonoBehaviour
{
    [SerializeField] private PassiveDataBase itemDatabase;
    [SerializeField] private DataHolder dataHolder;
    [SerializeField] private GameObject itemPrefab;
    [SerializeField] private Transform itemContainer;
    private GameObject _player;
    private CharacterAttack _characterAttack;
    private int _itemSwapIndex = 0; 

    private void Start()
    {
        _player = GameObject.FindGameObjectWithTag("Player");
        _characterAttack = _player.GetComponentInChildren<CharacterAttack>();
        ClearPassiveGUI();
        LoadPassives();
    }

    // checks each slot of passive items, if a free slot is found then equip the new passive there
    // if all slots are full get the item that has been equipped the longest
    // item swap index is increased to keep track of which item is the oldest
    public void AddNewPassive(PermanentPassiveItem passive)
    {
        for (var i = 0; i < dataHolder.permanentPassiveItems.Length; i++)
        {
            if (dataHolder.permanentPassiveItems[i] == 0 || dataHolder.permanentPassiveItems[i] == passive.itemID)
            {
                dataHolder.permanentPassiveItems[i] = passive.itemID;
                ClearPassiveGUI();
                LoadPassives();
                return; // stops method early if a free slot is found
            }
        }
        
        dataHolder.permanentPassiveItems[_itemSwapIndex] = passive.itemID;
        _itemSwapIndex++;
        
        if (_itemSwapIndex == dataHolder.permanentPassiveItems.Length - 1)
        {
            _itemSwapIndex = 0;
        }
    }

    // destroy all ui elements in passive item container ui object before refreshing
    private void ClearPassiveGUI()
    {
        foreach (var index in itemContainer.GetComponentsInChildren<IndexHolder>())
        {
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

    // depending on the passives effect change things for the player (i.e. if the effect is more health then give more health)
    private void ActivatePassiveEffect(PermanentPassiveItem passiveItem)
    {
        switch (passiveItem.passiveEffect)
        {
            case PassiveEffect.None:
                Debug.LogWarning("Permanent passive item has no effect set.");
                break;
            case PassiveEffect.DefenseIncrease:
                _characterAttack.defense = (int)passiveItem.effectAmount;
                break;
            case PassiveEffect.PassiveEnergyRegen:
                break;
            case PassiveEffect.Companion:
                break;
            case PassiveEffect.LuckIncrease:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    // gets the PermanentPassiveItem script based on itemID
    private PermanentPassiveItem FindPassive(int itemID)
    {
        return itemDatabase.allPassives.FirstOrDefault(item => item.itemID == itemID);
    }
}
