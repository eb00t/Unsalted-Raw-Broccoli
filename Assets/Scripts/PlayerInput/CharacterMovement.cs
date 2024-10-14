using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CharacterMovement : MonoBehaviour
{
    Rigidbody rb;
    BoxCollider groundCheck;
    bool walkAllowed = true;

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
    private bool startSlide;
    private bool startSlideTimer;
    public bool slideAllowed = false;
    private float timer = 0f;

    public void Awake()
    {
        rb = GetComponent<Rigidbody>();
        groundCheck = GetComponent<BoxCollider>();
    }

    public void Crouch(InputAction.CallbackContext ctx)
    {

    }

    public void XAxis(InputAction.CallbackContext ctx)
    {
        
            input = ctx.ReadValue<float>();


            //Debug.Log(input);
    }

    public void Jump(InputAction.CallbackContext ctx)
    {
        if (ctx.performed && !doubleJumpPerformed)
        {
            Debug.Log("Jump");
            Vector3 jump = new Vector3(0f, jumpForce, 0f);
            rb.AddForce(jump);
        }

        if(ctx.performed && !grounded)
        {
            doubleJumpPerformed = true;
        }
    }

    public void Dash(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            Vector3 dashDir = new Vector3(input, 0f, 0f);
            rb.AddForce(dashDir * dashSpeed, ForceMode.Impulse);
        }
    }

    public void Update()
    {
        Velocity = rb.velocity;


        Vector3 inputDir = new Vector3(input, 0f, 0f);

        if (walkAllowed)
        {
            if ((rb.velocity.x <= maxSpeed && Mathf.Sign(rb.velocity.x) == 1) || (rb.velocity.x >= -maxSpeed && Mathf.Sign(rb.velocity.x) == -1) || (Mathf.Sign(rb.velocity.x) != input))
            {
                rb.AddForce((inputDir * acceleration * 1000f) * Time.deltaTime, ForceMode.Force);
            }
        }

        if (slideAllowed && !grounded)
        {
            Vector3 wallSlide = new Vector3(0f, -slideSpeed, 0f);
            rb.AddForce(wallSlide);
                //rb.AddForce(-rb.velocity.x, 0f, 0f);
            Debug.Log("StartSlide");
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

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Bottom Wall"))
        {
            grounded = true;
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
        }
    }
}
