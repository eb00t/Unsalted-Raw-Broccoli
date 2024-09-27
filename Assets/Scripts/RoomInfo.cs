using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomInfo : MonoBehaviour
{
    public Transform wallL, wallR, wallB, wallT;
    public bool canHaveLeftRoom, canHaveRightRoom, canHaveTopRoom, canHaveBottomRoom;
    public Vector3 distToRoomCentre;
    public List<GameObject> allWalls;
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
            switch (walls.name)
            {
                case "LeftWall":
                    wallL = walls.transform;
                    break;
                case "RightWall":
                    wallR = walls.transform;
                    break;
                case "TopWall":
                    wallT = walls.transform;
                    break;
                case "BottomWall":
                    wallB = walls.transform;
                    break;
                default:
                    break;
            }
        }

        distToRoomCentre.x = (wallL.transform.localPosition.x - wallR.transform.localPosition.x)/2;
        Debug.Log(gameObject + " Distance between left/right walls and centre: " + distToRoomCentre.x);
        distToRoomCentre.y = (wallT.transform.localPosition.y - wallB.transform.localPosition.y)/2;
        Debug.Log(gameObject + "Distance between top/bottom walls and centre: " + distToRoomCentre.y);
    }
    
    void Update()
    {
        
    }
}
