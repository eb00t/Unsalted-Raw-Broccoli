using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(BoxCollider))]
public class ResizeBoxCollider : MonoBehaviour
{
    private BoxCollider _collider;
    private RoomInfo _roomInfo;
    private RoomScripting _roomScripting;
    private ConnectorRoomInfo _connectorRoomInfo;
    public bool doorsCanClose;
    private bool _plrEnteredRoom;
    private GameObject _player;
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
            colliderSize.x = _roomInfo.roomLength + 2.5f;
            colliderSize.y = _roomInfo.roomHeight + 2.5f;
            _collider.size = colliderSize;
        }
       
    }

    void Start()
    { 
        _roomScripting = GetComponent<RoomScripting>();
        _player = GameObject.FindWithTag("Player");
    }

    private void Update()
    {
        if (_player != null && roomType == RoomType.Room)
        {
            if (_collider.bounds.Contains(_player.transform.position) && !_plrEnteredRoom)
            {
                _roomScripting.EnterSpecialRoom();
                _plrEnteredRoom = true;
            }

            if (doorsCanClose && _collider.bounds.Contains(_player.transform.position))
            {
                _roomScripting.playerIsInRoom = true;
            }
            else if (doorsCanClose && !_collider.bounds.Contains(_player.transform.position))
            {
                _roomScripting.playerIsInRoom = false;
            }

            if (!_collider.bounds.Contains(_player.transform.position) && _plrEnteredRoom)
            {
                _roomScripting.ExitSpecialRoom();
                _plrEnteredRoom = false;
            }
        }
    }
}
