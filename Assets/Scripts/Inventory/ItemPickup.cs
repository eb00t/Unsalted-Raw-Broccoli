using TMPro;
using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    private GameObject _prompt;
    private TextMeshProUGUI _text;
    [SerializeField] private float range;
    private Transform _player;
    public bool canPickup;
    private CharacterMovement _characterMovement;
    private InventoryStore _inventoryStore;
    [SerializeField] private GameObject outLineSprite;

    private void Start()
    {
        _player = GameObject.FindGameObjectWithTag("Player").transform;
        _characterMovement = _player.GetComponent<CharacterMovement>();
        _inventoryStore = GameObject.FindGameObjectWithTag("UIManager").GetComponent<InventoryStore>();
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
        _inventoryStore.AddNewItem(gameObject.GetComponent<Consumable>());
        gameObject.SetActive(false);
        canPickup = false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, range);
    }
}
