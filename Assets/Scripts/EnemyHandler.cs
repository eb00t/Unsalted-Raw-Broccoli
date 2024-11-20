using System;
using UnityEngine;
using UnityEngine.UI;


// Adapted from https://github.com/Chaker-Gamra/2.5D-Platformer-Game/blob/master/Assets/Scripts/Enemy/Enemy.cs
public class EnemyHandler : MonoBehaviour
{
    [Header("Enemy Stats")]
    [SerializeField] private int health = 100;
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private float chaseRange = 5;
    [SerializeField] private float attackRange = 2;
    [SerializeField] private float speed = 3;
    
    [Header("References")]
    [SerializeField] private Slider healthSlider; // if enemy is boss set this to HUD slider
    
    private Transform _target;
    private Animator _animator;
    private States _state =  States.Idle;

    private enum States
    {
        Idle,
        Chase,
        Attack
    }

    private void Start()
    {
        healthSlider.maxValue = maxHealth;
        healthSlider.value = health;
        _target = GameObject.FindGameObjectWithTag("Player").transform;
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
                break;
            }
            case States.Chase:
            {
                //play the run animation
                //animator.SetTrigger("chase");
                //animator.SetBool("isAttacking", false);

                if (distance < attackRange)
                {
                    _state = States.Attack;
                }

                //move towards the player
                if (_target.position.x > transform.position.x)
                {
                    //move right
                    transform.Translate(transform.right * (speed * Time.deltaTime));
                    transform.rotation = Quaternion.Euler(0, 180, 0);
                }
                else
                {
                    //move left
                    transform.Translate(-transform.right * (speed * Time.deltaTime));
                    transform.rotation = Quaternion.identity;
                }
                break;
            }
            case States.Attack:
            {
                //animator.SetBool("isAttacking", true);

                if (distance > attackRange)
                    _state = States.Chase;
                break;
            }
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
    
    public void TakeDamage(int damage)
    {
        if (health - damage > 0)
        {
            health -= damage;
            healthSlider.value = health;
        }
        else
        {
            healthSlider.value = 0;
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