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
        _collider = GetComponent<BoxCollider>();
        _roomInfo = GetComponent<RoomInfo>();
        _halfRoomLength = _roomInfo.roomLength / 2;
        _halfRoomHeight = _roomInfo.roomHeight / 2;
        _quarterRoomLength = _roomInfo.roomLength / 4;
        _quarterRoomHeight = _roomInfo.roomHeight / 4;
        
        MessUpLayers();
        
        _rayCastLength = _roomInfo.roomLength + 12;
        _rayCastHeight = _roomInfo.roomHeight + 12;

        Vector3 cornerTL = new Vector3(_roomInfo.wallL.position.x, _roomInfo.wallL.position.y + _halfRoomHeight, _roomInfo.wallL.position.z);
        Vector3 cornerTR = new Vector3(_roomInfo.wallR.position.x, _roomInfo.wallL.position.y + _halfRoomHeight, _roomInfo.wallL.position.z);
        Vector3 cornerBL = new Vector3(_roomInfo.wallL.position.x, _roomInfo.wallR.position.y - _halfRoomHeight, _roomInfo.wallR.position.z);
        Vector3 cornerBR = new Vector3(_roomInfo.wallR.position.x, _roomInfo.wallR.position.y - _halfRoomHeight, _roomInfo.wallR.position.z);
        Vector3 adjHorizRayPos = new Vector3(_roomInfo.wallL.position.x + 0.5f, _roomInfo.wallL.position.y, _roomInfo.wallL.position.z);
        Vector3 adjVertiRayPos = new Vector3(_roomInfo.wallT.position.x, _roomInfo.wallT.position.y - 0.5f, _roomInfo.wallT.position.z);
        //RAYCAST SETUP
        
        _topLeftRay = new Ray(cornerTL, Vector3.up);
        _topRightRay = new Ray(cornerTR, Vector3.up);
        _bottomLeftRay = new Ray(cornerBL, Vector3.down);
        _bottomRightRay = new Ray(cornerBR, Vector3.down);
        _leftTopRay = new Ray(cornerTL, Vector3.left);
        _rightTopRay = new Ray(cornerTR, Vector3.right);
        _leftBottomRay = new Ray(cornerBL, Vector3.left);
        _rightBottomRay = new Ray(cornerBR, Vector3.right);
        _horizMiddleRay = new Ray(adjHorizRayPos, Vector3.right);
        _verticMiddleRay = new Ray(adjVertiRayPos, Vector3.down);
    }

    void MessUpLayers()
    {
        Debug.Log("Messing up layers of " + gameObject.name);
        foreach (var child in _allChildren)
        {
            child.gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
        } 
    }
    void FixLayers()
    {
        Debug.Log("Fixing layers of " + gameObject.name);
        for(int i = 0; i < _allChildren.Count; i++)
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
        foreach (var door in _roomInfo.allDoors)
        {
            _doorLayers.Add(door.gameObject.layer);
            _allDoors.Add(door.transform);
            door.gameObject.layer = LayerMask.NameToLayer("Intersection Checker");
        }
    }

    public void FixWallLayers()
    {
        for(int i = 0; i < _allWalls.Count; i++)
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
        /*Vector3 quarterHeight = new Vector3(_horizMiddleRay.origin.x, _horizMiddleRay.origin.y - _quarterRoomHeight, _horizMiddleRay.origin.z); //QUARTER HEIGHT FROM THE TOP DOWN
        Vector3 threeQuarterHeight = new Vector3(_horizMiddleRay.origin.x, _horizMiddleRay.origin.y + _quarterRoomHeight, _horizMiddleRay.origin.z);
        Vector3 quarterLength = new Vector3(_horizMiddleRay.origin.x - _quarterRoomLength, _horizMiddleRay.origin.y, _horizMiddleRay.origin.z); // QUARTER LENGTH FROM THE LEFT
        Vector3 threeQuarterLength = new Vector3(_horizMiddleRay.origin.x + _quarterRoomLength, _horizMiddleRay.origin.y, _horizMiddleRay.origin.z);
        Ray quarterHeightRay = new Ray(quarterHeight, Vector3.right); //QUARTER HEIGHT FROM THE TOP DOWN
        Ray threeQuarterHeightRay = new Ray(threeQuarterHeight, Vector3.right);
        Ray quarterLengthRay = new Ray(quarterLength, Vector3.down); // QUARTER LENGTH FROM THE LEFT
        Ray threeQuarterLengthRay = new Ray(threeQuarterLength, Vector3.down);*/
        bool discard = false;
        if (Physics.Raycast(_horizMiddleRay, out RaycastHit horizHit, _roomInfo.roomLength + .1f, layerMask))
        {
            Debug.Log("HORIZ RAY HIT!");
            if (horizHit.transform.root.gameObject.GetComponent<RoomInfo>())
            {
               discard = true;
            }
            if (horizHit.transform.root.gameObject.GetComponent<ConnectorRoomInfo>())
            {
                Debug.Log("Horizontal Ray from: " + gameObject.name + " hit " + horizHit.collider.gameObject.name);
                foreach (var connector in _roomInfo.attachedConnectors)
                {
                    connector.GetComponent<ConnectorRoomInfo>().markedForDiscard = true;
                }
                discard = true;
            }
        }
        else if (Physics.Raycast(_verticMiddleRay, out RaycastHit vertHit, _roomInfo.roomHeight, layerMask))
        {
            Debug.Log("VERT RAY HIT!");
            if (vertHit.transform.root.gameObject.GetComponent<RoomInfo>())
            {
                Debug.Log("Vertical Ray from: " + gameObject.name + " hit " + vertHit.collider.gameObject.name);
                discard = true;
            }
            if (vertHit.transform.root.gameObject.GetComponent<ConnectorRoomInfo>())
            {
                foreach (var connector in _roomInfo.attachedConnectors)
                {
                    connector.GetComponent<ConnectorRoomInfo>().markedForDiscard = true;
                }
                Debug.Log("Vertical Ray from: " + gameObject.name + " hit " + vertHit.collider.gameObject.name);
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
            _roomInfo.markedForDiscard = true;
            foreach (var room in _roomInfo.allDoors)
            {
                LevelBuilder.Instance.spawnPoints.Remove(room.transform);
            }
            _roomInfo.allDoors.Clear();
            
            if (!LevelBuilder.Instance.discardedRooms.Contains(gameObject))
            {
                LevelBuilder.Instance.discardedRooms.Add(gameObject);
            }
            LevelBuilder.Instance.CleanUpBadRooms();
        } 
        _collider.enabled = true;
        if (discard == false)
        {
            gameObject.layer = LayerMask.NameToLayer("Intersection Checker");
            if (!_checkedTwice)
            {
                //StartCoroutine(SecondRoundInternalCheck());
            }
        }
        FixLayers();
    }
    public IEnumerator SecondRoundInternalCheck()
    {
        yield return new WaitForSecondsRealtime(1f);
        CheckForInternalIntersection();
    }
    
    void Update()
    {
        /*Debug.DrawRay(_leftTopRay.origin, _leftTopRay.direction * (_rayCastLength), Color.red);
        Debug.DrawRay(_rightTopRay.origin, _rightTopRay.direction * (_rayCastLength), Color.green);
        Debug.DrawRay(_leftBottomRay.origin, _leftBottomRay.direction * (_rayCastLength), Color.red);
        Debug.DrawRay(_rightBottomRay.origin, _rightBottomRay.direction * (_rayCastLength), Color.green);
        Debug.DrawRay(_topLeftRay.origin, _topLeftRay.direction * (_rayCastHeight), Color.blue);
        Debug.DrawRay(_bottomLeftRay.origin, _bottomLeftRay.direction * (_rayCastHeight), Color.yellow);
        Debug.DrawRay(_topRightRay.origin, _topRightRay.direction * (_rayCastHeight), Color.blue);
        Debug.DrawRay(_bottomRightRay.origin, _bottomRightRay.direction * (_rayCastHeight), Color.yellow);*/
        Debug.DrawRay(_horizMiddleRay.origin, _horizMiddleRay.direction * (_roomInfo.roomLength), Color.magenta);
        Debug.DrawRay(_verticMiddleRay.origin, _verticMiddleRay.direction * (_roomInfo.roomHeight), Color.magenta);
    }
}
