using System;
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
    private Animator _animator;
    private States _state =  States.Idle;
    private Vector3 _patrol1, _patrol2;
    [SerializeField] private bool _isIdle;
    
    private float targetTime;
    

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

        switch (_state)
        {
            case States.Idle:
            {
                if (distance < chaseRange && _isIdle)
                {
                    _state = States.Chase;
                }
                
                // play idle animation / add patrolling 
                break;
            }
            case States.Patrol:
                if (distance < chaseRange && !_isIdle)
                {
                    _state = States.Patrol;
                }
                
                var dist1 = Vector3.Distance(transform.position, _patrol1);
                var dist2 = Vector3.Distance(transform.position, _patrol2);
                    
                if (dist1 > dist2)
                {
                    if (_patrol1.x > transform.position.x)
                    {
                        transform.Translate(transform.right * (speed * Time.deltaTime));
                        transform.rotation = Quaternion.Euler(0, 180, 0);
                    }
                    else
                    {
                        transform.Translate(-transform.right * (speed * Time.deltaTime));
                        transform.rotation = Quaternion.identity;
                    }
                }
                else
                {
                    if (_patrol2.x > transform.position.x)
                    {
                        transform.Translate(transform.right * (speed * Time.deltaTime));
                        transform.rotation = Quaternion.Euler(0, 180, 0);
                    }
                    else
                    {
                        transform.Translate(-transform.right * (speed * Time.deltaTime));
                        transform.rotation = Quaternion.identity;
                    }
                }

                break;
            case States.Chase:
            {
                //animator.SetTrigger("chase");
                //animator.SetBool("isAttacking", false);

                if (distance < attackRange)
                {
                    _state = States.Attack;
                }
                
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

                if (distance > attackRange)
                {
                    _state = States.Chase;
                }
                
                targetTime -= Time.deltaTime;
                
                if (targetTime <= 0.0f)
                {
                    _characterAttack.TakeDamagePlayer(enemyAtk);
                    targetTime = 2f;
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