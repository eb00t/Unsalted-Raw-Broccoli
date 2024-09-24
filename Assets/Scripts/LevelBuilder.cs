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
    public List<Vector3> spawnPoints; //TODO: Make this unnecessary
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
        spawnPoints = new List<Vector3>()
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
        int randomNumber, randomNumber2;
        GameObject roomToSpawn;
        for (int i = 0; i < numberOfRooms; i++)
        { 
            randomNumber = Random.Range(0, possibleRooms.Count);
            randomNumber2 = Random.Range(0, spawnPoints.Count);
            Vector3 spawnPoint = spawnPoints[randomNumber2];
            switch (possibleRooms[randomNumber].gameObject.name)
            {      
                case "LeftWall":
                    spawnPoint.x -= possibleRooms[randomNumber].GetComponent<RoomInfo>().distToRoomCentre.x;
                    break; 
                case "RightWall":
                    spawnPoint.x += possibleRooms[randomNumber].GetComponent<RoomInfo>().distToRoomCentre.x;
                    break; 
                case "BottomWall":
                    spawnPoint.y -= possibleRooms[randomNumber].GetComponent<RoomInfo>().distToRoomCentre.y;
                    break; 
                case "TopWall":
                    spawnPoint.y += possibleRooms[randomNumber].GetComponent<RoomInfo>().distToRoomCentre.y;
                   break;
            }
            roomToSpawn = Instantiate(possibleRooms[randomNumber], spawnPoint, Quaternion.identity);
            Debug.Log("Spawned " + possibleRooms[randomNumber] + " at " + spawnPoint);
            UpdateSpawnWalls(roomToSpawn, randomNumber, randomNumber2);
          
        }
    }

    void UpdateSpawnWalls(GameObject spawnedRoom, int randomNumber, int randomNumber2)
    {
        RoomInfo roomInfo = spawnedRoom.GetComponent<RoomInfo>();
        spawnPoints.Add(roomInfo.wallT.transform.position);
        spawnPoints.Add(roomInfo.wallB.transform.position);
        spawnPoints.Add(roomInfo.wallL.transform.position);
        spawnPoints.Add(roomInfo.wallR.transform.position);
        spawnedRooms.Add(possibleRooms[randomNumber]);
        possibleRooms.Remove(possibleRooms[randomNumber]);
        spawnPoints.Remove(spawnPoints[randomNumber2]);
    }
    
    void Update()
    {
        
    }
}
