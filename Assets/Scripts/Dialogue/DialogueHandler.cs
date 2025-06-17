using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class DialogueHandler : MonoBehaviour
{
   public static DialogueHandler Instance { get; private set; }

   public List<LoreItemHandler> allViewedLoreItems;
   public int index; // The currently displayed message + speaker combo
   public DialogueObjectHandler currentDialogueObject; // The scriptable object that has been loaded
   public LoreItemHandler currentLoreItem; // The lore that has been loaded
   public float dialogueSpeed;
   
   [field: Header("References")]
   public TextMeshProUGUI _dialogueText; //Dialogue text object
   public TextMeshProUGUI _speakerText; // Speaker text object
   private GameObject _player, _dialogueCanvas, _uiManager;
   private MenuHandler _menuHandler;
   public GameObject trigger;
   public NPCHandler currentNPC;
   public DataHolder dataHolder;

   [field: Header("Objects to Load")] public string loadedTitleText; // The title (only used in lore)
   public List<string> loadedBodyText; // All the messages that need to displayed
   public List<string> loadedSpeakerText; // All the speakers that need to be displayed (this should always be the same size as the loadedBodyText string)

   private ItemPickupHandler _itemPickupHandler;
   private dialogueControllerScript _dialogueController;

   public bool flipped;

   private void Awake()
   {
      if (Instance != null)
      {
         Debug.LogError("More than one DialogueHandler script in the scene.");
      }

      Instance = this;
   }

   private void Start()
   {
      _player = GameObject.FindGameObjectWithTag("Player");
      _itemPickupHandler = _player.GetComponent<ItemPickupHandler>();
      _dialogueCanvas = GameObject.FindWithTag("Dialogue");
      _player.GetComponent<ItemPickupHandler>();
      _uiManager = GameObject.FindGameObjectWithTag("UIManager");
      _menuHandler = _uiManager.GetComponent<MenuHandler>();
      _speakerText = _dialogueCanvas.transform.Find("Text box").transform.Find("SpeakerHolder").transform.Find("SpeakerText").GetComponent<TextMeshProUGUI>();
      _dialogueText = _dialogueCanvas.transform.Find("Text box").transform.Find("Normal Text").GetComponent<TextMeshProUGUI>();
      
      if (dataHolder.eraseViewedLore)
      {
         foreach (var lore in LoreReference.Instance.allLoreItems)
         {
            lore.discoveredByPlayer = false;
         }
      }
      
      foreach (var lore in LoreReference.Instance.allLoreItems.ToList())
      {
         if (lore.discoveredByPlayer)
         {
            allViewedLoreItems.Add(lore);
            LoreReference.Instance.allLoreItems.Remove(lore);
         }
      }
      
   }

   public void NextSentence(InputAction.CallbackContext context) // when space/A is pressed
   {
      if (!context.performed) return;
      if (!_dialogueCanvas.activeSelf) return;

      if (_dialogueText.text == loadedBodyText[index])
      {
         // nextSen() moved here
         if (index < loadedBodyText.Count - 1)
         {
            index++;
            _dialogueText.text = string.Empty;
            
            if (loadedSpeakerText[index] == null)
            {
               loadedSpeakerText[index] = loadedSpeakerText[index - 1];
            }
            if (loadedBodyText[index] == null)
            {
               loadedBodyText[index] = loadedBodyText[index - 1];
            }

            StartCoroutine(TypeSentence());
         }
         else
         {
            index = 0;
            loadedBodyText.Clear();
            loadedSpeakerText.Clear();
            _speakerText.text = "";
            _dialogueText.text = "";
            _menuHandler.SetDialogueActive(false);

            if (_dialogueController.isShop && !_dialogueController.isEndText)
            {
               _menuHandler.ToggleShop();
            }
            else if (_dialogueController.isEndText)
            {
               _dialogueController.isEndText = false;
            }

            if (currentNPC != null)
            {
               currentNPC.spokenToAlready = true;
               currentNPC.SwitchOutDialogue();
            }

            if (trigger != null)
            {
               trigger.SetActive(false);
               _itemPickupHandler.isPlrNearDialogue = false;
            }

            if (currentLoreItem != null)
            {
               currentLoreItem.discoveredByPlayer = true;
               
            }

            if (flipped) // Checks if the player has been turned by an NPC and returns them to normal.
            {
               flipped = false;
               _player.GetComponentInChildren<SpriteRenderer>().flipX = false;
            }
         }
      }
      else
      {
         StopAllCoroutines();
         _dialogueText.text = loadedBodyText[index];
         _speakerText.text = loadedSpeakerText[index];
      }
   }
   
   public void StartSentence(dialogueControllerScript dialogueController)
   {
      if (currentNPC != null)
      {
         if ((_player.transform.position.x > currentNPC.transform.position.x && _player.transform.localScale.x > 0) ||_player.transform.position.x < currentNPC.transform.position.x &&
             _player.transform.localScale.x < 0) // Flips the player if they are facing away from an NPC
         {
            _player.GetComponentInChildren<SpriteRenderer>().flipX = true;
            flipped = true;
         }
      }
      else
      {
         _player.GetComponentInChildren<SpriteRenderer>().flipX = false;
      }
      if (_speakerText != null && _dialogueText != null && dialogueController != null)
      {
         _dialogueController = dialogueController;
         index = 0;
         _speakerText.text = "";
         _dialogueText.text = "";
         StartCoroutine(TypeSentence());
      }
   }

   IEnumerator TypeSentence()
   {
      _speakerText.text = loadedSpeakerText[index];
      int letterCount = 0;
      foreach (char Character in loadedBodyText[index].ToCharArray())
      {
         _dialogueText.text += Character;
         if (_dialogueText.gameObject.activeSelf)
         {
            if (letterCount >= 5)
            {
               AudioManager.Instance.PlayOneShot(FMODEvents.Instance.DialogueScroll, transform.position);
               letterCount = 0;
            }
            letterCount++;
         }
         
         yield return new WaitForSeconds(dialogueSpeed);
      }
   }

   public void LoadDialogueScriptableObject(DialogueObjectHandler dialogueObject)
   {
      if (currentLoreItem != null)
      {
         loadedSpeakerText.Clear();
         loadedBodyText.Clear();
         currentLoreItem = null;
      }
      
      currentDialogueObject = dialogueObject;
      loadedSpeakerText = new List<string>(currentDialogueObject.whoIsSpeaking);
      loadedBodyText = new List<string>(currentDialogueObject.dialogueBodyText);
      if (currentDialogueObject.isAnyoneSpeaking == false)
      {
         _menuHandler.SetDialogueActive(false);
      }
   }
   
   public void LoadLoreScriptableObject(LoreItemHandler loreItem)
   {
      if (currentDialogueObject != null)
      {
         loadedSpeakerText.Clear();
         loadedBodyText.Clear();
         currentDialogueObject = null;
      }
      currentLoreItem = loreItem;
      loadedTitleText = currentLoreItem.loreTitle;
      loadedSpeakerText = new List<string>(currentLoreItem.whoWroteThis);
      loadedBodyText = new List<string>(currentLoreItem.loreBodyText);
      if (currentLoreItem.doesThisHaveATitle)
      {
         loadedBodyText.Insert(0, loadedTitleText);
         loadedSpeakerText.Insert(0, loadedSpeakerText[0]);
         loadedBodyText = new List<string>(loadedBodyText);
         loadedSpeakerText = new List<string>(loadedSpeakerText);
      }
      if (currentLoreItem.didAnyoneWriteThis == false)
      {
         _menuHandler.SetDialogueActive(false);
      }
   }
}
   

