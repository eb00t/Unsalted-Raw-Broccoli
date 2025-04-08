using System;
using FMODUnity;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

// this script manages the tutorial for the player by making sure the player does important actions to play the game
public class TutorialController : MonoBehaviour
{
    private GameObject _player;
    private ItemPickupHandler _itemPickupHandler;

    [SerializeField] private DoorInfo doorItemDown1, doorItemDown2, doorItemRight1, doorItemRight2, doorUp1, doorUp2, doorUpRight, doorUpRight1, doorToEnd1, doorToEnd2;
    [SerializeField] private GameObject arrowItem1, arrowItem1Back, arrowItem2, arrowItem2Back, arrowUp, arrowToEnemy, arrowToEnd1, arrowToEnd2;
    [SerializeField] private GameObject highLight1, hightLight2, hightLight3, hightLight4;

    [SerializeField] private GameObject enemy;
    [SerializeField] private float rangeToEnemy;
    [SerializeField] private DataHolder dataHolder;
    [SerializeField] private GameObject pauseMenu;

    public enum TutorialStep
    {
        Move,
        Jump,
        DoubleJump,
        Crouch,
        FindItems,
        SwitchItem,
        UseItem,
        PauseGame,
        ExitUI,
        FindEnemy,
        LightAttack,
        HeavyAttack,
        JumpAttack,
        DefeatEnemy,
        Complete
    }

    [SerializeField] private TutorialStep _currentStep = TutorialStep.Move;
    private int _itemsFound;
    private CharacterMovement _characterMovement;
    private CharacterAttack _characterAttack;

    private void Start()
    {
        if (!SceneManager.GetActiveScene().name.Equals("Tutorial"))
        {
            enabled = false;
            return;
        }
        
        WipeData();

        _player = gameObject;
        _itemPickupHandler = _player.GetComponent<ItemPickupHandler>();
        _characterMovement = _player.GetComponent<CharacterMovement>();
        _characterAttack = _player.GetComponentInChildren<CharacterAttack>();
        ShowPrompt();
    }
    
    private void Update()
    {
        if (_currentStep == TutorialStep.FindEnemy)
        {
            var dist = Vector3.Distance(_player.transform.position, enemy.transform.position);
            if (dist <= rangeToEnemy)
            {
                AdvanceStep();
            }
        }

        if (_currentStep == TutorialStep.DoubleJump)
        {
            if (_characterMovement.doubleJumpPerformed)
            {
                AdvanceStep();
                highLight1.SetActive(true);
                hightLight2.SetActive(true);
                hightLight3.SetActive(true);
                hightLight4.SetActive(true);
            }
        }

        if (_currentStep == TutorialStep.JumpAttack)
        {
            if (_characterAttack._jumpAttackCount > 0)
            {
                AdvanceStep();
            }
        }
    }

    private void AdvanceStep()
    {
        if (_currentStep == TutorialStep.Complete)
        {
            return;
        }
        
        _currentStep++;
        ShowPrompt();
    }

    public void EnemyDefeated()
    {
        arrowToEnd1.SetActive(true);
        arrowToEnd2.SetActive(true);
        doorToEnd1.OpenDoor();
        doorToEnd2.OpenDoor();
        _currentStep = TutorialStep.DefeatEnemy;
        AdvanceStep();
    }

