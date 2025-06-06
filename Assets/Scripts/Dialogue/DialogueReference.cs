using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueReference : MonoBehaviour
{ 
    public static DialogueReference Instance { get; private set; }

    [field: Header("Initial Dialogue")]
    [field: SerializeField] public DialogueObjectHandler Floor1Intro { get; private set; }
    [field: SerializeField] public DialogueObjectHandler Floor1Boss { get; private set; }
    [field: SerializeField] public DialogueObjectHandler Floor2Intro { get; private set; }
    [field: SerializeField] public DialogueObjectHandler Floor2Boss { get; private set; }
    [field: SerializeField] public DialogueObjectHandler Floor3Intro { get; private set; }
    [field: SerializeField] public DialogueObjectHandler Floor3Boss { get; private set; }
    [field: SerializeField] public DialogueObjectHandler Floor4Intro { get; private set; }
    [field: SerializeField] public DialogueObjectHandler Floor4Boss { get; private set; }
    [field: SerializeField] public DialogueObjectHandler Floor5Intro { get; private set; }
    [field: SerializeField] public DialogueObjectHandler Floor5Boss { get; private set; }
    [field: SerializeField] public DialogueObjectHandler EndNormal { get; private set; }
    [field: SerializeField] public DialogueObjectHandler EndHard { get; private set; }
    [field: Header("Tutorial NPC")]
    [field: SerializeField] public DialogueObjectHandler TutorialIntro { get; private set; }
    [field: SerializeField] public DialogueObjectHandler TutorialFloor1 { get; private set; }
    [field: SerializeField] public DialogueObjectHandler TutorialFloor2 { get; private set; }
    [field: SerializeField] public DialogueObjectHandler TutorialFloor3 { get; private set; }
    [field: SerializeField] public DialogueObjectHandler TutorialFloor4 { get; private set; }
    
    [field: Header("Stats NPC")]
    [field: SerializeField] public DialogueObjectHandler Stats { get; private set; }
    
    [field: Header("Rich NPC")]
    [field: SerializeField] public DialogueObjectHandler RichIntro { get; private set; }
    [field: SerializeField] public DialogueObjectHandler RichFloor1 { get; private set; }
    [field: SerializeField] public DialogueObjectHandler RichFloor2 { get; private set; }
    [field: SerializeField] public DialogueObjectHandler RichFloor3 { get; private set; }
    [field: SerializeField] public DialogueObjectHandler RichFloor4 { get; private set; }
    
    [field: Header("Kid NPC")]
    [field: SerializeField] public DialogueObjectHandler KidIntro { get; private set; }
    [field: SerializeField] public DialogueObjectHandler KidFloor1  { get; private set; }
    [field: SerializeField] public DialogueObjectHandler KidFloor2  { get; private set; }
    [field: SerializeField] public DialogueObjectHandler KidFloor3  { get; private set; }
    [field: SerializeField] public DialogueObjectHandler KidFloor4 { get; private set; }
    
    [field: Header("Punk NPC")]
    [field: SerializeField] public DialogueObjectHandler PunkIntro { get; private set; }
    [field: SerializeField] public DialogueObjectHandler PunkFloor1 { get; private set; }
    [field: SerializeField] public DialogueObjectHandler PunkFloor2 { get; private set; }
    [field: SerializeField] public DialogueObjectHandler PunkFloor3 { get; private set; }
    [field: SerializeField] public DialogueObjectHandler PunkFloor4 { get; private set; }
    
    [field: Header("Shopkeep NPC")]
    [field: SerializeField] public DialogueObjectHandler ShopkeepIntro { get; private set; }
    [field: SerializeField] public DialogueObjectHandler ShopkeepFloor1 { get; private set; }
    [field: SerializeField] public DialogueObjectHandler ShopkeepFloor2 { get; private set; }
    [field: SerializeField] public DialogueObjectHandler ShopkeepFloor3 { get; private set; }
    [field: SerializeField] public DialogueObjectHandler ShopkeepFloor4 { get; private set; }

    [field: Header("Repeat Dialogue")]
    [field: Header("Tutorial NPC")]
    [field: SerializeField] public DialogueObjectHandler TutorialIntroRepeat { get; private set; }
    [field: SerializeField]  public DialogueObjectHandler TutorialFloor1Repeat { get; private set; }
    [field: SerializeField] public DialogueObjectHandler TutorialFloor2Repeat { get; private set; }
    [field: SerializeField] public DialogueObjectHandler TutorialFloor3Repeat { get; private set; }
    [field: SerializeField] public DialogueObjectHandler TutorialFloor4Repeat { get; private set; }
    [field: Header("Stats NPC")]
    [field: SerializeField] public DialogueObjectHandler StatsRepeat { get; private set; }
    
    [field: Header("Rich NPC")]
    [field: SerializeField] public DialogueObjectHandler RichIntroRepeat { get; private set; }
    [field: SerializeField] public DialogueObjectHandler RichFloor1Repeat { get; private set; }
    [field: SerializeField] public DialogueObjectHandler RichFloor2Repeat { get; private set; }
    [field: SerializeField] public DialogueObjectHandler RichFloor3Repeat { get; private set; }
    [field: SerializeField] public DialogueObjectHandler RichFloor4Repeat { get; private set; }
    
    [field: Header("Kid NPC")]
    [field: SerializeField] public DialogueObjectHandler KidIntroRepeat  { get; private set; }
    [field: SerializeField] public DialogueObjectHandler KidFloor1Repeat { get; private set; }
    [field: SerializeField] public DialogueObjectHandler KidFloor2Repeat { get; private set; }
    [field: SerializeField] public DialogueObjectHandler KidFloor3Repeat { get; private set; }
    [field: SerializeField] public DialogueObjectHandler KidFloor4Repeat { get; private set; }
    
    [field: Header("Punk NPC")]
    [field: SerializeField] public DialogueObjectHandler PunkIntroRepeat { get; private set; }
    [field: SerializeField] public DialogueObjectHandler PunkFloor1Repeat { get; private set; }
    [field: SerializeField] public DialogueObjectHandler PunkFloor2Repeat { get; private set; }
    [field: SerializeField] public DialogueObjectHandler PunkFloor3Repeat { get; private set; }
    [field: SerializeField] public DialogueObjectHandler PunkFloor4Repeat { get; private set; }
    
    [field: Header("Shopkeep NPC")]
    [field: SerializeField] public DialogueObjectHandler ShopkeepIntroRepeat { get; private set; }
    [field: SerializeField] public DialogueObjectHandler ShopkeepFloor1Repeat { get; private set; }
    [field: SerializeField] public DialogueObjectHandler ShopkeepFloor2Repeat { get; private set; }
    [field: SerializeField] public DialogueObjectHandler ShopkeepFloor3Repeat { get; private set; }
    [field: SerializeField] public DialogueObjectHandler ShopkeepFloor4Repeat { get; private set; }
    
    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("More than one DialogueReference script in the scene.");
        }

        Instance = this;
        
        //DIALOGUE REFERENCES (FLOOR SPECIFIC)
        Floor1Intro = Resources.Load<DialogueObjectHandler>("Dialogue/English/Floor 1/Floor 1 Intro");
        Floor1Boss = Resources.Load<DialogueObjectHandler>("Dialogue/English/Floor 1/Floor 1 Boss");
        Floor2Intro = Resources.Load<DialogueObjectHandler>("Dialogue/English/Floor 2/Floor 2 Intro");
        Floor2Boss = Resources.Load<DialogueObjectHandler>("Dialogue/English/Floor 2/Floor 2 Boss");
        Floor3Intro = Resources.Load<DialogueObjectHandler>("Dialogue/English/Floor 3/Floor 3 Intro");
        Floor3Boss = Resources.Load<DialogueObjectHandler>("Dialogue/English/Floor 3/Floor 3 Boss");
        Floor4Intro = Resources.Load<DialogueObjectHandler>("Dialogue/English/Floor 4/Floor 4 Intro");
        Floor4Boss = Resources.Load<DialogueObjectHandler>("Dialogue/English/Floor 4/Floor 4 Boss");
        Floor5Intro = Resources.Load<DialogueObjectHandler>("Dialogue/English/Final Boss/Final Boss Intro");
        Floor5Boss = Resources.Load<DialogueObjectHandler>("Dialogue/English/Final Boss/Final Boss Boss");
        EndNormal = Resources.Load<DialogueObjectHandler>("Dialogue/English/Ending/End Normal");
        EndHard = Resources.Load<DialogueObjectHandler>("Dialogue/English/Ending/End Hard");

        //DIALOGUE REFERENCES (TUTORIAL NPC)
        TutorialIntro = Resources.Load<DialogueObjectHandler>("Dialogue/English/Intermission/Guide/Tutorial Intro");
        TutorialIntroRepeat = Resources.Load<DialogueObjectHandler>("Dialogue/English/Intermission/Guide/Tutorial Intro Repeat");
        TutorialFloor1 = Resources.Load<DialogueObjectHandler>("Dialogue/English/Intermission/Guide/Tutorial Floor 1");
        TutorialFloor1Repeat = Resources.Load<DialogueObjectHandler>("Dialogue/English/Intermission/Guide/Tutorial Floor 1 Repeat");
        TutorialFloor2 = Resources.Load<DialogueObjectHandler>("Dialogue/English/Intermission/Guide/Tutorial Floor 2");
        TutorialFloor2Repeat = Resources.Load<DialogueObjectHandler>("Dialogue/English/Intermission/Guide/Tutorial Floor 2 Repeat");
        TutorialFloor3 = Resources.Load<DialogueObjectHandler>("Dialogue/English/Intermission/Guide/Tutorial Floor 3");
        TutorialFloor3Repeat = Resources.Load<DialogueObjectHandler>("Dialogue/English/Intermission/Guide/Tutorial Floor 3 Repeat");
        TutorialFloor4 = Resources.Load<DialogueObjectHandler>("Dialogue/English/Intermission/Guide/Tutorial Floor 4");
        TutorialFloor4Repeat = Resources.Load<DialogueObjectHandler>("Dialogue/English/Intermission/Guide/Tutorial Floor 4 Repeat");
        
        //DIALOGUE REFERENCES (RICH NPC)
        RichIntro = Resources.Load<DialogueObjectHandler>("Dialogue/English/Intermission/Richard Bullion III/Rich Intro");
        RichIntroRepeat = Resources.Load<DialogueObjectHandler>("Dialogue/English/Intermission/Richard Bullion III/Rich Intro Repeat");
        RichFloor1 = Resources.Load<DialogueObjectHandler>("Dialogue/English/Intermission/Richard Bullion III/Rich Floor 1");
        RichFloor1Repeat = Resources.Load<DialogueObjectHandler>("Dialogue/English/Intermission/Richard Bullion III/Rich Floor 1 Repeat");
        RichFloor2 = Resources.Load<DialogueObjectHandler>("Dialogue/English/Intermission/Richard Bullion III/Rich Floor 2");
        RichFloor2Repeat = Resources.Load<DialogueObjectHandler>("Dialogue/English/Intermission/Richard Bullion III/Rich Floor 2 Repeat");
        RichFloor3 = Resources.Load<DialogueObjectHandler>("Dialogue/English/Intermission/Richard Bullion III/Rich Floor 3");
        RichFloor3Repeat = Resources.Load<DialogueObjectHandler>("Dialogue/English/Intermission/Richard Bullion III/Rich Floor 3 Repeat");
        RichFloor4 = Resources.Load<DialogueObjectHandler>("Dialogue/English/Intermission/Richard Bullion III/Rich Floor 4");
        RichFloor4Repeat = Resources.Load<DialogueObjectHandler>("Dialogue/English/Intermission/Richard Bullion III/Rich Floor 4 Repeat");
        
        //DIALOGUE REFERENCES (KID NPC)
        KidIntro = Resources.Load<DialogueObjectHandler>("Dialogue/English/Intermission/Kid/Kid Intro");
        KidIntroRepeat = Resources.Load<DialogueObjectHandler>("Dialogue/English/Intermission/Kid/Kid Intro Repeat");
        KidFloor1 = Resources.Load<DialogueObjectHandler>("Dialogue/English/Intermission/Kid/Kid Floor 1");
        KidFloor1Repeat = Resources.Load<DialogueObjectHandler>("Dialogue/English/Intermission/Kid/Kid Floor 1 Repeat");
        KidFloor2 = Resources.Load<DialogueObjectHandler>("Dialogue/English/Intermission/Kid/Kid Floor 2");
        KidFloor2Repeat = Resources.Load<DialogueObjectHandler>("Dialogue/English/Intermission/Kid/Kid Floor 2 Repeat");
        KidFloor3 = Resources.Load<DialogueObjectHandler>("Dialogue/English/Intermission/Kid/Kid Floor 3");
        KidFloor3Repeat = Resources.Load<DialogueObjectHandler>("Dialogue/English/Intermission/Kid/Kid Floor 3 Repeat");
        KidFloor4 = Resources.Load<DialogueObjectHandler>("Dialogue/English/Intermission/Kid/Kid Floor 4");
        KidFloor4Repeat = Resources.Load<DialogueObjectHandler>("Dialogue/English/Intermission/Kid/Kid Floor 4");

        //DIALOGUE REFERENCES (PUNK NPC)
        PunkIntro = Resources.Load<DialogueObjectHandler>("Dialogue/English/Intermission/Punk/Punk Intro");
        PunkIntroRepeat = Resources.Load<DialogueObjectHandler>("Dialogue/English/Intermission/Punk/Punk Intro Repeat");
        PunkFloor1 = Resources.Load<DialogueObjectHandler>("Dialogue/English/Intermission/Punk/Punk Floor 1");
        PunkFloor1Repeat = Resources.Load<DialogueObjectHandler>("Dialogue/English/Intermission/Punk/Punk Floor 1 Repeat");
        PunkFloor2 = Resources.Load<DialogueObjectHandler>("Dialogue/English/Intermission/Punk/Punk Floor 2");
        PunkFloor2Repeat = Resources.Load<DialogueObjectHandler>("Dialogue/English/Intermission/Punk/Punk Floor 2 Repeat");
        PunkFloor3 = Resources.Load<DialogueObjectHandler>("Dialogue/English/Intermission/Punk/Punk Floor 3");
        PunkFloor3Repeat = Resources.Load<DialogueObjectHandler>("Dialogue/English/Intermission/Punk/Punk Floor 3 Repeat");
        PunkFloor4 = Resources.Load<DialogueObjectHandler>("Dialogue/English/Intermission/Punk/Punk Floor 4");
        PunkFloor4Repeat = Resources.Load<DialogueObjectHandler>("Dialogue/English/Intermission/Punk/Punk Floor 4");


        //DIALOGUE REFERENCES (SHOPKEEPER NPC)
        ShopkeepIntro = Resources.Load<DialogueObjectHandler>("Dialogue/English/Intermission/Shopkeep/Shopkeep Intro");
        ShopkeepIntroRepeat = Resources.Load<DialogueObjectHandler>("Dialogue/English/Intermission/Shopkeep/Shopkeep Intro Repeat");
        ShopkeepFloor1 = Resources.Load<DialogueObjectHandler>("Dialogue/English/Intermission/Shopkeep/Shopkeep Floor 1");
        ShopkeepFloor1Repeat = Resources.Load<DialogueObjectHandler>("Dialogue/English/Intermission/Shopkeep/Shopkeep Floor 1 Repeat");
        ShopkeepFloor2 = Resources.Load<DialogueObjectHandler>("Dialogue/English/Intermission/Shopkeep/Shopkeep Floor 2");
        ShopkeepFloor2Repeat = Resources.Load<DialogueObjectHandler>("Dialogue/English/Intermission/Shopkeep/Shopkeep Floor 2 Repeat");
        ShopkeepFloor3 = Resources.Load<DialogueObjectHandler>("Dialogue/English/Intermission/Shopkeep/Shopkeep Floor 3");
        ShopkeepFloor3Repeat = Resources.Load<DialogueObjectHandler>("Dialogue/English/Intermission/Shopkeep/Shopkeep Floor 3 Repeat");
        ShopkeepFloor4 = Resources.Load<DialogueObjectHandler>("Dialogue/English/Intermission/Shopkeep/Shopkeep Floor 4");
        ShopkeepFloor4Repeat = Resources.Load<DialogueObjectHandler>("Dialogue/English/Intermission/Shopkeep/Shopkeep Floor 4");


    }
}
