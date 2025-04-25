using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FacePlayer : MonoBehaviour
{
    [SerializeField] private bool isFacingRight;
    [SerializeField] private float facePlayerDistance;
    private Transform _player;
    private Vector3 _originalScale;

    private void Awake()
    {
        _originalScale = transform.localScale;
        _player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    private void Update()
    {
        var playerDir = transform.position - _player.position;
        var playerDist = Vector3.Distance(transform.position, _player.position);

        if (!(playerDist <= facePlayerDistance))
        {
            transform.localScale = _originalScale;
            return;
        }
        
        if (playerDir.x < 0)
        {
            transform.localScale = !isFacingRight ? new Vector3(-_originalScale.x, _originalScale.y, _originalScale.z) : _originalScale;
        }
        else if (playerDir.x >= 0)
        {
            transform.localScale = !isFacingRight ? _originalScale : new Vector3(-_originalScale.x, _originalScale.y, _originalScale.z);
        }
    }
}
