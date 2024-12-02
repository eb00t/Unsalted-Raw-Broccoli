using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class SemiSolidPlatform : MonoBehaviour
{
    private BoxCollider _boxCollider;
    private LayerMask _layerToIgnore;

    public bool canFall;
    private bool FallThrough;
    private Rigidbody rigid;
    public int fallSpeed;
    private void Awake()
    {
        _boxCollider = GetComponent<BoxCollider>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            _layerToIgnore = LayerMask.GetMask("Player");
        }
        else if (other.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            _layerToIgnore = LayerMask.GetMask("Enemy");
        }
        TurnOffCollision(_layerToIgnore);
    }

    private void OnTriggerStay(Collider other)
    {
        if (canFall)
        {
            if (FallThrough && other.gameObject.layer == LayerMask.NameToLayer("Player"))
            {
                 TurnOffCollision(_layerToIgnore);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            _layerToIgnore = LayerMask.GetMask("Player");
        }
        else if (other.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            _layerToIgnore = LayerMask.GetMask("Enemy");
        }
        TurnOnCollision(_layerToIgnore);
        FallThrough = true;
    }
    private void OnCollisionEnter(Collision collision)
    {
        rigid = collision.rigidbody.GetComponent<Rigidbody>();
        if (canFall)
        {
            if (FallThrough && collision.gameObject.layer == LayerMask.NameToLayer("Player"))
            {
                rigid.drag = fallSpeed;
            }
            else
            {
                rigid.drag = 0;
                FallThrough = false;
                TurnOnCollision(_layerToIgnore);
            }
            TurnOffCollision(_layerToIgnore);
        }

        else if (canFall == false)
        {
            TurnOnCollision(_layerToIgnore);
            rigid.drag = 0;
        }
        
    }
    
    private void OnCollisionExit(Collision collisionExit)
    {
        rigid = collisionExit.rigidbody.GetComponent<Rigidbody>();
        
          if (FallThrough == false &&  collisionExit.gameObject.layer == LayerMask.NameToLayer("Player"))
            {
                rigid.drag = 0;
            }
            else
            {
                FallThrough = true;
            } 
            FallThrough = false;
            TurnOnCollision(_layerToIgnore);

    }
    public void TurnOffCollision(LayerMask layerMask)
    {
        _boxCollider.excludeLayers += layerMask;
    }

    public void TurnOnCollision(LayerMask layerMask)
    {
        _boxCollider.excludeLayers -= layerMask;
    }

    
}
