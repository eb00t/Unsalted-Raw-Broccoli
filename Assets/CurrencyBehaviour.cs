using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class CurrencyBehaviour : MonoBehaviour
{
    public Vector3 launchPower;
    private Rigidbody _rigidbody;
    private GameObject _player;
    private bool _moveToPlayer;
    private bool _collected;
    private SpriteRenderer _spriteRenderer;
    private int _currencyAmount;
    private CurrencyManager _currencyManager;
    private BoxCollider _boxCollider;
    private Light _light;
    [Range(0, 3)]
    private int _randomCurrencySize;
    public enum CurrencySize
    {
        Small,
        Medium,
        Large,
        VeryLarge,
    }
    
    public CurrencySize currencySize;
    void Awake()
    {
        launchPower = new Vector3(Random.Range(-10f, 10f), Random.Range(0, 20f), 0);
        _rigidbody = GetComponent<Rigidbody>();
        _randomCurrencySize = Random.Range(0, 100);
        _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        switch (_randomCurrencySize)
        {
            case <= 49:
                currencySize = CurrencySize.Small;
                _currencyAmount = 1;
                //_spriteRenderer.sprite = Resources.Load<Sprite>("PUT THE SMALL CURRENCY SPRITE HERE");
                break;
            case >= 50 and <= 75:
                currencySize = CurrencySize.Medium;
                _currencyAmount = 2;
                //_spriteRenderer.sprite = Resources.Load<Sprite>("PUT THE MEDIUM CURRENCY SPRITE HERE");
                break;
            case >= 75 and < 99:
                currencySize = CurrencySize.Large;
                _currencyAmount = 5;
                //_spriteRenderer.sprite = Resources.Load<Sprite>("PUT THE LARGE CURRENCY SPRITE HERE");
                break;
            case 99:
                currencySize = CurrencySize.VeryLarge;
                _currencyAmount = 20;
               //_spriteRenderer.sprite = Resources.Load<Sprite>("PUT THE VERY LARGE CURRENCY SPRITE HERE");
                break;
        }
    }

    private void Start()
    {
        _currencyManager = GameObject.FindWithTag("UIManager").GetComponent<CurrencyManager>();
        _player = GameObject.FindWithTag("Player");
        _light = gameObject.GetComponentInChildren<Light>();
        _boxCollider = GetComponent<BoxCollider>();
        _boxCollider.enabled = true;
        _rigidbody.AddForce(launchPower, ForceMode.Impulse);
        StartCoroutine(GoToPlayer());
    }

    IEnumerator GoToPlayer()
    {
        yield return new WaitForSeconds(1.5f);
        _moveToPlayer = true;
        _boxCollider.enabled = true;
        _rigidbody.velocity = Vector3.zero;
        _rigidbody.useGravity = false;
        StartCoroutine(TeleportToPlayer());
    }

    IEnumerator TeleportToPlayer()
    {
        yield return new WaitForSeconds(5f);
        if (_collected == false)
        {
            GetComponent<TrailRenderer>().Clear();
            _rigidbody.velocity = Vector3.zero;
            transform.position = _player.transform.position;
            Debug.Log("Currency not collected; warping it to player");
        }
        
    }
    
    private void Update()
    {
        if (_moveToPlayer)
        {
            transform.position = Vector3.MoveTowards(transform.position, _player.transform.position, 20f * Time.deltaTime);
        }

        if (_collected)
        {
            _rigidbody.isKinematic = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _currencyManager.UpdateCurrency(_currencyAmount);
            AudioManager.Instance.PlayOneShot(FMODEvents.Instance.CurrencyPickup, _player.transform.position);
            _collected = true;
            _spriteRenderer.enabled = false;
            _boxCollider.enabled = false;
            _light.enabled = false;
            _rigidbody.velocity = Vector3.zero;
            _moveToPlayer = false;
            StartCoroutine(WaitToDestroy());
        }
    }

    IEnumerator WaitToDestroy()
    {
        yield return new WaitForSeconds(5f);
        Destroy(gameObject);
    }
    
}

