using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class PlayerMove : NetworkBehaviour
{
    [Header("이동 관련 변수")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("점수 관련 변수")]
    [SerializeField] private float jumpForce = 50f;

    [Header("착지 관련 변수")]
    public GroundCheck groundCheck;
    [SerializeField] private bool isGround = false;
    [SerializeField] private LayerMask blockLayer;

    [Header("벽 밀기 관련 변수")]
    public bool isPushing;
    public MovingWall wallScript;
    public PlayerMove frontPlayer;
    public PlayerMove backPlayer;
    private PlayerMove detectPlayer;
    private bool prevIsPushing; 
    private NetworkIdentity currentWallNetId;

    [Header("플랫폼 관련 변수")]
    public CeilCheck ceilCheck;
    public int stackCnt => ceilCheck.ceilingCnt;

    [Header("리턴 위치")]
    [SerializeField] private Transform returnPos;

    [Header("넉백")]
    private Coroutine knockbackCoroutine;

    [Header("문 안 상태")]
    private bool isInsideDoor = false;

    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private Vector2 moveInput;

    private void Awake()
    {
        TryGetComponent(out rb);
        TryGetComponent(out animator);
        TryGetComponent(out spriteRenderer);

        groundCheck = GetComponentInChildren<GroundCheck>();
        ceilCheck = GetComponentInChildren<CeilCheck>();

        IgnoreSelfCollision();
    }

    private void Update()
    {
        if (!isLocalPlayer) return;
        isGround = groundCheck.IsGround;
        animator.SetBool("IsGround", isGround);
    }

    private void FixedUpdate()
    {
        if (!isLocalPlayer) return;
        if (knockbackCoroutine == null && !isInsideDoor)
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
        if (!isLocalPlayer) return;

        // 문 안에 있으면 입력 무시
        if (isInsideDoor) return;

        moveInput = input;

        bool isRun = (Mathf.Abs(moveInput.x) > 0.01f);
        animator.SetBool("IsRun", isRun);

        if (isRun)
        {
            //spriteRenderer.flipX = moveInput.x < 0;
            float direction = moveInput.x > 0 ? 1f : -1f;
            transform.localScale = new Vector3(direction, 1f, 1f);
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
        if (!isLocalPlayer) return;

        if (collision.gameObject.CompareTag("DeadLine"))
        {
            if (returnPos != null)
            {
                transform.position = returnPos.position;
            }
        }
        if (collision.gameObject.CompareTag("FlatForm") && collision.contacts[0].normal.y > 0.7f)
        {
            CmdAddRider(collision.gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            detectPlayer = collision.gameObject.GetComponentInParent<PlayerMove>();
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (!isLocalPlayer) return;

        // 밀격 상태가 아닐 경우 리턴
        if (!isPushing) return;

        // 플레이어와 충돌할 경우, 미는 인원 추가
        if (collision.gameObject.CompareTag("Player") && detectPlayer != null)
        {
            // 밀격 인원 확인 -> 내가 밀고 있는 방향에 상대가 있는지
            float dir = Mathf.Sign(moveInput.x); // 부호만 확인
            float detectDir = Mathf.Sign(detectPlayer.transform.position.x - transform.position.x);

            if (detectDir == dir)
            {
                frontPlayer = detectPlayer;
                detectPlayer.backPlayer = this;
            }
        }

        if (collision.gameObject.CompareTag("MovingWall"))
        {
            if (currentWallNetId != null) return;

            NetworkIdentity wallNetId = collision.GetComponent<NetworkIdentity>();
            if (wallNetId != null)
            {
                currentWallNetId = wallNetId;
                CmdStartPush(wallNetId);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!isLocalPlayer) return;

        if (collision.CompareTag("MovingWall"))
        {
            if (currentWallNetId != null)
            {
                CmdStopPush(currentWallNetId);
                currentWallNetId = null;
            }
        }

        if (collision.CompareTag("FlatForm"))
        {
            CmdRemoveRider(collision.gameObject);
        }
    }

    // FlatForm에 인원수 추가 Command
    [Command]
    private void CmdAddRider(GameObject platformObj)
    {
        FlatForm platform = platformObj.GetComponent<FlatForm>();
        if (platform == null) return;

        platform.AddRider(this);
    }

    // FlatForm에 인원수 감소 Command
    [Command]
    private void CmdRemoveRider(GameObject platformObj)
    {
        FlatForm platform = platformObj.GetComponent<FlatForm>();
        if (platform == null) return;

        platform.RemoveRider(this);
    }

    // MovingWall 이동 시작 Command
    [Command]
    private void CmdStartPush(NetworkIdentity wallNetId)
    {
        if (wallNetId == null) return;

        MovingWall wall = wallNetId.GetComponent<MovingWall>();
        if (wall == null) return;

        wall.AddPusher(this);
        wallScript = wall;
    }

    // MovingWall 이동 종료 Command
    [Command]
    private void CmdStopPush(NetworkIdentity wallNetId)
    {
        if (wallNetId == null) return;

        MovingWall wall = wallNetId.GetComponent<MovingWall>();
        if (wall == null) return;

        wall.RemovePusher(this);

        if (wallScript == wall)
            wallScript = null;
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
            EndPush();
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
            EndPush();
        }

        animator.SetBool("IsPush", isPushing);
    }

    private void EndPush()
    {
        if (!isLocalPlayer) return;

        if (currentWallNetId != null)
        {
            CmdStopPush(currentWallNetId);
            currentWallNetId = null;
        }

        // 어부바 연결 해제
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

    public void SetInsideDoor(bool inside)
    {
        isInsideDoor = inside;
    }
}