    private void ShowPrompt()
    {
        ButtonHandler.Instance.PlayConfirmSound();
        switch (_currentStep)
        {
            case TutorialStep.Move:
                ShowMessage("Move left and right with", ControlsManager.ButtonType.LThumbstick, null);
                break;
            case TutorialStep.Jump:
                ShowMessage("Jump by pressing", ControlsManager.ButtonType.ButtonSouth, null);
                break;
            case TutorialStep.DoubleJump:
                ShowMessage("Double jump by jumping again in the air", ControlsManager.ButtonType.ButtonSouth, ControlsManager.ButtonType.ButtonSouth);
                break;
            case TutorialStep.Crouch:
                ShowMessage("Crouch to fall through certain platforms by pressing", ControlsManager.ButtonType.LThumbstickDown, null);
                break;
            case TutorialStep.FindItems:
                ShowMessage("Find and pick up two items by pressing", ControlsManager.ButtonType.ButtonEast, null);
                break;
            case TutorialStep.SwitchItem:
                ShowMessage("Switch between items by pressing", ControlsManager.ButtonType.DpadEast, null);
                break;
            case TutorialStep.UseItem:
                ShowMessage("Use an item by pressing", ControlsManager.ButtonType.DpadNorth, null);
                break;
            case TutorialStep.PauseGame:
                ShowMessage("Pause the game by pressing", ControlsManager.ButtonType.Start, null);
                break;
            case TutorialStep.ExitUI:
                ShowMessage("Exit the pause menu by pressing", ControlsManager.ButtonType.ButtonEast, null);
                break;
            case TutorialStep.FindEnemy:
                ShowMessage("Find an enemy by exploring the rooms", ControlsManager.ButtonType.LThumbstick, null);
                break;
            case TutorialStep.LightAttack:
                ShowMessage("Perform a light attack", ControlsManager.ButtonType.ButtonWest, null);
                break;
            case TutorialStep.HeavyAttack:
                ShowMessage("Perform a heavy attack", ControlsManager.ButtonType.ButtonNorth, null);
                break;
            case TutorialStep.JumpAttack:
                ShowMessage("To perform a jump attack, jump and perform a light attack", ControlsManager.ButtonType.ButtonSouth, ControlsManager.ButtonType.ButtonWest);
                break;
            case TutorialStep.DefeatEnemy:
                ShowMessage("Defeat the enemy", ControlsManager.ButtonType.ButtonWest, null);
                break;
            case TutorialStep.Complete:
                ShowMessage("Tutorial complete! You may now continue to the game", ControlsManager.ButtonType.LThumbstick, null);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void ShowMessage(string message, ControlsManager.ButtonType button, ControlsManager.ButtonType? button2)
    {
        _itemPickupHandler.TogglePrompt(message, true, button, button2);
    }

    public void OnItemPickedUp()
    {
        _itemsFound++;
        if (_currentStep != TutorialStep.FindItems) return;
        
        if (_itemsFound == 1)
        {
            arrowItem1.SetActive(false);
            arrowItem1Back.SetActive(true);
            doorItemRight1.OpenDoor();
            doorItemRight2.OpenDoor();
            arrowItem2.SetActive(true);
        }

        if (_itemsFound >= 2 && _currentStep == TutorialStep.FindItems)
        {
            arrowItem2.SetActive(false);
            
            AdvanceStep();
        }
    }

    public void HasPlayerMoved(InputAction.CallbackContext context)
    {
        if (context.ReadValue<float>() > 0.1f && _currentStep == TutorialStep.Move)
        {
            AdvanceStep();
        }
    }

    public void PlayerJumped(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        if (_currentStep == TutorialStep.Jump)
        {
            AdvanceStep();
        }
    }

    public void PlayerCrouched(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        if (_currentStep == TutorialStep.Crouch)
        {
            highLight1.SetActive(false);
            hightLight2.SetActive(false);
            hightLight3.SetActive(false);
            hightLight4.SetActive(false);
            
            AdvanceStep();
            if (_itemsFound == 0)
            {
                doorItemDown1.OpenDoor();
                doorItemDown2.OpenDoor();
                arrowItem1.SetActive(true);
            }
        }
    }

    public void ItemSwitched(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        var dir = context.ReadValue<Vector2>();
        
        if (_currentStep == TutorialStep.SwitchItem)
        {
            switch (dir.x, dir.y)
            {
                case (1, 0): // right (1)
                    break;
                case (-1, 0): // left (3)
                    break;
                default:
                    return;
            }
            
            AdvanceStep();
        }
    }
    
    public void ItemUsed(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        var dir = context.ReadValue<Vector2>();
        
        if (_currentStep == TutorialStep.UseItem)
        {
            switch (dir.x, dir.y)
            {
                case (0, 1): // up
                    break;
                default:
                    return;
            }
            
            AdvanceStep();
        }
    }

    public void OpenPause(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        if (_currentStep == TutorialStep.PauseGame)
        {
            AdvanceStep();
        }
    }
    
    public void GoBack(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        if (_currentStep == TutorialStep.ExitUI && !pauseMenu.activeSelf)
        {
            arrowItem2Back.SetActive(true);
            arrowUp.SetActive(true);
            arrowToEnemy.SetActive(true);
            doorUp1.OpenDoor();
            doorUp2.OpenDoor();
            doorUpRight.OpenDoor();
            doorUpRight1.OpenDoor();
            AdvanceStep();
        }
    }
    
    public void TryLightAttack(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        
        if (_currentStep == TutorialStep.LightAttack)
        {
            AdvanceStep();
        }
    }
    
    public void TryHeavyAttack(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        
        if (_currentStep == TutorialStep.HeavyAttack)
        {
            AdvanceStep();
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, rangeToEnemy);
    }
    
    private void WipeData()
    {
        dataHolder.savedItems.Clear();
        dataHolder.savedItemCounts.Clear();
        dataHolder.equippedConsumables = new int[5];
        dataHolder.currencyHeld = 0;
        dataHolder.currentLevel = LevelBuilder.LevelMode.Floor1;
    }
}
