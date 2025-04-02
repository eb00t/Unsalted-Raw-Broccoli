using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Upthrust : MonoBehaviour
{
   private Rigidbody _playerRb;
   public float forceMultiplier;

   private void Start()
   {
      _playerRb = GameObject.FindWithTag("Player").GetComponent<Rigidbody>();
   }

   private void OnTriggerEnter(Collider other)
   {
      if (other.CompareTag("Player"))
      {
         if (_playerRb.velocity.y > 0.1f)
         {
            _playerRb.AddForce(Vector3.up * forceMultiplier, ForceMode.Impulse);
         }
      }
   }
}

