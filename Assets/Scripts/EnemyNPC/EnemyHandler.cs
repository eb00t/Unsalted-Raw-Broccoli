using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;
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
    [SerializeField] private int poisonResistance;
    [SerializeField] private float attackRange;
    [SerializeField] private float attackCooldown;
    [SerializeField] private float chaseRange;
    [SerializeField] private float chaseDuration;
    [SerializeField] private float patrolRange;
    [SerializeField] private float freezeDuration;
    [SerializeField] private float freezeCooldown;
    [SerializeField] private bool canFreeze, canBeStunned, isBomb; // if by default set to false the enemy will never freeze
    
    private int _poisonBuildup, _poiseBuildup, _health;
    private bool _isFrozen, _isPoisoned, _hasPlayerBeenSeen;
    private float _targetTime;
    private Vector3 _patrolTarget, _patrolPoint1, _patrolPoint2;
    
    
    [Header("References")]
    [SerializeField] private BoxCollider atkHitbox;
    [SerializeField] private Image healthFillImage;
    private Slider _healthSlider;
    private Animator _animator;
    private Transform _target;
    private NavMeshAgent _agent;
    private CharacterAttack _characterAttack;
    private SpriteRenderer _spriteRenderer;
    
    [SerializeField] private bool isIdle, debugPatrol, debugRange;
    [SerializeField] private float maxTimeToReachTarget = 5f;
    private float _timeSinceLastMove;
    private Vector3 _lastPosition;
    private bool _isStuck;
    
    int IDamageable.Attack { get => attack; set => attack = value; }
    int IDamageable.Poise { get => poise; set => poise = value; }

    public bool isPlayerInRange { get; set; }
    public RoomScripting RoomScripting { get; set; }
    public Spawner Spawner { get; set; }

    private int _knockbackDir;
    
    private void Start()
    {
        RoomScripting = gameObject.transform.root.GetComponent<RoomScripting>();
        RoomScripting.enemies.Add(gameObject);
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
        
        PickPatrolPoints();
        _patrolTarget = _patrolPoint1;
        if (gameObject.name.Contains("Stalker"))
        {
            gameObject.transform.localScale = new Vector3(1.25f, 1.25f, 1.25f);
        }

        if (isBomb)
        {
            _targetTime = attackCooldown;
        }
    }

    private void Update()
    {
        var distance = Vector3.Distance(transform.position, _target.position);

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
            }
            else if (!isIdle)
            {
                _state = States.Patrol;
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
        
        var velocity = _agent.velocity;
        
        _animator.SetFloat("vel", Mathf.Abs(velocity.x));

        var localScale = _spriteRenderer.transform.localScale;
        
        if (velocity.x > 0.1f)
        {
            localScale = new Vector3(Mathf.Abs(localScale.x), localScale.y, localScale.z);
            atkHitbox.center = new Vector3(1.2f, -0.1546797f, 0);
        }
        else if (velocity.x < -0.1f)
        {
            localScale = new Vector3(-Mathf.Abs(localScale.x), localScale.y, localScale.z);
            atkHitbox.center = new Vector3(-1.2f, -0.1546797f, 0);
        }
        
        _spriteRenderer.transform.localScale = localScale;
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

        if (!(_targetTime <= 0.0f)) return;
        
        if (isBomb)
        {
            StartCoroutine(BeginExplode());
        }
        else
        {
            _animator.SetTrigger("Attack"); 
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

        if (_poiseBuildup >= poise)
        {
            if (knockback.HasValue)
            {
                ApplyKnockback(knockback.Value);
            }
        }
        else
        {
            if (poiseDmg.HasValue)
            {
                _poiseBuildup += poiseDmg.Value;
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
        Spawner.spawnedEnemy = null;
        Spawner.SpawnEnemies();
        gameObject.SetActive(false);
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
        RoomScripting.enemies.Remove(gameObject);
        RoomScripting._enemyCount--;
        Spawner.spawnedEnemies.Remove(gameObject);
    }

    public void ApplyKnockback(Vector2 knockbackPower)
    {
        if (_isFrozen || _poiseBuildup < poise) return;
        
        _poiseBuildup = 0;
        _knockbackDir = transform.position.x > _target.position.x ? 1 : -1;
        
        var knockbackForce = new Vector3(knockbackPower.x * _knockbackDir, knockbackPower.y, 0);
        _agent.velocity = knockbackForce; 
        
        StartCoroutine(ApplyVerticalKnockback(knockbackPower.y, .5f));
        StartCoroutine(StunTimer(.5f));
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
       yield return new WaitForSecondsRealtime(stunTime);
       _agent.velocity = Vector3.zero;
    }
}