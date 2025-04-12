using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;
using Random = UnityEngine.Random;
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
    private CharacterAttack _characterAttack;
    private CharacterMovement _characterMovement;
    private SettingManager _settingManager;
    [SerializeField] private Animator blackoutCircle;
    [SerializeField] private GameObject deathScreen;

    private void Start()
    {
        _settingManager = GameObject.Find("Settings").GetComponent<SettingManager>();
        _characterAttack = GameObject.FindGameObjectWithTag("PlayerAttackBox").GetComponent<CharacterAttack>();
        _characterMovement = GameObject.FindGameObjectWithTag("Player").GetComponent<CharacterMovement>();
    }

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

    public void Push(int LMH)
    {
        _characterAttack.AttackForce(LMH);
    }

    public void ReduceEnergy(int LMH) //Light = 0, Medium = 1, Heavy = 0
    {
        //_characterAttack = transform.root.transform.Find("PlayerAttack").GetComponent<CharacterAttack>();
        int energyUsed = 0;
        switch (LMH)
        {
            case 0:
                energyUsed = _characterAttack.lightEnergyCost;
                break;
            case 1:
                energyUsed = _characterAttack.mediumEnergyCost;
                break;
            case 2:
                energyUsed = _characterAttack.heavyEnergyCost;
                break;
            case 3:
                energyUsed = _characterMovement.dashEnergyCost;
                break;
        }
        _characterAttack.UseEnergy(energyUsed);
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
        gameObject.GetComponent<CinemachineImpulseSource>().GenerateImpulseWithVelocity(new Vector3(Random.Range(-1f, 1f), 3f, 0f) * _settingManager.screenShakeMultiplier);
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
    
    // UI
    public void FadeToBlack()
    {
        deathScreen.SetActive(true);
        blackoutCircle.enabled = true;
    }
}
