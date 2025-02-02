using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(RoomInfo))]
public class RoomScripting : MonoBehaviour
{
    private int _enemyCount;
    public List<GameObject> enemies;
    public List<GameObject> allDoors;
    public bool allDoorsClosed;
    private RoomInfo _roomInfo;
    private bool _roomCleared;
    public bool _playerIsInRoom;
    public CinemachineVirtualCamera _roomCam;

    private void Start()
    {
        _roomInfo = GetComponent<RoomInfo>();
        _roomCam = _roomInfo.roomCam;
        allDoors = new List<GameObject>(_roomInfo.allDoors);
        foreach (var door in allDoors)
        {
            door.AddComponent<DoorInfo>();
        }

        StartCoroutine(CheckIfRoomHasEnemies());
    }

    public void CheckDoors()
    {
        foreach (var door in allDoors)
        {
            door.GetComponent<DoorInfo>().CheckDoors();
        }
    }

    IEnumerator CheckIfRoomHasEnemies()
    {
        while (true)
        {
            if (_enemyCount > 0 && _playerIsInRoom)
            {
                CloseAllRoomDoors();
            }
            else if (_playerIsInRoom == false)
            {
                OpenAllRoomDoors();
            }
            else if (_enemyCount <= 0)
            {
                OpenAllRoomDoors();
                _roomCleared = true;
            }

            if (_enemyCount >= 2)
            {
                AudioManager.Instance.SetMusicParameter("Combat Weight", 1);
            }
            else
            {
                AudioManager.Instance.SetMusicParameter("Combat Weight", 0);
            }

            if (_roomCleared)
            {
                StopCoroutine(CheckIfRoomHasEnemies());
            }
            yield return new WaitForSeconds(1f);
        }
    }

    void OpenAllRoomDoors()
    {
        foreach (var door in _roomInfo.usableDoors)
        {
            door.GetComponent<DoorInfo>().OpenDoor();
            door.GetComponent<DoorInfo>().closed = false;
        }
        if (allDoorsClosed && _playerIsInRoom)
        {
            AudioManager.Instance.SetMusicParameter("Music Track", 0);
        }
        allDoorsClosed = false;
    }

    void CloseAllRoomDoors()
    {
        foreach (var door in _roomInfo.usableDoors)
        {
            door.GetComponent<DoorInfo>().CloseDoor();
            door.GetComponent<DoorInfo>().closed = true;
        }
        if (allDoorsClosed == false && _roomInfo.bossRoom == false)
        {
            AudioManager.Instance.SetMusicParameter("Music Track", 1);
        }
        else if (allDoorsClosed == false && _roomInfo.bossRoom)
        {
            AudioManager.Instance.SetMusicParameter("Music Track", 2);
        }
        allDoorsClosed = true;
    }

    void Update()
    {
        _enemyCount = enemies.Count;
        if (_roomCam.Priority > 9)
        {
            _playerIsInRoom = true;
        }
        else
        {
            _playerIsInRoom = false;
        }
    }
    
}
