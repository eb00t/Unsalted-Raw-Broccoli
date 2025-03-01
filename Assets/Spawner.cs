using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class Spawner : MonoBehaviour
{
    [field: Header("Configuration")]
    public enum SpawnMode
    {
        Random,
        Specific_NOT_IMPLEMENTED
    }
    public SpawnMode spawnMode = SpawnMode.Random;
    public int waves = 1;
    private int _rng;
    public RoomScripting roomScripting;
    public List<GameObject> spawnQueue, possibleEnemies, spawnedEnemies;
    public bool disabled;
    void Start()
    {
        roomScripting = transform.root.GetComponent<RoomScripting>();
        for (int i = 0; i < waves; i++)
        {
            roomScripting._enemyCount++;
        }
        //waves = RandomiseNumber(3);
        if (spawnMode == SpawnMode.Random)
        {
            foreach (var enemy in Resources.LoadAll<GameObject>("Enemies/Normal Enemies"))
            {
                possibleEnemies.Add(enemy);
            }
            for (int i = 0; i < waves; i++)
            {
                _rng = RandomiseNumber(possibleEnemies.Count);
                spawnQueue.Add(possibleEnemies[_rng]);
            }
        }
        SpawnEnemies();
    }

    public void SpawnEnemies()
    {
        if (!disabled)
        { 
            GameObject enemyToSpawn = spawnQueue[0];
            enemyToSpawn = Instantiate(enemyToSpawn, transform.position, Quaternion.identity);
            enemyToSpawn.GetComponent<EnemyHandler>().roomScripting = roomScripting;
            enemyToSpawn.GetComponent<EnemyHandler>()._spawner = this;
            enemyToSpawn.transform.parent = gameObject.transform;
            spawnedEnemies.Add(enemyToSpawn);
            spawnQueue.Remove(spawnQueue[0]);
            if (spawnQueue.Count == 0)
            {
                DisableSpawner();
            }
        }
    }

    void DisableSpawner()
    {
        disabled = true;
        roomScripting.spawners.Remove(gameObject);
    }

    private int RandomiseNumber(int setSize)
    {
        int rng = Random.Range(0, setSize);
        return rng;
    }
}
