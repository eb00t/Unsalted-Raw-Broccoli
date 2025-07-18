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
    public float roomLength; //
    public float roomHeight; //YOU MUST ASSIGN THESE TWO MANUALLY FOR THINGS TO WORK
    public bool specialRoom = false;
    public bool bossRoom = false;
    public bool shop = false;
    public bool lootRoom = false;
    public bool loreRoom = false;
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
    public List<Light> allLights;
    public List<GameObject> attachedConnectors;
    public Transform doorL, doorR, doorB, doorT;
    public Transform wallL, wallR, wallB, wallT;
    public GameObject connectorSpawnedOff;
    public GameObject mapIcons;
    private GameObject _playerRenderer;
    public CinemachineVirtualCamera roomCam;
    public bool canBeDiscarded = true;
    public bool markedForDiscard = false;
    public bool coveredUp;
    public string roomPath;
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
       
        if (specialRoom) // Get the path so it can be re-added to the special rooms list if discarded.
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

        foreach (var child in GetComponentsInChildren<Transform>()) // Assigning map icons
        {
           if (child.CompareTag("Map Icon Parent") && !child.name.Contains("item"))
           {
               mapIcons = child.gameObject;
           }
        }
        if (!gameObject.CompareTag("StartingRoom"))
        {
            mapIcons.SetActive(false);    
        }
        
    }

    void Start()
    {
        LevelBuilder.Instance.spawnedRooms.Add(gameObject); //  Add to the list of rooms already in the level
        BlackoutManager.Instance.failSafeTimer += 0.25f;
        foreach (var lit in gameObject.GetComponentsInChildren<Light>())
        {
            LightManager.Instance.allRoomLights.Add(lit);
            allLights.Add(lit.GetComponent<Light>());
        }

        if (!gameObject.CompareTag("StartingRoom"))
        {
            foreach (var lit in allLights)
            {
                lit.enabled = false;
            }

            coveredUp = true;
        }

        _playerRenderer = GameObject.FindGameObjectWithTag("Player").transform.Find("Renderer").gameObject;
        if (bigRoom)
        {
            roomCam.Follow = _playerRenderer.transform;
            roomCam.LookAt = null;
        }
        if (gameObject.CompareTag("StartingRoom"))
        {
            canBeDiscarded = false;
            roomCam.Priority = 9999;
        }
        else
        {
            canBeDiscarded = true;
            roomCam.Priority = 0;
        }
        
        
        if (bossRoom)
        {
            LevelBuilder.Instance.spawnedBossRooms.Add(gameObject);
        }

        if (lootRoom)
        {
            LevelBuilder.Instance.spawnedLootRooms.Add(gameObject);
            if (LevelBuilder.Instance.spawnedLootRooms.Count > LevelBuilder.Instance.lootRoomsToSpawn)
            {
               MarkRoomForDiscard();
            }
        }

        if (shop)
        {
            LevelBuilder.Instance.spawnedShops.Add(gameObject);
            if (LevelBuilder.Instance.spawnedShops.Count > 1)
            {
               MarkRoomForDiscard();
            }
        }
        if (loreRoom)
        {
            LevelBuilder.Instance.spawnedLoreRooms.Add(gameObject);
            LevelBuilder.Instance.howManyRoomsToSpawn++;
            if (LevelBuilder.Instance.spawnedLoreRooms.Count > 1)
            {
                MarkRoomForDiscard();
            }
        }
        CameraManager.Instance.virtualCameras.Add(roomCam.GetComponent<CinemachineVirtualCamera>());
        //connectorSpawnedOff = LevelBuilder.Instance._spawnedConnectors[^1];
        /*distToRoomCentre.x = (wallL.transform.localPosition.x - wallR.transform.localPosition.x);
        Debug.Log(gameObject + " Distance between left/right walls and centre: " + distToRoomCentre.x);
        distToRoomCentre.y = (wallT.transform.localPosition.y - wallB.transform.localPosition.y);
        Debug.Log(gameObject + "Distance between top/bottom walls and centre: " + distToRoomCentre.y);*/
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (allLights.Count != 0)
            {
                foreach (var lit in allLights)
                {
                    lit.GetComponent<Light>().enabled = true;
                    foreach (var lit2 in attachedConnectors)
                    {
                        lit2.GetComponentInChildren<Light>().enabled = true;
                    }
                    if (LightManager.Instance.roomLightQueue.Contains(lit.transform.root.gameObject))
                    {
                        LightManager.Instance.roomLightQueue.Remove(lit.transform.root.gameObject);
                    }
                    LightManager.Instance.roomLightQueue.Add(lit.transform.root.gameObject);
                    LightManager.Instance.CheckQueues();
                }
            }
        }
    }

    private void Update()
    {
        if(connectorSpawnedOff == null && !gameObject.CompareTag("StartingRoom"))
        {
            MarkRoomForDiscard();
        }   
    }


    public void MarkRoomForDiscard()
    {
        if (markedForDiscard == false)
        {
            markedForDiscard = true;
            if (!LevelBuilder.Instance.discardedRooms.Contains(gameObject))
            {
                LevelBuilder.Instance.discardedRooms.Add(gameObject);
            }

            foreach (var lit in allLights)
            {
                LightManager.Instance.allRoomLights.Remove(lit);
            }

            LevelBuilder.Instance.spawnedRooms.Remove(gameObject);

            if (specialRoom)
            {
                LevelBuilder.Instance.possibleSpecialRooms.Add(Resources.Load<GameObject>(roomPath));
            }

            if (shop)
            {
                if (LevelBuilder.Instance.spawnedShops.Count <= 0)
                {
                    LevelBuilder.Instance._spawnMode = LevelBuilder.SpawnMode.Shops;
                }
                LevelBuilder.Instance.spawnedShops.Remove(gameObject);
                LevelBuilder.Instance.spawnModeChangedByDestroy = true;
            }

            if (lootRoom)
            {
                LevelBuilder.Instance.spawnedLootRooms.Remove(gameObject);
            }

            if (bossRoom)
            {
                LevelBuilder.Instance.spawnedBossRooms.Remove(gameObject);
                LevelBuilder.Instance.roomRandomNumber--;
                switch (LevelBuilder.Instance.roomRandomNumber)
                {
                    case -1:
                        LevelBuilder.Instance.firstBossRoomSpawnPoints =
                            new List<Transform>(LevelBuilder.Instance.lootRoomSpawnPoints);
                        LevelBuilder.Instance.secondBossRoomSpawnPoints.Clear();
                        break;
                    case 0:
                        LevelBuilder.Instance.thirdBossRoomSpawnPoints.Clear();
                        LevelBuilder.Instance.otherConnectorSideRoomInfo =
                            LevelBuilder.Instance.spawnedBossRooms[0].GetComponent<RoomInfo>();
                        break;
                    case 1:
                        LevelBuilder.Instance.otherConnectorSideRoomInfo =
                            LevelBuilder.Instance.spawnedBossRooms[1].GetComponent<RoomInfo>();
                        break;
                }
            }

            if (loreRoom)
            {
                LevelBuilder.Instance.spawnedLoreRooms.Remove(gameObject);
                LevelBuilder.Instance.howManyRoomsToSpawn--;
            }

            foreach (var connector in attachedConnectors)
            {
                if (connector != null)
                {
                    connector.GetComponent<ConnectorRoomInfo>().attachedRooms.Remove(gameObject);
                }
            }

            foreach (var door in doorSpawnPoints)
            {
                LevelBuilder.Instance.spawnPoints.Remove(door.transform);
                LevelBuilder.Instance.lootRoomSpawnPoints.Remove(door.transform);
            }

            doorSpawnPoints.Clear();
            LevelBuilder.Instance.CleanUpBadRooms();
        }
    }

    private void OnDestroy()
    {
        CameraManager.Instance.virtualCameras.Remove(roomCam);
        switch (LevelBuilder.Instance._spawnMode)
        {
            case LevelBuilder.SpawnMode.Normal or LevelBuilder.SpawnMode.SpecialRooms or LevelBuilder.SpawnMode.Shops or LevelBuilder.SpawnMode.LoreRooms:
                LevelBuilder.Instance.spawnRandomNumber = LevelBuilder.Instance.RandomiseNumber(LevelBuilder.Instance.spawnPoints.Count);
                break;
            case LevelBuilder.SpawnMode.LootRooms:
                LevelBuilder.Instance.spawnRandomNumber = LevelBuilder.Instance.RandomiseNumber(LevelBuilder.Instance.lootRoomSpawnPoints.Count);
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
