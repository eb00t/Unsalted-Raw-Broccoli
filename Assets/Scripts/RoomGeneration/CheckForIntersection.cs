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

    void Start()
    {
        _roomInfo = transform.root.gameObject.GetComponent<RoomInfo>();
        _leftRay = new Ray(_roomInfo.doorL.transform.localPosition, Vector3.right);
        _rightRay = new Ray(_roomInfo.doorR.transform.localPosition, Vector3.left);
        _upRay = new Ray(_roomInfo.doorT.transform.localPosition, Vector3.down);
        _downRay = new Ray(_roomInfo.doorB.transform.localPosition, Vector3.up);
    }

   public void CheckForIntersections()
   {
       if (Physics.Raycast(_leftRay, _roomInfo.roomLength + 2, layerMask))
       {
           intersecting = true;
       }
       if (Physics.Raycast(_rightRay, _roomInfo.roomLength + 2, layerMask))
       {
           intersecting = true;
       }
       if (Physics.Raycast(_upRay, _roomInfo.roomHeight + 2, layerMask))
       {
           intersecting = true;
       }
       if (Physics.Raycast(_downRay, _roomInfo.roomHeight + 2, layerMask))
       {
           intersecting = true;
       }

       if (intersecting)
       {
           Debug.Log("Something is intersecting");
       }
   }

   private void Update()
   {
       Debug.DrawRay(_roomInfo.doorL.transform.position, Vector3.right * (_roomInfo.roomLength + 2), Color.red);
       Debug.DrawRay(_roomInfo.doorR.transform.position, Vector3.left * (_roomInfo.roomLength + 2), Color.green);
       Debug.DrawRay(_roomInfo.doorT.transform.position, Vector3.down * (_roomInfo.roomHeight + 2), Color.blue);
       Debug.DrawRay(_roomInfo.doorB.transform.position, Vector3.up * (_roomInfo.roomHeight + 2), Color.yellow);
   }
}
