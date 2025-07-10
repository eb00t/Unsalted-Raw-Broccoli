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
    public Collider collider1, collider2;

    private void Awake()
    {
        _collider = GetComponent<Collider>();
        _collider.enabled = true;
        _roomInfo = transform.root.gameObject.GetComponent<RoomInfo>();
    }   
    
    private void Start()
    { 
        _roomScripting = GetComponent<RoomScripting>();
        _player = GameObject.FindWithTag("Player");
    }

    private void Update()
    {
        var isPlayerInCollider = _collider.bounds.Contains(_player.transform.position) ||
                                 collider1.bounds.Contains(_player.transform.position) ||
                                 collider2.bounds.Contains(_player.transform.position);
            
        if (isPlayerInCollider && !_plrEnteredRoom)
        {
            _roomScripting.EnterSpecialRoom();
            _plrEnteredRoom = true;
        }

        if (doorsCanClose && isPlayerInCollider)
        {
            _roomScripting.playerIsInRoom = true;
        }
        else if (doorsCanClose && !isPlayerInCollider)
        {
            _roomScripting.playerIsInRoom = false;
        }

        if (!isPlayerInCollider && _plrEnteredRoom)
        {
            _roomScripting.ExitSpecialRoom();
            _plrEnteredRoom = false;
        }
    }
}

