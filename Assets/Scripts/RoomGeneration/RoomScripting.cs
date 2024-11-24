using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(RoomInfo))]
public class RoomScripting : MonoBehaviour
{
    public int enemyCount;
    public List<GameObject> allDoors;
    public RoomInfo roomInfo;

    private void Awake()
    {
        roomInfo = GetComponent<RoomInfo>();
        allDoors = new List<GameObject>(roomInfo.allDoors);
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
