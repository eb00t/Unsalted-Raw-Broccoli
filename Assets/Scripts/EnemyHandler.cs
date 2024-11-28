using System;
using System.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Random = UnityEngine.Random;


// Adapted from https://github.com/Chaker-Gamra/2.5D-Platformer-Game/blob/master/Assets/Scripts/Enemy/Enemy.cs

// TODO: fix issue with enemy ai breaking references when being spawned by code
public class EnemyHandler : MonoBehaviour
{
    [Header("Enemy Stats")]
    [SerializeField] private int health = 100;
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private int enemyAtk = 10;
    [SerializeField] private float chaseRange = 5;
    [SerializeField] private float attackRange = 2;
    [SerializeField] private float speed = 3;
    [SerializeField] private float maxPatrolRange = 15;
    [SerializeField] private float minPatrolRange = 6;
    
    [Header("References")]
    private Slider _healthSlider;
    private CharacterAttack _characterAttack;
    
    private Transform _target;
    private Vector3 _patrolTarget;
    private Animator _animator;
    private States _state =  States.Idle;
    private Vector3 _patrol1, _patrol2;
    private bool _isIdle;
    private float _targetTime;
    

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
        _healthSlider.value = health;
        _target = GameObject.FindGameObjectWithTag("Player").transform;
        _characterAttack = _target.GetComponentInChildren<CharacterAttack>();
        PickPatrolPoints();
        _patrolTarget = _patrol1;
    }

    private void Update()
    {
        /*
        if (PlayerManager.gameOver)
        {
            animator.enabled = false;
            this.enabled = false;
        }
        */
        
        
        var distance = Vector3.Distance(transform.position, _target.position);
        
        if (distance < chaseRange || (distance > attackRange && distance < chaseRange))
        {
            _state = States.Chase;
        } 
        else if (distance > chaseRange && !_isIdle)
        {
            _state = States.Patrol;
        }
        else if (distance < attackRange)
        {
            _state = States.Attack;
        }

        switch (_state)
        {
            case States.Idle:
            {
                
                break;
            }
            case States.Patrol:
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

                break;
            case States.Chase:
            {
                //animator.SetTrigger("chase");
                //animator.SetBool("isAttacking", false);
                
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
                break;
            }
            case States.Attack:
            {
                //animator.SetBool("isAttacking", true);
                
                _targetTime -= Time.deltaTime;
                
                if (_targetTime <= 0.0f)
                {
                    _characterAttack.TakeDamagePlayer(enemyAtk);
                    _targetTime = 2f;
                }

                break;
            }
            default:
                throw new ArgumentOutOfRangeException();
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
        if (health - damage > 0)
        {
            health -= damage;
            _healthSlider.value = health;
        }
        else
        {
            health = 0;
            _healthSlider.value = 0;
            Die();
        }
    }
    
    private void Die()
    {
        //animator.SetTrigger("isDead");
        
        /*
        GetComponent<CapsuleCollider>().enabled = false;
        gameObject.SetActive(false);
        enabled = false;
        */
        Destroy(gameObject);
    }
    

    private void OnDrawGizmosSelected()
    {
        var position = transform.position;
        //PickPatrolPoints();
        
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(position, attackRange);
        
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(position, chaseRange);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(position, maxPatrolRange);
        Gizmos.DrawWireSphere(position, minPatrolRange);
        
        var v = new Vector3(1, 1, 1);
        
        Gizmos.DrawWireCube(_patrol1, v);
        Gizmos.DrawWireCube(_patrol2, v);
    }

}