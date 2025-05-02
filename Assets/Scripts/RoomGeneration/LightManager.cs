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
            if (lit == null) continue;
            lit.enabled = false;
        }

        foreach (var lit in allRoomLights)
        {
            if (lit == null) continue;
            lit.enabled = false;
        }
    }

    public void CheckQueues()
    {
        if (connectorLightQueue.Count > 4)
        {
            if (connectorLightQueue[0] != null && connectorLightQueue[0].GetComponent<ConnectorRoomInfo>() != null)
            {
                foreach (var lit in connectorLightQueue[0].GetComponent<ConnectorRoomInfo>().allLights)
                {
                    if (lit == null) continue;
                    lit.enabled = false;
                }

                connectorLightQueue.RemoveAt(0);
            }
        }

        if (roomLightQueue.Count > 2)
        {
            if (roomLightQueue[0] != null && roomLightQueue[0].GetComponent<RoomInfo>() != null)
            {
                foreach (var lit in roomLightQueue[0].GetComponent<RoomInfo>().allLights)
                {
                    if (lit == null) continue;
                    lit.enabled = false;
                }

                roomLightQueue.RemoveAt(0);
            }
        }
    }
}
