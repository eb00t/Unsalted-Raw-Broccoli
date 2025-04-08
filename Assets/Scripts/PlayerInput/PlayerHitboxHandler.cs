using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHitboxHandler : MonoBehaviour
{
    [SerializeField] private CharacterAttack characterAttack;
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Enemy")) return;

        characterAttack.AttackHit(other);
    }
}
