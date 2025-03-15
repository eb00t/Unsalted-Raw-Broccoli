using System;
using UnityEngine;
using UnityEngine.SceneManagement;

// this script manages the tutorial for the player by making sure the player does important actions to play the game
// TODO: i apologise if this script is a mess, this will be updated for final submission
public class TutorialController : MonoBehaviour
{
    private GameObject _player;
    private ItemPickupHandler _itemPickupHandler;
    
    [SerializeField] private bool _hasMoved; // Movement (Left/Right)
    [SerializeField]private bool _hasJumped; // Jumping
    [SerializeField]private bool _hasDoubleJumped; // Double Jump
    [SerializeField]private bool _hasCrouched; // Crouch
    [SerializeField]private int _itemsFound; // Find 2 items
    [SerializeField]private bool _hasSwitchedItem; // toggle/switch items
    [SerializeField]private bool _useItem; // use an item
    [SerializeField]private bool _hasPaused; // pause game
    [SerializeField]private bool _hasExitedUI; // close the pause menu to learn about going back
    [SerializeField]private bool _hasOpenedInventory; // quickly open the inventory
    [SerializeField]private bool _hasFoundEnemy; // explore and find enemy
    [SerializeField]private bool _hasLockedOn; // lock onto enemy
    [SerializeField]private bool _hasLightAttacked; // light attack enemy
    [SerializeField]private bool _hasHeavyAttacked; // heavy attack enemy
    // talk about attack combos
    private bool _isEnemyDead; // when the enemy dies direct player to the end of the tutorial to start the game
    
    private void Start()
    {
        if (!SceneManager.GetActiveScene().name.Equals("Tutorial"))
        {
           enabled = false; 
        }
        
        _player = GameObject.Find("PlayerCharacter");
        _itemPickupHandler = _player.GetComponent<ItemPickupHandler>();
    }

    private void Update()
    {
        if (!_hasMoved)
        {
            _itemPickupHandler.TogglePrompt("Move left and right with", true, ControlsManager.ButtonType.LThumbstick);
        }
        else if (!_hasJumped)
        {
            _itemPickupHandler.TogglePrompt("Jump by pressing", true, ControlsManager.ButtonType.ButtonSouth);
        }
        else if (!_hasDoubleJumped)
        {
            _itemPickupHandler.TogglePrompt("To double jump twice or jump while in the air", true, ControlsManager.ButtonType.ButtonSouth);
        }
        else if (!_hasCrouched)
        {
            _itemPickupHandler.TogglePrompt("Crouching allows you to go through certain platforms to fall through them, to crouch press", true, ControlsManager.ButtonType.LThumbstickDown);
        }
        else if (_itemsFound < 2)
        {
            _itemPickupHandler.TogglePrompt("Find and pick up two items, pick them up by pressing", true, ControlsManager.ButtonType.ButtonEast);
        }
        else if (_itemsFound == 2 && !_hasSwitchedItem)
        {
            _itemPickupHandler.TogglePrompt("To switch between items press", true, ControlsManager.ButtonType.DpadEast);
        }
        else if (_itemsFound == 2 && !_useItem && _hasSwitchedItem)
        {
            _itemPickupHandler.TogglePrompt("To use an item press", true, ControlsManager.ButtonType.DpadNorth);
        }
        else if (!_hasPaused && _useItem)
        {
            _itemPickupHandler.TogglePrompt("To access the pause menu press", true, ControlsManager.ButtonType.Start);
        }
        else if (!_hasExitedUI)
        {
            _itemPickupHandler.TogglePrompt("To go back press", true, ControlsManager.ButtonType.ButtonEast);
        }
        else if (!_hasOpenedInventory)
        {
            _itemPickupHandler.TogglePrompt("Quickly open the inventory by pressing", true, ControlsManager.ButtonType.DpadSouth);
        }
        else if (!_hasOpenedInventory)
        {
            _itemPickupHandler.TogglePrompt("Quickly open the inventory by pressing", true, ControlsManager.ButtonType.DpadSouth);
        }
        else if (!_hasFoundEnemy)
        {
            _itemPickupHandler.TogglePrompt("Find an enemy by exploring the rooms", true, ControlsManager.ButtonType.LThumbstick);
        }
        else if (_hasFoundEnemy && !_hasLockedOn)
        {
            _itemPickupHandler.TogglePrompt("To lock onto an enemy press", true, ControlsManager.ButtonType.RThumbstickDown);
        }
        else if (_hasLockedOn && !_hasLightAttacked)
        {
            _itemPickupHandler.TogglePrompt("Perform a light attack", true, ControlsManager.ButtonType.ButtonWest);
        }
        else if (_hasLightAttacked && !_hasHeavyAttacked)
        {
            _itemPickupHandler.TogglePrompt("Perform a heavy attack", true, ControlsManager.ButtonType.ButtonNorth);
        }
        else if (_hasLightAttacked && _hasHeavyAttacked && !_isEnemyDead)
        {
            _itemPickupHandler.TogglePrompt("Defeat the enemy", true, ControlsManager.ButtonType.ButtonWest);
        }
        else if (_isEnemyDead)
        {
            _itemPickupHandler.TogglePrompt("That's the tutorial! You may now continue to the game", true, ControlsManager.ButtonType.LThumbstick);
        }
    }
}
