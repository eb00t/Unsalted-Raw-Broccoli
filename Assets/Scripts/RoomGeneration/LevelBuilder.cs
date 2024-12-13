using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class LevelBuilder : MonoBehaviour
{
    public static LevelBuilder Instance { get; private set; }

    public enum LevelMode
    {
        TEST,
        Floor1,
        Floor2,
        Floor3,
    }

    [field: Header("Configuration")] 
    private int _numberOfRoomsToSpawn;
    public int howManyRoomsToSpawn;
    public int roomsDiscarded;
    public LevelMode currentFloor;
    private GameObject _startingRoom;

    private GameObject _roomToSpawnOn; // The room containing the wall this room used as its spawn position.
    [field: Header("Debugging")] 
    public List<GameObject> possibleRooms; // Rooms that CAN spawn.
    public List<GameObject> possibleBossRooms;
    public int roomsRemaining; // Rooms yet to spawn
    public List<GameObject> discardedRooms; // Rooms that were unable to spawn
    public List<GameObject> possibleConnectors; // Connectors that can spawn.
    public List<GameObject> spawnedRooms; // Rooms that have ALREADY spawned.
    public List<GameObject> spawnedBossRooms; //Boss rooms that have spawned.
    public List<Transform> bossRoomSpawnPoints;
    public List<Transform> spawnPoints; // Doors that rooms can spawn on
    public RoomInfo spawningRoomInfo; // The RoomInfo component of the room currently being spawned
    public RoomInfo previouslySpawnedRoomInfo; // The RoomInfo component of the last room spawned in.
    public ConnectorRoomInfo spawnedConnectorInfo; // The ConnectorInfo component of the spawning connector.
    public Transform spawnRoomDoorL;
    public Transform spawnRoomDoorR;
    public Transform spawnRoomDoorT;
    public Transform spawnRoomDoorB;
    private GameObject _connectorToSpawn;
    public int roomRandomNumber;
    public int spawnRandomNumber;
    private IntersectionRaycast _spawningRoomIntersectionCheck;
    private ConnectorIntersectionRaycast _spawnedConnectorIntersectionCheck;
    public List<GameObject> _spawnedConnectors;
    public GameObject roomToSpawn;
    public bool roomGeneratingFinished;
    public bool bossRoomGeneratingFinished;
    private string _multiFloorRoomPath, _bossRoomPath;
    public string floorSpecificRoomPath;
    private float _spawnTimer;

    public enum SpawnMode
    {
        Normal,
        BossRooms,
    }

    public SpawnMode _spawnMode;
    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("More than one LevelBuilder script in the scene.");
        }

        Instance = this;
    }

    void Start()
    {
        _spawnMode = SpawnMode.Normal;
        _numberOfRoomsToSpawn = howManyRoomsToSpawn;
        StartCoroutine(DelayStart());
    }

    IEnumerator DelayStart()
    {
        yield return new WaitForSecondsRealtime(.5f);
        roomsRemaining = _numberOfRoomsToSpawn;
        _startingRoom = GameObject.FindWithTag("StartingRoom");
        GetStartingRoomWalls();
        possibleRooms = new List<GameObject>();
        _spawnedConnectors = new List<GameObject>();
        AddRoomsToList();
        StartCoroutine(SpawnConnector());
    }
    void GetStartingRoomWalls()
    {
        spawnRoomDoorL = _startingRoom.GetComponent<RoomInfo>().doorL;
        spawnRoomDoorR = _startingRoom.GetComponent<RoomInfo>().doorR;
        spawnRoomDoorB = _startingRoom.GetComponent<RoomInfo>().doorB;
        spawnRoomDoorT = _startingRoom.GetComponent<RoomInfo>().doorT;
        spawnPoints = new List<Transform>()
        {
            spawnRoomDoorL.transform,
            spawnRoomDoorR.transform,
            spawnRoomDoorB.transform,
            spawnRoomDoorT.transform
        };
    }

    void AddRoomsToList()
    {
        floorSpecificRoomPath = "YOU SHOULDN'T SEE THIS"; //Path for floor exclusive rooms
        _multiFloorRoomPath = "Room Layouts/Multi Floor Rooms"; //TODO: Path for rooms used in multiple floors 
        _bossRoomPath = "YOU STILL SHOULDN'T SEE THIS";
        string connectorPath = "Room Layouts/Connectors";
        switch (currentFloor)
        {
            case LevelMode.TEST:
                floorSpecificRoomPath = "Room Layouts/Test Rooms";
                _bossRoomPath = "Room Layouts/Boss Rooms/Test";
                break;
            case LevelMode.Floor1:
                floorSpecificRoomPath = "Room Layouts/Floor 1";
                _bossRoomPath = "Room Layouts/Boss Rooms/Floor 1";
                break;
            case LevelMode.Floor2:
                floorSpecificRoomPath = "Room Layouts/Floor 2";
                _bossRoomPath = "Room Layouts/Boss Rooms/Floor 2";
                break;
            case LevelMode.Floor3:
                floorSpecificRoomPath = "Room Layouts/Floor 3";
                _bossRoomPath = "Room Layouts/Boss Rooms/Floor 3";
                break;
        }

        foreach (var room in Resources.LoadAll<GameObject>(floorSpecificRoomPath))
        {
            possibleRooms.Add(room);
        }

        foreach (var room in Resources.LoadAll<GameObject>(_multiFloorRoomPath))
        {
            possibleRooms.Add(room);
        }

        foreach (var connector in Resources.LoadAll<GameObject>(connectorPath))
        {
            possibleConnectors.Add(connector);
        }
        
        foreach (var bossRoom in Resources.LoadAll<GameObject>(_bossRoomPath))
        {
            possibleBossRooms.Add(bossRoom);
        }

        Debug.Log(floorSpecificRoomPath + " " + _multiFloorRoomPath);
    }

    IEnumerator SpawnConnector()
    {
        for (int i = 0; i < _numberOfRoomsToSpawn; i++) //Spawn amount of rooms
        {
            yield return new WaitForSeconds(.1f);
            GameObject connectorPathReference = null;
            
           
            Vector3 spawnPointPosition = Vector3.zero; //    Position that the room will use to spawn on
            switch (_spawnMode)
            {
                case SpawnMode.Normal:
                    spawnRandomNumber = RandomiseNumber(spawnPoints.Count); //  RNG for where to spawn connectors
                    spawnPointPosition = spawnPoints[spawnRandomNumber].position;
                    break;
                case SpawnMode.BossRooms:
                    spawnRandomNumber = RandomiseNumber(bossRoomSpawnPoints.Count); //  RNG for where to spawn connectors
                    spawnPointPosition = bossRoomSpawnPoints[spawnRandomNumber].position;
                    break;
            }
            Debug.Log("Spawn Random Number: " + spawnRandomNumber);
            Vector3 newSpawnPoint = Vector3.zero; //    Where the connecting room will spawn

            GameObject previouslySpawnedRoom = null;
            string doorTag = "";
            switch (_spawnMode)
            {
                case SpawnMode.Normal:
                    previouslySpawnedRoom = spawnPoints[spawnRandomNumber].root.gameObject; 
                    doorTag = spawnPoints[spawnRandomNumber].gameObject.tag;
                    break;
                case SpawnMode.BossRooms:
                    previouslySpawnedRoom = bossRoomSpawnPoints[spawnRandomNumber].root.gameObject;
                    doorTag = bossRoomSpawnPoints[spawnRandomNumber].gameObject.tag;
                    break;
            }
            previouslySpawnedRoomInfo = previouslySpawnedRoom.GetComponent<RoomInfo>(); // Getting the roomInfo components;
            
            switch (doorTag) // Get the tag of the randomly generated spawn point.
            {
                case "Left Door":
                    Debug.Log("LEFT");
                    connectorPathReference = ConnectorPathSetup("Left"); 
                    spawnedConnectorInfo = connectorPathReference.GetComponent<ConnectorRoomInfo>();
                    newSpawnPoint.x = (spawnPointPosition.x + spawnedConnectorInfo.wallL.localPosition.x);
                    newSpawnPoint.y = (spawnPointPosition.y - spawnedConnectorInfo.wallL.localPosition.y);
                    spawnedConnectorInfo.spawnedOnSide = "Left";
                    break;
                case "Right Door":
                    Debug.Log("RIGHT");
                    connectorPathReference = ConnectorPathSetup("Right");
                    spawnedConnectorInfo = connectorPathReference.GetComponent<ConnectorRoomInfo>();
                    newSpawnPoint.x = (spawnPointPosition.x + spawnedConnectorInfo.wallR.localPosition.x);
                    newSpawnPoint.y = (spawnPointPosition.y - spawnedConnectorInfo.wallR.localPosition.y);
                    spawnedConnectorInfo.spawnedOnSide = "Right";
                    break;
                case "Bottom Door":
                    Debug.Log("BOTTOM");
                    connectorPathReference = ConnectorPathSetup("Bottom");
                    spawnedConnectorInfo = connectorPathReference.GetComponent<ConnectorRoomInfo>();
                    newSpawnPoint.y = (spawnPointPosition.y - spawnedConnectorInfo.wallT.localPosition.y);
                    newSpawnPoint.x = (spawnPointPosition.x - spawnedConnectorInfo.wallT.localPosition.x);
                    spawnedConnectorInfo.spawnedOnSide = "Bottom";
                    break;
                case "Top Door":
                    Debug.Log("TOP");
                    connectorPathReference = ConnectorPathSetup("Top");
                    spawnedConnectorInfo = connectorPathReference.GetComponent<ConnectorRoomInfo>();
                    newSpawnPoint.y = (spawnPointPosition.y - spawnedConnectorInfo.wallB.localPosition.y);
                    newSpawnPoint.x = (spawnPointPosition.x - spawnedConnectorInfo.wallB.localPosition.x);
                    spawnedConnectorInfo.spawnedOnSide = "Top";
                    break;
            }
            _connectorToSpawn = Instantiate(connectorPathReference, newSpawnPoint, quaternion.identity); //  Spawn the connector at the adjusted spawn point.
            StartCoroutine(SpawnRooms(newSpawnPoint)); // Spawn the room on the connector
            if (_spawnMode == SpawnMode.BossRooms)
            {
                bossRoomSpawnPoints.Clear();
            }
        }
//        MapTargetGroup.Instance.AddRoomsToTargetGroup();
        RerollDiscardedRooms();
    }

    IEnumerator SpawnRooms(Vector3 newSpawnPoint)
    {
        yield return new WaitForSeconds(0.1f);
        switch (_spawnMode)
        {
            case SpawnMode.Normal:
             roomRandomNumber = RandomiseNumber(possibleRooms.Count); // Spawn a random room from the list of possible rooms
                break;
            case SpawnMode.BossRooms:
                roomRandomNumber++;
                break;
        }
        if (roomToSpawn != null)
        {
            previouslySpawnedRoomInfo = roomToSpawn.GetComponent<RoomInfo>();
        }
        Vector3 realSpawnPosition = Vector3.zero; //    The room's spawn position
        
        spawningRoomInfo = possibleRooms[roomRandomNumber].GetComponent<RoomInfo>();
        float totalLength = spawningRoomInfo.roomLength / 2 + spawnedConnectorInfo.connectorLength / 2;
        float totalHeight = spawningRoomInfo.roomHeight / 2 + spawnedConnectorInfo.connectorHeight / 2;
        switch (spawnedConnectorInfo.spawnedOnSide) /*  Move the spawn point based on the length or width of the room, minus the x or y position of 
                                                        the wall it will spawn on   */
        {
            case "Left":
                Debug.Log("LEFT");
                realSpawnPosition.x = (newSpawnPoint.x - totalLength - 1); //   1 is the wall thickness. Do NOT change it.
                realSpawnPosition.y = (newSpawnPoint.y - (spawningRoomInfo.doorR.transform.localPosition.y * spawningRoomInfo.roomHeight));
                break;
            case "Right":
                Debug.Log("RIGHT");
                realSpawnPosition.x = (newSpawnPoint.x + totalLength + 1);
                realSpawnPosition.y = (newSpawnPoint.y - (spawningRoomInfo.doorL.transform.localPosition.y * spawningRoomInfo.roomHeight));
                break;
            case "Bottom":
                Debug.Log("BOTTOM");
                realSpawnPosition.y = (newSpawnPoint.y - totalHeight - 1);
                realSpawnPosition.x = (newSpawnPoint.x - (spawningRoomInfo.doorT.transform.localPosition.x * spawningRoomInfo.roomLength));
                break;
            case "Top":
                Debug.Log("TOP");
                realSpawnPosition.y = (newSpawnPoint.y + totalHeight + 1);
                realSpawnPosition.x = (newSpawnPoint.x - (spawningRoomInfo.doorB.transform.localPosition.x * spawningRoomInfo.roomLength));
                break;
        }
        roomToSpawn = Instantiate(possibleRooms[roomRandomNumber], realSpawnPosition, Quaternion.identity); //  Instantiate the room at the spawnpoint's position
        spawningRoomInfo = roomToSpawn.GetComponent<RoomInfo>();
        spawningRoomInfo.connectorSpawnedOff = _connectorToSpawn;
        Debug.Log("Spawned " + possibleRooms[roomRandomNumber] + " at " + realSpawnPosition);
        roomsRemaining--;
        Debug.Log("Rooms left to spawn: " + roomsRemaining);
        
        UpdateSpawnWalls();
    }

    void UpdateSpawnWalls()
    { 
        _spawningRoomIntersectionCheck = spawningRoomInfo.intersectionCheck;
        Debug.Log(_spawningRoomIntersectionCheck.gameObject.name + " is checking for spawning room intersections");

        switch (_spawnMode)
        {
            case SpawnMode.Normal:
                foreach (var door in spawningRoomInfo.doorSpawnPoints) // Adding doors of the spawned room to the list of possible spawn points
                {
                    spawnPoints.Add(door.transform);
                } 
                break;   
            case SpawnMode.BossRooms:
                foreach (var door in spawningRoomInfo.doorSpawnPoints) // Adding doors of the spawned room to the list of possible spawn points
                {
                    bossRoomSpawnPoints.Add(door.transform);
                } 
                break;  
        }
        if (spawningRoomInfo.rareRoom) 
        {
            possibleRooms.Remove(possibleRooms[roomRandomNumber]); // Remove the rare room from the list of rooms that can spawn
        }
        switch (_spawnMode)
        {
            case SpawnMode.Normal: 
                spawnPoints.Remove(spawnPoints[spawnRandomNumber]); //  Remove the door the room spawned on from the spawn point list.
                break;
            case SpawnMode.BossRooms:
                bossRoomSpawnPoints.Remove(bossRoomSpawnPoints[spawnRandomNumber]);
                break;
        }
        
        switch (spawnedConnectorInfo.spawnedOnSide) //  Removing spawn points based on where the room spawned.
        {
            case "Left":
                switch (_spawnMode)
                {
                    case SpawnMode.Normal: 
                        spawnPoints.Remove(spawningRoomInfo.doorR.transform);
                        spawnPoints.Remove(previouslySpawnedRoomInfo.doorL.transform);
                        break;
                    case SpawnMode.BossRooms:
                        bossRoomSpawnPoints.Remove(spawningRoomInfo.doorR.transform);
                        spawningRoomInfo.doorSpawnPoints.Remove(spawningRoomInfo.doorR.gameObject);
                        if (previouslySpawnedRoomInfo != null)
                        {
                            bossRoomSpawnPoints.Remove(previouslySpawnedRoomInfo.doorL.transform);
                        }
                        break;
                }
                spawningRoomInfo.canHaveRightRoom = false;
                previouslySpawnedRoomInfo.canHaveLeftRoom = false;
                break;
            case "Right":
                switch (_spawnMode)
                {
                    case SpawnMode.Normal:
                        spawnPoints.Remove(spawningRoomInfo.doorL.transform);
                        spawnPoints.Remove(previouslySpawnedRoomInfo.doorR.transform);
                        break;
                    case SpawnMode.BossRooms:
                        bossRoomSpawnPoints.Remove(spawningRoomInfo.doorL.transform);
                        spawningRoomInfo.doorSpawnPoints.Remove(spawningRoomInfo.doorL.gameObject);
                        if (previouslySpawnedRoomInfo != null)
                        {
                            bossRoomSpawnPoints.Remove(previouslySpawnedRoomInfo.doorR.transform);
                        }

                        break;
                }
                spawningRoomInfo.canHaveLeftRoom = false;
                previouslySpawnedRoomInfo.canHaveRightRoom = false;
                break;
            case "Top":
                switch (_spawnMode)
                {
                    case SpawnMode.Normal:
                         spawnPoints.Remove(spawningRoomInfo.doorB.transform);
                         spawnPoints.Remove(previouslySpawnedRoomInfo.doorT.transform);
                        break;
                    case SpawnMode.BossRooms:
                        bossRoomSpawnPoints.Remove(spawningRoomInfo.doorB.transform);
                        spawningRoomInfo.doorSpawnPoints.Remove(spawningRoomInfo.doorB.gameObject);
                        if (previouslySpawnedRoomInfo != null)
                        {
                            bossRoomSpawnPoints.Remove(previouslySpawnedRoomInfo.doorT.transform);
                        }
                        break;
                }
                spawningRoomInfo.canHaveBottomRoom = false;
                previouslySpawnedRoomInfo.canHaveTopRoom = false;
                break;
            case "Bottom":
                switch (_spawnMode)
                {
                    case SpawnMode.Normal:
                         spawnPoints.Remove(spawningRoomInfo.doorT.transform);
                         spawnPoints.Remove(previouslySpawnedRoomInfo.doorB.transform);
                        break;
                    case SpawnMode.BossRooms:
                        bossRoomSpawnPoints.Remove(spawningRoomInfo.doorT.transform);
                        spawningRoomInfo.doorSpawnPoints.Remove(spawningRoomInfo.doorT.gameObject);
                        if (previouslySpawnedRoomInfo != null)
                        {
                            bossRoomSpawnPoints.Remove(previouslySpawnedRoomInfo.doorB.transform);
                        }
                        break;
                }
                spawningRoomInfo.canHaveTopRoom = false;
                previouslySpawnedRoomInfo.canHaveBottomRoom = false;
                break;
        }
    }

    IEnumerator WaitASec()
    {
        yield return new WaitForSeconds(1f);
        for (int i = spawnedRooms.Count; i < spawnedRooms.Count; i--)
        {
            IntersectionRaycast intersectionRaycast = spawnedRooms[i].GetComponent<IntersectionRaycast>();
            intersectionRaycast.CheckForInternalIntersection();
        }
        if (roomsDiscarded > 0)
        {
           RerollDiscardedRooms();
        }
        else
        {
            Debug.Log("No rooms left to discard!");
            SpawnBossRoom();
            roomGeneratingFinished = true;
            AudioManager.Instance.SetEventParameter(AudioManager.Instance.loadingEventInstance, "Level Loaded", 1);
            foreach (var room in spawnedRooms)
            {
                RoomScripting roomScript = room.GetComponent<RoomScripting>();
                roomScript.CheckDoors();
                IntersectionRaycast intersectionRaycast = room.GetComponent<IntersectionRaycast>();
                intersectionRaycast.FixWallLayers();
            }
        }
    }

  void SpawnBossRoom()
  {
      if (_spawnMode != SpawnMode.BossRooms)
      {
          foreach (var room in _startingRoom.GetComponent<RoomInfo>().doorSpawnPoints)
          {
              spawnPoints.Remove(room.transform);
          }
          roomRandomNumber = -1;
          foreach (var spawnPoint in spawnPoints)
          {
              bossRoomSpawnPoints.Add(spawnPoint);
          }
          _numberOfRoomsToSpawn = possibleBossRooms.Count;
          possibleRooms.Clear();
          _spawnMode = SpawnMode.BossRooms;
          foreach (var bossRoom in possibleBossRooms)
          {
              possibleRooms.Add(bossRoom);
          }
          StartCoroutine(SpawnConnector());
      }
      if (spawnedBossRooms.Count == 3)
      {
          bossRoomGeneratingFinished = true;
      }
  }

  public void RegenerateBossRooms()
  {
      foreach (var room in spawnedBossRooms)
      {
          Destroy(room);
      }
      spawnedBossRooms.Clear();
      roomRandomNumber = 0;
      possibleRooms.Clear();
      foreach (var room in possibleBossRooms)
      {
          possibleRooms.Add(room);
      }
      SpawnBossRoom();
  }

  public int RandomiseNumber(int setSize)
    {
        int rng = Random.Range(0, setSize);
        return rng;
    }

    GameObject ConnectorPathSetup(string side)
    {
        string path = "";
        Debug.Log(side);
        switch (side)
        {
            case "Left":
                path = "Room Layouts/Connectors/ConnectorShortHoriz"; //TEMP CODE: MAY BE REPLACED
                break;
            case "Right":
                path = "Room Layouts/Connectors/ConnectorShortHoriz"; //TEMP CODE: MAY BE REPLACED
                break;
            case "Top":
                path = "Room Layouts/Connectors/ConnectorShortVerti"; //TEMP CODE: MAY BE REPLACED
                break;
            case "Bottom":
                path = "Room Layouts/Connectors/ConnectorShortVerti"; //TEMP CODE: MAY BE REPLACED
                break;
            default:
                path = "Room Layouts/Connectors/ConnectorShortHoriz";
                break;
        }

        GameObject connectorToSpawn = Resources.Load<GameObject>(path);
        return connectorToSpawn;
    }

    void CheckSpawnIsValid(GameObject roomToSpawn)
    {
        spawningRoomInfo = roomToSpawn.GetComponent<RoomInfo>();
        bool isValid = true;
        //CHECK IF THE ROOM CAN SPAWN ON THAT SIDE
        switch (spawnedConnectorInfo.spawnedOnSide)
        {
            case "Left" when spawningRoomInfo.canSpawnOnLeft == false:
                isValid = false;
                break;
            case "Right" when spawningRoomInfo.canSpawnOnRight == false:
                isValid = false;
                break;
            case "Top" when spawningRoomInfo.canSpawnOnTop == false:
                isValid = false;
                break;
            case "Bottom" when spawningRoomInfo.canSpawnOnBottom == false:
                isValid = false;
                break;
            default: 
                break;
        }
        
        if (isValid == false)
        {
            Debug.LogWarning("Impossible spawn.");
            //KillRoomAndConnector();
        }
        else
        {
            Debug.Log("Spawn is valid.");
        }
    }

    public void CleanUpBadRooms()
    {
       foreach (var room in discardedRooms)
       {
           RoomInfo badRoomInfo = room.GetComponent<RoomInfo>();
           if (badRoomInfo.markedForDiscard)
           {
               Debug.Log(room.name + " has been discarded.");
               Destroy(badRoomInfo.connectorSpawnedOff);
               Destroy(room);
           }
       }
    }


    void RerollDiscardedRooms()
    {
        _numberOfRoomsToSpawn = roomsDiscarded;
        if (_numberOfRoomsToSpawn > 0)
        {
            roomsDiscarded = 0;
            StartCoroutine(SpawnConnector());
        }
        else if (_numberOfRoomsToSpawn <= 0)
        {
            if (roomsDiscarded > 0)
            {
                _numberOfRoomsToSpawn = discardedRooms.Count;
                StartCoroutine(SpawnConnector());
            }
            else
            {
                StartCoroutine(WaitASec());
            }
        }
        foreach (var room in spawnedRooms)
        {
            //room.GetComponent<IntersectionRaycast>()._collider.enabled = false;
        }
        
    }

    private void Update()
    {
        if (spawnRandomNumber > spawnPoints.Count)
        {
            Debug.Log("Spawn points are out of range.");
            spawnRandomNumber = RandomiseNumber(spawnPoints.Count);
        }

        _spawnTimer -= Time.deltaTime;
    }
}

