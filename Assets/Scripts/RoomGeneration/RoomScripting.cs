using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(RoomInfo))]
public class RoomScripting : MonoBehaviour
{
    private int _enemyCount;
    public List<GameObject> allDoors;
    private RoomInfo _roomInfo;

    private void Awake()
    {
        _roomInfo = GetComponent<RoomInfo>();
        allDoors = new List<GameObject>(_roomInfo.allDoors);
        foreach (var door in allDoors)
        {
            door.AddComponent<DoorInfo>();
        }
    }

    public void CheckDoors()
    {
        foreach (var door in allDoors)
        {
            door.GetComponent<DoorInfo>().CheckDoors();
        }
        //OpenAllRoomDoors();
    }

    IEnumerator CheckIfRoomHasEnemies()
    {
        while (true)
        {
            if (_enemyCount == 0)
            {
                OpenAllRoomDoors();
            }
            yield return new WaitForSeconds(1f);
        }
    }

    void OpenAllRoomDoors()
    {
        foreach (var door in allDoors)
        {
            if (door.GetComponent<DoorInfo>().hasDoor)
            {
                 door.SetActive(false);
            }
        }
    }
}
