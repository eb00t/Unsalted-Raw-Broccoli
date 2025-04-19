using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class LootManager : MonoBehaviour
{    
    public static LootManager Instance { get; private set; }
    public List<GameObject> minorLoot, majorLoot, lore;
    private readonly int _willLootSpawn = 2; //It has a 10% chance to spawn by default
    private int _willMajorLootSpawn = 10;
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
        foreach (var item in Resources.LoadAll<GameObject>("ItemPrefabs/Lore"))
        {
            lore.Add(item);
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
        int majorLootChance = RandomiseNumber(_willMajorLootSpawn);
        if (spawnChance == 0)
        {
            int chosenLoot;
            GameObject lootToSpawn;
            Debug.Log("Spawning loot.");
            if (majorLootChance != 0)
            {
                chosenLoot = RandomiseNumber(minorLoot.Count);
            }
            else
            {
                chosenLoot = RandomiseNumber(majorLoot.Count);
            }

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
            if (majorLootChance != 0)
            {
                lootToSpawn = Instantiate(minorLoot[chosenLoot], realSpawnPos, Quaternion.identity);
            }
            else
            {
                lootToSpawn = Instantiate(majorLoot[chosenLoot], realSpawnPos, Quaternion.identity);
            }

            lootToSpawn.SetActive(true);
        }
        else
        {
            Debug.Log("No loot spawning.");
        }
    }

    public void SpawnRandomLootHere(Transform here)
    {
        int chosenLoot = RandomiseNumber(majorLoot.Count);
        GameObject lootToSpawn = Instantiate(majorLoot[chosenLoot], here.position, Quaternion.identity);
        lootToSpawn.SetActive(true);
        lootToSpawn.transform.parent = here.transform; 
        majorLoot.Remove(majorLoot[chosenLoot]);
    }

    public void SpawnSpecificLootHere(Transform here, string path)
    {
        GameObject chosenLoot = Resources.Load<GameObject>(path);
        GameObject lootToSpawn = Instantiate(chosenLoot, here.position, Quaternion.identity);
        lootToSpawn.SetActive(true);
        lootToSpawn.transform.parent = here.transform;
        majorLoot.Remove(chosenLoot);
    }
    
    public void SpawnRandomLoreHere(Transform here)
    {
        int chosenLoot = RandomiseNumber(lore.Count);
        GameObject lootToSpawn = Instantiate(lore[chosenLoot], here.position, Quaternion.identity);
        lootToSpawn.SetActive(true);
        lootToSpawn.transform.parent = here.transform; 
    }

    public void SpawnSpecificLoreHere(Transform here, string path) // TODO: THIS DOESN'T WORK, SON!
    {
        GameObject chosenLoot = Resources.Load<GameObject>("ItemPrefabs/Lore/Data Lore");
        GameObject lootToSpawn = Instantiate(chosenLoot, here.position, Quaternion.identity);
        LoreItemHandler loreItemHandler = Resources.Load<LoreItemHandler>(path);
        Debug.Log(loreItemHandler);
        lootToSpawn.GetComponent<dialogueControllerScript>().randomLore = false;
        lootToSpawn.GetComponent<dialogueControllerScript>().loreToLoad = loreItemHandler;
        lootToSpawn.GetComponent<ReadLore>().whatLore = loreItemHandler;
        lootToSpawn.SetActive(true);
        lootToSpawn.transform.parent = here.transform;
    }
    
    private int RandomiseNumber(int setSize)
    {
        int rng = Random.Range(0, setSize);
        return rng;
    }
}


