using System;
using System.Collections;
using Cinemachine;
using FMOD;
using FMOD.Studio;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.TextCore.Text;
using Random = UnityEngine.Random;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;

public class Boss2Hands : MonoBehaviour, IDamageable
{
    [Header("Defensive Stats")] 
    [SerializeField] private int maxHealth;
    private int _health;
    [SerializeField] private int defense;
    [SerializeField] private int poisonResistance;

    [Header("Offensive Stats")] 
    [SerializeField] private int attack;
    [SerializeField] private int poiseDamage;
    [SerializeField] private Vector3 knockbackPower;
    [SerializeField] private float hardcoreMultiplier;

    [Header("Tracking")] 
    private Vector3 _leftHandInitialPos, _rightHandInitialPos;
    private int _knockbackDir;
    private Vector3 _lastPosition;
    private bool _isPlayerInRange;
    private States _state;
    private enum States { Idle, Attack, Frozen }
    
    [Header("Timing")]
    [SerializeField] private float attackCooldown; // time between attacks
    [SerializeField] private float freezeDuration; // how long the enemy is frozen for
    [SerializeField] private float freezeCooldown; // how long until the enemy can be frozen again
    [SerializeField] private float handLerpSpeed;
    [SerializeField] private float handHoverHeight;
    private float _attackCdCounter;
    private float _timeSinceLastMove;

    [Header("Enemy Properties")] 
    [SerializeField] private string bossName;
    [SerializeField] private bool canBeFrozen;
    [SerializeField] private bool canBeStunned;
    [SerializeField] private bool isIdle;
    private Color _healthDefault;
    private Vector3 _impulseVector;
    private bool _isPoisoned;
    private bool _lowHealth;
    private bool _isStuck;
    private int _poisonBuildup;
    private int _poiseBuildup;
    private bool _canAttack;
    private bool _isFrozen;
    private bool _isFreezing;
    private bool _isIdling;

    [Header("References")] 
    [SerializeField] private Image healthFillImage;
    [SerializeField] private TextMeshProUGUI bossNameTxt;
    [SerializeField] private Material defaultMaterial, hitMaterial;
    [SerializeField] private Slider healthSlider;
    [SerializeField] private GameObject platform;
    [SerializeField] private DataHolder dataHolder;
    private Animator _animator;
    private Transform _target;
    private RoomScripting _roomScripting;
    private CharacterAttack _characterAttack;
    private SpriteRenderer _spriteRenderer;
    private LineRenderer _lineRenderer;
    private CinemachineImpulseSource _impulseSource;
    [SerializeField] private Transform leftHand, rightHand, groundPosition, bossEyePosition;
    [SerializeField] private GameObject lhColliderDown, rhColliderDown, lhColliderUp, rhColliderUp;
    [SerializeField] private GameObject handDownL, handDownR, handUpL, handUpR;
    [SerializeField] private TextMeshProUGUI bossTitle;
    private SettingManager _settingManager;
    private LockOnController _lockOnController;
    private DialogueTrigger[] _dialogueTriggers;
    private GameObject dialogueGui;
    [SerializeField] private Transform clapLeftTarget, clapRightTarget, downTarget;
    [SerializeField] private Transform downVFX, upVFXLeft, upVFXRight;
    
    [Header("Sound")]
    private EventInstance _armMovementL, _armMovementR;
    private EventInstance _laserEvent;
    private bool _soundLStarted, _soundRStarted;
    
    public int Poise { get; set; }
    bool IDamageable.isPlayerInRange { get => _isPlayerInRange; set => _isPlayerInRange = value; }
    Vector3 IDamageable.KnockbackPower { get => knockbackPower; set => knockbackPower = value; }
    public bool isDead { get; set; }
    public RoomScripting RoomScripting { get; set; }
    public EnemySpawner EnemySpawner { get; set; }
    int IDamageable.Attack { get => attack; set => attack = value; }
    int IDamageable.PoiseDamage { get => poiseDamage; set => poiseDamage = value; }

