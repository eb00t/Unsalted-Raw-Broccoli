using System;
using UnityEngine;

public class AreaCheck : MonoBehaviour
{
    private IDamageable _damageable;

    private void Start()
    {
        _damageable = GetComponentInParent<IDamageable>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _damageable.isPlayerInRange = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _damageable.isPlayerInRange = false;
        }
    }
}
