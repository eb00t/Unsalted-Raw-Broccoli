using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuHandler : MonoBehaviour
// TODO: selectedInv needs to be set by another script as inventory items are generated at runtime
{
	[SerializeField] private GameObject inventoryGui, menu, selectedMenu, equip, selectedEquip;
	[SerializeField] private EventSystem eventSystem;
	private bool _isEquip, _isInventory, _isInvInteractable;
	private InventoryStore _inventoryStore;
	private ToolbarHandler _toolbarHandler;
	[SerializeField] private GameObject grid;

	[SerializeField] private CharacterMovement characterMovement;

	private void Start()
	{
		_inventoryStore = GetComponent<InventoryStore>();
		_toolbarHandler = GetComponent<ToolbarHandler>();
	}

	private void Update()
	{
		if (_isEquip || _isInventory || _isInvInteractable || menu.activeSelf)
		{
			characterMovement.uiOpen = true;
		}
		else
		{
			characterMovement.uiOpen = false;
		}
	}

	public void ToggleInventory()
	{
		inventoryGui.SetActive(!inventoryGui.activeSelf);
		_isInventory = inventoryGui.activeSelf;

		foreach (var b in grid.GetComponentsInChildren<Button>())
		{
			b.interactable = false;
			_isInvInteractable = false;
		}
	}

	private void ToggleEquip()
	{
		if (_isEquip)
		{
			SwitchSelected(null);
			_isEquip = false;

			foreach (var b in equip.GetComponentsInChildren<Button>())
			{
				b.interactable = true;
				b.GetComponentInChildren<TextMeshProUGUI>().text = "";
			}
		}
		else
		{
			SwitchSelected(selectedEquip);
			_isEquip = true;

			foreach (var b in equip.GetComponentsInChildren<Button>())
			{
				b.interactable = true;
				b.GetComponentInChildren<TextMeshProUGUI>().text = "+";
			}
		}
	}

	public void Back(InputAction.CallbackContext context) // switch to just close everything and open what is needed ?
	{
		if (!context.performed) return;

		if (_isEquip && !_isInventory)
		{
			ToggleEquip();
			SwitchSelected(selectedMenu);
		}
		else if (_isInventory && _isInvInteractable)
		{
			ToggleInventory();
			SwitchSelected(selectedEquip);
		}
		else if (_isInventory && !_isInvInteractable)
		{
			ToggleInventory();
			ToggleEquip();
		}
		else if (!_isEquip && !_isInventory && menu.activeSelf)
		{
			menu.SetActive(false);
			SwitchSelected(selectedMenu);
		}
	}

	public void Pause(InputAction.CallbackContext context)
	{
		if (_isEquip || _isInventory) return;
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
				_isInvInteractable = true;
			}

			_toolbarHandler.slotNo = slot;
		}
		else
		{
			// no items held popup
			Debug.Log("No items in inventory");
		}
	}

	public void OverrideBack()
	{
		if (_isEquip && !_isInventory)
		{
			ToggleEquip();
			SwitchSelected(selectedMenu);
		}
		else if (_isInventory && _isInvInteractable)
		{
			ToggleInventory();
			SwitchSelected(selectedEquip);
		}
		else if (_isInventory && !_isInvInteractable)
		{
			ToggleInventory();
			ToggleEquip();
		}
		else if (!_isEquip && !_isInventory && menu.activeSelf)
		{
			menu.SetActive(false);
			SwitchSelected(selectedMenu);
		}
	}

	public void SceneReload()
	{
		SceneManager.LoadScene(SceneManager.GetActiveScene().name);
	}
}
