using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class CameraTrigger : MonoBehaviour
{
    private DoorInfo _doorInfo;
    private CinemachineVirtualCamera _playerCam;
    private CinemachineVirtualCamera _camera;
    void Start()
    {
        _doorInfo = transform.parent.GetComponent<DoorInfo>();
        StartCoroutine(CheckIfDoorCanOpen());
        _camera = transform.root.GetComponent<CinemachineVirtualCamera>();
    }

    IEnumerator CheckIfDoorCanOpen()
    {
        yield return new WaitForSecondsRealtime(1.6f);
        if (_doorInfo.hasDoor == false)
        {
            gameObject.SetActive(false);
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            foreach (var cam in CameraManager.Instance.virtualCameras)
            {
                cam.Priority = 9;
            }
            _camera.Priority = 10;
        }
       
    }
}
