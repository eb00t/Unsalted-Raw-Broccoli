using System;
using System.Collections;
using Cinemachine;
using Pathfinding;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.TextCore.Text;
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
    private int _hitCount;
    
    [Header("Offensive Stats")] 
    [SerializeField] private int attack;
    [SerializeField] private int poiseDamage;
    [SerializeField] private float comboChance;
    [SerializeField] private Vector3 knockbackPower;
    [SerializeField] private float hardcoreMultiplier;
    
    [Header("Tracking")] 
    [SerializeField] private float attackRange;
    private int _knockbackDir;
    private bool _hasPlayerBeenSeen;
    private Vector3 _lastPosition;
    private Vector3 _playerDir;
    private Vector3 _patrolTarget, _patrolPoint1, _patrolPoint2;
    private enum States { Idle, Chase, Attack, Frozen, Jumping, Crouching }
    private States _currentState = States.Idle;
    //private int _jumpCount;
    private Collider _roomBounds;
    
    [Header("Timing")]
    [SerializeField] private float attackCooldown; // time between attacks
    [SerializeField] private float chaseDuration; // how long the enemy will chase after player leaves range
    [SerializeField] private float freezeDuration; // how long the enemy is frozen for
    [SerializeField] private float freezeCooldown; // how long until the enemy can be frozen again
    private float _jumpTimer;
    private float _playerBelowTimer;
    private float _activeAtkDelay;
    
    [Header("Enemy Properties")]
    [SerializeField] private bool canBeFrozen;
    [SerializeField] private bool canBeStunned;
    [SerializeField] private bool isIdle;
    [SerializeField] private bool canJump;
    [SerializeField] private int maxJumpCount;
    [SerializeField] private float movementSpeed;
    [SerializeField] private float jumpCooldown;
    [SerializeField] private float jumpForce;
    [SerializeField] private float jumpTriggerDistance;
    [SerializeField] private float maxJumpHeight;
    [SerializeField] private float reboundForce;
    private int _attackType;
    private int _comboNumber;
    private Color _healthDefault;
    private bool _isFrozen;
    private bool _isPoisoned;
    private bool _lowHealth;
    private bool _isStuck;
    private int _poisonBuildup;
    private int _poiseBuildup;
    private bool _isFallingThrough;
    private bool _isStunned;
    private bool _isAttacking, _isDead, _isJumping, _isKnockedBack;
    private bool _hasDialogueTriggered;
    
    [Header("References")] 
    [SerializeField] private Transform atkHitbox;
    [SerializeField] private Image healthFillImage;
    [SerializeField] private Slider healthSlider;
    [SerializeField] private float fallThroughTime = 2f;
    [SerializeField] private Material defaultMaterial, hitMaterial;
    private AIPath _aiPath;
    private CinemachineImpulseSource _impulseSource;
    private Vector3 _impulseVector; 
    private CapsuleCollider _bossCollider;
    private RoomScripting _roomScripting;
    private Rigidbody _rigidbody;
    private Animator _animator;
    private Transform _player;
    private SpriteRenderer _spriteRenderer;
    private SettingManager _settingManager;
    private bool _tookDamage;
    private GameObject dialogueGui;
    private CharacterAttack _characterAttack;
    private Coroutine _atkCD;
    private DialogueTrigger[] _dialogueTriggers;
    [SerializeField] private DataHolder dataHolder;

    int IDamageable.Attack { get => attack; set => attack = value; }
    int IDamageable.Poise { get => poise; set => poise = value; }
    int IDamageable.PoiseDamage { get => poiseDamage; set => poiseDamage = value; }
    Vector3 IDamageable.KnockbackPower { get => knockbackPower; set => knockbackPower = value; }
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
        _aiPath = GetComponent<AIPath>();
        _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        _player = GameObject.FindGameObjectWithTag("Player").transform;
        dialogueGui = GameObject.FindGameObjectWithTag("UIManager").GetComponent<MenuHandler>().dialogueGUI;
        _characterAttack = _player.GetComponentInChildren<CharacterAttack>();
        _healthDefault = healthFillImage.color;
        _health = maxHealth;
        healthSlider.maxValue = maxHealth;
        healthSlider.value = maxHealth;
        healthSlider.gameObject.SetActive(false);
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
        if (_isDead) return;

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

        var distance = Vector3.Distance(transform.position, _player.position);
        var heightDiffAbove = _player.position.y - transform.position.y;
        var heightDiffBelow = transform.position.y - _player.position.y;
        _playerDir = _player.position - transform.position;
        _jumpTimer -= Time.deltaTime;

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
        else if (distance <= attackRange)
        {
            _currentState = States.Attack;
        }
        else if (IsPlayerInRoom())
        {
            if (heightDiffAbove > jumpTriggerDistance && _jumpTimer <= 0f && IsGrounded())
            {
                _jumpTimer = jumpCooldown;
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
                    UpdateSpriteDirection(_aiPath.velocity.x < 0);
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
                _aiPath.canMove = false;
                break;
            case States.Chase:
                _aiPath.canMove = true;
                Chase();
                break;
            case States.Attack:
                if (!_isAttacking)
                {
                    _aiPath.canMove = true;
                    StartCoroutine(Attack());
                }
                else
                {
                    _aiPath.canMove = false;
                }
                break;
            case States.Frozen:
                _aiPath.canMove = false;
                StartCoroutine(BeginFreeze());
                break;
            case States.Jumping:
                if (!_isAttacking)
                {
                    Jump();
                }
                break;
            case States.Crouching:
                if (!_isAttacking)
                {
                    Crouching();
                }
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        if (!_isKnockedBack)
        {
            _animator.SetFloat("XVelocity", Mathf.Abs(_aiPath.velocity.x));
        }
        
        _animator.SetBool("canMove", _aiPath.canMove);
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
        return Physics.Raycast(transform.position, Vector3.down, 2.25f, LayerMask.GetMask("Ground"));
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
        var platform = FindPlatform(Vector3.down); 

        if (platform != null)
        {
            _isFallingThrough = true;
            _animator.SetBool("isCrouching", true);
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
        var targetPos = new Vector3(_player.position.x, _player.position.y, transform.position.z);
        
        var platform = FindPlatform(Vector3.up);
        if (platform != null)
        {
            var verticalDistance = Mathf.Abs(platform.transform.position.y - transform.position.y);
    
            if (verticalDistance > maxJumpHeight)
            {
                _currentState = States.Chase;
                return;
            }

            StartCoroutine(DisableCollision(platform));
        }

        TriggerJump(targetPos);

        if (Random.Range(0f, 1f) < 0.5f)
        {
            StartCoroutine(JumpAttack());
        }
        else
        {
            _currentState = States.Chase;
        }
    }

    private IEnumerator JumpAttack()
    {
        _isAttacking = true;
        _animator.SetTrigger("jumpAttack");

        yield return new WaitUntil(() => IsGrounded() || isDead);

        _isAttacking = false;
        _currentState = States.Chase;
    }

    private void TriggerJump(Vector3 target)
    {
        _aiPath.canMove = false;
        _rigidbody.useGravity = true;

        var force = CalculateForce(transform.position, target, 8f);

        if (float.IsNaN(force.x) || float.IsNaN(force.y) || float.IsNaN(force.z)) return;

        _rigidbody.velocity = Vector3.zero;
        _rigidbody.AddForce(force, ForceMode.VelocityChange);
        AudioManager.Instance.PlayOneShot(FMODEvents.Instance.EnemyJump, transform.position);
        StartCoroutine(PostJumpDelay(0.5f));
    }
    
    private IEnumerator PostJumpDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        _aiPath.canMove = true;
    }
    
    private Vector3 CalculateForce(Vector3 start, Vector3 end, float jumpHeight)
    {
        var direction = end - start;
        var gravity = Mathf.Abs(Physics.gravity.y);
        var horizontalDir = new Vector3(direction.x, 0f, direction.z);
        var yDiff = end.y - start.y;

        if (jumpHeight < yDiff)
        {
            jumpHeight = yDiff + 2f;
        }

        var yVelocity = Mathf.Sqrt(2f * gravity * jumpHeight);
        var maxHeightTime = yVelocity / gravity;

        var fallHeight = jumpHeight - yDiff;
        if (fallHeight < 0f) fallHeight = 0f;

        var timeToDescend = Mathf.Sqrt(2f * fallHeight / gravity);
        var totalTime = maxHeightTime + timeToDescend;

        if (totalTime <= 0.01f) return Vector3.zero;

        var horizontalVelocity = horizontalDir / totalTime;
        return horizontalVelocity + Vector3.up * yVelocity;
    }

    private IEnumerator DisableCollision(GameObject platform)
    {
        if (_bossCollider == null || platform == null)
        {
            _animator.SetBool("isCrouching", false);
            _isFallingThrough = false;
            yield break;
        }

        var verticalDistance = Mathf.Abs(platform.transform.position.y - transform.position.y);
        var est = verticalDistance * 0.11f; // estimate time to disable collision based on how far the platform is

        foreach (var col in platform.GetComponentsInChildren<Collider>())
        {
            Physics.IgnoreCollision(_bossCollider, col, true);
        }

        yield return new WaitForSeconds(est);

        foreach (var col in platform.GetComponentsInChildren<Collider>())
        {
            Physics.IgnoreCollision(_bossCollider, col, false);
        }
        
        _animator.SetBool("isCrouching", false);
        _isFallingThrough = false;
    }
    
    private void Chase()
    {
        if (_isKnockedBack || _isAttacking || _aiPath == null)
        {
            _aiPath.destination = new Vector3(_aiPath.destination.x, transform.position.y, _aiPath.destination.z);
            return;
        }

        _aiPath.canMove = true;
        _aiPath.maxSpeed = movementSpeed;
        if (transform.position.y - _player.position.y > 0.25f)
        {
            _aiPath.destination = new Vector3(_player.position.x, _player.position.y, transform.position.z);
        }
        else
        {
            _aiPath.destination = new Vector3(_player.position.x, transform.position.y, transform.position.z);
        }
    }
    
    private IEnumerator Attack()
    {
        _isAttacking = true;

        var attackRoll = Random.Range(0, 10);
        defense = 40;

        switch (attackRoll)
        {
            case < 2: // 20% chance for heavy
                _animator.SetTrigger("HeavyAttack0");
                UpdateSpriteDirection(_playerDir.x < 0);
                break;
            case < 4: // 30% chance for medium
                _attackType = 1;
                _comboNumber = Random.Range(0, 2);
                _animator.SetTrigger("MediumAttack0");
                UpdateSpriteDirection(_playerDir.x < 0);
                break;
            case >= 4: // 50% chance for light
                _attackType = 2;
                _comboNumber = Random.Range(0, 3);
                _animator.SetTrigger("LightAttack0");
                UpdateSpriteDirection(_playerDir.x < 0);
                break;
        }
        
        yield return new WaitUntil(() => _comboNumber == 0);
        defense = 0;
        if (_atkCD == null)
        {
            _atkCD = StartCoroutine(CooldownAttack());
        }
    }

    private IEnumerator CooldownAttack()
    {
        yield return new WaitForSeconds(attackCooldown);
        _isAttacking = false;
        _atkCD = null;
    }

    private void BufferNextAttack()
    {
        if (_comboNumber > 0)
        {
            switch (_attackType)
            {
                case 1: // medium attack
                    _animator.SetTrigger("MediumAttack1");
                    break;
                case 2: // light attack
                    if (_comboNumber == 2)
                    {
                        _animator.SetTrigger("LightAttack1");
                    }
                    else
                    {
                        _animator.SetTrigger("LightAttack2");
                    }
                    break;
            }
            
            _comboNumber--;
        }
    }

    private IEnumerator BeginFreeze()
    {
        if (!canBeFrozen) yield break;

        _isFrozen = true;
        healthFillImage.color = Color.cyan;
        //_rigidbody.velocity = Vector3.zero;

        yield return new WaitForSeconds(freezeDuration);

        healthFillImage.color = _healthDefault;
        _isFrozen = false;
        _isAttacking = false;
        _aiPath.canMove = true;
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
            healthFillImage.color = _healthDefault;
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
        StartCoroutine(HitFlash());
        
        if (Random.Range(0, 10) < 4) // 40 percent chance on hit for enemy to drop energy
        {
            Instantiate(Resources.Load<GameObject>("ItemPrefabs/Other/Energy Prefab"), transform.position, Quaternion.identity);
        }
        
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
                _hitCount++;
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
            StartCoroutine(StunTimer(0.1f));
            StartCoroutine(WallHitCheck(3f));
            _hitCount = 0;
        }

        if (_poiseBuildup >= poise && !_isAttacking)
        {
            StartCoroutine(StunTimer(.5f));
            _poiseBuildup = 0;
        }
    }
    
    private IEnumerator TriggerKnockback(Vector3 force, float duration)
    {
        _isKnockedBack = true;
        _aiPath.canMove = false;
        _rigidbody.velocity = Vector3.zero;
        _rigidbody.AddForce(force, ForceMode.Impulse);
        yield return new WaitForSeconds(duration);
        //_isAttacking = false;
        _aiPath.canMove = true;
        _rigidbody.velocity = Vector3.zero;
        _isKnockedBack = false;
    }
    
    private IEnumerator WallHitCheck(float dur)
    {
        var elapsed = 0f;

        while (elapsed < dur)
        {
            var dir = new Vector3(_rigidbody.velocity.x, 0, 0).normalized;
            var hits = Physics.RaycastAll(transform.position, dir, 4f);

            foreach (var hit in hits)
            {
                if (Mathf.Abs(hit.normal.y) < 0.5 
                    && !hit.collider.tag.Contains("Player")
                    && !hit.collider.isTrigger
                    && !hit.collider.CompareTag("Enemy")
                    && hit.collider.gameObject.layer != 19
                    && hit.collider.gameObject.layer != 16
                    && hit.collider.gameObject.layer != 18)
                {
                    ReboundForce(hit.normal);
                    Debug.Log("rebound: " + hit.collider.name);
                    yield break;
                }
            }
            
            elapsed += Time.deltaTime;
            yield return null;
        }
    }

    private void ReboundForce(Vector3 normal)
    {
        var reflectVel = Vector3.Reflect(_rigidbody.velocity, normal);
        _rigidbody.velocity = Vector3.zero;
        _rigidbody.AddForce(reflectVel.normalized * reboundForce, ForceMode.Impulse);
    }
    
    private IEnumerator StunTimer(float stunTime)
    {
        _animator.SetBool("isStaggered", true);
        _isStunned = true;
        yield return new WaitForSecondsRealtime(stunTime);
        _isStunned = false;
        //_isAttacking = false;
        _animator.SetBool("isStaggered", false);
    }

    private void Die()
    {
        isDead = true; 
        _animator.SetBool("isDead", true);
        StopAllCoroutines();
        _spriteRenderer.material = defaultMaterial;
        _characterAttack.ChanceHeal();
        _impulseVector = new Vector3(Random.Range(-1, 1), 5, 0);
        _impulseSource.m_ImpulseDefinition.m_ImpulseShape = CinemachineImpulseDefinition.ImpulseShapes.Explosion;
        _impulseSource.GenerateImpulseWithVelocity(_impulseVector * _settingManager.screenShakeMultiplier);
        AudioManager.Instance.PlayOneShot(FMODEvents.Instance.Explosion, transform.position);
    }

    private void DisableBoss()
    {
        var energyToDrop = Random.Range(1, 10);
        for (var i = 0; i < energyToDrop; i++)
        {
            Instantiate(Resources.Load<GameObject>("ItemPrefabs/Other/Energy Prefab"), transform.position, Quaternion.identity);
        }
        
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
        
        AudioManager.Instance.SetMusicParameter("Boss Phase", 4);
        LevelBuilder.Instance.bossDead = true;
        _impulseVector = new Vector3(Random.Range(-1, 1), 5, 0);
        _impulseSource.m_ImpulseDefinition.m_ImpulseShape = CinemachineImpulseDefinition.ImpulseShapes.Explosion;
        _impulseSource.GenerateImpulseWithVelocity(_impulseVector * _settingManager.screenShakeMultiplier);
        AudioManager.Instance.PlayOneShot(FMODEvents.Instance.Explosion, transform.position);
        AudioManager.Instance.SetMusicParameter("Boss Phase", 3);
        //_characterMovement.lockedOn = false;
        //_lockOnController.lockedTarget = null;
        _roomScripting.enemies.Remove(gameObject);
        gameObject.SetActive(false);
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawRay(transform.position, Vector3.down * 2.25f);
    }
    
    private IEnumerator HitFlash()
    {
        _spriteRenderer.material = hitMaterial;
        yield return new WaitForSeconds(0.1f);
        _spriteRenderer.material = defaultMaterial;
    }
}