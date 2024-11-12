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
    public int totalNumberOfRooms;
    
    public int numberOfConnectors;
    public LevelMode currentFloor;
    private GameObject _startingRoom;

    private GameObject _roomToSpawnOn; // The room containing the wall this room used as its spawn position.
    [field: Header("Debugging")] public List<GameObject> possibleRooms; // Rooms that CAN spawn.
    public int roomsRemaining; // Rooms yet to spawn
    public int discardedRooms; // Rooms that were unable to spawn
    public List<GameObject> possibleConnectors; // Connectors that can spawn.
    public List<GameObject> spawnedRooms; // Rooms that have ALREADY spawned.
    public List<Transform> spawnPoints; // Doors that rooms can spawn on
    public RoomInfo spawningRoomInfo; // The RoomInfo component of the room currently being spawned
    public RoomInfo previouslySpawnedRoomInfo; // The RoomInfo component of the last room spawned in.
    public ConnectorRoomInfo spawnedConnectorInfo; // The ConnectorInfo component of the spawning connector.
    public Transform spawnRoomDoorL;
    public Transform spawnRoomDoorR;
    public Transform spawnRoomDoorT;
    public Transform spawnRoomDoorB;
    private GameObject _connectorToSpawn;
    

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
        roomsRemaining = totalNumberOfRooms;
        _startingRoom = GameObject.FindWithTag("StartingRoom");
        GetStartingRoomWalls();
        possibleRooms = new List<GameObject>();
        AddRoomsToList();
        SpawnConnector();
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
        string floorSpecificRoomPath = "YOU SHOULDN'T SEE THIS"; //Path for floor exclusive rooms
        string multiFloorRoomPath = "YOU SHOULDN'T SEE THIS EITHER"; //TODO: Path for rooms used in multiple floors 
        string connectorPath = "Room Layouts/Connectors";
        switch (currentFloor)
        {
            case LevelMode.TEST:
                floorSpecificRoomPath = "Room Layouts/Test Rooms";
                break;
            case LevelMode.Floor1:
                floorSpecificRoomPath = "Room Layouts/Floor 1";
                break;
            case LevelMode.Floor2:
                floorSpecificRoomPath = "Room Layouts/Floor 2";
                break;
            case LevelMode.Floor3:
                floorSpecificRoomPath = "Room Layouts/Floor 3";
                break;
        }

        foreach (var rooms in Resources.LoadAll<GameObject>(floorSpecificRoomPath))
        {
            possibleRooms.Add(rooms);
        }

        foreach (var rooms in Resources.LoadAll<GameObject>(multiFloorRoomPath))
        {
            possibleRooms.Add(rooms);
        }

        foreach (var rooms in Resources.LoadAll<GameObject>(connectorPath))
        {
            possibleConnectors.Add(rooms);
        }

        Debug.Log(floorSpecificRoomPath + " " + multiFloorRoomPath);
    }

    void SpawnConnector()
    {
        for (int i = 0; i < totalNumberOfRooms; i++) //Spawn amount of rooms
        {
            GameObject connectorPathReference = null;
            int spawnRandomNumber = RandomiseNumber(spawnPoints.Count); //  RNG for where to spawn connectors
            Vector3 spawnPointPosition = spawnPoints[spawnRandomNumber].position; //    Position that the room will use to spawn on
            Vector3 newSpawnPoint = Vector3.zero; //    Where the connecting room will spawn

            GameObject previouslySpawnedRoom = spawnPoints[spawnRandomNumber].root.gameObject; //
            previouslySpawnedRoomInfo = previouslySpawnedRoom.GetComponent<RoomInfo>(); //    Getting the roomInfo components;

            switch (spawnPoints[spawnRandomNumber].gameObject.tag) //  Get the tag of the randomly generated spawn point.
            {
                case "Left Door":
                    Debug.Log("LEFT");
                    connectorPathReference = ConnectorPathSetup("Left"); 
                    spawnedConnectorInfo = connectorPathReference.GetComponent<ConnectorRoomInfo>();
                    newSpawnPoint.x = (spawnPointPosition.x + spawnedConnectorInfo.wallR.transform.localPosition.x);
                    newSpawnPoint.y = (spawnPointPosition.y - spawnedConnectorInfo.wallR.transform.localPosition.y);
                    spawnedConnectorInfo.spawnedOnSide = "Left";
                    break;
                case "Right Door":
                    Debug.Log("RIGHT");
                    connectorPathReference = ConnectorPathSetup("Right");
                    spawnedConnectorInfo = connectorPathReference.GetComponent<ConnectorRoomInfo>();
                    newSpawnPoint.x = (spawnPointPosition.x + spawnedConnectorInfo.wallL.transform.localPosition.x);
                    newSpawnPoint.y = (spawnPointPosition.y - spawnedConnectorInfo.wallL.transform.localPosition.y);
                    spawnedConnectorInfo.spawnedOnSide = "Right";
                    break;
                case "Bottom Door":
                    Debug.Log("BOTTOM");
                    connectorPathReference = ConnectorPathSetup("Bottom");
                    spawnedConnectorInfo = connectorPathReference.GetComponent<ConnectorRoomInfo>();
                    newSpawnPoint.y = (spawnPointPosition.y - spawnedConnectorInfo.wallT.transform.localPosition.y);
                    newSpawnPoint.x = (spawnPointPosition.x - spawnedConnectorInfo.wallT.transform.localPosition.x);
                    spawnedConnectorInfo.spawnedOnSide = "Bottom";
                    break;
                case "Top Door":
                    Debug.Log("TOP");
                    connectorPathReference = ConnectorPathSetup("Top");
                    spawnedConnectorInfo = connectorPathReference.GetComponent<ConnectorRoomInfo>();
                    newSpawnPoint.y = (spawnPointPosition.y - spawnedConnectorInfo.wallB.transform.localPosition.y);
                    newSpawnPoint.x = (spawnPointPosition.x - spawnedConnectorInfo.wallB.transform.localPosition.x);
                    spawnedConnectorInfo.spawnedOnSide = "Top";
                    break;
            }

            //Debug.Log("Connector should spawn at: " + spawnPoints[spawnRandomNumber] + newSpawnPoint);
            _connectorToSpawn = Instantiate(connectorPathReference, newSpawnPoint, quaternion.identity); //  Spawn the connector at the adjusted spawn point.
            
            SpawnRooms(spawnRandomNumber, newSpawnPoint); // Spawn the room on the connector
        }

        MapTargetGroup.Instance.AddRoomsToTargetGroup();
    }

    void SpawnRooms(int spawnRandomNumber, Vector3 newSpawnPoint)
    {
        int roomRandomNumber = RandomiseNumber(possibleRooms.Count); // Spawn a random room from the list of possible rooms
        GameObject roomToSpawn; //  Room that will be spawned
        Vector3 realSpawnPosition = Vector3.zero; //    The room's spawn position
        
        spawningRoomInfo = possibleRooms[roomRandomNumber].GetComponent<RoomInfo>();
        int totalLength = spawningRoomInfo.roomLength / 2 + spawnedConnectorInfo.connectorSize / 2;
        int totalHeight = spawningRoomInfo.roomHeight / 2 + spawnedConnectorInfo.connectorSize / 2;
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
        //CheckSpawnIsValid(roomToSpawn);
        spawningRoomInfo = possibleRooms[roomRandomNumber].GetComponent<RoomInfo>();
        Debug.Log("Spawned " + possibleRooms[roomRandomNumber] + " at " + realSpawnPosition);
        spawningRoomInfo.attachedConnectors.Add(_connectorToSpawn);
        Debug.Log("Adding connector to" + spawningRoomInfo);
        previouslySpawnedRoomInfo.attachedConnectors.Add(_connectorToSpawn);
        Debug.Log("Adding connector to " + previouslySpawnedRoomInfo);
        roomsRemaining--;
        Debug.Log("Rooms left to spawn: " + roomsRemaining);
        spawningRoomInfo.intersectionCheck.CheckForIntersections();
        Debug.Log("Checking for intersecting rooms.");
        //StartCoroutine(WaitToUpdate(roomToSpawn, roomSpawnedOn, roomRandomNumber, spawnRandomNumber));
        UpdateSpawnWalls(roomToSpawn, roomRandomNumber, spawnRandomNumber);
    }

    void UpdateSpawnWalls(GameObject roomToSpawn, int roomRandomNumber, int spawnRandomNumber)
    {
        spawningRoomInfo = roomToSpawn.GetComponent<RoomInfo>();
        Debug.Log("Spawned room: " + spawningRoomInfo.gameObject.name);
        
        Debug.Log("and it spawned on: " + previouslySpawnedRoomInfo.gameObject.name);
        
        foreach (var door in spawningRoomInfo.allDoors) // Adding doors of the spawned room to the list of possible spawn points
        {
            spawnPoints.Add(door.transform);
        } 
        
        spawnedRooms.Add(possibleRooms[roomRandomNumber]); //   Add to the list of rooms already in the level
        //possibleRooms.Remove(possibleRooms[roomRandomNumber]); //Remove the room from the list of rooms that can spawn
        spawnPoints.Remove(spawnPoints[spawnRandomNumber]); //  Remove the door the room spawned on from the spawn point list.
        
        Debug.Log("Spawned on side: " + spawnedConnectorInfo.spawnedOnSide);
        
        switch (spawnedConnectorInfo.spawnedOnSide) //  Removing spawn points based on where the room spawned.
        {
            case "Left":
                Debug.Log("HI LEFT");
                spawnPoints.Remove(spawningRoomInfo.doorR.transform);
                spawnPoints.Remove(previouslySpawnedRoomInfo.doorL.transform);
                spawningRoomInfo.canHaveRightRoom = false;
                previouslySpawnedRoomInfo.canHaveLeftRoom = false;
                break;
            case "Right":
                Debug.Log("HI RIGHT");
                spawnPoints.Remove(spawningRoomInfo.doorL.transform);
                spawnPoints.Remove(previouslySpawnedRoomInfo.doorR.transform);
                spawningRoomInfo.canHaveLeftRoom = false;
                previouslySpawnedRoomInfo.canHaveRightRoom = false;
                break;
            case "Top":
                Debug.Log("HI TOP");
                spawnPoints.Remove(spawningRoomInfo.doorB.transform);
                spawnPoints.Remove(previouslySpawnedRoomInfo.doorT.transform);
                spawningRoomInfo.canHaveBottomRoom = false;
                previouslySpawnedRoomInfo.canHaveTopRoom = false;
                break;
            case "Bottom":
                Debug.Log("HI BOTTOM");
                spawnPoints.Remove(spawningRoomInfo.doorT.transform);
                spawnPoints.Remove(previouslySpawnedRoomInfo.doorB.transform);
                spawningRoomInfo.canHaveTopRoom = false;
                previouslySpawnedRoomInfo.canHaveBottomRoom = false;
                break;
        }
    }

    /*IEnumerator WaitToUpdate(int roomRandomNumber,
        int spawnRandomNumber)
    {
        UpdateSpawnWalls(roomRandomNumber, spawnRandomNumber);
        yield return new WaitForSecondsRealtime(1f);
    }*/

    int RandomiseNumber(int setSize)
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

    public void KillRoomAndConnector(GameObject room, GameObject connector)
    {
        Debug.Log("Destroying invalid room and connector.");
        Destroy(room.gameObject);
        Destroy(connector.gameObject);
    }
}

