using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class ConnectorRoomInfo : MonoBehaviour
{
    public bool horizontal;
    public List<Transform> spawnWalls;
    public List<GameObject> attachedRooms;
    public Transform wallL, wallR, wallT, wallB;
    public string spawnedOnSide;
    public float connectorLength;
    public float connectorHeight; // The smaller side (should typically be the same for each connector)
    public List<Light> allLights;
    public bool markedForDiscard;
    public List<GameObject> attachedDoors;

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

    void OnDestroy()
    {
        foreach (var room in attachedRooms)
        {
            if (room.GetComponent<RoomInfo>().attachedConnectors.Contains(gameObject))
            {
                 room.GetComponent<RoomInfo>().attachedConnectors.Remove(gameObject);
            }
           
        }

        foreach (var door in attachedDoors)
        {
            door.transform.root.GetComponent<RoomInfo>().usableDoors.Remove(door);
            door.GetComponent<DoorInfo>().CloseDoor();
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
                    lit.GetComponent<Light>().enabled = true;
                    foreach (var lit2 in attachedRooms)
                    {
                        if (lit2 != null)
                        {
                            lit2.GetComponentInChildren<Light>().enabled = true;
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
