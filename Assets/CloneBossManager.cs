using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CloneBossManager : MonoBehaviour
{
    public List<CloneBossHandler> cloneBossHandlers = new List<CloneBossHandler>();
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private Transform parent;
    [SerializeField] private int individualHealth;
    private int _collectiveHealth;
    private int _maxHealth;
    [SerializeField] private GameObject bossPrefab;
    [SerializeField] private int maxNumberOfBosses;
    [SerializeField] private Slider healthSlider;
    [SerializeField] private float spawnCooldown;
    private RoomScripting _roomScripting;
    private bool _lowHealth;
    private float _targetTime;
    private bool _isPlayerInRange;
    [SerializeField] private Canvas canvas;

    private void Start()
    {
        _roomScripting = gameObject.transform.root.GetComponent<RoomScripting>();
        _roomScripting.enemies.Add(gameObject);
        _maxHealth = maxNumberOfBosses * individualHealth;
        healthSlider.maxValue = _maxHealth;
        UpdateCollectiveHealth();
    }

    private void Update()
    {
        // implement spawning a new boss at a delay, the less bosses the faster it spawns them
        if (_isPlayerInRange && _collectiveHealth > 0)
        {
            canvas.gameObject.SetActive(true);
        }
        else
        {
            if (canvas.gameObject.activeSelf)
            {
                canvas.gameObject.SetActive(false);
            }

            return;
        }

        if (cloneBossHandlers.Count >= maxNumberOfBosses) return;

        _targetTime -= Time.deltaTime;
        if (!(_targetTime <= 0.0f)) return;
        
        InstantiateBoss(1);

        _targetTime = spawnCooldown * (cloneBossHandlers.Count / 2);
    }

    private void InstantiateBoss(int numberToSpawn)
    {
        for (var i = 0; i < numberToSpawn; i++)
        {
            var newBoss = Instantiate(bossPrefab, spawnPoint.position, Quaternion.identity, parent);
            var handler = newBoss.GetComponent<CloneBossHandler>();
            handler.maxHealth = individualHealth;
            handler.health = individualHealth;
            handler.cloneBossManager = this;
            newBoss.SetActive(true);

            cloneBossHandlers.Add(handler);
            UpdateCollectiveHealth();
        }
    }

    public void UpdateCollectiveHealth()
    {
        var healthCount = 0;
        healthSlider.maxValue = 0;
        
        foreach (var clone in cloneBossHandlers)
        {
            healthCount += clone.health;
            healthSlider.maxValue += individualHealth;
        }
        
        _collectiveHealth = healthCount;
        healthSlider.value = healthCount;

        if (_collectiveHealth <= 0)
        {
            LevelBuilder.Instance.bossDead = true;
            _roomScripting.enemies.Remove(gameObject);
            KillAllBosses();
        }
    }

    private void KillAllBosses()
    {
        foreach (var clone in cloneBossHandlers)
        {
            clone.Die();
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _isPlayerInRange = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _isPlayerInRange = false;
        }
    }
}