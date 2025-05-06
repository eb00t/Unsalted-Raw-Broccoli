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
    private DoorHide _doorHide;
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
        _doorHide = GetComponentInChildren<DoorHide>();
        roomScripting = transform.root.GetComponent<RoomScripting>();
        _waves = RandomiseNumber(100);
        for (int i = 0; i < _waveCount; i++)
        {
            roomScripting._enemyCount++;
        }
        switch (_waves)
        {
            case < 5: // 5% chance to not spawn
                _waveCount = 0;
                break;
            case < 60: // 55% chance to spawn 1
                _waveCount = 1;
                break;
            case < 95: // 35 % chance to spawn 2
                _waveCount = 2;
                break;
            case < 100: // 5% chance to spawn 3
                _waveCount = 3;
                break;
            default:
                _waveCount = 1;
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
                _doorHide.OpenDoor();
                StartCoroutine(WaitForDoor());
            }
        }
        else
        {
            _doorHide.CloseDoor();
            DisableSpawner();
            Debug.Log("Wave count is 0");
        }
    }

    private IEnumerator WaitForDoor()
    {
        yield return new WaitUntil(() => _doorHide.isDoorOpen);
        GameObject enemyToSpawn = spawnQueue[0];
        enemyToSpawn = Instantiate(enemyToSpawn, transform.position, Quaternion.identity);
        enemyToSpawn.GetComponent<IDamageable>().RoomScripting = roomScripting;
        enemyToSpawn.GetComponent<IDamageable>().EnemySpawner = this;
        enemyToSpawn.transform.parent = gameObject.transform;
        spawnedEnemy = enemyToSpawn;
        spawnedEnemies.Add(enemyToSpawn);
        spawnQueue.Remove(spawnQueue[0]);
        _doorHide.CloseDoor();
        
        if (spawnQueue.Count == 0)
        {
            DisableSpawner();
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
