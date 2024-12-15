using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

[RequireComponent(typeof(IntersectionRaycast))]
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
    public bool rareRoom = false;
    public bool bossRoom = false;


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
    private string _roomPath;
    void Awake()
    {
        //_roomPath = AssetDatabase.GetAssetPath(gameObject);
        //Debug.Log(_roomPath);
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
        if (rareRoom)
        {
            LevelBuilder.Instance.possibleRooms.Add(Resources.Load<GameObject>(_roomPath));
        }
        if (bossRoom)
        {
            LevelBuilder.Instance.spawnedBossRooms.Remove(gameObject);
            LevelBuilder.Instance.roomRandomNumber--;
            LevelBuilder.Instance.bossRoomSpawnPoints.Clear();
            foreach (var door in LevelBuilder.Instance.spawnedBossRooms[LevelBuilder.Instance.roomRandomNumber].GetComponent<RoomInfo>().doorSpawnPoints) 
            {
                LevelBuilder.Instance.bossRoomSpawnPoints.Add(door.transform);
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
            case LevelBuilder.SpawnMode.Normal:
                LevelBuilder.Instance.spawnRandomNumber = LevelBuilder.Instance.RandomiseNumber(LevelBuilder.Instance.spawnPoints.Count);
                break;
            case LevelBuilder.SpawnMode.BossRooms:
                LevelBuilder.Instance.spawnRandomNumber = LevelBuilder.Instance.RandomiseNumber(LevelBuilder.Instance.bossRoomSpawnPoints.Count);
                break;
            default:
               break; 
        }
        
    }

}
