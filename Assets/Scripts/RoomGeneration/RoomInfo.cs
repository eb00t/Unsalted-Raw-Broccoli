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
    public float roomLength; //
    public float roomHeight; //YOU MUST ASSIGN THESE TWO MANUALLY FOR THINGS TO WORK
    


    [field: Header("Debugging")] 
    public GameObject roomInstance;
    public IntersectionRaycast intersectionCheck;
    public List<GameObject> allDoors;
    public Transform doorL, doorR, doorB, doorT;
    public Transform wallL, wallR, wallB, wallT;
    public List<GameObject> attachedConnectors;
    public GameObject connectorSpawnedOff;
    public bool markedForDiscard;
    void Awake()
    {
        intersectionCheck = GetComponent<IntersectionRaycast>();
        roomInstance = gameObject;
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
<<<<<<< Updated upstream
=======
        //if (!gameObject.CompareTag("StartingRoom"))
        {
            LevelBuilder.Instance.spawnedRooms.Add(gameObject); //  Add to the list of rooms already in the level
        }

        CameraManager.Instance.virtualCameras.Add(_roomCam.GetComponent<CinemachineVirtualCamera>());
>>>>>>> Stashed changes
        //connectorSpawnedOff = LevelBuilder.Instance._spawnedConnectors[^1];
        /*distToRoomCentre.x = (wallL.transform.localPosition.x - wallR.transform.localPosition.x);
        Debug.Log(gameObject + " Distance between left/right walls and centre: " + distToRoomCentre.x);
        distToRoomCentre.y = (wallT.transform.localPosition.y - wallB.transform.localPosition.y);
        Debug.Log(gameObject + "Distance between top/bottom walls and centre: " + distToRoomCentre.y);*/
    }
<<<<<<< Updated upstream
=======

    private void OnDestroy()
    {
        LevelBuilder.Instance.spawnedRooms.Remove(gameObject);
        CameraManager.Instance.virtualCameras.Remove(_roomCam.GetComponent<CinemachineVirtualCamera>());
        if (rareRoom)
        {
            LevelBuilder.Instance.possibleRooms.Add(
                Resources.Load<GameObject>(LevelBuilder.Instance.floorSpecificRoomPath + gameObject.name.Substring(gameObject.name.Length - 7)));
        }

        foreach (var connector in attachedConnectors)
        {
            if (connector != null)
            {
                connector.GetComponent<ConnectorRoomInfo>().attachedRooms.Remove(gameObject);
            }
        }
    }
>>>>>>> Stashed changes
}
