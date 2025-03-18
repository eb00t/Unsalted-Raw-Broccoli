using System;
using System.Collections;
using UnityEngine;
public class HitboxHandler : MonoBehaviour
{
    [SerializeField] private bool _canDamage = true;
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        Debug.Log("Player Hit");
        
        if (!_canDamage) return;
        
        var characterAttack = other.GetComponentInChildren<CharacterAttack>();
        var damageable = GetComponentInParent<IDamageable>();
        
        characterAttack.TakeDamagePlayer(damageable.Attack, damageable.PoiseDamage);
        StartCoroutine(AtkCooldown());
    }

    private IEnumerator AtkCooldown()
    {
        _canDamage = false;
        yield return new WaitForSecondsRealtime(0.25f);
        _canDamage = true;
    }

    private void OnEnable()
    {
        _canDamage = true;
    }
}