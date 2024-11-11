using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class MenuHandler : MonoBehaviour
// TODO: selectedInv needs to be set by another script as inventory items are generated at runtime
{
	[SerializeField] private GameObject inventoryGui, menu, selectedMenu, equip, selectedEquip;
	[SerializeField] private EventSystem eventSystem;
	private bool isEquip, isInventory;
	private InventoryStore _inventoryStore;
	private ToolbarHandler _toolbarHandler;
	[SerializeField] private GameObject grid;
	private bool isInvInteractable;

	private void Start()
	{
		_inventoryStore = GetComponent<InventoryStore>();
		_toolbarHandler = GetComponent<ToolbarHandler>();
	}

	public void ToggleInventory()
	{
		inventoryGui.SetActive(!inventoryGui.activeSelf);
		isInventory = inventoryGui.activeSelf;
		
		foreach (var b in grid.GetComponentsInChildren<Button>())
		{
			b.interactable = false;
			isInvInteractable = false;
		}
	}

	private void ToggleEquip()
	{
		if (isEquip)
		{
			SwitchSelected(null);
			isEquip = false;

			foreach (var b in equip.GetComponentsInChildren<Button>())
			{
				b.interactable = true;
				b.GetComponentInChildren<TextMeshProUGUI>().text = "";
			}
		}
		else
		{
			SwitchSelected(selectedEquip);
			isEquip = true;

			foreach (var b in equip.GetComponentsInChildren<Button>())
			{
				b.interactable = true;
				b.GetComponentInChildren<TextMeshProUGUI>().text = "+";
			}
		}
	}

	public void Back(InputAction.CallbackContext context)
	{
		if (!context.performed) return;
		
		if (isEquip && !isInventory)
		{
			SwitchSelected(selectedMenu);
			ToggleEquip();
		}
		else if (isInventory && !isInvInteractable)
		{
			ToggleInventory();
		}
		else if (isInventory && isInvInteractable)
		{
			ToggleInventory();
			ToggleEquip();
		}
		else if (!isEquip && !isInventory && menu.activeSelf)
		{
			menu.SetActive(false);
			SwitchSelected(selectedMenu);
		}
	}

	public void Pause(InputAction.CallbackContext context)
	{
		if (isEquip || isInventory) return;
		menu.SetActive(!menu.activeSelf);
		if (menu.activeSelf)
		{
			SwitchSelected(selectedMenu);
		}
	}
	
	private void SwitchSelected(GameObject g)
	{
		eventSystem.SetSelectedGameObject(null);
		eventSystem.SetSelectedGameObject(g);
	}

	public void SlotSelected(int slot)
	{
		if (inventoryGui.activeSelf) return;
		
		if (_inventoryStore.items.Count > 0)
		{
			foreach (var b in grid.GetComponentsInChildren<IndexHolder>())
			{
				if (b.InventoryIndex == 0)
				{
					SwitchSelected(b.gameObject);
				}
			}
			
			ToggleInventory();
			foreach (var b in grid.GetComponentsInChildren<Button>())
			{
				b.interactable = true;
				isInvInteractable = true;
			}
			_toolbarHandler.slotNo = slot;
		}
		else
		{
			// no items held popup
			Debug.Log("No items in inventory");
		}
	}
}
