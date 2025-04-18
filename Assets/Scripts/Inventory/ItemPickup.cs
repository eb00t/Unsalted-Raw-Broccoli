using TMPro;
using UnityEditor;
using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    private GameObject _prompt;
    private TextMeshProUGUI _text;
    private Transform _player;
    public bool canPickup;
    private CharacterMovement _characterMovement;
    private GameObject _uiManager;
    private InventoryStore _inventoryStore;
    private ToolbarHandler _toolbarHandler;
    private ItemPickupHandler _itemPickupHandler;
    [SerializeField] private GameObject outLineSprite;
    private PromptTrigger _promptTrigger;

    private void Start()
    {
        _promptTrigger = GetComponent<PromptTrigger>();
        _promptTrigger.promptText = "Pick up " + gameObject.GetComponent<Consumable>().title;
        _player = GameObject.FindGameObjectWithTag("Player").transform;
        _itemPickupHandler = _player.GetComponent<ItemPickupHandler>();
        _uiManager = GameObject.FindGameObjectWithTag("UIManager");
        _characterMovement = _player.GetComponent<CharacterMovement>();
        _inventoryStore = _uiManager.GetComponent<InventoryStore>();
        _toolbarHandler = _uiManager.GetComponent<ToolbarHandler>();
    }
    
    private void Update()
    {
        if (_characterMovement.uiOpen || !gameObject.activeSelf) return;
        
        canPickup = _itemPickupHandler.nearestPromptTrigger == _promptTrigger;
        outLineSprite.SetActive(canPickup);
    }
    public void AddItemToInventory()
    {
        var consumable = gameObject.GetComponent<Consumable>();
        if (gameObject.GetComponent<Consumable>().isInstantUse)
        {
            _toolbarHandler.UseItemEffect(consumable);
            
            GetComponent<ItemPickup>().canPickup = false;
            _itemPickupHandler.promptTriggers.Remove(_promptTrigger);
            gameObject.SetActive(false);
        }
        else
        {
            _inventoryStore.AddNewItem(gameObject.GetComponent<Consumable>());
        }
    }
}
