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

   [field: Header("Lists")]
   public List<DialogueObjectHandler> allDialogueObjects;
   public List<LoreItemHandler> allLoreItems;
   public List<LoreItemHandler> allViewedLoreItems;
   
   public bool eraseViewedLore; // Bool to allow all lore to respawn
   public int index; // The currently displayed message + speaker combo
   public DialogueObjectHandler currentDialogueObject; // The scriptable object that has been loaded
   public LoreItemHandler currentLoreItem; // The lore that has been loaded
   public float dialogueSpeed;
   
   [field: Header("References")]
   private TextMeshProUGUI _dialogueText; //Dialogue text object
   private TextMeshProUGUI _speakerText; // Speaker text object
   private GameObject _player, _dialogueCanvas, _uiManager;
   private MenuHandler _menuHandler;
   public GameObject trigger;
   public NPCHandler currentNPC;

   [field: Header("Objects to Load")] public string loadedTitleText; // The title (only used in lore)
   public List<string> loadedBodyText; // All the messages that need to displayed
   public List<string> loadedSpeakerText; // All the speakers that need to be displayed (this should always be the same size as the loadedBodyText string)

   private ItemPickupHandler _itemPickupHandler;

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
      
      if (eraseViewedLore)
      {
         foreach (var lore in allLoreItems)
         {
            lore.discoveredByPlayer = false;
         }
         eraseViewedLore = false;
      }
      
      foreach (var lore in allLoreItems.ToList())
      {
         if (lore.discoveredByPlayer)
         {
            allViewedLoreItems.Add(lore);
            allLoreItems.Remove(lore);
         }
      }
      
      _speakerText = _dialogueCanvas.transform.Find("Text box").transform.Find("SpeakerHolder").transform.Find("SpeakerText").GetComponent<TextMeshProUGUI>();
      _dialogueText = _dialogueCanvas.transform.Find("Text box").transform.Find("Normal Text").GetComponent<TextMeshProUGUI>();
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
            _dialogueCanvas.SetActive(false);
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
         }
      }
      else
      {
         StopAllCoroutines();
         _dialogueText.text = loadedBodyText[index];
         _speakerText.text = loadedSpeakerText[index];
      }
   }

   public void StartSentence()
   {
      index = 0;
      //_speakerText.text = loadedSpeakerText[index];
      //_dialogueText.text = loadedBodyText[index];
      StartCoroutine(TypeSentence());
   }

   IEnumerator TypeSentence()
   {
      _speakerText.text = loadedSpeakerText[index];
      foreach (char Character in loadedBodyText[index].ToCharArray())
      {
         _dialogueText.text += Character;
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
         _speakerText.transform.parent.gameObject.SetActive(false);
         _speakerText.gameObject.SetActive(false);
      }
   }
   
   public void LoadLoreScriptableObject(int scriptableObjectID)
   {
      if (currentDialogueObject != null)
      {
         loadedSpeakerText.Clear();
         loadedBodyText.Clear();
         currentDialogueObject = null;
      }
      allLoreItems[scriptableObjectID].discoveredByPlayer = true;
      currentLoreItem = allLoreItems[scriptableObjectID];
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
         _speakerText.transform.parent.gameObject.SetActive(false);
         _speakerText.gameObject.SetActive(false);
      }
   }
}
   

