using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class LootSpawner : MonoBehaviour
{
   public enum HowToSpawn
   {
       Random,
       Specific,
   }
    public enum WhatToSpawn
    {
        Loot,
        Lore
    }
   
   public HowToSpawn howToSpawn;
   public WhatToSpawn whatToSpawn;
   private GameObject _spawnedItem;
   public string pathForSpecificItem;
    void Start()
    {
        SpawnItem();
    }

    void SpawnItem()
    {
        if (howToSpawn == HowToSpawn.Random)
        {
            if (whatToSpawn == WhatToSpawn.Loot)
            {
                LootManager.Instance.SpawnRandomLootHere(transform);
            }
            else if (whatToSpawn == WhatToSpawn.Lore)
            {
                LootManager.Instance.SpawnRandomLoreHere(transform);
            }
        }

        if (howToSpawn == HowToSpawn.Specific)
        {
            LootManager.Instance.SpawnSpecificLootHere(transform, pathForSpecificItem);
        }
    }
}
