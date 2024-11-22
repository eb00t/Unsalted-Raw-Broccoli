using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimationEvents : MonoBehaviour
{
    CharacterAttack charAttack;
    CharacterMovement charMovement;
    void Start()
    {
        charAttack = GameObject.FindGameObjectWithTag("PlayerAttackBox").GetComponent<CharacterAttack>();
        charMovement = GameObject.FindGameObjectWithTag("Player").GetComponent<CharacterMovement>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void disableCollider()
    {
        charAttack.DisableCollider();
    }

    private void enableCollider()
    {
        charAttack.EnableCollider();
    }

    private void disablePlayerMovement()
    {
        charMovement.allowMovement = false;
    }

    private void enablePlayerMovement()
    {
        charMovement.allowMovement = true;
    }
}
