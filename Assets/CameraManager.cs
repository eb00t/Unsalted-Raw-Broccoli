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
        playerCam = GameObject.FindGameObjectWithTag("PlayerCam").GetComponent<CinemachineVirtualCamera>();
        virtualCameras = new List<CinemachineVirtualCamera> { playerCam };
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
