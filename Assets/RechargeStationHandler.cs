using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class RechargeStationHandler : MonoBehaviour
{
    private GameObject _player, _uiManager;
    private ItemPickupHandler _itemPickupHandler;
    private MenuHandler _menuHandler;
    private int _energyStored;
    [SerializeField] private int minCost, maxCost;
    [SerializeField] private float range;
    [NonSerialized] public int cost;
    public bool hasBeenPurchased;
    [SerializeField] private Material disabledMaterial;

    private void Start()
    {
        cost = Random.Range(minCost, maxCost);
        _energyStored = (cost / 7) + (cost % 7);
        Debug.Log(_energyStored);
        _uiManager = GameObject.FindGameObjectWithTag("UIManager");
        _menuHandler = _uiManager.GetComponent<MenuHandler>();
        _player = GameObject.FindGameObjectWithTag("Player");
        _itemPickupHandler = _player.GetComponent<ItemPickupHandler>();
        GetComponent<PromptTrigger>().promptText = "Purchase energy refill for: " + cost;
    }

    public void InstantiateEnergy()
    {
        var instPos = new Vector3(_player.transform.position.x, _player.transform.position.y + 3f, _player.transform.position.z);
        for (var i = 0; i < _energyStored; i++)
        {
            Instantiate(Resources.Load<GameObject>("ItemPrefabs/Other/Energy Prefab"), instPos, Quaternion.identity);
        }
        hasBeenPurchased = true;
        GetComponentInChildren<SpriteRenderer>().material = disabledMaterial;
        GetComponentInChildren<Light>().enabled = false;
        GetComponent<PromptTrigger>().isDisabled = true;
    }
}
