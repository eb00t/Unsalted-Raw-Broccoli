using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryStore : MonoBehaviour
{
    [SerializeField] private Transform block;
    public Transform grid;
    public List<GameObject> items;
    private ToolbarHandler _toolbarHandler;
    
    private void Start()
    {
        _toolbarHandler = GetComponent<ToolbarHandler>();
        RefreshList();
    }

    private void RefreshList()
    {
        foreach (var n in grid.GetComponentsInChildren<Transform>())
        {
            if (n != grid)
            {
                Destroy(n.gameObject);
            }
        }
        
        for (var i = 0; i < items.Count; i++)
        {
	        AddNewItem(items[i].GetComponent<Consumable>());
        }
    }
    
    public void AddNewItem(Consumable consumable)
    {
        // checks if item added exists in inventory, if so it increases the number held and returns
        if (items.Count > 0)
        {
            foreach (var x in items)
            {
                //Debug.Log(consumable.title + x.GetComponent<Consumable>().title);
                
                if (consumable.title != x.GetComponent<Consumable>().title) continue;
                
                foreach (var b in grid.GetComponentsInChildren<IndexHolder>())
                {
                    if (b == null) continue;
                    if (b.consumable.title != consumable.title) continue;
                    if (b.numHeld >= consumable.maximumHold) return;
                    
                    b.numHeld++;
                    _toolbarHandler.UpdateActiveConsumables();
                    
                    foreach (var img in b.GetComponentsInChildren<Image>())
                    {
                        if (img.name == "Image")
                        {
                            img.GetComponentInChildren<TextMeshProUGUI>().text = b.numHeld + "/" + b.consumable.maximumHold;
                        }
                    }
                    
                    consumable.gameObject.SetActive(false);
                    consumable.gameObject.GetComponent<ItemPickup>().canPickup = false;

                    return;
                }
            }
        }
        
        
        // if the item did not exist in inventory already then a new inventory button is created
        items.Add(consumable.gameObject);
        consumable.gameObject.SetActive(false);
        consumable.gameObject.GetComponent<ItemPickup>().canPickup = false;
        
        var newBlock = Instantiate(block, block.position, block.rotation, grid);
        newBlock.GetComponentInChildren<TextMeshProUGUI>().text = consumable.title;
        
        var indexHolder = newBlock.GetComponent<IndexHolder>();
        indexHolder.consumable = consumable;
        indexHolder.numHeld++;
        
        // updates the onclick for the new inventory item button
        newBlock.GetComponent<Button>().onClick.AddListener(delegate
        {
            _toolbarHandler.InvItemSelected(indexHolder);
        });

        foreach (var s in newBlock.GetComponentsInChildren<Image>())
        {
            if (s.name == "Image")
            {
                s.sprite = consumable.uiIcon;
                s.GetComponentInChildren<TextMeshProUGUI>().text = indexHolder.numHeld + "/" + indexHolder.consumable.maximumHold;
            }
        }
    }

    public void UpdateItemsHeld(Consumable consumable)
    {
        foreach (var b in grid.GetComponentsInChildren<IndexHolder>())
        {
            if (b.consumable.title != consumable.title) continue;
            if (b.numHeld - 1 <= 0)
            {
                foreach (var item in items.ToList())
                {
                    if (item.GetComponent<Consumable>().title != b.consumable.title) continue;
                    items.Remove(item);
                    Destroy(b.gameObject);
                }
            }
            else
            {
                b.numHeld--;
                
                foreach (var img in b.GetComponentsInChildren<Image>())
                {
                    if (img.name == "Image")
                    {
                        img.GetComponentInChildren<TextMeshProUGUI>().text = b.numHeld + "/" + b.consumable.maximumHold;
                    }
                }
            }
        }
        
        _toolbarHandler.UpdateActiveConsumables();
    }
}
