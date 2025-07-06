using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NonBoxColliderHandler : MonoBehaviour
{
    private Collider _collider;
    private RoomInfo _roomInfo;
    private RoomScripting _roomScripting;
    private ConnectorRoomInfo _connectorRoomInfo;
    public bool doorsCanClose;
    private bool _plrEnteredRoom;
    private GameObject _player;

    private void Awake()
    {
        _collider = GetComponent<Collider>();
        _collider.enabled = true;
        _roomInfo = transform.root.gameObject.GetComponent<RoomInfo>();
    }   
    void Start()
    { 
        _roomScripting = GetComponent<RoomScripting>();
        _player = GameObject.FindWithTag("Player");
    }

    private void Update()
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

