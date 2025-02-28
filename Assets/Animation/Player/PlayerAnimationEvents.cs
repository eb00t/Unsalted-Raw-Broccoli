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
    }

    private void enablePlayerMovement()
    {
        charMovement.allowMovement = true;
    }

    private void signalAnimationEnd()
    {
        charAttack.animEnd = true;
    }

    public void PlayFootstepSound()
    {
        _footstepEvent = AudioManager.Instance.CreateEventInstance(FMODEvents.Instance.Footsteps);
        _footstepEvent.set3DAttributes(new Vector3(transform.position.x, transform.position.y - 4.96f, transform.position.z).To3DAttributes());
        _footstepEvent.start();
        _footstepEvent.release();
    }
}
