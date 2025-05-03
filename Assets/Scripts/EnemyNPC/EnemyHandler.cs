using System;
using System.Collections;
using FMOD.Studio;
using Pathfinding;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = UnityEngine.Random;


// Adapted from https://github.com/Chaker-Gamra/2.5D-Platformer-Game/blob/master/Assets/Scripts/Enemy/Enemy.cs

public class EnemyHandler : MonoBehaviour, IDamageable
{
    [Header("Defensive Stats")] 
    [SerializeField] private int maxHealth;
    private int _health;
    [SerializeField] private int poise;
    public int defense;
    [SerializeField] private int poisonResistance;
    [SerializeField] private int blockingDefense;

    [Header("Offensive Stats")] 
    [SerializeField] private int attack;
    [SerializeField] private int poiseDamage;
    [SerializeField] private int numberOfAttacks;
    [SerializeField] private Vector3 knockbackPower;
    [SerializeField] private float floor2Multiplier, floor3Multiplier, floor4Multiplier;

    [Header("Tracking")] 
    [SerializeField] private float attackRange;
    [SerializeField] private float patrolRange;
    private int _knockbackDir;
    private Vector3 _lastPosition;
    [SerializeField] private Collider _roomBounds;
    private Vector3 _playerDir;
    private Vector3 _patrolTarget, _patrolPoint1, _patrolPoint2;
    private enum States { Idle, Patrol, Chase, Attack, Frozen, Passive, Jump }
    private States _state = States.Idle;

    [Header("Timing")]
    [SerializeField] private float attackCooldown; // time between attacks
    [SerializeField] private float freezeDuration; // how long the enemy is frozen for
    [SerializeField] private float freezeCooldown; // how long until the enemy can be frozen again
    [SerializeField] private float maxTimeToReachTarget; // how long will the enemy try to get to the target before switching
    [SerializeField] private float bombTimeVariability;
    [SerializeField] private float lungeCooldown;
    [SerializeField] private float blockDuration;
    [SerializeField] private float jumpCooldown;
    //[SerializeField] private float jumpThroughTime;
    private float _timeSinceLastMove;
    private float _targetTime;
    private float _jumpTimer;
    
    [Header("Enemy Properties")] 
    public bool isBomb;
    [SerializeField] private bool isStalker;
    [SerializeField] private bool canBlock;
    [SerializeField] private bool isInvisible;
    [SerializeField] private bool canBeFrozen;
    [SerializeField] private bool canBeStunned;
    [SerializeField] private bool doesEnemyPatrol;
    [SerializeField] private bool isPassive;
    [SerializeField] private float lungeForce;
    [SerializeField] private float jumpTriggerDistance;
    [SerializeField] private float maxJumpHeight;
    [SerializeField] private float reboundForce;
    private Vector3 _knockbackForce;
    public bool alwaysShowHealth;
    private bool _isFrozen;
    private bool _isPoisoned;
    private bool _lowHealth;
    private bool _isStuck;
    private int _poisonBuildup;
    private int _poiseBuildup;
    private bool _canLunge = true;
    private bool _isBlocking;
    private bool _wasLastAttackBlock;

    [Header("References")] 
    [SerializeField] private Transform passiveTarget;
    [SerializeField] private BoxCollider attackHitbox;
    [SerializeField] private Image healthFillImage;
    [SerializeField] private Material defaultMaterial, hitMaterial, invisMaterial;
    [SerializeField] private GameObject gibs;
    [SerializeField] private Transform explosionVFX;
    private Slider _healthSlider;
    private Animator _animator;
    private Transform _target;
    private NavMeshAgent _agent;
    private CharacterAttack _characterAttack;
    private SpriteRenderer _spriteRenderer;
    private AIPath _aiPath;
    private Rigidbody _rigidbody;
    
    [Header("Sound")]
    private EventInstance _alarmEvent;
    private EventInstance _deathEvent;
    private EventInstance _jumpEvent;
    
    private static readonly int IsExplode = Animator.StringToHash("isExplode");
    private static readonly int Vel = Animator.StringToHash("vel");
    private static readonly int Attack1 = Animator.StringToHash("Attack");
    private static readonly int Attack2 = Animator.StringToHash("Attack2");
    private static readonly int IsBlocking = Animator.StringToHash("isBlocking");
    private static readonly int Detonate = Animator.StringToHash("Detonate");
    private static readonly int IsStaggered = Animator.StringToHash("isStaggered");
    private CapsuleCollider _enemyCollider;

