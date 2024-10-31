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
    public int connectorsToSpawn;
    public int roomLength; //
    public int roomHeight; //YOU MUST ASSIGN THESE MANUALLY FOR THINGS TO WORK

    [field: Header("Debugging")] 
    public List<GameObject> allDoors;
    public Transform doorL, doorR, doorB, doorT;
    public string spawnedOnSide;
    void Awake()
    {
        foreach (var doors in gameObject.GetComponentsInChildren<Transform>())
        {
            if (doors.name.Contains("Door")) 
            {
                allDoors.Add(doors.gameObject);
            }
        }

        foreach (var doors in allDoors)
        {
            switch (doors.tag)
            {
                case "Left Door":
                    if (doorL == null)
                    {
                        doorL = doors.transform;
                    }
                    break;
                case "Right Door":
                    if (doorR == null)
                    {
                        doorR = doors.transform;
                    }
                    break;
                case "Top Door":
                    if (doorT == null)
                    {
                        doorT = doors.transform;
                    }
                    break;
                case "Bottom Door":
                    if (doorB == null)
                    {
                        doorB = doors.transform;
                    }
                    break;
                default:
                    break;
            }

            canHaveLeftRoom = true;
            canHaveRightRoom = true;
            canHaveTopRoom = true;
            canHaveBottomRoom = true;

            if (doorL == null)
            {
                canHaveLeftRoom = false;
            } 
            if (doorR == null)
            {
                canHaveRightRoom = false;
            } 
            if (doorB == null)
            {
                canHaveBottomRoom = false;
            } 
            if (doorT == null)
            {
                canHaveTopRoom = false;
            }
            
            connectorsToSpawn = Random.Range(0, 4);
        }
        
    }

    void Start()
    {
        /*distToRoomCentre.x = (wallL.transform.localPosition.x - wallR.transform.localPosition.x);
        Debug.Log(gameObject + " Distance between left/right walls and centre: " + distToRoomCentre.x);
        distToRoomCentre.y = (wallT.transform.localPosition.y - wallB.transform.localPosition.y);
        Debug.Log(gameObject + "Distance between top/bottom walls and centre: " + distToRoomCentre.y);*/
    }

    void Update()
    {
        
    }
}
