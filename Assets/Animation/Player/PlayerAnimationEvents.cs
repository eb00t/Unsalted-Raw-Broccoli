using System.Collections;
using System.Collections.Generic;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;

public class PlayerAnimationEvents : MonoBehaviour
{
    CharacterAttack charAttack;
    CharacterMovement charMovement;
    private EventInstance _footstepEvent;
    void Start()
    {
        charAttack = GameObject.FindGameObjectWithTag("PlayerAttackBox").GetComponent<CharacterAttack>();
        charMovement = GameObject.FindGameObjectWithTag("Player").GetComponent<CharacterMovement>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void disableCollider()
    {
        charAttack.DisableCollider();
    }

    private void enableCollider()
    {
        charAttack.EnableCollider();
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

    private void signalAnimationEnd()
    {
        charAttack.animEnd = true;
    }

    public void setIFrameOn()
    {
        charAttack.isInvulnerable = true;
    }
    
    public void setIFrameOff()
    {
        charAttack.isInvulnerable = false;
    }
}
