using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public class LightManager : MonoBehaviour
{
    public static LightManager Instance { get; private set; }
    
    public List<Light> allConnectorLights;
    public List<Light> allRoomLights;

    public List<GameObject> connectorLightQueue;
    public List<GameObject> roomLightQueue;
    
    private RoomInfo _startingRoomInfo;

    void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("More than one LightManager script in the scene.");    
        }

        Instance = this;
        
        if (SceneManager.GetActiveScene().name is not "MainScene")
        {
            gameObject.SetActive(false);
        }
        
       
    }

    void Start()
    {
        _startingRoomInfo = GameObject.FindWithTag("StartingRoom").GetComponent<RoomInfo>();
        foreach (var lit in allConnectorLights)
        {
            lit.enabled = false;
        }

        foreach (var lit in allRoomLights)
        {
            lit.enabled = false;
        }
    }

    public void CheckQueues()
    {
        if (connectorLightQueue.Count > 4)
        {
            foreach (var lit in connectorLightQueue[0].GetComponent<ConnectorRoomInfo>().allLights)
            {
                lit.enabled = false;
            }
            connectorLightQueue.RemoveAt(0);
        }

        if (roomLightQueue.Count > 2)
        {
            foreach (var lit in roomLightQueue[0].GetComponent<RoomInfo>().allLights)
            {
                lit.enabled = false;
            }
            roomLightQueue.RemoveAt(0);
        }
    }
}
