using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyTest : MonoBehaviour
{
    Vector3 initTransform;
    Quaternion initRotation;
    SpriteRenderer _renderer;

    void Start()
    {
        initTransform = transform.position;
        initRotation = transform.rotation;
        _renderer = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            transform.position = initTransform;
            transform.rotation = initRotation;
            gameObject.GetComponent<Rigidbody>().velocity = Vector3.zero;
            gameObject.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
        }

        if(_renderer.color == Color.red)
        {
            Invoke(nameof(ChangeColor), .4f);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("PlayerAttackBox"))
        {
            _renderer.color = Color.red;
        }
    }

    private void ChangeColor()
    {
        _renderer.color = Color.white;
    }
}
