using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class InitialDialogue : MonoBehaviour
{
  public LevelBuilder.LevelMode currentFloor;

  [SerializeField] private DataHolder dataHolder;
  private dialogueControllerScript _dialogueController;
  
    private void Awake()
    {
      currentFloor = dataHolder.currentLevel;
      _dialogueController = GetComponent<dialogueControllerScript>();
    }

    private void Update()
    {
    if (LevelBuilder.Instance.bossRoomGeneratingFinished)
    {
      switch (currentFloor)
      {
        case LevelBuilder.LevelMode.Floor1:
          _dialogueController.dialogueToLoad = AllDialogue.Floor1Intro;
          break;
        case LevelBuilder.LevelMode.Floor2:
          _dialogueController.dialogueToLoad = AllDialogue.Floor2Intro;
          break;
        case LevelBuilder.LevelMode.Floor3:
          _dialogueController.dialogueToLoad = AllDialogue.Floor3Intro;
          break;
      }
    }
    }
}
