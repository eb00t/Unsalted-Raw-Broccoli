using FMODUnity;
using UnityEngine;

public class FMODEvents : MonoBehaviour
{ 
    /* EVENT RULES
    1. Use separate events for different entities, e.g., the Player getting hurt and an Infantry getting hurt should
    have two events (PlayerHurt and InfantryHurt).
    2. Use the same event for different intensities of the same sound effect. Use a local parameter in most cases.
    3. Do not touch any code that is written already, as this may cause the assigned event references to be lost.
    This means someone will have to reassign them all again manually. Nobody wants to do that. */
    public static FMODEvents Instance { get; private set; }
    
    /*[field: Header("INSERT NAME IF NECESSARY")]
    [field: SerializeField] public EventReference INSERT NAME HERE { get; private set; }*/
    
    
    [field: Header("Ambience")]
    [field: SerializeField] public EventReference Ambience { get; private set; }
    [field: SerializeField] public EventReference Loading { get; private set; }
    
    [field: Header("Music")]
    [field: SerializeField] public EventReference Music { get; private set; }
    [field: Header("SFX")]
    [field: SerializeField] public EventReference DoorSlam { get; private set; }
    [field: Header("Player SFX")]
    [field: SerializeField] public EventReference PlayerFootsteps { get; private set; }
    [field: SerializeField] public EventReference PlayerJump { get; private set; }
    [field: SerializeField] public EventReference PlayerLightAttack { get; private set; }
    [field: SerializeField] public EventReference PlayerMediumAttack { get; private set; }
    [field: SerializeField] public EventReference PlayerHeavyAttack { get; private set; }
    [field: Header("Enemy SFX")]
    [field: SerializeField] public EventReference EnemyFootsteps { get; private set; }
    [field: SerializeField] public EventReference BombEnemyFootsteps { get; private set; }
    [field: SerializeField] public EventReference EnemyLowHealthAlarm { get; private set; }
    [field: SerializeField] public EventReference EnemyDamage {get; private set;}
    [field: SerializeField] public EventReference EnemyDeath { get; private set; }
    [field: SerializeField] public EventReference Explosion { get; private set; }
    [field: Header("Hands Boss SFX")]
    [field: SerializeField] public EventReference BossHandSlam { get; private set; }
    [field: Header("Copy Boss SFX")]
    [field: SerializeField] public EventReference CopyBossFootsteps { get; private set; }
    [field: SerializeField] public EventReference CopyBossJump { get; private set; }
    [field: SerializeField] public EventReference CopyBossLightAttack { get; private set; }
    [field: SerializeField] public EventReference CopyBossMediumAttack { get; private set;}
    [field: SerializeField] public EventReference CopyBossHeavyAttack { get; private set; }
    [field:Header("UI")]
    [field: SerializeField] public EventReference UINavigate { get; private set; }
    [field: SerializeField] public EventReference UISelect { get; private set; }
    [field: SerializeField] public EventReference UIBack { get; private set; }
    [field: SerializeField] public EventReference DialogueScroll { get; private set; }
    [field: SerializeField] public EventReference CurrencyPickup { get; private set; }
    [field: SerializeField] public EventReference ItemPickup { get; private set; }
    [field: SerializeField] public EventReference ItemActivate { get; private set; }
    [field: SerializeField] public EventReference CycleItem { get; private set; }
    [field: SerializeField] public EventReference PurchaseMade { get; private set; }
    [field: SerializeField] public EventReference PurchaseFailed { get; private set; }
    
    
    
     
    
    private void Awake() 
    {
        if (Instance != null)
        {
            Debug.LogError("More than one FMOD Events script in scene.");
        }

        Instance = this;
    }
}
