using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class ResizeBoxCollider : MonoBehaviour
{
    private BoxCollider _collider;
    private RoomInfo _roomInfo;
    private RoomScripting _roomScripting;
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
            colliderSize.x = _roomInfo.roomLength + 1.5f;
            colliderSize.y = _roomInfo.roomHeight + 1.5f;
            _collider.size = colliderSize;
        }
       
    }

    void Start()
    {
        _roomScripting = GetComponent<RoomScripting>();
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player") && roomType == RoomType.Room)
        {
            _roomScripting.playerIsInRoom = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && roomType == RoomType.Room)
        {
            _roomScripting.playerIsInRoom = false;
        }
    }
}
