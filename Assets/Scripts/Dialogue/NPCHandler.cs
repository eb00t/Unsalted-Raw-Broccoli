using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class NPCHandler : MonoBehaviour
{
    private static readonly int WhoAmI = Animator.StringToHash("whoAmI"); //0 = Old Guy, 1 = Hurt Guy, 2 = Kid, 3 = Punk, 4 = Shopkeep

    public enum SpawnPointLogic
    {
        SetSpawn,
        RandomSpawn,
    }

    public SpawnPointLogic spawnPointLogic;

    public enum WhoToSpawn // TODO: GIVE THESE PEOPLE NAMES!
    {
        Nobody,
        Specto,
        DoctorStats,
        RichardBullionIII,
        Kid,
        Punk,
        Shopkeep,
    }

    public WhoToSpawn whoToSpawn;
    public bool spokenToAlready;

    private SpriteRenderer _spriteRenderer;
    private Animator _animator;
    private dialogueControllerScript _dialogueController;
    public bool noted;

    public DialogueObjectHandler
        dialogue1,
        dialogue2,
        dialogue3,
        dialogue4,
        dialogue5; // 1 = first time, 2 = upon first floor clear, 3 = upon second floor clear, 4 = upon third floor clear, 5 = upon fourth floor clear

    public DialogueObjectHandler 
        dialogue1Repeat, 
        dialogue2Repeat, 
        dialogue3Repeat, 
        dialogue4Repeat,
        dialogue5Repeat;
    
    public DataHolder dataHolder;

    void Start() //TODO: Get random spawn working
    {
        if (dataHolder.highestFloorCleared == 4)
        {
            noted = true;
        }
        _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        _animator = GetComponentInChildren<Animator>();
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
            case 4:
                _dialogueController.LoadDialogue(dialogue5);
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
                if (noted)
                {
                    _spriteRenderer.sprite = Resources.Load<Sprite>("NPCs/Sprites/HURTNPC_");
                    _animator.enabled = false;
                }
                else
                {
                    _spriteRenderer.sprite = Resources.Load<Sprite>("NPCs/Sprites/HURTNPC_IDLE");
                    _animator.SetInteger(WhoAmI, 1);
                }

                dialogue1 = DialogueReference.Instance.TutorialIntro;
                dialogue1Repeat = DialogueReference.Instance.TutorialIntroRepeat;
                dialogue2 = DialogueReference.Instance.TutorialFloor1;
                dialogue2Repeat = DialogueReference.Instance.TutorialFloor1Repeat;
                dialogue3 = DialogueReference.Instance.TutorialFloor2;
                dialogue3Repeat = DialogueReference.Instance.TutorialFloor2Repeat;
                dialogue4 = DialogueReference.Instance.TutorialFloor3;
                dialogue4Repeat = DialogueReference.Instance.TutorialFloor3Repeat;
                dialogue5 = DialogueReference.Instance.TutorialFloor4;
                dialogue5Repeat = DialogueReference.Instance.TutorialFloor4Repeat;
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
                dialogue5 = DialogueReference.Instance.Stats;
                dialogue5Repeat = DialogueReference.Instance.StatsRepeat;
                break;
            case WhoToSpawn.RichardBullionIII:
                if (noted)
                {
                    _spriteRenderer.sprite = Resources.Load<Sprite>("NPCs/Sprites/OLDNPC_");
                    _animator.enabled = false;
                }
                else
                {
                    _spriteRenderer.sprite = Resources.Load<Sprite>("NPCs/Sprites/OLDNPC_IDLE");
                    _animator.SetInteger(WhoAmI, 0);
                }
                dialogue1 = DialogueReference.Instance.RichIntro;
                dialogue1Repeat = DialogueReference.Instance.RichIntroRepeat;
                dialogue2 = DialogueReference.Instance.RichFloor1;
                dialogue2Repeat = DialogueReference.Instance.RichFloor1Repeat;
                dialogue3 = DialogueReference.Instance.RichFloor2;
                dialogue3Repeat = DialogueReference.Instance.RichFloor2Repeat;
                dialogue4 = DialogueReference.Instance.RichFloor3;
                dialogue4Repeat = DialogueReference.Instance.RichFloor3Repeat;
                dialogue5 = DialogueReference.Instance.RichFloor4;
                dialogue5Repeat = DialogueReference.Instance.RichFloor4Repeat;
                break;
            case WhoToSpawn.Kid:
                if (noted)
                {
                    _spriteRenderer.sprite = Resources.Load<Sprite>("NPCs/Sprites/KIDNPC_");
                    _animator.enabled = false;
                }
                else
                {
                    _spriteRenderer.sprite = Resources.Load<Sprite>("NPCs/Sprites/KIDNPC_IDLE");
                    _animator.SetInteger(WhoAmI, 2);
                }
                dialogue1 = DialogueReference.Instance.KidIntro;
                dialogue1Repeat = DialogueReference.Instance.KidIntroRepeat;
                dialogue2 = DialogueReference.Instance.KidFloor1;
                dialogue2Repeat = DialogueReference.Instance.KidFloor1Repeat;
                dialogue3 = DialogueReference.Instance.KidFloor2;
                dialogue3Repeat = DialogueReference.Instance.KidFloor2Repeat;
                dialogue4 = DialogueReference.Instance.KidFloor3;
                dialogue4Repeat = DialogueReference.Instance.KidFloor3Repeat;
                dialogue5 = DialogueReference.Instance.KidFloor4;
                dialogue5Repeat = DialogueReference.Instance.KidFloor4Repeat;
                break;
            case WhoToSpawn.Punk:
                if (noted)
                {
                    _spriteRenderer.sprite = Resources.Load<Sprite>("NPCs/Sprites/PUNKNPC_");
                    _animator.enabled = false;
                }
                else
                {
                    _spriteRenderer.sprite = Resources.Load<Sprite>("NPCs/Sprites/PUNKNPC_IDLE");
                    _animator.SetInteger(WhoAmI, 3);
                }
                dialogue1 = DialogueReference.Instance.PunkIntro;
                dialogue1Repeat = DialogueReference.Instance.PunkIntroRepeat;
                dialogue2 = DialogueReference.Instance.PunkFloor1;
                dialogue2Repeat = DialogueReference.Instance.PunkFloor1Repeat;
                dialogue3 = DialogueReference.Instance.PunkFloor2;
                dialogue3Repeat = DialogueReference.Instance.PunkFloor2Repeat;
                dialogue4 = DialogueReference.Instance.PunkFloor3;
                dialogue4Repeat = DialogueReference.Instance.PunkFloor3Repeat;
                dialogue5 = DialogueReference.Instance.PunkFloor4;
                dialogue5Repeat = DialogueReference.Instance.PunkFloor4Repeat;
                break;
            case WhoToSpawn.Shopkeep:
                if (noted)
                {
                    _spriteRenderer.sprite = Resources.Load<Sprite>("NPCs/Sprites/NPCTUT");
                    _animator.enabled = false;
                }
                else
                {
                    _spriteRenderer.sprite = Resources.Load<Sprite>("NPCs/Sprites/NPCTUT_idle");
                    _animator.SetInteger(WhoAmI, 4);
                }
                dialogue1 = DialogueReference.Instance.ShopkeepIntro;
                dialogue1Repeat = DialogueReference.Instance.ShopkeepIntroRepeat;
                dialogue2 = DialogueReference.Instance.ShopkeepFloor1;
                dialogue2Repeat = DialogueReference.Instance.ShopkeepFloor1Repeat;
                dialogue3 = DialogueReference.Instance.ShopkeepFloor2;
                dialogue3Repeat = DialogueReference.Instance.ShopkeepFloor2Repeat;
                dialogue4 = DialogueReference.Instance.ShopkeepFloor3;
                dialogue4Repeat = DialogueReference.Instance.ShopkeepFloor3Repeat;
                dialogue5 = DialogueReference.Instance.ShopkeepFloor4;
                dialogue5Repeat = DialogueReference.Instance.ShopkeepFloor4Repeat;
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
            case WhoToSpawn.Specto or WhoToSpawn.RichardBullionIII or WhoToSpawn.Kid or WhoToSpawn.Punk or WhoToSpawn.Shopkeep:
                dialogue1 = dialogue1Repeat;
                dialogue2 = dialogue2Repeat;
                dialogue3 = dialogue3Repeat;
                dialogue4 = dialogue4Repeat;
                dialogue5 = dialogue5Repeat;
                break;
            case WhoToSpawn.DoctorStats:
                dialogue1 = dialogue1Repeat;
                dialogue2 = dialogue1Repeat;
                dialogue3 = dialogue1Repeat;
                dialogue4 = dialogue1Repeat;
                dialogue5 = dialogue1Repeat;
                break;
        } 
        
        if (whoToSpawn != WhoToSpawn.Nobody)
        {
            LoadSpecificNPCDialogue();
        }
    }
    

}
