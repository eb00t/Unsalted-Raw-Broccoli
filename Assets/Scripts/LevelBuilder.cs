using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
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
    public RoomInfo roomInfo;
    
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
        string path = "YOU SHOULDN'T SEE THIS"; //Path for floor exclusive rooms
        string altPath = "YOU SHOULDN'T SEE THIS EITHER"; //TODO: Path for rooms used in multiple floors 
        switch (currentFloor)
        {
            case LevelMode.TEST:
                path = "Room Layouts/Test Rooms";
                break;
            case LevelMode.Floor1:
                path = "Room Layouts/Floor 1";
                break;
            case LevelMode.Floor2:
                path = "Room Layouts/Floor 2";
                break;
            case LevelMode.Floor3:
                path = "Room Layouts/Floor 3";
                break;
        }
        foreach (var rooms in Resources.LoadAll<GameObject>(path))
        {
            possibleRooms.Add(rooms);
        }
        Debug.Log(path + " " + altPath);
    }

    void SpawnRooms()
    {
        int roomRandomNumber, spawnRandomNumber; //RNG 1 and 2
        GameObject roomToSpawn; //Room that will be spawned
        for (int i = 0; i < numberOfRooms; i++) //Spawn amount of rooms
        {
            roomRandomNumber = RandomiseNumber(possibleRooms.Count); //Spawn a random room from the list of possible rooms
            spawnRandomNumber = RandomiseNumber(spawnPoints.Count); //Choose a random spawn point to spawn the room at
            Transform spawnPoint = spawnPoints[spawnRandomNumber]; //GameObject that the room will spawn on
            Vector3 newSpawnPoint = new Vector3();
            Debug.Log(spawnPoints[spawnRandomNumber]);
            Vector3 spawnPointPosition = spawnPoint.position; 
            Debug.Log("Before: " + spawnPointPosition.x + ", " + spawnPointPosition.y);
            roomInfo = possibleRooms[roomRandomNumber].GetComponent<RoomInfo>();
            switch (spawnPoints[spawnRandomNumber].gameObject.tag) //Move the room based on the distance between its centre and the wall it will spawn on
            {      
                case "Left Wall":
                    newSpawnPoint.x = (spawnPointPosition.x - roomInfo.wallL.localPosition.x);
                    roomInfo.spawnedOnSide = "Left";
                    break; 
                case "Right Wall":
                    newSpawnPoint.x = (spawnPointPosition.x - roomInfo.wallR.localPosition.x);
                    roomInfo.spawnedOnSide = "Right";
                    break; 
                case "Bottom Wall":
                    newSpawnPoint.y = (spawnPointPosition.y - roomInfo.wallB.localPosition.y);
                    roomInfo.spawnedOnSide = "Bottom";
                    break; 
                case "Top Wall":
                    newSpawnPoint.y = (spawnPointPosition.y - roomInfo.wallT.localPosition.y);
                    roomInfo.spawnedOnSide = "Top";
                   break;
            }
            Debug.Log("After: " + newSpawnPoint.x + ", " + newSpawnPoint.y);
            roomToSpawn = Instantiate(possibleRooms[roomRandomNumber], newSpawnPoint, Quaternion.identity); //Instantiate the room at the spawnpoint's position
            Debug.Log("Spawned " + possibleRooms[roomRandomNumber] + " at " + newSpawnPoint);
            StartCoroutine(WaitToUpdate(roomToSpawn, roomRandomNumber, spawnRandomNumber));
            //UpdateSpawnWalls(roomToSpawn, roomRandomNumber, spawnRandomNumber);
        }
    }

    void UpdateSpawnWalls(GameObject spawnedRoom, int roomRandomNumber, int spawnRandomNumber)
    {
        roomInfo = spawnedRoom.GetComponent<RoomInfo>();
        Debug.Log(spawnedRoom.name);
        spawnPoints.Add(roomInfo.wallT.transform); //
        spawnPoints.Add(roomInfo.wallB.transform); //
        spawnPoints.Add(roomInfo.wallL.transform); //
        spawnPoints.Add(roomInfo.wallR.transform); //Adding walls of the spawned room to the list of possible spawn points
        spawnedRooms.Add(possibleRooms[roomRandomNumber]); //Add to the list of rooms already in the level
       _spawnPointPositions.Add(roomInfo.wallT.transform.position); //
       _spawnPointPositions.Add(roomInfo.wallB.transform.position); //
       _spawnPointPositions.Add(roomInfo.wallL.transform.position); //
       _spawnPointPositions.Add(roomInfo.wallR.transform.position);
       _spawnPointPositions.Remove(possibleRooms[roomRandomNumber].transform.position);
       //possibleRooms.Remove(possibleRooms[roomRandomNumber]); //Remove the room from the list of rooms that can spawn
       spawnPoints.Remove(spawnPoints[spawnRandomNumber]); //Remove the wall the room spawned on from the spawn point list.
       switch (roomInfo.spawnedOnSide)
       {
           case "Left":
               Debug.Log("HI LEFT");
               spawnPoints.Remove(roomInfo.wallR.transform);
              _spawnPointPositions.Remove(roomInfo.wallR.transform.position);
               roomInfo.canHaveRightRoom = false;
               break;
           case "Right":
               Debug.Log("HI RIGHT");
               spawnPoints.Remove(roomInfo.wallL.transform);
              _spawnPointPositions.Remove(roomInfo.wallL.transform.position);
               roomInfo.canHaveLeftRoom = false;
               break;
           case "Top":
               Debug.Log("HI TOP");
               spawnPoints.Remove(roomInfo.wallB.transform);
              _spawnPointPositions.Remove(roomInfo.wallB.transform.position);
               roomInfo.canHaveBottomRoom = false;
               break;
           case "Bottom":
               Debug.Log("HI BOTTOM");
               spawnPoints.Remove(roomInfo.wallT.transform);
              _spawnPointPositions.Remove(roomInfo.wallT.transform.position);
               roomInfo.canHaveTopRoom = false;
               break;
       }
    }

    IEnumerator WaitToUpdate(GameObject spawnedRoom, int roomRandomNumber, int spawnRandomNumber)
    {
        UpdateSpawnWalls(spawnedRoom, roomRandomNumber, spawnRandomNumber);
        yield return new WaitForSecondsRealtime(1f);
        
    }

    int RandomiseNumber(int setSize)
    {
        int rng = Random.Range(0, setSize);
        return rng;
    }
}
