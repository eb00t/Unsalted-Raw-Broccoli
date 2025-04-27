using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandHitFlash : MonoBehaviour
{
    private Boss2Hands _boss2Hands;
    [SerializeField] private SpriteRenderer spriteRenderer1, spriteRenderer2;

    private void Awake()
    {
        _boss2Hands = GetComponentInParent<Boss2Hands>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("PlayerAttackBox"))
        {
            StartCoroutine(_boss2Hands.HitFlash(spriteRenderer1));
            StartCoroutine(_boss2Hands.HitFlash(spriteRenderer2));
        }
    }
}
