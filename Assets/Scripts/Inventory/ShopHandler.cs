using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class ShopHandler : MonoBehaviour
{
	[SerializeField] private List<GameObject> possibleItems;
	[SerializeField] private List<int> chanceToStockItem;
	[SerializeField] private List<int> minItemCost;
	[SerializeField] private List<int> maxItemCost;
	public List<GameObject> itemsHeld;
	[SerializeField] private List<int> itemStock; // each index directly relates to itemsheld index
	[SerializeField] private List<int> itemPrice; // each index directly relates to itemsheld index
	[SerializeField] private Transform block;
	public Transform grid;
	[SerializeField] private float range;
	[SerializeField] private float floor2CostMultiplier, floor3CostMultiplier, floor4CostMultiplier;
	[SerializeField] private float hardCoreBaseMultiplier;
	private float _activeFloorMultiplier;

	private GameObject _player, _uiManager, _lastSelected;
	private MenuHandler _menuHandler;
	private GameObject _shopGUI;
	[SerializeField] private GameObject shopBck, shopInfo, shopTitle;
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
		_menuHandler = _uiManager.GetComponent<MenuHandler>();
		_shopGUI = GetComponentInChildren<Canvas>(true).gameObject;
		_menuHandler.shopGUI = _shopGUI;
		_menuHandler.shopBck = shopBck;
		_menuHandler.shopTitleText = shopTitle;
		_menuHandler.shopInfo = shopInfo;
		_inventoryStore = _uiManager.GetComponent<InventoryStore>();
		_characterMovement = _player.GetComponent<CharacterMovement>();
		_itemPickupHandler = _player.GetComponent<ItemPickupHandler>();
		_currencyManager = _uiManager.GetComponent<CurrencyManager>();

		if (dataHolder.hardcoreMode)
		{
			_activeFloorMultiplier = hardCoreBaseMultiplier;
		}
		else
		{
			_activeFloorMultiplier = 1;
		}

		switch (LevelBuilder.Instance.currentFloor)
		{
			case LevelBuilder.LevelMode.Floor2:
				_activeFloorMultiplier += floor2CostMultiplier;
				break;
			case LevelBuilder.LevelMode.Floor3:
				_activeFloorMultiplier += floor3CostMultiplier;
				break;
			case LevelBuilder.LevelMode.Floor4:
				_activeFloorMultiplier += floor4CostMultiplier;
				break;
		}
		
		ScalePrices();
		RandomiseStock();
		RefreshShop();
	}

	private void ScalePrices()
	{
		for (var i = 0; i < minItemCost.Count; i++)
		{
			minItemCost[i] = (int)(minItemCost[i] * _activeFloorMultiplier);
		}
		
		for (var i = 0; i < maxItemCost.Count; i++)
		{
			maxItemCost[i] = (int)(maxItemCost[i] * _activeFloorMultiplier);
		}
	}

	private void Update()
	{
		var dist = Vector3.Distance(transform.position, _player.transform.position);

		if (dist <= range)
		{
			_itemPickupHandler.isPlrNearShop = true;
			if (_shopGUI.activeSelf)
			{
				_itemPickupHandler.TogglePrompt("Close shop", true, ControlsManager.ButtonType.Back, "", null);
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

	private void RandomiseStock()
	{
		// randomise which items are in the shop based on which items are possible
		for (var i = 0; i < possibleItems.Count; i++)
		{
			var item = possibleItems[i];
			var roll = Random.Range(0, 10);

			if (roll < chanceToStockItem[i])
			{
				itemsHeld.Add(item);
			}
		}

		// randomise how much of each item is held, and how much they cost
		for (var i = 0; i < itemsHeld.Count; i++)
		{
			var item = possibleItems.FirstOrDefault(item => item.GetComponent<Consumable>() == itemsHeld[i].GetComponent<Consumable>());
			var itemIndex = possibleItems.IndexOf(item);
			var roll = Random.Range(1, 4); // 1-3
			itemStock.Add(roll);

			var priceRoll = Random.Range(minItemCost[itemIndex], maxItemCost[itemIndex]);
			itemPrice.Add(priceRoll);
		}
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
			_inventoryStore.TriggerNotification(indexHolder.consumable.uiIcon, "Not enough robot coils held.", false);
			return;
		}
		if (indexHolder.numHeld <= 0)
		{
			AudioManager.Instance.PlayOneShot(FMODEvents.Instance.PurchaseFailed, transform.position);
			_inventoryStore.TriggerNotification(indexHolder.consumable.uiIcon, "Item is no longer in stock.", false);
			return;
		}

		foreach (var n in _inventoryStore.grid.GetComponentsInChildren<IndexHolder>())
		{
			if (n.consumable.title != indexHolder.consumable.title) continue;
			if (n.numHeld == n.consumable.maximumHold)
			{
				_inventoryStore.TriggerNotification(indexHolder.consumable.uiIcon, "Maximum number of item held", false);
				return;
			}
		}

		_currencyManager.UpdateCurrency(-indexHolder.price);
		// gets the consumable script on gameobject, updates the stock held
		var consumable = indexHolder.consumable;
		indexHolder.numHeld--;
		
		// add item to inventory when bought, instantiation prevents errors due to using a prefab
		_inventoryStore.AddNewItem(indexHolder.consumable);
		
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
