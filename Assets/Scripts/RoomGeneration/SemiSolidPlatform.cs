using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UIElements;

public class SemiSolidPlatform : MonoBehaviour
{
    private BoxCollider _boxCollider;
    public GameObject playerFeet;
    public CapsuleCollider playerCollider;
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
        playerCollider = playerFeet.GetComponentInParent<CapsuleCollider>();
    }


    private void TurnOffCollision(Collider col)
    {
        collisionOff = true;
        Physics.IgnoreCollision(col, _boxCollider, true);
    }
    private void TurnOnCollision(Collider col)
    {
        collisionOff = false;
        Physics.IgnoreCollision(col, _boxCollider, false);
    }
    
    private void Update()
    {
        if (_characterMovement.isCrouching && canDropThrough)
        {
            TurnOffCollision(playerCollider);
            
            if (SceneManager.GetActiveScene().name.Equals("Tutorial")) return;
            
            if (LevelBuilder.Instance.bossRoomGeneratingFinished)
            {
                gameObject.layer = LayerMask.NameToLayer("Ground");
            }
        }

        if (playerFeet.transform.position.y < transform.position.y && collisionOff == false)
        {
            TurnOffCollision(playerCollider);
            
            if (SceneManager.GetActiveScene().name.Equals("Tutorial")) return;
            
            if (LevelBuilder.Instance.bossRoomGeneratingFinished)
            {
                gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
            }
            
        }
        else if (playerFeet.transform.position.y > transform.position.y && canDropThrough == false && collisionOff)
        {
            TurnOnCollision(playerCollider);
            
            if (SceneManager.GetActiveScene().name.Equals("Tutorial")) return;
            
            if (LevelBuilder.Instance.bossRoomGeneratingFinished)
            {
                gameObject.layer = LayerMask.NameToLayer("Ground");
            }
        }
    }
}