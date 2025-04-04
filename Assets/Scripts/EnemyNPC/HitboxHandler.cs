using System;
using System.Collections;
using UnityEngine;
public class HitboxHandler : MonoBehaviour
{
    [SerializeField] private bool _canDamage = true;
    private bool _canDamageEnemies = true;
    public IDamageable damageable;
    [SerializeField] private bool doesSelfDestruct;

    private void Start()
    {
        if (doesSelfDestruct)
        {
            StartCoroutine(SelfDestructAfterSeconds(15));
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        damageable ??= GetComponentInParent<IDamageable>();

        if (other.CompareTag("Player"))
        {
            if (!_canDamage) return;

            var characterAttack = other.GetComponentInChildren<CharacterAttack>();

            characterAttack.TakeDamagePlayer(damageable.Attack, damageable.PoiseDamage);
            if (doesSelfDestruct) { Destroy(gameObject); }
            StartCoroutine(AtkCooldown());
        }

        if (!other.GetComponent<SemiSolidPlatform>() && !other.GetComponent<SemiSolidPlatformTrigger>() && !other.CompareTag("Player") && !other.CompareTag("Enemy") && !other.isTrigger)
        {
            Debug.Log(other.name);
            if (doesSelfDestruct) { Destroy(gameObject); }
        }

        if (other.CompareTag("Enemy"))
        {
            var enemyHandler = GetComponentInParent<EnemyHandler>();
            
            if (enemyHandler != null && enemyHandler.isBomb)
            {
                var otherDamageable = other.GetComponent<IDamageable>();

                if (otherDamageable == null) return;
                
                otherDamageable.TakeDamage(damageable.Attack, damageable.PoiseDamage, null);
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

    private IEnumerator SelfDestructAfterSeconds(float seconds)
    {
        yield return new WaitForSecondsRealtime(seconds);
        Destroy(gameObject);
    }

    private void OnEnable()
    {
        _canDamage = true;
        _canDamageEnemies = true;
    }
}