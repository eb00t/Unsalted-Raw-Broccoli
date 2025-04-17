using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class EnergyHandler : MonoBehaviour
{
    public Vector3 launchPower;
    private Rigidbody _rigidbody;
    private GameObject _player;
    public bool moveToPlayer;
    public bool collected;
    private SpriteRenderer _spriteRenderer;
    public float energyAmount;
    private BoxCollider _boxCollider;
    private Light _light;
    private TrailRenderer _trailRenderer;
    private CharacterAttack _characterAttack;
    [Range(0, 3)] 
    public int randomEnergySize;
    public enum EnergySize { Small, Medium, Large, VeryLarge, }
    public EnergySize energySize;
    private void Awake()
    {
        launchPower = new Vector3(Random.Range(-10f, 10f), Random.Range(0, 20f), 0);
        randomEnergySize = Random.Range(0, 100);
        _spriteRenderer = GetComponentInChildren<SpriteRenderer>(); 
        _light = gameObject.GetComponentInChildren<Light>();
        _trailRenderer = GetComponent<TrailRenderer>();
        
        switch (randomEnergySize)
        {
            case <= 49:
                energySize = EnergySize.Small;
                //_spriteRenderer.color = new Color(0.43137254902f, 0.30196078431f, 0.14509803921f);
                energyAmount = 1f;
                break;
            case <= 75:
                energySize = EnergySize.Medium;
                //_spriteRenderer.color = new Color(0.64705882352f, 0.66274509803f, 0.70588235294f);
                energyAmount = 2f;
                break;
            case < 99:
                energySize = EnergySize.Large;
                energyAmount = 4f;
                break;
            case 99:
                energySize = EnergySize.VeryLarge;
                //_spriteRenderer.color = new Color(0.22352941176f, 0.50588235294f, 0.44705882352f);
                energyAmount = 8f;
                break;
        }

        //_light.color = _spriteRenderer.color;
        //_trailRenderer.startColor = _spriteRenderer.color;
        //_trailRenderer.endColor = _spriteRenderer.color;
    }

    private void Start()
    {
        _player = GameObject.FindWithTag("Player");
        _characterAttack = _player.GetComponentInChildren<CharacterAttack>();
        _boxCollider = GetComponent<BoxCollider>();
        _rigidbody = GetComponent<Rigidbody>();
        _boxCollider.enabled = true;
        _rigidbody.AddForce(launchPower, ForceMode.Impulse);
        StartCoroutine(GoToPlayer());
    }

    private IEnumerator GoToPlayer()
    {
        yield return new WaitForSeconds(1.5f);
        moveToPlayer = true;
        _boxCollider.enabled = true;
        //_rigidbody.velocity = Vector3.zero;
        _rigidbody.useGravity = false;
        StartCoroutine(TeleportToPlayer());
    }

    private IEnumerator TeleportToPlayer()
    {
        yield return new WaitForSeconds(2f);
        if (collected == false)
        {
            GetComponent<TrailRenderer>().Clear();
            _rigidbody.velocity = Vector3.zero;
            transform.position = _player.transform.position;
        }
    }
    
    private void Update()
    {
        if (moveToPlayer)
        {
            transform.position = Vector3.MoveTowards(transform.position, _player.transform.position, 20f * Time.deltaTime);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            //AudioManager.Instance.PlayOneShot(FMODEvents.Instance.CurrencyPickup, _player.transform.position);
            _characterAttack.UseEnergy(-energyAmount);
            _rigidbody.isKinematic = true;
            collected = true;
            _spriteRenderer.enabled = false;
            _boxCollider.enabled = false;
            _light.enabled = false;
            moveToPlayer = false;
            StopCoroutine(GoToPlayer());
            StopCoroutine(TeleportToPlayer());
            StartCoroutine(WaitToDestroy());
        }
    }

    private IEnumerator WaitToDestroy()
    {
        yield return new WaitForSeconds(3f);
        Destroy(gameObject);
    }
}

