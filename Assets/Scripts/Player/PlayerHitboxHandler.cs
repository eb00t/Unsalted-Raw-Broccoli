using UnityEngine;

public class PlayerHitboxHandler : MonoBehaviour
{
    [SerializeField] private CharacterAttack characterAttack;
    [SerializeField] private GameObject lightImpactVFX;
    [SerializeField] private GameObject impactVFX;
    [SerializeField] private Transform impactOrigin;
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            characterAttack.AttackHit(other);
            
            if (other.gameObject.layer == 11)
            {
                var sign = Mathf.Sign(GetComponentInParent<CharacterMovement>().transform.localScale.x);
                var rot = sign == 1 ? 0f : 180f;
                var vfx = Instantiate(lightImpactVFX, impactOrigin.position, Quaternion.Euler(0f, 0f, rot));
                
                if (characterAttack.gameObject.layer == 13) // light
                {
                    vfx.transform.localScale = new Vector3(Mathf.Abs(vfx.transform.localScale.x) * sign * 1f, vfx.transform.localScale.y * 1f, vfx.transform.localScale.z);
                }
                else if (characterAttack.gameObject.layer == 14) // heavy
                {
                    vfx.transform.localScale = new Vector3(Mathf.Abs(vfx.transform.localScale.x) * sign * 3f, vfx.transform.localScale.y * 3f, vfx.transform.localScale.z);
                }
                else if (characterAttack.gameObject.layer == 15) // medium
                {
                    vfx.transform.localScale = new Vector3(Mathf.Abs(vfx.transform.localScale.x) * sign * 2f, vfx.transform.localScale.y * 2f, vfx.transform.localScale.z);
                }
            }
        }
    }
}
