using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class CameraTrigger : MonoBehaviour
{
    //private DoorInfo _doorInfo;
    private CinemachineVirtualCamera _playerCam;
    private CinemachineVirtualCamera _camera;

    public enum RoomOrConnector
    {
        Room,
        Connector,
    }

    public RoomOrConnector roomOrConnector;

    void Start()
    {
        //_doorInfo = transform.parent.GetComponent<DoorInfo>();
        _playerCam = CameraManager.Instance.playerCam;
        //StartCoroutine(CheckIfDoorCanOpen());
        switch (roomOrConnector)
        {
            case RoomOrConnector.Room:
            {
                _camera = transform.root.transform.Find("RoomCam").GetComponent<CinemachineVirtualCamera>();
                break;
            }
            default:
                break;
        }

    }

    IEnumerator CheckIfDoorCanOpen()
    {
        yield return new WaitForSecondsRealtime(1.6f);
        //if (_doorInfo.hasDoor == false)
        {
            gameObject.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            switch (roomOrConnector)
            {
                case RoomOrConnector.Room:
                    foreach (var cam in CameraManager.Instance.virtualCameras)
                    {
                        cam.Priority = 9;
                    }

                    _camera.Priority = 10;
                    break;
                case RoomOrConnector.Connector:
                    foreach (var cam in CameraManager.Instance.virtualCameras)
                    {
                        cam.Priority = 9;
                    }

                    _playerCam.Priority = 10;
                    break;
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (roomOrConnector == RoomOrConnector.Connector)
            {
                foreach (var cam in CameraManager.Instance.virtualCameras)
                {
                    cam.Priority = 9;
                }

                _playerCam.Priority = 10;
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (roomOrConnector == RoomOrConnector.Room)
            {
                foreach (var cam in CameraManager.Instance.virtualCameras)
                {
                    cam.Priority = 9;
                }

                _camera.Priority = 10;
            }
        }
    }
}