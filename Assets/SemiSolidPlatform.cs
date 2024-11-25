using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SemiSolidPlatform : MonoBehaviour
{
    private BoxCollider _boxCollider;
    public LayerMask layersToExclude, layersToInclude;
    private void Awake()
    {
        _boxCollider = GetComponent<BoxCollider>();
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            
        }
    }

    public void TurnOffCollision()
    {
        //_boxCollider.excludeLayers(layersToExclude);
    }
}
