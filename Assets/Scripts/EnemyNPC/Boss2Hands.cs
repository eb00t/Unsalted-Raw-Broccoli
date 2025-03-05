using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;
using UnityEngine.UI;

public class Boss2Hands : MonoBehaviour, IDamageable
{
    [Header("Enemy Stats")]
    [SerializeField] private int maxHealth;
    [SerializeField] private float atkRange, attackCooldown;
    [SerializeField] private float handLerpSpeed, handHoverHeight;
    [SerializeField] private int poisonResistance;
    [SerializeField] private bool canFreeze; 
    private float _attackCdCounter;
    private States _state = States.Idle;
    private int _poisonBuildup, _health;
    private bool _isFrozen, _isPoisoned, _canAttack;
    public int attack;

    [Header("References")]
    [SerializeField] private Image healthFillImage;
    [SerializeField] private Transform leftHand, rightHand, groundPosition;
    [SerializeField] private GameObject leftHandCollider, rightHandCollider;
    private Transform _target;
    private Vector3 _leftHandInitialPos, _rightHandInitialPos;
    private Slider _healthSlider;
    
    private enum States { Idle, Attack }
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
        var distance = Vector3.Distance(transform.position, _target.position);
        _attackCdCounter -= Time.deltaTime;

        if (distance < atkRange)
        {
            _state = States.Attack;
        }
        else
        {
            _state = States.Idle;
        }

        switch (_state)
        {
            case States.Idle:
                _healthSlider.gameObject.SetActive(false);
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

    private IEnumerator Attack()
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

            elapsed += Time.deltaTime * handLerpSpeed;
            yield return null;
        }

    }
    
    // hands slam to the ground and after a short duration the hand slide towards the player
    private IEnumerator GroundPoundAndLunge()
    {
        yield return StartCoroutine(ResetHands());
        _canAttack = false;

        var leftTargetPos = new Vector3(leftHand.position.x, groundPosition.position.y, leftHand.position.z);
        var rightTargetPos = new Vector3(rightHand.position.x, groundPosition.position.y, rightHand.position.z);

        yield return StartCoroutine(MoveHands(leftTargetPos, rightTargetPos, 2f));

        yield return new WaitForSecondsRealtime(0.5f);

        leftHandCollider.SetActive(true);
        rightHandCollider.SetActive(true);
        
        var lungeTarget = _target.position;
        yield return StartCoroutine(MoveHands(lungeTarget, lungeTarget, 1f));

        yield return new WaitForSecondsRealtime(0.5f);
        
        leftHandCollider.SetActive(false);
        rightHandCollider.SetActive(false);
        _canAttack = true;
    }

    private IEnumerator OverheadSlam()
    {
        yield return StartCoroutine(ResetHands());
        _canAttack = false;

        var hoverPosition = _target.position + Vector3.up * handHoverHeight;
        yield return StartCoroutine(MoveHands(null, hoverPosition, 1f));

        yield return new WaitForSecondsRealtime(1f);
        
        leftHandCollider.SetActive(true);
        rightHandCollider.SetActive(true);
        
        var slamPosition = new Vector3(hoverPosition.x, groundPosition.position.y, hoverPosition.z);
        yield return StartCoroutine(MoveHands(null, slamPosition, 0.5f));

        yield return StartCoroutine(MoveHands(null, hoverPosition, 0.5f));

        yield return new WaitForSecondsRealtime(0.5f);
        
        leftHandCollider.SetActive(false);
        rightHandCollider.SetActive(false);
        _canAttack = true;
    }

    private IEnumerator ClapAttack()
    {
        yield return StartCoroutine(ResetHands());
        _canAttack = false;

        var leftWidePos = _target.position + Vector3.left * 5;
        var rightWidePos = _target.position + Vector3.right * 5;
        yield return StartCoroutine(MoveHands(leftWidePos, rightWidePos, 1f));

        yield return new WaitForSeconds(0.5f);

        leftHandCollider.SetActive(true);
        rightHandCollider.SetActive(true);
        var clapTarget = _target.position;
        yield return StartCoroutine(MoveHands(clapTarget, clapTarget, 1f));

        yield return new WaitForSecondsRealtime(0.5f);
        
        leftHandCollider.SetActive(false);
        rightHandCollider.SetActive(false);
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
    
    private void OnDrawGizmosSelected()
    {
            var position = transform.position;

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(position, atkRange);
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