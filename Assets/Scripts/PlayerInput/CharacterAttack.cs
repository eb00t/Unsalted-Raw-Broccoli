using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class CharacterAttack : MonoBehaviour
{
    private MeshCollider _attackCollider;
    private Animator _playerAnimator;
    
    [Header("Stats")]
    private int _currentHealth = 100;
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private int charAtk = 10;
    [SerializeField] private float atkRange = 3;
    
    [Header("Tracking")]
    [SerializeField] private List<Transform> enemies;
    [SerializeField] private Transform nearestEnemy;
    
    
    [SerializeField] private Slider healthSlider;
    
    private void Start()
    {
        _attackCollider = GetComponent<MeshCollider>();
        _playerAnimator = GameObject.FindGameObjectWithTag("PlayerRenderer").GetComponent<Animator>();
        healthSlider.maxValue = maxHealth;
        healthSlider.value = maxHealth;
        _currentHealth = maxHealth;
    }

    public void LightAttack(InputAction.CallbackContext ctx)
    {
        if (ctx.performed && _playerAnimator.GetBool("Grounded"))
        {
            Debug.Log("LightAttack");
            _playerAnimator.SetBool("LightAttack1", true);
            //InitiateAttack();
        }
    }

    public void LightAttack1(InputAction.CallbackContext ctx)
    {
        if (ctx.performed && _playerAnimator.GetBool("Grounded"))
        {
            Debug.Log("LightAttack1");
            _playerAnimator.SetBool("LightAttack2", true);
            //InitiateAttack();
        }
    }

    public void DisableCollider()
    {
        _attackCollider.enabled = false;
    }

    public void EnableCollider()
    {
        _attackCollider.enabled = true;
        Debug.Log("Enable collider");
    }

    public void TakeDamagePlayer(int damage)
    {
        if (_currentHealth - damage > 0)
        {
            _currentHealth -= damage;
            healthSlider.value = _currentHealth;
        }
        else
        {
            _currentHealth = 0;
            healthSlider.value = 0;
            //Die();
        }
    }

    /*
    private void InitiateAttack()
    {
        CountEnemies();

        if (enemies.Count > 0)
        {
            foreach (var eh in enemies)
            {
                var dist = Vector3.Distance(transform.position, eh.transform.position);
                
                
                if (nearestEnemy == null)
                {
                    nearestEnemy = eh;
                }
                else if (dist <= Vector3.Distance(transform.position, nearestEnemy.transform.position))
                {
                    nearestEnemy = eh;
                }
            }
        }

        var dist1 = Vector3.Distance(transform.position, nearestEnemy.transform.position);

        if (dist1 <= atkRange)
        {
            if (nearestEnemy.GetComponent<EnemyHandler>())
            {
                nearestEnemy.GetComponent<EnemyHandler>().TakeDamageEnemy(charAtk);
            }
            else if (nearestEnemy.GetComponent<BossHandler>())
            {
                nearestEnemy.GetComponent<BossHandler>().TakeDamageEnemy(charAtk);
            }
            else if (nearestEnemy.GetComponent<Boss_TwoHands>())
            {
                nearestEnemy.GetComponent<Boss_TwoHands>().TakeDamageEnemy(charAtk);
            }
        }
    }
    */

    /*
    private void CountEnemies()
    {
        enemies.Clear();
        
        foreach (var e in GameObject.FindGameObjectsWithTag("Enemy"))
        {
            if (e.GetComponent<EnemyHandler>() || e.GetComponent<BossHandler>() || e.GetComponent<Boss_TwoHands>())
            {
                enemies.Add(e.transform);
            }
        }
    }
    */

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            if (other.GetComponentInParent<EnemyHandler>())
            {
                other.GetComponentInParent<EnemyHandler>().TakeDamageEnemy(charAtk);
            }
            if (other.GetComponent<BossHandler>())
            {
                other.GetComponent<BossHandler>().TakeDamageEnemy(charAtk);
            }
            if (other.GetComponentInParent<Boss_TwoHands>())
            {
                other.GetComponentInParent<Boss_TwoHands>().TakeDamageEnemy(charAtk);
            }
            Debug.Log("EnemyHit");
        }
    }
}
