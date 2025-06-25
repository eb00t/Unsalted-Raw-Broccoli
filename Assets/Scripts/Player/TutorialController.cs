using System;
using DG.Tweening;
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

    [SerializeField] private DoorInfo doorItemDown1,
        doorItemDown2,
        doorItemRight1,
        doorItemRight2,
        doorUp1,
        doorUp2,
        doorUpRight,
        doorUpRight1,
        doorToEnd1,
        doorToEnd2,
        doorPastLaser1,
        doorPastLaser2;
    [SerializeField] private GameObject arrowItem1, arrowItem1Back, arrowItem2, arrowItem2Back, arrowUp, arrowToEnemy, arrowToEnd1, arrowToEnd2;
    [SerializeField] private GameObject highLight1, arrowRect1, hightLight3, arrowRect2;

    [SerializeField] private GameObject enemy;
    [SerializeField] private float rangeToEnemy;
    [SerializeField] private DataHolder dataHolder;
    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private CanvasGroup toolbarGroup;

    public enum TutorialStep
    {
        Move,
        Jump,
        DoubleJump,
        Crouch,
        FindItems,
        UseUpThrust,
        SwitchItem,
        UseItem,
        Map,
        Explore,
        DashThroughLaser,
        FindEnemy,
        LightAttack,
        MediumAttack,
        HeavyAttack,
        JumpAttack,
        DefeatEnemy,
        Complete
    }

    [SerializeField] private TutorialStep _currentStep = TutorialStep.Move;
    private int _itemsFound;
    private CharacterMovement _characterMovement;
    private CharacterAttack _characterAttack;
    private EnemyHandler _enemyHandler;

    private void Start()
    {
        _enemyHandler = enemy.GetComponent<EnemyHandler>();
        if (!SceneManager.GetActiveScene().name.Equals("Tutorial"))
        {
            enabled = false;
            return;
        }

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
                highLight1.SetActive(true);
                hightLight3.SetActive(true);
                AdvanceStep();
            }
        }

        if (_currentStep == TutorialStep.JumpAttack)
        {
            if (_characterAttack.jumpAttackCount > 0)
            {
                _enemyHandler.defense = 0;
                _enemyHandler.alwaysShowHealth = true;
                
                AdvanceStep();
            }
        }

        if (_currentStep == TutorialStep.UseUpThrust)
        {
            arrowRect1.SetActive(true);
            arrowRect2.SetActive(true);
            if (_characterMovement.isInUpThrust)
            {
                AdvanceStep();
                arrowRect1.SetActive(false);
                arrowRect2.SetActive(false);
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
                ShowMessage("Move left and right with", ControlsManager.ButtonType.Move, "", null);
                break;
            case TutorialStep.Jump:
                ShowMessage("Jump by holding or pressing", ControlsManager.ButtonType.Jump, "", null);
                break;
            case TutorialStep.DoubleJump:
                ShowMessage("Double jump by jumping again in the air", ControlsManager.ButtonType.Jump, " -> ", ControlsManager.ButtonType.Jump);
                break;
            case TutorialStep.Crouch:
                ShowMessage("Crouch to fall through some platforms by pressing", ControlsManager.ButtonType.CrouchC, " or ",ControlsManager.ButtonType.CrouchR);
                break;
            case TutorialStep.FindItems:
                ShowMessage("Find and pick up two items by pressing", ControlsManager.ButtonType.Interact, "", null);
                break;
            case TutorialStep.UseUpThrust:
                ShowMessage("Jump into vertical hallways to go to rooms above you", ControlsManager.ButtonType.Jump, "", null);
                break;
            case TutorialStep.SwitchItem:
                ShowMessage("Switch between items by pressing", ControlsManager.ButtonType.CycleToolbarLeft, " or ",ControlsManager.ButtonType.CycleToolbarRight);
                break;
            case TutorialStep.UseItem:
                ShowMessage("Use an item by pressing", ControlsManager.ButtonType.UseItem, "", null);
                break;
            case TutorialStep.Map:
                ShowMessage("View the map", ControlsManager.ButtonType.OpenMap, "", null);
                break;
            case TutorialStep.Explore:
                ShowMessage("Explore rooms to find the exit", ControlsManager.ButtonType.Move, "", null);
                break;
            case TutorialStep.DashThroughLaser:
                ShowMessage("Dash through attacks and obstacles by pressing", ControlsManager.ButtonType.Dash, "", null);
                break;
            case TutorialStep.FindEnemy:
                ShowMessage("Find an enemy by exploring the rooms", ControlsManager.ButtonType.Move, "", null);
                break;
            case TutorialStep.LightAttack:
                ShowMessage("Perform a light attack", ControlsManager.ButtonType.LightAttack, "", null);
                break;
            case TutorialStep.MediumAttack:
                ShowMessage("Perform a medium attack", ControlsManager.ButtonType.MediumAttack, "", null);
                break;
            case TutorialStep.HeavyAttack:
                ShowMessage("Perform a heavy attack", ControlsManager.ButtonType.HeavyAttack, "", null);
                break;
            case TutorialStep.JumpAttack:
                ShowMessage("Perform a jump attack", ControlsManager.ButtonType.Jump, " -> ", ControlsManager.ButtonType.LightAttack);
                break;
            case TutorialStep.DefeatEnemy:
                ShowMessage("Defeat the enemy", ControlsManager.ButtonType.LightAttack, "", null);
                break;
            case TutorialStep.Complete:
                ShowMessage("Tutorial complete! You may now continue to Floor 1", ControlsManager.ButtonType.Move,"", null);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void ShowMessage(string message, ControlsManager.ButtonType button, string betweenText,ControlsManager.ButtonType? button2)
    {
        _itemPickupHandler.TogglePrompt(message, true, button, betweenText, button2, true);
    }

    public void OnItemPickedUp()
    {
        _itemsFound++;
        if (_currentStep != TutorialStep.FindItems) return;
        
        if (_itemsFound == 1)
        {
            doorItemDown1.OpenDoor();
            doorItemDown2.OpenDoor();
            arrowItem1.SetActive(true);
            arrowItem2.SetActive(false);
            arrowItem2Back.SetActive(true);
        }

        if (_itemsFound >= 2 && _currentStep == TutorialStep.FindItems)
        {
            arrowItem2.SetActive(false);
            arrowItem1.SetActive(false);
            arrowItem2Back.SetActive(false);
            arrowItem1Back.SetActive(true);
            
            AdvanceStep();
        }
    }

    public void OnLaserPassed()
    {
        if (_currentStep == TutorialStep.DashThroughLaser)
        {
            doorPastLaser1.OpenDoor();
            doorPastLaser2.OpenDoor();
            AdvanceStep();
        }
    }

    public void OnLaserFound()
    {
        if (_currentStep == TutorialStep.Explore)
        {
            AdvanceStep();
        }
    }

    public void MapOpened(InputAction.CallbackContext context)
    {
        if (context.performed && _currentStep == TutorialStep.Map)
        {
            doorUp1.OpenDoor();
            doorUp2.OpenDoor();
            doorUpRight.OpenDoor();
            doorUpRight1.OpenDoor();
            arrowToEnemy.SetActive(true);
            AdvanceStep();
        }
    }

    public void HasPlayerMoved(InputAction.CallbackContext context)
    {
        if (!BlackoutManager.Instance.blackoutComplete) return;
        
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
            hightLight3.SetActive(false);
            
            AdvanceStep();
            toolbarGroup.DOFade(1f, 0.5f);
            if (_itemsFound == 0)
            {
                doorItemRight1.OpenDoor();
                doorItemRight2.OpenDoor();
                arrowItem2.SetActive(true);
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
    
    public void TryLightAttack(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        
        if (_currentStep == TutorialStep.LightAttack)
        {
            AdvanceStep();
        }
    }
    
    public void TryMediumAttack(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        
        if (_currentStep == TutorialStep.MediumAttack)
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
}
