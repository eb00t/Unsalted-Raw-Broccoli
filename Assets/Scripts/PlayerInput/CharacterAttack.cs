using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class CharacterAttack : MonoBehaviour
{
    private MeshCollider _attackCollider;
    private Animator _playerAnimator;
    
    [Header("Stats")]
    [SerializeField] private int currentHealth = 100;
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private int charAtk = 10;
    [SerializeField] private float atkRange = 3;
    
    [Header("Tracking")]
    [SerializeField] private List<EnemyHandler> enemyHandlers;
    [SerializeField] private EnemyHandler _nearestEnemy;
    
    [SerializeField] private Slider healthSlider;
    
    private void Start()
    {
        _attackCollider = GetComponent<MeshCollider>();
        _playerAnimator = GameObject.FindGameObjectWithTag("PlayerRenderer").GetComponent<Animator>();
    }

    public void LightAttack(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            Debug.Log("LightAttack");
            _playerAnimator.SetBool("LightAttack1", true);
            InitiateAttack();
        }
    }

    public void LightAttack1(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            Debug.Log("LightAttack1");
            _playerAnimator.SetBool("LightAttack2", true);
            InitiateAttack();
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
        if (currentHealth - damage > 0)
        {
            currentHealth -= damage;
            healthSlider.value = currentHealth;
        }
        else
        {
            currentHealth = 0;
            healthSlider.value = 0;
            //Die();
        }
    }

    private void InitiateAttack()
    {
        CountEnemies();

        if (enemyHandlers.Count > 0)
        {
            foreach (var eh in enemyHandlers)
            {
                var dist = Vector3.Distance(transform.position, eh.transform.position);
                
                
                if (_nearestEnemy == null)
                {
                    _nearestEnemy = eh;
                }
                else if (dist <= Vector3.Distance(transform.position, _nearestEnemy.transform.position))
                {
                    _nearestEnemy = eh;
                }
            }
        }

        var dist1 = Vector3.Distance(transform.position, _nearestEnemy.transform.position);

        if (dist1 <= atkRange)
        {
            _nearestEnemy.TakeDamageEnemy(charAtk);
        }
    }

    private void CountEnemies()
    {
        enemyHandlers.Clear();
        
        foreach (var e in GameObject.FindGameObjectsWithTag("Enemy"))
        {
            if (e.GetComponent<EnemyHandler>())
            {
                enemyHandlers.Add(e.GetComponent<EnemyHandler>());
            }
        }
    }
}
