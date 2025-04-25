using System;
using System.Collections;
using System.Collections.Generic;
using FMOD.Studio;
using Unity.VisualScripting;
using UnityEngine;

public class LaserObjectHandler : MonoBehaviour
{
    private Transform _player;
    [SerializeField] private int attack, poiseDamage;
    [SerializeField] private float chargeTime;
    [SerializeField] private float fireTime;
    [SerializeField] private float trackSpeed;
    [SerializeField] private float delay;
    [SerializeField] private float laserTickCooldown;
    [SerializeField] private float attackCooldown;
    private float _timer;
    private bool _canAttack = true;
    private EventInstance _laserEvent;
    private LineRenderer _lineRenderer;
    [SerializeField] private Transform bossEyePosition;
    private RoomScripting _roomScripting;
    private Collider _roomBounds;

    private void Start()
    {
        _player = GameObject.FindGameObjectWithTag("Player").transform;
        _roomScripting = gameObject.transform.root.GetComponent<RoomScripting>();
        _roomBounds = _roomScripting.GetComponent<Collider>();
        _lineRenderer = GetComponentInChildren<LineRenderer>();
        _timer = attackCooldown / 2;
    }

    private void Update()
    {
        if (IsPlayerInRoom() && !LevelBuilder.Instance.bossDead)
        {
            _timer -= Time.deltaTime;

            if (_timer <= 0f && _canAttack)
            {
                StartCoroutine(LaserAttack());
            }
        }
    }

    private bool IsPlayerInRoom()
    {
        return _roomBounds != null && _roomBounds.bounds.Contains(_player.position);
    }

    private IEnumerator LaserAttack() // aims laser at player that tracks, then it stops and starts doing damage
    {
        _laserEvent = AudioManager.Instance.CreateEventInstance(FMODEvents.Instance.BossHandLaser);
        _canAttack = false;
        _lineRenderer.enabled = true;
        _lineRenderer.SetPosition(0, bossEyePosition.position);
        
        var targetPos = _player.position;
        var elapsed = 0f;

        AudioManager.Instance.AttachInstanceToGameObject(_laserEvent, gameObject.transform);
        _laserEvent.start();
        _lineRenderer.SetPosition(1, targetPos);
        
        while (elapsed < chargeTime)
        {
            _lineRenderer.SetPosition(0, bossEyePosition.position); 
            _lineRenderer.startWidth = 0.01f;
            _lineRenderer.endWidth = 0.01f;
        
            elapsed += Time.deltaTime;
            yield return null;
        }

        yield return new WaitForSecondsRealtime(delay);
        
        elapsed = 0f;
        var lastDamageTime = 0f;
        
        AudioManager.Instance.SetEventParameter(_laserEvent, "Firing", 1);
        _laserEvent.release();
        while (elapsed < fireTime)
        {
            var dist = Vector3.Distance(targetPos, bossEyePosition.position);
            var direction = (targetPos - bossEyePosition.position).normalized;
            var laserEndPos = bossEyePosition.position + direction * (dist + 5f);

            _lineRenderer.SetPosition(0, bossEyePosition.position);
            _lineRenderer.SetPosition(1, laserEndPos);
            _lineRenderer.startWidth = 0.1f;
            _lineRenderer.endWidth = 0.05f;
            var layerMask = LayerMask.GetMask("Player");
            
            if (Physics.Raycast(bossEyePosition.position, direction, out var hit, 50f, layerMask))
            {
                if (hit.collider.CompareTag("Player"))
                {
                    var player = hit.collider.GetComponentInChildren<CharacterAttack>();
                    if (player != null)
                    {
                        if (Time.time >= lastDamageTime + laserTickCooldown) // makes sure player only takes damage at intervals
                        {
                            player.TakeDamagePlayer(attack, poiseDamage, Vector3.zero);
                            lastDamageTime = Time.time;
                        }
                    }
                }
            }
            
            elapsed += Time.deltaTime;
            yield return null;
        }

        _lineRenderer.enabled = false;
        _canAttack = true;
    }
}
