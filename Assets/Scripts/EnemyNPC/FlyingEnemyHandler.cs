using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using Random = UnityEngine.Random;


public class FlyingEnemyHandler : MonoBehaviour, IDamageable
{
    [Header("Enemy Stats")]
    [SerializeField] private int maxHealth, poisonResistance, poise;
    [SerializeField] private float atkDelay, attackRange;
    [SerializeField] private float chaseRange, chaseDuration;
    [SerializeField] private float minPatrolRange, maxPatrolRange;
    [SerializeField] private float freezeDuration, freezeCooldown;
    [SerializeField] private bool canFreeze;
    public int attack;
    private int _poisonBuildup, _health;
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
    private Transform _target, _spriteTransform;

    [SerializeField] private bool isIdle, debugPatrol, debugRange;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    private Vector3 _currentVelocity;
    
    int IDamageable.Attack { get => attack; set => attack = value; }
    int IDamageable.Poise { get => poise; set => poise = value; }

    public bool isPlayerInRange { get; set; }
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
        _healthSlider = GetComponentInChildren<Slider>();
        _healthSlider.maxValue = maxHealth;
        _healthSlider.value = maxHealth;
        _health = maxHealth;
        _healthSlider.gameObject.SetActive(false);
        _animator = GetComponent<Animator>();
        _spriteTransform = GetComponentInChildren<SpriteRenderer>().transform;

        _target = GameObject.FindGameObjectWithTag("Player").transform;

        PickPatrolPoints();
        _patrolTarget = _patrol1;
    }

    private void Update()
    {
        if (_animator.GetBool("isDead")) return;
        
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
        
        if (_hasPlayerBeenSeen == false)
        {
            StartCoroutine(StartChaseDelay());
        }

        MoveTowards(_target.position);
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
        var hitNormal = Vector3.zero;

        if (Physics.SphereCast(transform.position, 0.5f, direction, out var hit, 1f))
        {
            if (!hit.collider.isTrigger)
            {
                hitNormal = hit.normal;
                direction = Vector3.Reflect(direction, hitNormal).normalized; 
                direction += hitNormal * 1.5f;
                
                if (Vector3.Dot(direction, (target - transform.position).normalized) < 0.2f)
                {
                    _patrolTarget = _patrolTarget == _patrol1 ? _patrol2 : _patrol1;
                    return;
                }
            }
        }

        if (Physics.Raycast(transform.position, direction, 1f))
        {
            _patrolTarget = _patrolTarget == _patrol1 ? _patrol2 : _patrol1;
            return;
        }

        transform.position += direction * (moveSpeed * Time.deltaTime);

        var localScale = _spriteTransform.localScale;
        if (Mathf.Abs(direction.x) > 0.1f)
        {
            localScale.x = direction.x > 0 ? Mathf.Abs(localScale.x) : -Mathf.Abs(localScale.x);
        }
        _spriteTransform.localScale = localScale;
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
        StopAllCoroutines();
        StartCoroutine(FallToGround());

        foreach (var hb in GetComponentsInChildren<BoxCollider>()) // stops player being able to hit enemy on death
        {
            hb.gameObject.SetActive(false);
        }
        Spawner.spawnedEnemy = null;
        Spawner.SpawnEnemies();
    }
    
    private IEnumerator FallToGround()
    {
        var fallSpeed = 1f; 
        var groundY = GetGroundY();

        while (transform.position.y > groundY)
        {
            transform.position += Vector3.down * (fallSpeed * Time.deltaTime);
        }

        transform.position = new Vector3(transform.position.x, groundY, transform.position.z);
    
        yield return new WaitForSeconds(2f);

        gameObject.SetActive(false);
    }
    
    private float GetGroundY()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out var hit, Mathf.Infinity))
        {
            return hit.point.y; // return y position of the raycast hit (ground)
        }
        
        return transform.position.y; // if no ground is found 
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
        /*
        _agent.velocity = Vector3.zero;
        if (_isFrozen) return;
        
        if (transform.position.x > _target.position.x)
        {
            _knockbackDir = 1;
        }
        else _knockbackDir = -1;
        _agent.velocity += new Vector3(KnockbackPower.x * _knockbackDir, KnockbackPower.y, 0f);
        */
    }
}
