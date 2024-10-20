using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class RoomInfo : MonoBehaviour
{
    [field: Header("Configuration")] 
    public bool canHaveLeftRoom;
    public bool canHaveRightRoom;
    public bool canHaveTopRoom;
    public bool canHaveBottomRoom;
    public int connectorsToSpawn;
    [field: Header("Debugging")]
    public Vector3 distToRoomCentre;
    public Transform wallL, wallR, wallB, wallT;
    public List<GameObject> allWalls;
    public string spawnedOnSide;
    void Awake()
    {
        foreach (var walls in gameObject.GetComponentsInChildren<Transform>())
        {
            if (walls.name.Contains("Wall")) 
            {
                allWalls.Add(walls.gameObject);
            }
        }

        foreach (var walls in allWalls)
        {
            switch (walls.tag)
            {
                case "Left Wall":
                    if (wallL == null)
                    {
                        wallL = walls.transform;
                    }
                    break;
                case "Right Wall":
                    if (wallT == null)
                    {
                        wallT = walls.transform;
                    }
                    break;
                case "Top Wall":
                    if (wallT == null)
                    {
                        wallT = walls.transform;
                    }
                    break;
                case "Bottom Wall":
                    if (wallB == null)
                    {
                        wallB = walls.transform;
                    }
                    break;
                default:
                    break;
            }

            canHaveLeftRoom = true;
            canHaveRightRoom = true;
            canHaveTopRoom = true;
            canHaveBottomRoom = true;
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
