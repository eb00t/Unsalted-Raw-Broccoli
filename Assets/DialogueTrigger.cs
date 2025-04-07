using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    public dialogueControllerScript dialogueControllerScript;
    public MenuHandler menuHandler;
    public bool reusable;

    void OnTriggerEnter(Collider other)
    {
        menuHandler.dialogueController = dialogueControllerScript;
        menuHandler.TriggerDialogue();
        if (reusable == false)
        {
            gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        if (dialogueControllerScript.transform.GetComponent<NPCHandler>().spokenToAlready)
        {
            gameObject.SetActive(false);
        }
    }
}
