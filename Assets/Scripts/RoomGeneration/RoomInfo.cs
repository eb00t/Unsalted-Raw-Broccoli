using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;


public class RoomInfo : MonoBehaviour
{
    [field: Header("Configuration")] 
    [NonSerialized] public bool canHaveLeftRoom;
    [NonSerialized]public bool canHaveRightRoom;
    [NonSerialized]public bool canHaveTopRoom;
    [NonSerialized]public bool canHaveBottomRoom;
    [NonSerialized]public bool canSpawnOnRight;
    [NonSerialized] public bool canSpawnOnLeft;
    [NonSerialized]public bool canSpawnOnTop;
    [NonSerialized] public bool canSpawnOnBottom;
    public float roomLength; //
    public float roomHeight; //YOU MUST ASSIGN THESE TWO MANUALLY FOR THINGS TO WORK
    public bool specialRoom = false;
    public bool bossRoom = false;
    public bool shop = false;
    public bool lootRoom = false;


    [field: Header("Debugging")] 
    public IntersectionRaycast intersectionCheck;
    public List<GameObject> doorSpawnPoints;
    public List<GameObject> usableDoors;
    public List<GameObject> allDoors;
    public List<GameObject> allWalls;
    public Transform doorL, doorR, doorB, doorT;
    public Transform wallL, wallR, wallB, wallT;
    public List<GameObject> attachedConnectors;
    public GameObject connectorSpawnedOff;
    public bool markedForDiscard; 
    public CinemachineVirtualCamera roomCam;
    public bool canBeDiscarded = true;
    public string roomPath;
    void Awake()
    {
        
        string roomPath1 = "Room Layouts/Special Rooms/" + gameObject.name;
        string roomPath2 = "(Clone)";
        roomPath = roomPath1.Replace(roomPath2, "");
        roomCam = GetComponentInChildren<CinemachineVirtualCamera>();
        intersectionCheck = GetComponent<IntersectionRaycast>();
        attachedConnectors.Clear();
        foreach (var door in gameObject.GetComponentsInChildren<Transform>())
        {
            if (door.tag.Contains("Door")) 
            {
                doorSpawnPoints.Add(door.gameObject);
                allDoors.Add(door.gameObject);
            }

            if (door.tag.Contains("Wall"))
            {
                allWalls.Add(door.gameObject);
            }
        }

        canHaveLeftRoom = true;
        canHaveRightRoom = true;
        canHaveTopRoom = true;
        canHaveBottomRoom = true;
        
        canSpawnOnRight = true;
        canSpawnOnLeft = true;
        canSpawnOnTop= true;
        canSpawnOnBottom = true;

        if (doorL == null)
        {
            canHaveLeftRoom = false;
            canSpawnOnRight = false;
        } 
        if (doorR == null)
        {
            canHaveRightRoom = false;
            canSpawnOnLeft = false;
        } 
        if (doorB == null)
        {
            canHaveBottomRoom = false;
            canSpawnOnTop = false;
        } 
        if (doorT == null)
        {
            canHaveTopRoom = false;
            canSpawnOnBottom = false;
        }
    }

    void Start()
    {
        if (gameObject.CompareTag("StartingRoom"))
        {
            canBeDiscarded = false;
        }
        else
        {
            canBeDiscarded = true;
        }
        
        LevelBuilder.Instance.spawnedRooms.Add(gameObject); //  Add to the list of rooms already in the level
        if (bossRoom)
        {
            LevelBuilder.Instance.spawnedBossRooms.Add(gameObject);
        }

        if (lootRoom)
        {
            LevelBuilder.Instance.spawnedLootRooms.Add(gameObject);
        }

        if (shop)
        {
            LevelBuilder.Instance.shopSpawned = true;
        }
        CameraManager.Instance.virtualCameras.Add(roomCam.GetComponent<CinemachineVirtualCamera>());
        //connectorSpawnedOff = LevelBuilder.Instance._spawnedConnectors[^1];
        /*distToRoomCentre.x = (wallL.transform.localPosition.x - wallR.transform.localPosition.x);
        Debug.Log(gameObject + " Distance between left/right walls and centre: " + distToRoomCentre.x);
        distToRoomCentre.y = (wallT.transform.localPosition.y - wallB.transform.localPosition.y);
        Debug.Log(gameObject + "Distance between top/bottom walls and centre: " + distToRoomCentre.y);*/
    }

