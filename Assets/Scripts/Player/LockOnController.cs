using System;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Linq;
using System.Collections;

public class LockOnController : MonoBehaviour // TODO: Make toggle states load from dataholder
{
    [SerializeField] private float manualLockOnDist;
    [SerializeField] private float autoLockOnDist;
    [SerializeField] private float autoSwitchDist;
    [SerializeField] private float maxLockOnDist;
    [SerializeField] private float switchThreshold;
    [SerializeField] private DataHolder dataHolder;
    public Transform lockedTarget;
    private Vector2 _switchDirection;
    private Vector3 _originalLocalScale;
    private CharacterMovement _characterMovement;
    private bool _isSwitchingInProgress;
    public bool isNearBoss;

    [Header("Debugging")]
    [SerializeField] private bool visualiseManualLockOn;
    [SerializeField] private bool visualiseAutoLockOn;
    [SerializeField] private bool visualiseAutoSwitch;
    [SerializeField] private bool visualiseMaxLockOn;
    
    private void Awake()
    {
        _characterMovement = GetComponent<CharacterMovement>();
    }

    public void ToggleLockOn(InputAction.CallbackContext context)
    {
        LockOn(false);
    }

    private void LockOn(bool isAuto)
    {
        if (lockedTarget == null)
        {
            if (dataHolder.isAutoLockOnEnabled && isAuto)
            {
                lockedTarget = isNearBoss ? FindNearestTarget(autoLockOnDist * 2) : FindNearestTarget(autoLockOnDist);
            }
            else
            {
                lockedTarget = FindNearestTarget(manualLockOnDist);
            }

            if (lockedTarget == null) return;

            UpdateTargetImg(lockedTarget.gameObject, true);
            _originalLocalScale = transform.localScale;
            _characterMovement.LockedOn = true;
            UpdateDir();
        }
        else if (!isAuto)
        {
            UpdateTargetImg(lockedTarget.gameObject, false);
            lockedTarget = null;
            _characterMovement.LockedOn = false;
            transform.localScale = _originalLocalScale;
        }
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

            UpdateTargetImg(lockedTarget.gameObject, false);
            lockedTarget = newTarget;
            UpdateTargetImg(lockedTarget.gameObject, true);

            UpdateDir();
        }
        else if (context.canceled)
        {
            _isSwitchingInProgress = false;
        }
    }
    
    // this just updates the direction that the player is facing while locked on to the locked on target
    private void UpdateDir()
    {
        if (lockedTarget == null || _characterMovement.uiOpen) return;
        // if (MathF.Abs(_characterMovement.Velocity.x) > 0.1f) return;
        var direction = Mathf.Sign(lockedTarget.position.x - transform.position.x);
        transform.localScale = new Vector3(Mathf.Abs(_originalLocalScale.x) * direction, _originalLocalScale.y, _originalLocalScale.z);
    }

    // updates the X that shows up to signify which enemy is locked on to
    private void UpdateTargetImg(GameObject target, bool setActive)
    {
        target.GetComponent<Animator>().enabled = setActive;
        target.GetComponentInChildren<SpriteRenderer>(true).gameObject.SetActive(setActive);
    }
    
    // if the player is not locked on this finds the closest enemy to the player and locks onto them
    public Transform FindNearestTarget(float radius)
    {
        var points = GameObject.FindGameObjectsWithTag("LockOnPoint");
        var minDistance = Mathf.Infinity;
        Transform nearestTarget = null;

        foreach (var point in points)
        {
            var dmg = point.GetComponentInParent<IDamageable>();
            if (dmg == null || dmg.isDead) continue;

            var distance = Vector3.Distance(transform.position, point.transform.position);

            if (distance < minDistance && distance <= radius)
            {
                minDistance = distance;
                nearestTarget = point.transform;
            }
        }

        return nearestTarget;
    }

    // when switching direction this basically checks which enemy best fits the direction of the thumbstick input based on the current locked on enemy position
    private Transform FindTargetInDirection(Vector2 direction)
    {
        var origin = lockedTarget != null ? lockedTarget.position : transform.position;
        var points   = GameObject.FindGameObjectsWithTag("LockOnPoint");
        
        var validTargets = points.Select(e => e.transform)
            .Where(t => t != null 
                        && t != lockedTarget
                        && Vector3.Distance(transform.position, t.position) <= maxLockOnDist
                        && Vector3.Distance(origin, t.position) <= manualLockOnDist
                        && t.GetComponentInParent<IDamageable>() != null 
                        && !t.GetComponentInParent<IDamageable>().isDead)
            .ToList();

        if (validTargets.Count == 0) return null;

        Transform closestTarget = null;
        var closestDistance = Mathf.Infinity;

        foreach (var point in validTargets)
        {
            var toTarget = (point.position - origin).normalized;
            var projected = new Vector2(toTarget.x, toTarget.y);
            var angle = Vector2.Angle(projected, direction);
            
            if (angle > 45f) continue;

            var horizontalDistance = Vector2.Distance(new Vector2(origin.x, origin.y), new Vector2(point.position.x, point.position.y));

            if (!(horizontalDistance < closestDistance)) continue;
            
            closestDistance = horizontalDistance;
            closestTarget = point.transform;
        }

        return closestTarget;
    }

    private void Update()
    {
        if (lockedTarget == null)
        {
            if (dataHolder.isAutoLockOnEnabled)
            {
                LockOn(true);
            }
            return;
        }

        var damageable = lockedTarget.GetComponentInParent<IDamageable>();
        
        if (damageable == null)
        {
            lockedTarget = null;
            _characterMovement.LockedOn = false;
            return;
        }

        if (dataHolder.isAutoSwitchEnabled && damageable.isDead)
        {
            UpdateTargetImg(lockedTarget.gameObject, false);
            var nearestTarget = FindNearestTarget(autoSwitchDist);

            if (nearestTarget != null && nearestTarget != lockedTarget)
            {
                lockedTarget = nearestTarget;
                UpdateTargetImg(lockedTarget.gameObject, true);
                _characterMovement.LockedOn = true;
                UpdateDir();
                
                damageable = lockedTarget.GetComponentInParent<IDamageable>();
            }
            else
            {
                lockedTarget = null;
                _characterMovement.LockedOn = false;
                return;
            }
        }

        var unlockDist = dataHolder.isAutoLockOnEnabled ? autoLockOnDist : manualLockOnDist;
        
        if (dataHolder.isAutoLockOnEnabled)
        {
            unlockDist = isNearBoss ? autoLockOnDist * 2 : autoLockOnDist;
        }

        if (Vector3.Distance(transform.position, lockedTarget.position) > unlockDist || damageable.isDead)
        {
            UpdateTargetImg(lockedTarget.gameObject, false);
            lockedTarget = null;
            _characterMovement.LockedOn = false;
            return;
        }

        UpdateDir();
    }

    private void OnDrawGizmos()
    {
        if (visualiseManualLockOn) { Gizmos.color = Color.green; Gizmos.DrawWireSphere(transform.position, manualLockOnDist); }
        if (visualiseAutoSwitch) { Gizmos.color = Color.yellow; Gizmos.DrawWireSphere(transform.position, autoSwitchDist); }
        if (visualiseAutoLockOn) { Gizmos.color = Color.magenta; Gizmos.DrawWireSphere(transform.position, autoLockOnDist); }
        if (visualiseMaxLockOn) { Gizmos.color = Color.red; Gizmos.DrawWireSphere(transform.position, maxLockOnDist); }
    }
}
