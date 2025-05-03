using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosionHandler : MonoBehaviour
{
    private static readonly int Explode = Animator.StringToHash("Explode");
    private SpriteRenderer _spriteRenderer;
    private Animator _animator;
    private bool _destruct;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    public IEnumerator Detonate(float delay, float size, bool doesSelfDestruct)
    {
        _spriteRenderer.transform.localScale = new Vector3(size, size, size);
        _destruct = doesSelfDestruct;
        yield return new WaitForSeconds(delay);
        _animator.SetTrigger(Explode);
    }

    public void SelfDestruct()
    {
        if (_destruct)
        {
            Destroy(gameObject);
        }
    }
}
