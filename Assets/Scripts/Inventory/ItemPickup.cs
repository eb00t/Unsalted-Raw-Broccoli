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
    [SerializeField] private GameObject outLineSprite;

    private void Start()
    {
        _player = GameObject.FindGameObjectWithTag("Player").transform;
        _uiManager = GameObject.FindGameObjectWithTag("UIManager");
        _characterMovement = _player.GetComponent<CharacterMovement>();
        _inventoryStore = _uiManager.GetComponent<InventoryStore>();
        _toolbarHandler = _uiManager.GetComponent<ToolbarHandler>();
        _passiveItemHandler = _uiManager.gameObject.GetComponent<PassiveItemHandler>();
    }
    
    private void Update()
    {
        if (_characterMovement.uiOpen || !gameObject.activeSelf) return;
        
        var dist = Vector3.Distance(_player.position, transform.position);
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

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, range);
    }
}
