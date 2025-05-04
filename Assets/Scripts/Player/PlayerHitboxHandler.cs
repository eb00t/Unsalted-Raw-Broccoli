using UnityEngine;

public class PlayerHitboxHandler : MonoBehaviour
{
    [SerializeField] private CharacterAttack characterAttack;
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
                var rot = sign == 1 ? 90f : -90f;
                var vfx = Instantiate(impactVFX, impactOrigin.position, Quaternion.Euler(0f, 0f, rot));
                vfx.transform.localScale = new Vector3(Mathf.Abs(vfx.transform.localScale.x) * sign, vfx.transform.localScale.y, vfx.transform.localScale.z);
            }
        }
    }
}
