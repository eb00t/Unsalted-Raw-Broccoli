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
    public List<RoomScripting> allRooms;
    private ResizeBoxCollider _resizeBoxCollider;

    public enum RoomOrConnector
    {
        Room,
        Connector,
    }

    public RoomOrConnector roomOrConnector;

    void Start()
    {
        _resizeBoxCollider = transform.root.GetComponent<ResizeBoxCollider>();
        foreach (GameObject room in LevelBuilder.Instance.spawnedRooms)
        {
            allRooms.Add(room.GetComponent<RoomScripting>());
        }
        //_doorInfo = transform.parent.GetComponent<DoorInfo>();
        _playerCam = CameraManager.Instance.playerCam;
        Debug.Log(_playerCam.name);
        //StartCoroutine(CheckIfDoorCanOpen());
        switch (roomOrConnector)
        {
            case RoomOrConnector.Room:
            {
                if (LevelBuilder.Instance.currentFloor != LevelBuilder.LevelMode.Intermission)
                {
                    _camera = transform.root.transform.Find("RoomCam").GetComponent<CinemachineVirtualCamera>();
                }
                else
                {
                    _camera = transform.parent.transform.parent.transform.parent.Find("RoomCam")
                        .GetComponent<CinemachineVirtualCamera>();
                }

            }
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
                        cam.Priority = 0;
                    }
                    _camera.Priority = 10;
                    if (LevelBuilder.Instance.currentFloor != LevelBuilder.LevelMode.Tutorial || LevelBuilder.Instance.currentFloor != LevelBuilder.LevelMode.Intermission)
                    {
                        _resizeBoxCollider.doorsCanClose = false;
                    }

                    break;
                case RoomOrConnector.Connector:
                    foreach (var cam in CameraManager.Instance.virtualCameras)
                    {
                        cam.Priority = 0;
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
                    cam.Priority = 0;
                }
                foreach (var room in allRooms)
                {
                    room.playerIsInRoom = false;
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
                    cam.Priority = 0;
                }
                _camera.Priority = 10;
                if (LevelBuilder.Instance.currentFloor != LevelBuilder.LevelMode.Tutorial || LevelBuilder.Instance.currentFloor != LevelBuilder.LevelMode.Intermission)
                {
                    _resizeBoxCollider.doorsCanClose = true;
                }
            }
        }
    }
}
