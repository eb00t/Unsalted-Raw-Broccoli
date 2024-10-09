using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class InventoryStore : MonoBehaviour
{
    //[SerializeField] private List<GameObject> inventory;
    [SerializeField] private Transform block;
    [SerializeField] private Transform grid;
    public List<Sprite> sprites;
    public List<ScriptableObject> items;
    private void Start()
    {
        RefreshList();
        //AddNewItem(Weapons.Sword);
    }

    private void RefreshList()
    {
        // clear existing grid TODO: this isn't very efficient but it works fine for now
        foreach (var n in grid.GetComponentsInChildren<Transform>())
        {
            if (n != grid)
            {
                Destroy(n.gameObject);
            }
        }
        
        // adds new button in inventory menu for each item in list inventory
        /*
        foreach (var g in inventory)
        {
	        var c = Instantiate(block, block.position, block.rotation, grid);
        }
        */
    }

    /*
    public void AddNewItem()
    {
        var b = Instantiate(block, block.position, block.rotation, grid); // create new inventory item
        b.GetComponent<DragDropUI>().weapons = item; // set the new object's item to correct weapon
        b.GetComponentInChildren<TextMeshProUGUI>().text = item.ToString();
        foreach (var s in sprites)
        {
            if (s.name == b.GetComponent<DragDropUI>().weapons.ToString())
            {
                var i = b.GetComponentsInChildren<Image>();
        
                foreach(var img in i)
                {
                    if(!img.GetComponent<DragDropUI>())
                    {
                        img.sprite = s;
                    }
                }
            }
        }
    }
    */
}
