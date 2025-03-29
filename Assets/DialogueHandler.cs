using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

public class DialogueHandler : MonoBehaviour
{
   public static DialogueHandler Instance { get; private set; }

   public List<DialogueObjectHandler> allDialogueObjects;
   public List<LoreItemHandler> allLoreItems;
   public List<LoreItemHandler> allViewedLoreItems;
   private AllDialogue _allDialogue;
   public bool eraseViewedLore;

   [field: Header("Objects to Load")] public string loadedTitleText;
   public string[] loadedBodyText;
   public List<string> loadedSpeakerText;


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
   }


   public DialogueObjectHandler LoadDialogueScriptableObject(int scriptableObjectID)
   {
      return allDialogueObjects[scriptableObjectID];
   }
   
   public LoreItemHandler LoadLoreScriptableObject(int scriptableObjectID)
   {
      allLoreItems[scriptableObjectID].discoveredByPlayer = true;
      return allLoreItems[scriptableObjectID];
   }

}
   

