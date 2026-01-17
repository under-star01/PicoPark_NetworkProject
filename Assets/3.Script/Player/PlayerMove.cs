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
    public PlayerMove frontPlayer;
    public PlayerMove backPlayer;
    private NetworkIdentity candidateWallNetId; // 밀 수 있는 후보 벽
    private NetworkIdentity currentWallNetId; // 현재 밀고 있는 벽의 NetworkIdentity

    [Header("플랫폼 관련 변수")]
    [SerializeField] private bool isOnPlatform = false;
    [SerializeField] private Transform currentPlatform;
    public CeilCheck ceilCheck;
    public int stackCnt => ceilCheck.ceilingCnt;
    
    [Header("리턴 위치")]
    [SerializeField] private Transform returnPos;

    [Header("넉백")]
    private Coroutine knockbackCoroutine;

    [Header("문 안 상태")]
    private bool isInsideDoor = false;

    [Header("죽음")]
    private bool isDead = false;

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

    public override void OnStartServer()
    {
        rb.bodyType = RigidbodyType2D.Dynamic;
    }

    public override void OnStartClient()
    {
        if (isLocalPlayer)
        {
            rb.simulated = true;   // 내 캐릭터는 물리 켬
        }
        else
        {
            rb.simulated = false;  // 남 캐릭터는 서버 결과만
        }
    }

    private void Update()
    {
        if (!isLocalPlayer) return;
        if (isDead) return;

        isGround = groundCheck.IsGround;
        animator.SetBool("IsGround", isGround);
    }

    private void FixedUpdate()
    {
        if (isDead) return;

         // 서버 전용 처리
        if (isServer)
        {
            ApplyPlatformVelocity_Server();
        }

        // 로컬 입력 처리
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

        rb.linearVelocity = new Vector2(moveX + (isPushing ? 0 : 1) * underMoveX, rb.linearVelocity.y);
    }

    public void SetMove(Vector2 input)
    {
        if (!isLocalPlayer) return;

        // 문 안에 있으면 입력 무시
        if (isInsideDoor) return;

        if (isDead || isInsideDoor) return;

        moveInput = input;

        bool isRun = (Mathf.Abs(moveInput.x) > 0.01f);
        animator.SetBool("IsRun", isRun);

        if (isRun)
        {
            Vector2 originScale = transform.localScale;
            float direction = moveInput.x > 0 ? 1f : -1f;
            transform.localScale = new Vector2(direction * Mathf.Abs(originScale.x), originScale.y);
        }
    }

    public void JumpStart()
    {
        if (!isGround) return;
        if (isDead || isInsideDoor) return;

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

        if (collision.gameObject.CompareTag("PlatForm"))
        {
            if (collision.contacts[0].normal.y > 0.7f)
            {
                NetworkIdentity netId = collision.transform.GetComponent<NetworkIdentity>();

                if (netId != null)
                {
                    CmdEnterPlatform(netId);
                }
            }
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (!isLocalPlayer) return;

        if (collision.CompareTag("MovingWall"))
        {
            if (candidateWallNetId == null)
            {
                candidateWallNetId = collision.GetComponent<NetworkIdentity>();
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!isLocalPlayer) return;

        if (collision.CompareTag("MovingWall"))
        {
            if (candidateWallNetId == collision.GetComponent<NetworkIdentity>())
            {
                candidateWallNetId = null;
            }
        }

        if (collision.gameObject.CompareTag("PlatForm"))
        {
            NetworkIdentity netId = collision.transform.GetComponent<NetworkIdentity>();

            if (netId != null)
            {
                CmdExitPlatform(netId);
            }
        }
    }

    // MovingWall를 미는 Pusher추가
    [Command]
    private void CmdAddPusher(NetworkIdentity wallNetId)
    {
        if (wallNetId == null) return;

        MovingWall wall = wallNetId.GetComponent<MovingWall>();
        if (wall == null) return;

        wall.AddPusher(this);
    }

    // MovingWall를 미는 Pusher 해제
    [Command]
    private void CmdRemovePusher(NetworkIdentity wallNetId)
    {
        if (wallNetId == null) return;

        MovingWall wall = wallNetId.GetComponent<MovingWall>();
        if (wall == null) return;

        wall.RemovePusher(this);
    }

    // 플랫폼과 충돌 Command 
    [Command]
    private void CmdEnterPlatform(NetworkIdentity platformNetId)
    {
        if (platformNetId == null) return;

        PlatForm platform = platformNetId.GetComponent<PlatForm>();
        if (platform == null) return;

        platform.ServerAttachPlayer(this);
    }

    // 플랫폼과 충동 해제 Command
    [Command]
    private void CmdExitPlatform(NetworkIdentity platformNetId)
    {
        if (platformNetId == null) return;

        PlatForm platform = platformNetId.GetComponent<PlatForm>();
        if (platform == null) return;

        platform.ServerDetachPlayer(this);

    }

    // 서버 전용 플랫폼 보정 메소드
    [Server]
    private void ApplyPlatformVelocity_Server()
    {
        if (!isOnPlatform || currentPlatform == null) return;

        Rigidbody2D platformRb = currentPlatform.GetComponent<Rigidbody2D>();
        if (platformRb == null) return;

        rb.linearVelocity = new Vector2(rb.linearVelocity.x + platformRb.linearVelocity.x, rb.linearVelocity.y);
    }

    [Server]
    public void SetOnPlatform_Server(Transform platform)
    {
        isOnPlatform = true;
        currentPlatform = platform;

        TargetSetOnPlatform(connectionToClient, platform.GetComponent<NetworkIdentity>());
    }

    [Server]
    public void ClearOnPlatform_Server()
    {
        isOnPlatform = false;
        currentPlatform = null;

        TargetClearOnPlatform(connectionToClient);
    }

    [TargetRpc]
    private void TargetSetOnPlatform(NetworkConnection target, NetworkIdentity platformNetId)
    {
        currentPlatform = platformNetId.transform;
    }

    [TargetRpc]
    private void TargetClearOnPlatform(NetworkConnection target)
    {
        currentPlatform = null;
    }

    //밀기 체크
    private void CheckPush()
    {
        bool prev = isPushing;
        bool wantPush = false;

        // 1. 입력 없으면 무조건 해제
        if (Mathf.Abs(moveInput.x) < 0.01f)
        {
            EndPush();
            animator.SetBool("IsPush", false);
            return;
        }

        // 2. 직접 벽 미는 Raycast
        Vector2 dir = new Vector2(Mathf.Sign(moveInput.x), 0f);
        BoxCollider2D col = GetComponent<BoxCollider2D>();
        float offset = col.size.x / 2f + 0.05f;
        Vector2 pos = rb.position + dir * offset;

        RaycastHit2D hit = Physics2D.Raycast(pos, dir, 0.3f, blockLayer);
        if (hit.collider != null && isGround)
        {
            candidateWallNetId = hit.collider.GetComponent<NetworkIdentity>();
            wantPush = candidateWallNetId != null;
        }

        // 3. 앞 플레이어를 통해 미는 경우
        if (!wantPush && frontPlayer != null && frontPlayer.currentWallNetId != null)
        {
            candidateWallNetId = frontPlayer.currentWallNetId;
            wantPush = true;
        }

        // 4. 상태 전이 감지
        if (!prev && wantPush)
        {
            StartPush();
        }
        else if (prev && !wantPush)
        {
            EndPush();
        }

        isPushing = wantPush;
        animator.SetBool("IsPush", isPushing);
    }

    private void StartPush()
    {
        if (candidateWallNetId == null) return;

        if (currentWallNetId == candidateWallNetId) return;

        currentWallNetId = candidateWallNetId;
        CmdAddPusher(currentWallNetId);
    }

    private void EndPush()
    {
        if (!isLocalPlayer) return;

        isPushing = false;

        if (currentWallNetId != null)
        {
            CmdRemovePusher(currentWallNetId);
            currentWallNetId = null;
        }

        candidateWallNetId = null;

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

    //자기 자신 콜라이더 무시
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

    public void Die()
    {
        if (isDead) return; // 죽었으면 나가

        isDead = true;

        // Rigidbody를 Kinematic으로 변경
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.linearVelocity = Vector2.zero;
        SetMove(Vector2.zero);

        // 다른 애니메이션 파라미터 초기화
        animator.SetBool("IsGround", true);
        animator.SetBool("IsRun", false);
        animator.SetBool("IsPush", false);
        animator.SetTrigger("Dead"); // Dead 트리거

        // 콜라이더 끄기
        Collider2D[] allColliders = GetComponentsInChildren<Collider2D>();
        foreach (var col in allColliders)
        {
            col.enabled = false;
        }

        // 모자 끄기
        PlayerCustom custom = GetComponent<PlayerCustom>();
        if (custom != null)
        {
            custom.HideHat();
        }

        // 죽는 애니메이션
        StartCoroutine(DeathAnimation());
    }

    // 애니메이터로 안돼서 코드로 구현한 애니메이션..
    private IEnumerator DeathAnimation()
    {
        Vector3 startPos = transform.position;

        // 1초 멈춤
        yield return new WaitForSeconds(1f);

        float time = 0f;
        while (time < 0.3f)
        {
            time += Time.deltaTime;
            transform.position = startPos + Vector3.up * (time / 0.3f) * 1.2f; //0.3초 동안 1.2만큼 올라감
            yield return null;
        }

        Vector3 topPos = transform.position;
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 10f;

        //time = 0f;
        //while (time < 4f)
        //{
        //    time += Time.deltaTime;
        //    transform.position = topPos - Vector3.up * (time / 5f) * 30f; // 초 동안 30만큼 내려감
        //    yield return null;
        //}

        // (여기에 게임오버 넣으면 됩니다)
        Debug.Log("게임오버!");
    }
}