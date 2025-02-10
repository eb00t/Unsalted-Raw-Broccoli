using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TMPro;
using Unity.VisualScripting;
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

    public void RefreshList()
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
	        AddNewItem(items[i], i);
        }
    }
    
    private void AddNewItem(GameObject item, int i)
    {
        var consumable = item.GetComponent<Consumable>();

        var newBlock = Instantiate(block, block.position, block.rotation, grid);
        newBlock.GetComponentInChildren<TextMeshProUGUI>().text = consumable.title;
        var indexHolder = newBlock.GetComponent<IndexHolder>();
        indexHolder.InventoryIndex = i;
        newBlock.GetComponent<Button>().onClick.AddListener(delegate { _toolbarHandler.InvItemSelected(indexHolder); });
        
        foreach (var s in newBlock.GetComponentsInChildren<Image>())
        {
            if (s.name == "Image")
            {
                s.sprite = consumable.uiIcon;
            }
        }
    }
}
