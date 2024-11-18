using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResizeBoxCollider : MonoBehaviour
{
    private BoxCollider _collider;
    private RoomInfo _roomInfo;
    private void Awake()
    { 
        _roomInfo = transform.root.gameObject.GetComponent<RoomInfo>();
        _collider = GetComponent<BoxCollider>();
        _collider.enabled = true;
        var colliderSize = _collider.size;
        colliderSize.x = _roomInfo.roomLength;
        colliderSize.y = _roomInfo.roomHeight;
        _collider.size = colliderSize;
    }
}
