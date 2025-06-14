using System;
using System.Collections;
using Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;

public class CharacterMovement : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform playerGround;
    private Rigidbody _rb;
    private Animator _playerAnimator;
    private Coroutine _dashCoroutine;
    private CharacterAttack _characterAttack;
    private CinemachineImpulseSource _impulseSource;
    [SerializeField] private PhysicMaterial characterPhysicMaterial;
    private MenuHandler _menuHandler;
    private GameObject _uiManager;

    [Header("Player Properties")] 
    public bool doesAttackStopFlip;
    public bool grounded;
    public bool canMove = true;
    public bool isCrouching;
    public bool isAttacking;
    public Vector3 velocity;
    private bool _isDashing;
    private int _midAirDashCount;
    private float _input;
    public bool isInUpThrust;
    
    [Header("Other Properties")] 
    [SerializeField] private float groundCheckDist;
    [SerializeField] private float groundCheckSpacing;
    public bool uiOpen; // makes sure player doesnt move when ui is open
    [NonSerialized] public bool LockedOn = false;
    //private float _groundTimer; // Timer to keep the grounded bool true if the player is off the ground for extremely brief periods of time.
    private float _hangTimer;
    private bool _canDash = true;
    
    [Header("Jumping")]
    [SerializeField] private bool doubleJumpResetJumpAttack;
    [SerializeField] private bool doesJumpInputCancel;
    [SerializeField] private float jumpCancelForce;
    [SerializeField] private float hangTime;
    [SerializeField] private float hangThreshold;
    [SerializeField] private float jumpCoyoteDur;
    [SerializeField] private float jumpBufferDur;
    private float _lastGroundedTime;
    private float _lastJumpInput;
    private bool _jumpBuffered;
    public bool isJumpAttacking;
    public bool doubleJumpPerformed;
    private bool _isHanging;
    private bool _jumpHeld;
    
    [Header("Movement Stats")]
    public float acceleration;
    public float maxSpeed;
    public float jumpForce;
    public float dashSpeed;
    
    [Header("Energy")]
    public float dashEnergyCost;  
    
    [Header("Animations")]
    private static readonly int Dash1 = Animator.StringToHash("Dash");
    private static readonly int Input1 = Animator.StringToHash("Input");
    private static readonly int YVelocity = Animator.StringToHash("YVelocity");
    private static readonly int XVelocity = Animator.StringToHash("XVelocity");
    private static readonly int Grounded = Animator.StringToHash("Grounded");
    private static readonly int IsWalkingBackwards = Animator.StringToHash("isWalkingBackwards");
    private static readonly int IsCrouching = Animator.StringToHash("isCrouching");
    private static readonly int Jump1 = Animator.StringToHash("Jump");
    private static readonly int DoubleJump = Animator.StringToHash("DoubleJump");
    
    /*
    [Header("Wall Jump")]
    public float slideSpeed = 1f;
    public float slideHoldTime = 1f;
    [SerializeField] private bool startSlide;
    [SerializeField] private bool startSlideTimer;
    [SerializeField] private bool sliding;
    public bool slideAllowed;
    [SerializeField] private float slideTimer;
    [SerializeField] bool isWallJumping;
    [SerializeField] private float wallJumpingCounter;
    [SerializeField] private float wallJumpingTime;
    [SerializeField] private Vector2 wallJumpForce;
    [SerializeField] private float wallJumpingDuration;
    */

    public void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        GetComponent<CapsuleCollider>().material = characterPhysicMaterial;
        _playerAnimator = GetComponentInChildren<Animator>();
        _characterAttack = GetComponentInChildren<CharacterAttack>();
        _impulseSource = GetComponent<CinemachineImpulseSource>();
        _uiManager = GameObject.FindGameObjectWithTag("UIManager");
        _menuHandler = _uiManager.GetComponent<MenuHandler>();
    }

    public void Crouch(InputAction.CallbackContext ctx)
    {
        if (uiOpen || !canMove) return;

        if (ctx.ReadValue<float>() > 0)
        {
            _playerAnimator.SetBool(IsCrouching, true);
            isCrouching = true;
        }
        else
        {
            _playerAnimator.SetBool(IsCrouching, false);
            isCrouching = false;
        }
    }

    public void XAxis(InputAction.CallbackContext ctx)
    {
        if (uiOpen) return;
        _input = ctx.ReadValue<float>();
        _playerAnimator.SetFloat(Input1, Mathf.Abs(_input));
        if (Mathf.Abs(_input) > 0)
        {
            _menuHandler.ResetIdleTime();
        }
    }

    public void Jump(InputAction.CallbackContext ctx)
    {
        if (uiOpen || !canMove || isInUpThrust || isCrouching) return;

        if (ctx.started)
        {
            _jumpHeld = true;
            _lastJumpInput = Time.time;
            _jumpBuffered = true;
        }

        if (ctx.performed)
        {
            if (grounded && !doubleJumpPerformed)
            {
                _rb.velocity = new Vector3(_rb.velocity.x, jumpForce, 0f);
                _playerAnimator.SetBool(Jump1, true);
                _characterAttack.ResetCombo();
                grounded = false;
            }
            else if (!grounded && !doubleJumpPerformed)
            {
                doubleJumpPerformed = true;
                if (doubleJumpResetJumpAttack)
                {
                    _characterAttack.jumpAttackCount = 0;
                }
                _rb.velocity = new Vector3(_rb.velocity.x, jumpForce, 0f);
                _playerAnimator.SetBool(DoubleJump, true);
            }
        }

        if (ctx.canceled)
        {
            _jumpHeld = false;
        }
    }
    
    
    /* ^ cut from Jump method for clarity
     else if (!grounded && wallJumpingCounter > 0f && (startSlideTimer || sliding) && !isWallJumping)
        {
            slideAllowed = false;
            startSlideTimer = false;
            isWallJumping = true;

            var direction = (input != 0) ? -Mathf.Sign(input) : -Mathf.Sign(transform.localScale.x);
            //rb.velocity = new Vector3(direction * wallJumpForce.x, wallJumpForce.y, 0f);
            Vector3 _wallJumpForce = new Vector3(direction * wallJumpForce.x, wallJumpForce.y, 0f);
            rb.AddForce(_wallJumpForce);

            wallJumpingCounter = 0f;

            if (!lockedOn && Mathf.Abs(rb.velocity.x) > 0.1f)
            {
                transform.localScale = new Vector3(transform.localScale.x * -1, transform.localScale.y, transform.localScale.z);
            }

            Invoke(nameof(stopWallJump), wallJumpingDuration);
        }*/

