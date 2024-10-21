using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimationEvents : MonoBehaviour
{
    CharacterAttack charAttack;
    void Start()
    {
        charAttack = GameObject.FindGameObjectWithTag("PlayerAttackBox").GetComponent<CharacterAttack>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void disableCollider()
    {
        charAttack.disableCollider();
    }

    private void enableCollider()
    {
        charAttack.enableCollider();
    }
}
