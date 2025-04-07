using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class CharacterMovement : MonoBehaviour
{
    Rigidbody rb;
    BoxCollider groundCheck;
    Animator PlayerAnimator;
    public bool walkAllowed = true;

    public bool allowMovement = true;

    public float acceleration = 1f;
    public float maxSpeed = 10f;
    public float jumpForce = 1f;
    public float dashSpeed = 1f;
    //public float slideSpeed = 1f;
    //public float slideHoldTime = 1f;

    [SerializeField] private bool grounded;
    private float groundTimer; // Timer to keep the grounded bool true if the player is off the ground for extremely brief periods of time. 
    private bool fallingThrough; // Bool to fix player not entering the fall state if they drop through platforms
    public bool doubleJumpPerformed;
    public bool isCrouching;
    public Vector3 Velocity;
    private GameObject _playerGroundPosition;

    private bool dashAllowed = true;
    private bool startDashTimer;
    private float dashTimer;
    [SerializeField] private float dashBreak;

    private float input;
    //[SerializeField] private bool startSlide;
    //[SerializeField] private bool startSlideTimer;
    //[SerializeField] private bool sliding;
    //public bool slideAllowed;
    //[SerializeField] private float slideTimer;

    //[SerializeField] bool isWallJumping;
    //[SerializeField] private float wallJumpingCounter;
    //[SerializeField] private float wallJumpingTime;
    //[SerializeField] private Vector2 wallJumpForce;
    //[SerializeField] private float wallJumpingDuration;

    [NonSerialized] public bool uiOpen; // makes sure player doesnt move when ui is open
    [NonSerialized] public bool lockedOn = false;

    public void Awake()
    {
        rb = GetComponent<Rigidbody>();
        _playerGroundPosition = transform.Find("GroundPos").gameObject;
        groundCheck = GetComponent<BoxCollider>();
        PlayerAnimator = GetComponentInChildren<Animator>();
    }

    public void Crouch(InputAction.CallbackContext ctx)
    {
        if (uiOpen) return;
        
        if (ctx.ReadValue<float>() > 0)
        {
            PlayerAnimator.SetBool("isCrouching", true);
            if (!allowMovement) return;
            fallingThrough = true;
            isCrouching = true;
        }
        else
        {
            PlayerAnimator.SetBool("isCrouching", false);
            if (!allowMovement) return;
            fallingThrough = true;
            isCrouching = false;
        }
    }

    public void XAxis(InputAction.CallbackContext ctx)
    {
        if (uiOpen) return;
        //if (!allowMovement) return;
        input = ctx.ReadValue<float>();
        PlayerAnimator.SetFloat("Input", input);
    }

    public void Jump(InputAction.CallbackContext ctx)
    {
        if (uiOpen || !allowMovement|| !ctx.performed) return;
        groundTimer = 0;
        if (grounded && !doubleJumpPerformed /*&& !startSlideTimer && !sliding*/)
        {
            rb.velocity = new Vector3(rb.velocity.x, jumpForce, 0f);
            PlayerAnimator.SetBool("Jump", true);
            grounded = false;
        }
        /*else if (!grounded && wallJumpingCounter > 0f && (startSlideTimer || sliding) && !isWallJumping)
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
        else if (!grounded /*&& !startSlideTimer && !sliding*/ && !doubleJumpPerformed && !PlayerAnimator.GetBool("WallCling"))
        {
            doubleJumpPerformed = true;
            rb.velocity = new Vector3(rb.velocity.x, jumpForce, 0f);
            PlayerAnimator.SetBool("DoubleJump", true);
        }
    }

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
        if (uiOpen || !allowMovement || !dashAllowed) return;

        if (ctx.performed && grounded) 
        {
            var direction = input != 0 ? Mathf.Sign(input) : Mathf.Sign(transform.localScale.x);
            var dashForce = new Vector3(direction * dashSpeed, rb.velocity.y, 0f);

            rb.velocity = dashForce;

            PlayerAnimator.SetBool("Dash", true);
            dashAllowed = false;
            startDashTimer = true;
            dashTimer = 0f;
        }
    }


    public void Update()
    {
        if (uiOpen || allowMovement == false)
        {
            input = 0;
            rb.velocity = Vector3.zero;
            PlayerAnimator.SetFloat("Input", 0);
        }
        groundTimer -= Time.deltaTime;
        groundTimer = Mathf.Clamp(groundTimer, 0, 1f);
        //PlayerAnimator.SetBool("WallJump", isWallJumping);
        Velocity = rb.velocity;
        PlayerAnimator.SetFloat("XVelocity", rb.velocity.x);
        PlayerAnimator.SetFloat("YVelocity", rb.velocity.y);
        PlayerAnimator.SetBool("Grounded", grounded);
        
        if (uiOpen) return;
        
        //wallJump();

        if (!uiOpen && !lockedOn && Mathf.Abs(Velocity.x) >= 0.1f && Mathf.Sign(transform.localScale.x) != Mathf.Sign(Velocity.x)) // this has a check if the player is locked on to prevent them flipping
        {
            transform.localScale = new Vector3(transform.localScale.x * -1, transform.localScale.y, transform.localScale.z);
        }
        if (!uiOpen && !lockedOn && PlayerAnimator.GetBool("WallCling") && Mathf.Sign(transform.localScale.x) != Mathf.Sign(input)) // if wallcling is true and player isn't facing direction of input, flip the player
        { 
            transform.localScale = new Vector3(transform.localScale.x * -1, transform.localScale.y, transform.localScale.z);
        }

        var direction = Mathf.Sign(input);
        var scaleDirection = Mathf.Sign(transform.localScale.x);
        
        if (direction != 0 && direction != scaleDirection)
        {
            PlayerAnimator.SetBool("isWalkingBackwards", true);
        }
        else
        {
            PlayerAnimator.SetBool("isWalkingBackwards", false);
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

        if (BlackoutManager.Instance != null)
        {
            //Stop player moving while game is loading
            if (BlackoutManager.Instance.blackoutComplete == false)
            {
                allowMovement = false;
                walkAllowed = false;
            }
            else if (BlackoutManager.Instance.blackoutComplete && uiOpen == false)
            {
                allowMovement = true;
                walkAllowed = true;
            }
        }
    }

    public void FixedUpdate()
    {
        if (uiOpen) return;
        if (walkAllowed && /*!isWallJumping &&*/ allowMovement)
        {
            if ((rb.velocity.x <= maxSpeed && Mathf.Sign(rb.velocity.x) == 1) || (rb.velocity.x >= -maxSpeed && Mathf.Sign(rb.velocity.x) == -1) || (Mathf.Sign(rb.velocity.x) != input))
            {
                Vector3 walk = new Vector3(input * acceleration, rb.velocity.y, rb.velocity.z);
                rb.velocity = walk;
            }
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

    private void OnTriggerStay(Collider other)
    {
        if (uiOpen) return;

        if (other.gameObject.layer == 10 && _playerGroundPosition.transform.position.y > other.gameObject.transform.position.y)
        {
            if (Velocity.y <= 0f)
            {
                grounded = true;
                //startSlide = false;
                doubleJumpPerformed = false;
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
}