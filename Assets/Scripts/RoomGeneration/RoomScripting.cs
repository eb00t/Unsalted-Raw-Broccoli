using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(RoomInfo))]
public class RoomScripting : MonoBehaviour
{
    public int _enemyCount, lastEnemyCount = 0;
    public int enabledSpawnerCount;
    public int currentWave;
    public List<GameObject> enemies;
    public List<GameObject> allDoors;
    public List<GameObject> spawners;
    public bool allDoorsClosed;
    private RoomInfo _roomInfo;
    private bool _roomCleared;
    public bool playerIsInRoom;
    public bool playerHasEnteredRoom;
    public bool roomHadEnemies;
    private bool _musicHasChanged;
    private bool _lootSpawned;
    public CinemachineVirtualCamera _roomCam;

    private void Start()
    {
        currentWave = 0;
        foreach (var spawner in transform.GetComponentsInChildren<Transform>())
        {
            if (spawner.CompareTag("Spawner"))
            {
                spawners.Add(spawner.gameObject);
            }
        }
        _roomInfo = GetComponent<RoomInfo>();
        _roomCam = _roomInfo.roomCam;
        allDoors = new List<GameObject>(_roomInfo.allDoors);
        foreach (var door in allDoors)
        {
            door.AddComponent<DoorInfo>();
        }
        ChangeCombatWeight();
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
            if (enabledSpawnerCount > 0 || enemies.Count > 0)
            {
                roomHadEnemies = true;
            }
            if (_enemyCount > 0 && playerIsInRoom && allDoorsClosed == false)
            {
                CloseAllRoomDoors();
            }
            else if (_enemyCount <= 0 && allDoorsClosed && enabledSpawnerCount <= 0)
            {
                OpenAllRoomDoors();
                _roomCleared = true;
            }

            if (_roomCleared)
            {
                if (roomHadEnemies && _lootSpawned == false)
                {
                    _lootSpawned = true;
                    LootManager.Instance.SpawnLootInCurrentRoom(gameObject);
                }

                StopCoroutine(CheckIfRoomHasEnemies());
            }

            if (lastEnemyCount != _enemyCount)
            {
                ChangeCombatWeight();
            }
            Debug.Log("Last enemy count: " + lastEnemyCount);
            lastEnemyCount = _enemyCount;
            
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
        AudioManager.Instance.SetMusicParameter("Music Track", 0);
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
        foreach (var spawner in spawners)
        {
            spawner.GetComponent<Spawner>().SpawnEnemies();
        }
    }

    void ChangeCombatWeight()
    {
        if (_enemyCount < 2)
        {
            AudioManager.Instance.SetMusicParameter("Combat Weight", 0);
        }
        else if (_enemyCount >= 2)
        {
            AudioManager.Instance.SetMusicParameter("Combat Weight", 1);
        }
    }
    
    void EnterSpecialRoom()
    {
        if (_roomInfo.shop)
        {
            AudioManager.Instance.SetMusicParameter("Music Track", 3);
            _musicHasChanged = true;
        }
    }

    void ExitSpecialRoom()
    {
        AudioManager.Instance.SetMusicParameter("Music Track", 0);
        _musicHasChanged = false;
    }
    
    void Update()
    {
        _enemyCount = enemies.Count;
        enabledSpawnerCount = spawners.Count;
        if (_roomCam.Priority > 9 && allDoorsClosed == false)
        {
            CameraManager.Instance.currentCamera = _roomCam;
            playerIsInRoom = true;
            playerHasEnteredRoom = true;
            if (_musicHasChanged == false)
            {
                EnterSpecialRoom();
            }
        }
        else
        {
            playerIsInRoom = false;
            if (_musicHasChanged)
            {
                ExitSpecialRoom();
            }
        }
    }
    
}
