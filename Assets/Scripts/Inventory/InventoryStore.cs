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
        var canPass = false;

        if (items.Count > 0)
        {
            foreach (var x in items)
            {
                Debug.Log(consumable.title + x.GetComponent<Consumable>().title);
                if (consumable.title == x.GetComponent<Consumable>().title)
                {
                    foreach (var b in grid.GetComponentsInChildren<IndexHolder>())
                    {
                        if (b != null)
                        {
                            if (b.consumable.title == consumable.title)
                            {
                                b.numHeld++;
                                return;
                            }
                        }
                    }
                }
            }
            canPass = true;
        }
        else
        {
            canPass = true;
        }

        if (!canPass) return;
        items.Add(item);
        Debug.Log("Passed check");
        var newBlock = Instantiate(block, block.position, block.rotation, grid);
        newBlock.GetComponentInChildren<TextMeshProUGUI>().text = consumable.title;
        var indexHolder = newBlock.GetComponent<IndexHolder>();
        //indexHolder.InventoryIndex = i;
        indexHolder.consumable = consumable;
        indexHolder.numHeld++;
        
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
