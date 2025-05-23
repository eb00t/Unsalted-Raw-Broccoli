public enum ConsumableEffect
{
    None,
    Heal, // Heals 50%
    RouletteHeal, // ?
    GiveCurrency, // gives player currency
    DamageBuff, // increases player attack power
    Poison,
    Ice,
    Invincibility, // give player invincibility during enemy attack string
    HorseFact // show fact about horses
}

public enum PassiveEffect
{
    None,
    DefenseIncrease,
    AttackIncrease,
    HpChanceOnKill,
    SurviveLethalHit,
    PassiveEnergyRegen,
    Companion,
    LuckIncrease
}