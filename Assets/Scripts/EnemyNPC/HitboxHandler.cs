using System.Collections;
using UnityEngine;
public class HitboxHandler : MonoBehaviour
{
    private bool _canDamage = true;
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (!_canDamage) return;
        
        var characterAttack = other.GetComponentInChildren<CharacterAttack>();
        var atk = GetComponentInParent<IDamageable>().Attack;
        
        characterAttack.TakeDamagePlayer(atk);
        StartCoroutine(AtkCooldown());
    }

    private IEnumerator AtkCooldown()
    {
        _canDamage = false;
        yield return new WaitForSecondsRealtime(0.5f);
        _canDamage = true;
    }
}
