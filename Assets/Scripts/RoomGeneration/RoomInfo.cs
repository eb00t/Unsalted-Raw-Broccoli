using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;
[RequireComponent(typeof(IntersectionRaycast))]
public class RoomInfo : MonoBehaviour
{
   [NonSerialized] public bool canHaveLeftRoom;
    [NonSerialized]public bool canHaveRightRoom;
    [NonSerialized]public bool canHaveTopRoom;
    [NonSerialized]public bool canHaveBottomRoom;
    [NonSerialized]public bool canSpawnOnRight;
    [NonSerialized] public bool canSpawnOnLeft;
    [NonSerialized]public bool canSpawnOnTop;
    [NonSerialized] public bool canSpawnOnBottom;
    [field: Header("Configuration")] 
    public float roomLength; //
    public float roomHeight; //YOU MUST ASSIGN THESE TWO MANUALLY FOR THINGS TO WORK
    public bool rareRoom = false;
    


    [field: Header("Debugging")] 
    public IntersectionRaycast intersectionCheck;
    public List<GameObject> allDoors;
    public Transform doorL, doorR, doorB, doorT;
    public Transform wallL, wallR, wallB, wallT;
    public List<GameObject> attachedConnectors;
    public GameObject connectorSpawnedOff;
    public bool markedForDiscard;
    private CinemachineVirtualCamera _roomCam;
    void Awake()
    {
        _roomCam = GetComponentInChildren<CinemachineVirtualCamera>();
        intersectionCheck = GetComponent<IntersectionRaycast>();
        attachedConnectors.Clear();
        foreach (var door in gameObject.GetComponentsInChildren<Transform>())
        {
            if (door.tag.Contains("Door")) 
            {
                allDoors.Add(door.gameObject);
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
        CameraManager.Instance.virtualCameras.Add(_roomCam);
        //connectorSpawnedOff = LevelBuilder.Instance._spawnedConnectors[^1];
        /*distToRoomCentre.x = (wallL.transform.localPosition.x - wallR.transform.localPosition.x);
        Debug.Log(gameObject + " Distance between left/right walls and centre: " + distToRoomCentre.x);
        distToRoomCentre.y = (wallT.transform.localPosition.y - wallB.transform.localPosition.y);
        Debug.Log(gameObject + "Distance between top/bottom walls and centre: " + distToRoomCentre.y);*/
    }
}
