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
    public float roomLength; //
    public float roomHeight; //YOU MUST ASSIGN THESE TWO MANUALLY FOR THINGS TO WORK
    public bool specialRoom = false;
    public bool bossRoom = false;
    public bool shop = false;
    public bool lootRoom = false;
    public bool bigRoom = false;
    [field: Header("Door Config (MAKE ABSOLUTELY SURE YOU SET THESE PROPERLY)")] 
    [field: Tooltip("SERIOUSLY! MAKE SURE THESE ARE ACCURATE TO THE DESIGN OF THE ROOM")]
    public bool missingLeftDoor;
    [field: Tooltip("SERIOUSLY! MAKE SURE THESE ARE ACCURATE TO THE DESIGN OF THE ROOM")]
    public bool missingRightDoor;
    [field: Tooltip("SERIOUSLY! MAKE SURE THESE ARE ACCURATE TO THE DESIGN OF THE ROOM")]
    public bool missingTopDoor;
    [field: Tooltip("SERIOUSLY! MAKE SURE THESE ARE ACCURATE TO THE DESIGN OF THE ROOM")]
    public bool missingBottomDoor;


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
    private GameObject _playerRenderer;
    void Awake()
    {
        if (doorT == null)
        {
            missingTopDoor = true;
        }
        if (doorB == null)
        {
            missingBottomDoor = true;
        } 
        if (doorL == null)
        {
           missingLeftDoor= true;
        } 
        if (doorR == null)
        {
            missingRightDoor = true;
        }
       
        if (specialRoom)
        {
            string roomPath1 = "Room Layouts/Special Rooms/" + gameObject.name;
            string roomPath2 = "(Clone)";
            roomPath = roomPath1.Replace(roomPath2, "");
        }
        roomCam = GetComponentInChildren<CinemachineVirtualCamera>();
        roomCam.m_Lens.OrthographicSize = 12.6f;
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
    }

    void Start()
    {
        _playerRenderer = GameObject.FindGameObjectWithTag("Player").transform.Find("Renderer").gameObject;
        if (bigRoom)
        {
            roomCam.Follow = _playerRenderer.transform;
            roomCam.LookAt = null;
        }
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
            LevelBuilder.Instance.lootRoomsToSpawn--;
        }

        if (shop)
        {
            if (LevelBuilder.Instance.shopSpawned)
            {
                markedForDiscard = true;
            }
            else
            {
                LevelBuilder.Instance.shopSpawned = true;
            }
            
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
            LevelBuilder.Instance.lootRoomsToSpawn++;
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
