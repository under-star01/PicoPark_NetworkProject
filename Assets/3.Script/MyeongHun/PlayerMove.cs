using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    [Header("이동 관련 변수")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("점수 관련 변수")]
    [SerializeField] private float jumpForce = 50f;
    
    [Header("충돌 관련 변수")]
    [SerializeField] private bool isGround = false;
    [SerializeField] private LayerMask blockLayer;
    private bool isPushing;

    [Header("리턴 위치")]
    [SerializeField] private Transform returnPos;

    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    [SerializeField] private GroundCheck groundCheck;

    [SerializeField] private Vector2 moveInput;

    private void Awake()
    {
        TryGetComponent(out rb);
        TryGetComponent(out animator);
        TryGetComponent(out spriteRenderer);

        groundCheck = GetComponentInChildren<GroundCheck>();

        IgnoreSelfCollision();
    }

    private void Update()
    {
        isGround = groundCheck.IsGround;
        animator.SetBool("IsGround", isGround);
    }

    private void FixedUpdate()
    {
        Move();
        CheckPush();
    }

    private void Move()
    {
        float moveX = moveInput.x * moveSpeed;

        rb.linearVelocity = new Vector2(moveX, rb.linearVelocity.y);
    }

    public void SetMove(Vector2 input)
    {
        moveInput = input;

        bool isRun = (Mathf.Abs(moveInput.x) > 0.01f);
        animator.SetBool("IsRun", isRun);

        if (isRun)
        {
            spriteRenderer.flipX = moveInput.x < 0;
        }
    }

    public void JumpStart()
    {
        if (!isGround) return;

        rb.linearVelocityY = jumpForce;
    }

    public void JumpStop()
    {
        Vector2 velocity = rb.linearVelocity;

        if (velocity.y > 0f)
        {
            velocity.y *= 0.5f;
            rb.linearVelocity = velocity;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("DeadLine"))
        {
            if (returnPos != null)
            {
                transform.position = returnPos.position;
            }
        }
    }

    private void CheckPush()
    {
        isPushing = false;

        if (Mathf.Abs(moveInput.x) < 0.01f) return;

        Vector2 dir = new Vector2(Mathf.Sign(moveInput.x), 0f);
        Vector2 pos = rb.position;

        RaycastHit2D hit = Physics2D.Raycast(pos, dir, 0.01f, blockLayer);

        if (hit.collider != null && isGround)
        {
            isPushing = true;
        }

        animator.SetBool("IsPush", isPushing);
    }

    private void IgnoreSelfCollision()
    {
        Collider2D[] parentCols = GetComponents<Collider2D>();
        Collider2D[] childCols = GetComponentsInChildren<Collider2D>(true);

        foreach (var child in childCols)
        {
            if (child.transform == transform) continue; // 자기 자신 제외

            foreach (var parent in parentCols)
            {
                Physics2D.IgnoreCollision(parent, child, true);
            }
        }
    }
}
