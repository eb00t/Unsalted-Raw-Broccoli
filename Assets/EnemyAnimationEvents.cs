using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;

public class EnemyAnimationEvents : MonoBehaviour
{
    private EventInstance _footstepEvent, _explosionEvent;
    void Start()
    {
        
    }
    
    public void PlayFootstepSound()
    {
        _footstepEvent = AudioManager.Instance.CreateEventInstance(FMODEvents.Instance.Footsteps);
        _footstepEvent.set3DAttributes(new Vector3(transform.position.x, transform.position.y, transform.position.z).To3DAttributes());
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

}
