using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class DialogueTrigger : MonoBehaviour
{
    public dialogueControllerScript dialogueControllerScript;
    public MenuHandler menuHandler;
    public bool triggered, reusable;
    private GameObject _player;
    public bool hasDialogueOpened;
    public DataHolder dataHolder;
    private ItemPickupHandler _itemPickupHandler;

    private void Start()
    {
        menuHandler = GameObject.FindWithTag("UIManager").GetComponent<MenuHandler>();
        _player = GameObject.FindWithTag("Player");
        _itemPickupHandler = _player.GetComponent<ItemPickupHandler>();
        //if (dialogueControllerScript == null || menuHandler == null)
        //{
        //    gameObject.SetActive(false);
        //}
        if (dataHolder.highestFloorCleared > 0 && LevelBuilder.Instance.currentFloor == LevelBuilder.LevelMode.Intermission)
        {
            gameObject.SetActive(false);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player") && BlackoutManager.Instance.blackoutComplete && !triggered)
        {
            _player = other.gameObject;
            triggered = true;
        }
    }

    private void Update()
    {
        if (!triggered || _player == null || menuHandler.mapCamera.activeSelf) return;
        
        if (_player.GetComponent<CharacterMovement>().grounded && !hasDialogueOpened)
        {
            menuHandler.dialogueController = dialogueControllerScript;
            menuHandler.TriggerDialogue(false, dialogueControllerScript);
            
            foreach (var dt in gameObject.transform.root.GetComponentsInChildren<DialogueTrigger>())
            {
                if (dt != null && dt == this) continue;
                dt.hasDialogueOpened = true;
                dt.triggered = true;
            }
            
            hasDialogueOpened = true;
        }

        if (reusable == false && hasDialogueOpened)
        {
            gameObject.SetActive(false);
        }

        if (menuHandler.dialogueController.GetComponent<NPCHandler>())
        {
            if (menuHandler.dialogueController.GetComponent<NPCHandler>().spokenToAlready)
            {
                gameObject.SetActive(false);
            }
        }
        
        /*
        if (hasDialogueOpened)
        {
            _itemPickupHandler.TogglePrompt("Next", true, ControlsManager.ButtonType.ProgressDialogue, "", null);
        }
        */
    }
}
