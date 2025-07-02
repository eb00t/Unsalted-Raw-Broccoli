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
         var dir = other.transform.position - transform.position;
         
         if (dir.y < 0 && (!_characterMovement.isInUpThrust || _characterMovement._mostRecentUpthrust == this))
         {
            _characterMovement.isInUpThrust = true;
            _characterMovement.doubleJumpPerformed = true;
            _playerRb.velocity = new Vector3(0f, 1f * forceMultiplier, 0f);
            AudioManager.Instance.PlayOneShot(FMODEvents.Instance.Upthrust, transform.position);
         }
      }
   }
   
   private void OnTriggerExit(Collider other)
   {
      if (other.CompareTag("Player"))
      {
         _characterMovement._mostRecentUpthrust = this;
      }
   }

   private IEnumerator WaitToReset()
   {
      yield return new WaitForSeconds(1f);
      _characterMovement.isInUpThrust = false;
   }
}

