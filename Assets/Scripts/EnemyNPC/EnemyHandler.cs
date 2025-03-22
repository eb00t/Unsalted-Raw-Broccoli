using System;
using System.Collections;
using FMOD.Studio;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Random = UnityEngine.Random;


// Adapted from https://github.com/Chaker-Gamra/2.5D-Platformer-Game/blob/master/Assets/Scripts/Enemy/Enemy.cs

public class EnemyHandler : MonoBehaviour, IDamageable
{
    private enum States { Idle, Patrol, Chase, Attack, Frozen }
    private States _state = States.Idle;
    
    [Header("Enemy Stats")]
    [SerializeField] private int maxHealth;
    [SerializeField] private int attack;
    [SerializeField] private int poise;
    [SerializeField] private int poiseDamage;
    [SerializeField] private int poisonResistance;
    [SerializeField] private float attackRange;
    [SerializeField] private float attackCooldown;
    [SerializeField] private float chaseRange;
    [SerializeField] private float chaseDuration;
    [SerializeField] private float patrolRange;
    [SerializeField] private float freezeDuration;
    [SerializeField] private float freezeCooldown;
    [SerializeField] private bool canFreeze, canBeStunned, isBomb; // if by default set to false the enemy will never freeze
    [SerializeField] private int atkNumber;
    
    private int _poisonBuildup, _poiseBuildup, _health;
    private bool _isFrozen, _isPoisoned, _hasPlayerBeenSeen;
    private float _targetTime;
    private Vector3 _patrolTarget, _patrolPoint1, _patrolPoint2;
    private Vector3 playerDir;
    
    [Header("References")]
    [SerializeField] private BoxCollider atkHitbox;
    [SerializeField] private Image healthFillImage;
    private Slider _healthSlider;
    private Animator _animator;
    private Transform _target;
    private NavMeshAgent _agent;
    private CharacterAttack _characterAttack;
    private CharacterMovement _characterMovement;
    private LockOnController _lockOnController;
    private SpriteRenderer _spriteRenderer;
    private EventInstance _alarmEvent;
    private EventInstance _deathEvent;
    
    [SerializeField] private bool isIdle, debugPatrol, debugRange;
    [SerializeField] private float maxTimeToReachTarget = 5f;
    private float _timeSinceLastMove;
    private Vector3 _lastPosition;
    private int _knockbackDir;
    private bool _isStuck;
    private bool _lowHealth;
    
    int IDamageable.Attack { get => attack; set => attack = value; }
    int IDamageable.Poise { get => poise; set => poise = value; }
    int IDamageable.PoiseDamage { get => poiseDamage; set => poiseDamage = value; }
    public bool isPlayerInRange { get; set; }
    public bool isDead { get; set; }
    public RoomScripting RoomScripting { get; set; }
    public Spawner Spawner { get; set; }
    
    private void Start()
    {
        if (!SceneManager.GetActiveScene().name.Contains("Tutorial"))
        {
            RoomScripting = gameObject.transform.root.GetComponent<RoomScripting>();
            RoomScripting.enemies.Add(gameObject);
        }

        _healthSlider = GetComponentInChildren<Slider>();
        _healthSlider.maxValue = maxHealth;
        _healthSlider.value = maxHealth;
        _health = maxHealth;
        _healthSlider.gameObject.SetActive(false);
        _animator = GetComponent<Animator>();
        _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        
        _target = GameObject.FindGameObjectWithTag("Player").transform;
        _agent = GetComponent<NavMeshAgent>();
        _agent.updateRotation = false;
        _characterMovement = _target.GetComponent<CharacterMovement>();
        _lockOnController = _target.GetComponent<LockOnController>();
        
        PickPatrolPoints();
        _patrolTarget = _patrolPoint1;
        if (gameObject.name.Contains("Stalker"))
        {
            gameObject.transform.localScale = new Vector3(1.25f, 1.25f, 1.25f);
        }
        
        if (gameObject.name.Contains("Robot"))
        {
            gameObject.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
        }

        if (isBomb)
        {
            _targetTime = attackCooldown;
        }

        _deathEvent = AudioManager.Instance.CreateEventInstance(FMODEvents.Instance.EnemyDeath);
    }

