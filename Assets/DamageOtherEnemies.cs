using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageOtherEnemies : MonoBehaviour
{
    private IDamageable _damageable;

    private void Start()
    {
        _damageable = GetComponentInParent<EnemyHandler>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Enemy")) return;
        var otherDamageable = other.GetComponentInParent<IDamageable>();
        otherDamageable?.TakeDamage(_damageable.Attack, _damageable.PoiseDamage, new Vector3(1, 1, 0));
    }
}
