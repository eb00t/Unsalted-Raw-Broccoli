using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CharacterMovement : MonoBehaviour
{
    Rigidbody rb;
    BoxCollider groundCheck;
    Animator PlayerAnimator;
    bool walkAllowed = true;

    public bool allowMovement = true;

    public float acceleration = 1f;
    public float maxSpeed = 10f;
    public float jumpForce = 1f;
    public float dashSpeed = 1f;
    public float slideSpeed = 1f;
    public float holdTime = 1f;

    [SerializeField] private bool grounded;
    [SerializeField] private bool doubleJumpPerformed;
    [SerializeField] private Vector3 Velocity;

    private float input;
    [SerializeField] private bool startSlide;
    [SerializeField] private bool startSlideTimer;
    [SerializeField] private bool sliding = false;
    public bool slideAllowed = false;
    [SerializeField] private float timer = 0f;

    [SerializeField] bool isWallJumping = false;
    [SerializeField] private float wallJumpingCounter;
    [SerializeField] private float wallJumpingTime = 0.2f;
    [SerializeField] private Vector2 wallJumpForce = new Vector2(300f, 300f);
    [SerializeField] private float wallJumpingDuration = 0.4f;

    public void Awake()
    {
        rb = GetComponent<Rigidbody>();
        groundCheck = GetComponent<BoxCollider>();
        PlayerAnimator = GetComponentInChildren<Animator>();
    }

    public void Crouch(InputAction.CallbackContext ctx)
    {

    }

    public void XAxis(InputAction.CallbackContext ctx)
    {
        input = ctx.ReadValue<float>();
        PlayerAnimator.SetFloat("Input", input);
        
    }

    public void Jump(InputAction.CallbackContext ctx)
    {
        if(!allowMovement) { }
            else
        if (ctx.performed && !doubleJumpPerformed && !startSlideTimer && !sliding)
        {
            Debug.Log("Jump");
            Vector3 jump = new Vector3(0f, jumpForce, 0f);
            rb.AddForce(jump);
            PlayerAnimator.SetBool("Jump", true);
        }

        if (ctx.performed && wallJumpingCounter > 0f)
        {
            slideAllowed = false;
            startSlideTimer = false;
            isWallJumping = true;
            Vector3 wallJump = new Vector3(-input * wallJumpForce.x, wallJumpForce.y, 0f);
            wallJumpingCounter = 0f;
            rb.AddForce(wallJump);
            Invoke(nameof(stopWallJump), wallJumpingDuration);
        }

        if(ctx.performed && !grounded && !startSlideTimer && !sliding && wallJumpingCounter < 0f)
        {
            doubleJumpPerformed = true;
            PlayerAnimator.SetBool("DoubleJump", true);
        }
    }

    private void wallJump()
    {
        if (slideAllowed || startSlideTimer)
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
        isWallJumping = false;
    }

    public void Dash(InputAction.CallbackContext ctx)
    {
        if (ctx.performed && grounded)
        {
            Vector3 dashDir = new Vector3(input, 0f, 0f);
            rb.AddForce(dashDir * dashSpeed * Time.deltaTime, ForceMode.Impulse);
            if (input != 0)
            {
                PlayerAnimator.SetBool("Dash", true);
            }
        }
    }

    public void Update()
    {
        Velocity = rb.velocity;
        PlayerAnimator.SetFloat("XVelocity", rb.velocity.x);
        PlayerAnimator.SetFloat("YVelocity", rb.velocity.y);
        PlayerAnimator.SetBool("Grounded", grounded);

        wallJump();

        if (input != 0 && Mathf.Sign(transform.localScale.x) != Mathf.Sign(rb.velocity.x))
        {
            transform.localScale = new Vector3(transform.localScale.x * -1, transform.localScale.y, transform.localScale.z);
        }

        

        

        if (startSlideTimer)
        {
            timer += 10f * Time.deltaTime;
            Debug.Log(timer);
            if(timer >= holdTime)
            {
                startSlide = true;
                timer = 0f;
                startSlideTimer = false;
            }
        }
    }

    public void FixedUpdate()
    {
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
        if (other.CompareTag("Bottom Wall"))
        {
            grounded = true;
            startSlide = false;
            doubleJumpPerformed = false;
            slideAllowed = false;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Bottom Wall"))
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
            startSlide = false;
            slideAllowed = false;
            sliding = false;
            PlayerAnimator.SetBool("WallCling", false);
        }
    }
}
