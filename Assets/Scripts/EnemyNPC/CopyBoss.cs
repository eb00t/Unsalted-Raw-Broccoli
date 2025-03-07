using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class CopyBoss : MonoBehaviour, IDamageable
{
    private enum States { Idle, Patrol, Chase, Attack, Retreat, Frozen, Dead }
    private States _currentState = States.Idle;

    private NavMeshAgent _agent;
    private Animator _animator;
    private Transform _player;
    private SpriteRenderer _spriteRenderer;
    
    [Header("Enemy Stats")]
    [SerializeField] private int maxHealth;
    [SerializeField] private int attack;
    [SerializeField] private int poise;
    [SerializeField] private int poisonResistance;
    [SerializeField] private float attackRange;
    [SerializeField] private float chaseRange;
    [SerializeField] private float chaseDuration;
    [SerializeField] private float patrolRange;
    [SerializeField] private float attackCooldown;
    [SerializeField] private float comboChance;
    [SerializeField] private bool canFreeze;
    [SerializeField] private float freezeDuration;
    [SerializeField] private float freezeCooldown;

    private int _currentHealth, _poisonBuildup;
    private bool _isAttacking, _isFrozen, _isDead, _isPoisoned, _hasPlayerBeenSeen;
    private Vector3 _patrolPoint1, _patrolPoint2, _currentPatrolTarget;

    [Header("UI")]
    [SerializeField] private Slider healthSlider;
    [SerializeField] private Image healthFillImage;
    
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

    private void Start()
    {
        _agent = GetComponent<NavMeshAgent>();
        _animator = GetComponent<Animator>();
        _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        _player = GameObject.FindGameObjectWithTag("Player").transform;

        _currentHealth = maxHealth;
        healthSlider.maxValue = maxHealth;
        healthSlider.value = maxHealth;
        healthSlider.gameObject.SetActive(false);

        _agent.updateRotation = false;

        PickPatrolPoints();
        _currentPatrolTarget = _patrolPoint1;
    }

    private void Update()
    {
        if (_isDead) return;

        var distance = Vector3.Distance(transform.position, _player.position);

        if (_isFrozen)
        {
            _currentState = States.Frozen;
        }
        else if (distance < attackRange)
        {
            _currentState = States.Attack;
        }
        else if (distance < chaseRange || _hasPlayerBeenSeen)
        {
            _currentState = States.Chase;
        }
        else
        {
            _currentState = States.Patrol;
        }

        switch (_currentState)
        {
            case States.Idle:
                _agent.isStopped = true;
                healthSlider.gameObject.SetActive(false);
                break;
            case States.Patrol:
                Patrol();
                break;
            case States.Chase:
                Chase();
                break;
            case States.Attack:
                if (!_isAttacking)
                {
                    StartCoroutine(Attack());
                }
                break;
            case States.Retreat:
                Retreat();
                break;
            case States.Frozen:
                _agent.isStopped = true;
                StartCoroutine(BeginFreeze());
                break;
            case States.Dead:
                Die();
                break;
        }
        
        var velocity = _agent.velocity;
        
        _animator.SetFloat("XVelocity", Mathf.Abs(velocity.x));

        var localScale = _spriteRenderer.transform.localScale;
        
        if (velocity.x > 0.1f)
        {
            localScale = new Vector3(Mathf.Abs(localScale.x), localScale.y, localScale.z);
        }
        else if (velocity.x < -0.1f)
        {
            localScale = new Vector3(-Mathf.Abs(localScale.x), localScale.y, localScale.z);
        }
        
        _spriteRenderer.transform.localScale = localScale;
    }

    private void Patrol()
    {
        if (_agent.pathPending) return;
        if (!(_agent.remainingDistance <= _agent.stoppingDistance)) return;

        _currentPatrolTarget = (_currentPatrolTarget == _patrolPoint1) ? _patrolPoint2 : _patrolPoint1;
        _agent.SetDestination(_currentPatrolTarget);
        healthSlider.gameObject.SetActive(false);
    }

    private void PickPatrolPoints()
    {
        var randomOffset = Random.Range(4f, patrolRange);
        _patrolPoint1 = transform.position + new Vector3(randomOffset, 0, 0);
        _patrolPoint2 = transform.position + new Vector3(-randomOffset, 0, 0);
    }

    private void Chase()
    {
        _agent.isStopped = false;
        healthSlider.gameObject.SetActive(true);

        if (!_hasPlayerBeenSeen)
        {
            StartCoroutine(StartChaseDelay());
        }

        _agent.SetDestination(_player.position);
    }

    private IEnumerator StartChaseDelay()
    {
        _hasPlayerBeenSeen = true;
        yield return new WaitForSeconds(chaseDuration);
        _hasPlayerBeenSeen = false;
    }

    private IEnumerator Attack()
    {
        _isAttacking = true;
        _agent.ResetPath();
        _agent.isStopped = true;

        var comboCount = Random.Range(2, 4);

        for (var i = 0; i < comboCount; i++)
        {
            var attackType = Random.Range(0, 6);

            switch (attackType)
            {
                case 0:
                    _animator.SetTrigger("LightAttack1");
                    break;
                case 1:
                    _animator.SetTrigger("LightAttack2");
                    break;
                case 2:
                    _animator.SetTrigger("LightAttack3");
                    break;
                case 3:
                    _animator.SetTrigger("HeavyAttack1");
                    break;
                case 4:
                    _animator.SetTrigger("HeavyAttack2");
                    break;
                case 5:
                    _animator.SetTrigger("HeavyAttack3");
                    break;
            }

            yield return new WaitForSeconds(attackCooldown);

            if (Random.value > comboChance) break;
        }

        _isAttacking = false;
        _currentState = States.Retreat;
    }

    private void Retreat()
    {
        var retreatDirection = (transform.position - _player.position).normalized;
        var retreatPosition = transform.position + retreatDirection * 3f;

        _agent.SetDestination(retreatPosition);

        if (Vector3.Distance(transform.position, retreatPosition) < 0.5f)
        {
            _currentState = States.Chase;
        }
    }

    private IEnumerator BeginFreeze()
    {
        if (!canFreeze) yield break;

        _isFrozen = true;
        healthFillImage.color = Color.cyan;
        _agent.velocity = Vector3.zero;

        yield return new WaitForSeconds(freezeDuration);

        healthFillImage.color = Color.red;
        _isFrozen = false;
        _currentState = States.Chase;

        StartCoroutine(StartFreezeCooldown());
    }

    private IEnumerator StartFreezeCooldown()
    {
        canFreeze = false;
        yield return new WaitForSeconds(freezeCooldown);
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
        if (_isDead) return;

        _currentHealth -= damage;
        healthSlider.value = _currentHealth;

        if (_currentHealth <= 0)
        {
            _currentState = States.Dead;
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

    public void ApplyKnockback(Vector2 knockbackPower)
    {
        throw new System.NotImplementedException();
    }

    private void Die()
    {
        _isDead = true;
        _agent.isStopped = true;
        _animator.SetTrigger("Death");
        gameObject.SetActive(false);
    }
}
