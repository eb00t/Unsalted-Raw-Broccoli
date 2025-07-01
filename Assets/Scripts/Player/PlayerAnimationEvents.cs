using System.Collections;
using System.Collections.Generic;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;
using UnityEngine.Serialization;

public class PlayerAnimationEvents : MonoBehaviour
{
    [SerializeField] private CharacterAttack charAttack;
    [SerializeField] private CharacterMovement charMovement;
    private EventInstance _footstepEvent;
    [SerializeField] private GameObject playerAtkHitbox;

    private void AdvanceLightCombo()
    {
        charAttack.AdvanceLightCombo();
    }
    
    private void AdvanceMediumCombo()
    {
        charAttack.AdvanceMediumCombo();
    }
    
    private void AdvanceHeavyCombo()
    {
        charAttack.AdvanceHeavyCombo();
    }

    private void disablePlayerMovement()
    {
    }

    private void enablePlayerMovement()
    {
    }

    private void EnablePlayerHitbox()
    {
        playerAtkHitbox.SetActive(true);
    }

    private void DisablePlayerHitbox()
    {
        playerAtkHitbox.SetActive(false);
    }

    private void SetTimeScaleOnAnimator()
    {
        GetComponent<Animator>().updateMode = AnimatorUpdateMode.Normal;
    }
}
