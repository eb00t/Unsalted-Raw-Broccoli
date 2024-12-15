using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UIElements;

public class SemiSolidPlatform : MonoBehaviour
{
    private BoxCollider _boxCollider;
    public GameObject playerFeet;
    public CharacterMovement characterMovement;
    public LayerMask interactableObjects;
    public LayerMask layersToExclude, layersToInclude;
    public LayerMask _layerToIgnore;
    public bool collisionOff;
    private void Awake()
    {
        _boxCollider = GetComponent<BoxCollider>();
        collisionOff = false;
    }

    void Start()
    {
        if (playerFeet == null)
        {
            playerFeet = GameObject.FindWithTag("Player Ground Position");
        }
        characterMovement = playerFeet.transform.root.gameObject.GetComponent<CharacterMovement>();
    }
    
   /* private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            _layerToIgnore = LayerMask.GetMask("Player");
        }
        else if (other.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            _layerToIgnore = LayerMask.GetMask("Enemy");
        }
        TurnOffCollision(_layerToIgnore);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            _layerToIgnore = LayerMask.GetMask("Player");
        }
        else if (other.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            _layerToIgnore = LayerMask.GetMask("Enemy");
        }
        TurnOnCollision(_layerToIgnore);
    }*/
    
    public void TurnOffCollision(LayerMask layerMask)
    {
        collisionOff = true;
        _boxCollider.excludeLayers += layerMask;
    }
    public void TurnOnCollision(LayerMask layerMask)
    {
        collisionOff = false;
        _boxCollider.excludeLayers -= layerMask;
    }


    private void Update()
    {
        if (playerFeet.transform.position.y < transform.position.y && collisionOff == false)
        {
            _layerToIgnore = LayerMask.GetMask("Player");
            TurnOffCollision(_layerToIgnore);
        }
        else if (playerFeet.transform.position.y > transform.position.y && collisionOff)
        {
            _layerToIgnore = LayerMask.GetMask("Player");
            TurnOnCollision(_layerToIgnore);
        }

        if (characterMovement.crouching)
        {
            _layerToIgnore = LayerMask.GetMask("Player");
            TurnOffCollision(_layerToIgnore);
        }
    }
}