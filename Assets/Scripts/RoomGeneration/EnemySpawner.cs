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

//        Debug.Log("Wave count: " + _waveCount);
        if (howToSpawn == HowToSpawn.Random)
        {
            for (int i = 0; i < _waveCount; i++)
            {
                int randomEnemy = RandomiseNumber(100);
                
                switch (randomEnemy)
                {
                    case <= 29:
                        spawnQueue.Add(Resources.Load<GameObject>("Enemies/Normal Enemies/RobotEnemy"));
                        break;
                    case >= 30 and <= 59:
                        int randomVariantF = RandomiseNumber(3);
                        switch (randomVariantF)
                        {
                            case 0:
                                spawnQueue.Add(Resources.Load<GameObject>("Enemies/Normal Enemies/FlyingEnemy"));
                                break;
                            case 1:
                                spawnQueue.Add(Resources.Load<GameObject>("Enemies/Normal Enemies/LaserEnemy"));
                                break;
                            case 2:
                                spawnQueue.Add(Resources.Load<GameObject>("Enemies/Normal Enemies/ProjectileEnemy"));
                                break;
                        }
                        break;
                    case >= 60 and <= 79:
                        spawnQueue.Add(Resources.Load<GameObject>("Enemies/Normal Enemies/BombEnemy"));
                        break;
                    case >= 80 and <= 99:
                        int randomVariantS = RandomiseNumber(3);
                        switch (randomVariantS)
                        {
                            case 0:
                                spawnQueue.Add(Resources.Load<GameObject>("Enemies/Normal Enemies/StalkerBlock"));
                                break;
                            case 1:
                                spawnQueue.Add(Resources.Load<GameObject>("Enemies/Normal Enemies/StalkerInvisible")); 
                                break;
                            default:
                                spawnQueue.Add(Resources.Load<GameObject>("Enemies/Normal Enemies/StalkerBlock"));
                                break;
                        }
                        break;
                    default:
                        spawnQueue.Add(Resources.Load<GameObject>("Enemies/Normal Enemies/RobotEnemy"));
                        break;
                }
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
