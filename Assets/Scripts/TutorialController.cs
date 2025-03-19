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
    private int _doubleJumpCount;

    [SerializeField] private GameObject doorItem1, doorItem2, doorUp, doorToEnemy, doorToEnd;
    [SerializeField] private GameObject arrowItem1, arrowItem1Back, arrowItem2, arrowItem2Back, arrowUp, arrowToEnemy, arrowToEnd1, arrowToEnd2;
    [SerializeField] private GameObject highLight1, hightLight2, hightLight3, hightLight4;

    [SerializeField] private GameObject enemy;
    [SerializeField] private float rangeToEnemy;

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
        OpenInventory,
        FindEnemy,
        LockOn,
        LightAttack,
        HeavyAttack,
        DefeatEnemy,
        Complete
    }

    [SerializeField] private TutorialStep _currentStep = TutorialStep.Move;
    private int _itemsFound;

    private void Start()
    {
        if (!SceneManager.GetActiveScene().name.Equals("Tutorial"))
        {
            enabled = false;
            return;
        }

        _player = gameObject;
        _itemPickupHandler = _player.GetComponent<ItemPickupHandler>();
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
    }

    private void AdvanceStep()
    {
        if (_currentStep == TutorialStep.Complete)
        {
            return;
        }
        
        _currentStep++;
        Debug.Log($"Tutorial advanced to: {_currentStep}");
        ShowPrompt();
    }

    public void EnemyDefeated()
    {
        if (_currentStep != TutorialStep.DefeatEnemy && _currentStep != TutorialStep.LightAttack &&
            _currentStep != TutorialStep.HeavyAttack && _currentStep != TutorialStep.LockOn) return;
        
        arrowToEnd1.SetActive(true);
        arrowToEnd2.SetActive(true);
        doorToEnd.transform.position = new Vector3(doorToEnd.transform.position.x, doorToEnd.transform.position.y, doorToEnd.transform.position.z + 3);
        AdvanceStep();
    }

    private void ShowPrompt()
    {
        ButtonHandler.Instance.PlayConfirmSound();
        switch (_currentStep)
        {
            case TutorialStep.Move:
                ShowMessage("Move left and right with", ControlsManager.ButtonType.LThumbstick);
                break;
            case TutorialStep.Jump:
                ShowMessage("Jump by pressing", ControlsManager.ButtonType.ButtonSouth);
                break;
            case TutorialStep.DoubleJump:
                ShowMessage("Double jump by jumping again in the air", ControlsManager.ButtonType.ButtonSouth);
                break;
            case TutorialStep.Crouch:
                ShowMessage("Crouch to fall through certain platforms by pressing", ControlsManager.ButtonType.LThumbstickDown);
                break;
            case TutorialStep.FindItems:
                ShowMessage("Find and pick up two items by pressing", ControlsManager.ButtonType.ButtonEast);
                break;
            case TutorialStep.SwitchItem:
                ShowMessage("Switch between items by pressing", ControlsManager.ButtonType.DpadEast);
                break;
            case TutorialStep.UseItem:
                ShowMessage("Use an item by pressing", ControlsManager.ButtonType.DpadNorth);
                break;
            case TutorialStep.PauseGame:
                ShowMessage("Pause the game by pressing", ControlsManager.ButtonType.Start);
                break;
            case TutorialStep.ExitUI:
                ShowMessage("Exit the pause menu by pressing", ControlsManager.ButtonType.ButtonEast);
                break;
            case TutorialStep.OpenInventory:
                ShowMessage("Quickly open the inventory by pressing", ControlsManager.ButtonType.DpadSouth);
                break;
            case TutorialStep.FindEnemy:
                ShowMessage("Find an enemy by exploring the rooms", ControlsManager.ButtonType.LThumbstick);
                break;
            case TutorialStep.LockOn:
                ShowMessage("Lock onto an enemy by pressing", ControlsManager.ButtonType.RThumbstickDown);
                break;
            case TutorialStep.LightAttack:
                ShowMessage("Perform a light attack", ControlsManager.ButtonType.ButtonWest);
                break;
            case TutorialStep.HeavyAttack:
                ShowMessage("Perform a heavy attack", ControlsManager.ButtonType.ButtonNorth);
                break;
            case TutorialStep.DefeatEnemy:
                ShowMessage("Defeat the enemy", ControlsManager.ButtonType.ButtonWest);
                break;
            case TutorialStep.Complete:
                ShowMessage("Tutorial complete! You may now continue to the game", ControlsManager.ButtonType.LThumbstick);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void ShowMessage(string message, ControlsManager.ButtonType button)
    {
        _itemPickupHandler.TogglePrompt(message, true, button);
    }

    public void OnItemPickedUp()
    {
        _itemsFound++;
        if (_currentStep != TutorialStep.FindItems) return;
        
        if (_itemsFound == 1)
        {
            arrowItem1.SetActive(false);
            arrowItem1Back.SetActive(true);
            doorItem2.transform.position = new Vector3(doorItem2.transform.position.x, doorItem2.transform.position.y, doorItem2.transform.position.z + 3);
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
        else if (_currentStep == TutorialStep.DoubleJump)
        {
            _doubleJumpCount++;
            if (_doubleJumpCount >= 2)
            {
                AdvanceStep();
                highLight1.SetActive(true);
                hightLight2.SetActive(true);
                hightLight3.SetActive(true);
                hightLight4.SetActive(true);
            }
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
                doorItem1.transform.position = new Vector3(doorItem1.transform.position.x, doorItem1.transform.position.y, doorItem1.transform.position.z + 3);
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
        Debug.Log("goback called");
        if (_currentStep == TutorialStep.ExitUI)
        {
            AdvanceStep();
        }
    }

    public void InventoryOpened(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        var dir = context.ReadValue<Vector2>();
        
        if (_currentStep == TutorialStep.OpenInventory)
        {
            switch (dir.x, dir.y)
            {
                case (0, -1): // down
                    break;
                default:
                    return;
            }
            
            AdvanceStep();
            
            arrowItem2Back.SetActive(true);
            arrowUp.SetActive(true);
            arrowToEnemy.SetActive(true);
            doorToEnemy.transform.position = new Vector3(doorToEnemy.transform.position.x, doorToEnemy.transform.position.y, doorToEnemy.transform.position.z + 3);
            doorUp.transform.position = new Vector3(doorUp.transform.position.x, doorUp.transform.position.y, doorUp.transform.position.z + 3);
        }
    }

    public void TryLockOn(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        
        if (_currentStep == TutorialStep.LockOn)
        {
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
}
