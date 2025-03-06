using UnityEngine;
public interface IDamageable
{
    int Attack { get; set; }
    int Poise { get; set; }
    bool isPlayerInRange { get; set; }
    public RoomScripting RoomScripting { get; set; }
    public Spawner Spawner { get; set; }
    void TakeDamage(int damage, int? poiseDmg, Vector3? knockback);
    void TriggerStatusEffect(ConsumableEffect effect);
    void ApplyKnockback(Vector2 knockbackPower);
}
