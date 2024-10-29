using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class MenuHandler : MonoBehaviour
{
	[SerializeField] private GameObject inventoryGui, menu, selectedMenu, equip, selectedEquip;
	[SerializeField] private EventSystem eventSystem;
	private bool isEquip, isInventory;
	[SerializeField] private PlayerInput playerInput;

	public void ToggleInventory()
	{
		inventoryGui.SetActive(!inventoryGui.activeSelf);
		isInventory = inventoryGui.activeSelf;
	}
	
	public void ToggleEquip()
	{
		SwitchSelected(selectedEquip);
		isEquip = true;
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
}
