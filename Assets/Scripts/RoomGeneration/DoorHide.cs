using System;
using System.Collections;
using System.Collections.Generic;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;

public class DoorHide : MonoBehaviour
{
    private Animator _animator;
    [SerializeField] private bool isEnemySpawner;
    public bool isDoorOpen;
    private EventInstance _doorEventInstance;
    
    private static readonly int Vanish = Animator.StringToHash("Vanish");
    private static readonly int Door = Animator.StringToHash("OpenDoor");

    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    private void Start()
    {
        _doorEventInstance = AudioManager.Instance.CreateEventInstance(FMODEvents.Instance.BGDoorOpen);
        _doorEventInstance.set3DAttributes(new Vector3(transform.position.x, transform.position.y, transform.position.z).To3DAttributes());
        if (isEnemySpawner)
        {
            _animator.SetTrigger(Vanish);
        }
    }
    
    private void Update()
    {
        if (!isEnemySpawner && BlackoutManager.Instance.blackoutComplete)
        {
            _animator.SetTrigger(Vanish);
        }
    }

    public void SetDoorBool()
    {
        isDoorOpen = true;
    }

    public void OpenDoor()
    {
        _animator.SetTrigger(Door);
    }

    public void PlayDoorSound(int direction)
    {
        AudioManager.Instance.SetEventParameter(_doorEventInstance, "Direction", direction);
        _doorEventInstance.start();
    }

    public void HideDoor()
    {
        if (isEnemySpawner) return;

        gameObject.SetActive(false);
    }
}
