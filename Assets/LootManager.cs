using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class LootManager : MonoBehaviour
{    
    public static LootManager Instance { get; private set; }
    public List<GameObject> possibleLoot;
    private int _willLootSpawn = 1; //It has a 20% chance to spawn by default
    private void Awake()
    {
        foreach (var item in Resources.LoadAll<GameObject>("ItemPrefabs"))
        {
            possibleLoot.Add(item);
        }
        if (Instance != null)
        {
            Debug.LogError("More than one LootManager script in the scene.");
        }

        Instance = this;
    }
    

    public void SpawnLootInCurrentRoom(GameObject room)
    {
        int spawnChance = RandomiseNumber(_willLootSpawn);
        if (spawnChance == 0)
        {
            Debug.Log("Rolled a zero; spawning loot.");
            int chosenLoot = RandomiseNumber(possibleLoot.Count);
            float offsetSpawnPos;
            int leftOffset = RandomiseNumber(2);
            Debug.Log(leftOffset);
            if (leftOffset == 0)
            {
                offsetSpawnPos = room.transform.position.x - room.GetComponent<RoomInfo>().roomLength / 4;
            }
            else
            {
                offsetSpawnPos = room.transform.position.x + room.GetComponent<RoomInfo>().roomLength / 4;
            }
            Vector3 realSpawnPos = new Vector3(offsetSpawnPos, room.transform.position.y, room.transform.position.z);
            GameObject lootToSpawn = Instantiate(possibleLoot[chosenLoot], realSpawnPos, Quaternion.identity);
            possibleLoot.Remove(possibleLoot[chosenLoot]);
        }
        else
        {
            Debug.Log("No loot spawning.");
        }
    }
    
    private int RandomiseNumber(int setSize)
    {
        int rng = Random.Range(0, setSize);
        return rng;
    }
}


