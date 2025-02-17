using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SemiSolidPlatformTrigger : MonoBehaviour
{
    private SemiSolidPlatform _semiSolidPlatform;
 
    private void Awake()
    {
        _semiSolidPlatform = transform.parent.GetComponent<SemiSolidPlatform>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _semiSolidPlatform.canDropThrough = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _semiSolidPlatform.canDropThrough = false;
        }

    }
}
