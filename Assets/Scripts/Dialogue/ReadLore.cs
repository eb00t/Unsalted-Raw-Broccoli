using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
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
    private GameObject _mapIcon;
    private RoomScripting _roomScripting;
    [SerializeField] private float pickupRange;
    private GameObject _player;
    public LoreItemHandler whatLore;
    public DialogueControllerScript dialogueController;
    public bool loadSpecificLore;
    public bool hasBeenRead; 
    public bool inRange;

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
        dialogueController = GetComponentInChildren<DialogueControllerScript>();
        _mapIcon = transform.Find("mapicon_item").gameObject;
        _roomScripting = gameObject.transform.root.GetComponent<RoomScripting>();

        switch (loreType)
        {
            case LoreType.Book:
                _loreObject = "Book";
                break;
            case LoreType.Data:
                _loreObject = "Datapad";
                break;
            case LoreType.ScrapOfPaper:
                _loreObject = "Scrap Of Paper";
                break;
            case LoreType.Other:
                _loreObject = "Document";
                break;
        }

        _lorePath = "Lore";
        _language = "English"; //TODO: Fix this to make it work with whatever language the game is.
        _fullLorePath = _lorePath + "/" + _language + "/" + _loreObject;
        Debug.Log(_fullLorePath);

        if (loadSpecificLore)
        {
            whatLore = dialogueController.loreToLoad;
        }
        else
        {
            int whatLoreToLoad = Random.Range(0, LoreReference.Instance.allLoreItems.Count);
            whatLore = LoreReference.Instance.allLoreItems[whatLoreToLoad];
        }
    }

    private void Update()
    {
        if (_roomScripting != null)
        {
            if (_roomScripting.playerHasEnteredRoom)
            {
                _mapIcon.SetActive(true);
            }
            else
            {
                _mapIcon.SetActive(false);
            }
        }

        if (!_characterMovement.uiOpen)
        {
            var dist = Vector3.Distance(transform.position, _player.transform.position);

            if (!inRange && dist <= pickupRange)
            {
                inRange = true;
                _itemPickupHandler.TogglePrompt("Read " + _loreObject, true, ControlsManager.ButtonType.Interact, "", null, false);
                _menuHandler.nearestLore = this;
            }
            else if (inRange && dist > pickupRange)
            {
                if (_menuHandler.nearestLore != this) return;
                inRange = false;
                _itemPickupHandler.TogglePrompt("", false, ControlsManager.ButtonType.Interact, "", null, false);
            }
        }
    }

    private void OnDisable()
    {
        inRange = false;
        _itemPickupHandler.TogglePrompt("", false, ControlsManager.ButtonType.Interact, "", null, false);
    }
}
