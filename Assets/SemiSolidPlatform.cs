using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class SemiSolidPlatform : MonoBehaviour
{
    private BoxCollider _boxCollider;
    private LayerMask _layerToIgnore;
    private void Awake()
    {
        _boxCollider = GetComponent<BoxCollider>();
    }

    private void OnTriggerEnter(Collider other)
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
    }

    public void TurnOffCollision(LayerMask layerMask)
    {
        _boxCollider.excludeLayers += layerMask;
    }

    public void TurnOnCollision(LayerMask layerMask)
    {
        _boxCollider.excludeLayers -= layerMask;
    }

    
}
