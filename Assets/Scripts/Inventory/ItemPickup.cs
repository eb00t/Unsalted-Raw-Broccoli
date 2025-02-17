using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.TextCore.Text;

public class ItemPickup : MonoBehaviour
{
    [SerializeField] private GameObject popUpGUI;
    [SerializeField] private float range;
    private Transform _player;
    public bool canPickup;
    private CharacterMovement _characterMovement;
    private InventoryStore _inventoryStore;

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
        
        if (dist <= range)
        {
            popUpGUI.SetActive(true);
            canPickup = true;
        }
        else
        {
            popUpGUI.SetActive(false);
            canPickup = false;
        }
    }

    public void AddItemToInventory()
    {
        _inventoryStore.items.Add(gameObject);
        _inventoryStore.RefreshList();
        popUpGUI.SetActive(false);
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
