using FMOD.Studio;
using UnityEngine;

public class StaticLaser : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private int attack, poiseDamage;
    [SerializeField] private float laserTickCooldown;
    [SerializeField] private bool instantKill;
    [SerializeField] private Transform checkPoint;
    [SerializeField] private Transform bossEyePosition;

    private EventInstance _laserEvent;
    private LineRenderer _lineRenderer;
    private Collider _roomBounds;
    private float _lastDamageTime;
    private LayerMask _layerMask;
    private float _distance;
    private Vector3 _direction;

    private void Start()
    {
        _lineRenderer = GetComponentInChildren<LineRenderer>();
        _layerMask = LayerMask.GetMask("Player");
        _distance = Vector3.Distance(bossEyePosition.position, target.position);
        _direction = (target.position - bossEyePosition.position).normalized;
    }

    private void Update()
    {
        _lineRenderer.SetPosition(0, bossEyePosition.position);
        _lineRenderer.SetPosition(1, target.position);
        _lineRenderer.startWidth = 0.1f;
        _lineRenderer.endWidth = 0.05f;

        if (Physics.Raycast(bossEyePosition.position, _direction, out var hit, _distance, _layerMask))
        {
            if (hit.collider.CompareTag("Player"))
            {
                var characterAttack = hit.collider.GetComponentInChildren<CharacterAttack>();

                if (instantKill && !characterAttack.isInvulnerable)
                {
                    hit.collider.transform.position = checkPoint.position;
                }
                // makes sure player only takes damage at intervals
                else if (characterAttack != null && Time.time >= _lastDamageTime + laserTickCooldown) 
                {
                    characterAttack.TakeDamagePlayer(attack, poiseDamage, Vector3.zero);
                    _lastDamageTime = Time.time;
                }
            }
        }
    }
}