    private void OnDestroy()
    {
        LevelBuilder.Instance.spawnedRooms.Remove(gameObject);
        foreach (var door in doorSpawnPoints)
        {
           LevelBuilder.Instance.spawnPoints.Remove(door.transform); 
        }
        CameraManager.Instance.virtualCameras.Remove(roomCam.GetComponent<CinemachineVirtualCamera>());
        if (specialRoom)
        {
            
            LevelBuilder.Instance.possibleSpecialRooms.Add(Resources.Load<GameObject>(roomPath));
        }
        if (shop)
        {
            LevelBuilder.Instance.shopSpawned = false;
            LevelBuilder.Instance._spawnMode = LevelBuilder.SpawnMode.Shop;
            LevelBuilder.Instance.spawnModeChangedByDestroy = true;
        }
        if (lootRoom)
        {
            LevelBuilder.Instance.spawnedLootRooms.Remove(gameObject);
            LevelBuilder.Instance._spawnMode = LevelBuilder.SpawnMode.LootRoom;
            LevelBuilder.Instance.spawnModeChangedByDestroy = true;
            LevelBuilder.Instance.lootRoomsToSpawn--;
        }
        if (bossRoom)
        {
            LevelBuilder.Instance.spawnedBossRooms.Remove(gameObject);
            LevelBuilder.Instance.roomRandomNumber--;
            switch (LevelBuilder.Instance.roomRandomNumber)
            {
                case -1:
                    LevelBuilder.Instance.firstBossRoomSpawnPoints = new List<Transform>(LevelBuilder.Instance.spawnPoints);
                    LevelBuilder.Instance.secondBossRoomSpawnPoints.Clear();
                    break;
                case 0:
                    LevelBuilder.Instance.thirdBossRoomSpawnPoints.Clear();
                    LevelBuilder.Instance.otherConnectorSideRoomInfo = LevelBuilder.Instance.spawnedBossRooms[0].GetComponent<RoomInfo>();
                    break;
                case 1:
                    LevelBuilder.Instance.otherConnectorSideRoomInfo = LevelBuilder.Instance.spawnedBossRooms[1].GetComponent<RoomInfo>();
                    break;
            }
        }
       
        if (LevelBuilder.Instance.discardedRooms.Contains(gameObject))
        {
            LevelBuilder.Instance.discardedRooms.Remove(gameObject);
            LevelBuilder.Instance.roomsDiscarded += 1;
        }
        
        foreach (var connector in attachedConnectors)
        {
            if (connector != null)
            {
                connector.GetComponent<ConnectorRoomInfo>().attachedRooms.Remove(gameObject);
            }
        }

        switch (LevelBuilder.Instance._spawnMode)
        {
            case LevelBuilder.SpawnMode.Normal or LevelBuilder.SpawnMode.SpecialRooms or LevelBuilder.SpawnMode.Shop or LevelBuilder.SpawnMode.LootRoom:
                LevelBuilder.Instance.spawnRandomNumber = LevelBuilder.Instance.RandomiseNumber(LevelBuilder.Instance.spawnPoints.Count);
                break;
            case LevelBuilder.SpawnMode.BossRooms:
                switch (LevelBuilder.Instance.roomRandomNumber)
                {
                    case -1:
                        LevelBuilder.Instance.spawnRandomNumber = LevelBuilder.Instance.RandomiseNumber(LevelBuilder.Instance.firstBossRoomSpawnPoints.Count);
                        break;
                    case 0:
                        LevelBuilder.Instance.spawnRandomNumber = LevelBuilder.Instance.RandomiseNumber(LevelBuilder.Instance.secondBossRoomSpawnPoints.Count);
                        break;
                    case 1:
                        LevelBuilder.Instance.spawnRandomNumber = LevelBuilder.Instance.RandomiseNumber(LevelBuilder.Instance.thirdBossRoomSpawnPoints.Count);
                        break;
                }
                break;
        }
        
    }

}
