using UnityEngine;
using UnityEngine.SceneManagement;

// this script manages the tutorial for the player by making sure the player does important actions to play the game
public class TutorialController : MonoBehaviour
{
    private GameObject _player;
    
    private bool _hasMoved; // Movement (Left/Right)
    private bool _hasJumped; // Jumping
    private bool _hasDoubleJumped; // Double Jump
    private bool _hasCrouched; // Crouch
    private int _itemsFound; // Find 2 items
    private bool _hasSwitchedItem; // toggle/switch items
    private bool _useItem; // use an item
    private bool _hasPaused; // pause game
    private bool _hasOpenedInventory; // open the inventory
    private bool _hasExitedUI; // close the inventory and pause menu
    private bool _hasFoundEnemy; // explore and find enemy
    private bool _hasLockedOn; // lock onto enemy
    private bool _hasLightAttacked; // light attack enemy
    private bool _hasHeavyAttacked; // heavy attack enemy
    // talk about attack combos
    private bool _isEnemyDead; // when the enemy dies direct player to the end of the tutorial to start the game
    
    private void Start()
    {
        if (!SceneManager.GetActiveScene().name.Equals("Tutorial"))
        {
           enabled = false; 
        }
    }
}
