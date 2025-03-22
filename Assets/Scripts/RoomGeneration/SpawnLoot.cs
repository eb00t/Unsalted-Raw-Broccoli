using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class SpawnLoot : MonoBehaviour
{
   public enum SpawnMode
   {
       Random,
       Specific,
   }

   public SpawnMode spawnMode;
   private GameObject _spawnedItem;
   public string pathForSpecificItem;
    // Start is called before the first frame update
    void Start()
    {
        SpawnItem();
    }

    void SpawnItem()
    {
        if (spawnMode == SpawnMode.Random)
        {
            LootManager.Instance.SpawnRandomLootHere(transform);
        }

        if (spawnMode == SpawnMode.Specific)
        {
            LootManager.Instance.SpawnSpecificLootHere(transform, pathForSpecificItem);
        }
    }
}
