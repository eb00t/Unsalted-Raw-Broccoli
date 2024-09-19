using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IndexObj : MonoBehaviour
{
    public Items kind;
    [SerializeField] private GameObject popUp;
    [SerializeField] private float range;
    [SerializeField] private Transform player;
    [SerializeField] private InventoryStore inventoryStore;
    
    // Update is called once per frame
    private void Update()
    {
        var dist = Vector3.Distance(transform.position, player.position);

        if (dist <= range)
        {
            popUp.SetActive(true);
            if (Input.GetKeyDown(KeyCode.E))
            {
                inventoryStore.AddNewItem(kind);
                gameObject.SetActive(false);
            }
        }
        else
        {
            popUp.SetActive(false);
        }
    }
}
