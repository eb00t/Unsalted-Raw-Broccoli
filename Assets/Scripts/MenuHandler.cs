using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class MenuHandler : MonoBehaviour
{
	[SerializeField] private GameObject inventoryGui;
	[SerializeField] private GameObject menu;
	[SerializeField] private GameObject selectedMenu;
	[SerializeField] private EventSystem eventSystem;

	public void ToggleInventory()
	{
		Debug.Log("Toggle Inventory");
		inventoryGui.SetActive(!inventoryGui.activeSelf);
	}

	public void Pause(InputAction.CallbackContext context)
	{
		menu.SetActive(!menu.activeSelf);
		SwitchSelected(selectedMenu);
	}


	private void SwitchSelected(GameObject g)
	{
		eventSystem.SetSelectedGameObject(null);
		eventSystem.SetSelectedGameObject(g);
	}
}