    private void Update()
    {
        var distance = Vector3.Distance(transform.position, _target.position);
        playerDir = _target.position - transform.position;
        var velocity = _agent.velocity;

        if (_isFrozen)
        {
            _state = States.Frozen;
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
            else if (!isIdle)
            {
                _state = States.Patrol;
                
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
                _healthSlider.gameObject.SetActive(false);
                break;
            case States.Patrol:
                Patrol();
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
            //atkHitbox.center = new Vector3(1.2f, -0.1546797f, 0);
        }
        else
        {
            localScale = new Vector3(-Mathf.Abs(localScale.x), localScale.y, localScale.z);
            //atkHitbox.center = new Vector3(-1.2f, -0.1546797f, 0);
        }
        
        _spriteRenderer.transform.localScale = localScale;
        atkHitbox.transform.localScale = localScale;
    }

    private void Patrol()
    {
        CheckIfStuck();
        if (_agent.pathPending && !_isStuck) return;
        if (!(_agent.remainingDistance <= _agent.stoppingDistance) && !_isStuck) return;
        
        var newTarget = (_patrolTarget == _patrolPoint1) ? _patrolPoint2 : _patrolPoint1;
        
        _patrolTarget = newTarget;
        _agent.SetDestination(_patrolTarget);
        
        _healthSlider.gameObject.SetActive(false);
        _isStuck = false;
        _timeSinceLastMove = 0f;
    }
    
   private void PickPatrolPoints()
    {
        var randomOffset = Random.Range(4f, patrolRange);
        _patrolPoint1 = transform.position + new Vector3(randomOffset, 0, 0);
        _patrolPoint2 = transform.position + new Vector3(-randomOffset, 0, 0);
    }
    
    private void CheckIfStuck()
    {
        if (Vector3.Distance(transform.position, _lastPosition) > 0.1f) 
        {
            _timeSinceLastMove = 0f;
            _lastPosition = transform.position;
            _isStuck = false;
        }
        else
        {
            _timeSinceLastMove += Time.deltaTime;
        }
        
        if (!(_timeSinceLastMove > maxTimeToReachTarget)) return;
        _isStuck = true;
        PickPatrolPoints();
    }


    private void Chase()
    {
        _agent.isStopped = false;
        _healthSlider.gameObject.SetActive(true);
        
        if (_hasPlayerBeenSeen == false)
        {
            StartCoroutine(StartChaseDelay());
        }
        
        _agent.SetDestination(_target.position);
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
        
        _targetTime -= Time.deltaTime;
        
        UpdateSpriteDirection(playerDir.x < 0);

        if (!(_targetTime <= 0.0f)) return;
        
        if (isBomb)
        {
            StartCoroutine(BeginExplode());
        }
        else
        {
            var i = Random.Range(0, atkNumber);

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
        }
        
        _targetTime = attackCooldown;
    }

    private IEnumerator BeginExplode()
    {
        _animator.SetBool("isExplode", true);
        yield return new WaitForSecondsRealtime(attackCooldown);
        _animator.SetBool("isExplode", false);
        _animator.SetTrigger("Detonate");
    }

    private void Frozen()
    {
        if (!canFreeze) return;
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
        canFreeze = false;
        yield return new WaitForSecondsRealtime(freezeCooldown);
        canFreeze = true;
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
        if (_health - damage > 0)
        {
            _health -= damage;
            _healthSlider.value = _health;
        }
        else
        {
            _health = 0;
            _healthSlider.value = 0;
            Die();
        }

        if (_health <= maxHealth / 3 && _lowHealth == false)
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
    }
    
    public void TriggerStatusEffect(ConsumableEffect effect)
    {
        switch (effect)
        {
            case ConsumableEffect.Ice:
                if (!canFreeze) return;
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
    
    private void Die()
    {
        isDead = true;
        _characterMovement.lockedOn = false;
        _lockOnController.lockedTarget = null;
        StopAllCoroutines();

        if (!SceneManager.GetActiveScene().name.Contains("Tutorial"))
        {
            Spawner.spawnedEnemy = null;
            Spawner.SpawnEnemies(); 
        }
        else if (SceneManager.GetActiveScene().name.Contains("Tutorial"))
        {
            _target.GetComponent<TutorialController>().EnemyDefeated();
        }
        
        if (!isBomb)
        {
            AudioManager.Instance.AttachInstanceToGameObject(_deathEvent, gameObject.transform);
            _deathEvent.start();
            _deathEvent.release();
        }
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
    

    private void OnDrawGizmos()
    {
        var position = transform.position;
        //PickPatrolPoints();

        if (debugRange)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(position, attackRange);

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(position, chaseRange);
        }

        if (debugPatrol)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(position, patrolRange);

            var v = new Vector3(1, 1, 1);

            Gizmos.DrawWireCube(_patrolPoint1, v);
            Gizmos.DrawWireCube(_patrolPoint2, v);
        }
    }
    
    private void OnDisable()
    {
        if (!SceneManager.GetActiveScene().name.Contains("Tutorial"))
        {
            RoomScripting.enemies.Remove(gameObject);
            RoomScripting._enemyCount--;
            Spawner.spawnedEnemies.Remove(gameObject);
        }
    }

    public void ApplyKnockback(Vector2 knockbackPower)
    {
        if (_isFrozen || isDead) return;

        _knockbackDir = transform.position.x > _target.position.x ? 1 : -1;
        
        var knockbackMultiplier = (_poiseBuildup >= poise) ? 2f : 0.5f; 
        var knockbackForce = new Vector3(knockbackPower.x * _knockbackDir * knockbackMultiplier, knockbackPower.y * knockbackMultiplier, 0);

        StartCoroutine(TriggerKnockback(knockbackForce, 0.2f));
        StartCoroutine(ApplyVerticalKnockback(knockbackPower.y, .2f));

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
}