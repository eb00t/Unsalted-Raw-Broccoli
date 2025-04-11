using System.Collections;
using System.Collections.Generic;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;

public class PlayerAnimationEvents : MonoBehaviour
{
    private CharacterAttack charAttack;
    private CharacterMovement charMovement;
    private EventInstance _footstepEvent;
    private GameObject _playerAtkHitbox;
    
    void Start()
    {
        charAttack = GameObject.FindGameObjectWithTag("PlayerAttackBox").GetComponent<CharacterAttack>();
        charMovement = GameObject.FindGameObjectWithTag("Player").GetComponent<CharacterMovement>();
        _playerAtkHitbox = GetComponentInChildren<PlayerHitboxHandler>(true).gameObject;
    }

    private void AdvanceLightCombo()
    {
        charAttack.AdvanceLightCombo();
    }
    
    private void AdvanceHeavyCombo()
    {
        charAttack.AdvanceHeavyCombo();
    }

    private void disablePlayerMovement()
    {
        charMovement.allowMovement = false;
        charMovement.walkAllowed = false;
    }

    private void enablePlayerMovement()
    {
        charMovement.allowMovement = true;
        charMovement.walkAllowed = true;
    }

    private void EnablePlayerHitbox()
    {
        _playerAtkHitbox.SetActive(true);
    }

    private void DisablePlayerHitbox()
    {
        _playerAtkHitbox.SetActive(false);
    }
}
