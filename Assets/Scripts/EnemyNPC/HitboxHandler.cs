using System;
using System.Collections;
using UnityEngine;
public class HitboxHandler : MonoBehaviour
{
    [SerializeField] private bool _canDamage = true;
    private bool _canDamageEnemies = true;

    private void OnTriggerEnter(Collider other)
    {
        var damageable = GetComponentInParent<IDamageable>();
        
        if (other.CompareTag("Player"))
        {
            if (!_canDamage) return;

            var characterAttack = other.GetComponentInChildren<CharacterAttack>();

            characterAttack.TakeDamagePlayer(damageable.Attack, damageable.PoiseDamage);
            StartCoroutine(AtkCooldown());
        }

        if (other.CompareTag("Enemy"))
        {
            var enemyHandler = GetComponentInParent<EnemyHandler>();
            
            if (enemyHandler != null && enemyHandler.isBomb)
            {
                var otherDamageable = other.GetComponent<IDamageable>();

                if (!_canDamageEnemies || otherDamageable == null) return;
                
                otherDamageable.TakeDamage(damageable.Attack, damageable.PoiseDamage, null);
                StartCoroutine(AtkCooldown());
            }
        }
    }

    private IEnumerator AtkCooldown()
    {
        _canDamage = false;
        _canDamageEnemies = false;
        yield return new WaitForSecondsRealtime(0.25f);
        _canDamage = true;
        _canDamageEnemies = true;
    }

    private void OnEnable()
    {
        _canDamage = true;
        _canDamageEnemies = true;
    }
}