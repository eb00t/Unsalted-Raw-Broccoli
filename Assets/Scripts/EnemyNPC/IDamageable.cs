using UnityEngine;
public interface IDamageable
{
    int Attack { get; set; }
    int Poise { get; set; }
    int PoiseDamage { get; set; }
    Vector3 KnockbackPower { get; set; }
    bool isPlayerInRange { get; set; }
    bool isDead { get; set; }
    public RoomScripting RoomScripting { get; set; }
    public EnemySpawner EnemySpawner { get; set; }
    void TakeDamage(int damage, int? poiseDmg, Vector3? knockback);
    void TriggerStatusEffect(ConsumableEffect effect);
    void ApplyKnockback(Vector2 knockbackPower);
}
