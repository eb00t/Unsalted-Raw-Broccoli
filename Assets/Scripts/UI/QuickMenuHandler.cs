using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem.Processors;
using UnityEngine.Serialization;

public class QuickMenuHandler : MonoBehaviour
{
	private bool _invOpen;
    [SerializeField] private InventoryStore inventoryStore; // to remove items used from index

    /*
     TODO:
     - method to add items
     - check item limit
     - when inventory is open switch to selectable quick menu for quick changing

    */

    private void Update()
    {
	    if (_invOpen)
	    {
		    // allow changes to .this
		    
	    }
    }

    private void AddToMenu(GameObject item)
    {
	    /*
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
	    */
    }
}
