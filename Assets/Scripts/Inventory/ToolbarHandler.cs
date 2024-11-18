using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor.Rendering.LookDev;
using UnityEngine.InputSystem;
using Vector2 = UnityEngine.Vector2;

public class ToolbarHandler : MonoBehaviour
{
    public int slotNo;
    [SerializeField] private GameObject[] slots;
    private InventoryStore _inventoryStore;

    private void Start()
    {
        _inventoryStore = GetComponent<InventoryStore>();
    }

    private void AddToToolbar(Sprite newSprite, string txt)
    {
        slots[slotNo].GetComponentInChildren<TextMeshProUGUI>().text = ""; // set amount held
        
        foreach (var s in slots[slotNo].GetComponentsInChildren<Image>())
        {
            if (s.name == "Image")
            {
                s.sprite = newSprite;
                foreach (var t in s.GetComponentsInChildren<TextMeshProUGUI>())
                {
                    if (t.name == "title")
                    {
                        t.text = txt;
                    }
                }
                s.enabled = true;
            }
        }
    }
    
    public void InvItemSelected(IndexHolder indexHolder)
    {
        var consumable = _inventoryStore.items[indexHolder.InventoryIndex].GetComponent<Consumable>();
        var s = consumable.uiIcon;
        var t = consumable.title;
        
        AddToToolbar(s, t);
    }

    public void SlotItemActivated(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        var dir = context.ReadValue<Vector2>();

        switch (dir.x, dir.y)
        {
            case (0, 1): // up (0)
                // use consumable in slot 0, and so on for each case
                Debug.Log("item 0 used");
                break;
            case (1, 0): // right (1)
                Debug.Log("item 1 used");
                break;
            case (0, -1): // down (2)
                Debug.Log("item 2 used");
                break;
            case (-1, 0): // left (3)
                Debug.Log("item 3 used");
                break;
        }
    }
}
