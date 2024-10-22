using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConnectorRoomInfo : MonoBehaviour
{
    public bool horizontal;
    public List<GameObject> spawnWalls;
    public GameObject wallL, wallR, wallT, wallB;
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
