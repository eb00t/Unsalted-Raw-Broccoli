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

public class Boss_TwoHands : MonoBehaviour
{
    [Header("Enemy Stats")]
    [SerializeField] private int maxHealth;
    [SerializeField] private float chaseRange;
    [SerializeField] private float atkRange;
    [SerializeField] private float maxPatrolRange;
    [SerializeField] private float minPatrolRange;
    public int bossAtk;
    private int _health;
    
    private float _targetTime;
    private Slider _healthSlider;
    private Animator _animator;
    private Transform _target;
    private NavMeshAgent _agent;
    private States _state =  States.Idle;

    [Header("Debugging")] 
    [SerializeField] private bool debugRange;
    
    private enum States
    {
        Idle,
        Chase,
        Attack
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
            default:
                throw new ArgumentOutOfRangeException();
        }
        
        /*
        var velocity = _agent.velocity;
        var localScale = transform.localScale;
        
        localScale = velocity.x switch
        {
            > 0.1f => new Vector3(Mathf.Abs(localScale.x), localScale.y, localScale.z),
            < -0.1f => new Vector3(-Mathf.Abs(localScale.x), localScale.y, localScale.z),
            _ => localScale
        };
        transform.localScale = localScale;
        */
    }

    private void Chase()
    {
        _healthSlider.gameObject.SetActive(true);
    }

    private void Attack()
    {
        _targetTime -= Time.deltaTime;

        if (!(_targetTime <= 0.0f)) return;
        //_characterAttack.TakeDamagePlayer(atk);
        var atkNum = Random.Range(0, 2);
        Debug.Log(atkNum);

        if (atkNum == 0)
        {
            _animator.SetTrigger("Hand1Swipe");
        }
        else if (atkNum == 1)
        {
            _animator.SetTrigger("Hand2Swipe");
        }
        
        _targetTime = 4f;
    }
    
    public void TakeDamageEnemy(int damage)
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
}