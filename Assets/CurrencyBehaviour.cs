using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class CurrencyBehaviour : MonoBehaviour
{
    public Vector3 launchPower;
    public Rigidbody _rigidbody;
    public GameObject _player;
    public bool _moveToPlayer;
    public bool _collected;
    public SpriteRenderer _spriteRenderer;
    public int _currencyAmount;
    public CurrencyManager _currencyManager;
    public BoxCollider _boxCollider;
    public Light _light;
    public TrailRenderer _trailRenderer;
    [Range(0, 3)]
    
    public int _randomCurrencySize;
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
        _randomCurrencySize = Random.Range(0, 100);
        _spriteRenderer = GetComponentInChildren<SpriteRenderer>(); 
        _light = gameObject.GetComponentInChildren<Light>();
        _trailRenderer = GetComponent<TrailRenderer>();
        switch (_randomCurrencySize)
        {
            case <= 49:
                currencySize = CurrencySize.Small;
                _spriteRenderer.color = new Color(0.43137254902f, 0.30196078431f, 0.14509803921f);
                _currencyAmount = 1;
                //_spriteRenderer.sprite = Resources.Load<Sprite>("PUT THE SMALL CURRENCY SPRITE HERE");
                break;
            case >= 50 and <= 75:
                currencySize = CurrencySize.Medium;
                _spriteRenderer.color = new Color(0.64705882352f, 0.66274509803f, 0.70588235294f);
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
                _spriteRenderer.color = new Color(0.22352941176f, 0.50588235294f, 0.44705882352f);
                _currencyAmount = 20;
               //_spriteRenderer.sprite = Resources.Load<Sprite>("PUT THE VERY LARGE CURRENCY SPRITE HERE");
                break;
        }

        _light.color = _spriteRenderer.color;
        _trailRenderer.startColor = _spriteRenderer.color;
        _trailRenderer.endColor = _spriteRenderer.color;
    }

    private void Start()
    {
        _currencyManager = GameObject.FindWithTag("UIManager").GetComponent<CurrencyManager>();
        _player = GameObject.FindWithTag("Player");
        _boxCollider = GetComponent<BoxCollider>();
        _rigidbody = GetComponent<Rigidbody>();
        _boxCollider.enabled = true;
        _rigidbody.AddForce(launchPower, ForceMode.Impulse);
        StartCoroutine(GoToPlayer());
    }

    IEnumerator GoToPlayer()
    {
        yield return new WaitForSeconds(1.5f);
        _moveToPlayer = true;
        _boxCollider.enabled = true;
        //_rigidbody.velocity = Vector3.zero;
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
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _currencyManager.UpdateCurrency(_currencyAmount);
            AudioManager.Instance.PlayOneShot(FMODEvents.Instance.CurrencyPickup, _player.transform.position);
            //_rigidbody.velocity = Vector3.zero;
            _rigidbody.isKinematic = true;
            _collected = true;
            _spriteRenderer.enabled = false;
            _boxCollider.enabled = false;
            _light.enabled = false;
            _moveToPlayer = false;
            StopCoroutine(GoToPlayer());
            StopCoroutine(TeleportToPlayer());
            StartCoroutine(WaitToDestroy());
        }
    }

    IEnumerator WaitToDestroy()
    {
        yield return new WaitForSeconds(5f);
        Destroy(gameObject);
    }
    
}

