using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class ShopHandler : MonoBehaviour
{
	[SerializeField] private List<GameObject> itemsHeld;
	[SerializeField] private List<int> itemStock; // foreach in items held add stock number (how much to sell)
	[SerializeField] private Transform block, grid;
	[SerializeField] private float range;

	private GameObject _prompt, _player, _uiHandler;
	[SerializeField] private GameObject shopGUI;
	private CharacterMovement _characterMovement;
	private ItemPickupHandler _itemPickupHandler;

	private void Start()
	{
		_player = GameObject.FindGameObjectWithTag("Player");
		_prompt = GameObject.FindGameObjectWithTag("Prompt");
		_characterMovement = _player.GetComponent<CharacterMovement>();
		_itemPickupHandler = _player.GetComponent<ItemPickupHandler>();
		RefreshShop();
	}

	private void Update()
	{
		if (_characterMovement.uiOpen) return;

		var dist = Vector3.Distance(transform.position, _player.transform.position);

		if (dist <= range)
		{
			_itemPickupHandler.isPlrNearShop = true;
			_prompt.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, 200, 0);
			_prompt.GetComponentInChildren<TextMeshProUGUI>().text = "Open Shop (Backspace / [O]";
		}
		else if (dist > range)
		{
			_itemPickupHandler.isPlrNearShop = false;
			_prompt.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, -200, 0);
			_prompt.GetComponentInChildren<TextMeshProUGUI>().text = "";
		}
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
		newBlock.GetComponentInChildren<TextMeshProUGUI>().text = itemConsumable.title;
		newBlock.GetComponent<Button>().interactable = true;
        
		var indexHolder = newBlock.GetComponent<IndexHolder>();
		indexHolder.consumable = itemConsumable;
		indexHolder.numHeld = itemStock[i];
        
		
		// updates the onclick for the new inventory item button
		newBlock.GetComponent<Button>().onClick.AddListener(delegate
		{
			ShopItemSelected(indexHolder);
		});
		

		foreach (var s in newBlock.GetComponentsInChildren<Image>())
		{
			if (s.name == "Image")
			{
				s.sprite = itemConsumable.uiIcon;
				s.GetComponentInChildren<TextMeshProUGUI>().text = indexHolder.numHeld.ToString();
			}
		}
	}

	private void ShopItemSelected(IndexHolder indexHolder)
	{
		// gets the consumable script on gameobject, gets the image and title, calls method to add inv item to toolbar
		var consumable = indexHolder.consumable;
		
		// TODO: needs to add item selected to inventory and deduct currency
	}

	public void ToggleShop(InputAction.CallbackContext context)
	{
		if (!context.performed) return;
		
		if (shopGUI.activeSelf)
		{
			shopGUI.SetActive(false);
		}
		else if (!shopGUI.activeSelf)
		{
			if (_characterMovement.uiOpen) return;
			if (!_itemPickupHandler.isPlrNearShop) return;
			
			shopGUI.SetActive(true);

			if (itemsHeld.Count > 0)
			{
				//uiHandler.GetComponent<MenuHandler>().SwitchSelected(grid.GetComponentInChildren<Button>().gameObject);
			}
		}
	}
	
	// EventSystem.current.currentSelectedGameObject == slot
}
