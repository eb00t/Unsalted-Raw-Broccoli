using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using UnityEngine.UIElements;

public class SemiSolidPlatform : MonoBehaviour
{
    private BoxCollider _boxCollider;
    public GameObject playerFeet;
    public LayerMask _layerToIgnore;
    public bool collisionOff;
    public bool canDropThrough;
    private CharacterMovement _characterMovement;
    
    private void Awake()
    {
        _boxCollider = GetComponent<BoxCollider>();
        collisionOff = false;
        _layerToIgnore = LayerMask.GetMask("Player");
    }

    void Start()
    {
        if (playerFeet == null)
        {
            playerFeet = GameObject.FindWithTag("Player Ground Position");
        }
        _characterMovement = playerFeet.GetComponentInParent<CharacterMovement>();
    }


    public void TurnOffCollision(LayerMask layerMask)
    {
        collisionOff = true;
        _boxCollider.excludeLayers += layerMask;
    }
    public void TurnOnCollision(LayerMask layerMask)
    {
        collisionOff = false;
        _boxCollider.excludeLayers -= layerMask;
    }


    private void Update()
    {
        if (_characterMovement.isCrouching && canDropThrough)
        {
            _layerToIgnore = LayerMask.GetMask("Player");
            TurnOffCollision(_layerToIgnore);
            if (LevelBuilder.Instance.bossRoomGeneratingFinished)
            {
                gameObject.layer = LayerMask.NameToLayer("Ground");
            }
        }

        if (playerFeet.transform.position.y < transform.position.y && collisionOff == false)
        {
            TurnOffCollision(_layerToIgnore);
            if (LevelBuilder.Instance.bossRoomGeneratingFinished)
            {
                gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
            }
            
        }
        else if (playerFeet.transform.position.y > transform.position.y && canDropThrough == false && collisionOff)
        {
            TurnOnCollision(_layerToIgnore);
            if (LevelBuilder.Instance.bossRoomGeneratingFinished)
            {
                gameObject.layer = LayerMask.NameToLayer("Ground");
            }
        }
    }
}