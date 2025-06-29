using System;
using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class DialogueControllerScript : MonoBehaviour
{
    public bool replayable;
    public DialogueObjectHandler dialogueToLoad;
    public DialogueObjectHandler closeDialogue;
    public LoreItemHandler loreToLoad;
    private ItemPickupHandler _itemPickupHandler;
    [SerializeField] private GameObject shopCanvas;
    public bool hasBeforeAfterDialogue, isEndText, isShop;
    [SerializeField] private float range;
    [SerializeField] public int dialogueID;
    private GameObject _player, _dialogueCanvas, _uiManager;
    private MenuHandler _menuHandler;
    public bool randomLore; // Usually true, false if the lore is spawned directly.
    [SerializeField] private bool dontShowInteract;
    public bool inRange;
    
    public enum DialogueOrLore
    {
        Dialogue,
        Lore
    }
    public DialogueOrLore dialogueOrLore;

    //DIALOGUE CODE
    private TextMeshProUGUI dialogueText;
    private TextMeshProUGUI speakerText;
    private string _title;
    [SerializeField] private int _index; // = 0;

    public float dialogueSpeed;

    //public float fasterSpeed;


    private void Awake()
    {
       /* if (_dialogueObjectHandler == null)
        {
            Debug.LogError(gameObject.name + " is missing a dialogueObjectHandler");
        }*/
    }

    private void Start()
    {
        _player = GameObject.FindGameObjectWithTag("Player");
        _itemPickupHandler = _player.GetComponent<ItemPickupHandler>();
        _uiManager = GameObject.FindGameObjectWithTag("UIManager");
        _menuHandler = _uiManager.GetComponent<MenuHandler>();
        _dialogueCanvas = _menuHandler.dialogueGUI;
        int loreChoice = Random.Range(0, LoreReference.Instance.allLoreItems.Count);
        /* foreach (var text in _dialogueCanvas.GetComponentsInChildren<TextMeshProUGUI>())
         {
             switch (text.name)
             {
                 case "Normal Text":
                     dialogueText = text;
                     break;
                 case "SpeakerText":
                     speakerText = text;
                     break;
             }
         }*/

        switch (dialogueOrLore)
        {
            case DialogueOrLore.Lore:
                if (randomLore)
                {
                    loreToLoad = LoreReference.Instance.allLoreItems[loreChoice];
                }
                break;
        }
            //Start writing sentences
            //startSentence();
        
    }

    private void Update()
    {
        var dist = Vector3.Distance(transform.position, _player.transform.position);

        if (_dialogueCanvas.activeSelf && (dontShowInteract || inRange))
        {
            _itemPickupHandler.TogglePrompt("", false, ControlsManager.ButtonType.ProgressDialogue, "", null, false);
        }

        if (!inRange && dist <= range && !dontShowInteract)
        {
            inRange = true;

            if (!hasBeforeAfterDialogue && !_dialogueCanvas.activeSelf)
            {
                _itemPickupHandler.TogglePrompt("Interact", true, ControlsManager.ButtonType.Interact, "", null, false);
                
                _menuHandler.dialogueController = this;
            }
            else if (isShop && hasBeforeAfterDialogue && shopCanvas.activeSelf)
            {
                _itemPickupHandler.TogglePrompt("Close Shop", true, ControlsManager.ButtonType.Back, "", null, false);
                _menuHandler.dialogueController = this;
            }
            else if (!_dialogueCanvas.activeSelf)
            {
                _itemPickupHandler.TogglePrompt("Interact", true, ControlsManager.ButtonType.Interact, "", null, false);
                _menuHandler.dialogueController = this;
            }
            else
            {
                _itemPickupHandler.TogglePrompt("", false, ControlsManager.ButtonType.Interact, "", null, false);
                _menuHandler.dialogueController = this;
            }
        }
        else if (inRange && dist > range)
        {
            inRange = false;
            if (_menuHandler.dialogueController != this) return;
            
            _itemPickupHandler.TogglePrompt("", false, ControlsManager.ButtonType.Interact, "", null, false);
        }
    }

    public void LoadDialogue(DialogueObjectHandler dialogueHandler)
   {
       dialogueToLoad = dialogueHandler;
       
       if (hasBeforeAfterDialogue && isEndText)
       {
           DialogueHandler.Instance.LoadDialogueScriptableObject(closeDialogue);
       }
       else
       {
           DialogueHandler.Instance.LoadDialogueScriptableObject(dialogueHandler);
       }
       
       DialogueHandler.Instance.StartSentence(this);
       if (replayable == false)
       {
           DialogueHandler.Instance.trigger = transform.gameObject;
       }
       DialogueHandler.Instance.currentNPC = gameObject.GetComponent<NPCHandler>();

   }

    public void LoadLore(LoreItemHandler loreItem)
    {
        DialogueHandler.Instance.LoadLoreScriptableObject(loreItem);
        DialogueHandler.Instance.StartSentence(this);
        if (replayable == false)
        {
            DialogueHandler.Instance.trigger = transform.parent.gameObject;
        }
    }
}


