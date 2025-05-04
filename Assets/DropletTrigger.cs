using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DropletTrigger : MonoBehaviour
{
    private static readonly int Splash = Animator.StringToHash("Splash");
    private Rigidbody _rb;
    private Animator _animator;
    private DropletSpawner _dropletSpawner;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _animator = GetComponent<Animator>();
        _dropletSpawner = GetComponentInParent<DropletSpawner>();
        StartCoroutine(DestroyAfterDelay(10f));
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.layer == 10)
        {
            _rb.velocity = Vector3.zero;
            _rb.isKinematic = true;
            _animator.SetTrigger(Splash);
        }
    }

    private void SpawnAnotherDroplet()
    {
        _dropletSpawner.canSpawn = true;
    }

    private void DestroyDroplet()
    {
        StopAllCoroutines();
        Destroy(gameObject);
    }

    private IEnumerator DestroyAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        DestroyDroplet();
    }
}
