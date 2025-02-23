using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryStore : MonoBehaviour
{
    [SerializeField] private Transform block;
    [SerializeField] private Transform grid;
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
	        AddNewItem(items[i]);
        }
    }
    
    public void AddNewItem(GameObject item)
    {
        var consumable = item.GetComponent<Consumable>();

        // checks if item added exists in inventory, if so it increases the number held and returns
        if (items.Count > 0)
        {
            foreach (var x in items)
            {
                Debug.Log(consumable.title + x.GetComponent<Consumable>().title);
                
                if (consumable.title != x.GetComponent<Consumable>().title) continue;
                
                foreach (var b in grid.GetComponentsInChildren<IndexHolder>())
                {
                    if (b == null) continue;
                    if (b.consumable.title != consumable.title) continue;
                    
                    b.numHeld++;
                    return;
                }
            }
        }
        
        // if the item did not exist in inventory already then a new inventory button is created
        items.Add(item);
        var newBlock = Instantiate(block, block.position, block.rotation, grid);
        newBlock.GetComponentInChildren<TextMeshProUGUI>().text = consumable.title;
        
        var indexHolder = newBlock.GetComponent<IndexHolder>();
        //indexHolder.InventoryIndex = i;
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
            }
        }
    }
}
