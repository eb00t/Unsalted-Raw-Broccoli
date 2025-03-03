public interface IDamageable
{
    int Attack { get; set; }
    void TakeDamage(int damage);
    void TriggerStatusEffect(ConsumableEffect effect);
}
