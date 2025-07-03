using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;
using DG.Tweening;

public class HealthBehaviour : MonoBehaviour
{
    public Vector3 launchPower;
    private Rigidbody _rigidbody;
    private GameObject _player;
    public bool moveToPlayer;
    public bool collected;
    private SpriteRenderer _spriteRenderer;
    public float healthAmount;
    private BoxCollider _boxCollider;
    private Light _light;
    private TrailRenderer _trailRenderer;
    private CharacterAttack _characterAttack;
    [Range(0, 3)] 
    public int randomEnergySize;
    public enum HealthSize { Small, Medium, Large, VeryLarge }
    public HealthSize healthSize;
    [SerializeField] private DataHolder dataHolder;
    private Sequence _flashSeq;
    private Color _defaultColor;
    private void Awake()
    {
        launchPower = new Vector3(Random.Range(-10f, 10f), Random.Range(0, 20f), 0);
        randomEnergySize = Random.Range(0, 100);
        _spriteRenderer = GetComponentInChildren<SpriteRenderer>(); 
        _defaultColor = _spriteRenderer.color;
        _light = gameObject.GetComponentInChildren<Light>();
        _trailRenderer = GetComponent<TrailRenderer>();
        
        switch (randomEnergySize)
        {
            case < 25:
                healthSize = HealthSize.Small;
                //_spriteRenderer.color = new Color(0.43137254902f, 0.30196078431f, 0.14509803921f);
                healthAmount = 50f;
                break;
            case < 50:
                healthSize = HealthSize.Medium;
                //_spriteRenderer.color = new Color(0.64705882352f, 0.66274509803f, 0.70588235294f);
                healthAmount = 60f;
                break;
            case < 80:
                healthSize = HealthSize.Large;
                healthAmount = 75f;
                break;
            case < 100:
                healthSize = HealthSize.VeryLarge;
                //_spriteRenderer.color = new Color(0.22352941176f, 0.50588235294f, 0.44705882352f);
                healthAmount = 100f;
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
        
        if (collected == false)
        {
            if (dataHolder.playerHealth < dataHolder.playerMaxHealth)
            {
                _rigidbody.AddForce(launchPower, ForceMode.Impulse);
                _rigidbody.velocity = Vector3.zero;
                _rigidbody.isKinematic = true;
                _boxCollider.enabled = true;
                _rigidbody.useGravity = false;
                moveToPlayer = true;
                StartCoroutine(TeleportToPlayer());
            }
            else
            {
                StartCoroutine(WaitToDestroy());
            }
        }
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

    private IEnumerator WaitToDestroy()
    {
        yield return new WaitForSeconds(3f);
        
        _flashSeq = DOTween.Sequence();

        _flashSeq.Append(
            _light.DOColor(Color.white, .3f).SetEase(Ease.InOutSine)
        );
        _flashSeq.Append(
            _light.DOColor(_defaultColor, .3f).SetEase(Ease.InOutSine)
        );

        _flashSeq.SetLoops(-1, LoopType.Restart);
        _flashSeq.Play();
        
        yield return new WaitForSeconds(5f);
        
        _flashSeq.Kill();
        Destroy(gameObject);
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
            _characterAttack.TakeDamagePlayer((int)-healthAmount, 0, Vector3.zero);
            //_rigidbody.velocity = Vector3.zero;
            collected = true;
            _spriteRenderer.enabled = false;
            _boxCollider.enabled = false;
            _light.enabled = false;
            moveToPlayer = false;
            Destroy(gameObject);
        }
    }
}

