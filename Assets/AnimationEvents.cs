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
    private EventInstance _footstepEvent, _explosionEvent, _alarmEvent;
    public void PlayPlayerFootstepSound()
    {
        _footstepEvent = AudioManager.Instance.CreateEventInstance(FMODEvents.Instance.PlayerFootsteps);
        _footstepEvent.set3DAttributes(new Vector3(transform.position.x, transform.position.y - 4.96f, transform.position.z).To3DAttributes());
        _footstepEvent.start();
        _footstepEvent.release();
    }
    public void PlayExplosionSound()
    {
        _explosionEvent = AudioManager.Instance.CreateEventInstance(FMODEvents.Instance.Explosion);
        _explosionEvent.set3DAttributes(new Vector3(transform.position.x, transform.position.y, transform.position.z).To3DAttributes());
        _explosionEvent.start();
        _explosionEvent.release();
    }

  

    public void BlowUp()
    {
        gameObject.GetComponent<CinemachineImpulseSource>().GenerateImpulse();
    }

    public void Update()
    {
    }
}