/*private void wallJump()
{
    if (uiOpen) return;
    if (slideAllowed || startSlideTimer || grounded)
    {
        isWallJumping = false;
        wallJumpingCounter = wallJumpingTime;
        CancelInvoke(nameof(stopWallJump));
    }
    else
    {
        wallJumpingCounter -= Time.deltaTime;
    }
}

private void stopWallJump()
{
    if (uiOpen) return;
    isWallJumping = false;
}*/

    public void Dash(InputAction.CallbackContext ctx)
    {
        if (uiOpen || !canMove || _isDashing || !_canDash) return;

        if (ctx.performed && _midAirDashCount == 0) 
        {
            isAttacking = false;
            isJumpAttacking = false;
            _characterAttack.ResetCombo();
            float facingImpulse = 0;
            switch (transform.localScale.x)
            {
                case > 0:
                    facingImpulse = 0.25f;
                    break;
                case < 0:
                    facingImpulse = -0.25f;
                    break;
            }
            _impulseSource.GenerateImpulseWithVelocity(new Vector3(facingImpulse, 0, 0));
            StartCoroutine(DashRoutine());
            StartCoroutine(DashWait());

            if (!grounded)
            {
                _midAirDashCount++;
            }
        }
    }
    
    private IEnumerator DashRoutine()
    {
        _playerAnimator.SetBool(Dash1, true);
        _characterAttack.isInvulnerable = true;
        _isDashing = true;
        _rb.useGravity = false;

        var direction = _input != 0 ? (1.25f * Mathf.Sign(_input)): Mathf.Sign(transform.localScale.x);
        var dashVelocity = new Vector3(direction * dashSpeed, 0f, 0f);
        
        _rb.velocity += dashVelocity;
        _rb.drag = 8f;
        yield return new WaitForSeconds(0.2f);

        _rb.drag = 0f;
        _rb.velocity = new Vector3(0f, _rb.velocity.y, 0f);
        _rb.useGravity = true;
        _isDashing = false;
        _characterAttack.isInvulnerable = false;
        _playerAnimator.SetBool(Dash1, false);
    }

    private IEnumerator DashWait()
    {
        _canDash = false;
        yield return new WaitForSeconds(.4f);
        _canDash = true;
    }
    
    public void Update()
    {
        if (uiOpen)
        {
            _input = 0;
            _rb.velocity = new Vector3(0, _rb.velocity.y, 0);
            _playerAnimator.SetFloat(Input1, 0);
        }
        //_groundTimer -= Time.deltaTime;
        //_groundTimer = Mathf.Clamp(_groundTimer, 0, 1f);
        //PlayerAnimator.SetBool("WallJump", isWallJumping);
        velocity = _rb.velocity;
        _playerAnimator.SetFloat(XVelocity, _rb.velocity.x);
        _playerAnimator.SetFloat(YVelocity, _rb.velocity.y);
        _playerAnimator.SetBool(Grounded, grounded);
        
        if (!canMove || uiOpen)
        {
            _rb.useGravity = true;
            isCrouching = false;
            _playerAnimator.SetBool(IsCrouching, false);
        }
        
        if (uiOpen) return;
        
        //wallJump();
        
        // if the player is moving or crouching while not locked on this updates the players local scale based on vel/input
        if (!uiOpen && !LockedOn && (Mathf.Abs(velocity.x) >= 0.1f) || (isCrouching && Mathf.Abs(_input) > 0))
        {
            var localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
            var dir = Mathf.Abs(velocity.x) >= 0.1f ? Mathf.Sign(velocity.x) : Mathf.Sign(_input);
            
            if ((doesAttackStopFlip && !isAttacking) || !doesAttackStopFlip)
            {
                transform.localScale = new Vector3(dir * localScale.x, localScale.y, localScale.z);
            }
        }

        /*
        if (!uiOpen && !LockedOn && _playerAnimator.GetBool(WallCling) && Mathf.Sign(transform.localScale.x) != Mathf.Sign(_input)) // if wallcling is true and player isn't facing direction of input, flip the player
        {
            transform.localScale = new Vector3(transform.localScale.x * -1, transform.localScale.y, transform.localScale.z);
        }
        */

        var direction = Mathf.Sign(_input);
        var scaleDirection = Mathf.Sign(transform.localScale.x);
        
        if (direction != 0 && direction != scaleDirection)
        {
            _playerAnimator.SetBool(IsWalkingBackwards, true);
        }
        else
        {
            _playerAnimator.SetBool(IsWalkingBackwards, false);
        }

        /*if (startSlideTimer)
        {
            slideTimer += 10f * Time.deltaTime;
            //Debug.Log(slideTimer);
            if(slideTimer >= slideHoldTime)
            {
                startSlide = true;
                slideTimer = 0f;
                startSlideTimer = false;
            }
        }*/
        
        /*
        if (startDashTimer)
        {
            dashTimer += Time.deltaTime;
            if(dashTimer >= dashBreak)
            {
                dashAllowed = true;
                dashTimer = 0f;
                startDashTimer = false;
            }
        }
        */

        if (BlackoutManager.Instance != null)
        {
            //Stop player moving while game is loading
            if (BlackoutManager.Instance.blackoutComplete == false)
            {
                canMove = false;
            }
            else if (BlackoutManager.Instance.blackoutComplete && uiOpen == false)
            {
                canMove = true;
            }
        }
    }

    public void FixedUpdate()
    {
        var lRayStart = playerGround.position - new Vector3(groundCheckSpacing, 0, 0);
        var rRayStart = playerGround.position + new Vector3(groundCheckSpacing, 0, 0);

        var leftGrounded = Physics.Raycast(lRayStart, Vector3.down, groundCheckDist, LayerMask.GetMask("Ground"));
        var rightGrounded = Physics.Raycast(rRayStart, Vector3.down, groundCheckDist, LayerMask.GetMask("Ground"));

        grounded = leftGrounded || rightGrounded;
        _playerAnimator.SetBool(Grounded, grounded);
        
        if (uiOpen) return;

        if (grounded)
        {
            doubleJumpPerformed = false;
            _midAirDashCount = 0;
            isInUpThrust = false;
            _isHanging = false;
            _rb.useGravity = true;
            _lastGroundedTime = Time.time;
        }

        if (isCrouching)
        {
            _rb.velocity = new Vector3(0f, _rb.velocity.y, _rb.velocity.z);
        }
        else if (canMove & !_isDashing && !isJumpAttacking && !isCrouching)
        {
            var acc = acceleration;
            if (isAttacking) acc = acceleration / 3;
            if ((_rb.velocity.x <= maxSpeed && Mathf.Sign(_rb.velocity.x) == 1) || (_rb.velocity.x >= -maxSpeed && Mathf.Sign(_rb.velocity.x) == -1) || (Mathf.Sign(_rb.velocity.x) != _input))
            {
                _rb.velocity = new Vector3(_input * acc, _rb.velocity.y, _rb.velocity.z);
            }
        }

        // pushes player down if jump is cancelled early
        if (doesJumpInputCancel && !isInUpThrust && _rb.velocity.y > 0 && !_jumpHeld)
        {
            _rb.velocity -= Vector3.down * (Physics.gravity.y * (jumpCancelForce) * Time.fixedDeltaTime);
        }
        
        // at the apex of a jump this basically gives the player a split second where they don't immediately fall back down
        if (!grounded && !_isHanging && !isInUpThrust)
        {
            if (Mathf.Abs(_rb.velocity.y) <= hangThreshold)
            {
                _isHanging = true;
                _hangTimer = hangTime;
                _rb.velocity = new Vector3(_rb.velocity.x, 0f, _rb.velocity.z);
                _rb.useGravity = false;
            }
        }
        
        if (_isHanging)
        {
            _hangTimer -= Time.fixedDeltaTime;
            
            if (_hangTimer <= 0f || !_jumpHeld)
            {
                _rb.useGravity = true;
                _isHanging = false;
            }
        }
        
        if (_jumpBuffered && !isInUpThrust)
        {
            var canCoyote = Time.time - _lastGroundedTime <= jumpCoyoteDur;
            var canBuffer = Time.time - _lastJumpInput <= jumpBufferDur;

            if (canCoyote && canBuffer)
            {
                _rb.velocity = new Vector3(_rb.velocity.x, jumpForce, 0f);
                _playerAnimator.SetBool(Jump1, true);
                _characterAttack.ResetCombo();
                grounded = false;
                _jumpBuffered = false;
            }
            else if (!grounded && !doubleJumpPerformed && canBuffer)
            {
                doubleJumpPerformed = true;
                if (doubleJumpResetJumpAttack)
                {
                    _characterAttack.jumpAttackCount = 0;
                }
                _rb.velocity = new Vector3(_rb.velocity.x, jumpForce, 0f);
                _playerAnimator.SetBool(DoubleJump, true);
                _jumpBuffered = false;
            }
        }
        
        if (Time.time - _lastJumpInput > jumpBufferDur)
        {
            _jumpBuffered = false;
        }

        /*if (slideAllowed && !grounded)
        {
            Vector3 wallSlide = new Vector3(rb.velocity.x, -slideSpeed, 0f);
            rb.velocity = wallSlide;
            sliding = true;
            Debug.Log("StartSlide");
        }
        else sliding = false;*/
    }

    /*
    private void OnTriggerStay(Collider other)
    {
        if (uiOpen) return;

        if (other.gameObject.layer == 10 && _playerGroundPosition.transform.position.y > other.gameObject.transform.position.y)
        {
            if (Velocity.y <= 0)
            {
                grounded = true;
                //startSlide = false;
                doubleJumpPerformed = false;
                _midAirDashCount = 0;
                //slideAllowed = false;
                PlayerAnimator.SetBool("Grounded", true);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (fallingThrough)
        {
            groundTimer = 0f;
        }
        else 
        {
            groundTimer = 0.15f;
        }
        
        if (other.gameObject.layer == 10 && groundTimer <= 0)
        {
            grounded = false;
            fallingThrough = false;
        }
        else if (groundTimer >= 0)
        {
            StartCoroutine(CheckIfStillInAir());
        }
    }
    

    IEnumerator CheckIfStillInAir()
    {
        yield return new WaitForSecondsRealtime(groundTimer);
        if (groundTimer == 0)
        {
            grounded = false;
        }
        else
        {
            grounded = true;
        }
    }
    */
    
    /*private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Right Wall") || collision.collider.CompareTag("Left Wall"))
        {
            if (!grounded && input != 0)
            {
                rb.velocity = new Vector3(0f, Mathf.Min(rb.velocity.y, 0f), 0f);
                rb.useGravity = false;
                startSlideTimer = true;
                PlayerAnimator.SetBool("WallCling", true);
            }
        }
    }


    private void OnCollisionStay(Collision collision)
    {
        if (collision.collider.CompareTag("Right Wall") || collision.collider.CompareTag("Left Wall"))
        {
            if (!grounded && input != 0)
            {
                PlayerAnimator.SetBool("WallCling", true);
                if (startSlide)
                {
                    slideAllowed = true;
                    Debug.Log("slideAllowed");
                }
            }
            else rb.useGravity = true; startSlide = false;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.collider.CompareTag("Right Wall") || collision.collider.CompareTag("Left Wall"))
        {
            rb.useGravity = true;
            startSlideTimer = false;
            startSlide = false;
            slideAllowed = false;
            sliding = false;
            slideTimer = 0f;
            PlayerAnimator.SetBool("WallCling", false);
        }
    }*/
    
    private void OnDrawGizmosSelected()
    {
        var basePos = playerGround.transform.position;
        var lRayStart = basePos - new Vector3(groundCheckSpacing, 0, 0);
        var rRayStart = basePos + new Vector3(groundCheckSpacing, 0, 0);
        
        Gizmos.color = Color.red;
        Gizmos.DrawLine(lRayStart, lRayStart + Vector3.down * groundCheckDist);
        Gizmos.DrawLine(rRayStart, rRayStart + Vector3.down * groundCheckDist);
    }
}