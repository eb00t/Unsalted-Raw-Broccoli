using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

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
    private GameObject _player;
    private bool _musicHasChanged;
    private bool _lootSpawned;
    public CinemachineVirtualCamera _roomCam;
    private float _failsafeTeleport = 3.5f;

    private void Start()
    {
        currentWave = 0;
        _player = GameObject.FindWithTag("Player");
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

                yield break;
            }

            if (lastEnemyCount > _enemyCount)
            {
                ChangeCombatWeight();
            }
            //Debug.Log("Last enemy count: " + lastEnemyCount);
            lastEnemyCount = _enemyCount;
            
            yield return new WaitForSeconds(.25f);
        }
    }

    void OpenAllRoomDoors()
    {
        foreach (var door in _roomInfo.usableDoors)
        {
            door.GetComponent<DoorInfo>().OpenDoor();
            door.GetComponent<DoorInfo>().closed = false;
        }

        if (!_roomInfo.bossRoom)
        {
            switch (LevelBuilder.Instance.bossDead)
            {
                case true:
                    AudioManager.Instance.SetGlobalEventParameter("Music Track", 4);
                    break;
                default:
                    AudioManager.Instance.SetGlobalEventParameter("Music Track", 0);
                    break;
            }
        }
        allDoorsClosed = false;
    }

    void CloseAllRoomDoors()
    {
        int rng = Random.Range(0, 3);
        AudioManager.Instance.SetMusicParameter("Combat Weight", 0);
        foreach (var door in _roomInfo.usableDoors)
        {
            door.GetComponent<DoorInfo>().CloseDoor();
            door.GetComponent<DoorInfo>().closed = true;
        }
        if (allDoorsClosed == false && _roomInfo.bossRoom == false)
        {
            AudioManager.Instance.SetGlobalEventParameter("Music Track", 1);
            AudioManager.Instance.SetMusicParameter("Music Type", rng);
        }
        else if (allDoorsClosed == false && _roomInfo.bossRoom)
        {
            AudioManager.Instance.SetGlobalEventParameter("Music Track", 2);
        }
        allDoorsClosed = true;
        foreach (var spawner in spawners)
        {
            spawner.GetComponent<EnemySpawner>().SpawnEnemies();
        }
    }

    void ChangeCombatWeight()
    {
        if (_enemyCount >= 2)
        {
            AudioManager.Instance.SetMusicParameter("Combat Weight", 1);
        }
    }

    public void EnterSpecialRoom()
    {
        if (_roomInfo.shop && _musicHasChanged == false)
        {
            AudioManager.Instance.SetGlobalEventParameter("Music Track", 3);
            _musicHasChanged = true;
        }

        /*if (_roomInfo.bossRoom)
        {
            AudioManager.Instance.SetMusicParameter("Music Track", 2);
            _musicHasChanged = true;
        }*/
    }

    public void ExitSpecialRoom()
    {
        switch (LevelBuilder.Instance.bossDead)
        {
            case true:
                AudioManager.Instance.SetGlobalEventParameter("Music Track", 4);
                break;
            default:
                AudioManager.Instance.SetGlobalEventParameter("Music Track", 0);
                break;
        }

        _musicHasChanged = false;
    }
    
    void Update()
    {
        _enemyCount = enemies.Count;
        enabledSpawnerCount = spawners.Count;
        if (playerIsInRoom)
        {
            playerHasEnteredRoom = true;
        }
        if (playerHasEnteredRoom)
        {
            if (!_roomInfo.mapIcons.activeSelf)
            {
                _roomInfo.mapIcons.SetActive(true);
                foreach (var connector in _roomInfo.attachedConnectors)
                {
                    ConnectorRoomInfo conRoomInfo = connector.GetComponent<ConnectorRoomInfo>();
                    conRoomInfo.mapIconParent.SetActive(true);
                }
            }

            _roomInfo.coveredUp = false;
        }
        
        /*
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
        */

        if (allDoorsClosed && playerIsInRoom == false)
        {
                OpenAllRoomDoors();
        }
    }
}
