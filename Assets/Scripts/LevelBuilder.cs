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
    public LevelMode currentFloor;
    private GameObject _startingRoom;
    [field: Header("Debugging")]
    private GameObject _roomToSpawnOn; //The room containing the wall this room used as its spawn position.
    public List<GameObject> possibleRooms; //Rooms that CAN spawn.
    public List<GameObject> spawnedRooms; //Rooms that have ALREADY spawned.
    public List<Transform> spawnPoints;
    private List<Vector3>_spawnPointPositions;
    public RoomInfo spawnedRoomInfo;
    public RoomInfo roomSpawnedOnInfo;
    
    public Transform spawnWallL, spawnWallR, spawnWallT, spawnWallB;

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
        spawnWallL = _startingRoom.GetComponent<RoomInfo>().wallL;
        spawnWallR = _startingRoom.GetComponent<RoomInfo>().wallR;
        spawnWallB = _startingRoom.GetComponent<RoomInfo>().wallB;
        spawnWallT = _startingRoom.GetComponent<RoomInfo>().wallT;
        spawnPoints = new List<Transform>()
        {
            spawnWallL.transform,
            spawnWallR.transform,
            spawnWallB.transform,
            spawnWallT.transform
        };
       _spawnPointPositions = new List<Vector3>()
        {
            spawnWallL.transform.position,
            spawnWallR.transform.position,
            spawnWallB.transform.position,
            spawnWallT.transform.position
        };
    }
    void AddRoomsToList()
    {
        string floorSpecificRoomPath = "YOU SHOULDN'T SEE THIS"; //Path for floor exclusive rooms
        string multiFloorRoomPath = "YOU SHOULDN'T SEE THIS EITHER"; //TODO: Path for rooms used in multiple floors 
        //TODO: Add string for connecting rooms.
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
        Debug.Log(floorSpecificRoomPath + " " + multiFloorRoomPath);
    }

    void SpawnRooms()
    {
        int roomRandomNumber, spawnRandomNumber; //RNG 1 and 2
        GameObject roomToSpawn; //Room that will be spawned
        GameObject roomSpawnedOn;
        for (int i = 0; i < numberOfRooms; i++) //Spawn amount of rooms
        {
            roomRandomNumber = RandomiseNumber(possibleRooms.Count); //Spawn a random room from the list of possible rooms
            spawnRandomNumber = RandomiseNumber(spawnPoints.Count); //Choose a random spawn point to spawn the room at
            Transform spawnPoint = spawnPoints[spawnRandomNumber]; //GameObject that the room will spawn on
            Vector3 newSpawnPoint = new Vector3();
            Vector3 spawnPointPosition = spawnPoint.position; 
            //Debug.Log("Before: " + spawnPointPosition.x + ", " + spawnPointPosition.y);
            spawnedRoomInfo = possibleRooms[roomRandomNumber].GetComponent<RoomInfo>();
            roomSpawnedOn = spawnPoints[spawnRandomNumber].root.gameObject;
            roomSpawnedOnInfo = roomSpawnedOn.GetComponent<RoomInfo>();
            switch (spawnPoints[spawnRandomNumber].gameObject.tag) //Move the room based on the distance between its centre and the wall it will spawn on
            {      
                case "Left Wall":
                    newSpawnPoint.x = (spawnPointPosition.x - spawnedRoomInfo.wallR.localPosition.x);
                    newSpawnPoint.y = (spawnPointPosition.y - spawnedRoomInfo.wallR.localPosition.y);
                    spawnedRoomInfo.spawnedOnSide = "Left";
                    break; 
                case "Right Wall":
                    newSpawnPoint.x = (spawnPointPosition.x - spawnedRoomInfo.wallL.localPosition.x);
                    newSpawnPoint.x = (spawnPointPosition.x - spawnedRoomInfo.wallR.localPosition.x);
                    spawnedRoomInfo.spawnedOnSide = "Right";
                    break; 
                case "Bottom Wall":
                    newSpawnPoint.y = (spawnPointPosition.y - spawnedRoomInfo.wallT.localPosition.y);
                    newSpawnPoint.x = (spawnPointPosition.x - spawnedRoomInfo.wallT.localPosition.x);
                    spawnedRoomInfo.spawnedOnSide = "Bottom";
                    break; 
                case "Top Wall":
                    newSpawnPoint.y = (spawnPointPosition.y - spawnedRoomInfo.wallB.localPosition.y);
                    newSpawnPoint.y = (spawnPointPosition.y - spawnedRoomInfo.wallB.localPosition.y);
                    spawnedRoomInfo.spawnedOnSide = "Top";
                   break;
            }
            Debug.Log("After: " + newSpawnPoint.x + ", " + newSpawnPoint.y);
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
        spawnPoints.Add(spawnedRoomInfo.wallT.transform); //
        spawnPoints.Add(spawnedRoomInfo.wallB.transform); //
        spawnPoints.Add(spawnedRoomInfo.wallL.transform); //
        spawnPoints.Add(spawnedRoomInfo.wallR.transform); //Adding walls of the spawned room to the list of possible spawn points
        spawnedRooms.Add(possibleRooms[roomRandomNumber]); //Add to the list of rooms already in the level
       _spawnPointPositions.Add(spawnedRoomInfo.wallT.transform.position); //
       _spawnPointPositions.Add(spawnedRoomInfo.wallB.transform.position); //
       _spawnPointPositions.Add(spawnedRoomInfo.wallL.transform.position); //
       _spawnPointPositions.Add(spawnedRoomInfo.wallR.transform.position);
       _spawnPointPositions.Remove(possibleRooms[roomRandomNumber].transform.position);
       //possibleRooms.Remove(possibleRooms[roomRandomNumber]); //Remove the room from the list of rooms that can spawn
       spawnPoints.Remove(spawnPoints[spawnRandomNumber]); //Remove the wall the room spawned on from the spawn point list.
       switch (spawnedRoomInfo.spawnedOnSide)
       {
           case "Left":
               Debug.Log("HI LEFT");
               spawnPoints.Remove(spawnedRoomInfo.wallR.transform);
              _spawnPointPositions.Remove(spawnedRoomInfo.wallR.transform.position);
               spawnPoints.Remove(roomSpawnedOnInfo.wallL.transform);
               _spawnPointPositions.Remove(roomSpawnedOnInfo.wallL.transform.position);
               spawnedRoomInfo.canHaveRightRoom = false;
               roomSpawnedOnInfo.canHaveLeftRoom = false;
               break;
           case "Right":
               Debug.Log("HI RIGHT");
               spawnPoints.Remove(spawnedRoomInfo.wallL.transform);
              _spawnPointPositions.Remove(spawnedRoomInfo.wallL.transform.position);
               spawnPoints.Remove(roomSpawnedOnInfo.wallR.transform);
               _spawnPointPositions.Remove(roomSpawnedOnInfo.wallR.transform.position);
               spawnedRoomInfo.canHaveLeftRoom = false;
               roomSpawnedOnInfo.canHaveRightRoom = false;
               break;
           case "Top":
               Debug.Log("HI TOP");
               spawnPoints.Remove(spawnedRoomInfo.wallB.transform);
              _spawnPointPositions.Remove(spawnedRoomInfo.wallB.transform.position);
               spawnPoints.Remove(roomSpawnedOnInfo.wallT.transform);
               _spawnPointPositions.Remove(roomSpawnedOnInfo.wallT.transform.position);
               spawnedRoomInfo.canHaveBottomRoom = false;
               roomSpawnedOnInfo.canHaveTopRoom = false;
               break;
           case "Bottom":
               Debug.Log("HI BOTTOM");
               spawnPoints.Remove(spawnedRoomInfo.wallT.transform);
              _spawnPointPositions.Remove(spawnedRoomInfo.wallT.transform.position);
               spawnPoints.Remove(roomSpawnedOnInfo.wallB.transform);
               _spawnPointPositions.Remove(roomSpawnedOnInfo.wallB.transform.position);
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
