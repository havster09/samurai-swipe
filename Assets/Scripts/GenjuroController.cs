using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenjuroController : MonoBehaviour
{
    private Rigidbody2D genjuroRB;
    private SpriteRenderer genjuroRenderer;
    private Animator genjuroAnimator;

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
        genjuroRB = GetComponent<Rigidbody2D>();
        genjuroRenderer = GetComponent<SpriteRenderer>();
        genjuroAnimator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (canMove && grounded && Input.GetAxis("Jump") > 0)
        {
            genjuroAnimator.SetBool("IsGrounded", false);
            genjuroRB.velocity = new Vector2(genjuroRB.velocity.x, 0f);
            genjuroRB.AddForce(new Vector2(0, jumpPower), ForceMode2D.Impulse);
            grounded = false;
        }

        if (canMove && grounded && Input.GetButtonDown("Fire1"))
        {
            genjuroAnimator.SetTrigger("IsStandLightSlash");
        }

        if (canMove && grounded && Input.GetButtonDown("Fire2"))
        {
            genjuroAnimator.SetTrigger("IsStandMediumSlash");
        }

        if (canMove && grounded && Input.GetButtonDown("Fire3"))
        {
            genjuroAnimator.SetTrigger("IsStandHardSlash");
        }



        grounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        genjuroAnimator.SetBool("IsGrounded", grounded);

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

            genjuroRB.velocity = new Vector2(move * maxSpeed, genjuroRB.velocity.y);
            genjuroAnimator.SetFloat("MoveSpeed", Mathf.Abs(move));
        }
        else
        {
            genjuroRB.velocity = new Vector2(0, genjuroRB.velocity.y);
            genjuroAnimator.SetFloat("MoveSpeed", 0);
        }
    }

    private void Flip()
    {
        facingRight = !facingRight;
        genjuroRenderer.flipX = !genjuroRenderer.flipX;
    }

    public void ToggleCanMove()
    {
        canMove = !canMove;
    }
}
