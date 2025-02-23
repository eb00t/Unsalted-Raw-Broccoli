using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    public RoomScripting _roomScripting;
    public List<GameObject> spawnQueue, possibleEnemies, spawnedEnemies;
    void Start()
    {
        _roomScripting = transform.root.GetComponent<RoomScripting>();
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
    }

    public void SpawnEnemies()
    {
        GameObject enemyToSpawn = spawnQueue[0];
        enemyToSpawn = Instantiate(enemyToSpawn, transform.position, Quaternion.identity);
        enemyToSpawn.transform.parent = gameObject.transform.root;
        spawnedEnemies.Add(enemyToSpawn);
        spawnQueue.Remove(spawnQueue[0]);
    }
    
    private int RandomiseNumber(int setSize)
    {
        int rng = Random.Range(0, setSize);
        return rng;
    }
}
