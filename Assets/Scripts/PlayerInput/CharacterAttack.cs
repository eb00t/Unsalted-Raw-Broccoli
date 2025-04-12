using System;
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
    private static readonly int IsJumpAttacking = Animator.StringToHash("isJumpAttacking");
    private static readonly int MediumAttack0 = Animator.StringToHash("mediumAttack");
    private static readonly int HeavyAttack0 = Animator.StringToHash("heavyAttack0");
    private static readonly int LightAttack0 = Animator.StringToHash("lightAttack0");
    private static readonly int IsAttacking = Animator.StringToHash("isAttacking");
    private static readonly int HeavyAttack1 = Animator.StringToHash("heavyAttack1");
    private static readonly int IsDead = Animator.StringToHash("isDead");
    private static readonly int LightAttack1 = Animator.StringToHash("lightAttack1");
    private static readonly int LightAttack2 = Animator.StringToHash("lightAttack2");
    private Animator _playerAnimator;
    private CharacterMovement _characterMovement;

    // Combo variables
    [Header("Combo Variables")]
    public int lightEnergyCost, mediumEnergyCost, heavyEnergyCost;
    [SerializeField] private float rechargeSpeed;
    private float _rechargeTime;
    private enum LightComboStep { None, Step1, Step2, Step3 }
    private enum MediumComboStep { None, Step1 }
    private enum HeavyComboStep { None, Step1, Step2 }
    private LightComboStep _lightComboStep = LightComboStep.None;
    private MediumComboStep _mediumComboStep = MediumComboStep.None;
    private HeavyComboStep _heavyComboStep = HeavyComboStep.None;
    private bool _inputBuffer;
    private float _comboTimer;
    [SerializeField] private float comboResetTime = 1f;
    [SerializeField] private float lightAttackForce, mediumAttackForce, heavyAttackForce;
    
    [Header("Stats")]
    public int currentHealth;
    public int maxHealth;
    public int maxEnergy;
    public int currentEnergy;
    public int baseAtk;
    [SerializeField] private float mediumAtkMultiplier;
    [SerializeField] private float heavyAtkMultiplier;
    public int charAtk;
    [SerializeField] private int poise;
    public int poiseDamageLight, poiseDamageMedium, poiseDamageHeavy;
    public int isInvincible;
    private int _poiseBuildup;
    public bool isPoison;
    public bool isIce;
    public bool isInvulnerable;
    public bool isDead;
    [SerializeField] private float stunDuration;
    
    [SerializeField] private Slider healthSlider, energySlider;
    [SerializeField] private GameObject diedScreen;
    private GameObject _uiManager;
    private MenuHandler _menuHandler;
    private PlayerStatus _playerStatus;
    public GameObject hitFlash;
    private EventInstance _enemyDamageEvent;
    private SettingManager _settingManager;
    public int _jumpAttackCount;
    
    [Header("Knockback Types")]
    public Vector2 knockbackPowerLight;
    public Vector2 knockbackPowerMedium;
    public Vector2 knockbackPowerHeavy;
    private CinemachineCollisionImpulseSource _impulseSource;
    private InventoryStore _inventoryStore;
    private Coroutine _coyoteRoutine;
    private Rigidbody _rigidbody;
    [SerializeField] private float jumpAttackDrag;

    private void Start()
    {
        _characterMovement = transform.root.GetComponent<CharacterMovement>();
        _playerAnimator = GameObject.FindGameObjectWithTag("PlayerRenderer").GetComponent<Animator>();
        _settingManager = GameObject.Find("Settings").GetComponent<SettingManager>();
        _uiManager = GameObject.FindGameObjectWithTag("UIManager");
        _menuHandler = _uiManager.GetComponent<MenuHandler>();
        _playerStatus = _uiManager.GetComponent<PlayerStatus>();
        _inventoryStore = _menuHandler.GetComponent<InventoryStore>();
        _rigidbody = GetComponentInParent<Rigidbody>();
        healthSlider.maxValue = maxHealth;
        energySlider.maxValue = maxEnergy;
        energySlider.value = currentEnergy;
        healthSlider.value = maxHealth;
        currentHealth = maxHealth;
        charAtk = baseAtk;
        hitFlash = GameObject.FindWithTag("Hit Flash");
        hitFlash.SetActive(false);
        _impulseSource = GetComponent<CinemachineCollisionImpulseSource>();
    }
    
    public void LightAttack(InputAction.CallbackContext ctx)
    {
        if (isDead) return;
            
        if (ctx.performed && _characterMovement.grounded)
        {
            gameObject.layer = 13;
            
            if (currentEnergy < lightEnergyCost)
            {
                _inventoryStore.TriggerNotification(null, "Not enough energy.", false);
                return;
            }

            // Start of chain
            if (_lightComboStep == LightComboStep.None)
            {
                _lightComboStep = LightComboStep.Step1;
                _comboTimer = comboResetTime;
                _playerAnimator.SetTrigger(LightAttack0);
                _rigidbody.velocity = new Vector3(0f, _rigidbody.velocity.y, 0f);
                _characterMovement.isAttacking = true;
            }
            else
            {
                _inputBuffer = true;
            }
        }
        else if (ctx.performed && !_characterMovement.grounded && _jumpAttackCount == 0)
        {
            gameObject.layer = 15;
            if (_coyoteRoutine != null) StopCoroutine(_coyoteRoutine);
            _coyoteRoutine = StartCoroutine(CoyoteTimer());
            _jumpAttackCount++;
        }
    }

    // holds player in air briefly while triggering a jump attack, cancelled if player hits ground
    private IEnumerator CoyoteTimer()
    {
        _characterMovement.isJumpAttacking = true;
        _playerAnimator.SetBool(IsJumpAttacking, true);
        _rigidbody.drag = jumpAttackDrag;

        yield return null;

        var elapsed = 0f;
        while (elapsed < 0.25f)
        {
            if (!_playerAnimator.GetBool(IsJumpAttacking) || _characterMovement.grounded)
            {
                break;
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        _characterMovement.isJumpAttacking = false;
        _playerAnimator.SetBool(IsJumpAttacking, false);
        _rigidbody.drag = 0f;
    }

    public void MediumAttack(InputAction.CallbackContext ctx)
    {
        if (isDead || _characterMovement.uiOpen) return;
        if (!ctx.performed || _characterMovement.isJumpAttacking || !_characterMovement.grounded) return;
        
        gameObject.layer = 15;
        
        if (currentEnergy < mediumEnergyCost)
        {
            _inventoryStore.TriggerNotification(null, "Not enough energy.", false);
            return;
        }
        
        _mediumComboStep = MediumComboStep.Step1;
        _comboTimer = comboResetTime;
        _playerAnimator.SetTrigger(MediumAttack0);
        _rigidbody.velocity = new Vector3(0f, _rigidbody.velocity.y, 0f);
        _characterMovement.isAttacking = true;
    }

    public void HeavyAttack(InputAction.CallbackContext ctx)
    {
        if (isDead || _characterMovement.uiOpen) return;
        if (!ctx.performed || _characterMovement.isJumpAttacking || !_characterMovement.grounded) return;
        
        gameObject.layer = 14;
        
        if (currentEnergy < heavyEnergyCost)
        {
            _inventoryStore.TriggerNotification(null, "Not enough energy.", false);
            return;
        }
        
        // Start of chain
        if (_heavyComboStep == HeavyComboStep.None)
        {
            _heavyComboStep = HeavyComboStep.Step1;
            _comboTimer = comboResetTime;
            _playerAnimator.SetTrigger(HeavyAttack0);
            _rigidbody.velocity = new Vector3(0f, _rigidbody.velocity.y, 0f);
            _characterMovement.isAttacking = true;
        }
        else
        {
            _inputBuffer = true;
        }
    }
    
    // for animation events (light attacks): if another input is made during an existing attack animation it is added to the input buffer and played next
    public void AdvanceLightCombo()
    {
        if (!_inputBuffer)
        {
            ResetCombo();
            return;
        }
        
        gameObject.layer = 15;
        _characterMovement.isAttacking = true;
        _rigidbody.velocity = new Vector3(0f, _rigidbody.velocity.y, 0f);
        _playerAnimator.SetBool(IsAttacking, true);
        _inputBuffer = false;
        _comboTimer = comboResetTime;

        switch (_lightComboStep)
        {
            case LightComboStep.Step1:
                _lightComboStep = LightComboStep.Step2;
                _playerAnimator.SetTrigger(LightAttack1);
                break;
            case LightComboStep.Step2:
                _lightComboStep = LightComboStep.Step3;
                _playerAnimator.SetTrigger(LightAttack2);
                break;
            case LightComboStep.Step3:
                ResetCombo();
                break;
        }
    }
    
    // for animation events (heavy attacks): if another input is made during an existing attack animation it is added to the input buffer and played next
    public void AdvanceHeavyCombo()
    {
        if (!_inputBuffer)
        {
            ResetCombo();
            return;
        }

        gameObject.layer = 14;
        _characterMovement.isAttacking = true;
        _rigidbody.velocity = new Vector3(0f, _rigidbody.velocity.y, 0f);
        _playerAnimator.SetBool(IsAttacking, true);
        _inputBuffer = false;
        _comboTimer = comboResetTime;

        switch (_heavyComboStep)
        {
            case HeavyComboStep.Step1:
                _heavyComboStep = HeavyComboStep.Step2;
                _playerAnimator.SetTrigger(HeavyAttack1);
                break;
            case HeavyComboStep.Step2:
                ResetCombo();
                break;
        }
    }

    // if a combo is completed or cancelled then
    private void ResetCombo()
    {
        _lightComboStep = LightComboStep.None;
        _mediumComboStep = MediumComboStep.None;
        _heavyComboStep = HeavyComboStep.None;
        _characterMovement.isAttacking = false;
        _playerAnimator.SetBool(IsAttacking, false);
        _comboTimer = 0f;
        _inputBuffer = false;
    }
    
    public void AttackForce(int LMH)
    {
        var force = 0f;
        switch (LMH)
        {
            case 0:
                force = lightAttackForce;
                break;
            case 1:
                force = mediumAttackForce;
                break;
            case 2:
                force = heavyAttackForce;
                break;
        }

        var dir = Mathf.Sign(transform.root.localScale.x);
        _rigidbody.velocity = new Vector3(dir * force, _rigidbody.velocity.y, 0f);
        StartCoroutine(StopAttackForce());
    }
    
    private IEnumerator StopAttackForce()
    {
        yield return new WaitForSeconds(0.1f);
        _rigidbody.velocity = new Vector3(0f, _rigidbody.velocity.y, 0f);
    }

    public void TakeDamagePlayer(int damage, int poiseDmg)
    {
        if (isDead || _characterMovement.uiOpen) return;
        if (isInvulnerable) return;
        
        if (isInvincible > 0)
        {
            isInvincible--;
            _playerStatus.UpdateStatuses(isInvincible);
            return;
        }
        
        var hitColor = (currentHealth - damage < currentHealth) ? Color.red : Color.green;

        if (currentHealth <= damage)
        {
            currentHealth = 0;
            healthSlider.value = 0;
            Die();
        }
        else if (currentHealth - damage > maxHealth)
        {
            currentHealth = maxHealth;
        }
        else
        {
            hitFlash.GetComponent<Image>().color = hitColor;
            hitFlash.SetActive(true);
            
            currentHealth -= damage;
        }
        
        healthSlider.value = currentHealth;
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
        if (isDead) return;
        isDead = true;
        _playerAnimator.updateMode = AnimatorUpdateMode.UnscaledTime;
        _playerAnimator.SetTrigger(IsDead);
        if (!diedScreen.activeSelf)
        {
            Time.timeScale = 0.2f;
        }
    }

    public void AttackHit(Collider other) // gets takedamage and trigger status methods from all enemy types
    {
        if (isDead) return;

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
                _impulseSource.GenerateImpulseWithVelocity(new Vector3(randomTinyX, randomTinyY, 0) * _settingManager.screenShakeMultiplier);
            }
        }
        
        // Layer = Medium (final hit of light combo)
        if (gameObject.layer == 15)
        {
            float randomTinyX = Random.Range(0.5f, 1f);
            float randomTinyY = Random.Range(-0.25f, 0.25f);
            var calcAtk = (charAtk - baseAtk) + (baseAtk * mediumAtkMultiplier);
            damageable.TakeDamage((int)calcAtk, poiseDamageMedium, knockbackPowerMedium);
            _enemyDamageEvent = AudioManager.Instance.CreateEventInstance(FMODEvents.Instance.EnemyDamage);
            _enemyDamageEvent.set3DAttributes(new Vector3(transform.position.x, transform.position.y, transform.position.z).To3DAttributes());
            _enemyDamageEvent.start();
            _enemyDamageEvent.release();
            
            if (_impulseSource != null)
            {
                _impulseSource.m_ImpulseDefinition.m_ImpulseDuration = 0.1f;
                _impulseSource.GenerateImpulseWithVelocity(new Vector3(randomTinyX, randomTinyY, 0) * _settingManager.screenShakeMultiplier);
            }
        }
        
        //Layer = Heavy
        if (gameObject.layer == 14)
        {
            float randomTinyX = Random.Range(1.5f, 2f);
            float randomTinyY = Random.Range(-1f, 1f);
            var calcAtk = (charAtk - baseAtk) + (baseAtk * heavyAtkMultiplier);
            damageable.TakeDamage((int)calcAtk, poiseDamageHeavy, knockbackPowerHeavy);
            _enemyDamageEvent = AudioManager.Instance.CreateEventInstance(FMODEvents.Instance.EnemyDamage);
            _enemyDamageEvent.set3DAttributes(new Vector3(transform.position.x, transform.position.y, transform.position.z).To3DAttributes());
            _enemyDamageEvent.start();
            _enemyDamageEvent.release();
            if (_impulseSource != null)
            {
                _impulseSource.m_ImpulseDefinition.m_ImpulseDuration = 0.2f;
                _impulseSource.GenerateImpulseWithVelocity(new Vector3(randomTinyX, randomTinyY, 0) * _settingManager.screenShakeMultiplier);
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
        if (isDead || _characterMovement.uiOpen) return;
        
        // if this timer reaches 0 then reset combo so it starts from attack step 1
        if (_comboTimer > 0f)
        {
            _comboTimer -= Time.deltaTime;

            if (_comboTimer <= 0f)
            {
                ResetCombo();
            }
        }

        if (_jumpAttackCount > 0)
        {
            if (_characterMovement.grounded)
            {
                _jumpAttackCount = 0;
            }
        }

        if (currentEnergy < maxEnergy)
        {
            _rechargeTime -= Time.deltaTime * rechargeSpeed;
            
            if (_rechargeTime <= 0)
            {
                UseEnergy(-1);
                _rechargeTime = 1f;
            }
        }
        
        /*
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
        */
    }
}

