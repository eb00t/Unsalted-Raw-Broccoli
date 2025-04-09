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
    public List<Light> allLights = new List<Light>();
    public bool markedForDiscard;

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

        foreach (var lit in GetComponentsInChildren<Light>())
        {
            allLights.Add(lit.GetComponent<Light>());
        }
    }

    void Start()
    {
        LevelBuilder.Instance.spawnedConnectors.Add(gameObject);
    }

    void OnDestroy()
    {
        LevelBuilder.Instance.spawnedConnectors.Remove(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        foreach (var lit in LevelBuilder.Instance.spawnedConnectors)
        {
            foreach (var lit2 in lit.GetComponent<ConnectorRoomInfo>().allLights)
            {
                lit2.GetComponent<Light>().enabled = false;
            }
        }
        foreach (var lit in allLights)
        {
            lit.GetComponent<Light>().enabled = true;
        }
        foreach (var lit in LevelBuilder.Instance.spawnedRooms)
        {
            foreach (var lit2 in lit.GetComponent<RoomInfo>().allLights)
            {
                lit2.GetComponent<Light>().enabled = false;
            }
        }
        foreach (var lit in attachedRooms)
        {
            foreach (var lit2 in lit.GetComponent<RoomInfo>().allLights)
            {
                lit2.GetComponent<Light>().enabled = true;
            }
        }
        
    }
}
