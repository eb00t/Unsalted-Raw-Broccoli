using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FacePlayer : MonoBehaviour
{
    [SerializeField] private bool isFacingRight;
    [SerializeField] private float facePlayerDistance;
    [SerializeField] private float flipDelay;
    private Transform _player;
    private Vector3 _originalScale;
    private bool _canFlip;
    private Coroutine _flipCoroutine;

    private void Awake()
    {
        _originalScale = transform.localScale;
        _player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    private void Update()
    {
        var playerDir = transform.position - _player.position;
        var playerDist = Vector3.Distance(transform.position, _player.position);

        if (playerDist > facePlayerDistance && _canFlip)
        {
            transform.localScale = _originalScale;
            return;
        }
        
        if (playerDir.x < 0 && _canFlip)
        {
            transform.localScale = !isFacingRight ? new Vector3(-_originalScale.x, _originalScale.y, _originalScale.z) : _originalScale;
            StartCoroutine(DelayFlip());
        }
        else if (playerDir.x >= 0 && _canFlip)
        {
            transform.localScale = !isFacingRight ? _originalScale : new Vector3(-_originalScale.x, _originalScale.y, _originalScale.z);
            StartCoroutine(DelayFlip());
        }
        else if (!_canFlip)
        {
            if (_flipCoroutine == null)
            {
                _flipCoroutine = StartCoroutine(DelayFlip());
            }
        }
    }

    private IEnumerator DelayFlip()
    {
        _canFlip = false;
        yield return new WaitForSeconds(flipDelay);
        _flipCoroutine = null;
        _canFlip = true;
    }
}
