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

    public LevelMode currentFloor;
    private GameObject _startingRoom;
    private GameObject _roomToSpawnOn; //The room containing the wall this room used as its spawn position.
    public List<GameObject> possibleRooms; //Rooms that CAN spawn.
    public List<GameObject> spawnedRooms; //Rooms that have ALREADY spawned.
    private List<Transform> _spawnPoints;
    public List<Vector3> spawnPointPositions;
    public int numberOfRooms;
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
        _spawnPoints = new List<Transform>()
        {
            spawnWallL.transform,
            spawnWallR.transform,
            spawnWallB.transform,
            spawnWallT.transform
        };
        spawnPointPositions = new List<Vector3>()
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
            roomRandomNumber = Random.Range(0, possibleRooms.Count); //Spawn a random room from the list of possible rooms
            spawnRandomNumber = Random.Range(0, _spawnPoints.Count); //Choose a random spawn point to spawn the room at
            Transform spawnPoint = _spawnPoints[spawnRandomNumber]; //GameObject that the room will spawn on
            Debug.Log(_spawnPoints[spawnRandomNumber]);
            Vector3 spawnPointPosition = spawnPoint.position; 
            Debug.Log("Before: " + spawnPointPosition.x + ", " + spawnPointPosition.y);
            switch (_spawnPoints[spawnRandomNumber].gameObject.tag) //Move the room based on the distance between its centre and the wall it will spawn on
            {      
                case "Left Wall":
                    spawnPointPosition.x -= possibleRooms[roomRandomNumber].GetComponent<RoomInfo>().distToRoomCentre.x * 2;
                    break; 
                case "Right Wall":
                    spawnPointPosition.x += possibleRooms[roomRandomNumber].GetComponent<RoomInfo>().distToRoomCentre.x * 2;
                    break; 
                case "Bottom Wall":
                    spawnPointPosition.y -= possibleRooms[roomRandomNumber].GetComponent<RoomInfo>().distToRoomCentre.y * 2;
                    break; 
                case "Top Wall":
                    spawnPointPosition.y += possibleRooms[roomRandomNumber].GetComponent<RoomInfo>().distToRoomCentre.y * 2;
                   break;
            }
            Debug.Log("After: " + spawnPointPosition.x + ", " + spawnPointPosition.y);
            roomToSpawn = Instantiate(possibleRooms[roomRandomNumber], spawnPointPosition, Quaternion.identity); //Instantiate the room at the spawnpoint's position
            Debug.Log("Spawned " + possibleRooms[roomRandomNumber] + " at " + spawnPoint);
            UpdateSpawnWalls(roomToSpawn, roomRandomNumber, spawnRandomNumber);
        }
    }

    void UpdateSpawnWalls(GameObject spawnedRoom, int roomRandomNumber, int spawnRandomNumber)
    {
        RoomInfo roomInfo = spawnedRoom.GetComponent<RoomInfo>();
        _spawnPoints.Add(roomInfo.wallT.transform); //
        _spawnPoints.Add(roomInfo.wallB.transform); //
        _spawnPoints.Add(roomInfo.wallL.transform); //
        _spawnPoints.Add(roomInfo.wallR.transform); //Adding walls of the spawned room to the list of possible spawn points
        spawnedRooms.Add(possibleRooms[roomRandomNumber]); //Add to the list of rooms already in the level
        spawnPointPositions.Remove(possibleRooms[roomRandomNumber].transform.position);
        spawnPointPositions.Add(roomInfo.wallT.transform.position); //
        spawnPointPositions.Add(roomInfo.wallB.transform.position); //
        spawnPointPositions.Add(roomInfo.wallL.transform.position); //
        spawnPointPositions.Add(roomInfo.wallR.transform.position);
        possibleRooms.Remove(possibleRooms[roomRandomNumber]); //Remove the room from the list of rooms that can spawn
        _spawnPoints.Remove(_spawnPoints[spawnRandomNumber]); //Remove the wall the room spawned on from the spawn point list.
    }
    
    void Update()
    {
        
    }
}
