using System;
using System.Collections;
using System.Collections.Generic;
using FMOD.Studio;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using Random = UnityEngine.Random;


public class FlyingEnemyHandler : MonoBehaviour, IDamageable
{
    [Header("Enemy Stats")] 
    [SerializeField] private int maxHealth;
    [SerializeField] private int  poisonResistance, poise, poiseDamage;
    [SerializeField] private float atkDelay, attackRange;
    [SerializeField] private float chaseRange, chaseDuration;
    [SerializeField] private float minPatrolRange, maxPatrolRange;
    [SerializeField] private float freezeDuration, freezeCooldown;
    [SerializeField] private bool canFreeze;
    public int attack;
    private int _poisonBuildup, _health;
    private bool _isFrozen, _isPoisoned, _hasPlayerBeenSeen, _isKnockedBack, _isStunned;

    [Header("Values")]
    private float _targetTime;
    private int _poiseBuildup;
    private Vector3 _patrolTarget, _patrol1, _patrol2;
    [SerializeField] private States _state = States.Idle;

    [Header("References")]
    [SerializeField] private BoxCollider atkHitbox;
    [SerializeField] private Image healthFillImage;
    private BoxCollider _collider;
    private Slider _healthSlider;
    private Animator _animator;
    private Transform _target, _spriteTransform;
    private CharacterMovement _characterMovement;
    private LockOnController _lockOnController;
    private EventInstance _alarmEvent;
    private EventInstance _deathEvent;
    private Rigidbody _rigidbody;
    private int _knockbackDir;

    [SerializeField] private bool isIdle, debugPatrol, debugRange;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    private Vector3 _currentVelocity;
    private bool _lowHealth;
    
    int IDamageable.Attack { get => attack; set => attack = value; }
    int IDamageable.Poise { get => poise; set => poise = value; }
    int IDamageable.PoiseDamage { get => poiseDamage; set => poiseDamage = value; }
    public bool isPlayerInRange { get; set; }
    public bool isDead { get; set; }
    public RoomScripting RoomScripting { get; set; }
    public Spawner Spawner { get; set; }

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
        _deathEvent = AudioManager.Instance.CreateEventInstance(FMODEvents.Instance.EnemyDeath);
        _healthSlider = GetComponentInChildren<Slider>();
        _healthSlider.maxValue = maxHealth;
        _healthSlider.value = maxHealth;
        _health = maxHealth;
        _healthSlider.gameObject.SetActive(false);
        _animator = GetComponent<Animator>();
        _spriteTransform = GetComponentInChildren<SpriteRenderer>().transform;

        _target = GameObject.FindGameObjectWithTag("Player").transform;
        _lockOnController = _target.GetComponent<LockOnController>();
        _characterMovement = _target.GetComponent<CharacterMovement>();
        _rigidbody = GetComponent<Rigidbody>();
        _collider = GetComponent<BoxCollider>();

