using System.Collections;
using DG.Tweening;
using FMOD.Studio;
using FMODUnity;
using Pathfinding;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;
using STOP_MODE = FMOD.Studio.STOP_MODE;


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
    [SerializeField] private Vector3 knockbackPower;
    [SerializeField] private int numberOfAttacks;
    [SerializeField] private float projectileSpeed;
    [SerializeField] private float multiProjectileOffset;
    [SerializeField] private float floor2Multiplier, floor3Multiplier, floor4Multiplier, hardcoreMultiplier;
    
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
    private Vector3 _reposPosition;
    
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
    [SerializeField] private bool doesHaveLaserAttack;
    [SerializeField] private bool doesHaveProjectileAttack;
    [SerializeField] private float numberOfProjectiles;
    private Vector3 _knockbackForce;
    private bool _isRepositioning;
    private bool _isStunned;
    private bool _isFrozen;
    private bool _isPoisoned;
    private bool _lowHealth;
    private int _poisonBuildup;
    private int _poiseBuildup;
    
    [Header("References")] 
    [SerializeField] private BoxCollider attackHitbox;
    [SerializeField] private GameObject flashEffect;
    [SerializeField] private Image healthFillImage;
    [SerializeField] private GameObject lightProjectile;
    [SerializeField] private Transform projectileOrigin;
    [SerializeField] private Material defaultMaterial, hitMaterial;
    [SerializeField] private GameObject gibs;
    [SerializeField] private DataHolder dataHolder;
    [SerializeField] private Slider healthChangeSlider;
    [SerializeField] private Image healthChangeImage;
    private Tween _healthTween;
    private Collider _roomBounds;
    private Slider _healthSlider;
    private Animator _animator;
    private Transform _target;
    private CharacterAttack _characterAttack;
    private SpriteRenderer _spriteRenderer;
    private Transform _spriteTransform;
    private SphereCollider _collider;
    private Rigidbody _rigidbody;
    private AIPath _aiPath;
    
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
    Vector3 IDamageable.KnockbackPower { get => knockbackPower; set => knockbackPower = value; }
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
        ScaleStats();
        gameObject.transform.parent = gameObject.transform.root;
        _healthSlider = GetComponentInChildren<Slider>();
        _healthSlider.maxValue = maxHealth;
        _healthSlider.value = maxHealth;
        _health = maxHealth;
        healthChangeSlider.maxValue = maxHealth;
        healthChangeSlider.value = _health;
        _healthSlider.gameObject.SetActive(false);
        _animator = GetComponent<Animator>();
        _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        _spriteTransform = _spriteRenderer.transform;
        _aiPath = GetComponent<AIPath>();
        _aiPath.maxSpeed = moveSpeed;
        _target = GameObject.FindGameObjectWithTag("Player").transform;
        _rigidbody = GetComponent<Rigidbody>();
        _collider = GetComponent<SphereCollider>();
        _lineRenderer = GetComponentInChildren<LineRenderer>();
        _characterAttack = _target.GetComponentInChildren<CharacterAttack>();
        _alarmEvent = AudioManager.Instance.CreateEventInstance(FMODEvents.Instance.EnemyLowHealthAlarm);
        AudioManager.Instance.AttachInstanceToGameObject(_alarmEvent, gameObject);
        DisablePlatformCollisions();
        _laserEvent = AudioManager.Instance.CreateEventInstance(FMODEvents.Instance.FlyingEnemyLaser);
        AudioManager.Instance.AttachInstanceToGameObject(_laserEvent, gameObject);
        _targetTime = attackCooldown / 2;
        _deathEvent = AudioManager.Instance.CreateEventInstance(FMODEvents.Instance.EnemyDeath);
        _deathEvent.set3DAttributes(new Vector3(transform.position.x, transform.position.y, transform.position.z).To3DAttributes());
        AudioManager.Instance.AttachInstanceToGameObject(_deathEvent, gameObject);
    }

    private void Update()
    {
        if (_animator.GetBool("isDead")) return;
        
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
    
    private void ScaleStats()
    {
        var multiplier = dataHolder.hardcoreMode ? hardcoreMultiplier : 1f;

        switch (LevelBuilder.Instance.currentFloor)
        {
            case LevelBuilder.LevelMode.Floor2:
                multiplier += floor2Multiplier;
                break;
            case LevelBuilder.LevelMode.Floor3:
                multiplier += floor3Multiplier;
                break;
            case LevelBuilder.LevelMode.Floor4:
                multiplier += floor4Multiplier;
                break;
        }

        maxHealth = (int)(maxHealth * multiplier);
        poise = (int)(poise * multiplier);
        poisonResistance = (int)(poisonResistance * multiplier);
        attack = (int)(attack * multiplier);
        knockbackPower = new Vector3(knockbackPower.x * multiplier, knockbackPower.y * multiplier, 0);
    }
    
    private bool IsPlayerInRoom()
    {
        return _roomBounds != null && _roomBounds.bounds.Contains(_target.position);
    }

    private void Chase()
    {
        _healthSlider.gameObject.SetActive(true);

        if (_aiPath != null)
        {
            _aiPath.canMove = true;
            _aiPath.destination = _target.position;
        }
    }

    private void Attack()
    {
        _healthSlider.gameObject.SetActive(true);
        _rigidbody.velocity = Vector3.zero;
        UpdateSpriteDirection(_playerDir.x < 0f);
        _targetTime -= Time.deltaTime;

        if (!_isRepositioning)
        {
            StartCoroutine(NewPosition());
        }
    }
    
    private IEnumerator NewPosition() // picks a new position within the room and moves to it
    {
        _isRepositioning = true;

        if (_roomBounds == null)
        {
            _isRepositioning = false;
            yield break;
        }

        var bounds = _roomBounds.bounds;
        _reposPosition = new Vector3(Random.Range(bounds.min.x + 1f, bounds.max.x - 1f), Random.Range(bounds.min.y + 1f, bounds.max.y - 1f), transform.position.z);

        _aiPath.canMove = true;
        _aiPath.destination = _reposPosition;

        while (Vector3.Distance(transform.position, _reposPosition) > 0.5f)
        {
            _targetTime -= Time.deltaTime;

            if (_targetTime <= 0f && _canAttack)
            {
                _aiPath.canMove = false;
                yield return StartCoroutine(TriggerAttack());
                _aiPath.canMove = true;
                _targetTime = attackCooldown;
            }

            yield return null;
        }

        _aiPath.canMove = false;
        _isRepositioning = false;
    }
    
    private IEnumerator TriggerAttack()
    {
        _canAttack = false;
        var ran = 0;

        if (doesHaveLaserAttack && doesHaveProjectileAttack)
        {
            ran = Random.Range(0, 2);
        }
        else if (doesHaveProjectileAttack && !doesHaveLaserAttack)
        {
            ran = 1;
        }

        if (ran == 0)
        {
            yield return StartCoroutine(LaserAttack());
        }
        else
        {
            _animator.SetTrigger("Projectile");
            yield return new WaitForSeconds(0.25f);
        }

        _canAttack = true;
    }

    public void ProjectileAttack()
    {
        _canAttack = false;
        var centre = numberOfProjectiles / 2;

        for (var i = 0; i < numberOfProjectiles; i++)
        {
            var position = projectileOrigin.position;
            var newProjectile = Instantiate(lightProjectile, position, Quaternion.identity);
            newProjectile.GetComponent<HitboxHandler>().damageable = this;
            newProjectile.SetActive(true);
            
            var rb = newProjectile.GetComponent<Rigidbody>();
            var dir = _playerDir.normalized;
            var centreOffset = i - centre;
            var rotation = Quaternion.AngleAxis(centreOffset * multiProjectileOffset, Vector3.forward);
            var angledDir = rotation * dir;

            rb.velocity = new Vector3(angledDir.x, angledDir.y, 0) * projectileSpeed;
            
            var newRot = Mathf.Atan2(angledDir.y, angledDir.x) * Mathf.Rad2Deg;
            rb.rotation = Quaternion.Euler(0, 0, newRot);
        }
        
        AudioManager.Instance.PlayOneShot(FMODEvents.Instance.FlyingEnemyShoot, transform.position);
        
        _canAttack = true;
    }

    private IEnumerator LaserAttack() // aims laser at player that tracks, then it stops and starts doing damage
    {
        
        _canAttack = false;
        _lineRenderer.enabled = true;
        _lineRenderer.SetPosition(0, bossEyePosition.position);
        
        var targetPos = _target.position;
        var elapsed = 0f;
        
        AudioManager.Instance.SetEventParameter(_laserEvent, "Firing", 0);
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
        var hasAppliedKnockback = false;
        
        AudioManager.Instance.SetEventParameter(_laserEvent, "Firing", 1);
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
                            if (!hasAppliedKnockback)
                            {
                                player.TakeDamagePlayer(attack, poiseDamage, knockbackPower);
                                hasAppliedKnockback = true;
                            }
                            else
                            {
                                player.TakeDamagePlayer(attack, poiseDamage, Vector3.zero);
                            }
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
        var previousHealth = _health;
        defense = Mathf.Clamp(defense, 0, 100);
        var dmgReduction = (100 - defense) / 100f;
        damage = Mathf.RoundToInt(damage * dmgReduction);
        
        StartCoroutine(HitFlash());
        
        if (Random.value <= 0.15f) // 15 percent chance on hit for enemy to drop energy
        {
            Instantiate(Resources.Load<GameObject>("ItemPrefabs/Other/Energy Prefab"), transform.position, Quaternion.identity);
        }

        if (_health - damage > 0)
        {
            _health -= damage;
            //_healthSlider.value = _health;
        }
        else
        {
            _health = 0;
            //_healthSlider.value = 0;
            if (knockback.HasValue)
            {
                ApplyKnockback(knockback.Value);
            }
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
        
        var isDamaged = _health < previousHealth;
        var changeColor = isDamaged ? new Color(1f, 0.9f, 0.4f) : new Color(.5f, 1f, 0.4f);
        
        _healthTween?.Kill();
        healthChangeImage.color = changeColor;

        if (isDamaged)
        {
            _healthSlider.value = _health;
            _healthTween = DOVirtual.Float(healthChangeSlider.value, _health, 1f, v => healthChangeSlider.value = v).SetEase(Ease.OutExpo).SetDelay(0.2f);
        }
        else
        {
            healthChangeSlider.value = _health;
            _healthTween = DOVirtual.Float(_healthSlider.value, _health, 1f, v => _healthSlider.value = v).SetEase(Ease.OutExpo).SetDelay(0.2f);
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
        
        var newGibs = Instantiate(gibs, transform.position, Quaternion.identity);

        foreach (var gib in newGibs.GetComponentsInChildren<Rigidbody>())
        {
            var dir = new Vector3(_knockbackForce.x, _knockbackForce.y, 0);
            gib.AddForce(dir * (_knockbackForce.magnitude * Random.Range(2f, 4f)), ForceMode.Impulse);
        }
        
        _characterAttack.ChanceHeal();
        
        StopAllCoroutines();

        foreach (var hb in GetComponentsInChildren<Collider>()) // stops player being able to hit enemy on death
        {
            hb.gameObject.SetActive(false);
        }
        
        EnemySpawner.spawnedEnemy = null;
        EnemySpawner.SpawnEnemies();
        int currencyToDrop = Random.Range(0, 5);
        for (int i = 0; i < currencyToDrop; i++)
        {
            Instantiate(Resources.Load<GameObject>("ItemPrefabs/Other/Currency Prefab"), transform.position, Quaternion.identity);
        }
        
        var energyToDrop = Random.Range(0, 3);
        for (var i = 0; i < energyToDrop; i++)
        {
            Instantiate(Resources.Load<GameObject>("ItemPrefabs/Other/Energy Prefab"), transform.position, Quaternion.identity);
        }
        
        _deathEvent.start();
        _deathEvent.release();
        StopAlarmSound();
    }
    
    public void PlayAlarmSound()
    {
        _alarmEvent.start();
    }

    public void StopAlarmSound()
    {
        _alarmEvent.stop(STOP_MODE.IMMEDIATE);
        _laserEvent.stop(STOP_MODE.IMMEDIATE);
        _alarmEvent.release();
        _laserEvent.release();
    }
    
    public void ApplyKnockback(Vector2 knockbackPower)
    {
        if (_isFrozen || isDead) return;

        _knockbackDir = transform.position.x > _target.position.x ? 1 : -1;
        
        var knockbackMultiplier = (_poiseBuildup >= poise) ? 2f : 1f; 
        _knockbackForce = new Vector3(knockbackPower.x * _knockbackDir * knockbackMultiplier, knockbackPower.y * knockbackMultiplier, 0);

        StartCoroutine(TriggerKnockback(_knockbackForce, 0.2f));
        StartCoroutine(StunTimer(0.1f));

        if (_poiseBuildup >= poise)
        {
            StartCoroutine(StunTimer(1.5f));
            _poiseBuildup = 0;
        }
    }
    
    private IEnumerator TriggerKnockback(Vector3 force, float duration)
    {
        _aiPath.canMove = false;
        _rigidbody.velocity = Vector3.zero;
        _rigidbody.AddForce(force, ForceMode.Impulse);
        yield return new WaitForSeconds(duration);
        _rigidbody.velocity = Vector3.zero;
        _aiPath.canMove = true;
        //_aiPath.SearchPath();
    }
    
    private IEnumerator StunTimer(float stunTime)
    {
        _animator.SetBool("isStaggered", true);
        _isStunned = true;
        yield return new WaitForSecondsRealtime(stunTime);
        _isStunned = false;
        _animator.SetBool("isStaggered", false);
    }
    
    private IEnumerator HitFlash()
    {
        _spriteRenderer.material = hitMaterial;
        yield return new WaitForSeconds(0.1f);
        _spriteRenderer.material = defaultMaterial;
    }
}
