using System.Collections;
using Cinemachine;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class CharacterAttack : MonoBehaviour
{
    [Header("Stats")]
    public int currentHealth;
    public int maxHealth;
    public int baseAtk;
    public int defense;
    [SerializeField] private float mediumAtkMultiplier;
    [SerializeField] private float heavyAtkMultiplier;
    public int charAtk;
    [SerializeField] private int poise;
    public int poiseDamageLight, poiseDamageMedium, poiseDamageHeavy;
    public int isInvincible;
    public bool isPoison;
    public bool isIce;
    public bool isInvulnerable;
    public bool isDead;
    [SerializeField] private float stunDuration;
    [SerializeField] private float jumpAttackDrag;
    public int jumpAttackCount;
    private int _poiseBuildup;
    
    [Header("Combo Variables")]
    [SerializeField] private bool doCombosLoop;
    [SerializeField] private float animationCancelCooldown;
    [SerializeField] private float lightAttackForce, mediumAttackForce, heavyAttackForce;
    private float _rechargeTime;
    [SerializeField] private bool _canCancel = true;
    private bool _inputBuffer;
    private LightComboStep _lightComboStep = LightComboStep.None;
    private MediumComboStep _mediumComboStep = MediumComboStep.None;
    private HeavyComboStep _heavyComboStep = HeavyComboStep.None;
    private enum LightComboStep { None, Step1, Step2, Step3 }
    private enum MediumComboStep { None, Step1, Step2 }
    private enum HeavyComboStep { None, Step1 }
    
    [Header("Energy")]
    public float maxEnergy;
    public float currentEnergy;
    [SerializeField] private float rechargeSpeed;
    public float lightEnergyCost, mediumEnergyCost, heavyEnergyCost;
    private bool _isEnergyPaused;
    
    [Header("Knockback Types")]
    public Vector2 knockbackPowerLight;
    public Vector2 knockbackPowerMedium;
    public Vector2 knockbackPowerHeavy;
    
    [Header("References")]
    [SerializeField] private GameObject diedScreen;
    [SerializeField] private GameObject atkHitbox;
    [SerializeField] private Slider healthSlider, energySlider;
    [SerializeField] private Material defaultMaterial, hitMaterial;
    private SpriteRenderer _spriteRenderer;
    private CinemachineCollisionImpulseSource _impulseSource;
    private InventoryStore _inventoryStore;
    private Rigidbody _rigidbody;
    private GameObject _uiManager;
    private CharacterMovement _characterMovement;
    private MenuHandler _menuHandler;
    private PlayerStatus _playerStatus;
    private SettingManager _settingManager;
    public GameObject hitFlash;
    private EventInstance _enemyDamageEvent;
    private Coroutine _coyoteRoutine;
    
    [Header("Animation")]
    private Animator _playerAnimator;
    private static readonly int IsStaggered = Animator.StringToHash("isStaggered");
    private static readonly int IsJumpAttacking = Animator.StringToHash("isJumpAttacking");
    private static readonly int MediumAttack0 = Animator.StringToHash("mediumAttack0");
    private static readonly int MediumAttack1 = Animator.StringToHash("mediumAttack1");
    private static readonly int HeavyAttack0 = Animator.StringToHash("heavyAttack");
    private static readonly int LightAttack0 = Animator.StringToHash("lightAttack0");
    private static readonly int IsAttacking = Animator.StringToHash("isAttacking");
    private static readonly int IsDead = Animator.StringToHash("isDead");
    private static readonly int LightAttack1 = Animator.StringToHash("lightAttack1");
    private static readonly int LightAttack2 = Animator.StringToHash("lightAttack2");
    private static readonly int IsPlayerDead = Animator.StringToHash("isPlayerDead");

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
        _spriteRenderer = _rigidbody.gameObject.GetComponentInChildren<SpriteRenderer>();
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
        if (isDead || _characterMovement.uiOpen) return;
            
        if (ctx.performed && _characterMovement.grounded)
        {
            gameObject.layer = 13;
            
            if (currentEnergy < lightEnergyCost)
            {
                _inventoryStore.TriggerNotification(null, "Not enough energy.", false);
                return;
            }
            
            if (_mediumComboStep != MediumComboStep.None || _heavyComboStep != HeavyComboStep.None)
            {
                if (_canCancel)
                {
                    ResetCombo();
                    _canCancel = false;
                    StartCoroutine(AttackCancelCooldown());
                }
                else
                {
                    return;
                }
            }

            // Start of chain
            if (_lightComboStep == LightComboStep.None)
            {
                _lightComboStep = LightComboStep.Step1;
                _playerAnimator.SetTrigger(LightAttack0);
                _rigidbody.velocity = new Vector3(0f, _rigidbody.velocity.y, 0f);
                _playerAnimator.SetBool(IsAttacking, true);
                _characterMovement.isAttacking = true;
            }
            else
            {
                _inputBuffer = true;
            }
        }
        else if (ctx.performed && !_characterMovement.grounded && jumpAttackCount == 0)
        {
            gameObject.layer = 15;
            if (_coyoteRoutine != null) StopCoroutine(_coyoteRoutine);
            _coyoteRoutine = StartCoroutine(CoyoteTimer());
            jumpAttackCount++;
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
        while (elapsed < 0.3f)
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
        
        if (_lightComboStep != LightComboStep.None || _heavyComboStep != HeavyComboStep.None)
        {
            if (_canCancel)
            {
                ResetCombo();
                _canCancel = false;
                StartCoroutine(AttackCancelCooldown());
            }
            else
            {
                return;
            }
        }
        
        if (_mediumComboStep == MediumComboStep.None)
        {
            _mediumComboStep = MediumComboStep.Step1;
            _playerAnimator.SetTrigger(MediumAttack0);
            _rigidbody.velocity = new Vector3(0f, _rigidbody.velocity.y, 0f);
            _playerAnimator.SetBool(IsAttacking, true);
            _characterMovement.isAttacking = true;
        }
        else
        {
            _inputBuffer = true;
        }
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
        
        if (_lightComboStep != LightComboStep.None || _mediumComboStep != MediumComboStep.None)
        {
            if (_canCancel)
            {
                ResetCombo();
                _canCancel = false;
                StartCoroutine(AttackCancelCooldown());
            }
            else
            {
                return;
            }
        }
        
        // Start of chain
        if (_heavyComboStep == HeavyComboStep.None)
        {
            _heavyComboStep = HeavyComboStep.Step1;
            _playerAnimator.SetTrigger(HeavyAttack0);
            _rigidbody.velocity = new Vector3(0f, _rigidbody.velocity.y, 0f);
            _playerAnimator.SetBool(IsAttacking, true);
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
        if (_mediumComboStep != MediumComboStep.None || _heavyComboStep != HeavyComboStep.None) return;
        if (!_inputBuffer) { ResetCombo(); return; }
        
        _characterMovement.isAttacking = true;
        _rigidbody.velocity = new Vector3(0f, _rigidbody.velocity.y, 0f);
        _playerAnimator.SetBool(IsAttacking, true);
        _inputBuffer = false;

        switch (_lightComboStep)
        {
            case LightComboStep.Step1:
                gameObject.layer = 13;
                _lightComboStep = LightComboStep.Step2;
                _playerAnimator.SetTrigger(LightAttack1);
                break;
            case LightComboStep.Step2:
                gameObject.layer = 15;
                _lightComboStep = LightComboStep.Step3;
                _playerAnimator.SetTrigger(LightAttack2);
                break;
            case LightComboStep.Step3:
                gameObject.layer = 13;
                if (doCombosLoop)
                {
                    _lightComboStep = LightComboStep.Step1;
                    _playerAnimator.SetTrigger(LightAttack0);
                }
                else
                {
                    ResetCombo();
                }
                break;
        }
    }
    
    public void AdvanceMediumCombo()
    {
        if (_lightComboStep != LightComboStep.None || _heavyComboStep != HeavyComboStep.None) return;
        if (!_inputBuffer) { ResetCombo(); return; }
        
        gameObject.layer = 15;
        _characterMovement.isAttacking = true;
        _rigidbody.velocity = new Vector3(0f, _rigidbody.velocity.y, 0f);
        _playerAnimator.SetBool(IsAttacking, true);
        _inputBuffer = false;

        switch (_mediumComboStep)
        {
            case MediumComboStep.Step1:
                _mediumComboStep = MediumComboStep.Step2;
                _playerAnimator.SetTrigger(MediumAttack1);
                break;
            case MediumComboStep.Step2:
                if (doCombosLoop)
                {
                    _mediumComboStep = MediumComboStep.Step1;
                    _playerAnimator.SetTrigger(MediumAttack0);
                }
                else
                {
                    ResetCombo();
                }
                break;
        }
    }
    
    // for animation events (heavy attacks): if another input is made during an existing attack animation it is added to the input buffer and played next
    public void AdvanceHeavyCombo()
    {
        if (_lightComboStep != LightComboStep.None || _mediumComboStep != MediumComboStep.None) return;
        if (!_inputBuffer) { ResetCombo(); return; }

        gameObject.layer = 14;
        _characterMovement.isAttacking = true;
        _rigidbody.velocity = new Vector3(0f, _rigidbody.velocity.y, 0f);
        _playerAnimator.SetBool(IsAttacking, true);
        _inputBuffer = false;

        switch (_heavyComboStep)
        {
            case HeavyComboStep.Step1:
                if (doCombosLoop)
                {
                    _playerAnimator.SetTrigger(HeavyAttack0);
                }
                else
                {
                    ResetCombo();
                }
                break;
        }
    }

    private IEnumerator AttackCancelCooldown()
    {
        yield return new WaitForSeconds(animationCancelCooldown);
        _canCancel = true;
    }

    // if a combo is completed or cancelled then reset combos and stop animations
    public void ResetCombo()
    {
        _lightComboStep = LightComboStep.None;
        _mediumComboStep = MediumComboStep.None;
        _heavyComboStep = HeavyComboStep.None;
        _playerAnimator.ResetTrigger(LightAttack0);
        _playerAnimator.ResetTrigger(LightAttack1);
        _playerAnimator.ResetTrigger(LightAttack2);
        _playerAnimator.ResetTrigger(MediumAttack0);
        _playerAnimator.ResetTrigger(MediumAttack1);
        _playerAnimator.ResetTrigger(HeavyAttack0);
        atkHitbox.SetActive(false);
        _playerAnimator.SetBool(IsAttacking, false);
        _characterMovement.isAttacking = false;
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
        
        defense = Mathf.Clamp(defense, 0, 100);
        var dmgReduction = (100 - defense) / 100f;
        damage = Mathf.RoundToInt(damage * dmgReduction);
        
        var hitColor = (currentHealth - damage < currentHealth) ? Color.red : Color.green;
        if (hitColor == Color.red)
        {
            StartCoroutine(HitFlash());
        }

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
    
    private IEnumerator HitFlash()
    {
        _spriteRenderer.material = hitMaterial;
        yield return new WaitForSeconds(0.1f);
        _spriteRenderer.material = defaultMaterial;
    }
    
    public void UseEnergy(float amount)
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
        AudioManager.Instance.SetGlobalEventParameter("Music Track", 7);
        isDead = true;
        _playerAnimator.updateMode = AnimatorUpdateMode.UnscaledTime;
        _playerAnimator.SetBool(IsPlayerDead, true);
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

        if (jumpAttackCount > 0)
        {
            if (_characterMovement.grounded)
            {
                jumpAttackCount = 0;
            }
        }

        if (currentEnergy < maxEnergy)
        {
            _rechargeTime -= Time.deltaTime * rechargeSpeed;
            
            if (_rechargeTime <= 0)
            {
                UseEnergy(-1f);
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

