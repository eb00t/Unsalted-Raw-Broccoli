using System;
using System.Collections;
using TreeEditor;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering.Universal;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Random = UnityEngine.Random;


// Adapted from https://github.com/Chaker-Gamra/2.5D-Platformer-Game/blob/master/Assets/Scripts/Enemy/Enemy.cs

public class EnemyHandler : MonoBehaviour
{
    [Header("Enemy Stats")]
    private int _health = 100;
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private int enemyAtk = 10;
    [SerializeField] private float chaseRange = 5;
    [SerializeField] private float attackRange = 2;
    [SerializeField] private float maxPatrolRange = 15;
    [SerializeField] private float minPatrolRange = 6;
    private float _targetTime;
    
    private Slider _healthSlider;
    private Animator _animator;
    private Transform _target;
    private NavMeshAgent _agent;
    private Transform _canvasTransform;
    
    private CharacterAttack _characterAttack;
    
    private Vector3 _patrolTarget, _patrol1, _patrol2;
    private States _state =  States.Idle;
    [SerializeField] private bool _isIdle;

    [SerializeField] private bool debugPatrol;

    private enum States
    {
        Idle,
        Patrol,
        Chase,
        Attack
    }

    private void Start()
    {
        _healthSlider = GetComponentInChildren<Slider>();
        _healthSlider.maxValue = maxHealth;
        _healthSlider.value = maxHealth;
        _health = maxHealth;
        _healthSlider.gameObject.SetActive(false);

        _canvasTransform = GetComponentInChildren<Canvas>().transform;
        
        _target = GameObject.FindGameObjectWithTag("Player").transform;
        _characterAttack = _target.GetComponentInChildren<CharacterAttack>();
        _agent = GetComponent<NavMeshAgent>();
        _agent.updateRotation = false;
        
        PickPatrolPoints();
        _patrolTarget = _patrol1;
    }

    private void Update()
    {
        var distance = Vector3.Distance(transform.position, _target.position);

        if (distance < attackRange)
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

        var localScale = GetComponentInChildren<SpriteRenderer>().transform.localScale;
        
        localScale = velocity.x switch
        {
            > 0.1f => new Vector3(Mathf.Abs(localScale.x), localScale.y, localScale.z),
            < -0.1f => new Vector3(-Mathf.Abs(localScale.x), localScale.y, localScale.z),
            _ => localScale
        };
        GetComponentInChildren<SpriteRenderer>().transform.localScale = localScale;
        
        
    }

    private void Patrol()
    {
        if (_agent.pathPending || !(_agent.remainingDistance <= _agent.stoppingDistance)) return;
        _patrolTarget = _patrolTarget == _patrol1 ? _patrol2 : _patrol1;
        
        _healthSlider.gameObject.SetActive(false);

        _agent.SetDestination(_patrolTarget);
        
        /*
                  var dist = Mathf.Abs(transform.position.x - _patrolTarget.x);
          
                  if (dist <= 0.1f)
                  {
                      _patrolTarget = (_patrolTarget == _patrol1) ? _patrol2 : _patrol1;
                  }
          
                  var x = Mathf.MoveTowards(transform.position.x, _patrolTarget.x, speed * Time.deltaTime);
                  transform.position = new Vector3(x, transform.position.y, transform.position.z);
          
                  if (_patrolTarget.x > transform.position.x)
                  {
                      transform.rotation = Quaternion.Euler(0, 180, 0);
                  }
                  else if (_patrolTarget.x < transform.position.x)
                  {
                      transform.rotation = Quaternion.identity;
                  }
                  */
    }

    private void Chase()
    {
        _agent.isStopped = false;
        _healthSlider.gameObject.SetActive(true);
        _agent.SetDestination(_target.position);
        
        /*
        if (_target.position.x > transform.position.x)
        {
            // right
            transform.Translate(transform.right * (speed * Time.deltaTime));
            transform.rotation = Quaternion.Euler(0, 180, 0);
        }
        else
        {
            // left
            transform.Translate(-transform.right * (speed * Time.deltaTime));
            transform.rotation = Quaternion.identity;
        }
        */
    }

    private void Attack()
    {
        _agent.ResetPath();
        _agent.isStopped = true;
        _targetTime -= Time.deltaTime;

        if (!(_targetTime <= 0.0f)) return;
        _characterAttack.TakeDamagePlayer(enemyAtk);
        _targetTime = 2f;
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
        //animator.SetTrigger("isDead");
        
        gameObject.SetActive(false);
        //Destroy(gameObject);
    }
    

    private void OnDrawGizmos()
    {
        var position = transform.position;
        //PickPatrolPoints();
        
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(position, attackRange);
        
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(position, chaseRange);

        if (debugPatrol)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(position, maxPatrolRange);
            Gizmos.DrawWireSphere(position, minPatrolRange);

            var v = new Vector3(1, 1, 1);

            Gizmos.DrawWireCube(_patrol1, v);
            Gizmos.DrawWireCube(_patrol2, v);
        }
    }

}