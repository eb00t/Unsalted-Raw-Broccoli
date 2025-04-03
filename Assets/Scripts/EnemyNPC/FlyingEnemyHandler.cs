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
    [Header("Defensive Stats")]
    [SerializeField] private int maxHealth;
    private int _health;
    [SerializeField] private int poise;
    [SerializeField] private int defense;
    [SerializeField] private int poisonResistance;

    [Header("Offensive Stats")] 
    [SerializeField] private int attack;
    [SerializeField] private int poiseDamage;
    [SerializeField] private int numberOfAttacks;
    [SerializeField] private float projectileSpeed;
    
    [Header("Laser Stats")]
    [SerializeField] private float chargeTime;
    [SerializeField] private float fireTime;
    [SerializeField] private float trackSpeed;
    [SerializeField] private float delay;
    [SerializeField] private float laserTickCooldown;

    [Header("Tracking")] 
    [SerializeField] private float attackRange;
    [SerializeField] private float chaseRange;
    private Vector3 _currentVelocity;
    private int _knockbackDir;
    private bool _hasPlayerBeenSeen;
    private Vector3 _lastPosition;
    private Vector3 _playerDir;
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
    [SerializeField] private float moveSpeed;
    private bool _isKnockedBack;
    private bool _isStunned;
    private bool _isFrozen;
    private bool _isPoisoned;
    private bool _lowHealth;
    private bool _isStuck;
    private int _poisonBuildup;
    private int _poiseBuildup;
    
    [Header("References")] 
    [SerializeField] private BoxCollider attackHitbox;
    [SerializeField] private GameObject flashEffect;
    [SerializeField] private Image healthFillImage;
    [SerializeField] private GameObject lightProjectile;
    [SerializeField] private GameObject projectileOrigin;
    private Collider _roomBounds;
    private Slider _healthSlider;
    private Animator _animator;
    private Transform _target;
    private CharacterAttack _characterAttack;
    private SpriteRenderer _spriteRenderer;
    private Transform _spriteTransform;
    private BoxCollider _collider;
    private Rigidbody _rigidbody;
    private MaterialPropertyBlock _propertyBlock;
    private static readonly int BaseColor = Shader.PropertyToID("_BaseColor");
    
    [Header("Sound")]
    private EventInstance _alarmEvent;
    private EventInstance _deathEvent;
    private EventInstance _laserEvent;
    private bool _canAttack = true;
    private LineRenderer _lineRenderer;
    [SerializeField] private Transform bossEyePosition;

    int IDamageable.Attack { get => attack; set => attack = value; }
    int IDamageable.Poise { get => poise; set => poise = value; }
    int IDamageable.PoiseDamage { get => poiseDamage; set => poiseDamage = value; }
    public bool isPlayerInRange { get; set; }
    public bool isDead { get; set; }
    public RoomScripting RoomScripting { get; set; }
    public EnemySpawner EnemySpawner { get; set; }

    private enum States
    {
        Idle,
        Chase,
        Attack,
        Frozen
    }

    private void Start()
    {
        RoomScripting = gameObject.transform.root.GetComponent<RoomScripting>();
        RoomScripting.enemies.Add(gameObject);
        _roomBounds = RoomScripting.GetComponent<Collider>();
        gameObject.transform.parent = gameObject.transform.root;
        _deathEvent = AudioManager.Instance.CreateEventInstance(FMODEvents.Instance.EnemyDeath);
        _healthSlider = GetComponentInChildren<Slider>();
        _healthSlider.maxValue = maxHealth;
        _healthSlider.value = maxHealth;
        _health = maxHealth;
        _healthSlider.gameObject.SetActive(false);
        _animator = GetComponent<Animator>();
        _spriteTransform = GetComponentInChildren<SpriteRenderer>().transform;
        _propertyBlock = new MaterialPropertyBlock();

        _target = GameObject.FindGameObjectWithTag("Player").transform;
        _rigidbody = GetComponent<Rigidbody>();
        _collider = GetComponent<BoxCollider>();
        _lineRenderer = GetComponentInChildren<LineRenderer>();
        
        DisablePlatformCollisions();
    }

    private void Update()
    {
        if (_animator.GetBool("isDead")) return;
        
        CheckIfStuck();
        
        var distance = Vector3.Distance(transform.position, _target.position);
        _playerDir = _target.position - transform.position;
        
        if (_isStunned) return;

        if (_isFrozen)
        {
            _state = States.Frozen;
        }
        else
        {
            if (distance <= attackRange && IsPlayerInRoom())
            {
                _state = States.Attack;
            }
            else if (IsPlayerInRoom())
            {
                _state = States.Chase;
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
    
    private bool IsPlayerInRoom()
    {
        return _roomBounds != null && _roomBounds.bounds.Contains(_target.position);
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

        if (_timeSinceLastMove > maxTimeToReachTarget)
        {
            _isStuck = true;
        }
    }

    private void Chase()
    {
        _healthSlider.gameObject.SetActive(true);

        var targetPosition = _target.position;

        MoveTowards(targetPosition);
    }

    private void Attack()
    {
        _healthSlider.gameObject.SetActive(true);
        _rigidbody.velocity = Vector3.zero;
        UpdateSpriteDirection(_playerDir.x < 0f);
        _targetTime -= Time.deltaTime;
        
        if (_targetTime <= 0.0f && _canAttack)
        {
            var i = Random.Range(0, 10);

            switch (i)
            {
                case < 4:
                    StartCoroutine(LaserAttack());
                    attackCooldown = 4f;
                    break;
                case >= 4 and <= 8:
                    _animator.SetTrigger("Attack");
                    attackCooldown = 3f;
                    break;
                case > 8:
                    Reposition();
                    attackCooldown = 1f;
                    break;
            }
            
            _targetTime = attackCooldown;
        }
    }

    public void ProjectileAttack()
    {
        _canAttack = false;
        var newProjectile = Instantiate(lightProjectile, transform.position, Quaternion.identity);
        newProjectile.GetComponent<HitboxHandler>().damageable = this;
        newProjectile.SetActive(true);
        var rb = newProjectile.GetComponent<Rigidbody>();
        var dir = _playerDir.normalized;
        var newVelocity = new Vector3(dir.x, dir.y, 0);
        var newRot = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
 
        rb.velocity = newVelocity * projectileSpeed;
        rb.rotation = Quaternion.Euler(0, 0, newRot);
        _canAttack = true;
    }

    private void Reposition() // makes enemy more difficult to track down
    {
        _canAttack = false;
        StartCoroutine(NewPosition());
    }
    
    private IEnumerator NewPosition() // picks a new position within the room and moves to it
    {
        if (_roomBounds == null)
        {
            _canAttack = true;
            yield break;
        }

        var bounds = _roomBounds.bounds;
        var newPos = new Vector3(Random.Range(bounds.min.x + 1f, bounds.max.x - 1f), Random.Range(bounds.min.y + 1f, bounds.max.y - 1f), transform.position.z);

        var distance = Vector3.Distance(transform.position, newPos);
        var travelTime = distance / moveSpeed;
        var elapsed = 0f;

        while (elapsed < travelTime)
        {
            MoveTowards(newPos);
            elapsed += Time.deltaTime;
            yield return null;
        }

        _rigidbody.velocity = Vector3.zero;
        _canAttack = true;
    }


    private IEnumerator LaserAttack() // aims laser at player that tracks, then it stops and starts doing damage
    {
        _laserEvent = AudioManager.Instance.CreateEventInstance(FMODEvents.Instance.BossHandLaser);
        _canAttack = false;
        _lineRenderer.enabled = true;
        _lineRenderer.SetPosition(0, bossEyePosition.position);
        
        var targetPos = _target.position;
        var elapsed = 0f;

        AudioManager.Instance.AttachInstanceToGameObject(_laserEvent, gameObject.transform);
        _laserEvent.start();
        
        while (elapsed < chargeTime)
        {
            targetPos = Vector3.Lerp(targetPos, _target.position, Time.deltaTime * trackSpeed);
            
            _lineRenderer.SetPosition(0, bossEyePosition.position); 
            _lineRenderer.SetPosition(1, targetPos);
            _lineRenderer.startWidth = 0.01f;
            _lineRenderer.endWidth = 0.01f;
            //_lineRenderer.startColor = Color.white;
            //_lineRenderer.endColor = Color.white;
        
            elapsed += Time.deltaTime;
            yield return null;
        }

        yield return new WaitForSecondsRealtime(delay);
        
        elapsed = 0f;
        var lastDamageTime = 0f;
        
        AudioManager.Instance.SetEventParameter(_laserEvent, "Firing", 1);
        _laserEvent.release();
        while (elapsed < fireTime)
        {
            var dist = Vector3.Distance(targetPos, bossEyePosition.position);
            var direction = (targetPos - bossEyePosition.position).normalized;
            var laserEndPos = bossEyePosition.position + direction * (dist + 5f);

            _lineRenderer.SetPosition(0, bossEyePosition.position);
            _lineRenderer.SetPosition(1, laserEndPos);
            _lineRenderer.startWidth = 0.1f;
            _lineRenderer.endWidth = 0.05f;
            //_lineRenderer.endColor = Color.red;
            var layerMask = LayerMask.GetMask("Player");
            
            if (Physics.Raycast(bossEyePosition.position, direction, out var hit, 50f, layerMask))
            {
                if (hit.collider.CompareTag("Player"))
                {
                    var player = hit.collider.GetComponentInChildren<CharacterAttack>();
                    if (player != null)
                    {
                        if (Time.time >= lastDamageTime + laserTickCooldown) // makes sure player only takes damage at intervals
                        {
                            player.TakeDamagePlayer(attack, poiseDamage);
                            lastDamageTime = Time.time;
                        }
                    }
                }
            }
            
            elapsed += Time.deltaTime;
            yield return null;
        }

        _lineRenderer.enabled = false;
        _canAttack = true;
    }
    
    private void UpdateSpriteDirection(bool isLeft)
    {
        var localScale = _spriteTransform.localScale;
        var hitboxLocalScale = attackHitbox.transform.localScale;
        
        if (!isLeft)
        {
            localScale = new Vector3(Mathf.Abs(localScale.x), localScale.y, localScale.z);
            hitboxLocalScale = new Vector3(Mathf.Abs(hitboxLocalScale.x), hitboxLocalScale.y, hitboxLocalScale.z);
        }
        else
        {
            localScale = new Vector3(-Mathf.Abs(localScale.x), localScale.y, localScale.z);
            hitboxLocalScale = new Vector3(-Mathf.Abs(hitboxLocalScale.x), hitboxLocalScale.y, hitboxLocalScale.z);
        }

        _spriteTransform.localScale = localScale;
        attackHitbox.transform.localScale = hitboxLocalScale;
    }

    private void MoveTowards(Vector3 target)
    {
        var direction = (target - transform.position).normalized;

        if (!_isKnockedBack)
        {
            _rigidbody.velocity = direction * moveSpeed;
            UpdateSpriteDirection(direction.x < 0f);
        }
        
        ClampToRoom();
    }
    
    private void ClampToRoom()
    {
        if (_roomBounds == null) return;

        var bounds = _roomBounds.bounds;
        var pos = transform.position;
        
        pos.x = Mathf.Clamp(pos.x, bounds.min.x + 0.5f, bounds.max.x - 0.5f);
        pos.y = Mathf.Clamp(pos.y, bounds.min.y + 0.5f, bounds.max.y - 0.5f);

        transform.position = pos;
    }

    private void DisablePlatformCollisions()
    {
        var platforms = gameObject.transform.root.GetComponentsInChildren<SemiSolidPlatform>();
        
        foreach (var semiSolidPlatform in platforms)
        {
            var collider2 = semiSolidPlatform.GetComponent<Collider>();
            Physics.IgnoreCollision(_collider, collider2, true);
        }
    }

    public void TakeDamage(int damage, int? poiseDmg, Vector3? knockback)
    {
        defense = Mathf.Clamp(defense, 0, 100);
        var dmgReduction = (100 - defense) / 100f;
        damage = Mathf.RoundToInt(damage * dmgReduction);
        
        if (_health - damage > 0)
        {
            _health -= damage;
            _healthSlider.value = _health;
            //StartCoroutine(HitFlash(Color.red, 0.1f));
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
        if (!canBeFrozen) return;
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

    private void Die()
    {
        _animator.SetBool("isDead", true);
        isDead = true;
        RoomScripting.enemies.Remove(gameObject);
        RoomScripting._enemyCount--;
        EnemySpawner.spawnedEnemies.Remove(gameObject);
        
        StopAllCoroutines();
        StartCoroutine(FallToGround());

        foreach (var hb in GetComponentsInChildren<BoxCollider>()) // stops player being able to hit enemy on death
        {
            hb.gameObject.SetActive(false);
        }
        
        EnemySpawner.spawnedEnemy = null;
        EnemySpawner.SpawnEnemies();
        AudioManager.Instance.AttachInstanceToGameObject(_deathEvent, gameObject.transform);
        int currencyToDrop = Random.Range(0, 5);
        for (int i = 0; i < currencyToDrop; i++)
        {
            Instantiate(Resources.Load<GameObject>("ItemPrefabs/Other/Currency Prefab"), transform.position, Quaternion.identity);
        }
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
    
    public void ApplyKnockback(Vector2 knockbackPower)
    {
        if (_isFrozen || isDead) return;

        _knockbackDir = transform.position.x > _target.position.x ? 1 : -1;
        
        var knockbackMultiplier = (_poiseBuildup >= poise) ? 2f : 1f; 
        var knockbackForce = new Vector3(knockbackPower.x * _knockbackDir * knockbackMultiplier, knockbackPower.y * knockbackMultiplier, 0);

        StartCoroutine(TriggerKnockback(knockbackForce, 0.2f));

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
        _rigidbody.AddForce(force, ForceMode.Impulse);

        yield return new WaitForSeconds(duration);

        _rigidbody.velocity = Vector3.zero;
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
