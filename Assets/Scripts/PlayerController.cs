using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private Rigidbody2D myRB;
    private SpriteRenderer myRenderer;
    private Animator myAnimator;

    private bool canMove = true;

    private bool facingRight = true;
    // move
    public float maxSpeed;

    // jump
    bool grounded = false;

    float groundCheckRadius = 0.2f;

    public LayerMask groundLayer;
    public Transform groundCheck;
    public float jumpPower;

    // Use this for initialization
    void Start()
    {
        myRB = GetComponent<Rigidbody2D>();
        myRenderer = GetComponent<SpriteRenderer>();
        myAnimator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (canMove && grounded && Input.GetAxis("Jump") > 0)
        {
            myAnimator.SetBool("IsGrounded", false);
            myRB.velocity = new Vector2(myRB.velocity.x, 0f);
            myRB.AddForce(new Vector2(0, jumpPower), ForceMode2D.Impulse);
            grounded = false;
        }

        grounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        myAnimator.SetBool("IsGrounded", grounded);

        float move = Input.GetAxis("Horizontal");

        if (canMove)
        {
            if (move > 0 && !facingRight)
            {
                Flip();
            }
            else if (move < 0 && facingRight)
            {
                Flip();
            }

            myRB.velocity = new Vector2(move * maxSpeed, myRB.velocity.y);
            myAnimator.SetFloat("MoveSpeed", Mathf.Abs(move));
        }
        else
        {
            myRB.velocity = new Vector2(0, myRB.velocity.y);
            myAnimator.SetFloat("MoveSpeed", 0);
        }
    }

    private void Flip()
    {
        facingRight = !facingRight;
        myRenderer.flipX = !myRenderer.flipX;
    }

    public void ToggleCanMove()
    {
        canMove = !canMove;
    }
}
