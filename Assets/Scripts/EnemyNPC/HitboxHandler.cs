using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Serialization;

public class HitboxHandler : MonoBehaviour
{
    [SerializeField] private EnemyType state =  EnemyType.Enemy;
    
    private enum EnemyType
    {
      Enemy,
      TwoHands
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (state == EnemyType.Enemy)
        {
            if (!other.CompareTag("Player")) return;
            Debug.Log("player hit");
            var characterAttack = other.GetComponentInChildren<CharacterAttack>();
            var atk = GetComponentInParent<EnemyHandler>().enemyAtk;
            characterAttack.TakeDamagePlayer(atk);
        }
        else if (state == EnemyType.TwoHands)
        {
            if (!other.CompareTag("Player")) return;
            Debug.Log("player hit");
            var characterAttack = other.GetComponentInChildren<CharacterAttack>();
            var atk = GetComponentInParent<Boss_TwoHands>().bossAtk;
            characterAttack.TakeDamagePlayer(atk);
        }
    }
}
