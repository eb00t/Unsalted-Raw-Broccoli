using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConnectorIntersectionRaycast : MonoBehaviour
{
    private Ray _horizMiddleRay, _verticMiddleRay;
    private ConnectorRoomInfo _connectorRoomInfo;
    private float _rayCastLength, _rayCastHeight; //Stored for use later
    private float _rayCastDistance; //Used as a variable when checking raycasts
    private float _innerRayCastDistance;
    public LayerMask layerMask;
    public BoxCollider _collider;
    
    void Awake()
    {
        _collider = GetComponent<BoxCollider>();
        _connectorRoomInfo = GetComponent<ConnectorRoomInfo>();

        _rayCastLength = _connectorRoomInfo.connectorLength;
        _rayCastHeight = _connectorRoomInfo.connectorHeight;

       
        _horizMiddleRay = new Ray(_connectorRoomInfo.wallL.position, Vector3.right);
        _verticMiddleRay = new Ray(_connectorRoomInfo.wallT.position, Vector3.down);
    }

    public void CheckForInvalidSpawn()
    {
        gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
        bool discard = false;

       if (FireInternalRayCast())
        {
            Debug.Log("INTERNAL RAY HIT");
            discard = true;
        }
            
        if (discard)
        {
            Debug.Log(name + " is trying to spawn in occupied space.");
            _connectorRoomInfo.markedForDiscard = true;
            LevelBuilder.Instance.discardedRooms.Add(gameObject);
        }
        _collider.enabled = true;
        gameObject.layer = LayerMask.NameToLayer("Intersection Checker");
        StartCoroutine(SecondRoundInternalCheck());
    }

    private bool FireInternalRayCast()
    {
        bool discard;
         if (Physics.Raycast(_horizMiddleRay, _connectorRoomInfo.connectorLength + 1, layerMask))
         {
             Debug.Log("HORIZ RAY HIT!");
             discard = true;
         } 
         else if (Physics.Raycast(_verticMiddleRay, _connectorRoomInfo.connectorHeight + 1, layerMask))
         {
             Debug.Log("VERT RAY HIT!");
             discard = true;
         }
         else
         {
             discard = false;
         }

         return discard;
    }

    private void CheckForInternalIntersection()
    {
        gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
        bool discard = FireInternalRayCast();
        if (discard)
        {
            Debug.Log(name + " is trying to spawn in occupied space.");
            _connectorRoomInfo.markedForDiscard = true;
            LevelBuilder.Instance.discardedRooms.Add(gameObject);
        }
        _collider.enabled = true;
        gameObject.layer = LayerMask.NameToLayer("Intersection Checker");
    }

    IEnumerator SecondRoundInternalCheck()
    {
        yield return new WaitForSecondsRealtime(.5f);
        CheckForInternalIntersection();
    }
    
    void Update()
    {
        Debug.DrawRay(_horizMiddleRay.origin, _horizMiddleRay.direction * (_connectorRoomInfo.connectorLength + 1), Color.magenta);
        Debug.DrawRay(_verticMiddleRay.origin, _verticMiddleRay.direction * (_connectorRoomInfo.connectorHeight + 1), Color.magenta);
    }
}
