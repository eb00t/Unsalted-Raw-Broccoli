using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IntersectionRaycast : MonoBehaviour
{
    private Ray _rayTop, _rayBottom;
    private Ray _leftTopRay, _rightTopRay, _leftBottomRay, _rightBottomRay;
    private Ray _topLeftRay, _topRightRay, _bottomLeftRay, _bottomRightRay;
    private RoomInfo _roomInfo;
    private float _rayCastLength, _rayCastHeight;
    private float _halfRoomLength, _halfRoomHeight;
    void Awake()
    {
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
    }

    public void CheckForInvalidSpawn(ConnectorRoomInfo spawnedConnectorInfo)
    {
        switch (spawnedConnectorInfo.spawnedOnSide)
        {
            case "Left":
                break;
            case "Right":  
                break;
            case "Top":
                break;
            case "Bottom":
                break;
        }
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
      
    }
}
