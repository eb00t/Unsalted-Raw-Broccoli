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
    }

    void Start()
    {
        LevelBuilder.Instance.spawnedConnectors.Add(gameObject);
    }

    void OnDestroy()
    {
        LevelBuilder.Instance.spawnedConnectors.Remove(gameObject);
    }
    
}
