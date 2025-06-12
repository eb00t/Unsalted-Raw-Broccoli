using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ExplosionHandler : MonoBehaviour
{
    private static readonly int Explode = Animator.StringToHash("Explode");
    private SpriteRenderer _spriteRenderer;
    private Animator _animator;
    private bool _destruct;
    private CharacterAttack _characterAttack;
    private GameObject _player;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        _player = GameObject.FindGameObjectWithTag("Player");
    }

    public IEnumerator Detonate(float delay, float size, bool doesSelfDestruct)
    {
        _spriteRenderer.transform.localScale = new Vector3(size, size, size);
        _destruct = doesSelfDestruct;
        yield return new WaitForSeconds(delay);
        _animator.SetTrigger(Explode);
        var dist = Vector3.Distance(transform.position, _player.transform.position);
        if (dist <= 3f)
        {
            StartCoroutine(TimedVibration(1f, 1f, .75f));
        }
        else
        {
            StartCoroutine(TimedVibration(.75f, .75f, .75f));
        }
    }
    
    private IEnumerator TimedVibration(float lSpeed, float hSpeed, float duration)
    {
        Gamepad.current.SetMotorSpeeds(lSpeed, hSpeed);
        yield return new WaitForSecondsRealtime(duration);
        InputSystem.ResetHaptics();
    }

    public void SelfDestruct()
    {
        if (_destruct)
        {
            Destroy(gameObject);
        }
    }
}
