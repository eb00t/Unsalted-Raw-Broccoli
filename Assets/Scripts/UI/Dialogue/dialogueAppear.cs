using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class dialogueAppear : MonoBehaviour
{
    public GameObject dialogueBox, dialogueConA, dialogueConB, enemyChar; // yesText, noText, npcChar;
    public GameObject npcChar, bossIndicator, indicator;
    private ItemPickupHandler _itemPickupHandler;
    private CharacterMovement _characterMovement;
    [SerializeField] private float range;

    //public bool enemyNear = false;
   // public bool npcNear = false;

    private void Start()
    { 
        _characterMovement = GetComponent<CharacterMovement>();
        _itemPickupHandler = GetComponent<ItemPickupHandler>();
       // npcChar = GameObject.Find("NPC");
       npcChar = GameObject.Find("NPC");
    }

    //Text appear
    public void EnableDialogueBox(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        if (_characterMovement.uiOpen) return;

        if (GetComponent<ItemPickupHandler>().isPlrNearDialogue || GetComponent<ItemPickupHandler>().isPlrNearDialogue1)
        {
            dialogueBox.SetActive(true);
        }
    }

    // Update is called once per frame
    private void Update()
    {
        if (!_characterMovement.uiOpen)
        {
            var dist = Vector3.Distance(transform.position, enemyChar.transform.position);
            //var dist1 = Vector3.Distance(transform.position, npcChar.transform.position);

            if (dist <= range)
            {
                dialogueConA.SetActive(true);
                indicator.SetActive(true);
                _itemPickupHandler.isPlrNearDialogue = true;

                if (dialogueBox.activeSelf)
                {
                    _itemPickupHandler.TogglePrompt("Next sentence", true, ControlsManager.ButtonType.ButtonSouth);
                }
                else
                {
                    _itemPickupHandler.TogglePrompt("Interact", true, ControlsManager.ButtonType.ButtonEast);
                }
            }
            else if (dist > range)
            {
                dialogueConA.SetActive(false);
                indicator.SetActive(false);
                _itemPickupHandler.isPlrNearDialogue = false;
            }
            
            /*
            if (dist1 <= range)
            {
                dialogueConB.SetActive(true);
                _itemPickupHandler.isPlrNearDialogue1 = true;
                _itemPickupHandler.TogglePrompt("Interact", true, ControlsManager.ButtonType.ButtonEast);
            }
            else if (dist1 > range)
            {
                dialogueConB.SetActive(false);
                _itemPickupHandler.isPlrNearDialogue1 = false;
            }
            */
        }

        
        /*
        //Activate Prompt
        if(playerNear)
        {
            indicator.SetActive(true);
        }
        else
        {
            indicator.SetActive(false);
        }
        */
    }
  
    /*
    private void OnTriggerEnter(Collider collider)
    {
        if(collider.gameObject.tag == "Enemy")
        {
            playerNear = true;
        }
    }

    private void OnTriggerExit(Collider collider)
    {
        if (collider.gameObject.tag == "Enemy")
        {
            playerNear = false;
        }
    }
    */
}
