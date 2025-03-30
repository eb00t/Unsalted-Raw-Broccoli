using System;
using System.Collections;
using FMOD.Studio;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class CloneBossHandler : MonoBehaviour, IDamageable
{
    [Header("Defensive Stats")] 
    [NonSerialized] public int maxHealth;
    public int health;
    [SerializeField] private int poise;
    [SerializeField] private int defense;
    [SerializeField] private int poisonResistance;

    [Header("Offensive Stats")] 
    [SerializeField] private int attack;
    [SerializeField] private int poiseDamage;
    [SerializeField] private int numberOfAttacks;

    [Header("Tracking")] 
    [SerializeField] private float attackRange;
    [SerializeField] private float chaseRange;
    private int _knockbackDir;
    private bool _hasPlayerBeenSeen;
    private Vector3 _lastPosition;
    private Vector3 _playerDir;
    private enum States { Idle, Chase, Attack, Frozen, Passive }
    private States _state = States.Idle;

    [Header("Timing")]
    [SerializeField] private float attackCooldown; // time between attacks
    [SerializeField] private float chaseDuration; // how long the enemy will chase after player leaves range
    [SerializeField] private float freezeDuration; // how long the enemy is frozen for
    [SerializeField] private float freezeCooldown; // how long until the enemy can be frozen again
    [SerializeField] private float maxTimeToReachTarget; // how long will the enemy try to get to the target before switching
    private float _timeSinceLastMove;
    private float _targetTime;

    [Header("Enemy Properties")] 
    [SerializeField] private bool canBeFrozen;
    [SerializeField] private bool canBeStunned;
    [SerializeField] private bool isPassive;
    private bool _isFrozen;
    private bool _isPoisoned;
    private bool _lowHealth;
    private bool _isStuck;
    private int _poisonBuildup;
    private int _poiseBuildup;

    [Header("References")] 
    [SerializeField] private Transform passiveTarget;
    [SerializeField] private BoxCollider attackHitbox;
    [SerializeField] private Image healthFillImage;
    public CloneBossManager cloneBossManager;
    private Slider _healthSlider;
    private Animator _animator;
    private Transform _target;
    private NavMeshAgent _agent;
    private CharacterAttack _characterAttack;
    private SpriteRenderer _spriteRenderer;
    private MaterialPropertyBlock _propertyBlock;
    private static readonly int BaseColor = Shader.PropertyToID("_BaseColor");
    
    [Header("Sound")]
    private EventInstance _alarmEvent;
    private EventInstance _deathEvent;
    
    int IDamageable.Attack { get => attack; set => attack = value; }
    int IDamageable.Poise { get => poise; set => poise = value; }
    int IDamageable.PoiseDamage { get => poiseDamage; set => poiseDamage = value; }
    public bool isPlayerInRange { get; set; }
    public bool isDead { get; set; }
    public RoomScripting RoomScripting { get; set; }
    public EnemySpawner EnemySpawner { get; set; }
    
    private void Start()
    {
        _healthSlider = GetComponentInChildren<Slider>();
        _animator = GetComponent<Animator>();
        _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        
        _target = GameObject.FindGameObjectWithTag("Player").transform;
        _agent = GetComponent<NavMeshAgent>();
        _agent.updateRotation = false;
        _agent.updateUpAxis = false;
        _agent.updatePosition = true;
        _propertyBlock = new MaterialPropertyBlock();
        
        gameObject.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
        
        _deathEvent = AudioManager.Instance.CreateEventInstance(FMODEvents.Instance.EnemyDeath);
    }

    private void Update()
    {
        var vector3 = transform.position;
        vector3.z = 0;
        transform.position = vector3;
        
        if (isPassive)
        {
            health = maxHealth;
            _healthSlider.value = maxHealth;
        }
        var velocity = _agent.velocity;
        var distance = Vector3.Distance(transform.position, _target.position);
        _playerDir = _target.position - transform.position;
        
        if (_isFrozen)
        {
            _state = States.Frozen;
        }
        else if (isPassive)
        {
            _state = States.Passive;
            _playerDir = passiveTarget.position - transform.position;
            
            UpdateSpriteDirection(_playerDir.x < 0);
            
            if (velocity.x > 0.1f)
            {
                UpdateSpriteDirection(false);
            }
            else if (velocity.x < -0.1f)
            {
                UpdateSpriteDirection(true);
            }
            
            _target = passiveTarget;
        }
        else
        {
            if (distance < attackRange)
            {
                _state = States.Attack;
            }
            else if (distance < chaseRange || _hasPlayerBeenSeen)
            {
                _state = States.Chase;
            
                if (velocity.x > 0.1f)
                {
                    UpdateSpriteDirection(false);
                }
                else if (velocity.x < -0.1f)
                {
                    UpdateSpriteDirection(true);
                }
            }
            else
            {
                _state = States.Idle;
            }
        }
        
        switch (_state)
        {
            case States.Idle:
                _agent.isStopped = true;
                break;
            case States.Chase:
                Chase();
                break;
            case States.Attack:
                Attack();
                break;
            case States.Frozen:
                _agent.isStopped = true;
                Frozen();
                break;
            case States.Passive:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        
        _animator.SetFloat("vel", Mathf.Abs(velocity.x));
    }

    private void UpdateSpriteDirection(bool isLeft)
    {
        var localScale = _spriteRenderer.transform.localScale;
        
        if (!isLeft)
        {
            localScale = new Vector3(Mathf.Abs(localScale.x), localScale.y, localScale.z);
        }
        else
        {
            localScale = new Vector3(-Mathf.Abs(localScale.x), localScale.y, localScale.z);
        }
        
        _spriteRenderer.transform.localScale = localScale;
        attackHitbox.transform.localScale = localScale;
    }

    private void Chase()
     {
         _agent.isStopped = false;
         _healthSlider.gameObject.SetActive(true);
     
         if (_hasPlayerBeenSeen == false)
         {
             StartCoroutine(StartChaseDelay());
         }
         
         var offset = Vector3.zero;
     
         foreach (var other in cloneBossManager.cloneBossHandlers)
         {
             if (other == this || other.isDead) continue;

             var dist = Vector3.Distance(transform.position, other.transform.position);

             if (dist > 0.01f && dist < 4f)
             {
                 offset += (transform.position - other.transform.position).normalized / dist;
             }
         }
         
         var destination = _target.position + new Vector3(offset.x, offset.y, 0);
         destination.z = transform.position.z;
     
         _agent.SetDestination(destination);
     }
    
    private IEnumerator StartChaseDelay()
    {
        _hasPlayerBeenSeen = true;
        yield return new WaitForSecondsRealtime(chaseDuration);
        _hasPlayerBeenSeen = false;
    }

    private void Attack()
    {
        _agent.ResetPath();
        _agent.isStopped = true;
        _healthSlider.gameObject.SetActive(true);
        _targetTime -= Time.deltaTime;
        
        UpdateSpriteDirection(_playerDir.x < 0);

        if (!(_targetTime <= 0.0f)) return;
        
        var i = Random.Range(0, numberOfAttacks);

        switch (i)
        {
            case 0:
                _animator.SetTrigger("Attack");
                break;
            case 1:
                _animator.SetTrigger("Attack2");
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        
        _targetTime = attackCooldown;
    }

    private void Frozen()
    {
        if (!canBeFrozen) return;
        StartCoroutine(BeginFreeze());
    }

    private IEnumerator BeginFreeze()
    {
        StartCoroutine(StartCooldown());
        healthFillImage.color = Color.cyan;
        _agent.velocity = Vector3.zero;
        yield return new WaitForSecondsRealtime(freezeDuration);
        healthFillImage.color = new Color(1f, .48f, .48f, 1);
        _isFrozen = false;
    }

    private IEnumerator StartCooldown()
    {
        canBeFrozen = false;
        yield return new WaitForSecondsRealtime(freezeCooldown);
        canBeFrozen = true;
    }

    private IEnumerator TakePoisonDamage()
    {
        if (_poisonBuildup > 0)
        {
            _isPoisoned = true;
            healthFillImage.color = new Color(0, .83f, .109f, 1f);
            var damageToTake = maxHealth / 100 * 3;
            _poisonBuildup -= 5;
            TakeDamage(damageToTake, null, null);
        }
        else
        {
            _isPoisoned = false;
            healthFillImage.color = new Color(1f, .48f, .48f, 1);
            yield break;
        }
        
        yield return new WaitForSecondsRealtime(2f);
        StartCoroutine(TakePoisonDamage());
    }

    public void TakeDamage(int damage, int? poiseDmg, Vector3? knockback)
    {
        defense = Mathf.Clamp(defense, 0, 100);
        var dmgReduction = (100 - defense) / 100f;
        damage = Mathf.RoundToInt(damage * dmgReduction);
        
        if (health - damage > 0)
        {
            health -= damage;
            _healthSlider.value = health;
            StartCoroutine(HitFlash(Color.red, 0.05f));
        }
        else
        {
            health = 0;
            _healthSlider.value = 0;
            Die();
        }

        if (health <= maxHealth / 3 && _lowHealth == false)
        {
           PlayAlarmSound();
           _lowHealth = true;
        }

        if (poiseDmg.HasValue)
        {
            _poiseBuildup += poiseDmg.Value;

            if (knockback.HasValue)
            {
                ApplyKnockback(knockback.Value);
            }
        }
        
        cloneBossManager.UpdateCollectiveHealth();
    }
    
    public void TriggerStatusEffect(ConsumableEffect effect)
    {
        switch (effect)
        {
            case ConsumableEffect.Ice:
                if (!canBeFrozen) return;
                _isFrozen = true;
                break;
            case ConsumableEffect.Poison:
                if (_isPoisoned) return;
                _poisonBuildup += 10;
                
                if (_poisonBuildup >= poisonResistance)
                {
                    StartCoroutine(TakePoisonDamage());
                }
                break;
        }
    }
    
    public void Die()
    {
        isDead = true;
        cloneBossManager.cloneBossHandlers.Remove(this);
        
        StopAllCoroutines();
        
        gameObject.SetActive(false);
        StopAlarmSound();
    }
    
    public void PlayAlarmSound()
    {
        _alarmEvent = AudioManager.Instance.CreateEventInstance(FMODEvents.Instance.EnemyLowHealthAlarm);
        AudioManager.Instance.AttachInstanceToGameObject(_alarmEvent, gameObject.transform);
        _alarmEvent.start();
    }

    public void StopAlarmSound()
    {
        _alarmEvent.stop(STOP_MODE.IMMEDIATE);
        _alarmEvent.release();
    }

    public void ApplyKnockback(Vector2 knockbackPower)
    {
        if (_isFrozen || isDead) return;

        _knockbackDir = transform.position.x > _target.position.x ? 1 : -1;
        
        var knockbackMultiplier = (_poiseBuildup >= poise) ? 2f : 0.5f; 
        var knockbackForce = new Vector3(knockbackPower.x * _knockbackDir * knockbackMultiplier, knockbackPower.y * knockbackMultiplier, 0);

        StartCoroutine(TriggerKnockback(knockbackForce, 0.2f));
        StartCoroutine(ApplyVerticalKnockback(knockbackPower.y, .2f));
        //StartCoroutine(StunTimer(.1f));
        
        _animator.SetTrigger("lightStagger");

        if (_poiseBuildup >= poise)
        {
            StartCoroutine(StunTimer(1f));
            _poiseBuildup = 0;
        }
    }
    
    private IEnumerator TriggerKnockback(Vector3 force, float duration)
    {
        var elapsedTime = 0f;
        var startPos = transform.position;
        var targetPos = startPos + force;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            var t = elapsedTime / duration;
            transform.position = Vector3.Lerp(startPos, targetPos, t);
            yield return null;
        }
    }
    
    private IEnumerator ApplyVerticalKnockback(float height, float dur)
    {
        var elapsedTime = 0f;
        var startOffset = _agent.baseOffset;

        while (elapsedTime < dur)
        {
            elapsedTime += Time.deltaTime;
            _agent.baseOffset = startOffset + Mathf.Sin(elapsedTime / dur * Mathf.PI) * height;
            yield return null;
        }

        _agent.baseOffset = startOffset;
    }

    private IEnumerator StunTimer(float stunTime)
    {
        _animator.SetBool("isStaggered", true);
       yield return new WaitForSecondsRealtime(stunTime);
       _agent.velocity = Vector3.zero;
       _animator.SetBool("isStaggered", false);
    }
    
    private IEnumerator HitFlash(Color flashColor, float duration)
    {
        _spriteRenderer.GetPropertyBlock(_propertyBlock);
        _propertyBlock.SetColor(BaseColor, flashColor);
        _spriteRenderer.SetPropertyBlock(_propertyBlock);

        yield return new WaitForSecondsRealtime(duration);
        
        var elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            var newColor = Color.Lerp(flashColor, flashColor, elapsed / duration);

            _spriteRenderer.GetPropertyBlock(_propertyBlock);
            _propertyBlock.SetColor(BaseColor, newColor);
            _spriteRenderer.SetPropertyBlock(_propertyBlock);

            yield return null;
        }
        
        _spriteRenderer.GetPropertyBlock(_propertyBlock);
        _propertyBlock.SetColor(BaseColor, Color.white);
        _spriteRenderer.SetPropertyBlock(_propertyBlock);
    }
}