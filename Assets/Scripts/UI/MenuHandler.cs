using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuHandler : MonoBehaviour
{
	[Header("Code References")]
	[SerializeField] private CharacterMovement characterMovement;
	private InventoryStore _inventoryStore;
	private ToolbarHandler _toolbarHandler;
	private GameObject _player;
	
	[Header("UI References")]
	[SerializeField] private GameObject grid;
	[SerializeField] private GameObject invGui, toolbarGui, menuGui, quitPopupGui, statsGui, infoGui;
	
	[Header("Navigation")]
	[SerializeField] private EventSystem eventSystem;
	[SerializeField] private GameObject selectedMenu, selectedEquip, slots;

	public GameObject shopGUI;

	private void Start()
	{
		_inventoryStore = GetComponent<InventoryStore>();
		_toolbarHandler = GetComponent<ToolbarHandler>();
		_player = GameObject.FindGameObjectWithTag("Player");
	}

	private void Update()
	{
		// update if ui is open or not in player movement script
		if (invGui.activeSelf || menuGui.activeSelf || quitPopupGui.activeSelf)
		{
			characterMovement.uiOpen = true;
		}
		else if (shopGUI != null)
		{
			characterMovement.uiOpen = shopGUI.activeSelf;
		}
		else
		{
			characterMovement.uiOpen = false;
		}
	}

	// opens equip menu (with inventory), hides other menus and resets interaction bool to prevent unwanted ui navigation
	public void ToggleEquip()
	{
		foreach (var s in slots.GetComponentsInChildren<Button>())
		{
			s.interactable = true;
		}
		
		foreach (var b in grid.GetComponentsInChildren<Button>())
		{
			b.interactable = false;
		}
		
		invGui.SetActive(true);
		infoGui.SetActive(false);
		_toolbarHandler.isInfoOpen = false;
		menuGui.SetActive(false);
		SwitchSelected(selectedEquip);
	}
	
	public void ToggleShop(InputAction.CallbackContext context)
	{
		if (!context.performed) return;
		if (shopGUI == null) return;
		
		var shopHandler = shopGUI.GetComponentInParent<ShopHandler>();
		
		if (shopGUI.activeSelf)
		{
			shopGUI.SetActive(false);
		}
		else if (!shopGUI.activeSelf)
		{
			if (characterMovement.uiOpen) return;
			if (!_player.GetComponent<ItemPickupHandler>().isPlrNearShop) return;
			
			shopGUI.SetActive(true);

			if (shopHandler.itemsHeld.Count > 0)
			{
				SwitchSelected(shopHandler.grid.GetComponentInChildren<Button>().gameObject);
			}
		}
	}
	
	// when Button East/Backspace is pressed close current menu and open previous menus
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
			infoGui.SetActive(false);
			_toolbarHandler.isInfoOpen = false;
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

	// toggle pause menu
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

	// switches controller navigation to a new object
	public void SwitchSelected(GameObject g)
	{
		eventSystem.SetSelectedGameObject(null);
		eventSystem.SetSelectedGameObject(g);
	}

	// If player has items, switches navigation to inventory to add item to selected toolbar index when toolbar button is pressed
	public void SlotSelected(int slot)
	{
		if (_inventoryStore.items.Count > 0)
		{
			SwitchSelected(grid.GetComponentInChildren<Button>().gameObject);
			
			foreach (var b in grid.GetComponentsInChildren<Button>())
			{
				b.interactable = true;
			}
			
			foreach (var s in slots.GetComponentsInChildren<Button>())
			{
				s.interactable = false;
			}

			_toolbarHandler.slotNo = slot;
			infoGui.SetActive(true);
			_toolbarHandler.isInfoOpen = true;
		}
		else
		{
			// no items held popup
			Debug.Log("No items in inventory");
		}
	}

	public void SceneReload() // reloads scene
	{
		SceneManager.LoadScene(SceneManager.GetActiveScene().name);
	}

	public void Quit() // quits game
	{
		Application.Quit();
	}
}
