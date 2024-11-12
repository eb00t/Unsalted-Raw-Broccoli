using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class RoomInfo : MonoBehaviour
{
    [field: Header("Configuration")] 
    public bool canHaveLeftRoom;
    public bool canHaveRightRoom;
    public bool canHaveTopRoom;
    public bool canHaveBottomRoom;
    public bool canSpawnOnRight;
    public bool canSpawnOnLeft;
    public bool canSpawnOnTop;
    public bool canSpawnOnBottom;
    public int roomLength; //
    public int roomHeight; //YOU MUST ASSIGN THESE TWO MANUALLY FOR THINGS TO WORK


    [field: Header("Debugging")] 
    public GameObject roomInstance;
    public CheckForIntersection intersectionCheck;
    public List<GameObject> allDoors;
    public Transform doorL, doorR, doorB, doorT;
    public List<GameObject> attachedConnectors;
    void Awake()
    {
        roomInstance = gameObject;
        attachedConnectors.Clear();
        foreach (var door in gameObject.GetComponentsInChildren<Transform>())
        {
            if (door.name.Contains("Door")) 
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
        intersectionCheck = GetComponentInChildren<CheckForIntersection>();
        /*distToRoomCentre.x = (wallL.transform.localPosition.x - wallR.transform.localPosition.x);
        Debug.Log(gameObject + " Distance between left/right walls and centre: " + distToRoomCentre.x);
        distToRoomCentre.y = (wallT.transform.localPosition.y - wallB.transform.localPosition.y);
        Debug.Log(gameObject + "Distance between top/bottom walls and centre: " + distToRoomCentre.y);*/
    }
}
