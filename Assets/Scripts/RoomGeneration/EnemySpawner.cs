using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class EnemySpawner : MonoBehaviour
{
    [field: Header("Configuration")]
    private int _waves, _waveCount = 1;
    public enum HowToSpawn
    {
        Random,
        Specific_NOT_IMPLEMENTED
    }
    public HowToSpawn howToSpawn = HowToSpawn.Random;
    
    private int _rng;
    public GameObject spawnedEnemy;
    public RoomScripting roomScripting;
    public List<GameObject> spawnQueue, possibleEnemies, spawnedEnemies;
    public int basicWeight, stalkerWeight, bomberWeight, cameraWeight;
    public bool disabled;

    void Start()
    {
        roomScripting = transform.root.GetComponent<RoomScripting>();
        _waves = RandomiseNumber(100);
        for (int i = 0; i < _waveCount; i++)
        {
            roomScripting._enemyCount++;
        }
        switch (_waves)
        {
            case 0:
                _waveCount = 0;
                break;
            case > 0 and < 74:
                _waveCount = 1;
                break;
            case > 74 and < 98:
                _waveCount = 2;
                break;
            case 99:
                _waveCount = 3;
                break;
        }

        Debug.Log("Wave count: " + _waveCount);
        if (howToSpawn == HowToSpawn.Random)
        {
            foreach (var enemy in Resources.LoadAll<GameObject>("Enemies/Normal Enemies"))
            {
                possibleEnemies.Add(enemy);
            }

            for (int i = 0; i < _waveCount; i++)
            {
                _rng = RandomiseNumber(possibleEnemies.Count);
                spawnQueue.Add(possibleEnemies[_rng]);
            }
        }
        SpawnEnemies();
    }

    public void SpawnEnemies()
    {
        if (_waveCount > 0)
        {
            if (!disabled && spawnedEnemy == null)
            {
                GameObject enemyToSpawn = spawnQueue[0];
                enemyToSpawn = Instantiate(enemyToSpawn, transform.position, Quaternion.identity);
                enemyToSpawn.GetComponent<IDamageable>().RoomScripting = roomScripting;
                enemyToSpawn.GetComponent<IDamageable>().EnemySpawner = this;
                enemyToSpawn.transform.parent = gameObject.transform;
                spawnedEnemy = enemyToSpawn;
                spawnedEnemies.Add(enemyToSpawn);
                spawnQueue.Remove(spawnQueue[0]);
                if (spawnQueue.Count == 0)
                {
                    DisableSpawner();
                }
            }
        }
        else
        {
            DisableSpawner();
            Debug.Log("Wave count is 0");
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
