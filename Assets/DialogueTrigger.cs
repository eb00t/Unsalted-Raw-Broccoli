using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    public dialogueControllerScript dialogueControllerScript;
    public MenuHandler menuHandler;
    public bool reusable;

    private void Awake()
    {
        if (dialogueControllerScript == null || menuHandler == null)
        {
            gameObject.SetActive(false);
        }
    }

    private void Start()
    {
        menuHandler = GameObject.FindWithTag("UIManager").GetComponent<MenuHandler>();
    }

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
