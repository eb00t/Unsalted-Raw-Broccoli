using System;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Linq;
using System.Collections;

public class LockOnController : MonoBehaviour
{
    [SerializeField] private float lockOnRadius;
    [SerializeField] private float switchThreshold;
    [SerializeField] private float maxDistance;

    public Transform lockedTarget;
    public bool isAutoSwitchEnabled;
    private Vector2 _switchDirection;
    private Vector3 _originalLocalScale;
    private CharacterMovement _characterMovement;
    private bool _isSwitchingInProgress;
    
    
    private void Awake()
    {
        _characterMovement = GetComponent<CharacterMovement>();
    }

    public void ToggleLockOn(InputAction.CallbackContext context)
    {
        if (lockedTarget == null)
        {
            lockedTarget = FindNearestTarget();

            if (lockedTarget == null) return;

            UpdateTargetImg(lockedTarget.gameObject, true);
            _originalLocalScale = transform.localScale;
            _characterMovement.lockedOn = true;
            UpdateDir();
        }
        else
        {
            UpdateTargetImg(lockedTarget.gameObject, false);
            lockedTarget = null;
            _characterMovement.lockedOn = false;
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
            Debug.Log("Target switched");

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
        var direction = Mathf.Sign(lockedTarget.position.x - transform.position.x);
        transform.localScale = new Vector3(Mathf.Abs(_originalLocalScale.x) * direction, _originalLocalScale.y, _originalLocalScale.z);
    }

    // updates the X that shows up to signify which enemy is locked on to
    private void UpdateTargetImg(GameObject target, bool setActive)
    {
        foreach (var i in target.GetComponentsInChildren<SpriteRenderer>(true))
        {
            if (i.name == "LockOnImg")
            {
                i.gameObject.SetActive(setActive);
            }
        }
    }
    
    // if the player is not locked on this finds the closest enemy to the player and locks onto them
    private Transform FindNearestTarget()
    {
        var enemies = GameObject.FindGameObjectsWithTag("Enemy");
        var minDistance = Mathf.Infinity;
        Transform nearestTarget = null;

        foreach (var enemy in enemies)
        {
            if (enemy.GetComponent<IDamageable>() == null) continue;
            var distance = Vector3.Distance(transform.position, enemy.transform.position);
            
            if (!(distance < minDistance) || !(distance <= lockOnRadius) || enemy.GetComponent<IDamageable>().isDead) continue;
            
            minDistance = distance;
            nearestTarget = enemy.transform;
        }
        
        return nearestTarget;
    }

    // when switching direction this basically checks which enemy best fits the direction of the thumbstick input based on the current locked on enemy position
    private Transform FindTargetInDirection(Vector2 direction)
    {
        var origin = lockedTarget != null ? lockedTarget.position : transform.position;
        var enemies = GameObject.FindGameObjectsWithTag("Enemy");
        
        var validTargets = enemies.Select(e => e.transform)
            .Where(t => t != null 
                        && t != lockedTarget
                        && Vector3.Distance(transform.position, t.position) <= maxDistance
                        && Vector3.Distance(origin, t.position) <= lockOnRadius
                        && t.GetComponent<IDamageable>() != null 
                        && !t.GetComponent<IDamageable>().isDead)
            .ToList();

        if (validTargets.Count == 0) return null;

        Transform closestTarget = null;
        var closestDistance = Mathf.Infinity;

        foreach (var target in validTargets)
        {
            var toTarget = (target.position - origin).normalized;
            var projected = new Vector2(toTarget.x, toTarget.z);
            var angle = Vector2.Angle(projected, direction);
            
            if (angle > 45f) continue;

            var horizontalDistance = Vector2.Distance(new Vector2(origin.x, origin.z), new Vector2(target.position.x, target.position.z));

            if (!(horizontalDistance < closestDistance)) continue;
            
            closestDistance = horizontalDistance;
            closestTarget = target;
        }

        return closestTarget;
    }

    private void Update()
    {
        if (lockedTarget == null) return;

        var damageable = lockedTarget.GetComponent<IDamageable>();

        if (isAutoSwitchEnabled && damageable.isDead)
        {
            var nearestTarget = FindNearestTarget();
            if (nearestTarget != null)
            {
                lockedTarget = nearestTarget;
                UpdateTargetImg(lockedTarget.gameObject, true);
            }
        }
        else
        {
            UpdateDir();
        }

        if (Vector3.Distance(transform.position, lockedTarget.position) > maxDistance || damageable.isDead)
        {
            UpdateTargetImg(lockedTarget.gameObject, false);
            lockedTarget = null;
            _characterMovement.lockedOn = false;
            transform.localScale = _originalLocalScale;
        }
        else
        {
            UpdateDir();
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, lockOnRadius);
        Gizmos.DrawWireSphere(transform.position, maxDistance);
    }
}
