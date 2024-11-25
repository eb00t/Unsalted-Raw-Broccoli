using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SemiSolidPlatform : MonoBehaviour
{
    private BoxCollider _boxCollider;
    public string _layerToIgnore;
    public LayerMask interactableObjects;
    public LayerMask layersToExclude, layersToInclude;
    private void Awake()
    {
        _boxCollider = GetComponent<BoxCollider>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            _layerToIgnore = "Player";
        }
        else if (other.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            _layerToIgnore = "Enemy";
        }
        TurnOffCollision(_layerToIgnore);
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            _layerToIgnore = "Player";
        }
        else if (other.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            _layerToIgnore = "Enemy";
        }
        TurnOnCollision(_layerToIgnore);
    }

    public void TurnOffCollision(string layerMask)
    {
        _boxCollider.excludeLayers += LayerMask.NameToLayer(layerMask);
    }

    public void TurnOnCollision(string layerMask)
    {
        _boxCollider.excludeLayers -= LayerMask.NameToLayer(layerMask);
    }
}
