using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class CloneBossManager : MonoBehaviour
{
    [Header("Stats")]
    private int _collectiveHealth;
    private int _maxHealth;
    [SerializeField] private int maxNumberOfBosses;
    [SerializeField] private int maxAttackingCount;
    [SerializeField] private int individualHealth;
    [SerializeField] private float spawnCooldown;
    [SerializeField] private float individualRetreatDist;
    [SerializeField] private float atkCheckCooldown;
    
    [Header("Properties")]
    public List<CloneBossHandler> cloneBossHandlers;
    private bool _lowHealth;
    private float _targetTime;
    private bool _isPlayerInRange;
    private int _attackingCount;
    private bool _isChecking;
    
    [Header("References")]
    [SerializeField] private Canvas canvas;
    [SerializeField] private Slider healthSlider;
    [SerializeField] private Transform spawnPoint1, spawnPoint2, spawnPoint3;
    [SerializeField] private Transform parent;
    [SerializeField] private GameObject bossPrefab;
    private RoomScripting _roomScripting;
    private GameObject _dialogueGui;
    private GameObject _player;
    private CharacterAttack _characterAttack;

    private void Start()
    {
        _roomScripting = gameObject.transform.root.GetComponent<RoomScripting>();
        _roomScripting.enemies.Add(gameObject);
        _player = GameObject.FindGameObjectWithTag("Player");
        _characterAttack = _player.GetComponentInChildren<CharacterAttack>();
        _dialogueGui = GameObject.FindGameObjectWithTag("UIManager").GetComponent<MenuHandler>().dialogueGUI;
        InstantiateBoss(6);
        _maxHealth = maxNumberOfBosses * individualHealth;
        healthSlider.maxValue = _maxHealth;
        UpdateCollectiveHealth();
    }

    private void Update()
    {
        if (_dialogueGui.activeSelf)
        {
            _targetTime = 0;
            return;
        }

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
        _targetTime = spawnCooldown;
    }

    private void InstantiateBoss(int numberToSpawn)
    {
        for (var i = 0; i < numberToSpawn; i++)
        {
            var ran = Random.Range(0, 3);
            var spawn = Vector3.zero;
        
            switch (ran)
            {
                case 0:
                    spawn = spawnPoint1.position;
                    break;
                case 1:
                    spawn = spawnPoint2.position;
                    break;
                case 2:
                    spawn = spawnPoint3.position;
                    break;
            }
            
            var newBoss = Instantiate(bossPrefab, spawn, Quaternion.identity, parent);
            var handler = newBoss.GetComponent<CloneBossHandler>();
            handler.maxHealth = individualHealth;
            handler.health = individualHealth;
            handler.cloneBossManager = this;
            handler.dialogue = _dialogueGui;
            newBoss.SetActive(true);

            cloneBossHandlers.Add(handler);
            UpdateCollectiveHealth();
        }

        spawnCooldown += 0.25f;
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

        if (_collectiveHealth <= 0 || cloneBossHandlers.Count == 0)
        {
            LevelBuilder.Instance.bossDead = true;
            _roomScripting.enemies.Remove(gameObject);
            var currencyToDrop = Random.Range(5, 20);
            for (var i = 0; i < currencyToDrop; i++)
            {
                Instantiate(Resources.Load<GameObject>("ItemPrefabs/Other/Currency Prefab"), transform.position, Quaternion.identity);
            }
            _characterAttack.ChanceHeal();
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