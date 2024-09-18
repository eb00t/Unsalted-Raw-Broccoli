using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

public class LevelBuilder : MonoBehaviour
{
    public enum LevelMode
    {
        TEST,
        Floor1,
        Floor2,
        Floor3,
    }

    public LevelMode currentFloor;
    public List<GameObject> possibleRooms;
    public List<Vector3> spawnPoints; //TODO: Make this unnecessary
    public int numberOfRooms;
    
    void Start()
    {
        possibleRooms = new List<GameObject>();
        AddRoomsToList();
        SpawnRooms();
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
        int randomNumber;
        object roomToSpawn;
        for (int i = 0; i < numberOfRooms; i++)
        {
            randomNumber = Random.Range(0, possibleRooms.Count);
            //TEST CODE, DO NOT USE
            roomToSpawn = Instantiate(possibleRooms[randomNumber], spawnPoints[i], Quaternion.identity);
            Debug.Log("Spawned " + possibleRooms[randomNumber] +" at " + spawnPoints[i]);
            possibleRooms.Remove(possibleRooms[randomNumber]);
        }
    }
    void Update()
    {
        
    }
}