        PickPatrolPoints();
        _patrolTarget = _patrol1;
    }

    private void Update()
    {
        if (_animator.GetBool("isDead")) return;
        
        var distance = Vector3.Distance(transform.position, _target.position);
        var playerDir =  Mathf.Abs(_target.position.x - transform.position.x);

        if (_isStunned) return;

        if (_isFrozen)
        {
            _state = States.Frozen;
        }
        else
        {
            if (distance <= attackRange)
            {
                _state = States.Attack;
            }
            else if (distance <= chaseRange || _hasPlayerBeenSeen)
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
                Frozen();
                break;
        }
    }

    private void Patrol()
    {
        if (Vector3.Distance(transform.position, _patrolTarget) < 1f)
        {
            _patrolTarget = _patrolTarget == _patrol1 ? _patrol2 : _patrol1;
        }
        MoveTowards(_patrolTarget);
    }

    private void Chase()
    {
        _healthSlider.gameObject.SetActive(true);
    
        if (!_hasPlayerBeenSeen)
        {
            StartCoroutine(StartChaseDelay());
        }

        var targetPosition = _target.position;

        MoveTowards(targetPosition);
    }

    private IEnumerator StartChaseDelay()
    {
        _hasPlayerBeenSeen = true;
        yield return new WaitForSecondsRealtime(chaseDuration);
        _hasPlayerBeenSeen = false;
    }

    private void Attack()
    {
        _targetTime -= Time.deltaTime;
        if (_targetTime <= 0.0f)
        {
            _animator.SetTrigger("Attack");
            _targetTime = atkDelay;
        }
    }

    private void PickPatrolPoints()
    {
        var position = transform.position;
        
        var newPatrol1 = new Vector3(position.x + Random.Range(minPatrolRange, maxPatrolRange), position.y, position.z);
        var newPatrol2 = new Vector3(position.x - Random.Range(minPatrolRange, maxPatrolRange), position.y, position.z);
        
        _patrol1 = newPatrol1;
        _patrol2 = newPatrol2;
    }

    private void MoveTowards(Vector3 target)
    {
        var direction = (target - transform.position).normalized;

        if (Physics.Raycast(transform.position, direction, out var hit, 1f))
        {
            if (!hit.collider.CompareTag("Player") &&
                !hit.collider.CompareTag("Top Wall") &&
                !hit.collider.CompareTag("Bottom Wall") &&
                !hit.collider.CompareTag("Left Wall") &&
                !hit.collider.CompareTag("Right Wall") &&
                !hit.collider.CompareTag("Right Door") &&
                !hit.collider.CompareTag("Left Door") &&
                !hit.collider.CompareTag("Bottom Door") &&
                !hit.collider.CompareTag("Top Door") &&
                !hit.collider.isTrigger)
            {
                StartCoroutine(DisableCollision());
            }
        }

        if (!_isKnockedBack)
        {
            _rigidbody.velocity = direction * moveSpeed;

            var localScale = _spriteTransform.localScale;
            var hitboxLocalScale = atkHitbox.transform.localScale;
            if (Mathf.Abs(direction.x) > 0.1f)
            {
                localScale.x = direction.x > 0 ? Mathf.Abs(localScale.x) : -Mathf.Abs(localScale.x);
                hitboxLocalScale.x = direction.x > 0 ? Mathf.Abs(hitboxLocalScale.x) : -Mathf.Abs(hitboxLocalScale.x);
            }

            _spriteTransform.localScale = localScale;
            atkHitbox.transform.localScale = hitboxLocalScale;
        }
    }
    
    private IEnumerator DisableCollision()
    {
        if (_collider != null)
            _collider.enabled = false;

        yield return new WaitForSeconds(1.5f);

        if (_collider != null)
            _collider.enabled = true;
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
    
    private void Frozen()
    {
        if (!canFreeze) return;
        StartCoroutine(BeginFreeze());
    }
    
    private IEnumerator BeginFreeze()
    {
        StartCoroutine(StartCooldown());
        healthFillImage.color = Color.cyan;
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
        _animator.SetBool("isDead", true);
        isDead = true;
        _characterMovement.lockedOn = false;
        _lockOnController.lockedTarget = null;
        RoomScripting.enemies.Remove(gameObject);
        RoomScripting._enemyCount--;
        Spawner.spawnedEnemies.Remove(gameObject);
        
        StopAllCoroutines();
        StartCoroutine(FallToGround());

        foreach (var hb in GetComponentsInChildren<BoxCollider>()) // stops player being able to hit enemy on death
        {
            hb.gameObject.SetActive(false);
        }
        
        Spawner.spawnedEnemy = null;
        Spawner.SpawnEnemies();
        AudioManager.Instance.AttachInstanceToGameObject(_deathEvent, gameObject.transform);
        _deathEvent.start();
        _deathEvent.release();
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
    
    private IEnumerator FallToGround()
    {
        _rigidbody.isKinematic = false;
        _rigidbody.useGravity = true;

        yield return new WaitForSeconds(2f);

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
    
    public void ApplyKnockback(Vector2 knockbackPower)
    {
        if (_isFrozen || isDead) return;

        _knockbackDir = transform.position.x > _target.position.x ? 1 : -1;
        
        var knockbackMultiplier = (_poiseBuildup >= poise) ? 2f : 1f; 
        var knockbackForce = new Vector3(knockbackPower.x * _knockbackDir * knockbackMultiplier, knockbackPower.y * knockbackMultiplier, 0);

        StartCoroutine(TriggerKnockback(knockbackForce, 0.5f));

        if (_poiseBuildup >= poise)
        {
            StartCoroutine(StunTimer(1.5f));
            _poiseBuildup = 0;
        }
    }
    
    private IEnumerator TriggerKnockback(Vector3 force, float duration)
    {
        _isKnockedBack = true;

        _rigidbody.velocity = Vector3.zero;
        yield return null;

        _rigidbody.AddForce(force, ForceMode.Impulse);

        yield return new WaitForSeconds(duration);

        _isKnockedBack = false;
    }
    
    private IEnumerator StunTimer(float stunTime)
    {
        //_animator.SetBool("isStaggered", true);
        _isStunned = true;
        yield return new WaitForSecondsRealtime(stunTime);
        _isStunned = false;
        //_animator.SetBool("isStaggered", false);
    }
}
