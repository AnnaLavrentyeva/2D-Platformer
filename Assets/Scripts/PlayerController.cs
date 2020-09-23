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
    private float jumpForce = 20.0f;
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
    [SerializeField]
    private float airMovementSpeed;
    [SerializeField]
    private float airMultiplier = 0.5f;
    [SerializeField]
    private float jumpHeightMult = 0.5f;
    [SerializeField]
    private float wallHopForce;
    [SerializeField]
    private float wallJumpForce;
    [SerializeField]
    private float jumpTimerSet = 0.15f;
    [SerializeField]
    private float turnTimerSet;
    [SerializeField]
    private float wallJumpTimerSet = 0.5f;
    [SerializeField]
    private Vector2 wallHopDirection;
    [SerializeField]
    private Vector2 wallJumpDirection;

    private int jumpsLeft;
    private int facingDirection = 1;
    private int lastWallJumpDirection;

    private float movementInputDirection;
    private float jumpTimer;
    private float turnTimer;
    private float wallJumpTimer;
    private bool isFacingRight = true;

    private bool isWalking;

    private bool isGrounded;
    private bool canNormalJump;
    private bool canWallJump;
    private bool isTouchingWall;
    private bool isWallSliding;
    private bool isDoingJump;
    private bool checkJumpMult;
    private bool canMove;
    private bool canFlip;
    private bool hasWallJumped;

    private Rigidbody2D rb2d;
    private Animator animator;

    void Start()
    {
        rb2d = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        jumpsLeft = amountOfJumps;
        wallHopDirection.Normalize();
        wallJumpDirection.Normalize();
    }

    void Update()
    {
        Movement();

        CheckInput();
        CheckFacingDirection();
        UpdateAnimationState();
        CheckWallSliding();
        CheckJump();
        Jump();
    }

    private void FixedUpdate()
    {
        CheckObstacles();
    }

    private void UpdateAnimationState()
    {
        animator.SetBool("isWalking", isWalking);
        animator.SetBool("isGrounded", isGrounded);
        animator.SetFloat("yVelocity", rb2d.velocity.y);
        animator.SetBool("isWallSliding", isWallSliding);
    }
    private void CheckInput()
    {
        movementInputDirection = Input.GetAxisRaw("Horizontal");
        if (Input.GetButtonDown("Jump")) 
        { 
            if(isGrounded || (jumpsLeft > 0 && isTouchingWall))
            {
                NormalJump();
            }
            else
            {
                jumpTimer = jumpTimerSet;
                isDoingJump = true;
            }
        }
        if(Input.GetButtonDown("Horizontal") && isTouchingWall)
        {
            if(!isGrounded && movementInputDirection != facingDirection)
            {
                canMove = false;
                canFlip = false;

                turnTimer = turnTimerSet;
            }
        }

        if (!canMove)
        {
            turnTimer -= Time.deltaTime;
        }
        if(turnTimer <= 0)
        {
            canMove = true;
            canFlip = true;
        }

        if (checkJumpMult && !Input.GetButton("Jump"))
        {
            checkJumpMult = false;
            rb2d.velocity = new Vector2(rb2d.velocity.x, rb2d.velocity.y * jumpHeightMult);
        }
    }

    private void CheckJump()
    {
        if (isGrounded)
        {
            jumpsLeft = amountOfJumps;
        }
        if (isTouchingWall)
        {
            canWallJump = true;
        }
        if(jumpsLeft <= 0)
        {
            canNormalJump = false;
        }
        else
        {
            canNormalJump = true;
        }
    }

    private void Movement()
    {


        if(!isGrounded && !isWallSliding && movementInputDirection == 0)
        {
            rb2d.velocity = new Vector2(rb2d.velocity.x * airMultiplier, rb2d.velocity.y);
        }

        else if(canMove)
        {
            rb2d.velocity = new Vector2(movementSpeed * movementInputDirection, rb2d.velocity.y);
        }
        
        if (isWallSliding)
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
        if(jumpTimer > 0)
        {
            if(!isGrounded && isTouchingWall && movementInputDirection !=0 && movementInputDirection != facingDirection)
            {
                WallJump();
            }
            else if (isGrounded)
            {
                NormalJump();
            }
        }
        if(isDoingJump)
        {
            jumpTimer -= Time.deltaTime;
        }
        if(wallJumpTimer > 0)
        {
            if (hasWallJumped && movementInputDirection == -lastWallJumpDirection)
            {
                rb2d.velocity = new Vector2(rb2d.velocity.x, 0.0f);
                hasWallJumped = false;
            }
            else if (wallJumpTimer <= 0)
            {
                hasWallJumped = false;

            }
            else
            {
                wallJumpTimer -= Time.deltaTime;
            }
        }
        
    }

    private void NormalJump()
    {
        if (canNormalJump)
        {
            rb2d.velocity = new Vector2(rb2d.velocity.x, jumpForce);
            jumpsLeft--;
            jumpTimer = 0;
            isDoingJump = false;
            checkJumpMult = true;
        }
    }
    private void WallJump()
    {
        if (canWallJump)
        {
            rb2d.velocity = new Vector2(rb2d.velocity.x, 0.0f);
            isWallSliding = false;
            jumpsLeft = amountOfJumps;
            jumpsLeft--;
            Vector2 addToForce = new Vector2(wallJumpForce * wallJumpDirection.x * movementInputDirection, wallHopForce * wallJumpDirection.y);
            rb2d.AddForce(addToForce, ForceMode2D.Impulse);
            jumpTimer = 0;
            isDoingJump = false;
            checkJumpMult = true;
            turnTimer = 0;
            canMove = true;
            canFlip = true;
            hasWallJumped = true;
            wallJumpTimer = wallJumpTimerSet;
            lastWallJumpDirection = -facingDirection;
        }
    }
    private void CheckObstacles()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundMask);

        isTouchingWall = Physics2D.Raycast(wallCheck.position, transform.right, wallCheckDistance, groundMask);
    }

    private void CheckWallSliding()
    {
        if(isTouchingWall && movementInputDirection == facingDirection && rb2d.velocity.y < 0)
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
        if (!isWallSliding && canFlip)
        {
            facingDirection *= -1;
            isFacingRight = !isFacingRight;
            transform.Rotate(0.0f, 180.0f, 0.0f);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        Gizmos.DrawLine(wallCheck.position, new Vector3(wallCheck.position.x + wallCheckDistance, wallCheck.position.y, wallCheck.position.z));
    }
}
