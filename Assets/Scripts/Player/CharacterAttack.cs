using System.Collections;
using Cinemachine;
using DG.Tweening;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class CharacterAttack : MonoBehaviour
{
    [Header("Stats")]
    //public int currentHealth;
    //public int maxHealth;
    //public int baseAtk;
    //public int defense;
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
    [SerializeField] private int _poiseBuildup;
    [SerializeField] private float hitIframeDur;
    private bool _hasHitIframes;
    [SerializeField] private int poiseRecoveryAmount;
    [SerializeField] private float poiseRecoveryDelay;
    private float _poiseRecoverTimer;
    
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
    [SerializeField] private Slider healthSlider, energySlider, healthChangeSlider, energyChangeSlider;
    [SerializeField] private Material defaultMaterial, hitMaterial;
    private SpriteRenderer _spriteRenderer;
    private CinemachineImpulseSource _impulseSource;
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
    private PassiveItemHandler _passiveItemHandler;
    [SerializeField] private DataHolder dataHolder;
    private Coroutine _knockbackRoutine;
    [SerializeField] private Animator evilAnimator;
    private Coroutine _vibration;
    private Image _healthChangeImg;
    private Image _energyChangeImg;
    private Image _hitFlashImg;
    private Tween _healthTween;
    private Tween _energyTween;
    private Coroutine _stunRoutine;
    
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
    private static readonly int Laugh = Animator.StringToHash("Laugh");
    private static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");
    private static readonly int StanceBroken = Animator.StringToHash("stanceBroken");

    private void Start()
    {
        _hitFlashImg = hitFlash.GetComponent<Image>();
        _characterMovement = transform.root.GetComponent<CharacterMovement>();
        _playerAnimator = GameObject.FindGameObjectWithTag("PlayerRenderer").GetComponent<Animator>();
        _settingManager = GameObject.Find("Settings").GetComponent<SettingManager>();
        _uiManager = GameObject.FindGameObjectWithTag("UIManager");
        _menuHandler = _uiManager.GetComponent<MenuHandler>();
        _playerStatus = _uiManager.GetComponent<PlayerStatus>();
        _inventoryStore = _menuHandler.GetComponent<InventoryStore>();
        _rigidbody = GetComponentInParent<Rigidbody>();
        _spriteRenderer = _rigidbody.gameObject.GetComponentInChildren<SpriteRenderer>();
        _passiveItemHandler = _uiManager.GetComponent<PassiveItemHandler>();
        healthChangeSlider = FindSliderExcludingSelf(healthSlider.gameObject);
        energyChangeSlider = FindSliderExcludingSelf(energySlider.gameObject);
        _healthChangeImg = healthChangeSlider.fillRect.GetComponent<Image>();
        _energyChangeImg = energyChangeSlider.fillRect.GetComponent<Image>();
        _poiseRecoverTimer = poiseRecoveryDelay;
        
        if (!dataHolder.hardcoreMode)
        {
            dataHolder.playerHealth = dataHolder.playerMaxHealth;
        }
        else if (dataHolder.hardcoreMode && dataHolder.currentLevel == LevelBuilder.LevelMode.Floor1)
        {
            dataHolder.playerHealth = dataHolder.playerMaxHealth;
        }

        healthSlider.maxValue = dataHolder.playerMaxHealth;
        energySlider.maxValue = maxEnergy;
        energySlider.value = currentEnergy;
        healthSlider.value = dataHolder.playerHealth;
        healthChangeSlider.maxValue = healthSlider.maxValue;
        energyChangeSlider.maxValue = energySlider.maxValue;
        healthChangeSlider.value = healthSlider.value;
        energyChangeSlider.value = energySlider.value;
        
        charAtk = dataHolder.playerBaseAttack;
        hitFlash = GameObject.FindWithTag("Hit Flash");
        hitFlash.SetActive(false);
        _impulseSource = _characterMovement.GetComponent<CinemachineImpulseSource>();
    }

    private Slider FindSliderExcludingSelf(GameObject self)
    {
        foreach (var child in self.GetComponentsInChildren<Slider>())
        {
            if (child.gameObject != self)
            {
                return child;
            }
        }

        return null;
    }

    public void LightAttack(InputAction.CallbackContext ctx)
    {
        if (isDead || _characterMovement.uiOpen || !_characterMovement.canMove) return;
            
        if (ctx.performed && _characterMovement.grounded)
        {
            gameObject.layer = 13;
            
            if (currentEnergy < lightEnergyCost)
            {
                _inventoryStore.TriggerNotification(null, "Not enough energy.", false, 2f);
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
        if (isDead || _characterMovement.uiOpen || !_characterMovement.canMove) return;
        if (!ctx.performed || _characterMovement.isJumpAttacking || !_characterMovement.grounded) return;
        
        gameObject.layer = 15;
        
        if (currentEnergy < mediumEnergyCost)
        {
            _inventoryStore.TriggerNotification(null, "Not enough energy.", false, 2f);
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
        if (isDead || _characterMovement.uiOpen || !_characterMovement.canMove) return;
        if (!ctx.performed || _characterMovement.isJumpAttacking || !_characterMovement.grounded) return;
        
        gameObject.layer = 14;
        
        if (currentEnergy < heavyEnergyCost)
        {
            _inventoryStore.TriggerNotification(null, "Not enough energy.", false, 2f);
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
        _rigidbody.AddForce(new Vector3(dir * force, 0f, 0f), ForceMode.VelocityChange);
    }
    
    public void TakeDamagePlayer(int damage, int poiseDmg, Vector3 knockback)
    {
        if (isDead || _characterMovement.uiOpen) return;

        var previousHealth = dataHolder.playerHealth;
        var hitColor = Color.green;
        
        if (isInvincible > 0)
        {
            isInvincible--;
            _playerStatus.UpdateStatuses(isInvincible);
            return;
        }

        if (dataHolder.playerHealth - damage < dataHolder.playerHealth) // if damage is taken
        {
            dataHolder.playerDefense = Mathf.Clamp(dataHolder.playerDefense, 0, 100);
            var dmgReduction = (100 - dataHolder.playerDefense) / 100f;
            damage = Mathf.RoundToInt(damage * dmgReduction);
            
            if (isInvulnerable || _hasHitIframes) return;
            
            StartCoroutine(TimedVibration(0.25f, 0.75f, .5f));
            _impulseSource.GenerateImpulseWithForce(1f);
            
            hitColor = Color.red;
            
            StartCoroutine(HitFlash());
            AudioManager.Instance.PlayOneShot(FMODEvents.Instance.PlayerDamage, transform.position);
            if (dataHolder.hardcoreMode)
            {
                evilAnimator.SetTrigger(Laugh);
            }

            if (dataHolder.playerHealth > 0f && _characterMovement.grounded)
            {
                ApplyKnockback(knockback);
            }
        }
        else if (dataHolder.playerHealth - damage > dataHolder.playerHealth) // if health is increased
        {
            AudioManager.Instance.PlayOneShot(FMODEvents.Instance.Heal, transform.position);
            
            hitColor = Color.green;
        }

        if (dataHolder.playerHealth <= damage) // if damage taken causes health to become 0 or less
        {
            if (!dataHolder.surviveLethalHit)
            {
                dataHolder.playerHealth = 0;
                healthSlider.value = 0;
                Die();
            }
            else
            {
                dataHolder.playerHealth = 1;
                healthSlider.value = 1;
                dataHolder.surviveLethalHit = false;
                _passiveItemHandler.RemovePassive(_passiveItemHandler.FindPassive(3));
            }
        }
        else if (dataHolder.playerHealth - damage > dataHolder.playerMaxHealth) // if health exceeds max health
        {
            dataHolder.playerHealth = dataHolder.playerMaxHealth;
        }
        else
        {
            _hitFlashImg.color = hitColor;
            hitFlash.SetActive(true);
            dataHolder.playerHealth -= damage;
        }
        
        _poiseBuildup += poiseDmg;

        var isDamaged = dataHolder.playerHealth < previousHealth;
        var changeColor = isDamaged ? new Color(1f, 0.9f, 0.4f) : new Color(.5f, 1f, 0.4f);
        
        _healthTween?.Kill();
        _healthChangeImg.color = changeColor;

        if (isDamaged)
        {
            healthSlider.value = dataHolder.playerHealth;
            _healthTween = DOVirtual.Float(healthChangeSlider.value, dataHolder.playerHealth, 1f, v => healthChangeSlider.value = v).SetEase(Ease.OutExpo).SetDelay(0.4f);
        }
        else
        {
            healthChangeSlider.value = dataHolder.playerHealth;
            
            _healthTween = DOVirtual.Float(healthSlider.value, dataHolder.playerHealth, 1f, v => healthSlider.value = v).SetEase(Ease.OutExpo).SetDelay(0.4f);
        }
    }

    public IEnumerator TimedVibration(float lSpeed, float hSpeed, float duration)
    {
        if (!dataHolder.isGamepad) yield break;
        Gamepad.current.SetMotorSpeeds(lSpeed, hSpeed);
        yield return new WaitForSecondsRealtime(duration);
        InputSystem.ResetHaptics();
    }

    public void ApplyKnockback(Vector2 knockbackPower)
    {
        if (isDead) return;

        var knockbackDir = -_characterMovement.transform.localScale.x;
        var knockbackMultiplier = (_poiseBuildup >= poise) ? 15f : 10f; 
        var knockbackForce = new Vector3(knockbackPower.x * knockbackDir * knockbackMultiplier, knockbackPower.y * knockbackMultiplier, 0);
        
        //_knockbackRoutine = StartCoroutine(TriggerKnockback(knockbackForce, 0.35f));

        if (_poiseBuildup >= poise)
        {
            if (_stunRoutine != null) StopCoroutine(_stunRoutine);
            _stunRoutine = StartCoroutine(StunTimer(1.12f, true, knockbackForce));
            _poiseBuildup = 0;
        }
        else if (_characterMovement.canMove)
        {
            if (_stunRoutine == null)
            {
                _stunRoutine = StartCoroutine(StunTimer(stunDuration, false, knockbackForce));
            }
        }
    }
    
    private IEnumerator StunTimer(float stunTime, bool isPoiseBreak, Vector3 knockbackForce)
    {
        _characterMovement.canMove = false;
        
        if (isPoiseBreak)
        {
            yield return new WaitUntil(() => _characterMovement.grounded);
            _rigidbody.velocity = Vector3.zero;
            _rigidbody.drag = 8f;
            ResetCombo();
            _playerAnimator.SetBool(StanceBroken, true);
        }
        else
        {
            _rigidbody.velocity = Vector3.zero;
            _rigidbody.AddForce(knockbackForce, ForceMode.Impulse);
            ResetCombo();
            _playerAnimator.SetBool(IsStaggered, true);
        }
        
        yield return new WaitForSecondsRealtime(stunTime);

        _playerAnimator.SetBool(IsStaggered, false);
        _playerAnimator.SetBool(StanceBroken, false);

        _rigidbody.velocity = Vector3.zero;
        _rigidbody.drag = 0f;
        _characterMovement.canMove = true;
        _stunRoutine = null;
    }
    
    private IEnumerator TriggerKnockback(Vector3 force, float duration)
    {
        _characterMovement.canMove = false;
        _rigidbody.velocity = Vector3.zero;
        _rigidbody.AddForce(force, ForceMode.Impulse);
        yield return new WaitForSeconds(duration);
        _rigidbody.velocity = Vector3.zero;
        _characterMovement.canMove = true;
        _knockbackRoutine = null;
    }

    public void ChanceHeal()
    {
        if (!dataHolder.hpChanceOnKill) return;
        if (Random.Range(0, 100) >= dataHolder.changeToRegen) return;
            
        var healAmount = dataHolder.playerMaxHealth / 100 * dataHolder.hpChanceHealPercentage;
        TakeDamagePlayer(-healAmount, 0, Vector3.zero);
    }
    
    private IEnumerator HitFlash()
    {
        yield return new WaitForSeconds(0.2f);
        _hasHitIframes = true;
        _spriteRenderer.material = hitMaterial;
        var defaultColor = Color.black;
        var flashColor = new Color(0.2641509f, 0.2641509f, 0.2641509f);
        var flashSpeed = 0.1f;
        var fadeIn = true;

        var timer = 0f;

        while (timer < hitIframeDur)
        {
            var t = 0f;

            while (t < flashSpeed)
            {
                t += Time.deltaTime;
                
                if (fadeIn)
                {
                    hitMaterial.SetColor(EmissionColor, Color.Lerp(defaultColor, flashColor, t / flashSpeed));
                }
                else
                {
                    hitMaterial.SetColor(EmissionColor, Color.Lerp(flashColor, defaultColor, t / flashSpeed));
                }
                
                yield return null;
            }
            
            fadeIn = !fadeIn;
            timer += flashSpeed;
        }
        
        hitMaterial.SetColor(EmissionColor, defaultColor);
        _spriteRenderer.material = defaultMaterial;
        _hasHitIframes = false;
    }
    
    public void UseEnergy(float amount)
    {
        var previousEnergy = currentEnergy;
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
        
        var energyLost = currentEnergy < previousEnergy;
        var changeColor = energyLost ? new Color(1f, 0.9f, 0.4f) : new Color(.5f, 1f, 0.4f);
        
        _energyTween?.Kill();
        _energyChangeImg.color = changeColor;

        if (energyLost)
        {
            energySlider.value = currentEnergy;
            _energyTween = DOVirtual.Float(energyChangeSlider.value, currentEnergy, 1f, v => energyChangeSlider.value = v).SetEase(Ease.OutExpo).SetDelay(0.4f);
        }
        else
        {
            energyChangeSlider.value = currentEnergy;
            
            _energyTween = DOVirtual.Float(energySlider.value, currentEnergy, 1f, v => energySlider.value = v).SetEase(Ease.OutExpo).SetDelay(0.4f);
        }
    }
    
    private void Die()
    {
        if (isDead) return;
        dataHolder.playerDeaths++;
        dataHolder.totalDeaths++;
        AudioManager.Instance.SetGlobalEventParameter("Music Track", 7);
        AudioManager.Instance.SetGlobalEventParameter("NoMusicUIVolume", 0.1f);
        StartCoroutine(WaitUntilGrounded());
    }

    private IEnumerator WaitUntilGrounded()
    {
        yield return new WaitUntil(() => _characterMovement.grounded);
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
            //StartCoroutine(TimedVibration(0.1f, 0.25f, .25f));
            
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
            var calcAtk = (charAtk - dataHolder.playerBaseAttack) + (dataHolder.playerBaseAttack * mediumAtkMultiplier);
            damageable.TakeDamage((int)calcAtk, poiseDamageMedium, knockbackPowerMedium);
            _enemyDamageEvent = AudioManager.Instance.CreateEventInstance(FMODEvents.Instance.EnemyDamage);
            _enemyDamageEvent.set3DAttributes(new Vector3(transform.position.x, transform.position.y, transform.position.z).To3DAttributes());
            _enemyDamageEvent.start();
            _enemyDamageEvent.release();
            //StartCoroutine(TimedVibration(0.25f, 0.5f, .3f));
            
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
            var calcAtk = (charAtk - dataHolder.playerBaseAttack) + (dataHolder.playerBaseAttack * heavyAtkMultiplier);
            damageable.TakeDamage((int)calcAtk, poiseDamageHeavy, knockbackPowerHeavy);
            _enemyDamageEvent = AudioManager.Instance.CreateEventInstance(FMODEvents.Instance.EnemyDamage);
            _enemyDamageEvent.set3DAttributes(new Vector3(transform.position.x, transform.position.y, transform.position.z).To3DAttributes());
            _enemyDamageEvent.start();
            _enemyDamageEvent.release();
            //StartCoroutine(TimedVibration(0.35f, .75f, .5f));
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

        if (_poiseBuildup > 0)
        {
            _poiseRecoverTimer -= Time.deltaTime;

            if (_poiseRecoverTimer <= 0)
            {
                if (_poiseBuildup - poiseRecoveryAmount <= 0)
                    _poiseBuildup = 0;
                else
                    _poiseBuildup -= poiseRecoveryAmount;

                _poiseRecoverTimer = poiseRecoveryDelay;
            }
        }

        if (!_characterMovement.grounded || !_rigidbody.useGravity)
        {
            if (_knockbackRoutine != null)
            {
                StopCoroutine(_knockbackRoutine);
            }
        }

        if (jumpAttackCount > 0)
        {
            if (_characterMovement.grounded)
            {
                jumpAttackCount = 0;
            }
        }

        if (dataHolder.passiveEnergyRegen && currentEnergy < maxEnergy)
        {
            _rechargeTime -= Time.deltaTime * rechargeSpeed;

            if (_rechargeTime <= 0)
            {
                UseEnergy(-1f);
                _rechargeTime = 1f;
            }
        }
    }
}

