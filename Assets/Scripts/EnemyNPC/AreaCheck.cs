using System;
using UnityEngine;

public class AreaCheck : MonoBehaviour
{
    [SerializeField] private GameObject activeEnemy;
    private IDamageable _damageable;

    private void Start()
    {
        _damageable = activeEnemy.GetComponent<IDamageable>();
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
