using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    [Header("Move")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("Jump")]
    [SerializeField] private float jumpForce = 50f;
    [SerializeField] private float airSpeed = 1f;
    [SerializeField] private bool isGround = false;
    [SerializeField] private int jumpCnt = 1;
    [SerializeField] private float longPressTime = 0.3f; // 길게누름 판정값

    [SerializeField] private Vector2 moveInput;
    
    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private Coroutine holdRoutine;

    [SerializeField] private bool isReadyJump = false;      // 점프 버튼이 눌린 상태

    private void Awake()
    {
        TryGetComponent(out rb);
        TryGetComponent(out animator);
        TryGetComponent(out spriteRenderer);

        rb.interpolation = RigidbodyInterpolation2D.Interpolate; // 해당 기능 알아보기
    }

    private void FixedUpdate()
    {
        CalculateMovePosition();
    }

    private void CalculateMovePosition()
    {
        if (!isGround) return;

        float moveX = moveInput.x * moveSpeed * Time.fixedDeltaTime;
        Vector2 nextPos = rb.position + new Vector2(moveX, 0f);

        rb.MovePosition(nextPos);
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
        if (jumpCnt < 1 || !isGround) return;

        isGround = false;
        isReadyJump = true;

        if (holdRoutine != null)
        {
            StopCoroutine(holdRoutine);
        }
        holdRoutine = StartCoroutine(LongPressCheck());
    }

    private IEnumerator LongPressCheck()
    {
        float t = 0f;

        while (isReadyJump && t < longPressTime)
        {
            t += Time.deltaTime;
            yield return null;
        }

        if (isReadyJump)
        {
            // 상승 중일 때만 추가 점프력 적용
            Vector2 velocity = rb.linearVelocity;

            if (velocity.y > 0f)
            {
                Debug.Log("추가 점프 실행");
                rb.linearVelocity = new Vector2(moveInput.x * airSpeed, jumpForce);
            }
        }
    }

    public void JumpStop()
    {
        isReadyJump = false;

        rb.linearVelocity = new Vector2(moveInput.x * airSpeed * 0.5f, jumpForce);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.CompareTag("Floor"))
        {
            isGround = true;
            jumpCnt = 1;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Floor"))
        {
            isGround = false;
        }
    }
}
