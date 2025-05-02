using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorHide : MonoBehaviour
{
    private Animator _animator;
    [SerializeField] private bool isEnemySpawner;
    public bool isDoorOpen;
    
    private static readonly int Vanish = Animator.StringToHash("Vanish");
    private static readonly int Door = Animator.StringToHash("OpenDoor");

    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    private void Start()
    {
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

    public void HideDoor()
    {
        if (isEnemySpawner) return;

        gameObject.SetActive(false);
    }
}
