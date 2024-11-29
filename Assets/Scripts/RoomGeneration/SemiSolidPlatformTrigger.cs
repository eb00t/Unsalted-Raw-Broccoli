using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SemiSolidPlatformTrigger : MonoBehaviour
{
    public bool up;
    private SemiSolidPlatform _semiSolidPlatform;
    private BoxCollider _boxCollider;
    private void Awake()
    {
        _boxCollider = GetComponent<BoxCollider>();
        _semiSolidPlatform = transform.parent.GetComponent<SemiSolidPlatform>();
        /*var boxColliderSize = new Vector3(_semiSolidPlatform.gameObject.transform.localScale.x * 1.5f, _semiSolidPlatform.gameObject.transform.localScale.y, _semiSolidPlatform.gameObject.transform.localScale.z);
        _boxCollider.size = boxColliderSize;
        var boxColliderCenter = Vector3.zero;
        switch (up)
        {
            case false:
                 boxColliderCenter = new Vector3(_semiSolidPlatform.transform.position.x, _semiSolidPlatform.transform.position.y - (boxColliderSize.y + boxColliderSize.y /2) , _semiSolidPlatform.transform.position.z);
                break;
            case true:
                boxColliderCenter = new Vector3(_semiSolidPlatform.transform.position.x, _semiSolidPlatform.transform.position.y + (boxColliderSize.y + boxColliderSize.y /2) , _semiSolidPlatform.transform.position.z);
                break;
            default: boxColliderCenter = Vector3.zero;
                break;
        }
        _boxCollider.center = boxColliderCenter;*/
    }

    private void OnTriggerStay(Collider other)
    {
        if (Input.GetKeyDown(KeyCode.S))
        {
            _semiSolidPlatform.TurnOffCollision(LayerMask.GetMask("Player"));
        }
    }
}
