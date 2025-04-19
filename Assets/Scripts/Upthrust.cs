using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Upthrust : MonoBehaviour
{
   private GameObject _player;
   private CharacterMovement _characterMovement;
   private Rigidbody _playerRb;
   public float forceMultiplier;

   private void Start()
   {
      _player = GameObject.FindWithTag("Player");
      _playerRb = _player.GetComponent<Rigidbody>();
      _characterMovement = _player.GetComponent<CharacterMovement>();
   }

   private void OnTriggerEnter(Collider other)
   {
      if (other.CompareTag("Player"))
      {
         if (_playerRb.velocity.y >= 0f && !_characterMovement.isInUpThrust)
         {
            _playerRb.AddForce(Vector3.up * forceMultiplier, ForceMode.Impulse);
            _characterMovement.doubleJumpPerformed = true;
            _characterMovement.isInUpThrust = true;
         }
      }
   }
   
   private void OnTriggerExit(Collider other)
   {
      if (other.CompareTag("Player"))
      {
      }
   }

   private IEnumerator WaitToReset()
   {
      yield return new WaitForSeconds(1f);
      _characterMovement.isInUpThrust = false;
   }
}

