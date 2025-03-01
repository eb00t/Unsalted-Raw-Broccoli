using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class LootManager : MonoBehaviour
{    
    public static LootManager Instance { get; private set; }
    public List<GameObject> minorLoot, majorLoot;
    private int _willLootSpawn = 11; //It has a 10% chance to spawn by default
    private void Awake()
    {
        foreach (var item in Resources.LoadAll<GameObject>("ItemPrefabs/Minor Items"))
        {
            minorLoot.Add(item);
        }
        foreach (var item in Resources.LoadAll<GameObject>("ItemPrefabs/Major Items"))
        {
            majorLoot.Add(item);
        }
        if (Instance != null)
        {
            Debug.LogError("More than one LootManager script in the scene.");
        }

        Instance = this;
    }
    

    public void SpawnLootInCurrentRoom(GameObject room) //TODO: Add a chance to spawn an item from the other loot table
    {
        int spawnChance = RandomiseNumber(_willLootSpawn);
        if (spawnChance == 0)
        {
            Debug.Log("Rolled a zero; spawning loot.");
            int chosenLoot = RandomiseNumber(minorLoot.Count);
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
            GameObject lootToSpawn = Instantiate(minorLoot[chosenLoot], realSpawnPos, Quaternion.identity);
        }
        else
        {
            Debug.Log("No loot spawning.");
        }
    }

    public void SpawnRandomLootHere(Vector3 here)
    {
        int chosenLoot = RandomiseNumber(majorLoot.Count);
        GameObject lootToSpawn = Instantiate(majorLoot[chosenLoot], here, Quaternion.identity);
        majorLoot.Remove(majorLoot[chosenLoot]);
    }

    public void SpawnSpecificLootHere(Vector3 here, string path)
    {
        GameObject chosenLoot = Resources.Load<GameObject>(path);
        GameObject lootToSpawn = Instantiate(chosenLoot, here, Quaternion.identity);
        majorLoot.Remove(chosenLoot);
    }
    
    private int RandomiseNumber(int setSize)
    {
        int rng = Random.Range(0, setSize);
        return rng;
    }
}


