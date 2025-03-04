using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class CharacterAttack : MonoBehaviour
{
    private MeshCollider _attackCollider;
    private Animator _playerAnimator;

    // Combo variables
    [SerializeField] private float timer = 0f;
    [SerializeField] private float timer1 = 0f;
    [SerializeField] private bool[] lightCombo = new bool[4];
    [SerializeField] private bool[] heavyCombo = new bool[4];
    [SerializeField] private float maxInputDelay = 10f;
    [HideInInspector] public bool animEnd;
    
    [Header("Stats")]
    public int currentHealth = 100;
    public int maxHealth = 100;
    public int baseAtk;
    public int charAtk = 10;
    public int isInvincible;
    public bool isPoison;
    public bool isIce;
    
    [Header("Tracking")]
    [SerializeField] private List<Transform> enemies;
    [SerializeField] private Transform nearestEnemy;
    
    
    [SerializeField] private Slider healthSlider;
    [SerializeField] private GameObject diedScreen;
    private GameObject _uiManager;
    private MenuHandler _menuHandler;
    private PlayerStatus _playerStatus;
    public GameObject hitFlash;

    [Header("Knockback Types")]
    [SerializeField] private Vector2 knockbackPowerLight = new Vector2(10f, 1f);
    [SerializeField] private Vector2 knockbackPowerHeavy = new Vector2(20f, 3f);
    private CinemachineCollisionImpulseSource _impulseSource;

    private void Start()
    {
        _attackCollider = GetComponent<MeshCollider>();
        _playerAnimator = GameObject.FindGameObjectWithTag("PlayerRenderer").GetComponent<Animator>();
        _uiManager = GameObject.FindGameObjectWithTag("UIManager");
        _menuHandler = _uiManager.GetComponent<MenuHandler>();
        _playerStatus = _uiManager.GetComponent<PlayerStatus>();
        healthSlider.maxValue = maxHealth;
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
            gameObject.layer = 13;
            //Debug.Log("LightAttack");
            //_playerAnimator.SetBool("LightAttack1", true);
            //InitiateAttack();
            animEnd = false;

            // Start of chain
            if (!lightCombo[0])
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
                    if (lightCombo[1])
                    {
                        if (timer1 <= maxInputDelay && timer1 > 0f)
                        {
                            if (animEnd)
                            {
                                
                                lightCombo[2] = true;
                            }
                        }
                    }
                    lightCombo[1] = true;
                }
            }
            
            lightCombo[0] = true;
            if (lightCombo[2] && animEnd)
            {
                Debug.Log("LightAttack2");
                _playerAnimator.SetBool("LightAttack2", true);

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
            gameObject.layer = 14;
            //Debug.Log("LightAttack");
            //_playerAnimator.SetBool("LightAttack1", true);
            //InitiateAttack();
            animEnd = false;

            // Start of chain
            if (!heavyCombo[0])
            {
                Debug.Log("HeavyAttack");
                _playerAnimator.SetBool("HeavyAttack", true);
            }

            if (heavyCombo[0])
            {


                if (timer <= maxInputDelay && timer > 0f)
                {
                    Debug.Log("HeavyAttack1");
                    _playerAnimator.SetBool("HeavyAttack1", true);
                    if (heavyCombo[1])
                    {
                        if (timer1 <= maxInputDelay && timer1 > 0f)
                        {
                            if (animEnd)
                            {

                                heavyCombo[2] = true;
                            }
                        }
                    }
                    heavyCombo[1] = true;
                }
            }

            heavyCombo[0] = true;
            if (heavyCombo[2] && animEnd)
            {
                Debug.Log("HeavyAttack2");
                _playerAnimator.SetBool("HeavyAttack2", true);

                timer1 = 0f; heavyCombo[1] = false;
                timer = 0f; heavyCombo[0] = false;


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

    public void TakeDamagePlayer(int damage)
    {
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
    }

    private void Die()
    {
        diedScreen.SetActive(true);
        _menuHandler.SwitchSelected(diedScreen.GetComponentInChildren<Button>().gameObject);
    }

    /*
    private void InitiateAttack()
    {
        CountEnemies();

        if (enemies.Count > 0)
        {
            foreach (var eh in enemies)
            {
                var dist = Vector3.Distance(transform.position, eh.transform.position);


                if (nearestEnemy == null)
                {
                    nearestEnemy = eh;
                }
                else if (dist <= Vector3.Distance(transform.position, nearestEnemy.transform.position))
                {
                    nearestEnemy = eh;
                }
            }
        }

        var dist1 = Vector3.Distance(transform.position, nearestEnemy.transform.position);

        if (dist1 <= atkRange)
        {
            if (nearestEnemy.GetComponent<EnemyHandler>())
            {
                nearestEnemy.GetComponent<EnemyHandler>().TakeDamageEnemy(charAtk);
            }
            else if (nearestEnemy.GetComponent<BossHandler>())
            {
                nearestEnemy.GetComponent<BossHandler>().TakeDamageEnemy(charAtk);
            }
            else if (nearestEnemy.GetComponent<Boss_TwoHands>())
            {
                nearestEnemy.GetComponent<Boss_TwoHands>().TakeDamageEnemy(charAtk);
            }
        }
    }
    */

    /*
    private void CountEnemies()
    {
        enemies.Clear();
        
        foreach (var e in GameObject.FindGameObjectsWithTag("Enemy"))
        {
            if (e.GetComponent<EnemyHandler>() || e.GetComponent<BossHandler>() || e.GetComponent<Boss_TwoHands>())
            {
                enemies.Add(e.transform);
            }
        }
    }
    */

    private void OnTriggerEnter(Collider other) // gets takedamage and trigger status methods from all enemy types
    {
        if (!other.CompareTag("Enemy")) return;

        var damageable = other.GetComponentInParent<IDamageable>();
        if (damageable == null) return;
        
       damageable.TakeDamage(charAtk);
        //Layer = Light
        if (gameObject.layer == 13)
        {
            float randomTiny = Random.Range(-0.25f, 0.25f);
            damageable.ApplyKnockback(knockbackPowerLight);
            _impulseSource.m_ImpulseDefinition.m_ImpulseDuration = 0.05f;
            _impulseSource.GenerateImpulseWithVelocity(new Vector3(0.25f, randomTiny, 0));
        }
        //Layer = Heavy
        if (gameObject.layer == 14)
        {
            float randomTiny = Random.Range(-1f, 1f);
            damageable.ApplyKnockback(knockbackPowerHeavy);
            _impulseSource.m_ImpulseDefinition.m_ImpulseDuration = 0.2f;
            _impulseSource.GenerateImpulseWithVelocity(new Vector3(2, randomTiny, 0));
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
                timer += 1f * Time.deltaTime;
            }
            if (heavyCombo[1])
            {
                timer1 += 1f * Time.deltaTime;
                if (timer1 >= maxInputDelay)
                {
                    heavyCombo[0] = false;
                    heavyCombo[1] = false;
                    timer = 0f;
                    timer1 = 0f;
                }
            }
            if (timer >= maxInputDelay && !heavyCombo[1])
            {
                heavyCombo[0] = false;
                timer = 0f;
            }
        }

    }
}

