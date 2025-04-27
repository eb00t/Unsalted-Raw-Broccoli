using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

[RequireComponent(typeof(RoomInfo))]
public class IntersectionRaycast : MonoBehaviour
{
    private Ray _firstRay, _secondRay; //Which rays are being used to check for rooms in the way.
    private Ray _leftTopRay, _rightTopRay, _leftBottomRay, _rightBottomRay;
    private Ray _topLeftRay, _topRightRay, _bottomLeftRay, _bottomRightRay;
    private Ray _horizMiddleRay, _verticMiddleRay;
    private RoomInfo _roomInfo;
    public List<int> _layers, _wallLayers, _doorLayers;
    public List<Transform> _allChildren, _allWalls, _allDoors;
    private float _rayCastLength, _rayCastHeight; //Stored for use later
    private float _rayCastDistance; //Used as a variable when checking raycasts
    private float _innerRayCastDistance;
    private float _halfRoomLength, _halfRoomHeight, _quarterRoomLength, _quarterRoomHeight;
    public LayerMask layerMask;
    private BoxCollider _collider;
    private bool _checkedTwice;
    public List<GameObject> objectsToIgnore;
    private Vector3 _halfExtents;


    void Start()
    {
        _allChildren = new List<Transform>();
        _allWalls = new List<Transform>();
        _allDoors = new List<Transform>();
        _layers = new List<int>();
        _wallLayers = new List<int>();
        _doorLayers = new List<int>();

        foreach (var child in gameObject.GetComponentsInChildren<Transform>())
        {
            _allChildren.Add(child);
        }
        
        foreach (var child in _allChildren)
        {
            objectsToIgnore.Add(child.gameObject);
            _layers.Add(child.gameObject.layer);
        }

        foreach (var child in objectsToIgnore)
        {
            child.layer = LayerMask.NameToLayer("Ignore Raycast");
        }

        objectsToIgnore.Add(GameObject.FindWithTag("Player"));
        _collider = GetComponent<BoxCollider>();
        _roomInfo = GetComponent<RoomInfo>();
        _halfRoomLength = _roomInfo.roomLength / 2;
        _halfRoomHeight = _roomInfo.roomHeight / 2;
        _quarterRoomLength = _roomInfo.roomLength / 4;
        _quarterRoomHeight = _roomInfo.roomHeight / 4;

        _rayCastLength = _roomInfo.roomLength + 2.5f;
        _rayCastHeight = _roomInfo.roomHeight + 2.5f;

        Vector3 cornerTL = new Vector3(_roomInfo.wallL.position.x - .45f,
            _roomInfo.wallL.position.y + _halfRoomHeight + .95f, _roomInfo.wallL.position.z);
        Vector3 cornerTR = new Vector3(_roomInfo.wallR.position.x +.45f,
            _roomInfo.wallL.position.y + _halfRoomHeight + .95f, _roomInfo.wallL.position.z);
        Vector3 cornerBL = new Vector3(_roomInfo.wallL.position.x -.45f,
            _roomInfo.wallR.position.y - _halfRoomHeight - .95f, _roomInfo.wallR.position.z);
        Vector3 cornerBR = new Vector3(_roomInfo.wallR.position.x, _roomInfo.wallR.position.y - _halfRoomHeight + 0.5f,
            _roomInfo.wallR.position.z);
        Vector3 adjHorizRayPos = new Vector3(_roomInfo.wallL.position.x + 0.5f, _roomInfo.wallL.position.y,
            _roomInfo.wallL.position.z);
        Vector3 adjVertiRayPos = new Vector3(_roomInfo.wallT.position.x, _roomInfo.wallT.position.y - 0.5f,
            _roomInfo.wallT.position.z);
        //RAYCAST SETUP

        _topLeftRay = new Ray(cornerTL, Vector3.down);
        _topRightRay = new Ray(cornerTR, Vector3.down);
        //_bottomLeftRay = new Ray(cornerBL, Vector3.up);
        //_bottomRightRay = new Ray(cornerBR, Vector3.up);
        _leftTopRay = new Ray(cornerTL, Vector3.right);
        //_rightTopRay = new Ray(cornerTR, Vector3.left);
        _leftBottomRay = new Ray(cornerBL, Vector3.right);
        //_rightBottomRay = new Ray(cornerBR, Vector3.left);
        _horizMiddleRay = new Ray(adjHorizRayPos, Vector3.right);
        _verticMiddleRay = new Ray(adjVertiRayPos, Vector3.down);

        CheckForInternalIntersection(); //_roomInfo.connectorSpawnedOff.GetComponent<ConnectorRoomInfo>());

        _halfExtents = new Vector3(_halfRoomLength, _halfRoomHeight, 1.5f);

    }

    
    private bool FireInternalRayCast()
    {
        bool discard = false;
        Collider[] horizHits = Physics.OverlapBox(transform.position, _halfExtents, Quaternion.identity, layerMask);
        //Debug.Log("HORIZ RAY HIT!");
        foreach (var horizHit in horizHits)
        {
            if (!objectsToIgnore.Contains(horizHit.gameObject))
            {
                Debug.Log("Horizontal Ray from " + gameObject.name + " hit " + horizHit.gameObject.name);
                discard = true;
            }
        }
        Debug.Log(horizHits);
        /*RaycastHit[] vertHits = Physics.RaycastAll(_verticMiddleRay, _roomInfo.roomHeight + .9f, layerMask, QueryTriggerInteraction.Collide);
        foreach (var vertHit in vertHits)
        {
            //Debug.Log("VERT RAY HIT!");
            if (!objectsToIgnore.Contains(vertHit.collider.gameObject))
            {
                Debug.Log("Vertical Ray from: " + gameObject.name + " hit " + vertHit.collider.gameObject.name);
                discard = true;
            }
        }
        Debug.Log(vertHits);
        RaycastHit[] topWallHits = Physics.RaycastAll(_topLeftRay, _roomInfo.roomHeight, layerMask, QueryTriggerInteraction.Collide);
        foreach (var topWallHit in topWallHits)
        {
            //Debug.Log("TOP WALL RAY HIT!");
            if (!objectsToIgnore.Contains(topWallHit.collider.gameObject))
            {
                Debug.Log("Top Ray from: " + gameObject.name + " hit " + topWallHit.collider.gameObject.name);
                discard = true;
            }
        }
        Debug.Log(topWallHits);
        RaycastHit[] bottomWallHits = Physics.RaycastAll(_topRightRay, _roomInfo.roomHeight, layerMask, QueryTriggerInteraction.Collide);
        foreach (var bottomWallHit in bottomWallHits)
        {
            //Debug.Log("BOTTOM WALL RAY HIT!");
            if (!objectsToIgnore.Contains(bottomWallHit.collider.gameObject))
            {
                Debug.Log("Bottom Ray from: " + gameObject.name + " hit " + bottomWallHit.collider.gameObject.name);
                discard = true;
            }
        }
        Debug.Log(bottomWallHits);
        RaycastHit[] leftWallHits = Physics.RaycastAll(_leftTopRay, _roomInfo.roomLength, layerMask, QueryTriggerInteraction.Collide);
        foreach (var leftWallHit in leftWallHits)
        {
            Debug.Log("LEFT WALL RAY HIT!");
            if (!objectsToIgnore.Contains(leftWallHit.collider.gameObject))
            {
                Debug.Log("Left Ray from: " + gameObject.name + " hit " + leftWallHit.collider.gameObject.name);
                discard = true;
            }
        }
        
        RaycastHit[] rightWallHits = Physics.RaycastAll(_leftBottomRay, _roomInfo.roomLength, layerMask, QueryTriggerInteraction.Collide);
        foreach (var rightWallHit in rightWallHits)
        {
            Debug.Log("RIGHT WALL RAY HIT!");
            if (!objectsToIgnore.Contains(rightWallHit.collider.gameObject))
            {
                Debug.Log("Right Ray from: " + gameObject.name + " hit " + rightWallHit.collider.gameObject.name);
                discard = true;
            }
        }
        Debug.Log(rightWallHits);
        */
        if (_roomInfo.canBeDiscarded == false)
        {
            return false;
        }
        else
        {
            return discard;
        }
    }



