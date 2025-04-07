using System;
using System.Collections;
using FMOD.Studio;
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
    [SerializeField] private int defense;
    [SerializeField] private int poisonResistance;
    [SerializeField] private int blockingDefense;

    [Header("Offensive Stats")] 
    [SerializeField] private int attack;
    [SerializeField] private int poiseDamage;
    [SerializeField] private int numberOfAttacks;

    [Header("Tracking")] 
    [SerializeField] private float attackRange;
    [SerializeField] private float patrolRange;
    private int _knockbackDir;
    private Vector3 _lastPosition;
    [SerializeField] private Collider _roomBounds;
    private Vector3 _playerDir;
    private Vector3 _patrolTarget, _patrolPoint1, _patrolPoint2;
    private enum States { Idle, Patrol, Chase, Attack, Frozen, Passive }
    private States _state = States.Idle;

    [Header("Timing")]
    [SerializeField] private float attackCooldown; // time between attacks
    [SerializeField] private float freezeDuration; // how long the enemy is frozen for
    [SerializeField] private float freezeCooldown; // how long until the enemy can be frozen again
    [SerializeField] private float maxTimeToReachTarget; // how long will the enemy try to get to the target before switching
    [SerializeField] private float bombTimeVariability;
    [SerializeField] private float lungeCooldown;
    [SerializeField] private float blockDuration;
    private float _timeSinceLastMove;
    private float _targetTime;

    [Header("Enemy Properties")] 
    public bool isBomb;
    [SerializeField] private bool isStalker;
    [SerializeField] private bool canBeFrozen;
    [SerializeField] private bool canBeStunned;
    [SerializeField] private bool doesEnemyPatrol;
    [SerializeField] private bool isPassive;
    [SerializeField] private float lungeForce;
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
    private Slider _healthSlider;
    private Animator _animator;
    private Transform _target;
    private NavMeshAgent _agent;
    private CharacterAttack _characterAttack;
    private SpriteRenderer _spriteRenderer;
    private MaterialPropertyBlock _propertyBlock;
    
    [Header("Sound")]
    private EventInstance _alarmEvent;
    private EventInstance _deathEvent;
    
    private static readonly int BaseColor = Shader.PropertyToID("_BaseColor");
    private static readonly int IsExplode = Animator.StringToHash("isExplode");
    private static readonly int Vel = Animator.StringToHash("vel");
    private static readonly int Attack1 = Animator.StringToHash("Attack");
    private static readonly int Attack2 = Animator.StringToHash("Attack2");
    private static readonly int IsBlocking = Animator.StringToHash("isBlocking");
    private static readonly int Detonate = Animator.StringToHash("Detonate");
    private static readonly int IsStaggered = Animator.StringToHash("isStaggered");

    int IDamageable.Attack { get => attack; set => attack = value; }
    int IDamageable.Poise { get => poise; set => poise = value; }
    int IDamageable.PoiseDamage { get => poiseDamage; set => poiseDamage = value; }
    public bool isPlayerInRange { get; set; }
    public bool isDead { get; set; }
    public RoomScripting RoomScripting { get; set; }
    public EnemySpawner EnemySpawner { get; set; }
    
    private void Start()
    {
        if (LevelBuilder.Instance.currentFloor != LevelBuilder.LevelMode.Tutorial || LevelBuilder.Instance.currentFloor != LevelBuilder.LevelMode.Intermission)
        {
            RoomScripting = gameObject.transform.root.GetComponent<RoomScripting>();
            RoomScripting.enemies.Add(gameObject);
            _roomBounds = RoomScripting.GetComponent<Collider>();
        }

        _healthSlider = GetComponentInChildren<Slider>();
        _healthSlider.maxValue = maxHealth;
        _healthSlider.value = maxHealth;
        _health = maxHealth;
        _healthSlider.gameObject.SetActive(false);
        _animator = GetComponent<Animator>();
        _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        
        _target = GameObject.FindGameObjectWithTag("Player").transform;
        _agent = GetComponent<NavMeshAgent>();
        _agent.updateRotation = false;
        _propertyBlock = new MaterialPropertyBlock();
        
        PickPatrolPoints();
        _patrolTarget = _patrolPoint1;
        
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
    }

    private void Update()
    {
        var velocity = _agent.velocity;
        _target = isPassive ? passiveTarget : _target;
        _playerDir = _target.position - transform.position;
        var distance = Vector3.Distance(transform.position, _target.position);

        if (isStalker)
        {
            if (_isBlocking)
            {
                var facingRight = _spriteRenderer.transform.localScale.x > 0;
                var playerInFront = (facingRight && _playerDir.x > 0) || (!facingRight && _playerDir.x < 0);

                defense = playerInFront ? blockingDefense : 0;
            }
            else if (_state != States.Attack)
            {
                defense = 0;
            }
        }

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
            _state = distance < attackRange ? States.Attack : States.Chase;
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

        switch (_state)
        {
            case States.Idle:
                _agent.isStopped = true;
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
                _agent.isStopped = true;
                Frozen();
                break;
            case States.Passive:
                Passive();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        _animator.SetFloat(Vel, Mathf.Abs(velocity.x));
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
        if (_agent.pathPending && !_isStuck) return;
        if (!(_agent.remainingDistance <= _agent.stoppingDistance) && !_isStuck) return;
        
        var newTarget = (_patrolTarget == _patrolPoint1) ? _patrolPoint2 : _patrolPoint1;
        
        _patrolTarget = newTarget;
        _agent.SetDestination(_patrolTarget);
        
        _healthSlider.gameObject.SetActive(false);
        _isStuck = false;
        _timeSinceLastMove = 0f;
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
        if ((isStalker && _isBlocking) || isBomb && _animator.GetBool(IsExplode))
        {
            _agent.isStopped = true;
        }
        else
        {
            _agent.isStopped = false;
            _healthSlider.gameObject.SetActive(true);

            if (isBomb && _canLunge)
            {
                StartCoroutine(BeginLunge());
            }

            _agent.SetDestination(_target.position);
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
        _agent.isStopped = false;
        _healthSlider.gameObject.SetActive(true);
        
        _agent.velocity = Vector3.zero;
        _agent.velocity = new Vector3(_playerDir.x * lungeForce, 0f, 0f);
    }

    private void Passive()
    {
        _agent.isStopped = false;
        _healthSlider.gameObject.SetActive(true);
        _agent.SetDestination(passiveTarget.position);
    }

    private void Attack()
    {
        _agent.ResetPath();
        _agent.isStopped = true;
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
                    if (isStalker)
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
        _agent.velocity = Vector3.zero;
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
        
        if (_health - damage > 0)
        {
            _health -= damage;
            _healthSlider.value = _health;
            StartCoroutine(HitFlash(Color.red, 0.1f));
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

    private void Die()
    {
        isDead = true;
        StopAllCoroutines();

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
            AudioManager.Instance.AttachInstanceToGameObject(_deathEvent, gameObject.transform);
            _deathEvent.start();
            _deathEvent.release();
        }

        var currencyToDrop = Random.Range(0, 6);
        for (var i = 0; i < currencyToDrop; i++)
        {
            Instantiate(Resources.Load<GameObject>("ItemPrefabs/Other/Currency Prefab"), transform.position, Quaternion.identity);
        }
        
        StopAlarmSound();
        gameObject.SetActive(false);

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
        
        var knockbackMultiplier = (_poiseBuildup >= poise) ? 2f : 0.5f; 
        var knockbackForce = new Vector3(knockbackPower.x * _knockbackDir * knockbackMultiplier, knockbackPower.y * knockbackMultiplier, 0);

        StartCoroutine(TriggerKnockback(knockbackForce, 0.2f));
        StartCoroutine(ApplyVerticalKnockback(knockbackPower.y, .2f));
        
        var facingRight = _spriteRenderer.transform.localScale.x > 0;
        var playerInFront = (facingRight && _playerDir.x > 0) || (!facingRight && _playerDir.x < 0);

        if (isStalker)
        {
            if (!_isBlocking || !playerInFront)
            {
                defense = 0;
                StartCoroutine(StunTimer(.1f));
            }
        }
        else
        {
            StartCoroutine(StunTimer(.1f));
        }

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
        var elapsedTime = 0f;
        var startPos = transform.position;
        var targetPos = startPos + force;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            var t = elapsedTime / duration;
            transform.position = Vector3.Lerp(startPos, targetPos, t);
            yield return null;
        }
    }
    
    private IEnumerator ApplyVerticalKnockback(float height, float dur)
    {
        var elapsedTime = 0f;
        var startOffset = _agent.baseOffset;

        while (elapsedTime < dur)
        {
            elapsedTime += Time.deltaTime;
            _agent.baseOffset = startOffset + Mathf.Sin(elapsedTime / dur * Mathf.PI) * height;
            yield return null;
        }

        _agent.baseOffset = startOffset;
    }

    private IEnumerator StunTimer(float stunTime)
    {
        _animator.SetBool(IsStaggered, true);
       yield return new WaitForSecondsRealtime(stunTime);
       _agent.velocity = Vector3.zero;
       _animator.SetBool(IsStaggered, false);
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