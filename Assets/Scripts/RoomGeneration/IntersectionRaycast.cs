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


    void Awake()
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
            _layers.Add(child.gameObject.layer);
        }

        foreach (var child in _allChildren)
        {
            objectsToIgnore.Add(child.gameObject);
        }

        objectsToIgnore.Add(GameObject.FindWithTag("Player"));
        _collider = GetComponent<BoxCollider>();
        _roomInfo = GetComponent<RoomInfo>();
        _halfRoomLength = _roomInfo.roomLength / 2;
        _halfRoomHeight = _roomInfo.roomHeight / 2;
        _quarterRoomLength = _roomInfo.roomLength / 4;
        _quarterRoomHeight = _roomInfo.roomHeight / 4;

        MessUpLayers();

        _rayCastLength = _roomInfo.roomLength + 2;
        _rayCastHeight = _roomInfo.roomHeight + 2;

        Vector3 cornerTL = new Vector3(_roomInfo.wallL.position.x - .4f,
            _roomInfo.wallL.position.y + _halfRoomHeight + .9f, _roomInfo.wallL.position.z);
        Vector3 cornerTR = new Vector3(_roomInfo.wallR.position.x +.4f,
            _roomInfo.wallL.position.y + _halfRoomHeight + .9f, _roomInfo.wallL.position.z);
        Vector3 cornerBL = new Vector3(_roomInfo.wallL.position.x -.4f,
            _roomInfo.wallR.position.y - _halfRoomHeight - .9f, _roomInfo.wallR.position.z);
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
    }

    void MessUpLayers() // Changes all layers of children to ensure they don't trigger the raycasts.
    {
        Debug.Log("Messing up layers of " + gameObject.name);
        foreach (var child in _allChildren)
        {
            child.gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
        }
    }

    void FixLayers() // Return layers to normal
    {
        Debug.Log("Fixing layers of " + gameObject.name);
        for (int i = 0; i < _allChildren.Count; i++)
        {
            _allChildren[i].gameObject.layer = _layers[i];
        }

        _layers.Clear();
        foreach (var wall in _roomInfo.allWalls)
        {
            _wallLayers.Add(wall.gameObject.layer);
            _allWalls.Add(wall.transform);
            wall.gameObject.layer = LayerMask.NameToLayer("Intersection Checker");
        }

        foreach (var door in _roomInfo.doorSpawnPoints)
        {
            _doorLayers.Add(door.gameObject.layer);
            _allDoors.Add(door.transform);
            door.gameObject.layer = LayerMask.NameToLayer("Intersection Checker");
        }
    }

    public void FixWallLayers()
    {
        for (int i = 0; i < _allWalls.Count; i++)
        {
            _allWalls[i].gameObject.layer = _wallLayers[i];
        }
    }

    public void FixDoorLayers()
    {
        for (int i = 0; i < _allDoors.Count; i++)
        {
            _allDoors[i].gameObject.layer = _doorLayers[i];
        }
    }

    private void Start()
    {
        CheckForInternalIntersection(); //_roomInfo.connectorSpawnedOff.GetComponent<ConnectorRoomInfo>());
    }

    private bool FireInternalRayCast()
    {
        bool discard = false;
        if (Physics.Raycast(_horizMiddleRay, out RaycastHit horizHit, _roomInfo.roomLength + .9f, layerMask))
        {
            Debug.Log("HORIZ RAY HIT!");
            if (!objectsToIgnore.Contains(horizHit.collider.gameObject))
            {
                discard = true;
            }
        }
        else if (Physics.Raycast(_verticMiddleRay, out RaycastHit vertHit, _roomInfo.roomHeight + .9f, layerMask))
        {
            Debug.Log("VERT RAY HIT!");
            if (!objectsToIgnore.Contains(vertHit.collider.gameObject))
            {
                Debug.Log("Vertical Ray from: " + gameObject.name + " hit " + vertHit.collider.gameObject.name);
                discard = true;
            }
        }
        else if (Physics.Raycast(_topLeftRay, out RaycastHit topWallHit, _roomInfo.roomHeight, layerMask))
        {
            Debug.Log("TOP WALL RAY HIT!");
            if (!objectsToIgnore.Contains(topWallHit.collider.gameObject))
            {
                Debug.Log("Top Ray from: " + gameObject.name + " hit " + topWallHit.collider.gameObject.name);
                discard = true;
            }
        }
        else if (Physics.Raycast(_topRightRay, out RaycastHit bottomWallHit, _roomInfo.roomHeight, layerMask))
        {
            Debug.Log("BOTTOM WALL RAY HIT!");
            if (!objectsToIgnore.Contains(bottomWallHit.collider.gameObject))
            {
                Debug.Log("Bottom Ray from: " + gameObject.name + " hit " + bottomWallHit.collider.gameObject.name);
                discard = true;
            }
        }
        else if (Physics.Raycast(_leftTopRay, out RaycastHit leftWallHit, _roomInfo.roomLength, layerMask))
        {
            Debug.Log("LEFT WALL RAY HIT!");
            if (!objectsToIgnore.Contains(leftWallHit.collider.gameObject))
            {
                Debug.Log("Left Ray from: " + gameObject.name + " hit " + leftWallHit.collider.gameObject.name);
                discard = true;
            }
        }
        else if (Physics.Raycast(_leftBottomRay, out RaycastHit rightWallHit, _roomInfo.roomLength, layerMask))
        {
            Debug.Log("RIGHT WALL RAY HIT!");
            if (!objectsToIgnore.Contains(rightWallHit.collider.gameObject))
            {
                Debug.Log("Right Ray from: " + gameObject.name + " hit " + rightWallHit.collider.gameObject.name);
                discard = true;
            }
        }
        else
        {
            discard = false;
        }
        
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
        MessUpLayers();
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
                _roomInfo.canBeDiscarded = false;
                 FixLayers();
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
