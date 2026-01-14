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

    [Header("착지 관련 변수")]
    [SerializeField] private bool isGround = false;
    [SerializeField] private LayerMask blockLayer;

    [Header("벽 밀기 관련 변수")]
    public bool isPushing;
    public MovingWall wallScript;
    public PlayerMove frontPlayer;
    public PlayerMove backPlayer;
    private PlayerMove tempPlayer;
    private bool prevIsPushing;

    [Header("리턴 위치")]
    [SerializeField] private Transform returnPos;

    [Header("넉백")]
    private Coroutine knockbackCoroutine;

    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private GroundCheck groundCheck;
    private Vector2 moveInput;

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
        if (knockbackCoroutine == null)
        {
            Move();
        }
        CheckPush();
    }

    private void Move()
    {
        float moveX = moveInput.x * moveSpeed;
        float underMoveX = 0f;

        if (groundCheck.UnderPlayerRb != null)
        {
            underMoveX = groundCheck.UnderPlayerRb.linearVelocity.x;
        }

        rb.linearVelocity = new Vector2(moveX + (isPushing ? 0 : 1) * underMoveX, rb.linearVelocity.y); //더하기
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
        if (collision.gameObject.TryGetComponent<FlatForm>(out var platform) && collision.contacts[0].normal.y > 0.7f)
        {
            platform.AddRider(this);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            tempPlayer = collision.gameObject.GetComponentInParent<PlayerMove>();
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        // 밀격 상태가 아닐 경우 리턴
        if (!isPushing) return;

        // 플레이어와 충돌할 경우, 미는 인원 추가
        if (collision.gameObject.CompareTag("Player") && tempPlayer != null)
        {
            // 내가 밀고 있는 방향에 상대가 있는지
            float dir = Mathf.Sign(moveInput.x);
            float delta = tempPlayer.transform.position.x - transform.position.x;

            if (Mathf.Sign(delta) == dir)
            {
                frontPlayer = tempPlayer;
                tempPlayer.backPlayer = this;

                // 앞사람이 이미 벽과 연결돼 있다면 공유
                if (tempPlayer.wallScript != null)
                {
                    wallScript = tempPlayer.wallScript;
                    wallScript.AddPusher(this);
                }
            }
        }

        if (collision.gameObject.CompareTag("MovingWall"))
        {
            wallScript = collision.gameObject.GetComponent<MovingWall>();
            wallScript.AddPusher(this);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("MovingWall"))
        {
            if (wallScript != null)
            {
                wallScript.RemovePusher(this);
                wallScript = null;
            }
        }
        if (collision.TryGetComponent<FlatForm>(out var platform))
        {
            platform.RemoveRider(this);
        }
    }

    //밀기 체크
    private void CheckPush()
    {
        // 이전 상태 저장
        prevIsPushing = isPushing;
        isPushing = false;

        if (Mathf.Abs(moveInput.x) < 0.01f)
        {
            // input이 없을 경우, 밀기 상태 해제
            EndPushIfNeeded();
            return;
        }

        // 자기 콜라이더 밖에서 레이 시작
        Vector2 dir = new Vector2(Mathf.Sign(moveInput.x), 0f); //바라보는 방향

        BoxCollider2D Col = GetComponent<BoxCollider2D>(); //내 콜라이더
        float offset = Col.size.x / 2f + 0.05f; // 콜라이더 절반 + 살짝 앞에서 발사(자기감지방지)
        Vector2 pos = rb.position + dir * offset;

        RaycastHit2D hit = Physics2D.Raycast(pos, dir, 0.3f, blockLayer);

        if (hit.collider != null && isGround)
        {
            isPushing = true;
        }
        if (prevIsPushing && !isPushing)
        {
            // isPushiong 변경시, 밀기 상태 해제
            EndPushIfNeeded();
        }

        animator.SetBool("IsPush", isPushing);
    }

    private void EndPushIfNeeded()
    {
        // 벽과 연결되어 있으면 해제
        if (wallScript != null)
        {
            wallScript.RemovePusher(this);
            wallScript = null;
        }

        // 앞 / 뒤 플레이어 연결 해제
        if (frontPlayer != null)
        {
            frontPlayer.backPlayer = null;
            frontPlayer = null;
        }

        if (backPlayer != null)
        {
            backPlayer.frontPlayer = null;
            backPlayer = null;
        }
    }

    //자시자신 콜라이더 무시
    private void IgnoreSelfCollision()
    {
        Collider2D[] Cols = GetComponents<Collider2D>(); // 내 콜라이더
        Collider2D[] childCols = GetComponentsInChildren<Collider2D>(); //자식들 콜라이더

        //자식 콜라이더 갯수만큼 가져와서 무시하기
        foreach (var child in childCols)
        {
            if (child.transform == transform) continue; // 자기 자신 제외

            foreach (var parent in Cols)
            {
                Physics2D.IgnoreCollision(parent, child, true);
            }
        }
    }

    // 넉백 함수
    public void Knockback(Vector2 force)
    {
        if (knockbackCoroutine != null)
        {
            StopCoroutine(knockbackCoroutine);
        }
        knockbackCoroutine = StartCoroutine(KnockbackRoutine(force));
    }

    private IEnumerator KnockbackRoutine(Vector2 force)
    {
        rb.linearVelocity = Vector2.zero;
        rb.AddForce(force, ForceMode2D.Impulse);
        yield return new WaitForSeconds(0.35f);
        knockbackCoroutine = null;
    }
}
