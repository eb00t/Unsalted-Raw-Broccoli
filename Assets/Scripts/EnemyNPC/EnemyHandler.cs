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
    [Header("Enemy Stats")]
    [SerializeField] private int maxHealth, poisonResistance, poise;
    [SerializeField] private float atkDelay, attackRange;
    [SerializeField] private float chaseRange, chaseDuration;
    [SerializeField] private float minPatrolRange, maxPatrolRange;
    [SerializeField] private float freezeDuration, freezeCooldown;
    [SerializeField] private bool canFreeze, canBeStunned, isBomb; // if by default set to false the enemy will never freeze or be stunned
    public int attack;
    private int _poisonBuildup, _poiseBuildup, _health;
    private bool _isFrozen, _isPoisoned, _hasPlayerBeenSeen;
    
    [Header("Values")]
    private float _targetTime;
    private Vector3 _patrolTarget, _patrol1, _patrol2;
    private States _state = States.Idle;
    
    [Header("References")]
    [SerializeField] private BoxCollider atkHitbox;
    [SerializeField] private Image healthFillImage;
    private Slider _healthSlider;
    private Animator _animator;
    private Transform _target;
    private NavMeshAgent _agent;
    private CharacterAttack _characterAttack;
    
    [SerializeField] private bool isIdle, debugPatrol, debugRange;
    
    int IDamageable.Attack
    {
        get => attack;
        set => attack = value;
    }
    
    int IDamageable.Poise
    {
        get => poise;
        set => poise = value;
    }

    public bool isPlayerInRange { get; set; }
    public RoomScripting RoomScripting { get; set; }
    public Spawner Spawner { get; set; }

    private int _knockbackDir;

    private enum States
    {
        Idle,
        Patrol,
        Chase,
        Attack,
        Frozen
    }
    
    private void Start()
    {
        RoomScripting = gameObject.transform.root.GetComponent<RoomScripting>();
        RoomScripting.enemies.Add(gameObject);
        gameObject.transform.parent = gameObject.transform.root;
        _healthSlider = GetComponentInChildren<Slider>();
        _healthSlider.maxValue = maxHealth;
        _healthSlider.value = maxHealth;
        _health = maxHealth;
        _healthSlider.gameObject.SetActive(false);
        _animator = GetComponent<Animator>();
        
        _target = GameObject.FindGameObjectWithTag("Player").transform;
        _agent = GetComponent<NavMeshAgent>();
        _agent.updateRotation = false;
        
        PickPatrolPoints();
        _patrolTarget = _patrol1;
        if (gameObject.name.Contains("Stalker"))
        {
            gameObject.transform.localScale = new Vector3(1.25f, 1.25f, 1.25f);
        }

        if (isBomb)
        {
            _targetTime = atkDelay;
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

        var localScale = GetComponentInChildren<SpriteRenderer>().transform.localScale;
        
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
        
        GetComponentInChildren<SpriteRenderer>().transform.localScale = localScale;
    }

    private void Patrol()
    {
        if (_agent.pathPending || !(_agent.remainingDistance <= _agent.stoppingDistance)) return;
        _patrolTarget = _patrolTarget == _patrol1 ? _patrol2 : _patrol1;
        
        _healthSlider.gameObject.SetActive(false);

        _agent.SetDestination(_patrolTarget);
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
        
        _targetTime = atkDelay;
    }

    private IEnumerator BeginExplode()
    {
        _animator.SetBool("isExplode", true);
        yield return new WaitForSecondsRealtime(atkDelay);
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

    private void PickPatrolPoints()
    {
        var position = transform.position;
        var ranDist = Random.Range(position.x + minPatrolRange, maxPatrolRange);
        _patrol1 = new Vector3(ranDist, position.y, position.z);
        _patrol2 = new Vector3(-ranDist, position.y, position.z);
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
            Gizmos.DrawWireSphere(position, maxPatrolRange);
            Gizmos.DrawWireSphere(position, minPatrolRange);

            var v = new Vector3(1, 1, 1);

            Gizmos.DrawWireCube(_patrol1, v);
            Gizmos.DrawWireCube(_patrol2, v);
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