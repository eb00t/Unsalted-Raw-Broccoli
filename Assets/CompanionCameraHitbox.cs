using System;
using System.Collections;
using UnityEngine;

public class CompanionCameraHitbox : MonoBehaviour
{
    public CompanionCameraAI companionCameraAI;
    public Transform target;
    private Rigidbody _rb;
    public float speed;
    public float rotSpeed;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        StartCoroutine(SelfDestructAfterSeconds(15f));
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            var damageable = other.GetComponentInParent<IDamageable>();
            damageable.TakeDamage(companionCameraAI.attack, companionCameraAI.poiseDamage, Vector3.zero);
            Destroy(gameObject);
        }
        
        if (!other.GetComponent<SemiSolidPlatform>() && !other.GetComponent<SemiSolidPlatformTrigger>() && !other.CompareTag("Player") && !other.isTrigger && !other.GetComponent<CompanionCameraAI>())
        {
            Destroy(gameObject);
        }
    }
    
    private IEnumerator SelfDestructAfterSeconds(float seconds)
    {
        yield return new WaitForSecondsRealtime(seconds);
        Destroy(gameObject);
    }
    
    private void FixedUpdate()
    {
        if (target == null) return;
        
        var dir = (target.position - transform.position).normalized;
        var newDir = Vector3.RotateTowards(_rb.velocity.normalized, dir, rotSpeed * Mathf.Deg2Rad * Time.fixedDeltaTime, 10f);
        var angle = Mathf.Atan2(newDir.y, newDir.x) * Mathf.Rad2Deg;
        
        _rb.velocity = newDir * speed;
        _rb.rotation = Quaternion.Euler(0, 0, angle);
    }
}
