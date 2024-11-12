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
    public bool intersecting;

    void Start()
    { 
        _roomInfo = transform.root.GetComponent<RoomInfo>();
        _leftRay = new Ray(_roomInfo.doorL.transform.localPosition, Vector3.left);
        _rightRay = new Ray(_roomInfo.doorR.transform.localPosition, Vector3.right);
        _upRay = new Ray(_roomInfo.doorT.transform.localPosition, Vector3.up);
        _downRay = new Ray(_roomInfo.doorB.transform.localPosition, Vector3.down);
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
    
    
    
}
