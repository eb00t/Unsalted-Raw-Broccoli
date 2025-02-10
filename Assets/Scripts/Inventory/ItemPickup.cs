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
    [SerializeField] private InventoryStore inventoryStore;

    private void Start()
    {
        _player = GameObject.FindGameObjectWithTag("Player").transform;
        _characterMovement = _player.GetComponent<CharacterMovement>();
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
        inventoryStore.items.Add(gameObject);
        inventoryStore.RefreshList();
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
