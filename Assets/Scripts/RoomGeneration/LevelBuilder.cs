using System;
using System.Collections;
using System.Collections.Generic;
using FMOD.Studio;
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
        Intermission,
        Tutorial,
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
    public List<GameObject> possibleSpecialRooms;
    public List<GameObject> possibleLootRooms;
    public int roomsRemaining; // Rooms yet to spawn
    public List<GameObject> discardedRooms; // Rooms that were unable to spawn
    public List<GameObject> possibleConnectors; // Connectors that can spawn.
    public List<GameObject> spawnedRooms; // Rooms that have ALREADY spawned.
    public List<GameObject> spawnedBossRooms; //Boss rooms that have spawned.
    public List<GameObject> spawnedLootRooms;
    public List<Transform> firstBossRoomSpawnPoints;
    public List<Transform> secondBossRoomSpawnPoints;
    public List<Transform> thirdBossRoomSpawnPoints;
    public List<Transform> spawnPoints; // Doors that rooms can spawn on
    public RoomInfo spawningRoomInfo; // The RoomInfo component of the room currently being spawned
    public RoomInfo otherConnectorSideRoomInfo; // The RoomInfo component of the last room spawned in.
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
    public List<GameObject> spawnedConnectors;
    public GameObject roomToSpawn;
    public bool roomGeneratingFinished;
    public bool bossRoomGeneratingFinished;
    private string _multiFloorRoomPath, _bossRoomPath, _lootRoomPath;
    public string floorSpecificRoomPath;
    private float _spawnTimer;
    private string _specialRoomPath;
    private GameObject _shop;
    public bool shopSpawned;
    public int lootRoomsToSpawn;
    public bool spawnModeChangedByDestroy;
    public bool bossDead;
    public enum SpawnMode
    {
        Normal,
        BossRooms,
        SpecialRooms,
        Shop,
        LootRoom,
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
        lootRoomsToSpawn = (int)Mathf.Floor((_numberOfRoomsToSpawn / 10));
        Debug.Log(lootRoomsToSpawn);
        if (lootRoomsToSpawn < 2)
        {
            lootRoomsToSpawn = 2;
        }
        _numberOfRoomsToSpawn += lootRoomsToSpawn;
        Debug.Log("Loot rooms to spawn: " + lootRoomsToSpawn);
        StartCoroutine(DelayStart());
    }

    IEnumerator DelayStart()
    {
        if (currentFloor is not (LevelMode.Intermission or LevelMode.Tutorial))
        {
            yield return new WaitForSecondsRealtime(.5f);
            roomsRemaining = _numberOfRoomsToSpawn;
            _startingRoom = GameObject.FindWithTag("StartingRoom");
            otherConnectorSideRoomInfo = _startingRoom.GetComponent<RoomInfo>();
            GetStartingRoomWalls();
            possibleRooms = new List<GameObject>();
            spawnedConnectors = new List<GameObject>();
            AddRoomsToList();
            StartCoroutine(SpawnConnector());
        }
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
        floorSpecificRoomPath = "YOU SHOULDN'T SEE THIS"; // Path for floor exclusive rooms
        _multiFloorRoomPath = "Room Layouts/Multi Floor Rooms"; // Path for rooms used in multiple floors 
        _specialRoomPath = "Room Layouts/Special Rooms"; // Path for special rooms
        _shop = Resources.Load<GameObject>("Room Layouts/Shop/Shop"); // Path for shop
        _lootRoomPath = "Room Layouts/Loot Rooms";
        _bossRoomPath = "YOU STILL SHOULDN'T SEE THIS"; // Path for boss rooms
        string connectorPath = "Room Layouts/Connectors";
        switch (currentFloor)
        {
            case LevelMode.TEST:
                floorSpecificRoomPath = "Room Layouts/Test Rooms";
                _bossRoomPath = "Room Layouts/Boss Rooms/Test";
                break;
            case LevelMode.Floor1:
                floorSpecificRoomPath = "Room Layouts/Floor 1";
                _bossRoomPath = "Room Layouts/Boss Rooms/Copy Boss";
                break;
            case LevelMode.Floor2:
                floorSpecificRoomPath = "Room Layouts/Floor 2";
                _bossRoomPath = "Room Layouts/Boss Rooms/Floor 2";
                break;
            case LevelMode.Floor3:
                floorSpecificRoomPath = "Room Layouts/Floor 3";
                _bossRoomPath = "Room Layouts/Boss Rooms/Hands Boss";
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

        foreach (var specialRoom in Resources.LoadAll<GameObject>(_specialRoomPath))
        {
            possibleSpecialRooms.Add(specialRoom);
        }
        
        foreach (var lootRoom in Resources.LoadAll<GameObject>(_lootRoomPath))
        {
            possibleLootRooms.Add(lootRoom);
        }
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
                case SpawnMode.Normal or SpawnMode.SpecialRooms or SpawnMode.Shop or SpawnMode.LootRoom:
                    spawnRandomNumber = RandomiseNumber(spawnPoints.Count); //  RNG for where to spawn connectors
                    spawnPointPosition = spawnPoints[spawnRandomNumber].position;
                    break;
                case SpawnMode.BossRooms:
                    switch (roomRandomNumber)
                    {
                        case -1:
                        spawnRandomNumber = RandomiseNumber(firstBossRoomSpawnPoints.Count); //  RNG for where to spawn connectors
                        spawnPointPosition = firstBossRoomSpawnPoints[spawnRandomNumber].position;
                            break;
                        case 0:
                            spawnRandomNumber = RandomiseNumber(secondBossRoomSpawnPoints.Count); //  RNG for where to spawn connectors
                            spawnPointPosition = secondBossRoomSpawnPoints[spawnRandomNumber].position;
                            break;
                        case 1:
                            spawnRandomNumber = RandomiseNumber(thirdBossRoomSpawnPoints.Count); //  RNG for where to spawn connectors
                            spawnPointPosition = thirdBossRoomSpawnPoints[spawnRandomNumber].position;
                            break;
                    }

                    break;
            }
            Debug.Log("Spawn Random Number: " + spawnRandomNumber);
            Vector3 newSpawnPoint = Vector3.zero; //    Where the connecting room will spawn

            GameObject otherConnectorSide = null;
            string doorTag = "";
            switch (_spawnMode)
            {
                case SpawnMode.Normal or SpawnMode.SpecialRooms or SpawnMode.Shop or SpawnMode.LootRoom:
                    otherConnectorSide = spawnPoints[spawnRandomNumber].root.gameObject; 
                    doorTag = spawnPoints[spawnRandomNumber].gameObject.tag;
                    break;
                case SpawnMode.BossRooms:
                    switch (roomRandomNumber)
                    {
                        case -1:
                            otherConnectorSide = firstBossRoomSpawnPoints[spawnRandomNumber].root.gameObject;
                            doorTag = firstBossRoomSpawnPoints[spawnRandomNumber].gameObject.tag;
                            break;
                        case 0:
                            otherConnectorSide = secondBossRoomSpawnPoints[spawnRandomNumber].root.gameObject;
                            doorTag = secondBossRoomSpawnPoints[spawnRandomNumber].gameObject.tag;
                            break;
                        case 1:
                            otherConnectorSide = thirdBossRoomSpawnPoints[spawnRandomNumber].root.gameObject;
                            doorTag = thirdBossRoomSpawnPoints[spawnRandomNumber].gameObject.tag;
                            break;
                    }
                    break;
            }
            otherConnectorSideRoomInfo = otherConnectorSide.GetComponent<RoomInfo>(); // Getting the roomInfo components;
            
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
        }
