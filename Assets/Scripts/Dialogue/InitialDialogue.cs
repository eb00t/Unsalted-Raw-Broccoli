using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class InitialDialogue : MonoBehaviour
{
    public LevelBuilder.LevelMode currentFloor;
    public bool endScreen;
    [SerializeField] private DataHolder dataHolder;
    private dialogueControllerScript _dialogueController;
    public enum BossOrIntro
    {
      Intro,
      Boss,
    }
    public BossOrIntro bossOrIntro;

    private void Awake()
    {
      _dialogueController = GetComponent<dialogueControllerScript>();
    }

    private void Start()
    {
      currentFloor = LevelBuilder.Instance.currentFloor;
      
      if (dataHolder.demoMode && bossOrIntro == BossOrIntro.Intro)
      {
        gameObject.SetActive(false);
      }
    }

    private void Update()
    {
      if (currentFloor != LevelBuilder.LevelMode.EndScreen)
      {

        if (LevelBuilder.Instance.bossRoomGeneratingFinished)
        {
          switch (currentFloor)
          {
            case LevelBuilder.LevelMode.Floor1:
              switch (bossOrIntro)
              {
                case BossOrIntro.Intro:
                  _dialogueController.dialogueToLoad = DialogueReference.Instance.Floor1Intro;
                  break;
                case BossOrIntro.Boss:
                  _dialogueController.dialogueToLoad = DialogueReference.Instance.Floor1Boss;
                  break;
              }

              break;
            case LevelBuilder.LevelMode.Floor2:
              switch (bossOrIntro)
              {
                case BossOrIntro.Intro:
                  _dialogueController.dialogueToLoad = DialogueReference.Instance.Floor2Intro;
                  dataHolder.demoMode = false;
                  break;
                case BossOrIntro.Boss:
                  _dialogueController.dialogueToLoad = DialogueReference.Instance.Floor2Boss;
                  break;
              }

              break;
            case LevelBuilder.LevelMode.Floor3:
              if (bossOrIntro == BossOrIntro.Intro)
              {
                _dialogueController.dialogueToLoad = DialogueReference.Instance.Floor3Intro;
                dataHolder.demoMode = false;
              }
              else if (bossOrIntro == BossOrIntro.Boss)
              {
                _dialogueController.dialogueToLoad = DialogueReference.Instance.Floor3Boss;
              }

              break;
            case LevelBuilder.LevelMode.Floor4:
              if (bossOrIntro == BossOrIntro.Intro)
              {
                _dialogueController.dialogueToLoad = DialogueReference.Instance.Floor4Intro;
                dataHolder.demoMode = false;
              }
              else if (bossOrIntro == BossOrIntro.Boss)
              {
                _dialogueController.dialogueToLoad = DialogueReference.Instance.Floor4Boss;
              }

              break;
            case LevelBuilder.LevelMode.FinalBoss:
              if (bossOrIntro == BossOrIntro.Intro)
              {
                _dialogueController.dialogueToLoad = DialogueReference.Instance.Floor5Intro;
              }
              else if (bossOrIntro == BossOrIntro.Boss)
              {
                _dialogueController.dialogueToLoad = DialogueReference.Instance.Floor5Boss;
              }

              break;
          }
        }
      }
      else
      {
        if (BlackoutManager.Instance.blackoutComplete && !dataHolder.hardcoreMode)
        {
          _dialogueController.dialogueToLoad = DialogueReference.Instance.EndNormal;
        } 
        else if (BlackoutManager.Instance.blackoutComplete && dataHolder.hardcoreMode)
        {
          _dialogueController.dialogueToLoad = DialogueReference.Instance.EndHard;
        }
      }
    }
}

