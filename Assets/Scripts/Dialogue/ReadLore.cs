using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class ReadLore : MonoBehaviour
{
    private string _language;
    private string _lorePath;
    private string _loreObject;
    private string _fullLorePath;
    private CharacterMovement _characterMovement;
    private ItemPickupHandler _itemPickupHandler;
    private MenuHandler _menuHandler;
    private GameObject _uiManager;
    [SerializeField] private float pickupRange;
    private GameObject _player;
    public LoreItemHandler whatLore;
    public enum LoreType
    {
        Book,
        Data,
        ScrapOfPaper,
        Other
    }
    public LoreType loreType;
    
    private void Start()
    {
        _player = GameObject.FindGameObjectWithTag("Player");
        _uiManager = GameObject.FindGameObjectWithTag("UIManager");
        _menuHandler = _uiManager.GetComponent<MenuHandler>();
        _characterMovement = _player.GetComponent<CharacterMovement>();
        _itemPickupHandler = _player.GetComponent<ItemPickupHandler>();
        
        switch (loreType)
        {
            case LoreType.Book:
                _loreObject = "Book Lore";
                break;
            case LoreType.Data:
                _loreObject = "Data Lore";
                break;
            case LoreType.ScrapOfPaper:
                _loreObject = "Scrap Of Paper Lore";
                break;
        }
        
        _lorePath = "Lore";
        _language = "English"; //TODO: Fix this to make it work with whatever language the game is.
        _fullLorePath = _lorePath + "/" + _language + "/" + _loreObject;
        Debug.Log(_fullLorePath);

        int whatLoreToLoad = Random.Range(0, LoreReference.Instance.allLoreItems.Count);
        whatLore = LoreReference.Instance.allLoreItems[whatLoreToLoad];
    }

    private void Update()
    {
        if (!_characterMovement.uiOpen)
        {
            var dist = Vector3.Distance(transform.position, _player.transform.position);

            if (dist <= pickupRange)
            {
                _itemPickupHandler.isPlrNearLore = true;
                _itemPickupHandler.TogglePrompt("Read " + _loreObject, true, ControlsManager.ButtonType.Interact, null);
                _menuHandler.nearestLore = this;
            }
            else if (dist > pickupRange)
            {
                if (_menuHandler.nearestLore != this) return;
                _itemPickupHandler.isPlrNearLore = false;
            }
        }
    }

    private void OnDisable()
    {
        _itemPickupHandler.isPlrNearLore = false;
        _menuHandler.nearestLore = null;
    }
}