//        MapTargetGroup.Instance.AddRoomsToTargetGroup();
        RerollDiscardedRooms();
    }

    IEnumerator SpawnRooms(Vector3 newSpawnPoint)
    {
       
        switch (_spawnMode) // Picking a random room from the pool of possible rooms
        {
            case SpawnMode.Normal:
                roomRandomNumber = RandomiseNumber(possibleRooms.Count); // Spawn a random room from the list of possible rooms
                spawningRoomInfo = possibleRooms[roomRandomNumber].GetComponent<RoomInfo>();
                break;
            case SpawnMode.Shop: // Only one shop
                roomRandomNumber = 0;
                spawningRoomInfo = _shop.GetComponent<RoomInfo>();
                break;
            case SpawnMode.SpecialRooms:
                roomRandomNumber = RandomiseNumber(possibleSpecialRooms.Count);
                spawningRoomInfo = possibleSpecialRooms[roomRandomNumber].GetComponent<RoomInfo>();
                break;
            case SpawnMode.BossRooms:
                roomRandomNumber++;
                spawningRoomInfo = possibleBossRooms[roomRandomNumber].GetComponent<RoomInfo>();
                break;
            case SpawnMode.LootRoom:
                roomRandomNumber = RandomiseNumber(possibleLootRooms.Count);
                spawningRoomInfo = possibleLootRooms[roomRandomNumber].GetComponent<RoomInfo>();
                break;
        }
        Vector3 realSpawnPosition = Vector3.zero; //    The room's spawn position
        
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

        switch (_spawnMode)
        {
            case SpawnMode.Normal or SpawnMode.BossRooms:
                roomToSpawn = Instantiate(possibleRooms[roomRandomNumber], realSpawnPosition, Quaternion.identity); //  Instantiate the room at the spawnpoint's position
                break;
            case SpawnMode.Shop:
                roomToSpawn = Instantiate(_shop, realSpawnPosition, Quaternion.identity); //There is only one type of shop
                break;
            case SpawnMode.SpecialRooms:
                roomToSpawn = Instantiate(possibleSpecialRooms[roomRandomNumber], realSpawnPosition, Quaternion.identity); //  Instantiate the special room at the spawnpoint's position
                break;
            case SpawnMode.LootRoom:
                roomToSpawn = Instantiate(possibleLootRooms[roomRandomNumber], realSpawnPosition, Quaternion.identity); 
                break;
        }
        spawningRoomInfo = roomToSpawn.GetComponent<RoomInfo>();
        spawningRoomInfo.connectorSpawnedOff = _connectorToSpawn;
        //Debug.Log("Spawned " + possibleRooms[roomRandomNumber] + " at " + realSpawnPosition);
        roomsRemaining--;
        //Debug.Log("Rooms left to spawn: " + roomsRemaining);
        UpdateSpawnWalls();
        yield return null;
    }

    void UpdateSpawnWalls()
    { 
        _spawningRoomIntersectionCheck = spawningRoomInfo.intersectionCheck;
        Debug.Log(_spawningRoomIntersectionCheck.gameObject.name + " is checking for spawning room intersections");

        switch (_spawnMode)
        {
            case SpawnMode.Normal or SpawnMode.SpecialRooms or SpawnMode.Shop or SpawnMode.LootRoom:
                foreach (var door in spawningRoomInfo.doorSpawnPoints) // Adding doors of the spawned room to the list of possible spawn points
                {
                    if (!spawningRoomInfo.markedForDiscard)
                    {
                        spawnPoints.Add(door.transform);
                    }
                } 
                break;   
            case SpawnMode.BossRooms:
                foreach (var door in spawningRoomInfo.doorSpawnPoints) // Adding doors of the spawned room to the list of possible spawn points
                {
                    if (!spawningRoomInfo.markedForDiscard)
                    {
                        switch (roomRandomNumber)
                        {
                            case 0:
                                secondBossRoomSpawnPoints.Add(door.transform);
                                break;
                            case 1:
                                thirdBossRoomSpawnPoints.Add(door.transform);
                                break;
                        }
                    }
                }
                break;  
        }
        if (spawningRoomInfo.specialRoom) 
        {
            possibleSpecialRooms.Remove(possibleSpecialRooms[roomRandomNumber]); // Remove the rare room from the list of rooms that can spawn
        }
        switch (_spawnMode)
        {
            case SpawnMode.Normal or SpawnMode.SpecialRooms or SpawnMode.Shop or SpawnMode.LootRoom: 
                spawnPoints.Remove(spawnPoints[spawnRandomNumber]); //  Remove the door the room spawned on from the spawn point list.
                break;
            case SpawnMode.BossRooms:
                switch (roomRandomNumber)
                {
                    case 0:
                        secondBossRoomSpawnPoints.Remove(firstBossRoomSpawnPoints[spawnRandomNumber]);
                        break;
                    case 1:
                        thirdBossRoomSpawnPoints.Remove(secondBossRoomSpawnPoints[spawnRandomNumber]);
                        break;
                    case 2:
                        //thirdBossRoomSpawnPoints.Remove(thirdBossRoomSpawnPoints[spawnRandomNumber]);
                        break;
                }
                break;
        }
        
        switch (spawnedConnectorInfo.spawnedOnSide) //  Removing spawn points based on where the room spawned.
        {
            case "Left":
                switch (_spawnMode)
                {
                    case SpawnMode.Normal or SpawnMode.SpecialRooms or SpawnMode.Shop or SpawnMode.LootRoom:
                        spawnPoints.Remove(spawningRoomInfo.doorR.transform);
                        spawnPoints.Remove(otherConnectorSideRoomInfo.doorL.transform);
                        break;
                    case SpawnMode.BossRooms:
                        switch (roomRandomNumber)
                        {
                            case 0:
                                secondBossRoomSpawnPoints.Remove(spawningRoomInfo.doorR.transform);
                                firstBossRoomSpawnPoints.Remove(otherConnectorSideRoomInfo.doorL.transform);
                                break;
                            case 1:
                               thirdBossRoomSpawnPoints.Remove(spawningRoomInfo.doorR.transform);
                               secondBossRoomSpawnPoints.Remove(otherConnectorSideRoomInfo.doorL.transform);
                               break;
                            case 2:
                                thirdBossRoomSpawnPoints.Remove(spawningRoomInfo.doorR.transform);
                                break;
                        }
                        spawningRoomInfo.doorSpawnPoints.Remove(spawningRoomInfo.doorR.gameObject);
                        break;
                }
                spawningRoomInfo.canHaveRightRoom = false;
                otherConnectorSideRoomInfo.canHaveLeftRoom = false;
                break;
            case "Right":
                switch (_spawnMode)
                {
                    case SpawnMode.Normal or SpawnMode.SpecialRooms or SpawnMode.Shop or SpawnMode.LootRoom:
                        spawnPoints.Remove(spawningRoomInfo.doorL.transform);
                        spawnPoints.Remove(otherConnectorSideRoomInfo.doorR.transform);
                        break;
                    case SpawnMode.BossRooms:
                        switch (roomRandomNumber)
                        {
                            case 0:
                                secondBossRoomSpawnPoints.Remove(spawningRoomInfo.doorL.transform);
                                firstBossRoomSpawnPoints.Remove(otherConnectorSideRoomInfo.doorR.transform);
                                break;
                            case 1:
                                thirdBossRoomSpawnPoints.Remove(spawningRoomInfo.doorL.transform);
                                secondBossRoomSpawnPoints.Remove(otherConnectorSideRoomInfo.doorR.transform);
                                break;
                            case 2:
                                thirdBossRoomSpawnPoints.Remove(spawningRoomInfo.doorL.transform);
                                break;
                        }
                        spawningRoomInfo.doorSpawnPoints.Remove(spawningRoomInfo.doorL.gameObject);
                        break;
                }
                spawningRoomInfo.canHaveLeftRoom = false;
                otherConnectorSideRoomInfo.canHaveRightRoom = false;
                break;
            case "Top":
                switch (_spawnMode)
                {
                    case SpawnMode.Normal or SpawnMode.SpecialRooms or SpawnMode.Shop or SpawnMode.LootRoom:
                        spawnPoints.Remove(spawningRoomInfo.doorB.transform);
                        spawnPoints.Remove(otherConnectorSideRoomInfo.doorT.transform);
                        break;
                    case SpawnMode.BossRooms:
                        switch (roomRandomNumber)
                        {
                            case 0:
                                secondBossRoomSpawnPoints.Remove(spawningRoomInfo.doorB.transform);
                                firstBossRoomSpawnPoints.Remove(otherConnectorSideRoomInfo.doorT.transform);
                                break;
                            case 1:
                                thirdBossRoomSpawnPoints.Remove(spawningRoomInfo.doorB.transform);
                                secondBossRoomSpawnPoints.Remove(otherConnectorSideRoomInfo.doorT.transform);
                                break;
                            case 2:
                                thirdBossRoomSpawnPoints.Remove(spawningRoomInfo.doorB.transform);
                                break;
                        }
                        spawningRoomInfo.doorSpawnPoints.Remove(spawningRoomInfo.doorB.gameObject);
                        break;
                }
                spawningRoomInfo.canHaveBottomRoom = false;
                otherConnectorSideRoomInfo.canHaveTopRoom = false;
                break;
            case "Bottom":
                switch (_spawnMode)
                {
                    case SpawnMode.Normal or SpawnMode.SpecialRooms or SpawnMode.Shop or SpawnMode.LootRoom:
                         spawnPoints.Remove(spawningRoomInfo.doorT.transform);
                         spawnPoints.Remove(otherConnectorSideRoomInfo.doorB.transform);
                        break;
                    case SpawnMode.BossRooms:
                        switch (roomRandomNumber)
                        {
                        case 0:
                            secondBossRoomSpawnPoints.Remove(spawningRoomInfo.doorT.transform);
                            firstBossRoomSpawnPoints.Remove(otherConnectorSideRoomInfo.doorB.transform);
                            break;
                        case 1:
                            thirdBossRoomSpawnPoints.Remove(spawningRoomInfo.doorT.transform);
                            secondBossRoomSpawnPoints.Remove(otherConnectorSideRoomInfo.doorB.transform);
                            break;
                        case 2:
                            thirdBossRoomSpawnPoints.Remove(spawningRoomInfo.doorT.transform);
                            break;
                        }
                        spawningRoomInfo.doorSpawnPoints.Remove(spawningRoomInfo.doorT.gameObject);
                        break;
                }
                spawningRoomInfo.canHaveTopRoom = false;
                otherConnectorSideRoomInfo.canHaveBottomRoom = false;
                break;
        }
        var rareSpawn = RandomiseNumber(12); //TEMP NUMBER; CHANGE
        Debug.Log("Rare spawn number: " + rareSpawn);
        if (_spawnMode != SpawnMode.BossRooms && spawnModeChangedByDestroy == false) //Special rooms will not spawn when boss rooms are being spawned
        {
            switch (rareSpawn)
            {
                case < 3:
                    _spawnMode = SpawnMode.SpecialRooms;
                    Debug.Log("RARE SPAWNING");
                    if (possibleSpecialRooms.Count <= 0)
                    {
                        _spawnMode = SpawnMode.Normal;
                    }
                    break;
                case > 3 and < 6:
                    if (shopSpawned == false)
                    {
                        //Debug.Log("SHOP SPAWNING");
                        //_spawnMode = SpawnMode.Shop;
                        _spawnMode = SpawnMode.Normal; //Only fix I could think of to stop double shops
                    }  else
                    {
                        _spawnMode = SpawnMode.Normal;
                    }
                    break;
                case > 7 and < 10:
                    if (lootRoomsToSpawn > 0)
                    {
                        Debug.Log("LOOT ROOM SPAWNING");
                        _spawnMode = SpawnMode.LootRoom;
                        lootRoomsToSpawn--;
                    }
                    else
                    {
                        _spawnMode = SpawnMode.Normal;
                    }
                    break;
                default:
                    _spawnMode = SpawnMode.Normal;
                    break;
            }

            if (lootRoomsToSpawn > 0 && roomsRemaining - 1 < lootRoomsToSpawn)
            {
                Debug.Log("FORCED LOOT ROOM SPAWNING");
                _spawnMode = SpawnMode.LootRoom;
                lootRoomsToSpawn--;
            }
            
            if (shopSpawned == false && roomsRemaining == 1)
            {
                Debug.Log("FORCED SHOP SPAWNING");
                _spawnMode = SpawnMode.Shop;
                shopSpawned = true;
            }
        }
        spawnModeChangedByDestroy = false;
    }

    IEnumerator WaitASec()
    {
        yield return new WaitForSeconds(1f);
        for (int i = spawnedRooms.Count; i < spawnedRooms.Count; i--)
        {
            IntersectionRaycast intersectionRaycast = spawnedRooms[i].GetComponent<IntersectionRaycast>();
            if (!spawningRoomInfo.markedForDiscard)
            {
                spawningRoomInfo.canBeDiscarded = false;
            }
            intersectionRaycast.CheckForInternalIntersection();
        }
        if (roomsDiscarded > 0)
        {
           RerollDiscardedRooms();
        }
        else
        {
            Debug.Log("No rooms left to discard!");
            yield return new WaitForSecondsRealtime(.5f);
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
          firstBossRoomSpawnPoints = new List<Transform>(spawnPoints);
          secondBossRoomSpawnPoints = new List<Transform>();
          thirdBossRoomSpawnPoints = new List<Transform>();
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

