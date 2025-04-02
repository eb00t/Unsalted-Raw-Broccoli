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
    private SpriteRenderer _spriteRenderer;
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
                _spriteRenderer.sprite = Resources.Load<Sprite>("PUT THE SMALL CURRENCY SPRITE HERE");
                break;
            case >= 50 and <= 75:
                currencySize = CurrencySize.Medium;
                _spriteRenderer.sprite = Resources.Load<Sprite>("PUT THE MEDIUM CURRENCY SPRITE HERE");
                break;
            case >= 75 and <= 99:
                currencySize = CurrencySize.Large;
                _spriteRenderer.sprite = Resources.Load<Sprite>("PUT THE LARGE CURRENCY SPRITE HERE");
                break;
            case 100:
                currencySize = CurrencySize.VeryLarge;
                _spriteRenderer.sprite = Resources.Load<Sprite>("PUT THE VERY CURRENCY SPRITE HERE");
                break;
        }
    }

    private void Start()
    {
        _player = GameObject.FindWithTag("Player");
        _rigidbody.AddForce(launchPower, ForceMode.Impulse);
        StartCoroutine(GoToPlayer());
    }

    IEnumerator GoToPlayer()
    {
        yield return new WaitForSeconds(1.5f);
        _moveToPlayer = true;
        StartCoroutine(TeleportToPlayer());
    }

    IEnumerator TeleportToPlayer()
    {
        yield return new WaitForSeconds(5f);
        _rigidbody.velocity = Vector3.zero;
        transform.position = _player.transform.position;
        Debug.Log("Currency not collected; warping it to player");
    }
    
    private void Update()
    {
        if (_moveToPlayer)
        {
            transform.position = Vector3.Lerp(transform.position, _player.transform.position, 2 * Time.deltaTime);
        }
    }
}

