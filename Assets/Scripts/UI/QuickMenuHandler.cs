using System;
using System.Collections;
using System.Collections.Generic;
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

    public void AddToQuickMenu()
    {
	    
    }
}
