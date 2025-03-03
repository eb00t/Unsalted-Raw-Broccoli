using UnityEngine;
public class HitboxHandler : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        var characterAttack = other.GetComponentInChildren<CharacterAttack>();
        var atk = GetComponentInParent<IDamageable>().Attack;
        
        characterAttack.TakeDamagePlayer(atk);
    }
}
