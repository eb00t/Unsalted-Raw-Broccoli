using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;
using STOP_MODE = FMOD.Studio.STOP_MODE;

public class AnimationEvents : MonoBehaviour
{
    private EventInstance _footstepEvent,
        _explosionEvent,
        _alarmEvent,
        _lightAttackEvent,
        _mediumAttackEvent,
        _heavyAttackEvent,
        _jumpEvent;
    
    //PLAYER
    public void PlayPlayerFootstepSound()
    {
        _footstepEvent = AudioManager.Instance.CreateEventInstance(FMODEvents.Instance.PlayerFootsteps);
        _footstepEvent.set3DAttributes(new Vector3(transform.position.x, transform.position.y - 4.96f, transform.position.z).To3DAttributes());
        _footstepEvent.start();
        _footstepEvent.release();
    }
    public void PlayPlayerLightAttackSound()
    {
        _lightAttackEvent = AudioManager.Instance.CreateEventInstance(FMODEvents.Instance.PlayerLightAttack);
        _lightAttackEvent.set3DAttributes(new Vector3(transform.position.x, transform.position.y, transform.position.z).To3DAttributes());
        _lightAttackEvent.start();
        _lightAttackEvent.release();
    }
    public void PlayPlayerMediumAttackSound()
    {
        _mediumAttackEvent = AudioManager.Instance.CreateEventInstance(FMODEvents.Instance.PlayerMediumAttack);
        _mediumAttackEvent.set3DAttributes(new Vector3(transform.position.x, transform.position.y, transform.position.z).To3DAttributes());
        _mediumAttackEvent.start();
        _mediumAttackEvent.release();
    }
    public void PlayPlayerHeavyAttackSound()
    {
        _heavyAttackEvent = AudioManager.Instance.CreateEventInstance(FMODEvents.Instance.PlayerMediumAttack);
        _heavyAttackEvent.set3DAttributes(new Vector3(transform.position.x, transform.position.y, transform.position.z).To3DAttributes());
        _heavyAttackEvent.start();
        _heavyAttackEvent.release();
    }

    public void PlayPlayerJumpSound()
    {
        _jumpEvent = AudioManager.Instance.CreateEventInstance(FMODEvents.Instance.PlayerJump);
        _jumpEvent.set3DAttributes(new Vector3(transform.position.x, transform.position.y, transform.position.z).To3DAttributes());
        _jumpEvent.start();
        _jumpEvent.release();
    }
    
    //ENEMIES
    public void PlayExplosionSound()
    {
        _explosionEvent = AudioManager.Instance.CreateEventInstance(FMODEvents.Instance.Explosion);
        AudioManager.Instance.AttachInstanceToGameObject(_explosionEvent, gameObject.transform);
        _explosionEvent.start();
        _explosionEvent.release();
    }

    public void BlowUp()
    {
        gameObject.GetComponent<CinemachineImpulseSource>().GenerateImpulse();
    }

    //COPY BOSS
    public void PlayCopyBossFootstepSound()
    {
        _footstepEvent = AudioManager.Instance.CreateEventInstance(FMODEvents.Instance.CopyBossFootsteps);
            
        _footstepEvent.start();
        _footstepEvent.release();
    }
    
    public void PlayCopyBossLightAttackSound()
    {
        _lightAttackEvent = AudioManager.Instance.CreateEventInstance(FMODEvents.Instance.CopyBossLightAttack);
        _lightAttackEvent.set3DAttributes(new Vector3(transform.position.x, transform.position.y, transform.position.z).To3DAttributes());
        _lightAttackEvent.start();
        _lightAttackEvent.release();
    }
    public void PlayCopyBossMediumAttackSound()
    {
        _mediumAttackEvent = AudioManager.Instance.CreateEventInstance(FMODEvents.Instance.CopyBossMediumAttack);
        _mediumAttackEvent.set3DAttributes(new Vector3(transform.position.x, transform.position.y, transform.position.z).To3DAttributes());
        _mediumAttackEvent.start();
        _mediumAttackEvent.release();
    }
    public void PlayCopyBossHeavyAttackSound()
    {
        _heavyAttackEvent = AudioManager.Instance.CreateEventInstance(FMODEvents.Instance.CopyBossHeavyAttack);
        _heavyAttackEvent.set3DAttributes(new Vector3(transform.position.x, transform.position.y, transform.position.z).To3DAttributes());
        _heavyAttackEvent.start();
        _heavyAttackEvent.release();
    }

    public void PlayCopyBossJumpSound()
    {
        _jumpEvent = AudioManager.Instance.CreateEventInstance(FMODEvents.Instance.CopyBossJump);
        _jumpEvent.set3DAttributes(new Vector3(transform.position.x, transform.position.y, transform.position.z).To3DAttributes());
        _jumpEvent.start();
        _jumpEvent.release();
    }
}
