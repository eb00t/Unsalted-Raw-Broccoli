using System;
using System.Collections;
using UnityEngine;
public class HitboxHandler : MonoBehaviour
{
    [SerializeField] private bool _canDamage = true;
    public IDamageable damageable;
    [SerializeField] private bool doesSelfDestruct;
    [SerializeField] private bool isConstantDamage;
    [SerializeField] private float damageDelay = 0.25f;
    [SerializeField] private GameObject impactVFX;
    [SerializeField] private Transform impactOrigin;

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
                else if (!isConstantDamage)
                {
                    var sign = Mathf.Sign(GetComponentInParent<Animator>().transform.localScale.x);
                    var rot = sign == 1 ? 90f : -90f;
                    var vfx = Instantiate(impactVFX, impactOrigin.position, Quaternion.Euler(0f, 0f, rot));
                    vfx.transform.localScale = new Vector3(Mathf.Abs(vfx.transform.localScale.x) * sign * 0.5f, vfx.transform.localScale.y * 0.5f, vfx.transform.localScale.z);
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
        yield return new WaitForSecondsRealtime(damageDelay);
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