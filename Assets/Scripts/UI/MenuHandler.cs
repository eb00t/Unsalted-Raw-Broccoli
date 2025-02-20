using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuHandler : MonoBehaviour
{
	[SerializeField] private GameObject selectedMenu, selectedEquip, slots;
	[SerializeField] private EventSystem eventSystem;
	private InventoryStore _inventoryStore;
	private ToolbarHandler _toolbarHandler;
	[SerializeField] private GameObject grid;

	[SerializeField] private CharacterMovement characterMovement;
	
	// to track what to do on back button press
	[SerializeField] private GameObject invGui, toolbarGui, menuGui, quitPopupGui, statsGui;

	private void Start()
	{
		_inventoryStore = GetComponent<InventoryStore>();
		_toolbarHandler = GetComponent<ToolbarHandler>();
	}

	private void Update()
	{
		if (invGui.activeSelf || menuGui.activeSelf || quitPopupGui.activeSelf)
		{
			characterMovement.uiOpen = true;
		}
		else
		{
			characterMovement.uiOpen = false;
		}
	}

	public void ToggleEquip()
	{
		foreach (var s in slots.GetComponentsInChildren<Button>())
		{
			s.interactable = true;
		}
		
		foreach (var b in grid.GetComponentsInChildren<Button>())
		{
			b.interactable = true;
		}
		
		invGui.SetActive(true);
		menuGui.SetActive(false);
		SwitchSelected(selectedEquip);
	}
	
	public void Back(InputAction.CallbackContext context)
	{
		if (!context.performed) return;
		
		if (invGui.activeSelf)
		{
			foreach (var b in grid.GetComponentsInChildren<Button>())
			{
				b.interactable = false;
			}
			
			foreach (var s in slots.GetComponentsInChildren<Button>())
			{
				s.interactable = true;
			}
			
			invGui.SetActive(false);
			menuGui.SetActive(true);
			SwitchSelected(selectedMenu);
		}
		else if (menuGui.activeSelf)
		{
			menuGui.SetActive(false);
			toolbarGui.SetActive(true);
			statsGui.SetActive(true);
			SwitchSelected(null);
		}
	}

	public void Pause(InputAction.CallbackContext context)
	{
		if (invGui.activeSelf) return;
		
		menuGui.SetActive(!menuGui.activeSelf);
		
		if (menuGui.activeSelf)
		{
			SwitchSelected(selectedMenu);
			statsGui.SetActive(false);
			toolbarGui.SetActive(false);
		}
		else if (!invGui.activeSelf)
		{
			SwitchSelected(null);
			statsGui.SetActive(true);
			toolbarGui.SetActive(true);
		}
	}

	public void SwitchSelected(GameObject g)
	{
		eventSystem.SetSelectedGameObject(null);
		eventSystem.SetSelectedGameObject(g);
	}

	public void SlotSelected(int slot)
	{
		if (_inventoryStore.items.Count > 0)
		{
			foreach (var b in grid.GetComponentsInChildren<IndexHolder>())
			{
				if (b.InventoryIndex == 0)
				{
					SwitchSelected(b.gameObject);
				}
			}
			
			foreach (var b in grid.GetComponentsInChildren<Button>())
			{
				b.interactable = true;
			}
			
			foreach (var s in slots.GetComponentsInChildren<Button>())
			{
				Debug.Log(s.name);
				s.interactable = false;
			}

			_toolbarHandler.slotNo = slot;
		}
		else
		{
			// no items held popup
			Debug.Log("No items in inventory");
		}
	}

	public void SceneReload()
	{
		SceneManager.LoadScene(SceneManager.GetActiveScene().name);
	}

	public void Quit()
	{
		Application.Quit();
	}
}
