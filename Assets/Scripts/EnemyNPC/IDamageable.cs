using UnityEngine;
public interface IDamageable
{
    int Attack { get; set; }
    public RoomScripting RoomScripting { get; set; }
    public Spawner Spawner { get; set; }
    void TakeDamage(int damage);
    void TriggerStatusEffect(ConsumableEffect effect);
    void ApplyKnockback(Vector2 knockbackPower);
}
