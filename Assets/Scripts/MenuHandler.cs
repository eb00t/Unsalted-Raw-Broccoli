using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MenuHandler : MonoBehaviour
{
	[SerializeField] private GameObject inventoryGui;
	[SerializeField] private GameObject menu;
	
	public void ToggleInventory()
	{
		inventoryGui.SetActive(!inventoryGui.activeSelf);
	}

	public void Pause(InputAction.CallbackContext context)
	{
		menu.SetActive(!menu.activeSelf);
	}
}
