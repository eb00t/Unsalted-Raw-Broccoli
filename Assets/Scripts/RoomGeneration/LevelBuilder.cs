using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cinemachine;
using FMOD.Studio;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.SceneManagement;
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
        FinalBoss,
        TitleScreen,
    }

    [field: Header("Configuration")] 
    public bool manuallySetRoomAmount;
    public int howManyRoomsToSpawn;
    public int roomsDiscarded;
    public LevelMode currentFloor;
    public GameObject _startingRoom;
    public int randomSeed = 0; // The same seed will cause the same rooms and same layouts to spawn, except boss rooms.
    
    [field: Header("Debugging")] 
    private int _numberOfRoomsToSpawn;
    public int lootRoomsToSpawn;
    private GameObject _roomToSpawnOn; // The room containing the wall this room used as its spawn position.
    private GameObject _connectorToSpawn;
    
    public List<GameObject> possibleRooms; // Rooms that CAN spawn.
    public List<GameObject> possibleBossRooms;
    public List<GameObject> possibleSpecialRooms;
    public List<GameObject> possibleLootRooms;
    public List<GameObject> possibleLoreRooms;
    public List<GameObject> possibleShops;
    
    public int roomsRemaining; // Rooms yet to spawn
    public List<GameObject> discardedRooms; // Rooms that were unable to spawn
    public List<GameObject> possibleConnectors; // Connectors that can spawn.
    public List<GameObject> spawnedRooms; // Rooms that have ALREADY spawned.
    public List<GameObject> spawnedBossRooms; //Boss rooms that have spawned.
    public List<GameObject> spawnedLootRooms;
    public List<GameObject> spawnedLoreRooms;
    public List<GameObject> spawnedShops;
    public List<Transform> firstBossRoomSpawnPoints;
    public List<Transform> secondBossRoomSpawnPoints;
    public List<Transform> thirdBossRoomSpawnPoints;
    public List<Transform> spawnPoints; // Doors that rooms can spawn on
    public List<Transform> lootRoomSpawnPoints; // Doors loot rooms can spawn on (to prevent them from spawning on each other)
    public RoomInfo spawningRoomInfo; // The RoomInfo component of the room currently being spawned
    public RoomInfo otherConnectorSideRoomInfo; // The RoomInfo component of the last room spawned in.
    public ConnectorRoomInfo spawnedConnectorInfo; // The ConnectorInfo component of the spawning connector.
    public Transform spawnRoomDoorL;
    public Transform spawnRoomDoorR;
    public Transform spawnRoomDoorT;
    public Transform spawnRoomDoorB;
    public int roomRandomNumber;
    public int spawnRandomNumber;
    private IntersectionRaycast _spawningRoomIntersectionCheck;
    private ConnectorIntersectionRaycast _spawnedConnectorIntersectionCheck;
    public List<GameObject> spawnedConnectors;
    public GameObject roomToSpawn;
    public bool roomGeneratingFinished;
    public bool bossRoomGeneratingFinished;
    public bool bossDead;
    public bool loreRoomChance;
    private bool _spawnValid;
    private string _multiFloorRoomPath, _bossRoomPath, _lootRoomPath, _shopPath;
    private string _floorSpecificRoomPath;
    private string _specialRoomPath;
    private string _loreRoomPath;
    private float _spawnTimer;
    private int _spawnFailCount;
    [SerializeField] private DataHolder dataHolder;
    public enum SpawnMode
    {
        Normal,
        BossRooms,
        SpecialRooms,
        Shops,
        LootRooms,
        LoreRooms
    }

    public SpawnMode _spawnMode;

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("More than one LevelBuilder script in the scene.");
        }

        Instance = this;
        if (SceneManager.GetActiveScene().name != "Tutorial" && SceneManager.GetActiveScene().name != "Intermission" && SceneManager.GetActiveScene().name != "StartScreen")
        {
            currentFloor = dataHolder.currentLevel;
        }

        if (manuallySetRoomAmount == false)
        {
            switch (currentFloor)
            {
                case LevelMode.Floor1:
                    howManyRoomsToSpawn = 5;
                    break;
                case LevelMode.Floor2:
                    howManyRoomsToSpawn = 7;
                    break;
                case LevelMode.Floor3:
                    howManyRoomsToSpawn = 9;
                    break;
                case LevelMode.FinalBoss:
                    howManyRoomsToSpawn = 0;
                    break;
            }
        }
    }

    void Start()
    {
        if (randomSeed == 0)
        {
            randomSeed = (int)DateTime.Now.Ticks;
        }
        Random.InitState(randomSeed);
        _spawnMode = SpawnMode.Normal;
        _numberOfRoomsToSpawn = howManyRoomsToSpawn;
        int spawnLoreRoom = RandomiseNumber(2);
        if (loreRoomChance == false)
        {
            switch (spawnLoreRoom)
            {
                case 0:
                    loreRoomChance = false;
                    break;
                case 1:
                    loreRoomChance = true;
                    break;
            }
        }

        lootRoomsToSpawn = (int)Mathf.Floor((_numberOfRoomsToSpawn / 10));
        Debug.Log(lootRoomsToSpawn);
        if (lootRoomsToSpawn < 2)
        {
            lootRoomsToSpawn = 2;
        }

        if (currentFloor == LevelMode.FinalBoss)
        {
            lootRoomsToSpawn = 0;
        }
        _numberOfRoomsToSpawn += lootRoomsToSpawn;
        Debug.Log("Loot rooms to spawn: " + lootRoomsToSpawn);
        StartCoroutine(DelayStart());
    }

    IEnumerator DelayStart()
    {
        if (currentFloor is not (LevelMode.Intermission or LevelMode.Tutorial or LevelMode.TitleScreen))
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
        lootRoomSpawnPoints = new List<Transform>(spawnPoints);
    }

    void AddRoomsToList()
    {
        _floorSpecificRoomPath = "YOU SHOULDN'T SEE THIS"; // Path for floor exclusive rooms
        _multiFloorRoomPath = "Room Layouts/Multi Floor Rooms"; // Path for rooms used in multiple floors 
        _specialRoomPath = "Room Layouts/Special Rooms"; // Path for special rooms
        _shopPath = "Room Layouts/Shop"; // Path for shop
        _lootRoomPath = "Room Layouts/Loot Rooms"; // Path for loot rooms
        _loreRoomPath = "Room Layouts/Lore Rooms"; // Path for lore rooms
        _bossRoomPath = "YOU STILL SHOULDN'T SEE THIS"; // Path for boss rooms
        string connectorPath = "Room Layouts/Connectors"; // Path for connectors
        switch (currentFloor) // Switching the type of rooms spawned based on the current floor
        {
            case LevelMode.TEST:
                _floorSpecificRoomPath = "Room Layouts/Test Rooms";
                _bossRoomPath = "Room Layouts/Boss Rooms/Test";
                break;
            case LevelMode.Floor1:
                _floorSpecificRoomPath = "Room Layouts/Floor 1";
                _bossRoomPath = "Room Layouts/Boss Rooms/Copy Boss";
                break;
            case LevelMode.Floor2:
                _floorSpecificRoomPath = "Room Layouts/Floor 2";
                _bossRoomPath = "Room Layouts/Boss Rooms/CloneBoss";
                break;
            case LevelMode.Floor3:
                _floorSpecificRoomPath = "Room Layouts/Floor 3";
                _bossRoomPath = "Room Layouts/Boss Rooms/Empty Boss";
                break;
            case LevelMode.FinalBoss:
                _bossRoomPath = "Room Layouts/Boss Rooms/Hands Boss";
                break;
        }

        foreach (var room in Resources.LoadAll<GameObject>(_floorSpecificRoomPath))
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

        foreach (var shop in Resources.LoadAll<GameObject> (_shopPath))
        {
            possibleShops.Add(shop);
        }
        
        foreach (var specialRoom in Resources.LoadAll<GameObject>(_specialRoomPath))
        {
            possibleSpecialRooms.Add(specialRoom);
        }
        
        foreach (var lootRoom in Resources.LoadAll<GameObject>(_lootRoomPath))
        {
            possibleLootRooms.Add(lootRoom);
        }

        foreach (var loreRoom in Resources.LoadAll<GameObject>(_loreRoomPath))
        {
            possibleLoreRooms.Add(loreRoom);
        }
    }

    IEnumerator SpawnConnector()
    {
        _spawnValid = false;
        for (int i = 0; i < _numberOfRoomsToSpawn; i++) //Spawn amount of rooms
        {
            yield return new WaitForSeconds(.1f);
            GameObject connectorPathReference = null;
            Vector3 spawnPointPosition = Vector3.zero; //Position that the room will use to spawn on
            switch (_spawnMode)
            {
                case SpawnMode.Normal or SpawnMode.SpecialRooms or SpawnMode.Shops or SpawnMode.LoreRooms:
                    spawnRandomNumber = RandomiseNumber(spawnPoints.Count); //  RNG for where to spawn connectors
                    spawnPointPosition = spawnPoints[spawnRandomNumber].position;
                    break;
                case SpawnMode.LootRooms:
                    spawnRandomNumber = RandomiseNumber(lootRoomSpawnPoints.Count);
                    if (lootRoomSpawnPoints.Count > 0)
                    {
                        spawnPointPosition = lootRoomSpawnPoints[spawnRandomNumber].position;
                    }
                    else
                    {
                        spawnPointPosition = spawnPoints[spawnRandomNumber].position;
                    }
                    break;
                case SpawnMode.BossRooms:
                    switch (roomRandomNumber)
                    {
                        case -1:
                            spawnRandomNumber = RandomiseNumber(firstBossRoomSpawnPoints.Count);//  RNG for where to spawn connectors
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
            Vector3 newSpawnPoint = Vector3.zero; // Where the connecting room will spawn
            GameObject otherConnectorSide = null; // The room on the other side of the connector
            string doorTag = "";
            switch (_spawnMode)
            {
                case SpawnMode.Normal or SpawnMode.SpecialRooms or SpawnMode.Shops or SpawnMode.LoreRooms:
                    otherConnectorSide = spawnPoints[spawnRandomNumber].root.gameObject; 
                    doorTag = spawnPoints[spawnRandomNumber].gameObject.tag;
                    break;
                case SpawnMode.LootRooms:
                    otherConnectorSide = lootRoomSpawnPoints[spawnRandomNumber].root.gameObject;
                    doorTag = lootRoomSpawnPoints[spawnRandomNumber].gameObject.tag;
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
                    newSpawnPoint.y = (spawnPointPosition.y + spawnedConnectorInfo.wallB.localPosition.y);
                    newSpawnPoint.x = (spawnPointPosition.x - spawnedConnectorInfo.wallB.localPosition.x);
                    spawnedConnectorInfo.spawnedOnSide = "Bottom";
                    break;
                case "Top Door":
                    Debug.Log("TOP");
                    connectorPathReference = ConnectorPathSetup("Top");
                    spawnedConnectorInfo = connectorPathReference.GetComponent<ConnectorRoomInfo>();
                    newSpawnPoint.y = (spawnPointPosition.y + spawnedConnectorInfo.wallT.localPosition.y);
                    newSpawnPoint.x = (spawnPointPosition.x - spawnedConnectorInfo.wallT.localPosition.x);
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
        _spawnValid = false;
        CheckIfRoomConnectorComboIsValid(_spawnMode); // Check if the instance of the room is OK.
        Vector3 realSpawnPosition = Vector3.zero; // The room's spawn position
        float totalLength = spawningRoomInfo.roomLength / 2 + spawnedConnectorInfo.connectorLength / 2;
        float totalHeight = spawningRoomInfo.roomHeight / 2 + spawnedConnectorInfo.connectorHeight / 2;
        switch (spawnedConnectorInfo.spawnedOnSide) /*  Move the spawn point based on the length or width of the room, minus the x or y position of 
                                                        the wall it will spawn on   */
        {
            case "Left":
                Debug.Log("LEFT");
                realSpawnPosition.x = (newSpawnPoint.x - totalLength - 1); //   1 is the wall thickness. Do NOT change it.
                if (spawningRoomInfo.doorR != null)
                {
                    realSpawnPosition.y = (newSpawnPoint.y - (spawningRoomInfo.doorR.transform.localPosition.y * spawningRoomInfo.roomHeight));
                }
                break;
            case "Right":
                Debug.Log("RIGHT");
                realSpawnPosition.x = (newSpawnPoint.x + totalLength + 1);
                if (spawningRoomInfo.doorL != null)
                {
                    realSpawnPosition.y = (newSpawnPoint.y - (spawningRoomInfo.doorL.transform.localPosition.y * spawningRoomInfo.roomHeight));
                }
                break;
            case "Bottom":
                Debug.Log("BOTTOM");
                realSpawnPosition.y = (newSpawnPoint.y - totalHeight - 1);
                if (spawningRoomInfo.doorT != null)
                {
                    realSpawnPosition.x = (newSpawnPoint.x - (spawningRoomInfo.doorT.transform.localPosition.x * spawningRoomInfo.roomLength));
                }
                break;
            case "Top":
                Debug.Log("TOP");
                realSpawnPosition.y = (newSpawnPoint.y + totalHeight + 1);
                if (spawningRoomInfo.doorB != null)
                {
                    realSpawnPosition.x = (newSpawnPoint.x - (spawningRoomInfo.doorB.transform.localPosition.x * spawningRoomInfo.roomLength));
                }
                break;
        }
        roomToSpawn = Instantiate(roomToSpawn, realSpawnPosition, Quaternion.identity); //  Instantiate the room at the spawnpoint's position.
        spawningRoomInfo = roomToSpawn.GetComponent<RoomInfo>(); // Get the RoomInfo component of the room instance.
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
            case SpawnMode.Normal or SpawnMode.SpecialRooms or SpawnMode.Shops or SpawnMode.LoreRooms:
                foreach (var door in spawningRoomInfo.doorSpawnPoints) // Adding doors of the spawned room to the list of possible spawn points
                {
                    if (!spawningRoomInfo.markedForDiscard)
                    {
                        spawnPoints.Add(door.transform);
                        lootRoomSpawnPoints.Add(door.transform);
                    }
                } 
                break;
            case SpawnMode.LootRooms: // Stop loot rooms spawning right next to each other
                foreach (var door in spawningRoomInfo.doorSpawnPoints)
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
        if (spawningRoomInfo.specialRoom && !spawningRoomInfo.markedForDiscard) 
        {
            possibleSpecialRooms.Remove(possibleSpecialRooms[roomRandomNumber]); // Remove the rare room from the list of rooms that can spawn
        }

        if (!spawningRoomInfo.markedForDiscard)
        {
            switch (_spawnMode)
            {
                case SpawnMode.Normal or SpawnMode.SpecialRooms or SpawnMode.Shops or SpawnMode.LootRooms
                    or SpawnMode.LoreRooms:
                    spawnPoints.Remove(spawnPoints[spawnRandomNumber]); //  Remove the door the room spawned on from the spawn point list.
                    lootRoomSpawnPoints.Remove(spawnPoints[spawnRandomNumber]);
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
                        case SpawnMode.Normal or SpawnMode.SpecialRooms or SpawnMode.Shops or SpawnMode.LootRooms
                            or SpawnMode.LoreRooms:
                            spawnPoints.Remove(spawningRoomInfo.doorR.transform);
                            spawnPoints.Remove(otherConnectorSideRoomInfo.doorL.transform);
                            lootRoomSpawnPoints.Remove(spawningRoomInfo.doorR.transform);
                            lootRoomSpawnPoints.Remove(otherConnectorSideRoomInfo.doorL.transform);
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

                    break;
                case "Right":
                    switch (_spawnMode)
                    {
                        case SpawnMode.Normal or SpawnMode.SpecialRooms or SpawnMode.Shops or SpawnMode.LootRooms
                            or SpawnMode.LoreRooms:
                            spawnPoints.Remove(spawningRoomInfo.doorL.transform);
                            spawnPoints.Remove(otherConnectorSideRoomInfo.doorR.transform);
                            lootRoomSpawnPoints.Remove(spawningRoomInfo.doorL.transform);
                            lootRoomSpawnPoints.Remove(otherConnectorSideRoomInfo.doorR.transform);
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

                    break;
                case "Top":
                    switch (_spawnMode)
                    {
                        case SpawnMode.Normal or SpawnMode.SpecialRooms or SpawnMode.Shops or SpawnMode.LootRooms
                            or SpawnMode.LoreRooms:
                            spawnPoints.Remove(spawningRoomInfo.doorB.transform);
                            spawnPoints.Remove(otherConnectorSideRoomInfo.doorT.transform);
                            lootRoomSpawnPoints.Remove(spawningRoomInfo.doorB.transform);
                            lootRoomSpawnPoints.Remove(otherConnectorSideRoomInfo.doorT.transform);
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

                    break;
                case "Bottom":
                    switch (_spawnMode)
                    {
                        case SpawnMode.Normal or SpawnMode.SpecialRooms or SpawnMode.Shops or SpawnMode.LootRooms
                            or SpawnMode.LoreRooms:
                            spawnPoints.Remove(spawningRoomInfo.doorT.transform);
                            spawnPoints.Remove(otherConnectorSideRoomInfo.doorB.transform);
                            lootRoomSpawnPoints.Remove(spawningRoomInfo.doorT.transform);
                            lootRoomSpawnPoints.Remove(otherConnectorSideRoomInfo.doorB.transform);
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

                    break;
            }
        }
        var rareSpawn = RandomiseNumber(100);
        Debug.Log("Rare spawn number: " + rareSpawn);
        if (_spawnMode != SpawnMode.BossRooms) //Special rooms will not spawn when boss rooms are being spawned
        {
            switch (rareSpawn)
            {
                case <= 20:
                    _spawnMode = SpawnMode.SpecialRooms;
                    Debug.Log("RARE SPAWNING");
                    if (possibleSpecialRooms.Count <= 0)
                    {
                        _spawnMode = SpawnMode.Normal;
                    }
                    break;
                case > 20 and <= 40:
                    //Debug.Log("SHOP SPAWNING");
                    //_spawnMode = SpawnMode.Shop;
                    _spawnMode = SpawnMode.Normal; //Only fix I could think of to stop double shops
                    break;
                case > 40 and <= 60:
                    if (loreRoomChance)
                    {
                        _spawnMode = SpawnMode.LoreRooms;
                    } 
                    else if (loreRoomChance == false && possibleSpecialRooms.Count <= 0)
                    {
                        _spawnMode = SpawnMode.SpecialRooms;
                    }
                    else
                    {
                        _spawnMode = SpawnMode.Normal;
                    }
                    break;
                case > 60 and <= 90:
                    if (lootRoomsToSpawn > spawnedLootRooms.Count && lootRoomSpawnPoints != null)
                    {
                        Debug.Log("LOOT ROOM SPAWNING");
                        _spawnMode = SpawnMode.LootRooms;
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

            if (spawnedLootRooms.Count < lootRoomsToSpawn && roomsRemaining is 2 or 3)
            {
                Debug.Log("FORCED LOOT ROOM SPAWNING");
                _spawnMode = SpawnMode.LootRooms;
            }
            if (spawnedShops.Count == 0 && roomsRemaining == 1)
            {
                Debug.Log("FORCED SHOP SPAWNING");
                _spawnMode = SpawnMode.Shops;
            }
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
            yield return new WaitForSecondsRealtime(.5f);
            SpawnBossRoom();
            roomGeneratingFinished = true;
            
            foreach (var room in spawnedRooms)
            {
                RoomScripting roomScript = room.GetComponent<RoomScripting>();
                roomScript.CheckDoors();
            }
        }
    }

  void SpawnBossRoom()
  {
      if (_spawnMode != SpawnMode.BossRooms)
      {
          roomRandomNumber = -1;
          if (currentFloor != LevelMode.FinalBoss)
          {
              foreach (var room in _startingRoom.GetComponent<RoomInfo>().doorSpawnPoints)
              {
                  spawnPoints.Remove(room.transform);
              }
              firstBossRoomSpawnPoints = new List<Transform>(lootRoomSpawnPoints);
          }
          else
          {
              firstBossRoomSpawnPoints = new List<Transform>(spawnPoints);
          }
          secondBossRoomSpawnPoints = new List<Transform>();
          thirdBossRoomSpawnPoints = new List<Transform>();
          _numberOfRoomsToSpawn = possibleBossRooms.Count;
          _spawnMode = SpawnMode.BossRooms;
          StartCoroutine(SpawnConnector());
      }
      if (spawnedBossRooms.Count == 3)
      {
          bossRoomGeneratingFinished = true;
          foreach (var room in spawnedRooms)
          {
              room.GetComponent<RoomScripting>().CheckDoors();
          }
          
          foreach (var connector in spawnedConnectors)
          {
              if (connector.GetComponent<ConnectorRoomInfo>().attachedRooms.Count < 2)
              {
                  Destroy(connector.gameObject);
              }
          }

          foreach (var room in spawnedRooms)
          {
              room.GetComponent<RoomScripting>().CheckDoors();
          }
          AudioManager.Instance.SetEventParameter(AudioManager.Instance.loadingEventInstance, "Level Loaded", 1);
          _startingRoom.GetComponent<RoomInfo>().roomCam.Priority = 99999999;
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
            case "Left" or "Right":
                path = "Room Layouts/Connectors/ConnectorShortHoriz"; //TEMP CODE: MAY BE REPLACED
                break;
            case "Top" or "Bottom":
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
       foreach (var room in discardedRooms.ToList())
       {
           RoomInfo badRoomInfo = room.GetComponent<RoomInfo>();
           if (badRoomInfo.markedForDiscard)
           {
               Debug.Log(room.name + " has been discarded.");
               discardedRooms.Remove(room);
               //Destroy(badRoomInfo.connectorSpawnedOff.gameObject);
               Destroy(room);
           }
           roomsDiscarded++;
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
    }

    void CheckIfRoomConnectorComboIsValid(SpawnMode spawnMode)
    {
        switch (spawnMode)
        {
            case SpawnMode.Normal:
                roomRandomNumber = RandomiseNumber(possibleRooms.Count); // Spawn a random room from the list of possible rooms
                roomToSpawn = possibleRooms[roomRandomNumber];
                break;
            case SpawnMode.SpecialRooms:
                roomRandomNumber = RandomiseNumber(possibleSpecialRooms.Count);
                roomToSpawn = possibleSpecialRooms[roomRandomNumber];
                break;
            case SpawnMode.LoreRooms:
                roomRandomNumber = RandomiseNumber(possibleLoreRooms.Count);
                roomToSpawn = possibleLoreRooms[roomRandomNumber];
                break;   
            case SpawnMode.LootRooms:
                roomRandomNumber = RandomiseNumber(possibleLootRooms.Count);
                roomToSpawn = possibleLootRooms[roomRandomNumber];
                break; 
            case SpawnMode.BossRooms:
                roomRandomNumber++;
                roomToSpawn = possibleBossRooms[roomRandomNumber];
                break;
            case SpawnMode.Shops:
                roomRandomNumber = RandomiseNumber(possibleShops.Count);
                roomToSpawn = possibleShops[roomRandomNumber];
                break;
        }
        spawningRoomInfo = roomToSpawn.GetComponent<RoomInfo>(); // The only values referenced here are all hardcoded, i.e. they're the same for every instance of the specific room.
        switch (spawnedConnectorInfo.spawnedOnSide)
        {
            case "Left" when spawningRoomInfo.missingRightDoor:
            case "Right" when spawningRoomInfo.missingLeftDoor:
            case "Top" when spawningRoomInfo.missingBottomDoor:
            case "Bottom" when spawningRoomInfo.missingTopDoor:
                if (_spawnFailCount < 10)
                {
                    _spawnValid = false;
                    Debug.Log("Room and connector combo (" + roomToSpawn + " and " + spawnedConnectorInfo.spawnedOnSide + ") is not valid (" + _spawnFailCount + ")" );
                    _spawnFailCount++;
                    CheckIfRoomConnectorComboIsValid(spawnMode);
                }
                else
                {
                    spawnMode = SpawnMode.Normal;
                    Debug.Log("Spawn " + roomToSpawn + " has failed completely, resetting to normal rooms.");
                }
                break;
            default:
                _spawnValid = true;
                _spawnFailCount = 0;
                break;
        }
    }
    
    private void Update()
    {
        if (spawnRandomNumber > spawnPoints.Count && _spawnMode != SpawnMode.LootRooms)
        {
            Debug.Log("Spawn points are out of range.");
            spawnRandomNumber = RandomiseNumber(spawnPoints.Count);
        }

        if (spawnRandomNumber > lootRoomSpawnPoints.Count && _spawnMode == SpawnMode.LootRooms)
        {
            Debug.Log("Spawn points are out of range.");
            spawnRandomNumber = RandomiseNumber(lootRoomSpawnPoints.Count);
        }

        _spawnTimer -= Time.deltaTime;
    }
}