    int IDamageable.Attack { get => attack; set => attack = value; }
    int IDamageable.Poise { get => poise; set => poise = value; }
    int IDamageable.PoiseDamage { get => poiseDamage; set => poiseDamage = value; }
    Vector3 IDamageable.KnockbackPower { get => knockbackPower; set => knockbackPower = value; }
    public bool isPlayerInRange { get; set; }
    public bool isDead { get; set; }
    public RoomScripting RoomScripting { get; set; }
    public EnemySpawner EnemySpawner { get; set; }
    
    private void Start()
    {
        if (LevelBuilder.Instance.currentFloor != LevelBuilder.LevelMode.Tutorial && LevelBuilder.Instance.currentFloor != LevelBuilder.LevelMode.Intermission)
        {
            RoomScripting = gameObject.transform.root.GetComponent<RoomScripting>();
            RoomScripting.enemies.Add(gameObject);
            _roomBounds = RoomScripting.GetComponent<Collider>();
        }
        
        ScaleStats();
        _jumpEvent = AudioManager.Instance.CreateEventInstance(FMODEvents.Instance.EnemyJump);
        AudioManager.Instance.AttachInstanceToGameObject(_jumpEvent, gameObject);;
        _healthSlider = GetComponentInChildren<Slider>();
        _healthSlider.maxValue = maxHealth;
        _healthSlider.value = maxHealth;
        _health = maxHealth;
        _healthSlider.gameObject.SetActive(alwaysShowHealth);
        _animator = GetComponent<Animator>();
        _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        _aiPath = GetComponent<AIPath>();
        _rigidbody = GetComponent<Rigidbody>();
        _enemyCollider = GetComponent<CapsuleCollider>();
        _target = GameObject.FindGameObjectWithTag("Player").transform;
        _characterAttack = _target.GetComponentInChildren<CharacterAttack>();
        PickPatrolPoints();
        _patrolTarget = _patrolPoint1;
        _jumpTimer = jumpCooldown;

        if (isStalker)
        {
            gameObject.transform.localScale = new Vector3(1.25f, 1.25f, 1.25f);
        }
        else if (gameObject.name.Contains("Robot"))
        {
            gameObject.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
        }

        if (isBomb)
        {
            var newCooldown = attackCooldown + Random.Range(-bombTimeVariability, bombTimeVariability);
            newCooldown = Mathf.Clamp(newCooldown, 0.1f, attackCooldown + bombTimeVariability);
            _targetTime = newCooldown;
        }

        _deathEvent = AudioManager.Instance.CreateEventInstance(FMODEvents.Instance.EnemyDeath);

        if (isPassive)
        {
            defense = 100;
        }
        
        _alarmEvent = AudioManager.Instance.CreateEventInstance(FMODEvents.Instance.EnemyLowHealthAlarm);
        AudioManager.Instance.AttachInstanceToGameObject(_alarmEvent, gameObject);
        _targetTime = attackCooldown / 2;
    }

