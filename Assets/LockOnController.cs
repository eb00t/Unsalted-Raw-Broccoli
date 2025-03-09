using UnityEngine;
using UnityEngine.InputSystem;
using System.Linq;

public class LockOnController : MonoBehaviour
{
    [SerializeField] private float lockOnRadius;
    [SerializeField] private float switchThreshold;
    [SerializeField] private float maxDistance;

    private Transform _lockedTarget;
    private Vector2 _switchDirection;
    private Vector3 _originalLocalScale;
    private CharacterMovement _characterMovement;

    private void Awake()
    {
        _characterMovement = GetComponent<CharacterMovement>();
    }

    public void ToggleLockOn(InputAction.CallbackContext context)
    {
        if (_lockedTarget == null)
        {
            _lockedTarget = FindNearestTarget();
            
            if (_lockedTarget == null) return;
            
            UpdateTargetImg(_lockedTarget.gameObject, true);
            _originalLocalScale = transform.localScale;
            _characterMovement.lockedOn = true;
            UpdateDir();
        }
        else
        {
            UpdateTargetImg(_lockedTarget.gameObject, false);
            _lockedTarget = null;
            _characterMovement.lockedOn = false;
            transform.localScale = _originalLocalScale;
        }
    }
    
    public void SwitchTarget(InputAction.CallbackContext context)
    {
        if (_lockedTarget == null) return;

        _switchDirection = context.ReadValue<Vector2>();
        if (_switchDirection.magnitude < switchThreshold) return;
        
        UpdateTargetImg(_lockedTarget.gameObject, false);

        var newTarget = FindTargetInDirection(_switchDirection);
        UpdateTargetImg(newTarget.gameObject, true);
        if (newTarget == _lockedTarget) return;
        
        _lockedTarget = newTarget;
        UpdateDir();
    }

    // this just updates the direction that the player is facing while locked on to the locked on target
    private void UpdateDir()
    {
        var direction = Mathf.Sign(_lockedTarget.position.x - transform.position.x);
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
            var distance = Vector3.Distance(transform.position, enemy.transform.position);
            
            if (!(distance < minDistance) || !(distance <= lockOnRadius) || !enemy.activeSelf) continue;
            
            minDistance = distance;
            nearestTarget = enemy.transform;
        }
        
        return nearestTarget;
    }

    // when switching direction this basically checks which enemy best fits the direction of the thumbstick input
    private Transform FindTargetInDirection(Vector2 direction)
    {
        var enemies = GameObject.FindGameObjectsWithTag("Enemy");
        var validTargets = enemies.Select(e => e.transform).Where(t => Vector3.Distance(transform.position, t.position) <= lockOnRadius).ToList();

        if (validTargets.Count == 0) return _lockedTarget;

        var bestTarget = _lockedTarget;
        var bestScore = -Mathf.Infinity;

        foreach (var target in validTargets)
        {
            var toTarget = (target.position - transform.position).normalized;
            var projected = new Vector2(toTarget.x, toTarget.z);
            var score = Vector2.Dot(projected.normalized, direction.normalized);

            if (!(score > bestScore) || !target.gameObject.activeSelf) continue;
            
            bestScore = score;
            bestTarget = target;
        }
        
        return bestTarget;
    }

    private void Update()
    {
        if (_lockedTarget == null) return;

        if (Vector3.Distance(transform.position, _lockedTarget.position) > maxDistance)
        {
            UpdateTargetImg(_lockedTarget.gameObject, false);
            _lockedTarget = null;
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
