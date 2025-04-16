using System;
using System.Collections;
using Cinemachine;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class CopyBoss : MonoBehaviour, IDamageable
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
    [SerializeField] private float comboChance;
    
    [Header("Tracking")] 
    [SerializeField] private float attackRange;
    private int _knockbackDir;
    private bool _hasPlayerBeenSeen;
    private Vector3 _lastPosition;
    private Vector3 _playerDir;
    private Vector3 _patrolTarget, _patrolPoint1, _patrolPoint2;
    private enum States { Idle, Chase, Attack, Frozen, Jumping, Crouching }
    private States _currentState = States.Idle;
    private int _jumpCount;
    private Collider _roomBounds;
    
    [Header("Timing")]
    [SerializeField] private float attackCooldown; // time between attacks
    [SerializeField] private float chaseDuration; // how long the enemy will chase after player leaves range
    [SerializeField] private float freezeDuration; // how long the enemy is frozen for
    [SerializeField] private float freezeCooldown; // how long until the enemy can be frozen again
    private float _playerBelowTimer;
    private float _activeAtkDelay;
    
    [Header("Enemy Properties")]
    [SerializeField] private bool canBeFrozen;
    [SerializeField] private bool canBeStunned;
    [SerializeField] private bool isIdle;
    [SerializeField] private bool canJump;
    [SerializeField] private int maxJumpCount;
    [SerializeField] private float movementSpeed;
    [SerializeField] private float jumpForce;
    [SerializeField] private float jumpTriggerDistance;
    private bool _isFrozen;
    private bool _isPoisoned;
    private bool _lowHealth;
    private bool _isStuck;
    private int _poisonBuildup;
    private int _poiseBuildup;
    private bool _isFallingThrough;
    private bool _isStunned;
    private bool _isAttacking, _isDead, _isJumping, _isKnockedBack;
    
    [Header("References")] 
    [SerializeField] private Transform atkHitbox;
    [SerializeField] private Image healthFillImage;
    [SerializeField] private Slider healthSlider;
    [SerializeField] private float fallThroughTime = 2f;
    private CinemachineImpulseSource _impulseSource;
    private Vector3 _impulseVector; 
    private CapsuleCollider _bossCollider;
    private RoomScripting _roomScripting;
    private Rigidbody _rigidbody;
    private Animator _animator;
    private Transform _player;
    private SpriteRenderer _spriteRenderer;
    private SettingManager _settingManager;
    private MaterialPropertyBlock _propertyBlock;
    private bool _tookDamage;
    private static readonly int BaseColor = Shader.PropertyToID("_BaseColor");
    private GameObject dialogueGui;

    int IDamageable.Attack { get => attack; set => attack = value; }
    int IDamageable.Poise { get => poise; set => poise = value; }
    int IDamageable.PoiseDamage { get => poiseDamage; set => poiseDamage = value; }
    public bool isPlayerInRange { get; set; }
    public bool isDead { get => _isDead; set => _isDead = value; }
    public RoomScripting RoomScripting { get; set; }
    public EnemySpawner EnemySpawner { get; set; }

    private void Start()
    {
        _roomScripting = gameObject.transform.root.GetComponent<RoomScripting>();
        _roomScripting.enemies.Add(gameObject);
        _roomBounds = _roomScripting.GetComponent<Collider>();
        _impulseSource = GetComponent<CinemachineImpulseSource>();
        _settingManager = GameObject.Find("Settings").GetComponent<SettingManager>();
        _bossCollider = GetComponent<CapsuleCollider>();
        _rigidbody = GetComponent<Rigidbody>();
        _animator = GetComponent<Animator>();
        _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        _player = GameObject.FindGameObjectWithTag("Player").transform;
        _propertyBlock = new MaterialPropertyBlock();
        dialogueGui = GameObject.FindGameObjectWithTag("UIManager").GetComponent<MenuHandler>().dialogueGUI;

        _health = maxHealth;
        healthSlider.maxValue = maxHealth;
        healthSlider.value = maxHealth;
        healthSlider.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (_isDead) return;

        if (dialogueGui.activeSelf)
        {
            return;
        }

        var distance = Vector3.Distance(transform.position, _player.position);
        var heightDiffAbove = _player.position.y - transform.position.y;
        var heightDiffBelow = transform.position.y - _player.position.y;
        _playerDir = _player.position - transform.position;
        
        if (IsGrounded())
        {
            _jumpCount = 0;
        }

        switch (IsPlayerInRoom())
        {
            case true when !healthSlider.gameObject.activeSelf:
                healthSlider.gameObject.SetActive(true);
                break;
            case false when healthSlider.gameObject.activeSelf:
                healthSlider.gameObject.SetActive(false);
                break;
        }

        if (_isStunned) return;
        
        if (_isFrozen)
        {
            _currentState = States.Frozen;
        }
        else if (distance < attackRange)
        {
            _currentState = States.Attack;
        }
        else if (IsPlayerInRoom())
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
                if (Mathf.Abs(_rigidbody.velocity.x) > 0.1f)
                {
                    UpdateSpriteDirection(_rigidbody.velocity.x < 0);
                }
                else
                {
                    UpdateSpriteDirection(_playerDir.x < 0);
                }
            }
        }
        else
        {
            _currentState = States.Idle;
        }

        switch (_currentState)
        {
            case States.Idle:
                break;
            case States.Chase:
                Chase();
                break;
            case States.Attack:
                if (!_isAttacking)
                {
                    UpdateSpriteDirection(_playerDir.x < 0);
                    StartCoroutine(Attack());
                }
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
            _animator.SetFloat("XVelocity", Mathf.Abs(_rigidbody.velocity.x));
        }
    }
    
    private void UpdateSpriteDirection(bool isLeft)
    {
        var localScale = _spriteRenderer.transform.localScale;
        var localScale2 = atkHitbox.localScale;
        
        if (!isLeft)
        {
            localScale = new Vector3(Mathf.Abs(localScale.x), localScale.y, localScale.z);
            localScale2 = new Vector3(Mathf.Abs(localScale2.x), localScale2.y, localScale2.z);
        }
        else
        {
            localScale = new Vector3(-Mathf.Abs(localScale.x), localScale.y, localScale.z);
            localScale2 = new Vector3(-Mathf.Abs(localScale2.x), localScale2.y, localScale2.z);
        }
        
        _spriteRenderer.transform.localScale = localScale;
        atkHitbox.localScale = localScale2;
    }
    
    private bool IsPlayerInRoom()
    {
        return _roomBounds != null && _roomBounds.bounds.Contains(_player.position) && !dialogueGui.activeSelf;
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
            StartCoroutine(DisableCollision(platform));
        }
    }
    
    private GameObject FindPlatform(Vector3 dir)
    {
        var layerMask = LayerMask.GetMask("Ground");
        
        if (Physics.Raycast(transform.position, dir, out var hit, 10f, layerMask))
        {
            var platform = hit.collider.GetComponentInParent<SemiSolidPlatform>();
            if (platform != null)
            {
                return platform.gameObject;
            }
        }
        
        return null;
    }

    private void Jump()
    {
        if (_jumpCount >= maxJumpCount) return;

        _jumpCount++;

        var platform = FindPlatform(Vector3.up);
    
        if (platform != null)
        {
            StartCoroutine(DisableCollision(platform));
        }

        var newForce = Mathf.Sqrt(2 * Mathf.Abs(Physics.gravity.y) * (_player.position.y - transform.position.y + 1.5f));
        newForce = Mathf.Max(newForce, jumpForce);
        
        var dirX = Mathf.Sign(transform.localScale.x);
        
        _rigidbody.velocity = new Vector3(2f * dirX, newForce, _rigidbody.velocity.z);
        _animator.SetTrigger("Jump");
    }

    private IEnumerator DisableCollision(GameObject platform)
    {
        if (platform.GetComponent<SemiSolidPlatform>() == null) yield break;
        
        if (_bossCollider != null && platform != null)
        {
            foreach (var collider in platform.GetComponentsInChildren<Collider>())
            {
                Physics.IgnoreCollision(_bossCollider, collider, true);  
            }
            
            yield return new WaitForSeconds(.8f);
            
            foreach (var collider in platform.GetComponentsInChildren<Collider>())
            {
                Physics.IgnoreCollision(_bossCollider, collider, false);
            }
        }

        _isFallingThrough = false;
    }
    
    private void MoveTowards(Vector3 target)
    {
        if (_isKnockedBack || _isAttacking) return;
        
        var direction = (target - transform.position).normalized;
        _rigidbody.velocity = new Vector3(direction.x * movementSpeed, _rigidbody.velocity.y, direction.z);
    }

    private void Chase()
    {
        MoveTowards(_player.position);
    }
    
    private IEnumerator Attack()
    {
        _isAttacking = true;
        _rigidbody.velocity = Vector3.zero;

        var comboCount = Random.Range(2, 6);
        defense = 40;

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

            yield return new WaitForSeconds(_activeAtkDelay);

            if (Random.value > comboChance) // if combo stopped increase chance to combo
            {
                comboChance += 0.1f;
                defense = 0;
                _activeAtkDelay = attackCooldown;
                break;
            }

            _activeAtkDelay = 0f;
            comboChance -= 0.05f; // if combo triggered reduce chance to combo
        }

        defense = 0;
        yield return new WaitForSeconds(attackCooldown);
        _isAttacking = false;
    }

    private IEnumerator BeginFreeze()
    {
        if (!canBeFrozen) yield break;

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
        canBeFrozen = false;
        yield return new WaitForSeconds(freezeCooldown);
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
    
    public void TakeDamage(int damage, int? poiseDmg, Vector3? knockback)
    {
        if (_isDead) return;
        defense = Mathf.Clamp(defense, 0, 100);
        var dmgReduction = (100 - defense) / 100f;
        damage = Mathf.RoundToInt(damage * dmgReduction);
        
        _health -= damage;
        healthSlider.value = _health;
        
        if (_health <= 0)
        {
            Die();
            return;
        }
        
        //StartCoroutine(HitFlash(Color.cyan, 0.1f));
        
        if (_health <= maxHealth / 2)
        {
            AudioManager.Instance.SetMusicParameter("Boss Phase", 1);
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

    public void ApplyKnockback(Vector2 knockbackPower)
    {
        if (_isFrozen || isDead) return;

        _knockbackDir = transform.position.x > _player.position.x ? 1 : -1;
        
        var knockbackMultiplier = (_poiseBuildup >= poise) ? 4 : 2; 
        var knockbackForce = new Vector3(knockbackPower.x * _knockbackDir * knockbackMultiplier, knockbackPower.y * knockbackMultiplier, 0);

        if (!_isAttacking)
        {
            StartCoroutine(TriggerKnockback(knockbackForce, 0.1f));
            StartCoroutine(StunTimer(0.005f));
        }

        if (_poiseBuildup >= poise)
        {
            StartCoroutine(StunTimer(.5f));
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
        _animator.SetBool("isDead", true);
        StopAllCoroutines();
        int currencyToDrop = 0;
        switch (_tookDamage)
        {
            case false:
                currencyToDrop = Random.Range(3, 11);
                break;
            case true:
                currencyToDrop = Random.Range(0, 5);
                break;
        }
       
        for (int i = 0; i < currencyToDrop; i++)
        {
            Instantiate(Resources.Load<GameObject>("ItemPrefabs/Other/Currency Prefab"), transform.position, Quaternion.identity);
        }
        _impulseVector = new Vector3(Random.Range(-1, 1), 5, 0);
        _impulseSource.m_ImpulseDefinition.m_ImpulseShape = CinemachineImpulseDefinition.ImpulseShapes.Explosion;
        _impulseSource.GenerateImpulseWithVelocity(_impulseVector * _settingManager.screenShakeMultiplier);
        AudioManager.Instance.PlayOneShot(FMODEvents.Instance.Explosion, transform.position);
        AudioManager.Instance.SetMusicParameter("Boss Phase", 4);
        //_characterMovement.lockedOn = false;
        //_lockOnController.lockedTarget = null;
        _roomScripting.enemies.Remove(gameObject);
        StartCoroutine(WaitToDisable());
    }

    private IEnumerator WaitToDisable()
    {
        yield return new WaitForSecondsRealtime(1.5f);
        gameObject.SetActive(false);
    }

    private void OnDrawGizmos()
    {
        var dist = 3f;
        Gizmos.DrawRay(transform.position, Vector3.down * dist);
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
        _propertyBlock.SetColor(BaseColor, new Color(11, 57, 94));
        _spriteRenderer.SetPropertyBlock(_propertyBlock);
    }
}