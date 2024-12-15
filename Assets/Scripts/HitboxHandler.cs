using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class HitboxHandler : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        Debug.Log("player hit");
        var characterAttack = other.GetComponentInChildren<CharacterAttack>();
        var atk = GetComponentInParent<Boss_TwoHands>().bossAtk;
        characterAttack.TakeDamagePlayer(atk);
    }
}
