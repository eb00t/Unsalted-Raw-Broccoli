using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ToolbarHandler : MonoBehaviour
{
    public int slotNo;
    [SerializeField] private GameObject[] slots;
    private InventoryStore _inventoryStore;

    private void Start()
    {
        _inventoryStore = GetComponent<InventoryStore>();
        foreach (var s in slots)
        {
            var n = s.GetComponentsInChildren<Image>();
            foreach (var b in n)
            {
                if (b.name == "Image")
                {
                    b.gameObject.SetActive(false);
                }
            }
        }
    }

    private void AddToToolbar(Sprite newSprite, string txt)
    {
        slots[slotNo].GetComponentInChildren<TextMeshProUGUI>().text = ""; // set amount held
        
        foreach (var s in slots[slotNo].GetComponentsInChildren<Image>())
        {
            if (s.name == "Image")
            {
                s.sprite = newSprite;
                s.GetComponentInChildren<TextMeshProUGUI>().text = txt;
                s.gameObject.SetActive(true);
            }
        }
    }
    
    public void InvItemSelected(IndexHolder indexHolder)
    {
        var consumable = _inventoryStore.items[indexHolder.InventoryIndex].GetComponent<Consumable>();
        var s = consumable.uiIcon;
        var t = consumable.title;
        
        AddToToolbar(s, t);
    }
}
