using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public Transform groundCheck;
    public Transform wallCheck;

    [SerializeField]
    private float movementSpeed = 10f;
    [SerializeField]
    private float groundCheckRadius;
    [SerializeField]
    private float wallCheckDistance;
    [SerializeField]
    private float wallSlidingSpeed;
    [SerializeField]
    private LayerMask groundMask;
    [SerializeField]
    private int amountOfJumps = 1;
    private int jumpsLeft;
    private float movementInputDirection;
    private float jumpForce = 16.0f;
    private bool isFacingRight = true;
    private bool isWalking;
    private bool isGrounded;
    private bool canJump;
    private bool isTouchingWall;
    private bool isWallSliding;

    private Rigidbody2D rb2d;
    private Animator animator;

    void Start()
    {
        rb2d = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        jumpsLeft = amountOfJumps;
    }

    void Update()
    {
        CheckInput();
        CheckFacingDirection();
        UpdateAnimationState();
        CheckJump();
        CheckWallSliding();
    }

    private void FixedUpdate()
    {
        Movement();
        CheckObstacles();
    }

    private void UpdateAnimationState()
    {
        animator.SetBool("isWalking", isWalking);
        animator.SetBool("isGrounded", isGrounded);
        animator.SetFloat("yVelocity", rb2d.velocity.y);
    }
    private void CheckInput()
    {
        movementInputDirection = Input.GetAxisRaw("Horizontal");
        if (Input.GetButtonDown("Jump")) 
        { 
            Jump();
        }
    }

    private void CheckJump()
    {
        if (isGrounded)
        {
            jumpsLeft = amountOfJumps;
        }
        if(jumpsLeft <= 0)
        {
            canJump = false;
        }
        else
        {
            canJump = true;
        }
    }

    private void Movement()
    {
        rb2d.velocity = new Vector2(movementSpeed * movementInputDirection, rb2d.velocity.y);
        if(isWallSliding)
        {
            if(rb2d.velocity.y < -wallSlidingSpeed)
            {
                rb2d.velocity = new Vector2(rb2d.velocity.x, -wallSlidingSpeed);
            }
        }
    }

    private void CheckFacingDirection()
    {
        if(isFacingRight && movementInputDirection < 0)
        {
            Flip();
        }
        else if(!isFacingRight && movementInputDirection > 0)
        {
            Flip();
        }
        if(rb2d.velocity.x != 0)
        {
            isWalking = true;
        }
        else
        {
            isWalking = false;
        }
    }

    private void Jump()
    {
        if (canJump)
        {
            rb2d.velocity = new Vector2(rb2d.velocity.x, jumpForce);
            jumpsLeft--;
        }
    }
    private void CheckObstacles()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundMask);

        isTouchingWall = Physics2D.Raycast(wallCheck.position, transform.right, wallCheckDistance, groundMask);
    }

    private void CheckWallSliding()
    {
        if(isTouchingWall && !isGrounded && rb2d.velocity.y < 0)
        {
            isWallSliding = true;
        }
        else
        {
            isWallSliding = false;
        }
    }

    private void Flip()
    {
        isFacingRight = !isFacingRight;
        transform.Rotate(0.0f, 180.0f, 0.0f);
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        Gizmos.DrawLine(wallCheck.position, new Vector3(wallCheck.position.x + wallCheckDistance, wallCheck.position.y, wallCheck.position.z));
    }
}
