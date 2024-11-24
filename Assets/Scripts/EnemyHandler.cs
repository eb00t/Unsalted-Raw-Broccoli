using System;
using UnityEngine;
using UnityEngine.UI;


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
    
    [Header("References")]
    //[SerializeField] private Slider healthSlider; // if enemy is boss set this to HUD slider
    private CharacterAttack _characterAttack;
    
    private Transform _target;
    private Animator _animator;
    private States _state =  States.Idle;
    
    // temp timer
    public float targetTime;
    

    private enum States
    {
        Idle,
        Chase,
        Attack
    }

    private void Start()
    {
        //healthSlider.maxValue = maxHealth;
        //healthSlider.value = health;
        _target = GameObject.FindGameObjectWithTag("Player").transform;
        _characterAttack = _target.GetComponentInChildren<CharacterAttack>();
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
                if (distance < chaseRange)
                {
                    _state = States.Chase;
                }
                
                // play idle animation / add patrolling 
                break;
            }
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
    
    public void TakeDamageEnemy(int damage)
    {
        if (health - damage > 0)
        {
            health -= damage;
            //healthSlider.value = health;
        }
        else
        {
            health = 0;
            //healthSlider.value = 0;
            Die();
        }
    }
    
    private void Die()
    {
        //animator.SetTrigger("isDead");
        
        GetComponent<CapsuleCollider>().enabled = false;
        gameObject.SetActive(false);
        enabled = false;
    }
    
    
    private void OnDrawGizmosSelected()
    {
        var position = transform.position;
        
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(position, attackRange);
        
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(position, chaseRange);
    }

}