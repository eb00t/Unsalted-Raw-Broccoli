using System;
using System.Collections;
using DG.Tweening;
using FMOD.Studio;
using Pathfinding;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = UnityEngine.Random;


// Adapted from https://github.com/Chaker-Gamra/2.5D-Platformer-Game/blob/master/Assets/Scripts/Enemy/Enemy.cs

public class CloneBossHandler : MonoBehaviour, IDamageable
{
    [Header("Defensive Stats")] 
    public int maxHealth;
    public int health;
    [SerializeField] private int poise;
    [SerializeField] private int defense;
    [SerializeField] private int poisonResistance;

    [Header("Offensive Stats")] 
    [SerializeField] private int attack;
    [SerializeField] private int poiseDamage;
    [SerializeField] private int numberOfAttacks;
    [SerializeField] private Vector3 knockbackPower;

    [Header("Tracking")] 
    [SerializeField] private float attackRange;
    private int _knockbackDir;
    private Vector3 _lastPosition;
    [SerializeField] private Collider _roomBounds;
    private Vector3 _playerDir;
    private enum States { Idle, Chase, Attack, Frozen, Jump }
    private States _state = States.Idle;

    [Header("Timing")]
    [SerializeField] private float attackCooldown; // time between attacks
    [SerializeField] private float freezeDuration; // how long the enemy is frozen for
    [SerializeField] private float freezeCooldown; // how long until the enemy can be frozen again
    [SerializeField] private float maxTimeToReachTarget; // how long will the enemy try to get to the target before switching
    [SerializeField] private float jumpCooldown;
    [SerializeField] private float jumpThroughTime;
    private float _timeSinceLastMove;
    private float _targetTime;
    private float _jumpTimer;
    
    [Header("Enemy Properties")] 
    [SerializeField] private bool canBeFrozen;
    [SerializeField] private bool canBeStunned;
    [SerializeField] private float jumpTriggerDistance;
    [SerializeField] private float maxJumpHeight;
    private Vector3 _knockbackForce;
    private bool _isFrozen;
    private bool _isPoisoned;
    private bool _lowHealth;
    private bool _isStuck;
    private int _poisonBuildup;
    private int _poiseBuildup;
    public bool _hasDialogueTriggered;

    [Header("References")] 
    [SerializeField] private BoxCollider attackHitbox;
    [SerializeField] private Image healthFillImage;
    [SerializeField] private Material defaultMaterial, hitMaterial;
    [SerializeField] private GameObject gibs;
    private Slider _healthSlider;
    private Animator _animator;
    private Transform _target;
    private NavMeshAgent _agent;
    private CharacterAttack _characterAttack;
    private SpriteRenderer _spriteRenderer;
    private AIPath _aiPath;
    private Rigidbody _rigidbody;
    public CloneBossManager cloneBossManager;
    public GameObject dialogue;
    private DialogueTrigger[] _dialogueTriggers;
    [SerializeField] private Slider healthChangeSlider;
    [SerializeField] private Image healthChangeImage;
    private Tween _healthTween;
    
    [Header("Sound")]
    private EventInstance _alarmEvent;
    private EventInstance _deathEvent;
    
    private static readonly int Vel = Animator.StringToHash("vel");
    private static readonly int Attack1 = Animator.StringToHash("Attack");
    private static readonly int Attack2 = Animator.StringToHash("Attack2");
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
        if (LevelBuilder.Instance.currentFloor != LevelBuilder.LevelMode.Tutorial &&
            LevelBuilder.Instance.currentFloor != LevelBuilder.LevelMode.Intermission)
        {
            RoomScripting = gameObject.transform.root.GetComponent<RoomScripting>();
            //RoomScripting.enemies.Add(gameObject);
            _roomBounds = RoomScripting.GetComponent<Collider>();
        }

        _healthSlider = GetComponentInChildren<Slider>();
        _healthSlider.maxValue = maxHealth;
        _healthSlider.value = maxHealth;
        health = maxHealth;
        healthChangeSlider.maxValue = maxHealth;
        healthChangeSlider.value = health;
        _healthSlider.gameObject.SetActive(false);
        _animator = GetComponent<Animator>();
        _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        _aiPath = GetComponent<AIPath>();
        _rigidbody = GetComponent<Rigidbody>();
        _enemyCollider = GetComponent<CapsuleCollider>();
        _target = GameObject.FindGameObjectWithTag("Player").transform;
        _jumpTimer = jumpCooldown;
        
