using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class MenuHandler : MonoBehaviour
// TODO: selectedInv needs to be set by another script as inventory is generated at runtime
{
	[SerializeField] private GameObject inventoryGui, menu, selectedMenu, equip, selectedEquip;
	[SerializeField] private EventSystem eventSystem;
	private bool isEquip, isInventory;
	private InventoryStore _inventoryStore;

	private void Start()
	{
		_inventoryStore = GetComponent<InventoryStore>();
	}

	public void ToggleInventory()
	{
		inventoryGui.SetActive(!inventoryGui.activeSelf);
		isInventory = inventoryGui.activeSelf;
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
		if (context.performed)
		{
			if (isInventory)
			{
				ToggleInventory();
				ToggleEquip();
			}
			else if (isEquip)
			{
				SwitchSelected(selectedMenu);
				isEquip = false;
			}
			else if (!isEquip && !isInventory)
			{
				menu.SetActive(false);
				eventSystem.SetSelectedGameObject(null);
			}
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
		if (_inventoryStore.items[0] != null)
		{
			SwitchSelected(_inventoryStore.items[0]);
		}
		
		ToggleEquip();
		ToggleInventory();

		switch (slot)
		{
			case 0:
				break;
			case 1:
				break;
			case 2:
				break;
			case 3:
				break;
		}
	}
}
