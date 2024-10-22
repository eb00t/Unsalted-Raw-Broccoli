using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CharacterAttack : MonoBehaviour
{
    MeshCollider attackCollider;
    Animator PlayerAnimator;

    public void LightAttack(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            Debug.Log("LightAttack");
            PlayerAnimator.SetBool("LightAttack1", true);
            
        }
        
    }

    public void LightAttack1(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            Debug.Log("LightAttack1");
            PlayerAnimator.SetBool("LightAttack2", true);
            
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        attackCollider = GetComponent<MeshCollider>();
        PlayerAnimator = GameObject.FindGameObjectWithTag("PlayerRenderer").GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void disableCollider()
    {
        attackCollider.enabled = false;
    }

    public void enableCollider()
    {
        attackCollider.enabled = true;
        Debug.Log("Enable collider");
    }
}
