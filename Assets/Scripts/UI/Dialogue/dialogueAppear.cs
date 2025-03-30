using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class dialogueAppear : MonoBehaviour
{
    public GameObject dialogueBox, enemyChar;
    private ItemPickupHandler _itemPickupHandler;
    [SerializeField] private float range;
    private List<dialogueControllerScript> _dialogueControllerScripts;

    private void Start()
    { 
        _itemPickupHandler = GetComponent<ItemPickupHandler>();
    }
    
    private void Update()
    {
        
    }
}
