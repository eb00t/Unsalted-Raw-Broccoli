using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class ResizeBoxCollider : MonoBehaviour
{
    private BoxCollider _collider;
    private RoomInfo _roomInfo;
    private ConnectorRoomInfo _connectorRoomInfo;
    public enum RoomType
    {
        Room,
        Connector,
    }
    public RoomType roomType;
    private void Awake()
    {
        _collider = GetComponent<BoxCollider>();
        _collider.enabled = true;
        if (roomType == RoomType.Connector)
        {
            _connectorRoomInfo = transform.root.gameObject.GetComponent<ConnectorRoomInfo>();
            var colliderSize = _collider.size;
            colliderSize.x = _connectorRoomInfo.connectorLength;
            colliderSize.y = _connectorRoomInfo.connectorHeight;
            _collider.size = colliderSize;
        }
        else
        {
            _roomInfo = transform.root.gameObject.GetComponent<RoomInfo>();
            var colliderSize = _collider.size;
            colliderSize.x = _roomInfo.roomLength;
            colliderSize.y = _roomInfo.roomHeight;
            _collider.size = colliderSize;
        }
       
    }
}
