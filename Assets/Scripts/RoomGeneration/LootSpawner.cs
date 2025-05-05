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
                LootManager.Instance.SpawnRandomLootHere(transform, new Vector3(transform.position.x, transform.position.y, 0.05f));
            }
            else if (whatToSpawn == WhatToSpawn.Lore)
            {
                LootManager.Instance.SpawnRandomLoreHere(transform);
            }
        }

        if (howToSpawn == HowToSpawn.Specific)
        {
            if (whatToSpawn == WhatToSpawn.Loot)
            {
                LootManager.Instance.SpawnSpecificLootHere(transform, pathForSpecificItem);
            }
            else if (whatToSpawn == WhatToSpawn.Lore)
            {
                LootManager.Instance.SpawnSpecificLoreHere(transform, pathForSpecificItem);
            }
        }
    }
}
