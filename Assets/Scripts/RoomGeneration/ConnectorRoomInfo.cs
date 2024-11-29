using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class ConnectorRoomInfo : MonoBehaviour
{
    public bool horizontal;
    public List<Transform> spawnWalls;
    public Transform wallL, wallR, wallT, wallB;
    public string spawnedOnSide;
    public float connectorLength;
    public float connectorHeight; // The smaller side (should typically be the same for each connector)
    public bool markedForDiscard;
    public ConnectorIntersectionRaycast intersectionCheck;

    private void Awake()
    {
        intersectionCheck = GetComponent<ConnectorIntersectionRaycast>();
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
        LevelBuilder.Instance._spawnedConnectors.Add(gameObject);
    }

    void OnDestroy()
    {
        LevelBuilder.Instance._spawnedConnectors.Remove(gameObject);
    }
    
}
