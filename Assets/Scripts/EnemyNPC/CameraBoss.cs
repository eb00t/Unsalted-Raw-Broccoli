using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using FMOD.Studio;
using Pathfinding;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Random = UnityEngine.Random;


public class CameraBoss : MonoBehaviour, IDamageable
{
    private static readonly int IsDead = Animator.StringToHash("isDead");
    private static readonly int Projectile = Animator.StringToHash("Projectile");

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
    [SerializeField] private float hardcoreMultiplier;

    [Header("Laser Shield")] 
    [SerializeField] private GameObject shieldObject;
    [SerializeField] private float shieldRotSpeed;
    [SerializeField] private float shieldDur;
    
    [Header("Projectiles")]
    [SerializeField] private float projectileSpeed;
    [SerializeField] private float waveOffset;
    [SerializeField] private float numberOfProjectiles;
    [SerializeField] private int maxProjectileWaves;
    [SerializeField] private float projectileSpawnRadius;
    private int _wavesLeft;

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
    private Color _healthDefault;
    private bool _isFreezing;
    private Vector3 _knockbackForce;
    private bool _isShieldUp;
    private bool _isRepositioning;
    private bool _isStunned;
    private bool _isFrozen;
    private bool _isPoisoned;
    private bool _lowHealth;
    private int _poisonBuildup;
    private int _poiseBuildup;
    private bool _hasDialogueTriggered;
    
    [Header("References")] 
    [SerializeField] private BoxCollider attackHitbox;
    [SerializeField] private GameObject flashEffect;
    [SerializeField] private Image healthFillImage;
    [SerializeField] private GameObject lightProjectile;
    [SerializeField] private Transform projectileOrigin;
    [SerializeField] private Material defaultMaterial, hitMaterial;
    [SerializeField] private DataHolder dataHolder;
    [SerializeField] private GameObject gibs;
    private Collider _roomBounds;
    private Slider _healthSlider;
    private Animator _animator;
    private Transform _target;
    private CharacterAttack _characterAttack;
    [SerializeField] private SpriteRenderer spriteRenderer;
    private Transform _spriteTransform;
    private BoxCollider _collider;
    private Rigidbody _rigidbody;
    private AIPath _aiPath;
    private GameObject dialogueGui;
    private DialogueTrigger[] _dialogueTriggers;
    [SerializeField] private Slider healthChangeSlider;
    [SerializeField] private Image healthChangeImage;
    private Tween _healthTween;
    