    public void CheckForInternalIntersection()
    {
        gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
        bool discard = FireInternalRayCast();
        if (discard)
        {
            Debug.Log(name + " is trying to spawn in occupied space.");
            _roomInfo.MarkRoomForDiscard();
            _collider.enabled = true;
        }
        else
        {
            if (_checkedTwice == false)
            {
                StartCoroutine(SecondRoundInternalCheck());
            }
            else
            {
                for (int i = 0; i < _layers.Count; i++)
                {
                    objectsToIgnore[i].layer = _layers[i];
                }
            }
        }
    }
    

    public IEnumerator SecondRoundInternalCheck()
    {
        yield return new WaitForSecondsRealtime(.05f);
        _checkedTwice = true;
        CheckForInternalIntersection();
    }
    
    void Update()
    {
        Debug.DrawRay(_leftTopRay.origin, _leftTopRay.direction * (_rayCastLength), Color.red);
        Debug.DrawRay(_rightTopRay.origin, _rightTopRay.direction * (_rayCastLength), Color.red);
        Debug.DrawRay(_leftBottomRay.origin, _leftBottomRay.direction * (_rayCastLength), Color.green);
        Debug.DrawRay(_rightBottomRay.origin, _rightBottomRay.direction * (_rayCastLength), Color.green);
        Debug.DrawRay(_topLeftRay.origin, _topLeftRay.direction * (_rayCastHeight), Color.blue);
        Debug.DrawRay(_bottomLeftRay.origin, _bottomLeftRay.direction * (_rayCastHeight), Color.blue);
        Debug.DrawRay(_topRightRay.origin, _topRightRay.direction * (_rayCastHeight), Color.yellow);
        Debug.DrawRay(_bottomRightRay.origin, _bottomRightRay.direction * (_rayCastHeight), Color.yellow);
        Debug.DrawRay(_horizMiddleRay.origin, _horizMiddleRay.direction * (_roomInfo.roomLength), Color.magenta);
        Debug.DrawRay(_verticMiddleRay.origin, _verticMiddleRay.direction * (_roomInfo.roomHeight), Color.magenta);
    }
}
