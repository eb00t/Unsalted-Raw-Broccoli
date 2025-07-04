using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class CloneBossManager : MonoBehaviour
{
    [Header("Stats")]
    private int _collectiveHealth;
    private int _maxHealth;
    [SerializeField] private int maxConcurrentIndividuals;
    [SerializeField] private int totalSpawn;
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
    private bool _hasDialogueTriggered;
    private int _numSpawned;
    
    [Header("References")]
    [SerializeField] private Canvas canvas;
    [SerializeField] private Slider healthSlider;
    [SerializeField] private Transform spawnPoint1, spawnPoint2, spawnPoint3;
    [SerializeField] private Transform parent;
    [SerializeField] private GameObject bossPrefab;
    private RoomScripting _roomScripting;
    private GameObject _dialogueGui;
    private DialogueTrigger[] _dialogueTriggers;
    private GameObject _player;
    private CharacterAttack _characterAttack;
    public int numKilled;
    [SerializeField] private Animator spawn1Animator, spawn2Animator, spawn3Animator;
    [SerializeField] private Slider healthChangeSlider;
    [SerializeField] private Image healthChangeImage;
    private Tween _healthTween;
    [SerializeField] private DataHolder dataHolder;
    private int _spawnIndex;

    private void Start()
    {
        if (dataHolder.hardcoreMode)
        {
            totalSpawn += totalSpawn / 2;
        }

        _roomScripting = gameObject.transform.root.GetComponent<RoomScripting>();
        _roomScripting.enemies.Add(gameObject);
        _player = GameObject.FindGameObjectWithTag("Player");
        _characterAttack = _player.GetComponentInChildren<CharacterAttack>();
        _dialogueGui = GameObject.FindGameObjectWithTag("UIManager").GetComponent<MenuHandler>().dialogueGUI;
        _dialogueTriggers = gameObject.transform.root.GetComponentsInChildren<DialogueTrigger>();
        InstantiateBoss(3);
        _maxHealth = totalSpawn * individualHealth;
        healthSlider.value = _maxHealth;
        healthSlider.maxValue = _maxHealth;
        healthChangeSlider.maxValue = _maxHealth;
        healthChangeSlider.value = _maxHealth;
        UpdateCollectiveHealth();
    }

    private void Update()
    {
        if (!_hasDialogueTriggered)
        {
            foreach (var trigger in _dialogueTriggers)
            {
                if (trigger.triggered && _dialogueGui.activeSelf)
                {
                    _hasDialogueTriggered = true;
                    break;
                }
            }
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

        if (cloneBossHandlers.Count >= maxConcurrentIndividuals) return;

        _targetTime -= Time.deltaTime;
        if (!(_targetTime <= 0.0f)) return;
        
        InstantiateBoss(1);
        _targetTime = spawnCooldown;
    }

    private void InstantiateBoss(int numberToSpawn)
    {
        if (_numSpawned >= totalSpawn)
        {
            return;
        }

        for (var i = 0; i < numberToSpawn; i++)
        {
            if (_spawnIndex > 2) _spawnIndex = 0;
            var spawn = Vector3.zero;
        
            switch (_spawnIndex)
            {
                case 0:
                    spawn = spawnPoint1.position;
                    spawn1Animator.SetTrigger("OpenDoor");
                    StartCoroutine(WaitForDoor(spawn1Animator.gameObject.GetComponent<DoorHide>()));
                    break;
                case 1:
                    spawn = spawnPoint2.position;
                    spawn2Animator.SetTrigger("OpenDoor");
                    StartCoroutine(WaitForDoor(spawn2Animator.gameObject.GetComponent<DoorHide>()));
                    break;
                case 2:
                    spawn = spawnPoint3.position;
                    spawn3Animator.SetTrigger("OpenDoor");
                    StartCoroutine(WaitForDoor(spawn3Animator.gameObject.GetComponent<DoorHide>()));
                    break;
            }
            
            var newBoss = Instantiate(bossPrefab, spawn, Quaternion.identity, parent);
            var handler = newBoss.GetComponent<CloneBossHandler>();
            handler.maxHealth = individualHealth;
            handler.health = individualHealth;
            handler.cloneBossManager = this;
            handler.dialogue = _dialogueGui;
            newBoss.SetActive(true);
            handler._hasDialogueTriggered = _hasDialogueTriggered;

            cloneBossHandlers.Add(handler);
            _numSpawned++;
            //Debug.Log(_numSpawned);
            //UpdateCollectiveHealth();
            _spawnIndex++;
        }

        //spawnCooldown += 0.3f;
    }

    private IEnumerator WaitForDoor(DoorHide doorHide)
    {
        yield return new WaitUntil(() => doorHide.isDoorOpen);
        doorHide.CloseDoor();
    }

    public void UpdateCollectiveHealth()
    {
        var previousHealth = _collectiveHealth;
        _collectiveHealth = (totalSpawn - numKilled) * individualHealth;
        //healthSlider.value = _collectiveHealth;
        
        var isDamaged = _collectiveHealth < previousHealth;
        var changeColor = isDamaged ? new Color(1f, 0.9f, 0.4f) : new Color(.5f, 1f, 0.4f);
        
        _healthTween?.Kill();
        healthChangeImage.color = changeColor;

        if (isDamaged)
        {
            healthSlider.value = _collectiveHealth;
            _healthTween = DOVirtual.Float(healthChangeSlider.value, _collectiveHealth, 1f, v => healthChangeSlider.value = v).SetEase(Ease.OutExpo).SetDelay(0.3f);
        }
        else
        {
            healthChangeSlider.value = _collectiveHealth;
            _healthTween = DOVirtual.Float(healthSlider.value, _collectiveHealth, 1f, v => healthSlider.value = v).SetEase(Ease.OutExpo).SetDelay(0.3f);
        }

        if (numKilled >= totalSpawn / 2)
        {
            AudioManager.Instance.SetMusicParameter("Boss Phase", 1);
        }

        if (_numSpawned >= totalSpawn && numKilled == totalSpawn && cloneBossHandlers.Count <= 0)
        {
            LevelBuilder.Instance.bossDead = true;
            AudioManager.Instance.SetMusicParameter("Boss Phase", 3);
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