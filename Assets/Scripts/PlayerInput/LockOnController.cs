using System;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Linq;
using System.Collections;

public class LockOnController : MonoBehaviour
{
    [SerializeField] private float switchThreshold;
    public Transform lockedTarget;
    public bool isAutoSwitchEnabled;
    public bool isAutoLockOnEnabled;
    private Vector2 _switchDirection;
    private CharacterMovement _characterMovement;
    private Vector3 _originalLocalScale;
    private bool _isSwitchingInProgress;
    private RoomScripting _roomScripting;
    private bool _canLock = true;
    
    private void Awake()
    {
        _characterMovement = GetComponent<CharacterMovement>();
        _originalLocalScale = transform.localScale;
    }

    private void Start()
    {
        FindCurrentRoom();
    }

    private void FindCurrentRoom()
    {
        foreach (var rs in FindObjectsOfType<RoomScripting>())
        {
            if (rs != null && rs.playerIsInRoom && _roomScripting != rs)
            {
                ClearLockOn();
                _roomScripting = rs;
                return;
            }
        }
    }

    public void ToggleLockOn(InputAction.CallbackContext context)
    {
        LockOn(false);
    }

    private void LockOn(bool isAuto)
    {
        _canLock = false;

        if (lockedTarget == null || isAuto)
        {
            var nearest = FindNearestTarget();

            if (nearest == null)
            {
                ClearLockOn();
                _canLock = true;
                return;
            }

            ClearIndicators();
            lockedTarget = nearest;
            UpdateTargetImg(lockedTarget.gameObject, true);
            _characterMovement.lockedOn = true;
            UpdateDir();
        }
        else
        {
            ClearLockOn();
        }

        _canLock = true;
    }

    public void SwitchTarget(InputAction.CallbackContext context)
    {
        if (lockedTarget == null) return;

        if (context.performed)
        {
            _switchDirection = context.ReadValue<Vector2>();

            if (!(_switchDirection.magnitude >= switchThreshold)) return;
            if (_isSwitchingInProgress) return;

            _isSwitchingInProgress = true;
            
            var newTarget = FindTargetInDirection(_switchDirection);

            if (newTarget == lockedTarget || newTarget == null) return;

            ClearIndicators();
            lockedTarget = newTarget;
            UpdateTargetImg(lockedTarget.gameObject, true);

            UpdateDir();
        }
        else if (context.canceled)
        {
            _isSwitchingInProgress = false;
        }
    }
    
    private void ClearLockOn()
    {
        if (lockedTarget != null)
        {
            UpdateTargetImg(lockedTarget.gameObject, false);
        }

        lockedTarget = null;
        _characterMovement.lockedOn = false;
    }
    
    private void ClearIndicators()
    {
        foreach (var target in GameObject.FindGameObjectsWithTag("LockOnPoint"))
        {
            var dmg = target.GetComponentInParent<IDamageable>();
            if (dmg == null || dmg.isDead) continue;

            var sprite = target.GetComponentInChildren<SpriteRenderer>(true);
            if (sprite != null) sprite.gameObject.SetActive(false);

            var animator = target.GetComponent<Animator>();
            if (animator != null) animator.enabled = false;
        }
    }
    
    // this just updates the direction that the player is facing while locked on to the locked on target
    private void UpdateDir()
    {
        if (lockedTarget == null) return;
        // if (MathF.Abs(_characterMovement.Velocity.x) > 0.1f) return;
        var direction = Mathf.Sign(lockedTarget.position.x - transform.position.x);
        transform.localScale = new Vector3(Mathf.Abs(_originalLocalScale.x) * direction, _originalLocalScale.y, _originalLocalScale.z);
    }

    // updates the X that shows up to signify which enemy is locked on to
    private void UpdateTargetImg(GameObject target, bool setActive)
    {
        if (!target.activeSelf) return;
        
        target.GetComponent<Animator>().enabled = setActive;
        target.GetComponentInChildren<SpriteRenderer>(true).gameObject.SetActive(setActive);
    }
    
    // if the player is not locked on this finds the closest enemy to the player and locks onto them
    private Transform FindNearestTarget()
    {
        var enemies = GameObject.FindGameObjectsWithTag("LockOnPoint");
        var minDistance = Mathf.Infinity;
        Transform nearestTarget = null;

        foreach (var enemy in enemies)
        {
            var dmg = enemy.GetComponentInParent<IDamageable>();
            if (dmg == null || dmg.isDead) continue;

            var distance = Vector3.Distance(transform.position, enemy.transform.position);
            FindCurrentRoom();

            if (distance < minDistance && enemy.transform.root.GetComponent<RoomScripting>() == _roomScripting)
            {
                minDistance = distance;
                nearestTarget = enemy.transform;
            }
        }

        return nearestTarget;
    }

    // when switching direction this basically checks which enemy best fits the direction of the thumbstick input based on the current locked on enemy position
    private Transform FindTargetInDirection(Vector2 direction)
    {
        var origin = lockedTarget != null ? lockedTarget.position : transform.position;
        var enemies = GameObject.FindGameObjectsWithTag("LockOnPoint");
        FindCurrentRoom();
        
        var validTargets = enemies.Select(e => e.transform)
            .Where(t => t != null 
                        && t != lockedTarget
                        && t.gameObject.transform.root.GetComponent<RoomScripting>() == _roomScripting
                        && t.GetComponentInParent<IDamageable>() != null 
                        && !t.GetComponentInParent<IDamageable>().isDead)
            .ToList();

        if (validTargets.Count == 0) return null;

        Transform closestTarget = null;
        var closestDistance = Mathf.Infinity;

        foreach (var target in validTargets)
        {
            var toTarget = (target.position - origin).normalized;
            var projected = new Vector2(toTarget.x, toTarget.y);
            var angle = Vector2.Angle(projected, direction);
            
            if (angle > 45f) continue;

            var horizontalDistance = Vector2.Distance(new Vector2(origin.x, origin.y), new Vector2(target.position.x, target.position.y));

            if (!(horizontalDistance < closestDistance)) continue;
            
            closestDistance = horizontalDistance;
            closestTarget = target;
        }

        return closestTarget;
    }

    private void Update()
    {
        if (isAutoLockOnEnabled && _canLock)
        {
            FindCurrentRoom();
            
            if (_roomScripting != null)
            {
               LockOn(true); 
            }
        }

        if (lockedTarget == null)
        {
            if (_characterMovement.lockedOn)
            {
                ClearLockOn();
            }

            return;
        }

        var damageable = lockedTarget.GetComponentInParent<IDamageable>();
        
        if (damageable == null)
        {
            ClearLockOn();
            return;
        }

        if (isAutoSwitchEnabled && damageable.isDead)
        {
            UpdateTargetImg(lockedTarget.gameObject, false);
            var nearestTarget = FindNearestTarget();

            if (nearestTarget != null && nearestTarget != lockedTarget)
            {
                lockedTarget = nearestTarget;
                UpdateTargetImg(lockedTarget.gameObject, true);
                _characterMovement.lockedOn = true;
                UpdateDir();
                
                damageable = lockedTarget.GetComponentInParent<IDamageable>();
            }
            else
            {
                ClearLockOn();
                return;
            }
        }
        
        FindCurrentRoom();
        
        if (lockedTarget.transform.root.GetComponent<RoomScripting>() != _roomScripting || damageable.isDead)
        {
            ClearLockOn();
            return;
        }

        UpdateDir();
    }
}
