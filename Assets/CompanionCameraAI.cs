using FMOD.Studio;
using Pathfinding;
using UnityEngine;
using UnityEngine.UI;


public class CompanionCameraAI : MonoBehaviour
{
    [Header("Offensive Stats")] 
    public int attack;
    public int poiseDamage;
    [SerializeField] private int numberOfAttacks;
    [SerializeField] private float projectileSpeed;
    [SerializeField] private float projectileRotSpeed;
    [SerializeField] private float multiProjectileOffset;

    [Header("Tracking")] 
    [SerializeField] private float attackRange;
    [SerializeField] private float playerTrackDistance;
    private bool _hasPlayerBeenSeen;
    private Vector3 _playerDir;
    
    [Header("Timing")]
    [SerializeField] private float attackCooldown;
    private float _attackTimer;
    
    [Header("Enemy Properties")]
    [SerializeField] private float moveSpeed;
    [SerializeField] private bool doesHaveProjectileAttack;
    [SerializeField] private float numberOfProjectiles;
    
    [Header("References")] 
    [SerializeField] private GameObject lightProjectile;
    [SerializeField] private Transform projectileOrigin;
    private Animator _animator;
    private Transform _player;
    private CharacterAttack _characterAttack;
    private SpriteRenderer _spriteRenderer;
    private Transform _spriteTransform;
    private BoxCollider _collider;
    private Rigidbody _rigidbody;
    private LockOnController _lockOnController;
    private Transform _target;
    
    [Header("Sound")]
    private EventInstance _alarmEvent;
    private EventInstance _deathEvent;
    private EventInstance _laserEvent;
    private bool _canAttack = true;
    private LineRenderer _lineRenderer;
    [SerializeField] private Transform bossEyePosition;

    private void Start()
    {
        _animator = GetComponent<Animator>();
        _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        _spriteTransform = _spriteRenderer.transform;
        _player = GameObject.FindGameObjectWithTag("Player").transform;
        _target = GameObject.FindGameObjectWithTag("CompanionTarget").transform;
        var newPos = new Vector3(_target.position.x, _target.position.y + 2, _target.position.z);
        transform.position = newPos;
        _lockOnController = _player.GetComponent<LockOnController>();
        _rigidbody = GetComponent<Rigidbody>();
        _collider = GetComponent<BoxCollider>();
        DisablePlatformCollisions();
    }

    private void Update()
    {
        var distance = Vector3.Distance(transform.position, _target.position);
        _playerDir = _player.position - transform.position;

        _attackTimer -= Time.deltaTime;

        if (distance > playerTrackDistance)
        {
            UpdateSpriteDirection(_playerDir.x < 0 );
            Chase();
        }
        else
        {
            UpdateSpriteDirection(_player.transform.localScale.x < 0);
        }

        if (_attackTimer <= 0)
        {
            var nearestTarget = _lockOnController.FindNearestTarget(attackRange);
            if (nearestTarget != null)
            {
                _attackTimer = attackCooldown;
                Attack(nearestTarget);
            }
        }
    }

    private void Chase()
    {
        var dir = (_target.position - transform.position).normalized;
        var newPos = transform.position + dir * (moveSpeed * Time.deltaTime);
        _rigidbody.MovePosition(newPos);
    }

    private void Attack(Transform nearestTarget)
    {
        //_rigidbody.velocity = Vector3.zero;
        ProjectileAttack(nearestTarget);
    }

    public void ProjectileAttack(Transform nearestTarget)
    {
        _canAttack = false;
        var centre = numberOfProjectiles / 2;
        var dir = (nearestTarget.position - transform.position).normalized;

        for (var i = 0; i < numberOfProjectiles; i++)
        {
            var position = projectileOrigin.position;
            var newProjectile = Instantiate(lightProjectile, position, Quaternion.identity);
            var hitbox = newProjectile.GetComponent<CompanionCameraHitbox>();
            hitbox.target = nearestTarget;
            hitbox.rotSpeed = projectileRotSpeed;
            hitbox.speed = projectileSpeed;
            hitbox.companionCameraAI = this;
            newProjectile.SetActive(true);
            
            var rb = newProjectile.GetComponent<Rigidbody>();
            var centreOffset = i - centre;
            var rotation = Quaternion.AngleAxis(centreOffset * multiProjectileOffset, Vector3.forward);
            var angledDir = rotation * dir;
            
            var newRot = Mathf.Atan2(angledDir.y, angledDir.x) * Mathf.Rad2Deg;
            rb.rotation = Quaternion.Euler(0, 0, newRot);
        }
        
        AudioManager.Instance.PlayOneShot(FMODEvents.Instance.FlyingEnemyShoot, transform.position);
        
        _canAttack = true;
    }
    
    private void UpdateSpriteDirection(bool isLeft)
    {
        var localScale = _spriteTransform.localScale;
        
        if (!isLeft)
        {
            localScale = new Vector3(Mathf.Abs(localScale.x), localScale.y, localScale.z);
        }
        else
        {
            localScale = new Vector3(-Mathf.Abs(localScale.x), localScale.y, localScale.z);
        }

        _spriteTransform.localScale = localScale;
    }

    private void DisablePlatformCollisions()
    {
        var platforms = gameObject.transform.root.GetComponentsInChildren<SemiSolidPlatform>();
        
        foreach (var semiSolidPlatform in platforms)
        {
            var collider2 = semiSolidPlatform.GetComponent<Collider>();
            Physics.IgnoreCollision(_collider, collider2, true);
        }
    }
}
