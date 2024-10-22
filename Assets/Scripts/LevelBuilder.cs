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

    [field: Header("Debugging")]
    private GameObject _roomToSpawnOn; //The room containing the wall this room used as its spawn position.

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
        spawnRoomDoorL = _startingRoom.GetComponent<RoomInfo>().doorT;
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

        foreach (var rooms in Resources.LoadAll<GameObject>(connectorPath))
        {
            possibleConnectors.Add(rooms);
        }

        Debug.Log(floorSpecificRoomPath + " " + multiFloorRoomPath);
    }

    void SpawnConnector()
    {
        var spawnRandomNumber = RandomiseNumber(spawnPoints.Count); //RNG for where to spawn connectors
        Vector3 spawnPointPosition = spawnPoints[spawnRandomNumber].position; //Position that the room will use to spawn on
        Vector3 newSpawnPoint = Vector3.zero; //Where the connecting room will spawn

        GameObject roomSpawnedOn = spawnPoints[spawnRandomNumber].root.gameObject; //
        roomSpawnedOnInfo = roomSpawnedOn.GetComponent<RoomInfo>(); //Getting the roomInfo components;
        
        bool xAxis = true; //Whether the room spawned on the x-axis or y-axis
        GameObject connectorToSpawn = null; //The type of connector to spawn
        
        string path = "NOTHING..."; //Path for the connector#
        bool connectorIsShort = true;
        ConnectorRoomInfo spawnedConnectorInfo;
        Vector3 connectorNewSpawnPoint; //Where the room will spawn as a result of the connector
      
        
        switch (spawnPoints[spawnRandomNumber].gameObject.tag) //Move the room based on the distance between where it was going to spawn minus the position of the wall it will spawn on
        {
            case "Left Door":
                connectorToSpawn = ConnectorPathSetup("Left");
                Debug.Log("LEFT");
                spawnedConnectorInfo = connectorToSpawn.GetComponent<ConnectorRoomInfo>();
                newSpawnPoint.x = (spawnPointPosition.x - spawnedConnectorInfo.wallR.transform.localPosition.x);
                newSpawnPoint.y = (spawnPointPosition.y - spawnedConnectorInfo.wallR.transform.localPosition.y);
                spawnedConnectorInfo.spawnedOnSide = "Left";
                break;
            case "Right Door":
                connectorToSpawn = ConnectorPathSetup("Right");
                Debug.Log("RIGHT");
                spawnedConnectorInfo = connectorToSpawn.GetComponent<ConnectorRoomInfo>();
                newSpawnPoint.x = (spawnPointPosition.x + spawnedConnectorInfo.wallL.transform.localPosition.x);
                newSpawnPoint.y = (spawnPointPosition.y - spawnedConnectorInfo.wallL.transform.localPosition.y);
                spawnedConnectorInfo.spawnedOnSide = "Right";
                break;
            case "Bottom Door":
                connectorToSpawn = ConnectorPathSetup("Bottom");
                Debug.Log("BOTTOM");
                spawnedConnectorInfo = connectorToSpawn.GetComponent<ConnectorRoomInfo>();
                newSpawnPoint.y = (spawnPointPosition.y - spawnedConnectorInfo.wallT.transform.localPosition.y);
                newSpawnPoint.x = (spawnPointPosition.x - spawnedConnectorInfo.wallT.transform.localPosition.x);
                spawnedConnectorInfo.spawnedOnSide = "Bottom";
                break;
            case "Top Door":
                connectorToSpawn = ConnectorPathSetup("Top");
                Debug.Log("TOP");
                spawnedConnectorInfo = connectorToSpawn.GetComponent<ConnectorRoomInfo>();
                newSpawnPoint.y = (spawnPointPosition.y - spawnedConnectorInfo.wallB.transform.localPosition.y);
                newSpawnPoint.x = (spawnPointPosition.x - spawnedConnectorInfo.wallB.transform.localPosition.x);
                spawnedConnectorInfo.spawnedOnSide = "Top";
                break;
        }
        connectorToSpawn = Instantiate(connectorToSpawn, newSpawnPoint, quaternion.identity);
        //newSpawnPoint = connectorNewSpawnPoint;
    }

    void SpawnRooms(GameObject roomSpawnedOn, int spawnRandomNumber)
    {
        int roomRandomNumber; //RNG for the type of room to spawn
        GameObject roomToSpawn; //Room that will be spawned
        for (int i = 0; i < numberOfRooms; i++) //Spawn amount of rooms
        {
            roomRandomNumber =
                RandomiseNumber(possibleRooms.Count); //Spawn a random room from the list of possible rooms
            //Choose a random spawn point to spawn the room at
            Vector3 newSpawnPoint = new Vector3(); //WHERE the new room will spawn
            //Debug.Log("Before: " + spawnPointPosition.x + ", " + spawnPointPosition.y);
            spawnedRoomInfo = possibleRooms[roomRandomNumber].GetComponent<RoomInfo>(); //
            

            //Debug.Log("After: " + newSpawnPoint.x + ", " + newSpawnPoint.y);
            roomToSpawn = Instantiate(possibleRooms[roomRandomNumber], newSpawnPoint,
                    Quaternion.identity); //Instantiate the room at the spawnpoint's position
            Debug.Log("Spawned " + possibleRooms[roomRandomNumber] + " at " + newSpawnPoint);
            StartCoroutine(WaitToUpdate(roomToSpawn, roomSpawnedOn, roomRandomNumber, spawnRandomNumber));
            //UpdateSpawnWalls(roomToSpawn, roomRandomNumber, spawnRandomNumber);
        }
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
        SpawnConnector();
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
        switch (side)
        {
            case "Left" or "Right":
                path = "Room Layouts/Connectors/ConnectorShortHoriz"; //TEMP CODE: MAY BE REPLACED
                break;
            case "Top" or "Bottom": 
                path = "Room Layouts/Connectors/ConnectorShortVerti"; //TEMP CODE: MAY BE REPLACED
                break;
        }
        GameObject connectorToSpawn = Resources.Load<GameObject>(path);
        return connectorToSpawn;
    }
}



//TODO: Implement connecting rooms (i.e. short hallways between rooms)
