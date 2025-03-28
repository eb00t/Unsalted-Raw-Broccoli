using System;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using Random = UnityEngine.Random;


// Adapted from https://github.com/Chaker-Gamra/2.5D-Platformer-Game/blob/master/Assets/Scripts/Enemy/Enemy.cs

public class BossHandler : MonoBehaviour
{
    [Header("Enemy Stats")]
    private int _health;
    [SerializeField] private int maxHealth;
    public int bossAtk;
    [SerializeField] private float chaseRange;
    [SerializeField] private float atkRange;
    [SerializeField] private float maxPatrolRange;
    [SerializeField] private float minPatrolRange;
    private float _targetTime;
    
    private Slider _healthSlider;
    private Animator _animator;
    private Transform _target;
    private NavMeshAgent _agent;
    
    private Vector3 _patrolTarget, _patrol1, _patrol2;
    private States _state =  States.Idle;
    
    [Header("Debugging")]
    [SerializeField] private bool _isIdle;
    [SerializeField] private bool debugPatrol;
    [SerializeField] private bool debugRange;

    private enum States
    {
        Idle,
        Patrol,
        Chase,
        Attack
    }

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
        _agent = GetComponent<NavMeshAgent>();
        _agent.updateRotation = false;
        _healthSlider.gameObject.SetActive(false);
        _animator = GetComponent<Animator>();
        
        PickPatrolPoints();
        _patrolTarget = _patrol1;
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
        else if (!_isIdle)
        {
            _state = States.Patrol;
        }
        else
        {
            _state = States.Idle;
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
            default:
                throw new ArgumentOutOfRangeException();
        }
        
        var velocity = _agent.velocity;
        var localScale = transform.localScale;
        
        localScale = velocity.x switch
        {
            > 0.1f => new Vector3(Mathf.Abs(localScale.x), localScale.y, localScale.z),
            < -0.1f => new Vector3(-Mathf.Abs(localScale.x), localScale.y, localScale.z),
            _ => localScale
        };
        transform.localScale = localScale;
    }

    private void Patrol() // TODO: make enemies return to spawn location when restarting patrol
    {
        if (_agent.pathPending || !(_agent.remainingDistance <= _agent.stoppingDistance)) return;
        _patrolTarget = _patrolTarget == _patrol1 ? _patrol2 : _patrol1;
        
        _healthSlider.gameObject.SetActive(false);
        _agent.SetDestination(_patrolTarget);
    }

    private void Chase()
    {
        _agent.isStopped = false;
        _healthSlider.gameObject.SetActive(true);
        _agent.SetDestination(_target.position);
    }

    private void Attack()
    {
        _agent.ResetPath();
        _agent.isStopped = true;
        _targetTime -= Time.deltaTime;

        if (!(_targetTime <= 0.0f)) return;
        //_characterAttack.TakeDamagePlayer(atk);
        var atkNum = Random.Range(0, 2);
        Debug.Log(atkNum);

        if (atkNum == 0)
        {
            _animator.SetTrigger("JumpAttack1");
        }
        else if (atkNum == 1)
        {
            _animator.SetTrigger("HandSwipe");
        }
        
        _targetTime = 4f;
    }
    
    private void PickPatrolPoints()
    {
        var position = transform.position;
        var ranDist = Random.Range(position.x + minPatrolRange, maxPatrolRange);
        _patrol1 = new Vector3(ranDist, position.y, position.z);
        _patrol2 = new Vector3(-ranDist, position.y, position.z);
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
        var position = transform.position;

        if (debugRange)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(position, atkRange);

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(position, chaseRange);
        }

        if (debugPatrol)
        {
            //PickPatrolPoints();
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(position, maxPatrolRange);
            Gizmos.DrawWireSphere(position, minPatrolRange);

            var v = new Vector3(1, 1, 1);

            Gizmos.DrawWireCube(_patrol1, v);
            Gizmos.DrawWireCube(_patrol2, v);
        }
    }
    
    private void OnDisable()
    {
        RoomScripting roomScripting = gameObject.transform.root.GetComponent<RoomScripting>();
        roomScripting.enemies.Remove(gameObject);
    }

}