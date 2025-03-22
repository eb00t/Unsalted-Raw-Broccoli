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
    [Header("Enemy Stats")] 
    [SerializeField] private string bossName;
    [SerializeField] private int maxHealth;
    [SerializeField] private float attackCooldown;
    [SerializeField] private float handLerpSpeed, handHoverHeight;
    [SerializeField] private int poisonResistance;
    [SerializeField] private int poiseDamage;
    [SerializeField] private bool canFreeze; 
    private float _attackCdCounter;
    private States _state = States.Idle;
    private int _poisonBuildup, _health;
    private bool _isFrozen, _isPoisoned, _canAttack, _isPlayerInRange;
    public int attack;

    [Header("References")]
    [SerializeField] private Image healthFillImage;
    [SerializeField] private Transform leftHand, rightHand, groundPosition, bossEyePosition;
    [SerializeField] private GameObject lhColliderDown, rhColliderDown, lhColliderUp, rhColliderUp;
    [SerializeField] private GameObject handDownL, handDownR, handUpL, handUpR;
    [SerializeField] private TextMeshProUGUI bossTitle;
    private LineRenderer _lineRenderer;
    private Transform _target;
    private Vector3 _leftHandInitialPos, _rightHandInitialPos;
    private Slider _healthSlider;
    private LockOnController _lockOnController;
    private CharacterMovement _characterMovement;
    private RoomScripting _roomScripting;
    private EventInstance _armMovementL, _armMovementR;
    private EventInstance _laserEvent;
    private bool _soundLStarted, _soundRStarted;
    private CinemachineImpulseSource _impulseSource;
    private Vector3 _impulseVector;
    
    private enum States { Idle, Attack }
    public int Poise { get; set; }
    bool IDamageable.isPlayerInRange { get => _isPlayerInRange; set => _isPlayerInRange = value; }
    public bool isDead { get; set; }
    public RoomScripting RoomScripting { get; set; }
    public Spawner Spawner { get; set; }
    int IDamageable.Attack { get => attack; set => attack = value; }
    int IDamageable.PoiseDamage { get => poiseDamage; set => poiseDamage = value; }

    private void Start()
    {
        _roomScripting = gameObject.transform.root.GetComponent<RoomScripting>();
        _roomScripting.enemies.Add(gameObject);
        _impulseSource = GetComponent<CinemachineImpulseSource>();
        _healthSlider = GetComponentInChildren<Slider>();
        _healthSlider.maxValue = maxHealth;
        _healthSlider.value = maxHealth;
        _target = GameObject.FindGameObjectWithTag("Player").transform;
        _lockOnController = _target.GetComponent<LockOnController>();
        _characterMovement = _target.GetComponent<CharacterMovement>();
        _health = maxHealth;
        _leftHandInitialPos = leftHand.position;
        _rightHandInitialPos = rightHand.position;
        _canAttack = true;
        bossTitle.text = bossName;
        _lineRenderer = GetComponentInChildren<LineRenderer>();
    }

    private void Update()
    {
        _attackCdCounter -= Time.deltaTime;

        if (_isPlayerInRange)
        {
            _state = States.Attack;
        }
        else
        {
            _healthSlider.gameObject.SetActive(false);
            if (_state == States.Idle) return;
            {
                _state = States.Idle;
            }
        }

        switch (_state)
        {
            case States.Idle:
                StartCoroutine(Idle());
                break;
            case States.Attack:
                _healthSlider.gameObject.SetActive(true);
                if (!_canAttack || _attackCdCounter > 0) return;
                StartCoroutine(Attack());
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
    
    private IEnumerator Idle() // makes the hands hover up and down while idle
    {
        var hoverSpeed = 2f;
        var height = 0.2f;

        while (_state == States.Idle)
        {
            var hoverOffset = Mathf.Sin(Time.time * hoverSpeed) * height;
        
            leftHand.position = _leftHandInitialPos + new Vector3(0, hoverOffset, 0);
            rightHand.position = _rightHandInitialPos + new Vector3(0, hoverOffset, 0);

            yield return null;
        }
    }

    private IEnumerator Attack() // randomly pick an attack
    {
        _attackCdCounter = attackCooldown;
        attack = 5;
        
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
        
        CreateHandMovementEvents();

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
        _canAttack = false;
        
        UpdateColliders(true, true, false, false);
        UpdateHandImg(true, true, false, false);

        var leftTargetPos = new Vector3(leftHand.position.x, groundPosition.position.y, leftHand.position.z);
        var rightTargetPos = new Vector3(rightHand.position.x, groundPosition.position.y, rightHand.position.z);

        yield return StartCoroutine(MoveHands(leftTargetPos, rightTargetPos, 2f));
        _impulseVector = new Vector3(0, 1, 0);
        OneHandAttackSoundFinish(true, true);
        OneHandAttackSoundFinish(false, true);
        
        UpdateColliders(false, false, false, false); // give player opening to attack

        yield return new WaitForSecondsRealtime(2f);

        UpdateColliders(false, false, true, true);
        UpdateHandImg(false, false, true, true);
        
        var lungeTarget = new Vector3(_target.position.x, groundPosition.position.y, groundPosition.position.z);
        
        yield return StartCoroutine(MoveHands(lungeTarget, lungeTarget, 1f));
        _impulseVector = new Vector3(0, 2, 0);
        TwoHandAttackSoundFinish(lungeTarget, true);

        yield return new WaitForSecondsRealtime(0.1f);
        
        UpdateColliders(false, false, false, false);
        
        _canAttack = true;
    }

    // hand hovers over player for short duration then slams down
    private IEnumerator OverheadSlam(bool isLeft)
    {
        yield return StartCoroutine(ResetHands());
        _canAttack = false;
        attack = 30;
        
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
            _impulseVector = new Vector3(0, 2, 0);
            OneHandAttackSoundFinish(isLeft, true);
            UpdateColliders(false, false, false, false);
            yield return new WaitForSecondsRealtime(2f);
            yield return StartCoroutine(MoveHands(null, hoverPosition, 0.5f));
            OneHandAttackSoundFinish(isLeft, false);
        }
        else
        {
            yield return StartCoroutine(MoveHands(slamPosition, null, 0.5f));
            _impulseVector = new Vector3(0, 2, 0);
            UpdateColliders(false, false, false, false);
            OneHandAttackSoundFinish(isLeft, true);
            yield return new WaitForSecondsRealtime(2f);
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
        _canAttack = false;
        
        UpdateHandImg(false, false, true, true);

        var leftWidePos = _target.position + Vector3.left * 5;
        var rightWidePos = _target.position + Vector3.right * 5;
        var clapCount = Random.Range(1, 4);
        var clapTarget = _target.position;
        

        for (var i = 0; i <= clapCount; i++)
        {
            yield return StartCoroutine(MoveHands(leftWidePos, rightWidePos, 1f));
            yield return new WaitForSeconds(0.1f);
            TwoHandAttackSoundFinish(transform.position, false);
            UpdateColliders(false, false, true, true);
            yield return StartCoroutine(MoveHands(clapTarget, clapTarget, .5f));
            _impulseVector = new Vector3(4, 0, 0);
            TwoHandAttackSoundFinish(clapTarget, true);
            
            yield return new WaitForSecondsRealtime(0.1f);

            UpdateColliders(false, false, false, false);
        }

        _canAttack = true;
    }
    
    private IEnumerator LaserAttack() // aims laser at player that tracks, then it stops and starts doing damage
    {
        yield return StartCoroutine(ResetHands());
        _laserEvent = AudioManager.Instance.CreateEventInstance(FMODEvents.Instance.BossHandLaser);
        _canAttack = false;
        _lineRenderer.enabled = true;
        _lineRenderer.SetPosition(0, bossEyePosition.position);
        var laserStartPos = _lineRenderer.GetPosition(0);;
        var chargeTime = 1.5f;
        var fireTime = 1f;
        var trackSpeed = 0.5f;
        var delay = 0.1f;
        var targetPos = new Vector3(0, 0, 0);
        var elapsed = 0f;

        AudioManager.Instance.AttachInstanceToGameObject(_laserEvent, gameObject.transform);
        _laserEvent.start();
        
        while (elapsed < chargeTime)
        {
            targetPos = _target.position;

            _lineRenderer.SetPosition(0, laserStartPos); 
            _lineRenderer.SetPosition(1, targetPos);
            _lineRenderer.startColor = Color.white;
            _lineRenderer.endColor = Color.white;
        
            elapsed += Time.deltaTime * trackSpeed;
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
            var laserEndPos = laserStartPos + direction * (dist + 5f);

            _lineRenderer.SetPosition(1, laserEndPos);
            _lineRenderer.endColor = Color.red;
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
        
        AudioManager.Instance.AttachInstanceToGameObject(_armMovementL, leftHand);
        AudioManager.Instance.AttachInstanceToGameObject(_armMovementR, rightHand);
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
        if (_health - damage > 0)
        {
            _health -= damage;
            _healthSlider.value = _health;
            if (_health <= maxHealth / 2)
            {
                AudioManager.Instance.SetMusicParameter("Boss Phase", 1);
            }
        }
        else
        {
            _health = 0;
            _healthSlider.value = 0;
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

    private void Die()
    {
        isDead = true;
        LevelBuilder.Instance.bossDead = true;
        _armMovementL.stop(STOP_MODE.IMMEDIATE);
        _armMovementL.release();
        _armMovementR.stop(STOP_MODE.IMMEDIATE);
        _armMovementR.release();
        _impulseVector = new Vector3(Random.Range(-1, 1), 5, 0);
        _impulseSource.m_ImpulseDefinition.m_ImpulseShape = CinemachineImpulseDefinition.ImpulseShapes.Explosion;
        _impulseSource.GenerateImpulseWithVelocity(_impulseVector);
        AudioManager.Instance.PlayOneShot(FMODEvents.Instance.Explosion, transform.position);
        AudioManager.Instance.PlayOneShot(FMODEvents.Instance.Explosion, leftHand.position);
        AudioManager.Instance.PlayOneShot(FMODEvents.Instance.Explosion, rightHand.position);
        AudioManager.Instance.SetMusicParameter("Boss Phase", 4);
        _characterMovement.lockedOn = false;
        _lockOnController.lockedTarget = null;
        _roomScripting.enemies.Remove(gameObject);
        gameObject.SetActive(false);
    }

    void CreateHandMovementEvents()
    {
        _armMovementL = AudioManager.Instance.CreateEventInstance(FMODEvents.Instance.BossHandMove);
        _armMovementR = AudioManager.Instance.CreateEventInstance(FMODEvents.Instance.BossHandMove);
    }
    
    void OneHandAttackSoundFinish(bool isLeft, bool slam)
    {
        switch (isLeft)
        {
          case true:
              AudioManager.Instance.SetEventParameter(_armMovementL, "Move Complete", 1);
              _armMovementL.release();
              _soundLStarted = false;
              break;
          case false:
              AudioManager.Instance.SetEventParameter(_armMovementR, "Move Complete", 1);
              _armMovementR.release();
              _soundRStarted = false;
              break;
        }

        switch (slam)
        {
            case true when isLeft is true:
                _impulseSource.GenerateImpulseWithVelocity(_impulseVector);
                AudioManager.Instance.PlayOneShot(FMODEvents.Instance.BossHandSlam, leftHand.position);
                _impulseVector = Vector3.zero;
                break;
            case true when isLeft is false:
                _impulseSource.GenerateImpulseWithVelocity(_impulseVector);
                AudioManager.Instance.PlayOneShot(FMODEvents.Instance.BossHandSlam, rightHand.position);
                _impulseVector = Vector3.zero;
                break;
        }
    }
    
    void TwoHandAttackSoundFinish(Vector3 slamTarget, bool slam)
    {
        AudioManager.Instance.SetEventParameter(_armMovementL, "Move Complete", 1);
        AudioManager.Instance.SetEventParameter(_armMovementR, "Move Complete", 1);
        _armMovementL.release();
        _armMovementR.release();
        if (slam)
        {
            _impulseSource.GenerateImpulseWithVelocity(_impulseVector);
            AudioManager.Instance.PlayOneShot(FMODEvents.Instance.BossHandSlam, slamTarget);
            _impulseVector = Vector3.zero;
        }
        _soundLStarted = false;
        _soundRStarted = false;
    }
}