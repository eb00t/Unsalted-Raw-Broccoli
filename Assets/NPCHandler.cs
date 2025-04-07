using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class NPCHandler : MonoBehaviour
{
    public enum SpawnPointLogic
    {
        SetSpawn,
        RandomSpawn,
    }

    public SpawnPointLogic spawnPointLogic;

    public enum WhoToSpawn // IDK how many of these we're going to have...
    {
        Nobody,
        Specto,
        DoctorStats,
    }

    public WhoToSpawn whoToSpawn;
    public bool spokenToAlready;

    private SpriteRenderer _spriteRenderer;
    private dialogueControllerScript _dialogueController;

    public DialogueObjectHandler
        dialogue1,
        dialogue2,
        dialogue3,
        dialogue4; // 1 = first time, 2 = upon first floor clear, 3 = upon second floor clear, 4 = upon blank's death

    public DialogueObjectHandler 
        dialogue1Repeat, 
        dialogue2Repeat, 
        dialogue3Repeat, 
        dialogue4Repeat;
    
    public DataHolder dataHolder;

    void Start() //TODO: Get random spawn working
    {
        _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        _dialogueController = GetComponent<dialogueControllerScript>();
        SpawnNPC();
    }

    void LoadSpecificNPCDialogue()
    {
        switch (dataHolder.highestFloorCleared)
        {
            case 0:
                _dialogueController.LoadDialogue(dialogue1);
                break;
            case 1:
                _dialogueController.LoadDialogue(dialogue2);
                break;
            case 2:
                _dialogueController.LoadDialogue(dialogue3);
                break;
            case 3:
                _dialogueController.LoadDialogue(dialogue4);
                break;
        }
    }

    void SpawnNPC()
    {
        switch (whoToSpawn)
        {
            case WhoToSpawn.Nobody:
                gameObject.SetActive(false);
                break;
            case WhoToSpawn.Specto:
                dialogue1 = DialogueReference.Instance.TutorialIntro;
                dialogue1Repeat = DialogueReference.Instance.TutorialIntroRepeat;
                dialogue2 = DialogueReference.Instance.TutorialFloor1;
                dialogue2Repeat = DialogueReference.Instance.TutorialFloor1Repeat;
                dialogue3 = DialogueReference.Instance.TutorialFloor2;
                dialogue3Repeat = DialogueReference.Instance.TutorialFloor2Repeat;
                dialogue4 = DialogueReference.Instance.TutorialFloor3;
                dialogue4Repeat = DialogueReference.Instance.TutorialFloor3Repeat;
                break;
            case WhoToSpawn.DoctorStats:
                dialogue1 = DialogueReference.Instance.Stats;
                dialogue1Repeat = DialogueReference.Instance.StatsRepeat;
                dialogue2 = DialogueReference.Instance.Stats;
                dialogue2Repeat = DialogueReference.Instance.StatsRepeat;
                dialogue3 = DialogueReference.Instance.Stats;
                dialogue3Repeat = DialogueReference.Instance.StatsRepeat;
                dialogue4 = DialogueReference.Instance.Stats;
                dialogue4Repeat = DialogueReference.Instance.StatsRepeat;
                break;
                
        }
        if (whoToSpawn != WhoToSpawn.Nobody)
        {
            LoadSpecificNPCDialogue();
        }
    }

    public void SwitchOutDialogue()
    {
        switch (whoToSpawn)
        {
            case WhoToSpawn.Specto:
                dialogue1 = dialogue1Repeat;
                dialogue2 = dialogue2Repeat;
                dialogue3 = dialogue3Repeat;
                dialogue4 = dialogue4Repeat;
                break;
            case WhoToSpawn.DoctorStats:
                dialogue1 = dialogue1Repeat;
                dialogue2 = dialogue1Repeat;
                dialogue3 = dialogue1Repeat;
                dialogue4 = dialogue4Repeat;
                break;
        } 
        
        if (whoToSpawn != WhoToSpawn.Nobody)
        {
            LoadSpecificNPCDialogue();
        }
    }
    

}
