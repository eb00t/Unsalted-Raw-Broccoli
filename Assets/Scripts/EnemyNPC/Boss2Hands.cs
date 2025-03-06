using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;
using UnityEngine.UI;

public class Boss2Hands : MonoBehaviour, IDamageable
{
    [Header("Enemy Stats")]
    [SerializeField] private int maxHealth;
    [SerializeField] private float attackCooldown;
    [SerializeField] private float handLerpSpeed, handHoverHeight;
    [SerializeField] private int poisonResistance;
    [SerializeField] private bool canFreeze; 
    private float _attackCdCounter;
    private States _state = States.Idle;
    private int _poisonBuildup, _health;
    private bool _isFrozen, _isPoisoned, _canAttack, _isPlayerInRange;
    public int attack;

    [Header("References")]
    [SerializeField] private Image healthFillImage;
    [SerializeField] private Transform leftHand, rightHand, groundPosition;
    [SerializeField] private GameObject lhColliderDown, rhColliderDown, lhColliderUp, rhColliderUp;
    [SerializeField] private GameObject handDownL, handDownR, handUpL, handUpR;
    private Transform _target;
    private Vector3 _leftHandInitialPos, _rightHandInitialPos;
    private Slider _healthSlider;
    
    private enum States { Idle, Attack }

    bool IDamageable.isPlayerInRange
    {
        get => _isPlayerInRange; 
        set => _isPlayerInRange = value;
    }
    
    public RoomScripting RoomScripting { get; set; }
    public Spawner Spawner { get; set; }
    
    int IDamageable.Attack
    {
        get => attack;
        set => attack = value;
    }

    private void Start()
    {
        _healthSlider = GetComponentInChildren<Slider>();
        _healthSlider.maxValue = maxHealth;
        _healthSlider.value = maxHealth;
        _target = GameObject.FindGameObjectWithTag("Player").transform;
        _health = maxHealth;
        _leftHandInitialPos = leftHand.position;
        _rightHandInitialPos = rightHand.position;
        _canAttack = true;
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
        
        var attackType = Random.Range(0, 3);

        switch (attackType)
        {
            case 0:
                yield return StartCoroutine(GroundPoundAndLunge());
                break;
            case 1:
                yield return StartCoroutine(OverheadSlam());
                break;
            case 2:
                yield return StartCoroutine(ClapAttack());
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

        yield return new WaitForSecondsRealtime(0.5f);

        UpdateColliders(false, false, true, true);
        UpdateHandImg(false, false, true, true);
        
        var lungeTarget = _target.position;
        yield return StartCoroutine(MoveHands(lungeTarget, lungeTarget, 1f));

        yield return new WaitForSecondsRealtime(0.5f);
        
        UpdateColliders(false, false, false, false);
        
        _canAttack = true;
    }

    // hand hovers over player for short duration then slams down
    private IEnumerator OverheadSlam()
    {
        yield return StartCoroutine(ResetHands());
        _canAttack = false;

        UpdateHandImg(false, true, true, false);
        var hoverPosition = _target.position + Vector3.up * handHoverHeight;
        yield return StartCoroutine(MoveHands(null, hoverPosition, 1f));

        yield return new WaitForSecondsRealtime(1f);
        
        UpdateColliders(true, true, false, false);
        
        var slamPosition = new Vector3(hoverPosition.x, groundPosition.position.y, hoverPosition.z);
        yield return StartCoroutine(MoveHands(null, slamPosition, 0.5f));

        yield return StartCoroutine(MoveHands(null, hoverPosition, 0.5f));

        yield return new WaitForSecondsRealtime(0.5f);
        
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
            yield return new WaitForSeconds(0.5f);

            UpdateColliders(false, false, true, true);
            
            yield return StartCoroutine(MoveHands(clapTarget, clapTarget, 1f));

            yield return new WaitForSecondsRealtime(0.5f);

            UpdateColliders(false, false, false, false);
        }

        _canAttack = true;
    }
    
    // lerps enemies hand based on which values are input (if right is null then right wont move)
    private IEnumerator MoveHands(Vector3? leftTarget, Vector3? rightTarget, float duration)
    {
        var elapsed = 0f;
        var leftStartPos = leftHand.position;
        var rightStartPos = rightHand.position;

        while (elapsed < duration)
        {
            var t = elapsed / duration;

            if (leftTarget.HasValue)
            {
                leftHand.position = Vector3.Lerp(leftStartPos, leftTarget.Value, t);
            }

            if (rightTarget.HasValue)
            {
                rightHand.position = Vector3.Lerp(rightStartPos, rightTarget.Value, t);
            }

            elapsed += Time.deltaTime * handLerpSpeed;
            yield return null;
        }
    }
    
    public void TakeDamage(int damage)
    {
        if (_health - damage > 0)
        {
            _health -= damage;
            _healthSlider.value = _health;
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
            TakeDamage(damageToTake);
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
        gameObject.SetActive(false);
    }
}