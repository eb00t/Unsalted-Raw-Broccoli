using System;
using System.Collections;
using Cinemachine;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class CopyBoss : MonoBehaviour, IDamageable
{
    private enum States { Idle, Patrol, Chase, Attack, Retreat, Frozen, Jumping, Crouching }
    [SerializeField] private States _currentState = States.Idle;

    private Rigidbody _rigidbody;
    private Animator _animator;
    private Transform _player;
    private SpriteRenderer _spriteRenderer;
    
    [Header("Enemy Stats")]
    [SerializeField] private int maxHealth;
    [SerializeField] private int attack;
    [SerializeField] private int poise;
    [SerializeField] private int poisonResistance, maxJumpCount;
    [SerializeField] private int poiseDamage;
    [SerializeField] private float attackRange;
    [SerializeField] private float chaseRange;
    [SerializeField] private float chaseDuration;
    [SerializeField] private float patrolRange;
    [SerializeField] private float attackCooldown;
    [SerializeField] private float comboChance;
    [SerializeField] private bool canFreeze, canJump;
    [SerializeField] private float freezeDuration;
    [SerializeField] private float freezeCooldown;
    [SerializeField] private float movementSpeed;
    [SerializeField] private float jumpForce, jumpTriggerDistance;
    [SerializeField] private Transform atkHitbox;
    [SerializeField] private float fallThroughTime = 2f;
    
    private float _playerBelowTimer;
    private bool _isFallingThrough, _isStunned;
    private int _currentHealth, _poisonBuildup, _poiseBuildup, _jumpCount;
    private bool _isAttacking, _isFrozen, _isDead, _isPoisoned, _hasPlayerBeenSeen, _isJumping, _isKnockedBack;
    private Vector3 _patrolPoint1, _patrolPoint2, _currentPatrolTarget;
    private CharacterMovement _characterMovement;
    private RoomScripting _roomScripting;
    private LockOnController _lockOnController;
    private CinemachineImpulseSource _impulseSource;
    private Vector3 _impulseVector;
    private int _knockbackDir;

    [Header("UI")]
    [SerializeField] private Slider healthSlider;
    [SerializeField] private Image healthFillImage;
    private Collider _bossCollider;

    int IDamageable.Attack { get => attack; set => attack = value; }
    int IDamageable.Poise { get => poise; set => poise = value; }
    int IDamageable.PoiseDamage { get => poiseDamage; set => poiseDamage = value; }
    public bool isPlayerInRange { get; set; }
    public bool isDead { get; set; }
    public RoomScripting RoomScripting { get; set; }
    public Spawner Spawner { get; set; }

    private void Start()
    {
        _roomScripting = gameObject.transform.root.GetComponent<RoomScripting>();
        _roomScripting.enemies.Add(gameObject);
        _impulseSource = GetComponent<CinemachineImpulseSource>();
        _bossCollider = GetComponent<Collider>();
        _rigidbody = GetComponent<Rigidbody>();
        _animator = GetComponent<Animator>();
        _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        _player = GameObject.FindGameObjectWithTag("Player").transform;
        _characterMovement = _player.GetComponent<CharacterMovement>();
        _lockOnController = _player.GetComponent<LockOnController>();

        _currentHealth = maxHealth;
        healthSlider.maxValue = maxHealth;
        healthSlider.value = maxHealth;
        healthSlider.gameObject.SetActive(false);

        //PickPatrolPoints();
        _currentPatrolTarget = _patrolPoint1;
    }

    private void Update()
    {
        if (_isDead) return;

        var distance = Vector3.Distance(transform.position, _player.position);
        var heightDiffAbove = _player.position.y - transform.position.y;
        var heightDiffBelow = transform.position.y - _player.position.y;
        var playerDir =  Mathf.Abs(_player.position.x - transform.position.x);
        
        if (IsGrounded())
        {
            _jumpCount = 0;
        }

        if (_isStunned) return;
        
        if (_isFrozen)
        {
            _currentState = States.Frozen;
        }
        else if (distance < attackRange && playerDir > 0)
        {
            _currentState = States.Attack;
        }
        else if (distance < chaseRange || _hasPlayerBeenSeen)
        {
            if (canJump && heightDiffAbove > jumpTriggerDistance)
            {
                _currentState = States.Jumping;
            }
            else if (heightDiffBelow > jumpTriggerDistance)
            {
                _currentState = States.Crouching;
            }
            else
            {
                _currentState = States.Chase;
            }
        }
        else
        {
            _currentState = States.Idle;
        }

        switch (_currentState)
        {
            case States.Idle:
                healthSlider.gameObject.SetActive(false);
                break;
            case States.Patrol:
                //Patrol();
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
                StartCoroutine(BeginFreeze());
                break;
            case States.Jumping:
                Jump();
                break;
            case States.Crouching:
                Crouching();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        if (!_isKnockedBack)
        {
            var velocity = _rigidbody.velocity;

            _animator.SetFloat("XVelocity", Mathf.Abs(velocity.x));

            var localScale = _spriteRenderer.transform.localScale;
            var localScale2 = atkHitbox.localScale;

            if (velocity.x > 0.1f)
            {
                localScale = new Vector3(Mathf.Abs(localScale.x), localScale.y, localScale.z);
                localScale2 = new Vector3(Mathf.Abs(localScale2.x), localScale2.y, localScale2.z);
            }
            else if (velocity.x < -0.1f)
            {
                localScale = new Vector3(-Mathf.Abs(localScale.x), localScale.y, localScale.z);
                localScale2 = new Vector3(-Mathf.Abs(localScale2.x), localScale2.y, localScale2.z);
            }

            _spriteRenderer.transform.localScale = localScale;
            atkHitbox.localScale = localScale2;
        }
    }
    
    private bool IsGrounded()
    {
        return Physics.Raycast(transform.position, Vector3.down, 3f, LayerMask.GetMask("Ground"));
    }

    private void Crouching()
    {
        _playerBelowTimer += Time.deltaTime;

        if (_playerBelowTimer >= fallThroughTime && !_isFallingThrough)
        {
            FallThroughPlatform();
            _playerBelowTimer = 0f;
        }
    }

    private void FallThroughPlatform()
    {
        _isFallingThrough = true;
        var platform = FindPlatform(Vector3.down); 

        if (platform != null)
        {
            StartCoroutine(DisableCollision());
        }
    }
    
    private Collider FindPlatform(Vector3 dir)
    {
        var layerMask = LayerMask.GetMask("Ground");
        return Physics.Raycast(transform.position, dir, out var hit, 20f, layerMask) ? hit.collider : null;
    }

    private void Jump()
    {
        if (_jumpCount >= maxJumpCount) return;

        _jumpCount++;

        var col = FindPlatform(Vector3.up);
    
        if (col != null)
        {
            StartCoroutine(DisableCollision());
        }

        var newForce = Mathf.Sqrt(2 * Mathf.Abs(Physics.gravity.y) * (_player.position.y - transform.position.y + 1.5f));
        newForce = Mathf.Max(newForce, jumpForce);
        
        var dirX = Mathf.Sign(transform.localScale.x);
        var speed = 2f;
        
        _rigidbody.velocity = new Vector3(speed * dirX, newForce, _rigidbody.velocity.z);

        _animator.SetTrigger("Jump");
    }

    private IEnumerator DisableCollision()
    {
        if (_bossCollider != null)
            _bossCollider.enabled = false;

        yield return new WaitForSeconds(.8f);

        if (_bossCollider != null)
            _bossCollider.enabled = true;

        _isFallingThrough = false;
        _currentState = States.Chase;
    }
    
    private void MoveTowards(Vector3 target)
    {
        if (_isKnockedBack) return;
        
        var direction = (target - transform.position).normalized;
        _rigidbody.velocity = new Vector3(direction.x * movementSpeed, _rigidbody.velocity.y, direction.z);
    }

    /*
    private void Patrol()
    {
        MoveTowards(_currentPatrolTarget);

        if (Vector3.Distance(transform.position, _currentPatrolTarget) < 0.5f)
        {
            _currentPatrolTarget = (_currentPatrolTarget == _patrolPoint1) ? _patrolPoint2 : _patrolPoint1;
        }
    }

    private void PickPatrolPoints()
    {
        var randomOffset = Random.Range(4f, patrolRange);
        _patrolPoint1 = transform.position + new Vector3(randomOffset, 0, 0);
        _patrolPoint2 = transform.position + new Vector3(-randomOffset, 0, 0);
    }
    */

    private void Chase()
    {
        healthSlider.gameObject.SetActive(true);

        if (!_hasPlayerBeenSeen)
        {
            StartCoroutine(StartChaseDelay());
        }

        MoveTowards(_player.position);
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
        _rigidbody.velocity = Vector3.zero;

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
        
        MoveTowards(transform.position + retreatDirection * 3f);

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
        _rigidbody.velocity = Vector3.zero;

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

        if (_currentHealth <= maxHealth / 2)
        {
            AudioManager.Instance.SetMusicParameter("Boss Phase", 2);
        }

        if (_currentHealth <= 0)
        {
            Die();
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

    public void ApplyKnockback(Vector2 knockbackPower)
    {
        if (_isFrozen || isDead) return;

        _knockbackDir = transform.position.x > _player.position.x ? 1 : -1;
        
        var knockbackMultiplier = (_poiseBuildup >= poise) ? 10f : 5f; 
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
        _animator.SetBool("isStaggered", true);
        _isStunned = true;
        yield return new WaitForSecondsRealtime(stunTime);
        _isStunned = false;
        _animator.SetBool("isStaggered", false);
    }

    private void Die()
    {
        isDead = true; 
        LevelBuilder.Instance.bossDead = true;
        _impulseVector = new Vector3(Random.Range(-1, 1), 5, 0);
        _impulseSource.m_ImpulseDefinition.m_ImpulseShape = CinemachineImpulseDefinition.ImpulseShapes.Explosion;
        _impulseSource.GenerateImpulseWithVelocity(_impulseVector);
        AudioManager.Instance.PlayOneShot(FMODEvents.Instance.Explosion, transform.position);
        AudioManager.Instance.SetMusicParameter("Boss Phase", 4);
        _characterMovement.lockedOn = false;
        _lockOnController.lockedTarget = null;
        _roomScripting.enemies.Remove(gameObject);
        gameObject.SetActive(false);
    }

    private void OnDrawGizmos()
    {
        var dist = 3f;
        Gizmos.DrawRay(transform.position, Vector3.down * dist);
    }
}