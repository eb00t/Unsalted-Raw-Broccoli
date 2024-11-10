using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using UnityEngine.Serialization;

public class MapTargetGroup : MonoBehaviour
{
     public static MapTargetGroup Instance { get; private set; }
     public CinemachineTargetGroup targetGroup;
   
    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("More than one CameraController script in the scene.");
        }

        Instance = this;
        
    }

    void Start()
    {
        targetGroup = gameObject.GetComponent<CinemachineTargetGroup>();
        AddRoomsToTargetGroup();
    }

    public void AddRoomsToTargetGroup()
    {
        foreach (var room in LevelBuilder.Instance.spawnedRooms)
        {
            targetGroup.AddMember(room.transform, 1, 5);
            Debug.Log("Adding " + room + " to CinemachineTargetGroup.");
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (CameraController.Instance.lookAtWholeMap)
        {
            
        }
    }
}
