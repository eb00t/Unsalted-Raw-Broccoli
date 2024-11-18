using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IntersectionRaycast : MonoBehaviour
{
    private Ray _firstRay, _secondRay; //Which rays are being used to check for rooms in the way.
    private Ray _leftTopRay, _rightTopRay, _leftBottomRay, _rightBottomRay;
    private Ray _topLeftRay, _topRightRay, _bottomLeftRay, _bottomRightRay;
    private Ray _horizMiddleRay, _verticMiddleRay;
    private RoomInfo _roomInfo;
    private float _rayCastLength, _rayCastHeight; //Stored for use later
    private float _rayCastDistance; //Used as a variable when checking raycasts
    private float _innerRayCastDistance;
    private float _halfRoomLength, _halfRoomHeight;
    public LayerMask layerMask;
    private BoxCollider _collider;
    
    void Awake()
    {
        _collider = GetComponent<BoxCollider>();
        _roomInfo = GetComponent<RoomInfo>();
        _halfRoomLength = _roomInfo.roomLength / 2;
        _halfRoomHeight = _roomInfo.roomHeight / 2;

        _rayCastLength = _roomInfo.roomLength + 12;
        _rayCastHeight = _roomInfo.roomHeight + 12;

        Vector3 cornerTL = new Vector3(_roomInfo.wallL.position.x, _roomInfo.wallL.position.y + _halfRoomHeight, _roomInfo.wallL.position.z);
        Vector3 cornerTR = new Vector3(_roomInfo.wallR.position.x, _roomInfo.wallL.position.y + _halfRoomHeight, _roomInfo.wallL.position.z);
        Vector3 cornerBL = new Vector3(_roomInfo.wallL.position.x, _roomInfo.wallR.position.y - _halfRoomHeight, _roomInfo.wallR.position.z);
        Vector3 cornerBR = new Vector3(_roomInfo.wallR.position.x, _roomInfo.wallR.position.y - _halfRoomHeight, _roomInfo.wallR.position.z);
        //RAYCAST SETUP
        
        _topLeftRay = new Ray(cornerTL, Vector3.up);
        _topRightRay = new Ray(cornerTR, Vector3.up);
        _bottomLeftRay = new Ray(cornerBL, Vector3.down);
        _bottomRightRay = new Ray(cornerBR, Vector3.down);
        _leftTopRay = new Ray(cornerTL, Vector3.left);
        _rightTopRay = new Ray(cornerTR, Vector3.right);
        _leftBottomRay = new Ray(cornerBL, Vector3.left);
        _rightBottomRay = new Ray(cornerBR, Vector3.right);
        _horizMiddleRay = new Ray(_roomInfo.wallL.position, Vector3.right);
        _verticMiddleRay = new Ray(_roomInfo.wallT.position, Vector3.down);
    }

    public void CheckForInvalidSpawn(ConnectorRoomInfo spawnedConnectorInfo)
    {
        gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
        bool discard = false;
        
        switch (spawnedConnectorInfo.spawnedOnSide)
        {
            case "Left":
                _firstRay = _leftTopRay;
                _secondRay = _leftBottomRay;
                _rayCastDistance = _rayCastLength;
                break;
            case "Right":  
                _firstRay = _rightTopRay;
                _secondRay = _rightBottomRay;
                _rayCastDistance = _rayCastLength;
                break;
            case "Top":
                _firstRay = _topLeftRay;
                _secondRay = _topRightRay;
                _rayCastDistance = _rayCastHeight;
                break;
            case "Bottom":
                _firstRay = _bottomLeftRay;
                _secondRay = _bottomRightRay;
                _rayCastDistance = _rayCastHeight;
                break;
            default:
                Debug.Log("THIS IS NOT WORKING!");
                break;
        }

        if (Physics.Raycast(_firstRay, _rayCastDistance, layerMask))
        {
            Debug.Log("TOP/LEFT RAY HIT!");
            discard = true;
        }
        else if (Physics.Raycast(_secondRay, _rayCastDistance, layerMask))
        {
            Debug.Log("BOTTOM/RIGHT RAY HIT!");
            discard = true;
        }
        else if (Physics.Raycast(_horizMiddleRay, _roomInfo.roomLength + 1, layerMask))
        {
            Debug.Log("HORIZ RAY HIT!");
            discard = true;
        } 
        else if (Physics.Raycast(_verticMiddleRay, _roomInfo.roomHeight + 1, layerMask))
        {
            Debug.Log("VERT RAY HIT!");
            discard = true;
        }
        

        if (discard)
        {
            Debug.Log(name + " is trying to spawn in occupied space.");
            _roomInfo.markedForDiscard = true;
            foreach (var door in _roomInfo.allDoors)
            {
                LevelBuilder.Instance.spawnPoints.Remove(door.transform);
            }
            LevelBuilder.Instance.discardedRooms.Add(gameObject);
        }
        _collider.enabled = true;
        gameObject.layer = LayerMask.NameToLayer("Intersection Checker");
    }

    void Update()
    {
        Debug.DrawRay(_leftTopRay.origin, _leftTopRay.direction * (_rayCastLength), Color.red);
        Debug.DrawRay(_rightTopRay.origin, _rightTopRay.direction * (_rayCastLength), Color.green);
        Debug.DrawRay(_leftBottomRay.origin, _leftBottomRay.direction * (_rayCastLength), Color.red);
        Debug.DrawRay(_rightBottomRay.origin, _rightBottomRay.direction * (_rayCastLength), Color.green);
        Debug.DrawRay(_topLeftRay.origin, _topLeftRay.direction * (_rayCastHeight), Color.blue);
        Debug.DrawRay(_bottomLeftRay.origin, _bottomLeftRay.direction * (_rayCastHeight), Color.yellow);
        Debug.DrawRay(_topRightRay.origin, _topRightRay.direction * (_rayCastHeight), Color.blue);
        Debug.DrawRay(_bottomRightRay.origin, _bottomRightRay.direction * (_rayCastHeight), Color.yellow);
        Debug.DrawRay(_horizMiddleRay.origin, _horizMiddleRay.direction * (_roomInfo.roomLength + 1), Color.magenta);
        Debug.DrawRay(_verticMiddleRay.origin, _verticMiddleRay.direction * (_roomInfo.roomHeight + 1), Color.magenta);

    }
}
