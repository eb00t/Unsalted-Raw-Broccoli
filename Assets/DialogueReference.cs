using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueReference : MonoBehaviour
{ 
    public static DialogueReference Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("More than one DialogueReference script in the scene.");
        }

        Instance = this;
    }

    [field: Header("Initial Dialogue")]
    [field: Header("Tutorial NPC")]
    [field: SerializeField] public DialogueObjectHandler Floor1Intro { get; private set; }
    [field: SerializeField] public DialogueObjectHandler Floor1Boss { get; private set; }
    [field: SerializeField] public DialogueObjectHandler Floor2Intro { get; private set; }
    [field: SerializeField] public DialogueObjectHandler Floor2Boss { get; private set; }
    [field: SerializeField] public DialogueObjectHandler Floor3Intro { get; private set; }
    [field: SerializeField] public DialogueObjectHandler Floor3Boss { get; private set; }
    [field: SerializeField] public DialogueObjectHandler TutorialIntro { get; private set; }
    [field: SerializeField] public DialogueObjectHandler TutorialFloor1 { get; private set; }
    [field: SerializeField] public DialogueObjectHandler TutorialFloor2 { get; private set; }
    [field: SerializeField] public DialogueObjectHandler TutorialFloor3 { get; private set; }
    [field: Header("Stats NPC")]
    [field: SerializeField] public DialogueObjectHandler Stats { get; private set; }
    //[field: SerializeField] public DialogueObjectHandler Floor1Intro {get; private set;}
    //[field: SerializeField] public DialogueObjectHandler Floor1Intro {get; private set;}
    [field: Header("Rich NPC")]
    [field: SerializeField] public DialogueObjectHandler RichIntro { get; private set; }
    [field: SerializeField] public DialogueObjectHandler RichFloor1 { get; private set; }
    [field: SerializeField] public DialogueObjectHandler RichFloor2 { get; private set; }
    [field: SerializeField] public DialogueObjectHandler RichFloor3 { get; private set; }
    
    [field: Header("Shopkeep NPC")]
    [field: SerializeField] public DialogueObjectHandler ShopkeepIntro { get; private set; }
    [field: SerializeField] public DialogueObjectHandler ShopkeepFloor1 { get; private set; }
    [field: SerializeField] public DialogueObjectHandler ShopkeepFloor2 { get; private set; }
    [field: SerializeField] public DialogueObjectHandler ShopkeepFloor3 { get; private set; }

    [field: Header("Repeat Dialogue")]
    [field: Header("Tutorial NPC")]
    [field: SerializeField] public DialogueObjectHandler TutorialIntroRepeat { get; private set; }
    [field: SerializeField]  public DialogueObjectHandler TutorialFloor1Repeat { get; private set; }
    [field: SerializeField] public DialogueObjectHandler TutorialFloor2Repeat { get; private set; }
    [field: SerializeField] public DialogueObjectHandler TutorialFloor3Repeat { get; private set; }
    [field: Header("Stats NPC")]
    [field: SerializeField] public DialogueObjectHandler StatsRepeat { get; private set; }
    
    [field: Header("Rich NPC")]
    [field: SerializeField] public DialogueObjectHandler RichIntroRepeat { get; private set; }
    [field: SerializeField] public DialogueObjectHandler RichFloor1Repeat { get; private set; }
    [field: SerializeField] public DialogueObjectHandler RichFloor2Repeat { get; private set; }
    [field: SerializeField] public DialogueObjectHandler RichFloor3Repeat { get; private set; }
    
    [field: Header("Shopkeep NPC")]
    [field: SerializeField] public DialogueObjectHandler ShopkeepIntroRepeat { get; private set; }
    [field: SerializeField] public DialogueObjectHandler ShopkeepFloor1Repeat { get; private set; }
    [field: SerializeField] public DialogueObjectHandler ShopkeepFloor2Repeat { get; private set; }
    [field: SerializeField] public DialogueObjectHandler ShopkeepFloor3Repeat { get; private set; }
   
}
