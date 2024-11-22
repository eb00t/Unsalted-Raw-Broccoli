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
    
    [Header("Tracking")]
    [SerializeField] private List<EnemyHandler> enemyHandlers;
    private EnemyHandler _nearestEnemy;
    
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
        }
    }

    public void LightAttack1(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            Debug.Log("LightAttack1");
            _playerAnimator.SetBool("LightAttack2", true);
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
}
