using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ItemPickupHandler : MonoBehaviour
{
    private Transform _player;
    private CharacterMovement _characterMovement;

    private void Start()
    {
        _player = GameObject.FindGameObjectWithTag("Player").transform;
        _characterMovement = _player.GetComponent<CharacterMovement>();
    }
    public void PickUpItem(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if (_characterMovement.uiOpen) return;
            CheckItems();
        }
    }

    private void CheckItems()
    {
        foreach (var item in GameObject.FindGameObjectsWithTag("Item"))
        {
            var ip = item.GetComponent<ItemPickup>();
            
            if (ip.canPickup)
            {
                ip.AddItemToInventory();
            }
        }
    }
}
