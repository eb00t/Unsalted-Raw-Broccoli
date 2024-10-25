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

    [field: Header("Configuration")] public int numberOfRooms;
    public int numberOfConnectors;
    public LevelMode currentFloor;
    private GameObject _startingRoom;
    
    private GameObject _roomToSpawnOn; //The room containing the wall this room used as its spawn position.
    [field: Header("Debugging")]
    public List<GameObject> possibleRooms; //Rooms that CAN spawn.
    public List<GameObject> possibleConnectors; //Connectors that can spawn.
    public List<GameObject> spawnedRooms; //Rooms that have ALREADY spawned.
    public List<Transform> spawnPoints;
    private List<Vector3> _spawnPointPositions;
    public RoomInfo spawnedRoomInfo;
    public RoomInfo roomSpawnedOnInfo;
    public Transform spawnRoomDoorL;
    public Transform spawnRoomDoorR;
    public Transform spawnRoomDoorT;
    public Transform spawnRoomDoorB;

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
        _spawnPointPositions = new List<Vector3>()
        {
            spawnRoomDoorL.transform.position,
            spawnRoomDoorR.transform.position,
            spawnRoomDoorB.transform.position,
            spawnRoomDoorT.transform.position
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
        for (int i = 0; i < numberOfRooms; i++) //Spawn amount of rooms
        { 
            int spawnRandomNumber = RandomiseNumber(spawnPoints.Count); //RNG for where to spawn connectors
            Vector3 spawnPointPosition =
                spawnPoints[spawnRandomNumber].position; //Position that the room will use to spawn on
            Vector3 newSpawnPoint = Vector3.zero; //Where the connecting room will spawn

            GameObject roomSpawnedOn = spawnPoints[spawnRandomNumber].root.gameObject; //
            roomSpawnedOnInfo = roomSpawnedOn.GetComponent<RoomInfo>(); //Getting the roomInfo components;
        
            GameObject connectorToSpawn = null; //The type of connector to spawn

            //bool connectorIsShort = true;
            ConnectorRoomInfo spawnedConnectorInfo = null;
            Vector3 connectorNewSpawnPoint = Vector3.zero; //Where the room will spawn as a result of the connector

            Debug.Log("Connector should spawn at: " + spawnPoints[spawnRandomNumber]);
       
            switch (spawnPoints[spawnRandomNumber].gameObject.tag) //Move the room based on the distance between where it was going to spawn minus the position of the wall it will spawn on
            {
                case "Left Door":
                    Debug.Log("LEFT");
                    connectorToSpawn = ConnectorPathSetup("Left");
                    spawnedConnectorInfo = connectorToSpawn.GetComponent<ConnectorRoomInfo>();
                    newSpawnPoint.x = (spawnPointPosition.x + spawnedConnectorInfo.wallR.transform.localPosition.x);
                    newSpawnPoint.y = (spawnPointPosition.y - spawnedConnectorInfo.wallR.transform.localPosition.y);
                    spawnedConnectorInfo.spawnedOnSide = "Left";
                    connectorNewSpawnPoint = spawnedConnectorInfo.wallL.transform.localPosition;
                    break;
                case "Right Door":
                    Debug.Log("RIGHT");
                    connectorToSpawn = ConnectorPathSetup("Right");
                    spawnedConnectorInfo = connectorToSpawn.GetComponent<ConnectorRoomInfo>();
                    newSpawnPoint.x = (spawnPointPosition.x + spawnedConnectorInfo.wallL.transform.localPosition.x);
                    newSpawnPoint.y = (spawnPointPosition.y - spawnedConnectorInfo.wallL.transform.localPosition.y);
                    spawnedConnectorInfo.spawnedOnSide = "Right";
                    connectorNewSpawnPoint = spawnedConnectorInfo.wallR.transform.localPosition;
                    break;
                case "Bottom Door":
                    Debug.Log("BOTTOM");
                    connectorToSpawn = ConnectorPathSetup("Bottom");
                    spawnedConnectorInfo = connectorToSpawn.GetComponent<ConnectorRoomInfo>();
                    newSpawnPoint.y = (spawnPointPosition.y - spawnedConnectorInfo.wallT.transform.localPosition.y);
                    newSpawnPoint.x = (spawnPointPosition.x - spawnedConnectorInfo.wallT.transform.localPosition.x);
                    spawnedConnectorInfo.spawnedOnSide = "Bottom";
                    connectorNewSpawnPoint = spawnedConnectorInfo.wallB.transform.localPosition;
                    break;
                case "Top Door":
                    Debug.Log("TOP");
                    connectorToSpawn = ConnectorPathSetup("Top");
                    spawnedConnectorInfo = connectorToSpawn.GetComponent<ConnectorRoomInfo>();
                    newSpawnPoint.y = (spawnPointPosition.y - spawnedConnectorInfo.wallB.transform.localPosition.y);
                    newSpawnPoint.x = (spawnPointPosition.x - spawnedConnectorInfo.wallB.transform.localPosition.x);
                    spawnedConnectorInfo.spawnedOnSide = "Top";
                    connectorNewSpawnPoint = spawnedConnectorInfo.wallT.transform.localPosition;
                    break;
            }

            connectorToSpawn = Instantiate(connectorToSpawn, newSpawnPoint, quaternion.identity);

            newSpawnPoint = connectorNewSpawnPoint; //WHERE the new room will spawn
            SpawnRooms(roomSpawnedOn, spawnRandomNumber, newSpawnPoint, spawnedConnectorInfo);
        }
        
    }

    void SpawnRooms(GameObject roomSpawnedOn, int spawnRandomNumber, Vector3 newSpawnPoint, ConnectorRoomInfo spawnedConnectorInfo)
    {
        Debug.Log("New spawn point: " + newSpawnPoint);
        int  roomRandomNumber = RandomiseNumber(possibleRooms.Count); //Spawn a random room from the list of possible rooms
        GameObject roomToSpawn; //Room that will be spawned
        Vector3 realSpawnPosition = Vector3.zero;
       
        //Debug.Log("Before: " + spawnPointPosition.x + ", " + spawnPointPosition.y);
        spawnedRoomInfo = possibleRooms[roomRandomNumber].GetComponent<RoomInfo>();
        switch (spawnedConnectorInfo.spawnedOnSide) //Move the room based on the distance between where it was going to spawn minus the position of the wall it will spawn on
            {
                case "Left":
                    Debug.Log("LEFT");
                    realSpawnPosition.x = (newSpawnPoint.x - spawnedRoomInfo.doorR.transform.localPosition.x);
                    realSpawnPosition.y = (newSpawnPoint.y - spawnedRoomInfo.doorR.transform.localPosition.y);
                    break;
                case "Right":
                    Debug.Log("RIGHT");
                    realSpawnPosition.x = (newSpawnPoint.x + spawnedRoomInfo.doorL.transform.localPosition.x);
                    realSpawnPosition.y = (newSpawnPoint.y - spawnedRoomInfo.doorL.transform.localPosition.y);
                    break;
                case "Bottom":
                    Debug.Log("BOTTOM");
                    realSpawnPosition.y = (newSpawnPoint.y - spawnedRoomInfo.doorT.transform.localPosition.y);
                    realSpawnPosition.x = (newSpawnPoint.x - spawnedRoomInfo.doorT.transform.localPosition.x);
                    break;
                case "Top":
                    Debug.Log("TOP");
                    realSpawnPosition.y = (newSpawnPoint.y + spawnedRoomInfo.doorB.transform.localPosition.y);
                    realSpawnPosition.x = (newSpawnPoint.x - spawnedRoomInfo.doorB.transform.localPosition.x);
                    break;
            }
        Debug.Log("Room should spawn at: " + realSpawnPosition);
        roomToSpawn = Instantiate(possibleRooms[roomRandomNumber], realSpawnPosition, Quaternion.identity); //Instantiate the room at the spawnpoint's position
        Debug.Log("Spawned " + possibleRooms[roomRandomNumber] + " at " + realSpawnPosition);
        StartCoroutine(WaitToUpdate(roomToSpawn, roomSpawnedOn, roomRandomNumber, spawnRandomNumber));
        //UpdateSpawnWalls(roomToSpawn, roomRandomNumber, spawnRandomNumber);
    }

    void UpdateSpawnWalls(GameObject spawnedRoom, GameObject roomSpawnedOn, int roomRandomNumber, int spawnRandomNumber)
    {
        spawnedRoomInfo = spawnedRoom.GetComponent<RoomInfo>();
        roomSpawnedOnInfo = roomSpawnedOn.GetComponent<RoomInfo>();
        foreach (var doors in spawnedRoomInfo.allDoors)
        {
            spawnPoints.Add(doors.transform);
            _spawnPointPositions.Add(doors.transform.position);
        } //Adding doors of the spawned room to the list of possible spawn points

        spawnedRooms.Add(possibleRooms[roomRandomNumber]); //Add to the list of rooms already in the level
        _spawnPointPositions.Remove(possibleRooms[roomRandomNumber].transform.position);
        //possibleRooms.Remove(possibleRooms[roomRandomNumber]); //Remove the room from the list of rooms that can spawn
        spawnPoints.Remove(
            spawnPoints[spawnRandomNumber]); //Remove the door the room spawned on from the spawn point list.
        switch (spawnedRoomInfo.spawnedOnSide) //Removing spawn points based on where the room spawned.
        {
            case "Left":
                Debug.Log("HI LEFT");
                spawnPoints.Remove(spawnedRoomInfo.doorR.transform);
                _spawnPointPositions.Remove(spawnedRoomInfo.doorR.transform.position);
                spawnPoints.Remove(roomSpawnedOnInfo.doorL.transform);
                _spawnPointPositions.Remove(roomSpawnedOnInfo.doorL.transform.position);
                spawnedRoomInfo.canHaveRightRoom = false;
                roomSpawnedOnInfo.canHaveLeftRoom = false;
                break;
            case "Right":
                Debug.Log("HI RIGHT");
                spawnPoints.Remove(spawnedRoomInfo.doorL.transform);
                _spawnPointPositions.Remove(spawnedRoomInfo.doorL.transform.position);
                spawnPoints.Remove(roomSpawnedOnInfo.doorR.transform);
                _spawnPointPositions.Remove(roomSpawnedOnInfo.doorR.transform.position);
                spawnedRoomInfo.canHaveLeftRoom = false;
                roomSpawnedOnInfo.canHaveRightRoom = false;
                break;
            case "Top":
                Debug.Log("HI TOP");
                spawnPoints.Remove(spawnedRoomInfo.doorB.transform);
                _spawnPointPositions.Remove(spawnedRoomInfo.doorB.transform.position);
                spawnPoints.Remove(roomSpawnedOnInfo.doorT.transform);
                _spawnPointPositions.Remove(roomSpawnedOnInfo.doorT.transform.position);
                spawnedRoomInfo.canHaveBottomRoom = false;
                roomSpawnedOnInfo.canHaveTopRoom = false;
                break;
            case "Bottom":
                Debug.Log("HI BOTTOM");
                spawnPoints.Remove(spawnedRoomInfo.doorT.transform);
                _spawnPointPositions.Remove(spawnedRoomInfo.doorT.transform.position);
                spawnPoints.Remove(roomSpawnedOnInfo.doorB.transform);
                _spawnPointPositions.Remove(roomSpawnedOnInfo.doorB.transform.position);
                spawnedRoomInfo.canHaveTopRoom = false;
                roomSpawnedOnInfo.canHaveBottomRoom = false;
                break;
        }
    }

    IEnumerator WaitToUpdate(GameObject spawnedRoom, GameObject roomSpawnedOn, int roomRandomNumber,
        int spawnRandomNumber)
    {
        UpdateSpawnWalls(spawnedRoom, roomSpawnedOn, roomRandomNumber, spawnRandomNumber);
        yield return new WaitForSecondsRealtime(1f);
    }

    int RandomiseNumber(int setSize)
    {
        int rng = Random.Range(0, setSize);
        return rng;
    }

    string RandomiseConnectorLength(bool length)
    {
        string path = "";
        switch (length)
        {
            case true:
                switch (RandomiseNumber(2))
                {
                    case 0:
                        path = "Room Layouts/Connectors/ConnectorShortHoriz";
                        break;

                    case 1:
                        path = "Room Layouts/Connectors/ConnectorLongHoriz";
                        break;
                }

                break;
            case false:
                switch (RandomiseNumber(2))
                {
                    case 0:
                        path = "Room Layouts/Connectors/ConnectorShortVerti";
                        break;

                    case 1:
                        path = "Room Layouts/Connectors/ConnectorLongVerti";
                        break;
                }

                break;
        }

    return path;
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
}



//TODO: Implement connecting rooms (i.e. short hallways between rooms)
