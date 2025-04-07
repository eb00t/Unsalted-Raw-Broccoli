using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    public dialogueControllerScript dialogueControllerScript;
    public MenuHandler menuHandler;
    public bool triggered, reusable;
    

    private void Start()
    {
        menuHandler = GameObject.FindWithTag("UIManager").GetComponent<MenuHandler>();
        //if (dialogueControllerScript == null || menuHandler == null)
        //{
        //    gameObject.SetActive(false);
        //}
    }

    void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player") && BlackoutManager.Instance.blackoutComplete)
        {
            menuHandler.dialogueController = dialogueControllerScript;
            menuHandler.TriggerDialogue();
            triggered = true;
            if (reusable == false && triggered)
            {
                gameObject.SetActive(false);
            }
        }
    }

    private void Update()
    {
        //if (dialogueControllerScript.transform.GetComponent<NPCHandler>() != null && dialogueControllerScript.transform.GetComponent<NPCHandler>().spokenToAlready)
        //{
        //    gameObject.SetActive(false);
        //}
    }
}
