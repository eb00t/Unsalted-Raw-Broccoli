using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UIElements;

public class SemiSolidPlatform : MonoBehaviour
{
    private BoxCollider _boxCollider;
    public GameObject playerFeet;
    public LayerMask _layerToIgnore;
    public bool collisionOff;
    public bool canDropThrough;
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
        if (Input.GetKeyDown(KeyCode.LeftControl) && canDropThrough)
        {
            _layerToIgnore = LayerMask.GetMask("Player");
            TurnOffCollision(_layerToIgnore);
        }

        if (playerFeet.transform.position.y < transform.position.y && collisionOff == false)
        {
            TurnOffCollision(_layerToIgnore);
        }
        else if (playerFeet.transform.position.y > transform.position.y && canDropThrough == false && collisionOff)
        {
            TurnOnCollision(_layerToIgnore);
        }
    }
}