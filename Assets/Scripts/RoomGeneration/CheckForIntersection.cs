using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;

public class CheckForIntersection : MonoBehaviour
{
    private RoomInfo _roomInfo;
    private Ray _upRay, _downRay, _leftRay, _rightRay;
    public LayerMask layerMask;
    public bool intersecting = false;
    private BoxCollider _collider;

    void Awake()
    {
        _roomInfo = transform.root.gameObject.GetComponent<RoomInfo>();
        _collider = GetComponent<BoxCollider>();
        _collider.enabled = true;
        var colliderSize = _collider.size;
        colliderSize.x = _roomInfo.roomLength;
        colliderSize.y = _roomInfo.roomHeight;
        _collider.size = colliderSize;
        _leftRay = new Ray(_roomInfo.doorL.transform.localPosition, Vector3.right);
        _rightRay = new Ray(_roomInfo.doorR.transform.localPosition, Vector3.left);
        _upRay = new Ray(_roomInfo.doorT.transform.localPosition, Vector3.down);
        _downRay = new Ray(_roomInfo.doorB.transform.localPosition, Vector3.up);
    }

   public void CheckForIntersections()
   {
       _collider.enabled = false;
       RaycastHit hitInfo;
       Debug.Log("Checking for intersections.");
       if (Physics.Raycast(_leftRay, out hitInfo, _roomInfo.roomLength + 2, layerMask))
       {
          if (hitInfo.collider.gameObject.CompareTag("Intersection Checker"))
          {
              intersecting = true;
          }
       }
       if (Physics.Raycast(_rightRay, out hitInfo,_roomInfo.roomLength + 2, layerMask))
       {
           if (hitInfo.collider.gameObject.CompareTag("Intersection Checker"))
           {
               intersecting = true;
           }
       }
       if (Physics.Raycast(_upRay, out hitInfo,_roomInfo.roomHeight + 2, layerMask))
       {
           if (hitInfo.collider.gameObject.CompareTag("Intersection Checker"))
           {
               intersecting = true;
           }
       }
       if (Physics.Raycast(_downRay, out hitInfo,_roomInfo.roomHeight + 2, layerMask))
       {
           if (hitInfo.collider.gameObject.CompareTag("Intersection Checker"))
           {
               intersecting = true;
           }
       }

       if (intersecting)
       {
           Debug.Log("Something is intersecting");
           //LevelBuilder.Instance.KillRoomAndConnector(gameObject, _roomInfo.attachedConnectors[0]);
       }
       _collider.enabled = true;
   }

   private void Update()
   {
       Debug.DrawRay(_roomInfo.doorL.transform.position, Vector3.right * (_roomInfo.roomLength + 2), Color.red);
       Debug.DrawRay(_roomInfo.doorR.transform.position, Vector3.left * (_roomInfo.roomLength + 2), Color.green);
       Debug.DrawRay(_roomInfo.doorT.transform.position, Vector3.down * (_roomInfo.roomHeight + 2), Color.blue);
       Debug.DrawRay(_roomInfo.doorB.transform.position, Vector3.up * (_roomInfo.roomHeight + 2), Color.yellow);
   }
}
