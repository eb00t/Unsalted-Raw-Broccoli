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
    private Consumable _consumable;

    private void Start()
    {
        _player = GameObject.FindGameObjectWithTag("Player").transform;
        _characterMovement = _player.GetComponent<CharacterMovement>();
        _inventoryStore = GameObject.FindGameObjectWithTag("UIManager").GetComponent<InventoryStore>();
        _prompt = GameObject.FindGameObjectWithTag("Prompt");
        _text = _prompt.GetComponentInChildren<TextMeshProUGUI>();
        _consumable = GetComponent<Consumable>();
    }

    private void Update()
    {
        if (_characterMovement.uiOpen || !gameObject.activeSelf) return;
        
        var dist = Vector3.Distance(_player.position, transform.position);
        
        if (dist <= range)
        {
            _prompt.SetActive(true);
            _text.text = "Pick up " + _consumable.title + "[O] / Backspace";
            canPickup = true;
        }
        else
        {
            _prompt.SetActive(false);
            _text.text = "";
            canPickup = false;
        }
    }

    public void AddItemToInventory()
    {
        _inventoryStore.AddNewItem(gameObject);
        _prompt.SetActive(false);
        gameObject.SetActive(false);
        canPickup = false;
    }

    /*
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log(other.name + " : " + other.gameObject.name);
        if (other.CompareTag("Player"))
        {
            _animator.SetTrigger("SlideIn");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _animator.SetTrigger("SlideOut");
        }
    }
    */

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, range);
    }
}
