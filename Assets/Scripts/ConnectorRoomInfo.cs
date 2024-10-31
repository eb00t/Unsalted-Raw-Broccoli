using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class ConnectorRoomInfo : MonoBehaviour
{
    public bool horizontal;
    public List<GameObject> spawnWalls;
    public GameObject wallL, wallR, wallT, wallB;
    public string spawnedOnSide;
    public int connectorSize;

    void Awake()
    {
        switch (horizontal)
        {
            case true:
                spawnWalls.Add(wallL);
                spawnWalls.Add(wallR);
                break;
            case false:
            {
                spawnWalls.Add(wallB);
                spawnWalls.Add(wallT);
                break;
            }
        }
    }
    
}
