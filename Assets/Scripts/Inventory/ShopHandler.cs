using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ShopHandler : MonoBehaviour
{
	public List<GameObject> itemsHeld;
	[SerializeField] private List<int> itemStock; // each index directly relates to itemsheld index
	[SerializeField] private List<int> itemPrice; // each index directly relates to itemsheld index
	[SerializeField] private Transform block;
	public Transform grid;
	[SerializeField] private float range;

	private GameObject _player, _uiManager, _lastSelected;
	private GameObject _shopGUI;
	private CharacterMovement _characterMovement;
	private ItemPickupHandler _itemPickupHandler;
	private CurrencyManager _currencyManager;
	private InventoryStore _inventoryStore;
	[SerializeField] private TextMeshProUGUI infoTxt, numHeldTxt, infoTitle;
	[SerializeField] private DataHolder dataHolder;

	private void Start()
	{
		_player = GameObject.FindGameObjectWithTag("Player");
		_uiManager = GameObject.FindGameObjectWithTag("UIManager");
		_shopGUI = GetComponentInChildren<Canvas>(true).gameObject;
		_uiManager.GetComponent<MenuHandler>().shopGUI = _shopGUI;
		_inventoryStore = _uiManager.GetComponent<InventoryStore>();
		_characterMovement = _player.GetComponent<CharacterMovement>();
		_itemPickupHandler = _player.GetComponent<ItemPickupHandler>();
		_currencyManager = _uiManager.GetComponent<CurrencyManager>();

		RefreshShop();
	}

	private void Update()
	{
		var dist = Vector3.Distance(transform.position, _player.transform.position);

		if (dist <= range)
		{
			_itemPickupHandler.isPlrNearShop = true;
			if (_shopGUI.activeSelf)
			{
				_itemPickupHandler.TogglePrompt("Close shop", true, ControlsManager.ButtonType.ButtonEast, null);
			}
		}
		else if (dist > range)
		{
			if (_itemPickupHandler.itemCount > 0) return;
			_itemPickupHandler.isPlrNearShop = false;
			//_itemPickupHandler.TogglePrompt("", false, ControlsManager.ButtonType.ButtonEast);
		}
		
		if (EventSystem.current.currentSelectedGameObject == _lastSelected) return;
		_lastSelected = EventSystem.current.currentSelectedGameObject;
		UpdateInfo();
	}
	
	private void UpdateInfo()
	{
		if (EventSystem.current.currentSelectedGameObject == null) return;
        
		var indexHolder = EventSystem.current.currentSelectedGameObject.GetComponentInChildren<IndexHolder>();

		if (indexHolder == null || indexHolder.consumable == null) return;
        
		infoTxt.text = indexHolder.consumable.description;
		infoTitle.text = indexHolder.consumable.title;
		numHeldTxt.text = indexHolder.numHeld.ToString();
		//infoImg.sprite = indexHolder.consumable.uiIcon;
	}

	private void RefreshShop()
	{
		foreach (var n in grid.GetComponentsInChildren<Transform>())
		{
			if (n != grid)
			{
				Destroy(n.gameObject);
			}
		}

		for (var i = 0; i < itemsHeld.Count; i++)
		{
			AddToShop(itemsHeld[i], i);
		}
	}

	private void AddToShop(GameObject item, int i)
	{
		var itemConsumable = item.GetComponent<Consumable>();
		
		var newBlock = Instantiate(block, block.position, block.rotation, grid);
		newBlock.GetComponent<Button>().interactable = true;
        
		var indexHolder = newBlock.GetComponent<IndexHolder>();
		indexHolder.consumable = itemConsumable;
		indexHolder.numHeld = itemStock[i];
		indexHolder.price = itemPrice[i];
        
		
		// updates the onclick for the new inventory item button
		newBlock.GetComponent<Button>().onClick.AddListener(delegate
		{
			ShopItemSelected(indexHolder);
		});

		foreach (var s in newBlock.GetComponentsInChildren<Image>())
		{
			if (s.name != "Image") continue;
			s.sprite = itemConsumable.uiIcon;
			s.GetComponentInChildren<TextMeshProUGUI>().text = indexHolder.numHeld.ToString();
		}

		foreach (var b in newBlock.GetComponentsInChildren<TextMeshProUGUI>())
		{
			if (b.name != "Price") continue;
			b.text = indexHolder.price.ToString();
		}
	}

	private void ShopItemSelected(IndexHolder indexHolder)
	{
		// if player does not have enough money then return
		if (dataHolder.currencyHeld - indexHolder.price < 0)
		{
			AudioManager.Instance.PlayOneShot(FMODEvents.Instance.PurchaseFailed, transform.position);
			return;
		}
		if (indexHolder.numHeld <= 0)
		{
			AudioManager.Instance.PlayOneShot(FMODEvents.Instance.PurchaseFailed, transform.position);
			return;
		}

		foreach (var n in _inventoryStore.grid.GetComponentsInChildren<IndexHolder>())
		{
			if (n.consumable.title != indexHolder.consumable.title) continue;
			if (n.numHeld == n.consumable.maximumHold) return;
		}

		_currencyManager.UpdateCurrency(-indexHolder.price);
		// gets the consumable script on gameobject, updates the stock held
		var consumable = indexHolder.consumable;
		indexHolder.numHeld--;
		
		// add item to inventory when bought, instantiation prevents errors due to using a prefab
		var inventoryStore = _uiManager.GetComponent<InventoryStore>();
		inventoryStore.AddNewItem(indexHolder.consumable);
		
		AudioManager.Instance.PlayOneShot(FMODEvents.Instance.PurchaseMade, transform.position);
		
		// update ui text for amount held in stock
		foreach (var i in grid.GetComponentsInChildren<Button>())
		{
			if (i.GetComponent<IndexHolder>().consumable == consumable)
			{
				i.GetComponentInChildren<TextMeshProUGUI>().text = indexHolder.numHeld.ToString();
			}
		}
		
		_uiManager.GetComponent<ToolbarHandler>().UpdateToolbar();
	}
}
