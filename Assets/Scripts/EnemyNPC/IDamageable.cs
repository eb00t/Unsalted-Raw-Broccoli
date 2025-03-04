using UnityEngine;
public interface IDamageable
{
    int Attack { get; set; }
    void TakeDamage(int damage);
    void TriggerStatusEffect(ConsumableEffect effect);
    void ApplyKnockback(Vector2 KnockbackPower);
}
