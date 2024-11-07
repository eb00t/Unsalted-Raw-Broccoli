using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class InventoryStore : MonoBehaviour
{
    [SerializeField] private Transform block;
    [SerializeField] private Transform grid;
    public List<GameObject> items;
    
    private void Start()
    {
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
        
        foreach (var g in items)
        {
	        AddNewItem(g);
        }
    }


    private void AddNewItem(GameObject item)
    {
        var consumable = item.GetComponent<Consumable>();

        var newBlock = Instantiate(block, block.position, block.rotation, grid);
        newBlock.GetComponentInChildren<TextMeshProUGUI>().text = consumable.title;
        
        foreach (var s in newBlock.GetComponentsInChildren<Image>())
        {
            if (s.name == "Image")
            {
                s.sprite = consumable.uiIcon;
            }
        }
    }
}
