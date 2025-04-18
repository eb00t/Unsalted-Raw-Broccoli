using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PromptTrigger : MonoBehaviour
{
    public enum PromptType
    {
        Consumable,
        Dialogue,
        Lore,
        LevelTrigger,
        RechargeStation,
        Shop
    }

    public PromptType promptType;
    private GameObject _player;
    private ItemPickupHandler _itemPickupHandler;
    [SerializeField] private float triggerRange;
    public int priority;
    public string promptText;
    public ControlsManager.ButtonType button1;
    public ControlsManager.ButtonType button2;
    public bool doesOverrideOtherPrompts;
    public bool isDisabled;

    private void Start()
    {
        _player = GameObject.FindGameObjectWithTag("Player");
        _itemPickupHandler = _player.GetComponent<ItemPickupHandler>();
    }

    private void Update()
    {
        var dist = Vector3.Distance(transform.position, _player.transform.position);

        if (!isDisabled && dist <= triggerRange)
        {
            if (!_itemPickupHandler.promptTriggers.Contains(this))
            {
                _itemPickupHandler.promptTriggers.Add(this);
            }
        }
        else if (isDisabled || dist > triggerRange && _itemPickupHandler.promptTriggers.Contains(this))
        {
            _itemPickupHandler.promptTriggers.Remove(this);
        }
    }
}