    private void Update()
    {
        var velocity = _aiPath.velocity;
        _target = isPassive ? passiveTarget : _target;
        _playerDir = _target.position - transform.position;
        var distance = Vector3.Distance(transform.position, _target.position);
        var heightDiffAbove = _target.position.y - transform.position.y;

        if (isStalker && canBlock)
        {
            if (_isBlocking)
            {
                var facingRight = _spriteRenderer.transform.localScale.x > 0;
                var playerInFront = (facingRight && _playerDir.x > 0) || (!facingRight && _playerDir.x < 0);

                defense = playerInFront ? blockingDefense : 0;
            }
            else if (_state != States.Attack && !isPassive)
            {
                defense = 0;
            }
        }

        if (alwaysShowHealth)
        {
            _healthSlider.gameObject.SetActive(true);
        }

        Repulsion();

        if (_isFrozen)
        {
            _state = States.Frozen;
        }
        else if (isPassive)
        {
            _state = States.Passive;
        }
        else if (IsPlayerInRoom())
        {
            _jumpTimer -= Time.deltaTime;
            if (distance < attackRange)
            {
                _state = States.Attack;
            }
            else
            {
                if (heightDiffAbove > jumpTriggerDistance && _jumpTimer <= 0f && IsGrounded())
                {
                    _jumpTimer = jumpCooldown;
                    _state = States.Jump;
                    _jumpEvent.start();
                }
                else
                {
                    _state = States.Chase;
                }
            }
        }
        else
        {
            _state = doesEnemyPatrol ? States.Patrol : States.Idle;
        }

        if (_state is States.Passive or States.Chase or States.Patrol)
        {
            if (isStalker && _isBlocking) return;

            if (Mathf.Abs(velocity.x) > 0.1f)
            {
                UpdateSpriteDirection(velocity.x < 0);
            }
            else
            {
                UpdateSpriteDirection(_playerDir.x < 0);
            }
        }

        if (isInvisible && _state is States.Chase or States.Jump)
        {
            _spriteRenderer.material = invisMaterial;
            if (!alwaysShowHealth)
            {
                _healthSlider.gameObject.SetActive(false);
            }
        }
        else if (isInvisible && _state is States.Attack)
        {
            _spriteRenderer.material = defaultMaterial;
            _healthSlider.gameObject.SetActive(true);
        }

        switch (_state)
        {
            case States.Idle:
                _aiPath.canMove = false;
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
                _aiPath.canMove = false;
                Frozen();
                break;
            case States.Passive:
                Passive();
                break;
            case States.Jump:
                Jump();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        _animator.SetFloat(Vel, Mathf.Abs(velocity.x));
    }

    private void ScaleStats()
    {
        switch (LevelBuilder.Instance.currentFloor)
        {
            case LevelBuilder.LevelMode.Floor2:
                maxHealth = (int)(maxHealth * floor2Multiplier);
                poise = (int)(poise * floor2Multiplier);
                poisonResistance = (int)(poisonResistance * floor2Multiplier);
                attack = (int)(attack * floor2Multiplier);
                knockbackPower = new Vector3(knockbackPower.x * floor2Multiplier, knockbackPower.y * floor2Multiplier, 0);
                break;
            case LevelBuilder.LevelMode.Floor3:
                maxHealth = (int)(maxHealth * floor3Multiplier);
                poise = (int)(poise * floor3Multiplier);
                poisonResistance = (int)(poisonResistance * floor3Multiplier);
                attack = (int)(attack * floor3Multiplier);
                knockbackPower = new Vector3(knockbackPower.x * floor3Multiplier, knockbackPower.y * floor3Multiplier, 0);
                break;
            case LevelBuilder.LevelMode.Floor4:
                maxHealth = (int)(maxHealth * floor4Multiplier);
                poise = (int)(poise * floor4Multiplier);
                poisonResistance = (int)(poisonResistance * floor4Multiplier);
                attack = (int)(attack * floor4Multiplier);
                knockbackPower = new Vector3(knockbackPower.x * floor4Multiplier, knockbackPower.y * floor4Multiplier, 0);
                break;
        }
    }

    private void Jump()
    {
        var targetPos = new Vector3(_target.position.x, _target.position.y, transform.position.z);
        
        var platform = FindPlatform();
        if (platform != null)
        {
            var verticalDistance = Mathf.Abs(platform.transform.position.y - transform.position.y);
    
            if (verticalDistance > maxJumpHeight)
            {
                _state = States.Chase;
                return;
            }

            StartCoroutine(DisableCollision(platform));
        }

        TriggerJump(targetPos);
        _state = States.Chase;
    }
    
    private GameObject FindPlatform()
    {
        var layerMask = LayerMask.GetMask("Ground");
        
        if (!Physics.Raycast(transform.position, Vector3.up, out var hit, 10f, layerMask)) return null;
        
        var platform = hit.collider.GetComponentInParent<SemiSolidPlatform>();
        
        return platform != null ? platform.gameObject : null;
    }
    
    private IEnumerator DisableCollision(GameObject platform)
    {
        if (_enemyCollider == null || platform == null) yield break;

        var verticalDistance = Mathf.Abs(platform.transform.position.y - transform.position.y);
        var est = verticalDistance * 0.125f; // estimate time to disable collision based on how far the platform is

        foreach (var col in platform.GetComponentsInChildren<Collider>())
        {
            Physics.IgnoreCollision(_enemyCollider, col, true);
        }

        yield return new WaitForSeconds(est);

        foreach (var col in platform.GetComponentsInChildren<Collider>())
        {
            Physics.IgnoreCollision(_enemyCollider, col, false);
        }
    }
    
    private void TriggerJump(Vector3 target)
    {
        _aiPath.canMove = false;
        _rigidbody.useGravity = true;

        var force = CalculateForce(transform.position, target, 7f);

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
        //_aiPath.SearchPath();
    }
    
    private bool IsGrounded()
    {
        return Physics.Raycast(transform.position, Vector3.down, 3f, LayerMask.GetMask("Ground"));
    }
    
    private Vector3 CalculateForce(Vector3 start, Vector3 end, float jumpHeight)
    {
        var direction = end - start;
        var gravity = Mathf.Abs(Physics.gravity.y);
        var horizontalDir = new Vector3(direction.x, 0f, direction.z);
        var yDiff = end.y - start.y;

        if (jumpHeight < yDiff)
        {
            jumpHeight = Mathf.Clamp(jumpHeight, yDiff + 0.5f, 4f); 
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

    private void Repulsion()
    {
        var nearby = Physics.OverlapSphere(transform.position, 1.5f, LayerMask.GetMask("Enemy"));
        foreach (var col in nearby)
        {
            if (col.gameObject == gameObject) continue;
            var dir = (transform.position - col.transform.position).normalized;
            _rigidbody.AddForce(dir * .2f, ForceMode.VelocityChange);
        }
    }
    
    private bool IsPlayerInRoom()
    {
        return _roomBounds != null && _roomBounds.bounds.Contains(_target.position);
    }

    private void UpdateSpriteDirection(bool isLeft)
    {
        var localScale = _spriteRenderer.transform.localScale;
        
        if (!isLeft)
        {
            localScale = new Vector3(Mathf.Abs(localScale.x), localScale.y, localScale.z);
        }
        else
        {
            localScale = new Vector3(-Mathf.Abs(localScale.x), localScale.y, localScale.z);
        }
        
        _spriteRenderer.transform.localScale = localScale;
        attackHitbox.transform.localScale = localScale;
    }

    private void Patrol()
    {
        CheckIfStuck();
        if (_isStuck) return;
        if (_aiPath.reachedDestination)
        {
            _patrolTarget = _patrolTarget == _patrolPoint1 ? _patrolPoint2 : _patrolPoint1;

            if (transform.position.y - _target.position.y > 0.5f)
            {
                _aiPath.destination = new Vector3(_target.position.x, _target.position.y, transform.position.z);
            }
            else
            {
                _aiPath.destination = new Vector3(_target.position.x, transform.position.y, transform.position.z);
            }
        }
        _aiPath.canMove = true;
        if (!alwaysShowHealth)
        {
            _healthSlider.gameObject.SetActive(false);
        }
    }
    
   private void PickPatrolPoints()
    {
        var randomOffset = Random.Range(4f, patrolRange);
        _patrolPoint1 = transform.position + new Vector3(randomOffset, 0, 0);
        _patrolPoint2 = transform.position + new Vector3(-randomOffset, 0, 0);
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
        
        if (!(_timeSinceLastMove > maxTimeToReachTarget)) return;
        _isStuck = true;
        PickPatrolPoints();
    }

    private void Chase()
    {
        if ((isStalker && _isBlocking) || (isBomb && _animator.GetBool(IsExplode)))
        {
            _aiPath.canMove = false;
        }
        else
        {
            _aiPath.canMove = true;
            if (transform.position.y - _target.position.y > 0.1f)
            {
                _aiPath.destination = new Vector3(_target.position.x, _target.position.y, transform.position.z);
            }
            else
            {
                _aiPath.destination = new Vector3(_target.position.x, transform.position.y, transform.position.z);
            }

            if (!isInvisible)
            {
                _healthSlider.gameObject.SetActive(true);
            }

            if (isBomb && _canLunge) StartCoroutine(BeginLunge());
        }
    }

    private IEnumerator BeginLunge()
    {
        _canLunge = false;
        var chance = Random.Range(0f, 1f);
        if (chance > 0.8f)
        {
            Lunge();
        }

        yield return new WaitForSecondsRealtime(lungeCooldown);
        _canLunge = true;
    }

    private void Lunge()
    {
        _aiPath.canMove = false;
        _healthSlider.gameObject.SetActive(true);

        _rigidbody.velocity = new Vector3(_playerDir.x * lungeForce, 0f, 0f);
    }

    private void Passive()
    {
        _aiPath.canMove = true;
        _aiPath.destination = new Vector3(passiveTarget.position.x, transform.position.y, passiveTarget.position.z);
        _healthSlider.gameObject.SetActive(true);
    }

    private void Attack()
    {
        _aiPath.canMove = false;
        _healthSlider.gameObject.SetActive(true);
        _targetTime -= Time.deltaTime;

        if (!(_targetTime <= 0.0f)) return;
        if (_isBlocking) return;
        
        if (isBomb)
        {
            UpdateSpriteDirection(_playerDir.x < 0);
            StartCoroutine(BeginExplode());
        }
        else
        {
            var i = Random.Range(0, numberOfAttacks);

            switch (i)
            {
                case 0:
                    _wasLastAttackBlock = false;
                    UpdateSpriteDirection(_playerDir.x < 0);
                    defense = 0;
                    _animator.SetTrigger(Attack1);
                    break;
                case 1:
                    if (isStalker && canBlock)
                    {
                        if (!_wasLastAttackBlock)
                        {
                            UpdateSpriteDirection(_playerDir.x < 0);
                        }

                        StartCoroutine(Block());
                        _wasLastAttackBlock = true;
                    }
                    else
                    {
                        UpdateSpriteDirection(_playerDir.x < 0);
                        _animator.SetTrigger(Attack2);
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
        _targetTime = attackCooldown;
    }

    private IEnumerator Block()
    {
        _isBlocking = true;
        _animator.SetBool(IsBlocking, true);
        yield return new WaitForSecondsRealtime(blockDuration);
        _animator.SetBool(IsBlocking, false);
        _isBlocking = false;
    }

    private IEnumerator BeginExplode()
    {
        StopAlarmSound();
        _animator.SetBool(IsExplode, true);
        var newCooldown = attackCooldown + Random.Range(-bombTimeVariability, bombTimeVariability);
        newCooldown = Mathf.Clamp(newCooldown, 0.1f, attackCooldown + bombTimeVariability);
        yield return new WaitForSecondsRealtime(newCooldown);
        _animator.SetBool(IsExplode, false);
        _animator.SetTrigger(Detonate);
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
       //_agent.velocity = Vector3.zero;
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

    public void TakeDamage(int damage, int? poiseDmg, Vector3? knockback)
    {
        defense = Mathf.Clamp(defense, 0, 100);
        var dmgReduction = (100 - defense) / 100f;
        damage = Mathf.RoundToInt(damage * dmgReduction);
        
        StartCoroutine(HitFlash());
        
        if (Random.Range(0, 10) < 1) // 10 percent chance on hit for enemy to drop energy
        {
            Instantiate(Resources.Load<GameObject>("ItemPrefabs/Other/Energy Prefab"), transform.position, Quaternion.identity);
        }
        
        if (_health - damage > 0)
        {
            _health -= damage;
            _healthSlider.value = _health;
        }
        else
        {
            _health = 0;
            _healthSlider.value = 0;
            if (isBomb)
            {
                StartCoroutine(BeginExplode());
            }
            else
            {
                if (knockback.HasValue)
                {
                    ApplyKnockback(knockback.Value);
                }
                Die();
            }
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
    
    private void TriggerExplodeVFX()
    {
        var newExplosion = Instantiate(explosionVFX, transform.position, Quaternion.identity);
        var handler = newExplosion.gameObject.GetComponent<ExplosionHandler>();
        handler.StartCoroutine(handler.Detonate(0f, 4.5f, true));
    }

    private void Die()
    {
        isDead = true;
        StopAllCoroutines();
        _characterAttack.ChanceHeal();
        
        var newGibs = Instantiate(gibs, transform.position, Quaternion.identity);

        foreach (var gib in newGibs.GetComponentsInChildren<Rigidbody>())
        {
            if (isBomb)
            {
                var dir = Random.onUnitSphere;
                dir.z = 0;
                gib.AddForce(dir.normalized * Random.Range(50f, 80f), ForceMode.Impulse);
                gib.AddTorque(Random.insideUnitSphere * 20f, ForceMode.Impulse);
            }
            else
            {
                var dir = new Vector3(_knockbackForce.x, _knockbackForce.y, 0);
                gib.AddForce(dir * (_knockbackForce.magnitude * Random.Range(0.1f, .15f)), ForceMode.Impulse);
            }
        }

        if (!SceneManager.GetActiveScene().name.Contains("Tutorial") && !SceneManager.GetActiveScene().name.Contains("Intermission"))
        {
            EnemySpawner.spawnedEnemy = null;
            EnemySpawner.SpawnEnemies();
        }
        else if (SceneManager.GetActiveScene().name.Contains("Tutorial"))
        {
            _target.GetComponent<TutorialController>().EnemyDefeated();
        }

        if (!isBomb)
        {
            AudioManager.Instance.AttachInstanceToGameObject(_deathEvent, gameObject);
            _deathEvent.start();
            _deathEvent.release();
        }

        var currencyToDrop = Random.Range(0, 6);
        for (var i = 0; i < currencyToDrop; i++)
        {
            Instantiate(Resources.Load<GameObject>("ItemPrefabs/Other/Currency Prefab"), transform.position, Quaternion.identity);
        }
        
        var energyToDrop = Random.Range(0, 3);
        for (var i = 0; i < energyToDrop; i++)
        {
            Instantiate(Resources.Load<GameObject>("ItemPrefabs/Other/Energy Prefab"), transform.position, Quaternion.identity);
        }
        
        StopAlarmSound();
        gameObject.SetActive(false);
    }

    public void PlayAlarmSound()
    {
        _alarmEvent.start();
    }

    public void StopAlarmSound()
    {
        _alarmEvent.stop(STOP_MODE.IMMEDIATE);
        _alarmEvent.release();
    }

    private void SetDefense(int amount)
    {
        defense = amount;
    }

    private void OnDisable()
    {
        if (!SceneManager.GetActiveScene().name.Contains("Tutorial") && !SceneManager.GetActiveScene().name.Contains("Intermission"))
        {
            RoomScripting.enemies.Remove(gameObject);
            RoomScripting._enemyCount--;
            EnemySpawner.spawnedEnemies.Remove(gameObject);
        }
    }

    public void ApplyKnockback(Vector2 knockbackPower) // TODO: make knockback in player facing direction
    {
        if (_isFrozen || isDead) return;

        _knockbackDir = transform.position.x > _target.position.x ? 1 : -1;
        var knockbackMultiplier = (_poiseBuildup >= poise) ? 15f : 10f; 
        _knockbackForce = new Vector3(knockbackPower.x * _knockbackDir * knockbackMultiplier, knockbackPower.y * knockbackMultiplier, 0);

        StartCoroutine(TriggerKnockback(_knockbackForce, 0.35f));
        
        var facingRight = _spriteRenderer.transform.localScale.x > 0;
        var playerInFront = (facingRight && _playerDir.x > 0) || (!facingRight && _playerDir.x < 0);

        if (isStalker)
        {
            if (!_isBlocking || !playerInFront)
            {
                if (!isPassive)
                {
                    defense = 0;
                }

                StartCoroutine(StunTimer(.05f));
            }
        }
        else
        {
            StartCoroutine(StunTimer(.05f));
        }

        StartCoroutine(WallHitCheck(3f));

        if (_poiseBuildup < poise) return;
        
        if (isStalker)
        {
            _isBlocking = false;
            _animator.SetBool(IsBlocking, false);
        }
            
        if (!isBomb)
        {
            StartCoroutine(StunTimer(1f));
        }

        _poiseBuildup = 0;
    }
    
    private IEnumerator TriggerKnockback(Vector3 force, float duration)
    {
        _aiPath.canMove = false;
        _rigidbody.velocity = Vector3.zero;
        _rigidbody.AddForce(force, ForceMode.Impulse);
        yield return new WaitForSeconds(duration);
        _rigidbody.velocity = Vector3.zero;
        _aiPath.canMove = true;
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
                   //Debug.Log("rebound: " + hit.collider.name);
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
        _animator.SetBool(IsStaggered, true);
        _aiPath.canMove = false;
       yield return new WaitForSecondsRealtime(stunTime);
       _rigidbody.velocity = Vector3.zero;
       _animator.SetBool(IsStaggered, false);
       _aiPath.canMove = true;
    }

    private IEnumerator HitFlash()
    {
        _spriteRenderer.material = hitMaterial;
        yield return new WaitForSeconds(0.1f);
        _spriteRenderer.material = defaultMaterial;
    }
}