using System;
using System.Collections;
using UnityEngine;
public class HitboxHandler : MonoBehaviour
{
    [SerializeField] private bool _canDamage = true;
    public IDamageable damageable;
    [SerializeField] private bool doesSelfDestruct;
    [SerializeField] private bool isConstantDamage;

    private void Start()
    {
        if (doesSelfDestruct)
        {
            StartCoroutine(SelfDestructAfterSeconds(15));
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isConstantDamage) return;
        damageable ??= GetComponentInParent<IDamageable>();

        if (other.CompareTag("Player"))
        {
            if (_canDamage)
            {
                var characterAttack = other.GetComponentInChildren<CharacterAttack>();

                characterAttack.TakeDamagePlayer(damageable.Attack, damageable.PoiseDamage, damageable.KnockbackPower);
                if (doesSelfDestruct)
                {
                    Destroy(gameObject);
                }

                StartCoroutine(AtkCooldown());
            }
        }

        if (!other.GetComponent<SemiSolidPlatform>() && !other.GetComponent<SemiSolidPlatformTrigger>() && !other.CompareTag("Player") && !other.CompareTag("Enemy") && !other.isTrigger)
        {
            if (doesSelfDestruct) { Destroy(gameObject); }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (!isConstantDamage) return;
        damageable ??= GetComponentInParent<IDamageable>();
        
        if (other.CompareTag("Player"))
        {
            if (_canDamage)
            {
                var characterAttack = other.GetComponentInChildren<CharacterAttack>();

                characterAttack.TakeDamagePlayer(damageable.Attack, damageable.PoiseDamage, damageable.KnockbackPower);
                if (doesSelfDestruct)
                {
                    Destroy(gameObject);
                }

                StartCoroutine(AtkCooldown());
            }
        }
    }

    private IEnumerator AtkCooldown()
    {
        _canDamage = false;
        yield return new WaitForSecondsRealtime(0.25f);
        _canDamage = true;
    }

    private IEnumerator SelfDestructAfterSeconds(float seconds)
    {
        yield return new WaitForSecondsRealtime(seconds);
        Destroy(gameObject);
    }

    private void OnEnable()
    {
        _canDamage = true;
    }
}