using System;
using TMPro;
using UnityEditor;
using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    private GameObject _prompt;
    private TextMeshProUGUI _text;
    [SerializeField] private float range;
    private Transform _player;
    public bool canPickup;
    public bool isPermanentPassive;
    private CharacterMovement _characterMovement;
    private GameObject _uiManager;
    private InventoryStore _inventoryStore;
    private ToolbarHandler _toolbarHandler;
    private PassiveItemHandler _passiveItemHandler;
    private bool _inRange;
    [SerializeField] private GameObject outLineSprite;
    private string _promptName;
    [SerializeField] private bool isTutorial;

    private void Start()
    {
        _player = GameObject.FindGameObjectWithTag("Player").transform;
        _uiManager = GameObject.FindGameObjectWithTag("UIManager");
        _characterMovement = _player.GetComponent<CharacterMovement>();
        _inventoryStore = _uiManager.GetComponent<InventoryStore>();
        _toolbarHandler = _uiManager.GetComponent<ToolbarHandler>();
        _passiveItemHandler = _uiManager.gameObject.GetComponent<PassiveItemHandler>();

        if (isPermanentPassive)
        {
            _promptName = GetComponent<PermanentPassiveItem>().title;
        }
        else
        {
            _promptName = GetComponent<Consumable>().title;
        }
    }
    
    private void Update()
    {
        if (_characterMovement.uiOpen || !gameObject.activeSelf) return;
        
        var dist = Vector3.Distance(transform.position, _player.position);

        if (!_inRange && dist <= range)
        {
            _inRange = true;
            if (!isTutorial)
                ItemPickupHandler.Instance.TogglePrompt("Pick Up <color=#00A2FF>" + _promptName + "</color>", true, ControlsManager.ButtonType.Interact, "", null, false);
        }
        else if (_inRange && dist > range)
        {
            _inRange = false;
            if (!isTutorial)
                ItemPickupHandler.Instance.TogglePrompt("", false, ControlsManager.ButtonType.Interact, "", null, false);
        }
        
        canPickup = dist <= range;
        outLineSprite.SetActive(canPickup);
    }
    
    public void AddItemToInventory()
    {
        if (isPermanentPassive)
        {
            _passiveItemHandler.AddNewPassive(GetComponent<PermanentPassiveItem>());
            canPickup = false;
            gameObject.SetActive(false);
        }
        else
        {
            var consumable = GetComponent<Consumable>();
            if (GetComponent<Consumable>().isInstantUse)
            {
                _toolbarHandler.UseItemEffect(consumable);
                canPickup = false;
                gameObject.SetActive(false);
            }
            else
            {
                _inventoryStore.AddNewItem(GetComponent<Consumable>());
            }
        }
    }

    private void OnDisable()
    {
        _inRange = false;
        if (!isTutorial)
            ItemPickupHandler.Instance.TogglePrompt("", false, ControlsManager.ButtonType.Interact, "", null, false);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, range);
    }
}
