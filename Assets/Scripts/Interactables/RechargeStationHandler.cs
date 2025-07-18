using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
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
    public bool inRange;

    private void Start()
    {
        cost = Random.Range(minCost, maxCost);
        _energyStored = (cost / 4) + (cost % 4);
        Debug.Log(_energyStored);
        _uiManager = GameObject.FindGameObjectWithTag("UIManager");
        _menuHandler = _uiManager.GetComponent<MenuHandler>();
        _player = GameObject.FindGameObjectWithTag("Player");
        _itemPickupHandler = _player.GetComponent<ItemPickupHandler>();
    }

    private void Update()
    {
        if (hasBeenPurchased) return;
        
        var dist = Vector3.Distance(transform.position, _player.transform.position);
        
        if (!inRange && dist <= range)
        {
            _itemPickupHandler.TogglePrompt("Purchase energy refill for  <sprite index=0 color=#FFBD00>" + "<color=#FFBD00>"  + cost + "</color>", true, ControlsManager.ButtonType.Interact, "", null, false);
            _menuHandler.currencyCanvasGroup.DOFade(1f, 0.5f);
            _menuHandler.rechargeStationHandler = this;
            inRange = true;
        }
        else if (inRange && dist > range)
        {
            if (_menuHandler.rechargeStationHandler != this) return;
            
            _itemPickupHandler.TogglePrompt("", false, ControlsManager.ButtonType.Interact, "", null, false);
            _menuHandler.currencyCanvasGroup.DOFade(0f, 0.5f);
            inRange = false;
        }
    }

    public void InstantiateEnergy()
    {
        var instPos = new Vector3(_player.transform.position.x, _player.transform.position.y + 3f, _player.transform.position.z);
        for (var i = 0; i < _energyStored; i++)
        {
            Instantiate(Resources.Load<GameObject>("ItemPrefabs/Other/Energy Prefab"), instPos, Quaternion.identity);
        }
        
        _itemPickupHandler.TogglePrompt("", false, ControlsManager.ButtonType.Interact, "", null, false);
        hasBeenPurchased = true;
        GetComponentInChildren<SpriteRenderer>().material = disabledMaterial;
        GetComponentInChildren<Light>().enabled = false;
        _menuHandler.rechargeStationHandler = null;
    }
}
