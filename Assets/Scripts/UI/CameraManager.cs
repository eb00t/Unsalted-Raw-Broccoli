using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using UnityEngine.Serialization;

public class CameraManager : MonoBehaviour
{
    public static CameraManager Instance { get; private set; }
    
    private CinemachineBrain _cinemachineBrain;
    public enum CameraMode
    {
        Default,
        Map
    }
    public CameraMode mode = CameraMode.Default;
    public CinemachineVirtualCamera playerCam;
    public CinemachineVirtualCamera currentCamera;
    public List<CinemachineVirtualCamera> virtualCameras;

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("More than one CameraManager script in the scene.");
        }
        Instance = this;
    }
    

    void Start()
    {
        playerCam = GameObject.FindWithTag("PlayerCam").GetComponent<CinemachineVirtualCamera>();
        if (LevelBuilder.Instance.currentFloor is not (LevelBuilder.LevelMode.Intermission
            or LevelBuilder.LevelMode.Tutorial)) // Set these up manually in the aforementioned scenes
        {
            virtualCameras = new List<CinemachineVirtualCamera> { playerCam };
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