    [Header("Sound")]
    private EventInstance _alarmEvent;
    private EventInstance _deathEvent;
    private EventInstance _laserEvent;
    private EventInstance _shieldEvent;
    private bool _canAttack = true;
    //private LineRenderer _lineRenderer;
    [SerializeField] private Transform bossEyePosition;
    private SpriteRenderer _spriteRenderer;
    private HitboxHandler _hitboxHandler;
    private Collider _collider1;

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
        _collider1 = shieldObject.GetComponent<Collider>();
        _hitboxHandler = shieldObject.GetComponent<HitboxHandler>();
        _spriteRenderer = shieldObject.GetComponent<SpriteRenderer>();
        RoomScripting = gameObject.transform.root.GetComponent<RoomScripting>();
        RoomScripting.enemies.Add(gameObject);
        _roomBounds = RoomScripting.GetComponent<Collider>();
        gameObject.transform.parent = gameObject.transform.root;
        _deathEvent = AudioManager.Instance.CreateEventInstance(FMODEvents.Instance.EnemyDeath);
        AudioManager.Instance.AttachInstanceToGameObject(_deathEvent, gameObject);;
        _healthSlider = GetComponentInChildren<Slider>();
        _healthSlider.maxValue = maxHealth;
        _healthSlider.value = maxHealth;
        _health = maxHealth;
        healthChangeSlider.maxValue = maxHealth;
        healthChangeSlider.value = _health;
        _healthSlider.gameObject.SetActive(false);
        _animator = GetComponent<Animator>();
        _spriteTransform = spriteRenderer.transform;
        _aiPath = GetComponent<AIPath>();
        _aiPath.maxSpeed = moveSpeed;
        _target = GameObject.FindGameObjectWithTag("Player").transform;
        dialogueGui = GameObject.FindGameObjectWithTag("UIManager").GetComponent<MenuHandler>().dialogueGUI;
        _rigidbody = GetComponent<Rigidbody>();
        _collider = GetComponent<BoxCollider>();
        _healthDefault = healthFillImage.color;
        //_lineRenderer = GetComponentInChildren<LineRenderer>();
        _characterAttack = _target.GetComponentInChildren<CharacterAttack>();
        _alarmEvent = AudioManager.Instance.CreateEventInstance(FMODEvents.Instance.EnemyLowHealthAlarm);
        AudioManager.Instance.AttachInstanceToGameObject(_alarmEvent, gameObject);
        _shieldEvent = AudioManager.Instance.CreateEventInstance(FMODEvents.Instance.CameraBossShield);
        AudioManager.Instance.AttachInstanceToGameObject(_shieldEvent, gameObject);;
        DisablePlatformCollisions();
        _laserEvent = AudioManager.Instance.CreateEventInstance(FMODEvents.Instance.FlyingEnemyLaser);
        AudioManager.Instance.AttachInstanceToGameObject(_laserEvent, gameObject);
        _dialogueTriggers = gameObject.transform.root.GetComponentsInChildren<DialogueTrigger>();
        if (dataHolder.hardcoreMode)
        {
            maxHealth = (int)(maxHealth * hardcoreMultiplier);
            poise = (int)(poise * hardcoreMultiplier);
            poisonResistance = (int)(poisonResistance * hardcoreMultiplier);
            attack = (int)(attack * hardcoreMultiplier);
            attackCooldown /= hardcoreMultiplier;
            knockbackPower = new Vector3(knockbackPower.x * hardcoreMultiplier, knockbackPower.y * hardcoreMultiplier, 0);
        }
    }

    private void Update()
    {
        if (_animator.GetBool(IsDead)) return;

        if (!_hasDialogueTriggered)
        {
            foreach (var trigger in _dialogueTriggers)
            {
                if (trigger.triggered && dialogueGui.activeSelf)
                {
                    _hasDialogueTriggered = true;
                    break;
                }
            }
        }
        if (!_hasDialogueTriggered || (dialogueGui != null && dialogueGui.activeSelf)) return;
        
        var distance = Vector3.Distance(transform.position, _target.position);
        _playerDir = _target.position - transform.position;
        
        if (_isStunned) return;

        if (_isFrozen && !_isFreezing)
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
                if (!_isFreezing && canBeFrozen)
                {
                    //StopAllCoroutines();
                    _canAttack = true;
                    StartCoroutine(BeginFreeze());
                }
                break;
        }
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

        if (!_isRepositioning && !_isShieldUp)
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

            if (_targetTime <= 0f && _canAttack && !_isShieldUp)
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
            yield return StartCoroutine(LaserShield());
        }
        else
        {
            InitiateProjectileAttack();
        }
    }

    private void InitiateProjectileAttack()
    {
        _canAttack = false;
        _wavesLeft = Random.Range(2, maxProjectileWaves + 1);

        FireWave();
    }
    
    private void FireWave()
    {
        if (_wavesLeft <= 0)
        {
            _canAttack = true;
            return;
        }

        _animator.SetTrigger(Projectile);
    }

    private void ProjectileAttack()
    {
        var angleOffset = 360f / numberOfProjectiles;
        var startAngle = _wavesLeft % 2 == 0 ? 0f : angleOffset / 2f;

        for (var i = 0; i < numberOfProjectiles; i++)
        {
            var angle = startAngle + (i * angleOffset);
            var rad = angle * Mathf.Deg2Rad;
            var direction = new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0);

            var position = projectileOrigin.position + direction.normalized * projectileSpawnRadius;
            var newProjectile = Instantiate(lightProjectile, position, Quaternion.identity);
            newProjectile.GetComponent<HitboxHandler>().damageable = this;
            newProjectile.SetActive(true);

            var rb = newProjectile.GetComponent<Rigidbody>();
            var projectileRotation = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            rb.rotation = Quaternion.Euler(0, 0, projectileRotation);
            StartCoroutine(DelayForce(rb, direction));
        }

        AudioManager.Instance.PlayOneShot(FMODEvents.Instance.FlyingEnemyShoot, transform.position);

        _wavesLeft--;
        
        if (_wavesLeft > 0)
        {
            Invoke(nameof(FireWave), .25f); 
        }
        else
        {
            _canAttack = true;
        }
    }

    private IEnumerator DelayForce(Rigidbody rb, Vector3 dir)
    {
        yield return new WaitForSeconds(0.25f);
        if (rb != null)
        {
            rb.velocity = dir.normalized * projectileSpeed;
        }
    }

    private IEnumerator LaserShield()
    {
        _canAttack = false;
        _isShieldUp = true;

        shieldObject.SetActive(true);
        _hitboxHandler.enabled = false;
        _collider1.enabled = false;
        yield return StartCoroutine(FadeInShield(1.75f));
        _hitboxHandler.enabled = true;
        _collider1.enabled = true;
        
        var elapsed = 0f;
        while (elapsed < shieldDur)
        {
            shieldObject.transform.Rotate(0f, 0f, shieldRotSpeed * Time.deltaTime);

            elapsed += Time.deltaTime;
            yield return null;
        }
        
        shieldObject.SetActive(false);
        _hitboxHandler.enabled = false;
        shieldObject.transform.rotation = Quaternion.identity;
        AudioManager.Instance.SetEventParameter(_shieldEvent, "Shield Up", 1);
        _isShieldUp = false;
        _canAttack = true;
    }
    
    private IEnumerator FadeInShield(float fadeDuration)
    {
        var elapsedTime = 0f;
        var color = _spriteRenderer.color;
        color.a = 0f;
        _spriteRenderer.color = color;
        AudioManager.Instance.SetEventParameter(_shieldEvent, "Shield Up", 0);
        _shieldEvent.start();

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            var a = Mathf.Clamp01(elapsedTime / fadeDuration);
            color.a = a;
            _spriteRenderer.color = color;
            yield return null;
        }
        
        color.a = 1f;
        _spriteRenderer.color = color;
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
        
        if (Random.Range(0, 10) < 4) // 40 percent chance on hit for enemy to drop energy
        {
            Instantiate(Resources.Load<GameObject>("ItemPrefabs/Other/Energy Prefab"), transform.position, Quaternion.identity);
        }

        if (_health - damage > 0)
        {
            _health -= damage;
            //_healthSlider.value = _health;
            if (_health <= maxHealth / 2)
            {
                AudioManager.Instance.SetMusicParameter("Boss Phase", 1);
            }
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
            _healthTween = DOVirtual.Float(healthChangeSlider.value, _health, 1f, v => healthChangeSlider.value = v).SetEase(Ease.OutExpo).SetDelay(0.3f);
        }
        else
        {
            healthChangeSlider.value = _health;
            _healthTween = DOVirtual.Float(_healthSlider.value, _health, 1f, v => _healthSlider.value = v).SetEase(Ease.OutExpo).SetDelay(0.3f);
        }
    }
    
    private IEnumerator BeginFreeze()
    {
        _isFrozen = true;
        _isFreezing = true;
        canBeFrozen = false;
        healthFillImage.color = Color.cyan;

        yield return new WaitForSeconds(freezeDuration);

        healthFillImage.color = _healthDefault;
        _aiPath.canMove = true;
        _isFrozen = false;
        _canAttack = true;
        _isShieldUp = false;
        _isFreezing = false;
        _state = States.Attack;

        StartCoroutine(StartCooldown());
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
            healthFillImage.color = _healthDefault;
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
                if (!canBeFrozen || _isFreezing) return;
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
        dataHolder.totalEnemiesKilled++;
        dataHolder.playerEnemiesKilled++;
        RoomScripting.enemies.Remove(gameObject);
        AudioManager.Instance.PlayOneShot(FMODEvents.Instance.Explosion, transform.position);
        AudioManager.Instance.SetMusicParameter("Boss Phase", 3);
        LevelBuilder.Instance.bossDead = true;
        _characterAttack.ChanceHeal();
        
        StopAllCoroutines();
        
        var newGibs = Instantiate(gibs, transform.position, Quaternion.identity);

        foreach (var gib in newGibs.GetComponentsInChildren<Rigidbody>())
        {
            gib.AddForce(knockbackPower, ForceMode.Impulse);
        }

        foreach (var hb in GetComponentsInChildren<BoxCollider>()) // stops player being able to hit enemy on death
        {
            hb.gameObject.SetActive(false);
        }
        
        AudioManager.Instance.AttachInstanceToGameObject(_deathEvent, gameObject);
        int currencyToDrop = Random.Range(0, 12);
        for (int i = 0; i < currencyToDrop; i++)
        {
            Instantiate(Resources.Load<GameObject>("ItemPrefabs/Other/Currency Prefab"), transform.position, Quaternion.identity);
        }
        
        var energyToDrop = Random.Range(1, 10);
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
        //StartCoroutine(StunTimer(0.1f));

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
        _canAttack = true;
        _animator.SetBool("isStaggered", false);
    }
    
    private IEnumerator HitFlash()
    {
        spriteRenderer.material = hitMaterial;
        yield return new WaitForSeconds(0.1f);
        spriteRenderer.material = defaultMaterial;
    }
}
