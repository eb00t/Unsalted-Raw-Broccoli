using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DropletSpawner : MonoBehaviour
{
    [SerializeField] private GameObject dropletPrefab;
    [SerializeField] private float spawnDelay;
    [SerializeField] private bool multipleDropletsAllowed;
    private float _timer;
    public bool canSpawn = true;

    private void Update()
    {
        _timer -= Time.deltaTime;

        if (_timer <= 0f)
        {
            if (multipleDropletsAllowed)
            {
                SpawnDroplet();
                _timer = spawnDelay;
            }
            else if (!multipleDropletsAllowed && canSpawn)
            {
                canSpawn = false;
                _timer = spawnDelay;
                SpawnDroplet();
            }
        }
    }

    private void SpawnDroplet()
    {
        var droplet = Instantiate(dropletPrefab, transform.position, Quaternion.identity, transform);
        droplet.SetActive(true);
    }
}