    private void Start()
    {
        _roomScripting = gameObject.transform.root.GetComponent<RoomScripting>();
        _roomScripting.enemies.Add(gameObject);
        _impulseSource = GetComponent<CinemachineImpulseSource>();
        _dialogueTriggers = gameObject.transform.root.GetComponentsInChildren<DialogueTrigger>();
        _settingManager = GameObject.Find("Settings").GetComponent<SettingManager>();
        healthSlider = GetComponentInChildren<Slider>();
        healthSlider.maxValue = maxHealth;
        healthSlider.value = maxHealth;
        _target = GameObject.FindGameObjectWithTag("Player").transform;
        _characterAttack = _target.GetComponentInChildren<CharacterAttack>();
        dialogueGui = GameObject.FindGameObjectWithTag("UIManager").GetComponent<MenuHandler>().dialogueGUI;
        _health = maxHealth;
        _leftHandInitialPos = leftHand.position;
        _rightHandInitialPos = rightHand.position;
        _canAttack = true;
        bossTitle.text = bossName;
        _lineRenderer = GetComponentInChildren<LineRenderer>();
        _lockOnController = _target.GetComponent<LockOnController>();
        _armMovementL = AudioManager.Instance.CreateEventInstance(FMODEvents.Instance.BossHandMove);
        _armMovementR = AudioManager.Instance.CreateEventInstance(FMODEvents.Instance.BossHandMove);
        _healthDefault = healthFillImage.color;
        
        if (dataHolder.hardcoreMode)
        {
            maxHealth = (int)(maxHealth * hardcoreMultiplier);
            poisonResistance = (int)(poisonResistance * hardcoreMultiplier);
            attack = (int)(attack * hardcoreMultiplier);
            attackCooldown /= hardcoreMultiplier;
            knockbackPower = new Vector3(knockbackPower.x * hardcoreMultiplier, knockbackPower.y * hardcoreMultiplier, 0);
        }
    }

