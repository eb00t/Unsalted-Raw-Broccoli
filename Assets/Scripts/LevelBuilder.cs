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
    public int numberOfRooms;
    public int numberOfConnectors;
    public LevelMode currentFloor;
    private GameObject _startingRoom;
    [field: Header("Debugging")]
    private GameObject _roomToSpawnOn; //The room containing the wall this room used as its spawn position.
    public List<GameObject> possibleRooms; //Rooms that CAN spawn.
    public List<GameObject> possibleConnectors; //Connectors that can spawn.
    public List<GameObject> spawnedRooms; //Rooms that have ALREADY spawned.
    public List<Transform> spawnPoints;
    private List<Vector3>_spawnPointPositions;
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
        SpawnRooms();
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

    Vector3 SpawnConnector(Vector3 newSpawnPoint, bool xAxis)
    {
        GameObject connectorToSpawn = null;
        string path = null;
        ConnectorRoomInfo spawnedConnectorInfo;
        Vector3 connectorNewSpawnPoint;
        switch (xAxis)
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
        connectorToSpawn = Resources.Load<GameObject>(path);
        spawnedConnectorInfo = connectorToSpawn.GetComponent<ConnectorRoomInfo>();
        connectorToSpawn = Instantiate(connectorToSpawn, newSpawnPoint, quaternion.identity);
        //newSpawnPoint = connectorNewSpawnPoint;
        return newSpawnPoint;
    }

    void SpawnRooms()
    {
        int roomRandomNumber, spawnRandomNumber; //RNG 1 and 2
        bool xAxis = true;
        GameObject roomToSpawn; //Room that will be spawned
        GameObject roomSpawnedOn; //Room that the spawned room is spawned on top of
        for (int i = 0; i < numberOfRooms; i++) //Spawn amount of rooms
        {
            roomRandomNumber = RandomiseNumber(possibleRooms.Count); //Spawn a random room from the list of possible rooms
            spawnRandomNumber = RandomiseNumber(spawnPoints.Count); //Choose a random spawn point to spawn the room at
            Vector3 spawnPointPosition = spawnPoints[spawnRandomNumber].position; //Position that the room will use to spawn on
            Vector3 newSpawnPoint = new Vector3(); //WHERE the new room will spawn
            //Debug.Log("Before: " + spawnPointPosition.x + ", " + spawnPointPosition.y);
            spawnedRoomInfo = possibleRooms[roomRandomNumber].GetComponent<RoomInfo>(); //
            roomSpawnedOn = spawnPoints[spawnRandomNumber].root.gameObject; //
            roomSpawnedOnInfo = roomSpawnedOn.GetComponent<RoomInfo>();//Getting the roomInfo components
            switch (spawnPoints[spawnRandomNumber].gameObject.tag) //Move the room based on the distance between where it was going to spawn minus the position of the wall it will spawn on
            {      
                case "Left Door":
                    newSpawnPoint.x = (spawnPointPosition.x - spawnedRoomInfo.doorR.localPosition.x);
                    newSpawnPoint.y = (spawnPointPosition.y - spawnedRoomInfo.doorR.localPosition.y);
                    xAxis = true;
                    spawnedRoomInfo.spawnedOnSide = "Left";
                    break; 
                case "Right Door":
                    newSpawnPoint.x = (spawnPointPosition.x - spawnedRoomInfo.doorL.parent.localPosition.x);
                    newSpawnPoint.y = (spawnPointPosition.y - spawnedRoomInfo.doorL.localPosition.y);
                    xAxis = true;
                    spawnedRoomInfo.spawnedOnSide = "Right";
                    break; 
                case "Bottom Door":
                    newSpawnPoint.y = (spawnPointPosition.y - spawnedRoomInfo.doorT.parent.localPosition.y);
                    newSpawnPoint.x = (spawnPointPosition.x - spawnedRoomInfo.doorT.localPosition.x);
                    xAxis = false;
                    spawnedRoomInfo.spawnedOnSide = "Bottom";
                    break; 
                case "Top Door":
                    newSpawnPoint.y = (spawnPointPosition.y - spawnedRoomInfo.doorB.parent.localPosition.y);
                    newSpawnPoint.x = (spawnPointPosition.x - spawnedRoomInfo.doorB.localPosition.x);
                    xAxis = false;
                    spawnedRoomInfo.spawnedOnSide = "Top";
                   break;
            }
            //Debug.Log("After: " + newSpawnPoint.x + ", " + newSpawnPoint.y);
            SpawnConnector(newSpawnPoint, xAxis);
            roomToSpawn = Instantiate(possibleRooms[roomRandomNumber], newSpawnPoint, Quaternion.identity); //Instantiate the room at the spawnpoint's position
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
       spawnPoints.Remove(spawnPoints[spawnRandomNumber]); //Remove the door the room spawned on from the spawn point list.
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

    IEnumerator WaitToUpdate(GameObject spawnedRoom, GameObject roomSpawnedOn, int roomRandomNumber, int spawnRandomNumber)
    {
        UpdateSpawnWalls(spawnedRoom, roomSpawnedOn, roomRandomNumber, spawnRandomNumber);
        yield return new WaitForSecondsRealtime(1f);
        
    }

    int RandomiseNumber(int setSize)
    {
        int rng = Random.Range(0, setSize);
        return rng;
    }
}


//TODO: Implement connecting rooms (i.e. short hallways between rooms)
