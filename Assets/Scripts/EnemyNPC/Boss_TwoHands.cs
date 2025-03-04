using System;
using System.Collections;
using System.Runtime.CompilerServices;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering.Universal;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Random = UnityEngine.Random;


// Adapted from https://github.com/Chaker-Gamra/2.5D-Platformer-Game/blob/master/Assets/Scripts/Enemy/Enemy.cs

public class Boss_TwoHands : MonoBehaviour, IDamageable
{
    [Header("Enemy Stats")]
    [SerializeField] private int maxHealth;
    [SerializeField] private float chaseRange;
    [SerializeField] private float atkRange;
    [SerializeField] private float minPatrolRange, maxPatrolRange;
    [SerializeField] private float freezeDuration, freezeCooldown;
    [SerializeField] private int poisonResistance;
    [SerializeField] private bool canFreeze; // if by default set to false the enemy will never freeze
    public int attack;
    private int _poisonBuildup, _health;
    private bool _isFrozen, _isPoisoned;
    private States _state =  States.Idle;

    [Header("References")] 
    [SerializeField] private Image healthFillImage;
    private Slider _healthSlider;
    private Animator _animator;
    private Transform _target;
    private NavMeshAgent _agent;
    private float _targetTime;

    [Header("Debugging")] 
    [SerializeField] private bool debugRange;

    private enum States
    {
        Idle,
        Chase,
        Attack,
        Frozen
    }
    
    int IDamageable.Attack
    {
        get => attack;
        set => attack = value;
    }

    public RoomScripting RoomScripting { get; set; }
    public Spawner Spawner { get; set; }

    private void Awake()
    {
        RoomScripting roomScripting = gameObject.transform.root.GetComponent<RoomScripting>();
        roomScripting.enemies.Add(gameObject);
    }

    private void Start()
    {
        _healthSlider = GetComponentInChildren<Slider>();
        _healthSlider.maxValue = maxHealth;
        _healthSlider.value = maxHealth;
        _health = maxHealth;
        
        _target = GameObject.FindGameObjectWithTag("Player").transform;
        _healthSlider.gameObject.SetActive(false);
        _animator = GetComponent<Animator>();
    }

    private void Update()
    {
        var distance = Vector3.Distance(transform.position, _target.position);
        
        
        if (_isFrozen)
        {
            _state = States.Frozen;
        }
        else
        {
            if (distance < atkRange)
            {
                _state = States.Attack;
            }
            else if (distance < chaseRange)
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
                Frozen();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void Chase()
    {
        _healthSlider.gameObject.SetActive(true);
    }

    private void Attack()
    {
        _healthSlider.gameObject.SetActive(true);
        _targetTime -= Time.deltaTime;
        
        if (!(_targetTime <= 0.0f)) return;
        var atkNum = Random.Range(0, 6);

        switch (atkNum)
        {
            case 0:
                _animator.SetTrigger("HandPoundLeft");
                break;
            case 1:
                _animator.SetTrigger("HandPoundRight");
                break;
            case 2:
                _animator.SetTrigger("HandClap");
                break;
            case 3:
                _animator.SetTrigger("HandPound");
                break;
            case 4:
                _animator.SetTrigger("HandSlideLeft");
                break;
            case 5:
                _animator.SetTrigger("HandSlideRight");
                break;
        }
        
        _targetTime = 4f;
    }
   
    private void Frozen()
    {
        if (!canFreeze) return;
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
        canFreeze = false;
        yield return new WaitForSecondsRealtime(freezeCooldown);
        canFreeze = true;
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
    
    private void Die()
    {
        gameObject.SetActive(false);
    }

    private void OnDrawGizmos()
    {
        if (debugRange)
        {
            var position = transform.position;

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(position, atkRange);

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(position, chaseRange);
        }
    }
    
    
    private void OnDisable()
    {
        RoomScripting roomScripting = gameObject.transform.root.GetComponent<RoomScripting>();
        roomScripting.enemies.Remove(gameObject);
    }

    void IDamageable.ApplyKnockback(Vector2 KnockbackPower)
    {
        throw new NotImplementedException();
    }
}