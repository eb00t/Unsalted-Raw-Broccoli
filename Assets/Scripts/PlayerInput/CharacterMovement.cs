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
    public float slideSpeed = 1f;
    public float holdTime = 1f;

    [SerializeField] private bool grounded;
    [SerializeField] private bool doubleJumpPerformed;
    public bool isCrouching;
    [SerializeField] private Vector3 Velocity;
    private GameObject _playerGroundPosition;

    private bool dashAllowed = true;
    private bool startDashTimer = false;
    private float dashTimer = 0f;
    [SerializeField] private float dashBreak = 10f;

    private float input;
    [SerializeField] private bool startSlide;
    [SerializeField] private bool startSlideTimer;
    [SerializeField] private bool sliding = false;
    public bool slideAllowed = false;
    [SerializeField] private float slideTimer = 0f;

    [SerializeField] bool isWallJumping = false;
    [SerializeField] private float wallJumpingCounter;
    [SerializeField] private float wallJumpingTime = 0.2f;
    [SerializeField] private Vector2 wallJumpForce = new Vector2(300f, 300f);
    [SerializeField] private float wallJumpingDuration = 0.4f;

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
        if (!allowMovement) return;
        if (ctx.ReadValue<float>() > 0)
        {
            isCrouching = true;
        }
        else
        {
            isCrouching = false;
        }
    }

    public void XAxis(InputAction.CallbackContext ctx)
    {
        if (uiOpen) return;
        if (!walkAllowed) return;
        input = ctx.ReadValue<float>();
        PlayerAnimator.SetFloat("Input", input);
    }

    public void Jump(InputAction.CallbackContext ctx)
    {
        if (uiOpen) return;
        if(!allowMovement) { }
            else
        if (ctx.performed && !doubleJumpPerformed && !startSlideTimer && !sliding && grounded)
        {
            Debug.Log("Jump");
            Vector3 jump = new Vector3(rb.velocity.x, jumpForce, 0f);
            //rb.AddForce(jump);
            rb.velocity = jump;
            PlayerAnimator.SetBool("Jump", true);
        }

        if (ctx.performed && wallJumpingCounter > 0f && (startSlideTimer || sliding))
        {
            slideAllowed = false;
            startSlideTimer = false;
            isWallJumping = true;
            Vector3 wallJump = new Vector3(-input * wallJumpForce.x, wallJumpForce.y, 0f); ;
            /*if (!sliding)
            {
                wallJump = new Vector3(-input * wallJumpForce.x, wallJumpForce.y, 0f);
            }
            else { wallJump = new Vector3(-input * wallJumpForce.x/1.2f, wallJumpForce.y * 1.1f, 0f); }*/
            wallJumpingCounter = 0f;
            rb.AddForce(wallJump);
            if (!lockedOn && Mathf.Abs(Velocity.x) >= 0.1f && Mathf.Sign(transform.localScale.x) != Mathf.Sign(Velocity.x)) // this has a check if the player is locked on to prevent them flipping
            {
                transform.localScale = new Vector3(transform.localScale.x * -1, transform.localScale.y, transform.localScale.z);
            }
            Invoke(nameof(stopWallJump), wallJumpingDuration);
        }

        if(ctx.performed && !grounded && !startSlideTimer && !sliding && wallJumpingCounter <= 0f && !doubleJumpPerformed && !PlayerAnimator.GetBool("WallCling"))
        {
            doubleJumpPerformed = true;
            Vector3 jump = new Vector3(rb.velocity.x, jumpForce, 0f);
            rb.velocity = jump;
            PlayerAnimator.SetBool("DoubleJump", true);
        }
    }

    private void wallJump()
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
    }

    public void Dash(InputAction.CallbackContext ctx)
    {
        if (uiOpen) return;
        if (!allowMovement) return;
        if (ctx.performed && grounded && Mathf.Abs(rb.velocity.x) <= 9.8f) // MAKE A COOLDOWN
        {
            Vector3 dashDir = new Vector3(input, 0f, 0f);
            Vector3 dashForce = new Vector3(rb.velocity.x + (Mathf.Sign(input) * dashSpeed * Time.deltaTime), rb.velocity.y, 0f);
            //rb.AddForce(dashDir * dashSpeed * Time.deltaTime, ForceMode.Impulse);
            rb.velocity = dashForce;
            if (input != 0)
            {
                PlayerAnimator.SetBool("Dash", true);
            }
            dashAllowed = false;
            startDashTimer = true;
        }
    }

    public void Update()
    {
        PlayerAnimator.SetBool("WallJump", isWallJumping);
        Velocity = rb.velocity;
        PlayerAnimator.SetFloat("XVelocity", rb.velocity.x);
        PlayerAnimator.SetFloat("YVelocity", rb.velocity.y);
        PlayerAnimator.SetBool("Grounded", grounded);
        
        if (uiOpen) return;
        
        wallJump();

        if (!lockedOn && Mathf.Abs(Velocity.x) >= 0.1f && Mathf.Sign(transform.localScale.x) != Mathf.Sign(Velocity.x)) // this has a check if the player is locked on to prevent them flipping
        {
            transform.localScale = new Vector3(transform.localScale.x * -1, transform.localScale.y, transform.localScale.z);
        }

        
        if (startSlideTimer)
        {
            slideTimer += 10f * Time.deltaTime;
            Debug.Log(slideTimer);
            if(slideTimer >= holdTime)
            {
                startSlide = true;
                slideTimer = 0f;
                startSlideTimer = false;
            }
        }

        if (startDashTimer)
        {
            dashTimer += 10f * Time.deltaTime;
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
        if (!allowMovement) { }
        else
        if (walkAllowed && !isWallJumping)
        {
            if ((rb.velocity.x <= maxSpeed && Mathf.Sign(rb.velocity.x) == 1) || (rb.velocity.x >= -maxSpeed && Mathf.Sign(rb.velocity.x) == -1) || (Mathf.Sign(rb.velocity.x) != input))
            {
                //rb.AddForce((inputDir * acceleration * 1000f) * Time.deltaTime, ForceMode.Force);
                Vector3 walk = new Vector3(input * acceleration, rb.velocity.y, rb.velocity.z);
                rb.velocity = walk;
            }
        }

        if (slideAllowed && !grounded)
        {
            Vector3 wallSlide = new Vector3(0f, -slideSpeed, 0f);
            rb.velocity = wallSlide;
            sliding = true;
            Debug.Log("StartSlide");
        }
        else sliding = false;
    }

    private void OnTriggerStay(Collider other)
    {
        if (uiOpen) return;
        if (other.gameObject.layer == 10 && _playerGroundPosition.transform.position.y > other.gameObject.transform.position.y && Velocity.y <= 0f)
        {
            grounded = true;
            startSlide = false;
            doubleJumpPerformed = false;
            slideAllowed = false;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == 10)
        {
            grounded = false;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.collider.CompareTag("Right Wall") || collision.collider.CompareTag("Left Wall"))
        {
            if (!grounded && input != 0)
            {
                rb.useGravity = false;
                rb.velocity = new Vector3(0f, Velocity.x, 0f);
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
                if (startSlide)
                {
                    slideAllowed = true;
                    Debug.Log("slideAllowed");
                }
            }
            else rb.useGravity = true; startSlide = false; //slideAllowed = false;
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
    }
}
