using UnityEngine;

public class CompanionCameraHitbox : MonoBehaviour
{
    public CompanionCameraAI companionCameraAI;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            var damageable = other.GetComponentInParent<IDamageable>();
            damageable.TakeDamage(companionCameraAI.attack, companionCameraAI.poiseDamage, Vector3.zero);
        }
    }
}
