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
    [SerializeField] private float airSpeed = 1f;
    [SerializeField] private int jumpCnt = 1;
    
    [Header("충돌 관련 변수")]
    [SerializeField] private HashSet<Collider2D> groundSet = new HashSet<Collider2D>(); // 중복방지, 빠른 검색을 위해서 HashSet를 사용했어!
    [SerializeField] private bool isGround = false; 
    private bool IsGround
    {
        get => isGround;
        set
        {
            // 값 변경시에만 적용
            if (isGround == value) return;
            
            isGround = value;

            // 착지 상태에 따라 상태 변경
            if (isGround) animator.SetTrigger("Land");
        }
    }

    [Header("리턴 위치")]
    [SerializeField] private Transform returnPos;

    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    [SerializeField] private Vector2 moveInput;

    private void Awake()
    {
        TryGetComponent(out rb);
        TryGetComponent(out animator);
        TryGetComponent(out spriteRenderer);
    }

    private void FixedUpdate()
    {
        CalculateMovePosition();
    }

    private void CalculateMovePosition()
    {
        if (IsGround)
        {
            float moveX = moveInput.x * moveSpeed * Time.fixedDeltaTime;
            Vector2 nextPos = rb.position + new Vector2(moveX, 0f);

            rb.MovePosition(nextPos);
        }
        else
        {
            float moveX = moveInput.x * airSpeed;
            rb.linearVelocityX = moveX;
        }
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
        if (jumpCnt < 1 || !IsGround) return;

        IsGround = false;
        rb.linearVelocityY = jumpForce;

        animator.SetTrigger("Jump");
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
        if(collision.gameObject.CompareTag("Floor") || collision.gameObject.CompareTag("Player"))
        {
            // 위에서 아래 방향으로 충돌시에만, 착지 판정
            foreach(var col in collision.contacts)
            {
                if (col.normal.y > 0.7f)
                {
                    // 충돌중인 바닥 콜라이더 저장
                    groundSet.Add(col.collider);
                    IsGround = true;
                }
            }
        }

        if (collision.gameObject.CompareTag("DeadLine"))
        {
            if (returnPos != null)
            {
                transform.position = returnPos.position;
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Floor") || collision.gameObject.CompareTag("Player"))
        {
            // 충돌중인 바닥 콜라이더 삭제
            groundSet.Remove(collision.collider);

            // 충돌하고 있는 오브젝트가 없을 경우 변경
            if (groundSet.Count <= 0)
            {
                IsGround = false;
            }
        }
    }
}