        gameObject.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
        _deathEvent = AudioManager.Instance.CreateEventInstance(FMODEvents.Instance.EnemyDeath);
        _alarmEvent = AudioManager.Instance.CreateEventInstance(FMODEvents.Instance.EnemyLowHealthAlarm);
        AudioManager.Instance.AttachInstanceToGameObject(_alarmEvent, gameObject);
        _dialogueTriggers = gameObject.transform.root.GetComponentsInChildren<DialogueTrigger>();
    }

    private void Update()
    {
        if (!_hasDialogueTriggered)
        {
            foreach (var trigger in _dialogueTriggers)
            {
                if (trigger.triggered && dialogue.activeSelf)
                {
                    _hasDialogueTriggered = true;
                    break;
                }
            }
        }
        
        if (!_hasDialogueTriggered || (dialogue != null && dialogue.activeSelf)) return;
        
        var velocity = _aiPath.velocity;
        _playerDir = _target.position - transform.position;
        var distance = Vector3.Distance(transform.position, _target.position);
        var heightDiffAbove = _target.position.y - transform.position.y;
        
        Repulsion();

        if (_isFrozen)
        {
            _state = States.Frozen;
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
                }
                else
                {
                    _state = States.Chase;
                }
            }
        }
        else
        {
            _state = States.Idle;
        }

        if (_state == States.Chase)
        {
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
                _aiPath.canMove = false;
                _healthSlider.gameObject.SetActive(false);
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
            case States.Jump:
                Jump();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        _animator.SetFloat(Vel, Mathf.Abs(velocity.x));
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
    
    private void Chase()
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
        _healthSlider.gameObject.SetActive(true);
    }

    private void Attack()
    {
        _aiPath.canMove = false;
        _healthSlider.gameObject.SetActive(true);
        _targetTime -= Time.deltaTime;

        if (!(_targetTime <= 0.0f)) return;
        
        var i = Random.Range(0, numberOfAttacks);

        switch (i)
        {
            case 0:
                UpdateSpriteDirection(_playerDir.x < 0);
                defense = 0;
                _animator.SetTrigger(Attack1);
                break;
            case 1:
                UpdateSpriteDirection(_playerDir.x < 0);
                _animator.SetTrigger(Attack2);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        _targetTime = attackCooldown;
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

    public void TakeDamage(int damage, int? poiseDmg, Vector3? knockback)
    {
        if (!_hasDialogueTriggered) return;

        var previousHealth = health;
        defense = Mathf.Clamp(defense, 0, 100);
        var dmgReduction = (100 - defense) / 100f;
        damage = Mathf.RoundToInt(damage * dmgReduction);
        StartCoroutine(HitFlash());
        
        if (Random.Range(0, 10) < 1) // 10 percent chance on hit for enemy to drop energy
        {
            Instantiate(Resources.Load<GameObject>("ItemPrefabs/Other/Energy Prefab"), transform.position, Quaternion.identity);
        }
        
        if (health - damage > 0)
        {
            health -= damage;
            _healthSlider.value = health;
        }
        else
        {
            health = 0;
            _healthSlider.value = 0;
            if (knockback.HasValue)
            {
                ApplyKnockback(knockback.Value);
            }
            Die();
        }

        if (health <= maxHealth / 3 && _lowHealth == false)
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
        
        var isDamaged = health < previousHealth;
        var changeColor = isDamaged ? new Color(1f, 0.9f, 0.4f) : new Color(.5f, 1f, 0.4f);
        
        _healthTween?.Kill();
        healthChangeImage.color = changeColor;

        if (isDamaged)
        {
            _healthSlider.value = health;
            _healthTween = DOVirtual.Float(healthChangeSlider.value, health, 1f, v => healthChangeSlider.value = v).SetEase(Ease.OutExpo).SetDelay(0.2f);
        }
        else
        {
            healthChangeSlider.value = health;
            _healthTween = DOVirtual.Float(_healthSlider.value, health, 1f, v => _healthSlider.value = v).SetEase(Ease.OutExpo).SetDelay(0.2f);
        }
        
        //cloneBossManager.UpdateCollectiveHealth();
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

    public void Die()
    {
        isDead = true;
        StopAllCoroutines();
        _deathEvent.start();
        var newGibs = Instantiate(gibs, transform.position, Quaternion.identity);

        foreach (var gib in newGibs.GetComponentsInChildren<Rigidbody>())
        {
            gib.AddForce(knockbackPower, ForceMode.Impulse);
        }

        
        var energyToDrop = Random.Range(0, 2);
        for (var i = 0; i < energyToDrop; i++)
        {
            Instantiate(Resources.Load<GameObject>("ItemPrefabs/Other/Energy Prefab"), transform.position, Quaternion.identity);
        }

        cloneBossManager.numKilled++;
        cloneBossManager.cloneBossHandlers.Remove(this);
        cloneBossManager.UpdateCollectiveHealth();
        
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

    public void ApplyKnockback(Vector2 knockbackPower) // TODO: make knockback in player facing direction
    {
        if (_isFrozen || isDead) return;

        _knockbackDir = transform.position.x > _target.position.x ? 1 : -1;
        
        var knockbackMultiplier = (_poiseBuildup >= poise) ? 15f : 10f; 
        _knockbackForce = new Vector3(knockbackPower.x * _knockbackDir * knockbackMultiplier, knockbackPower.y * knockbackMultiplier, 0);

        StartCoroutine(TriggerKnockback(_knockbackForce, 0.2f));
        StartCoroutine(StunTimer(.05f));

        if (_poiseBuildup < poise) return;
        StartCoroutine(StunTimer(1f));
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
        //_aiPath.SearchPath();
    }

    private IEnumerator StunTimer(float stunTime)
    {
        _animator.SetBool(IsStaggered, true);
       yield return new WaitForSecondsRealtime(stunTime);
       _rigidbody.velocity = Vector3.zero;
       _animator.SetBool(IsStaggered, false);
    }
    
    private IEnumerator HitFlash()
    {
        _spriteRenderer.material = hitMaterial;
        yield return new WaitForSeconds(0.1f);
        _spriteRenderer.material = defaultMaterial;
    }
}