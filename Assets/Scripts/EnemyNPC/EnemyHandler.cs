using System;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using Random = UnityEngine.Random;


// Adapted from https://github.com/Chaker-Gamra/2.5D-Platformer-Game/blob/master/Assets/Scripts/Enemy/Enemy.cs

public class EnemyHandler : MonoBehaviour
{
    [Header("Enemy Stats")]
    private int _health = 100;
    [SerializeField] private int maxHealth;
    public int enemyAtk;
    [SerializeField] private float atkDelay;
    [SerializeField] private float chaseRange;
    [SerializeField] private float attackRange;
    [SerializeField] private float maxPatrolRange;
    [SerializeField] private float minPatrolRange;
    
    private float _targetTime;
    private Slider _healthSlider;
    private Animator _animator;
    private Transform _target;
    private NavMeshAgent _agent;
    private Vector3 _patrolTarget, _patrol1, _patrol2;
    private States _state = States.Idle;
    
    [Header("Debugging")]
    [SerializeField] private bool _isIdle;
    [SerializeField] private bool debugPatrol;
    [SerializeField] private bool debugRange;
    
    [Header("References")]
    [SerializeField] private BoxCollider atkHitbox;

    private int knockbackDir = 0;
    [SerializeField] private Vector3 knockbackPower = new Vector3(10f, 1f, 0f);

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
        _healthSlider.gameObject.SetActive(false);
        _animator = GetComponent<Animator>();
        
        _target = GameObject.FindGameObjectWithTag("Player").transform;
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
        
        _animator.SetFloat("vel", Mathf.Abs(velocity.x));

        var localScale = GetComponentInChildren<SpriteRenderer>().transform.localScale;
        
        if (velocity.x > 0.1f)
        {
            localScale = new Vector3(Mathf.Abs(localScale.x), localScale.y, localScale.z);
            atkHitbox.center = new Vector3(1.2f, -0.1546797f, 0);
        }
        else if (velocity.x < -0.1f)
        {
            localScale = new Vector3(-Mathf.Abs(localScale.x), localScale.y, localScale.z);
            atkHitbox.center = new Vector3(-1.2f, -0.1546797f, 0);
        }
        
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

        if (_targetTime <= 0.0f)
        {
            _animator.SetTrigger("Attack");
            _targetTime = atkDelay;
        }
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

            // Stun
            _agent.velocity = Vector3.zero;
            if (transform.position.x > _target.position.x)
            {
                knockbackDir = 1;
            } else knockbackDir = -1;
            _agent.velocity += new Vector3(knockbackPower.x * knockbackDir, knockbackPower.y, knockbackPower.z);
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

        if (debugRange)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(position, attackRange);

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(position, chaseRange);
        }

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

    
    private void OnDisable()
    {
        RoomScripting roomScripting = gameObject.transform.root.GetComponent<RoomScripting>();
        roomScripting.enemies.Remove(gameObject);
    }
}