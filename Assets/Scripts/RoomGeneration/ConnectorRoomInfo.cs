using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class ConnectorRoomInfo : MonoBehaviour
{
    public List<Transform> spawnWalls; // The walls of the connector that rooms can spawn on.
    public List<GameObject> attachedDoors; 
    public List<GameObject> attachedRooms;
    public List<Light> allLights;
    public Transform wallL, wallR, wallT, wallB;
    public GameObject mapIconParent;
    public GameObject questionMark;
    public string spawnedOnSide;
    public float connectorLength;
    public float connectorHeight; // The smaller side (should typically be the same for each connector)
    public bool markedForDiscard;
    public bool horizontal;
    

    private void Awake()
    {
        attachedRooms = new List<GameObject>();
        spawnWalls = new List<Transform>();
        switch (horizontal)
        {
            case true:
                spawnWalls.Add(wallL.transform);
                spawnWalls.Add(wallR.transform);
                break;
            case false:
                spawnWalls.Add(wallB.transform);
                spawnWalls.Add(wallT.transform);
                break;
        }

        foreach (var child in GetComponentsInChildren<Transform>()) // Assigning map icons
        {
            if (child.CompareTag("Map Icon Parent"))
            {
                mapIconParent = child.gameObject;
            }

            if (child.name == "QuestionMark")
            {
                questionMark = child.gameObject;
            }
        }

        mapIconParent.SetActive(false);
    }

    void Start()
    {
        foreach (var lit in GetComponentsInChildren<Light>())
        {
            LightManager.Instance.allConnectorLights.Add(lit.GetComponent<Light>());
            allLights.Add(lit.GetComponent<Light>());
        }
        foreach (var lit in allLights)
        {
            lit.enabled = false;
        }
        LevelBuilder.Instance.spawnedConnectors.Add(gameObject);
    }

    void Update()
    {
        if (!mapIconParent.activeSelf && LevelBuilder.Instance.bossRoomGeneratingFinished)
        {
            foreach (var room in attachedRooms)
            {
                if (room.CompareTag("StartingRoom"))
                {
                    mapIconParent.SetActive(true);
                }
            }
        }
        if (questionMark.activeSelf && LevelBuilder.Instance.bossRoomGeneratingFinished) // Check to see if the question mark is active and level is built
        {
            if (attachedRooms[0].GetComponent<RoomScripting>().playerHasEnteredRoom &&
                attachedRooms[1].GetComponent<RoomScripting>().playerHasEnteredRoom) // Has the player entered both attached rooms?
            {
                questionMark.SetActive(false);
            }
            
        }
    }


    void OnDestroy()
    {
        foreach (var room in attachedRooms)
        {
            if (room != null && room.GetComponent<RoomInfo>().attachedConnectors.Contains(gameObject))
            {
                 room.GetComponent<RoomInfo>().attachedConnectors.Remove(gameObject);
            }
           
        }

        foreach (var door in attachedDoors)
        {
            if (door != null)
            {
                door.transform.root.GetComponent<RoomInfo>().usableDoors.Remove(door);
                door.GetComponent<DoorInfo>().CloseDoor();
            }
        }
        LevelBuilder.Instance.spawnedConnectors.Remove(gameObject);
        foreach (var lit in allLights)
        { 
            LightManager.Instance.allConnectorLights.Remove(lit);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            foreach (var lit in allLights)
            {
                if (allLights.Count != 0)
                {
                    var light = lit.GetComponent<Light>();
                    if (light != null)
                    {
                        light.enabled = true;
                    }

                    foreach (var lit2 in attachedRooms)
                    {
                        var light2 = lit2.GetComponent<Light>();
                        if (lit2 != null && light2 != null)
                        {
                            light2.enabled = true;
                        }
                    }
                    if (LightManager.Instance.connectorLightQueue.Contains(lit.transform.root.gameObject))
                    {
                        LightManager.Instance.connectorLightQueue.Remove(lit.transform.root.gameObject);
                    }
                    LightManager.Instance.connectorLightQueue.Add(lit.transform.root.gameObject);
                    LightManager.Instance.CheckQueues();
                }
            }
        }
    }

}