    private void Update()
    {
        foreach (var trigger in _dialogueTriggers)
        {
            if (trigger.triggered)
            {
                break;
            }
            
            healthSlider.gameObject.SetActive(false);
            return;
        }

        if (dialogueGui.activeSelf)
        {
            StopAllCoroutines();
            _attackCdCounter = 0;
            _canAttack = true;
            return;
        }

        _attackCdCounter -= Time.deltaTime;

        if (_isFrozen)
        {
            _state = States.Frozen;
        }
        else
        {
            if (_isPlayerInRange)
            {
                _state = States.Attack;
                _lockOnController.isNearBoss = true;
            }
            else
            {
                healthSlider.gameObject.SetActive(false);
                _lockOnController.isNearBoss = false;
                if (_state == States.Idle) return;
                {
                    _state = States.Idle;
                }
            }
        }

        switch (_state)
        {
            case States.Idle:
                if (!_isIdling)
                {
                    StartCoroutine(Idle());
                }

                break;
            case States.Attack:
                healthSlider.gameObject.SetActive(true);
                if (!_canAttack || _attackCdCounter > 0) return;
                StartCoroutine(Attack());
                break;
            case States.Frozen:
                if (!_isFreezing && canBeFrozen)
                {
                    StopAllCoroutines();
                    _canAttack = true;
                    _lineRenderer.enabled = false;
                    StartCoroutine(BeginFreeze());
                }
                break;
            default:
                throw new ArgumentOutOfRangeException();
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
        _isFrozen = false;
        _state = States.Attack;

        StartCoroutine(StartFreezeCooldown());
        _isFreezing = false;
    }
    
    private IEnumerator StartFreezeCooldown()
    {
        yield return new WaitForSeconds(freezeCooldown);
        canBeFrozen = true;
    }
    
    private IEnumerator Idle() // makes the hands hover up and down while idle
    {
        _isIdling = true;
        var hoverSpeed = 2f;
        var height = 0.2f;

        while (_state == States.Idle)
        {
            var hoverOffset = Mathf.Sin(Time.time * hoverSpeed) * height;
        
            leftHand.position = _leftHandInitialPos + new Vector3(0, hoverOffset, 0);
            rightHand.position = _rightHandInitialPos + new Vector3(0, hoverOffset, 0);

            yield return null;
        }
        
        _isIdling = false;
    }

    private IEnumerator Attack() // randomly pick an attack
    {
        _attackCdCounter = attackCooldown;
        attack = 50;
        
        var attackType = Random.Range(0, 5);

        switch (attackType)
        {
            case 0:
                yield return StartCoroutine(GroundPoundAndLunge());
                break;
            case 1:
                yield return StartCoroutine(OverheadSlam(true));
                break;
            case 2:
                yield return StartCoroutine(ClapAttack());
                break;
            case 3:
                yield return StartCoroutine(LaserAttack());
                break;
            case 4:
                yield return StartCoroutine(OverheadSlam(false));
                break;
        }
    }

    // just moves the hands back to their original position so that the player cant keep attacking for too long
    private IEnumerator ResetHands()
    {
        var resetDur = 2f;
        var elapsed = 0f;

        var leftStartPos = leftHand.position;
        var rightStartPos = rightHand.position;

        while (elapsed < resetDur)
        {
            leftHand.position = Vector3.Lerp(leftStartPos, _leftHandInitialPos, elapsed);
            rightHand.position = Vector3.Lerp(rightStartPos, _rightHandInitialPos, elapsed);
            
            UpdateHandImg(false, false, true, true);
            UpdateColliders(false, false, false, false);

            elapsed += Time.deltaTime * handLerpSpeed;
            yield return null;
        }
        
        TwoHandAttackSoundFinish(transform.position, false);

    }
    
    // hands slam to the ground and after a short duration the hand slide towards the player
    private IEnumerator GroundPoundAndLunge()
    {
        yield return StartCoroutine(ResetHands());
        attack = 20;
        _canAttack = false;
        
        UpdateColliders(true, true, false, false);
        UpdateHandImg(true, true, false, false);

        var leftTargetPos = new Vector3(leftHand.position.x, groundPosition.position.y, leftHand.position.z);
        var rightTargetPos = new Vector3(rightHand.position.x, groundPosition.position.y, rightHand.position.z);

        yield return StartCoroutine(MoveHands(leftTargetPos, rightTargetPos, 2f));
        _impulseVector = new Vector3(0, 1, 0);
        var target1 = new Vector3(leftTargetPos.x, downTarget.position.y, 0f);
        var target2 = new Vector3(rightTargetPos.x, downTarget.position.y, 0f);
        var lvfx = Instantiate(downVFX, target1, downVFX.rotation, transform);
        var rvfx = Instantiate(downVFX, target2, downVFX.rotation, transform);
        lvfx.gameObject.SetActive(true);
        rvfx.gameObject.SetActive(true);
        OneHandAttackSoundFinish(true, true);
        OneHandAttackSoundFinish(false, true);
        
        UpdateColliders(false, false, false, false); // give player opening to attack
        defense = 0;
        
        yield return new WaitForSecondsRealtime(1f);

        defense = 50;
        UpdateColliders(false, false, true, true);
        UpdateHandImg(false, false, true, true);
        
        var lungeTarget = new Vector3(groundPosition.position.x, groundPosition.position.y, groundPosition.position.z);
        
        yield return StartCoroutine(MoveHands(lungeTarget + Vector3.left, lungeTarget + Vector3.right, 1f));
        _impulseVector = new Vector3(0, 2, 0);
        var leftPos = new Vector3(clapLeftTarget.position.x + 0.25f, clapLeftTarget.position.y, clapLeftTarget.position.z);
        var vfxLeft = Instantiate(upVFXLeft, leftPos, upVFXLeft.rotation, transform);
        var vfxRight = Instantiate(upVFXRight, clapRightTarget.position, upVFXRight.rotation, transform);
        vfxLeft.gameObject.SetActive(true);
        vfxRight.gameObject.SetActive(true);
        TwoHandAttackSoundFinish(lungeTarget, true);

        yield return new WaitForSecondsRealtime(1f);
        
        UpdateColliders(false, false, false, false);
        
        _canAttack = true;
    }

    // hand hovers over player for short duration then slams down
    private IEnumerator OverheadSlam(bool isLeft)
    {
        yield return StartCoroutine(ResetHands());
        _canAttack = false;
        attack = 60;
        
        var hoverPosition = _target.position + Vector3.up * handHoverHeight;
        
        if (!isLeft)
        {
            _armMovementL = AudioManager.Instance.CreateEventInstance(FMODEvents.Instance.BossHandMove);
            UpdateHandImg(false, true, true, false);
            yield return StartCoroutine(MoveHands(null, hoverPosition, 1f));
            OneHandAttackSoundFinish(isLeft, false);
        }
        else
        {
            _armMovementR = AudioManager.Instance.CreateEventInstance(FMODEvents.Instance.BossHandMove);
            UpdateHandImg(true, false, false, true);
            yield return StartCoroutine(MoveHands(hoverPosition, null, 1f));
            OneHandAttackSoundFinish(isLeft, false);
            
        }
        
        yield return new WaitForSecondsRealtime(1f);
        
        UpdateColliders(true, true, false, false);
        
        var slamPosition = new Vector3(hoverPosition.x, groundPosition.position.y, hoverPosition.z);

        if (!isLeft) //  slam down
        {
            yield return StartCoroutine(MoveHands(null, slamPosition, 0.5f));
            var target = new Vector3(hoverPosition.x, downTarget.position.y, 0f);
            var vfx = Instantiate(downVFX, target, downVFX.rotation, transform);
            vfx.gameObject.SetActive(true);
            _impulseVector = new Vector3(0, 2, 0);
            defense = 0;
            OneHandAttackSoundFinish(isLeft, true);
            UpdateColliders(false, false, false, false);
            yield return new WaitForSecondsRealtime(1.5f);
            defense = 50;
            yield return StartCoroutine(MoveHands(null, hoverPosition, 0.5f));
            OneHandAttackSoundFinish(isLeft, false);
        }
        else
        {
            yield return StartCoroutine(MoveHands(slamPosition, null, 0.5f));
            var target = new Vector3(hoverPosition.x, downTarget.position.y, 0f);
            var vfx = Instantiate(downVFX, target, downVFX.rotation, transform);
            vfx.gameObject.SetActive(true);
            _impulseVector = new Vector3(0, 2, 0);
            UpdateColliders(false, false, false, false);
            defense = 0;
            OneHandAttackSoundFinish(isLeft, true);
            yield return new WaitForSecondsRealtime(1.5f);
            defense = 50;
            yield return StartCoroutine(MoveHands(hoverPosition, null, 0.5f));
            OneHandAttackSoundFinish(isLeft, false);
        }

        yield return new WaitForSecondsRealtime(0.1f);
        
        UpdateHandImg(false, false, true, true);
        UpdateColliders(false, false, false, false);
        
        _canAttack = true;
    }

    // both hands go beside the player and after a short duration the hands clap between 1 and 3 times
    private IEnumerator ClapAttack()
    {
        yield return StartCoroutine(ResetHands());
        attack = 20;
        _canAttack = false;
        
        UpdateHandImg(false, false, true, true);

        var leftWidePos = _target.position + Vector3.left * 5;
        var rightWidePos = _target.position + Vector3.right * 5;
        var clapCount = Random.Range(1, 4);
        var clapTargetLeft = _target.position + Vector3.left;
        var clapTargetRight = _target.position + Vector3.right;

        defense = 0;

        for (var i = 0; i <= clapCount; i++)
        {
            yield return StartCoroutine(MoveHands(leftWidePos, rightWidePos, 1f));
            yield return new WaitForSeconds(0.1f);
            TwoHandAttackSoundFinish(transform.position, false);
            UpdateColliders(false, false, true, true);
            yield return StartCoroutine(MoveHands(clapTargetLeft, clapTargetRight, .5f));
            var vfxLeft = Instantiate(upVFXLeft, clapLeftTarget.position, upVFXLeft.rotation, transform);
            var vfxRight = Instantiate(upVFXRight, clapRightTarget.position, upVFXRight.rotation, transform);
            vfxLeft.gameObject.SetActive(true);
            vfxRight.gameObject.SetActive(true);
            _impulseVector = new Vector3(4, 0, 0);
            TwoHandAttackSoundFinish(clapTargetLeft, true);
            
            yield return new WaitForSecondsRealtime(0.1f);

            UpdateColliders(false, false, false, false);
        }

        yield return new WaitForSecondsRealtime(1.25f);

        defense = 50;
        _canAttack = true;
    }
    
    private IEnumerator LaserAttack() // aims laser at player that tracks, then it stops and starts doing damage
    {
        yield return StartCoroutine(ResetHands());
        attack = 10;
        _laserEvent = AudioManager.Instance.CreateEventInstance(FMODEvents.Instance.BossHandLaser);
        _canAttack = false;
        _lineRenderer.enabled = true;
        _lineRenderer.SetPosition(0, bossEyePosition.position);
        var laserStartPos = _lineRenderer.GetPosition(0);;
        var chargeTime = 3f;
        var fireTime = 1f;
        var trackSpeed = 5f;
        var delay = 0.2f;
        var targetPos = _target.position;
        var elapsed = 0f;

        AudioManager.Instance.AttachInstanceToGameObject(_laserEvent, gameObject);
        _laserEvent.start();
        
        while (elapsed < chargeTime)
        {
            targetPos = Vector3.Lerp(targetPos, _target.position, Time.deltaTime * trackSpeed);

            _lineRenderer.SetPosition(0, laserStartPos); 
            _lineRenderer.SetPosition(1, targetPos);
            _lineRenderer.startWidth = 0.01f;
            _lineRenderer.endWidth = 0.01f;
        
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
            var dist = Vector3.Distance(targetPos, laserStartPos);
            var direction = (targetPos - laserStartPos).normalized;
            var laserEndPos = laserStartPos + direction * (dist + 10f);

            var hits = Physics.RaycastAll(laserStartPos, direction, 100f);
            foreach (var allHit in hits)
            {
                if (allHit.collider.isTrigger) continue;
                if (allHit.collider.GetComponent<SemiSolidPlatform>()) continue;
                if (allHit.collider.CompareTag("Bottom Wall") ||
                    allHit.collider.CompareTag("Top Wall") ||
                    allHit.collider.CompareTag("Left Wall") ||
                    allHit.collider.CompareTag("Right Wall") ||
                    allHit.collider.name.Contains("Door"))
                {
                    laserEndPos = allHit.point;
                }
            }

            _lineRenderer.SetPosition(1, laserEndPos);
            _lineRenderer.startWidth = .5f;
            _lineRenderer.endWidth = .5f;
            var layerMask = LayerMask.GetMask("Player");
            
            if (Physics.Raycast(laserStartPos, direction, out var hit, 50f, layerMask))
            {
                if (hit.collider.CompareTag("Player"))
                {
                    var player = hit.collider.GetComponentInChildren<CharacterAttack>();
                    if (player != null)
                    {
                        if (Time.time >= lastDamageTime + 0.25f) // makes sure player only takes damage at intervals
                        {
                            player.TakeDamagePlayer(attack, poiseDamage, knockbackPower);
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
    
    // lerps enemies hand based on which values are input (if right is null then right wont move)
    private IEnumerator MoveHands(Vector3? leftTarget, Vector3? rightTarget, float duration)
    {
        var elapsed = 0f;
        var leftStartPos = leftHand.position;
        var rightStartPos = rightHand.position;

        if (leftTarget != null)
        {
            _armMovementL = AudioManager.Instance.CreateEventInstance(FMODEvents.Instance.BossHandMove);
        }

        if (rightTarget != null)
        {
            _armMovementR = AudioManager.Instance.CreateEventInstance(FMODEvents.Instance.BossHandMove);
        }

        if (leftTarget != null && rightTarget != null)
        {
            _armMovementL.setVolume(0.25f);
            _armMovementR.setVolume(0.25f);
            Debug.Log("Lowering volume of hands to save your ears.");
        }
        
        AudioManager.Instance.AttachInstanceToGameObject(_armMovementL, leftHand.gameObject);
        AudioManager.Instance.AttachInstanceToGameObject(_armMovementR, rightHand.gameObject);
        while (elapsed < duration)
        {
            var t = elapsed / duration;

            if (leftTarget.HasValue)
            {
                if (_soundLStarted == false)
                {
                    _armMovementL.start();
                    _soundLStarted = true;
                }
                leftHand.position = Vector3.Lerp(leftStartPos, leftTarget.Value, t);
            }

            if (rightTarget.HasValue)
            { 
                if (_soundRStarted == false)
                {
                    _armMovementR.start();
                    _soundRStarted = true;
                }
                rightHand.position = Vector3.Lerp(rightStartPos, rightTarget.Value, t);
            }

            elapsed += Time.deltaTime * handLerpSpeed;
            yield return null;
        }
    }
    
    public void TakeDamage(int damage, int? poiseDmg, Vector3? knockback)
    {
        defense = Mathf.Clamp(defense, 0, 100);
        var dmgReduction = (100 - defense) / 100f;
        damage = Mathf.RoundToInt(damage * dmgReduction);
        
        if (Random.Range(0, 10) < 4) // 20 percent chance on hit for enemy to drop energy
        {
            Instantiate(Resources.Load<GameObject>("ItemPrefabs/Other/Energy Prefab"), transform.position, Quaternion.identity);
        }
        
        if (_health - damage > 0)
        {
            _health -= damage;
            healthSlider.value = _health;
            if (_health <= maxHealth / 2)
            {
                AudioManager.Instance.SetMusicParameter("Boss Phase", 1);
            }
        }
        else
        {
            _health = 0;
            healthSlider.value = 0;
            Die();
        }
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

    private void UpdateHandImg(bool handDL, bool handDR, bool handUL, bool handUR)
    {
        handDownL.SetActive(handDL);
        handDownR.SetActive(handDR);
        handUpL.SetActive(handUL);
        handUpR.SetActive(handUR);
    }
    
    private void UpdateColliders(bool colDL, bool colDR, bool colUL, bool colUR)
    {
        lhColliderDown.SetActive(colDL);
        rhColliderDown.SetActive(colDR);
        lhColliderUp.SetActive(colUL);
        rhColliderUp.SetActive(colUR);
    }

    public void ApplyKnockback(Vector2 knockbackPower)
    {
        throw new System.NotImplementedException();
    }
    
    public IEnumerator HitFlash(SpriteRenderer spriteRenderer)
    {
        spriteRenderer.material = hitMaterial;
        yield return new WaitForSeconds(0.1f);
        spriteRenderer.material = defaultMaterial;
    }

    private void Die()
    {
        isDead = true;
        LevelBuilder.Instance.bossDead = true;
        _characterAttack.ChanceHeal();
        _armMovementL.stop(STOP_MODE.IMMEDIATE);
        _armMovementL.release();
        _armMovementR.stop(STOP_MODE.IMMEDIATE);
        _armMovementR.release();
        _laserEvent.stop(STOP_MODE.IMMEDIATE);
        _laserEvent.release();
        _impulseVector = new Vector3(Random.Range(-1, 1), 5, 0);
        _impulseSource.m_ImpulseDefinition.m_ImpulseShape = CinemachineImpulseDefinition.ImpulseShapes.Explosion;
        _impulseSource.GenerateImpulseWithVelocity(_impulseVector * _settingManager.screenShakeMultiplier);
        AudioManager.Instance.PlayOneShot(FMODEvents.Instance.Explosion, transform.position);
        AudioManager.Instance.PlayOneShot(FMODEvents.Instance.Explosion, leftHand.position);
        AudioManager.Instance.PlayOneShot(FMODEvents.Instance.Explosion, rightHand.position);
        AudioManager.Instance.SetMusicParameter("Boss Phase", 4);
        
        var currencyToDrop = Random.Range(5, 20);
        for (var i = 0; i < currencyToDrop; i++)
        {
            Instantiate(Resources.Load<GameObject>("ItemPrefabs/Other/Currency Prefab"), transform.position, Quaternion.identity);
        }
        
        var energyToDrop = Random.Range(1, 10);
        for (var i = 0; i < energyToDrop; i++)
        {
            Instantiate(Resources.Load<GameObject>("ItemPrefabs/Other/Energy Prefab"), transform.position, Quaternion.identity);
        }
        
        //_characterMovement.lockedOn = false;
        //_lockOnController.lockedTarget = null;
        _roomScripting.enemies.Remove(gameObject);
        platform.SetActive(true);
        gameObject.SetActive(false);
    }
    
    
    private void OneHandAttackSoundFinish(bool isLeft, bool slam)
    {
        switch (isLeft)
        {
          case true:
              AudioManager.Instance.SetEventParameter(_armMovementL, "Move Complete", 1);
              _armMovementL.stop(STOP_MODE.ALLOWFADEOUT);
              _soundLStarted = false;
              break;
          case false:
              AudioManager.Instance.SetEventParameter(_armMovementR, "Move Complete", 1);
              _armMovementR.stop(STOP_MODE.ALLOWFADEOUT);
              _soundRStarted = false;
              break;
        }

        switch (slam)
        {
            case true when isLeft is true:
                _impulseSource.GenerateImpulseWithVelocity(_impulseVector * _settingManager.screenShakeMultiplier);
                AudioManager.Instance.PlayOneShot(FMODEvents.Instance.BossHandSlam, leftHand.position);
                _impulseVector = Vector3.zero;
                break;
            case true when isLeft is false:
                _impulseSource.GenerateImpulseWithVelocity(_impulseVector * _settingManager.screenShakeMultiplier);
                AudioManager.Instance.PlayOneShot(FMODEvents.Instance.BossHandSlam, rightHand.position);
                _impulseVector = Vector3.zero;
                break;
        }
    }
    
    private void TwoHandAttackSoundFinish(Vector3 slamTarget, bool slam)
    {
        AudioManager.Instance.SetEventParameter(_armMovementL, "Move Complete", 1);
        AudioManager.Instance.SetEventParameter(_armMovementR, "Move Complete", 1);
        _armMovementL.stop(STOP_MODE.ALLOWFADEOUT);
        _armMovementR.stop(STOP_MODE.ALLOWFADEOUT);
        if (slam)
        {
            _impulseSource.GenerateImpulseWithVelocity(_impulseVector * _settingManager.screenShakeMultiplier);
            AudioManager.Instance.PlayOneShot(FMODEvents.Instance.BossHandSlam, slamTarget);
            _impulseVector = Vector3.zero;
        }
        _soundLStarted = false;
        _soundRStarted = false;
    }
}