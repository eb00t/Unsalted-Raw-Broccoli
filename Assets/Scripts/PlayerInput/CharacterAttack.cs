using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CharacterAttack : MonoBehaviour
{
    MeshCollider attackCollider;

    public void LightAttack(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            attackCollider.enabled = true;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        attackCollider = GetComponent<MeshCollider>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
