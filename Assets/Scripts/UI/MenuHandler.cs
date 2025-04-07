using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuHandler : MonoBehaviour
{
	[Header("Code References")]
	[SerializeField] private CharacterMovement characterMovement;
	CharacterAttack _characterAttack;
	private ItemPickupHandler _itemPickupHandler;
	private InventoryStore _inventoryStore;
	private ToolbarHandler _toolbarHandler;
	private GameObject _player;
	private BlackoutManager _blackoutManager;
	
	[Header("UI References")]
	[SerializeField] private GameObject grid;
	[SerializeField] private GameObject invGui, toolbarGui, menuGui, quitPopupGui, statsGui, infoGui, settingGui, controlGui, diedScreen;
	public GameObject dialogueGUI;
	[SerializeField] private GameObject settingsBtn, controlsBtn, quitBtn;
	
	[Header("Navigation")]
	[SerializeField] private EventSystem eventSystem;
	[SerializeField] private GameObject selectedMenu, selectedEquip, slots;
	private GameObject _lastSelected;

	public GameObject shopGUI, nextLevelTrigger;
	[SerializeField] private DataHolder dataHolder;
	[NonSerialized] public ReadLore nearestLore;
	[NonSerialized] public dialogueControllerScript dialogueController;
	private bool _distanceBasedDialogue;

	private void Start()
	{
		_inventoryStore = GetComponent<InventoryStore>();
		_toolbarHandler = GetComponent<ToolbarHandler>();
		_player = GameObject.FindGameObjectWithTag("Player");
		_itemPickupHandler = _player.GetComponent<ItemPickupHandler>();
		_characterAttack = _player.GetComponentInChildren<CharacterAttack>();
		_blackoutManager = GameObject.Find("Game Manager").GetComponentInChildren<BlackoutManager>();
	}

	private void Update()
	{
		// update if ui is open or not in player movement script
		var pauseGuisOpen = invGui.activeSelf || menuGui.activeSelf || quitPopupGui.activeSelf || settingGui.activeSelf || controlGui.activeSelf || diedScreen.activeSelf;
		var noPauseGuisOpen = (shopGUI != null && shopGUI.activeSelf) || (dialogueGUI != null && dialogueGUI.activeSelf);

		if (!_characterAttack.isDead)
		{
			characterMovement.uiOpen = pauseGuisOpen || noPauseGuisOpen;
			Time.timeScale = pauseGuisOpen ? 0 : 1;
		}
		else
		{
			characterMovement.uiOpen = true;
		}

		if (dataHolder.currentControl == ControlsManager.ControlScheme.Keyboard)
		{
			var interactable = GetInteractable();
			if (interactable != null)
			{
				SwitchSelected(interactable);
			}

			if ((!Cursor.visible || Cursor.lockState == CursorLockMode.Locked) && characterMovement.uiOpen)
			{
				Cursor.visible = true;
				Cursor.lockState = CursorLockMode.None;
			}
			else if ((Cursor.visible || Cursor.lockState == CursorLockMode.None) && !characterMovement.uiOpen)
			{
				Cursor.visible = false;
				Cursor.lockState = CursorLockMode.Locked;
			}
		}
		else if (dataHolder.isGamepad && Cursor.visible || Cursor.lockState == CursorLockMode.None)
		{
			Cursor.visible = false;
			Cursor.lockState = CursorLockMode.Locked;
		}
		
		if (eventSystem.currentSelectedGameObject != _lastSelected)
		{
			if (dataHolder.currentControl == ControlsManager.ControlScheme.Keyboard) return;
			ButtonHandler.Instance.PlayNavigateSound();
			_lastSelected = eventSystem.currentSelectedGameObject;
		}
	}
	
	// checks if there is an interactable ui element below the mouse with a raycast
	private static GameObject GetInteractable()
	{
		var pointerData = new PointerEventData(EventSystem.current) { position = Input.mousePosition };

		var raycastResults = new List<RaycastResult>();
		EventSystem.current.RaycastAll(pointerData, raycastResults);

		foreach (var result in raycastResults)
		{
			var go = result.gameObject;
			
			if (go.TryGetComponent<Selectable>(out var selectable) && selectable.interactable)
			{
				return selectable.gameObject;
			}
		}

		return null;
	}

	// opens equip menu (with inventory), hides other menus and resets interaction bool to prevent unwanted ui navigation
	public void ToggleEquip()
	{
		ButtonHandler.Instance.PlayConfirmSound();
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
		SwitchSelected(_toolbarHandler.slots[_toolbarHandler.slotNo]);
	}

	public void ToggleShop(InputAction.CallbackContext context)
	{
		if (!context.performed) return;
		if (characterMovement.uiOpen) return;
		if (shopGUI == null || shopGUI.activeSelf) return;
		var shopHandler = shopGUI.GetComponentInParent<ShopHandler>();
		if (!_player.GetComponent<ItemPickupHandler>().isPlrNearShop) return;
		
		ButtonHandler.Instance.PlayConfirmSound();
		
		shopGUI.SetActive(true);

		if (shopHandler.itemsHeld.Count > 0)
		{
			SwitchSelected(shopHandler.grid.GetComponentInChildren<Button>().gameObject);
		}
	}

	public void NextLevelLoad(InputAction.CallbackContext context)
	{
		if (!context.performed) return;
		if (characterMovement.uiOpen) return;
		if (!_player.gameObject.GetComponent<ItemPickupHandler>().isPlrNearEnd) return;
		
		nextLevelTrigger.GetComponent<NextLevelTrigger>().LoadNextLevel();
	}

	// when Button East/Esc is pressed close current menu and open previous menus
	public void Back(InputAction.CallbackContext context)
	{
		if (!context.performed) return;
		if (!characterMovement.uiOpen) return;

		if (invGui.activeSelf && !_toolbarHandler.isInfoOpen)
		{
			ButtonHandler.Instance.PlayBackSound();
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
		else if (invGui.activeSelf && _toolbarHandler.isInfoOpen)
		{
			ButtonHandler.Instance.PlayBackSound();
			foreach (var b in grid.GetComponentsInChildren<Button>())
			{
				b.interactable = false;
			}
			
			foreach (var s in slots.GetComponentsInChildren<Button>())
			{
				s.interactable = true;
			}
			
			SwitchSelected(_toolbarHandler.slots[_toolbarHandler.slotNo]);
			infoGui.SetActive(false);
			_toolbarHandler.isInfoOpen = false;
		}
		else if (menuGui.activeSelf)
		{
			ButtonHandler.Instance.PlayBackSound();
			menuGui.SetActive(false);
			toolbarGui.SetActive(true);
			statsGui.SetActive(true);
			SwitchSelected(null);
		}
		else if (shopGUI != null  && shopGUI.activeSelf)
		{
			shopGUI.SetActive(false);
		}
		else if (settingGui.activeSelf)
		{
			ButtonHandler.Instance.PlayBackSound();
			settingGui.SetActive(false);
			menuGui.SetActive(true);
			SwitchSelected(settingsBtn);
		}
		else if (quitPopupGui.activeSelf)
		{
			ButtonHandler.Instance.PlayBackSound();
			quitPopupGui.SetActive(false);
			menuGui.SetActive(true);
			SwitchSelected(quitBtn);
		}
		else if (controlGui.activeSelf)
		{
			ButtonHandler.Instance.PlayBackSound();
			controlGui.SetActive(false);
			menuGui.SetActive(true);
			SwitchSelected(controlsBtn);
		}
	}

	public void DeathReload(InputAction.CallbackContext context)
	{
		if (!context.performed) return;

		if (diedScreen.activeSelf)
		{
			SceneReload();
		}
	}
	
	public void EnableDialogueBox(InputAction.CallbackContext context)
	{
		if (!context.performed) return;
		_distanceBasedDialogue = true;
		TriggerDialogue();
	}

	public void TriggerDialogue()
	{
		if (characterMovement.uiOpen) return;

		if (_itemPickupHandler.isPlrNearDialogue && _distanceBasedDialogue)
		{
			dialogueGUI.SetActive(true);
			dialogueController.LoadDialogue(dialogueController.dialogueToLoad);
			_distanceBasedDialogue = false;
		}
		else if(_itemPickupHandler.isPlrNearDialogue == false && _distanceBasedDialogue == false)
		{
			dialogueGUI.SetActive(true);
			dialogueController.LoadDialogue(dialogueController.dialogueToLoad);
		}
	}

	public void ShowLore(InputAction.CallbackContext context)
	{
		if (!context.performed) return;
		
		if (_itemPickupHandler.isPlrNearLore)
		{
			// when lore is interacted with do something here
			// you can check anything in the nearest ReadLore script from nearestLore variable
			Debug.Log(nearestLore.loreType);
		}
	}

	// toggle pause menu
	public void Pause(InputAction.CallbackContext context)
	{
		if (!context.performed) return;
		if (characterMovement.uiOpen && !menuGui.activeSelf) return;
		if (!_blackoutManager.blackoutComplete) return;
		
		menuGui.SetActive(!menuGui.activeSelf);
		
		if (menuGui.activeSelf)
		{
			SwitchSelected(selectedMenu);
			statsGui.SetActive(false);
			toolbarGui.SetActive(false);
		}
		else
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
		_lastSelected = g;
	}
	
	private void OnApplicationFocus(bool hasFocus)
	{
		if (!hasFocus) return;
		SwitchSelected(_lastSelected);
	}

	// If player has items, switches navigation to inventory to add item to selected toolbar index when toolbar button is pressed
	public void SlotSelected(int slot)
	{
		_toolbarHandler.slotNo = slot;
		
		if (dataHolder.savedItems.Count > 0)
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
			
			infoGui.SetActive(true);
			_toolbarHandler.isInfoOpen = true;
		}
		else
		{
			// no items held popup
			_inventoryStore.TriggerNotification(null, "No items held in inventory", false);
		}
	}

	public void SceneReload() // reloads scene
	{
		ButtonHandler.Instance.PlayBackSound();
		SceneManager.LoadScene(SceneManager.GetActiveScene().name);
	}

	public void LoadScene(string scene)
	{
		SceneManager.LoadScene(scene);
	}

	public void Quit() // quits game
	{
		ButtonHandler.Instance.PlayBackSound();
		Application.Quit();
	}
}
