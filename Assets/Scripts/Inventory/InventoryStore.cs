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
    //public List<Sprite> sprites;
    public List<GameObject> items;
    private void Start()
    {
        RefreshList();
        AddNewItem(items[0]);
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


    private void AddNewItem(GameObject itemA)
    {
        var s = itemA.GetComponent<WeaponHandler>();
        var sprite = s.GetComponent<SpriteRenderer>().sprite;
        
        var b = Instantiate(block, block.position, block.rotation, grid);
        b.GetComponentInChildren<Image>().sprite = sprite;
        b.GetComponentInChildren<TextMeshProUGUI>().text = s.title;

        /*
        foreach (var s in sprites)
        {
            if (s.name == b.GetComponent<DragDropUI>().weapons.ToString())
            {
                var i = b.GetComponentsInChildren<Image>();

                foreach (var img in i)
                {
                    if (!img.GetComponent<DragDropUI>())
                    {
                        img.sprite = s;
                    }
                }
            }
        }
        */
    }
}
