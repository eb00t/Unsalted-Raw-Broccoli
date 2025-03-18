using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class CharacterAttack : MonoBehaviour
{
    private static readonly int IsStaggered = Animator.StringToHash("isStaggered");
    private MeshCollider _attackCollider;
    private Animator _playerAnimator;
    private CharacterMovement _characterMovement;

    // Combo variables
    [Header("Combo Variables")]
    [SerializeField] private float timer = 0f;
    [SerializeField] private float timer1 = 0f;
    [SerializeField] private float heavyTimer = 0f;
    [SerializeField] private float heavyTimer1 = 0f;
    [SerializeField] private bool[] lightCombo = new bool[4];
    [SerializeField] private bool[] heavyCombo = new bool[4];
    [SerializeField] private float maxInputDelay = 10f;
    [HideInInspector] public bool animEnd;
    [SerializeField] private int heavyEnergyCost1, heavyEnergyCost2, heavyEnergyCost3;
    [SerializeField] private float rechargeSpeed;
    private float _rechargeTime;
    
    [Header("Stats")]
    public int currentHealth = 100;
    public int maxHealth = 100;
    public int maxEnergy;
    public int currentEnergy;
    public int baseAtk;
    public int charAtk = 10;
    [SerializeField] private int poise;
    public int poiseDamageLight, poiseDamageHeavy, poiseDamageMedium;
    public int isInvincible;
    private int _poiseBuildup;
    public bool isPoison;
    public bool isIce;
    public bool isInvulnerable;
    [SerializeField] private float stunDuration;
    
    [SerializeField] private Slider healthSlider, energySlider;
    [SerializeField] private GameObject diedScreen;
    private GameObject _uiManager;
    private MenuHandler _menuHandler;
    private PlayerStatus _playerStatus;
    public GameObject hitFlash;
    private EventInstance _enemyDamageEvent;

    [Header("Knockback Types")]
    public Vector2 knockbackPowerLight = new Vector2(10f, 1f);
    public Vector2 knockbackPowerMedium = new Vector2(0f, 0f);
    public Vector2 knockbackPowerHeavy = new Vector2(20f, 3f);
    private CinemachineCollisionImpulseSource _impulseSource;

    private void Start()
    {
        _characterMovement = transform.root.GetComponent<CharacterMovement>();
        _attackCollider = GetComponent<MeshCollider>();
        _playerAnimator = GameObject.FindGameObjectWithTag("PlayerRenderer").GetComponent<Animator>();
        _uiManager = GameObject.FindGameObjectWithTag("UIManager");
        _menuHandler = _uiManager.GetComponent<MenuHandler>();
        _playerStatus = _uiManager.GetComponent<PlayerStatus>();
        healthSlider.maxValue = maxHealth;
        energySlider.maxValue = maxEnergy;
        energySlider.value = currentEnergy;
        healthSlider.value = maxHealth;
        currentHealth = maxHealth;
        baseAtk = charAtk;
        hitFlash = GameObject.FindWithTag("Hit Flash");
        hitFlash.SetActive(false);
    }

    public void LightAttack(InputAction.CallbackContext ctx)
    {
        if (ctx.performed && _playerAnimator.GetBool("Grounded"))
        {
            _playerAnimator.SetBool("HeavyAttack", false);
            _playerAnimator.SetBool("HeavyAttack1", false);
            _playerAnimator.SetBool("HeavyAttack2", false);
            gameObject.layer = 13;
            //Debug.Log("LightAttack");
            //_playerAnimator.SetBool("LightAttack1", true);
            //InitiateAttack();
            animEnd = false;

            // Start of chain
            if (!lightCombo[0] && !_playerAnimator.GetBool("LightAttack2"))
            {
                Debug.Log("LightAttack");
                _playerAnimator.SetBool("LightAttack1", true);
            }

            if (lightCombo[0])
            {
                

                if (timer <= maxInputDelay && timer > 0f)
                {
                    Debug.Log("LightPunch");
                    _playerAnimator.SetBool("LightPunch", true);
                    
                    lightCombo[1] = true;
                }
            }

            if (lightCombo[1])
            {
                if (timer1 <= maxInputDelay && timer1 > 0f)
                {
                    Debug.Log("LightAttack2");
                    gameObject.layer = 15;
                    _playerAnimator.SetBool("LightAttack2", true);
                    if (animEnd)
                    {
                        lightCombo[2] = true;
                    }
                }
            }
            
            lightCombo[0] = true;
            if (lightCombo[2])
            {

                
                timer1 = 0f; lightCombo[1] = false;
                timer = 0f; lightCombo[0] = false;

                
                lightCombo[2] = false;
            }
        }
    }

    /*public void LightAttack1(InputAction.CallbackContext ctx)
    {
        if (ctx.performed && _playerAnimator.GetBool("Grounded"))
        {
            //Debug.Log("LightAttack1");
            //_playerAnimator.SetBool("LightAttack2", true);
            //InitiateAttack();

            if (lightCombo[1])
            {
                if (timer1 <= maxInputDelay && timer1 > 0f)
                {
                    lightCombo[3] = true;
                }
            }
            if (lightCombo[3])
            {
                timer1 = 0f; lightCombo[1] = false;
                timer = 0f; lightCombo[0] = false;

                _playerAnimator.StopPlayback();
                Debug.Log("LightAttack1");
                _playerAnimator.SetBool("LightAttack2", true);
                lightCombo[3] = false;
            }
        }
    }*/

    public void HeavyAttack(InputAction.CallbackContext ctx)
    {
        if (ctx.performed && _playerAnimator.GetBool("Grounded"))
        {
            _playerAnimator.SetBool("LightAttack1", false);
            _playerAnimator.SetBool("LightPunch", false);
            _playerAnimator.SetBool("LightAttack2", false);
            gameObject.layer = 14;
            //Debug.Log("LightAttack");
            //_playerAnimator.SetBool("LightAttack1", true);
            //InitiateAttack();
            animEnd = false;

            // Start of chain
            if (!heavyCombo[0] && !_playerAnimator.GetBool("HeavyAttack2"))
            {
                if (currentEnergy >= heavyEnergyCost1)
                {
                    Debug.Log("HeavyAttack");
                    _playerAnimator.SetBool("HeavyAttack", true);
                    UseEnergy(heavyEnergyCost1);
                }
            }

            if (heavyCombo[0])
            {
                if (heavyTimer <= maxInputDelay && heavyTimer > 0f)
                {
                    if (currentEnergy >= heavyEnergyCost2)
                    {
                        Debug.Log("HeavyAttack1");
                        _playerAnimator.SetBool("HeavyAttack1", true);
                        UseEnergy(heavyEnergyCost2);

                        heavyCombo[1] = true;
                    }
                }
            }

            if (heavyCombo[1])
            {
                if (heavyTimer1 <= maxInputDelay && heavyTimer1 > 0f)
                {
                    if (currentEnergy >= heavyEnergyCost3)
                    {
                        Debug.Log("HeavyAttack2");
                        _playerAnimator.SetBool("HeavyAttack3", true);
                        UseEnergy(heavyEnergyCost2);
                        
                        if (animEnd)
                        {
                            heavyCombo[2] = true;
                            
                        }
                    }
                }
            }

            heavyCombo[0] = true;
            if (heavyCombo[2])
            {
                heavyTimer1 = 0f; heavyCombo[1] = false;
                heavyTimer = 0f; heavyCombo[0] = false;


                heavyCombo[2] = false;
            }
        }
    }

    public void DisableCollider()
    {
        _attackCollider.enabled = false;
    }

    public void EnableCollider()
    {
        _attackCollider.enabled = true;
        Debug.Log("Enable collider");
    }

    public void TakeDamagePlayer(int damage, int poiseDmg)
    {
        if (isInvulnerable) return;
        
        if (isInvincible > 0)
        {
            isInvincible--;
            _playerStatus.UpdateStatuses(isInvincible);
            return;
        }

        if (currentHealth <= damage)
        {
            currentHealth = 0;
            healthSlider.value = 0;
            Die();
        }
        else
        {
            var hitColor = (currentHealth - damage < currentHealth) ? Color.red : Color.green;
            hitFlash.GetComponent<Image>().color = hitColor;
            hitFlash.SetActive(true);
            
            currentHealth -= damage;
            healthSlider.value = currentHealth;
        }
        
        _poiseBuildup += poiseDmg;
        
        if (_poiseBuildup >= poise)
        {
            StartCoroutine(StunTimer(stunDuration));
            _poiseBuildup = 0;
        }
    }
    
    public void UseEnergy(int amount)
    {
        if (currentEnergy <= amount)
        {
            currentEnergy = 0;
            energySlider.value = 0;
        }
        else if (currentEnergy - amount >= maxEnergy)
        {
            currentEnergy = maxEnergy;
        }
        else
        {
            currentEnergy -= amount;
        }
        
        energySlider.value = currentEnergy;
    }

    private IEnumerator StunTimer(float stunTime)
    {
        _playerAnimator.SetBool(IsStaggered, true);
        _characterMovement.allowMovement = false;
        _characterMovement.walkAllowed = false;
        
        yield return new WaitForSecondsRealtime(stunTime);
        
        _playerAnimator.SetBool(IsStaggered, false);
        _characterMovement.allowMovement = true;
        _characterMovement.walkAllowed = true;
    }
    
    private void Die()
    {
        diedScreen.SetActive(true);
    }

    private void OnTriggerEnter(Collider other) // gets takedamage and trigger status methods from all enemy types
    {
        if (!other.CompareTag("Enemy")) return;

        var damageable = other.GetComponentInParent<IDamageable>();
        if (damageable == null) return;
        
       
        //Layer = Light
        if (gameObject.layer == 13)
        {
            float randomTinyX = Random.Range(0.15f, 0.25f);
            float randomTinyY = Random.Range(-0.25f, 0.25f);
            damageable.TakeDamage(charAtk, poiseDamageLight, knockbackPowerLight);
            
            _enemyDamageEvent = AudioManager.Instance.CreateEventInstance(FMODEvents.Instance.EnemyDamage);
            _enemyDamageEvent.set3DAttributes(new Vector3(transform.position.x, transform.position.y, transform.position.z).To3DAttributes());
            _enemyDamageEvent.start();
            _enemyDamageEvent.release();
            
            if (_impulseSource != null)
            {
                _impulseSource.m_ImpulseDefinition.m_ImpulseDuration = 0.05f;
                _impulseSource.GenerateImpulseWithVelocity(new Vector3(randomTinyX, randomTinyY, 0));
            }
        }
        //Layer = Heavy
        if (gameObject.layer == 14)
        {
            float randomTinyX = Random.Range(1.5f, 2f);
            float randomTinyY = Random.Range(-1f, 1f);
            damageable.TakeDamage(charAtk, poiseDamageHeavy, knockbackPowerHeavy);
            _enemyDamageEvent = AudioManager.Instance.CreateEventInstance(FMODEvents.Instance.EnemyDamage);
            _enemyDamageEvent.set3DAttributes(new Vector3(transform.position.x, transform.position.y, transform.position.z).To3DAttributes());
            _enemyDamageEvent.start();
            _enemyDamageEvent.release();
            if (_impulseSource != null)
            {
                _impulseSource.m_ImpulseDefinition.m_ImpulseDuration = 0.2f;
                _impulseSource.GenerateImpulseWithVelocity(new Vector3(randomTinyX, randomTinyY, 0));
            }
        }
// Layer = Medium (final hit of light combo)
        if (gameObject.layer == 15)
        {
            float randomTinyX = Random.Range(0.5f, 1f);
            float randomTinyY = Random.Range(-0.25f, 0.25f);
            damageable.TakeDamage(charAtk, poiseDamageMedium, knockbackPowerMedium);
            
            _enemyDamageEvent = AudioManager.Instance.CreateEventInstance(FMODEvents.Instance.EnemyDamage);
            _enemyDamageEvent.set3DAttributes(new Vector3(transform.position.x, transform.position.y, transform.position.z).To3DAttributes());
            _enemyDamageEvent.start();
            _enemyDamageEvent.release();
            
            if (_impulseSource != null)
            {
                _impulseSource.m_ImpulseDefinition.m_ImpulseDuration = 0.1f;
                _impulseSource.GenerateImpulseWithVelocity(new Vector3(randomTinyX, randomTinyY, 0));
            }
        }
        if (isPoison)
        {
            damageable.TriggerStatusEffect(ConsumableEffect.Poison);
        }
        else if (isIce && Random.Range(0, 10) <= 2) // hits have 30% chance to trigger ice effect
        {
            damageable.TriggerStatusEffect(ConsumableEffect.Ice);
        }
    }

    private void Update()
    {
        if (currentEnergy < maxEnergy)
        {
            _rechargeTime -= Time.deltaTime * rechargeSpeed;
            
            if (_rechargeTime <= 0)
            {
                UseEnergy(-1);
                _rechargeTime = 1f;
            }
        }

        if (lightCombo[0])
        {
            if (!lightCombo[1])
            {
                timer += 1f * Time.deltaTime;
            }
            if (lightCombo[1])
            {
                timer1 += 1f * Time.deltaTime;
                if(timer1 >= maxInputDelay)
                {
                    lightCombo[0] = false;
                    lightCombo[1] = false;
                    timer = 0f;
                    timer1 = 0f;
                }
            }
            if(timer >= maxInputDelay && !lightCombo[1])
            {
                lightCombo[0] = false;
                timer = 0f;
            }
        }

        if (heavyCombo[0])
        {
            if (!heavyCombo[1])
            {
                heavyTimer += 1f * Time.deltaTime;
            }
            if (heavyCombo[1])
            {
                heavyTimer1 += 1f * Time.deltaTime;
                if (heavyTimer1 >= maxInputDelay)
                {
                    heavyCombo[0] = false;
                    heavyCombo[1] = false;
                    heavyTimer = 0f;
                    heavyTimer1 = 0f;
                }
            }
            if (heavyTimer >= maxInputDelay && !heavyCombo[1])
            {
                heavyCombo[0] = false;
                heavyTimer = 0f;
            }
        }
    }
}

