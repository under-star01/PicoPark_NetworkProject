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

    [Header("벽 밀기 관련 변수")]
    public bool isInputPushing; // 밀려고 입력 중인 상태
    public bool isContributingPush; // 벽 밀기에 기여중인지 상태
    public PlayerMove frontPlayer;
    private bool touchingWall;

    [Header("플랫폼 관련 변수")]
    public CeilCheck ceilCheck;
    public int stackCnt => ceilCheck.ceilingCnt;

    [Header("SyncVar 변수")]
    [SyncVar(hook = nameof(OnRunChanged))]
    private bool syncIsRun;

    [SyncVar(hook = nameof(OnPushChanged))]
    private bool syncIsPush;

    [SyncVar(hook = nameof(OnGroundChanged))]
    private bool syncIsGround;

    [SyncVar(hook = nameof(OnFlipChanged))]
    private int syncFlipDir;

    [SyncVar(hook = nameof(OnColorChanged))]
    private int colorIndex;

    [SyncVar(hook = nameof(OnHatChanged))]
    private int hatIndex;

    [SyncVar]
    public bool inputLocked; // UI표시

    [Header("리턴 위치")]
    [SerializeField] private Transform returnPos;

    [Header("넉백")]
    public Coroutine knockbackCoroutine;

    [Header("문 안 상태")]
    private bool isInsideDoor = false;

    [Header("죽음")]
    private bool isDead = false;



    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private PlayerCustom playerCustom;
    private Vector2 moveInput;

    private void Awake()
    {
        TryGetComponent(out rb);
        TryGetComponent(out animator);
        TryGetComponent(out spriteRenderer);
        TryGetComponent(out playerCustom);

        groundCheck = GetComponentInChildren<GroundCheck>();
        ceilCheck = GetComponentInChildren<CeilCheck>();

        IgnoreSelfCollision();

        isDead = false;
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        ApplyAppearance();
    }

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();

        var data = LobbyCustomCache.Instance.myCustomizeData;
        OnlineMenu_UIManager.Instance?.RegisterLocalPlayer(this);
        CmdSetAppearance(data.colorIndex, data.hatIndex);
    }

    [Command]
    private void CmdSetAppearance(int color, int hat)
    {
        colorIndex = color;
        hatIndex = hat;
    }

    private void ApplyAppearance()
    {
        if (playerCustom == null)
            playerCustom = GetComponent<PlayerCustom>();

        playerCustom.SetAppearance(colorIndex, hatIndex);
    }

    private void Update()
    {
        if (!isClient) return;
        if (isDead) return;

        animator.SetBool("IsGround", groundCheck.IsGround);
    }

    [ServerCallback]
    private void FixedUpdate()
    {

        if (isServer)
        {
            ServerMove();
        }
        else if (isClient && isOwned)
        {
            ClientPredictMove();
        }
    }

    [Server]
    private void ServerMove()
    {
        if (inputLocked) return;
        if (isDead) return;

        if (knockbackCoroutine != null || isInsideDoor) return;

        CheckPush();
        Move();

        // 서버에서 애니메이션 상태 계산
        syncIsRun = Mathf.Abs(moveInput.x) > 0.01f;
        syncIsPush = isContributingPush;
        syncIsGround = groundCheck.IsGround;

        // 방향 결정
        if (Mathf.Abs(moveInput.x) > 0.01f)
        {
            syncFlipDir = (moveInput.x > 0) ? 1 : -1;
        }
    }

    [Command]
    public void CmdLockInput(bool locked)
    {
        inputLocked = locked;
        if (locked)
            moveInput = Vector2.zero;
    }

    private void ClientPredictMove()
    {
        if (!isOwned) return;

        Vector2 predictedVelocity = new Vector2(moveInput.x * moveSpeed, rb.linearVelocity.y);

        rb.linearVelocity = new Vector2(predictedVelocity.x, rb.linearVelocity.y);
    }

    [Server]
    private void Move()
    {
        float moveX = moveInput.x * moveSpeed;
        float underMoveX = 0f;

        if (groundCheck.UnderPlayerRb != null)
        {
            underMoveX = groundCheck.UnderPlayerRb.linearVelocity.x;
        }

        rb.linearVelocity = new Vector2(
            moveX + (isInputPushing ? 0 : 1) * underMoveX,
            rb.linearVelocity.y
        );
    }

    [Server]
    public void SetMove(Vector2 input)
    {
        if (isInsideDoor) return;
        if (isDead) return;

        moveInput = input;
    }

    [Server]
    public void JumpStart()
    {
        if (!groundCheck.IsGround) return;
        if (isDead || isInsideDoor) return;

        AudioManager.Instance.PlaySFX("Jump");
        rb.linearVelocityY = jumpForce;
    }

    [Server]
    public void JumpStop()
    {
        if (rb.linearVelocity.y > 0f)
        {
            rb.linearVelocityY *= 0.5f;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!isServer) return;

        HandleCollisionEnterServer(collision);
    }

    [Server]
    private void HandleCollisionEnterServer(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("DeadLine"))
        {
            Vector3 respawnPos = StageCheckpointManager.Instance.GetRespawnPosition();

            transform.position = respawnPos;
        }

        if (collision.gameObject.CompareTag("FlatForm") && collision.contacts[0].normal.y > 0.7f)
        {
            PlatForm platform = collision.gameObject.GetComponent<PlatForm>();
            if (platform != null)
            {
                platform.AddRider(this);
            }
        }
    }

    [ServerCallback]
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!isServer) return;

        if (collision.CompareTag("SavePoint"))
        {
            SavePoint savePoint = collision.GetComponent<SavePoint>();
            if (savePoint == null) return;

            if (!savePoint.TryActivate())
                return;

            // 최초 도달한 경우만 갱신
            StageCheckpointManager.Instance.SetCheckpoint(savePoint.returnPos);
        }
    }

    [ServerCallback]
    private void OnCollisionStay2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag("MovingWall")) return;

        // 벽과 실제로 맞닿아 있음
        touchingWall = true;
    }

    [ServerCallback]
    private void OnCollisionExit2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag("MovingWall")) return;

        touchingWall = false;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!isServer) return;

        HandleTriggerExitServer(collision);
    }

    [Server]
    private void HandleTriggerExitServer(Collider2D collision)
    {
        if (collision.CompareTag("FlatForm"))
        {
            PlatForm platform = collision.GetComponent<PlatForm>();
            if (platform != null)
            {
                platform.RemoveRider(this);
            }
        }
    }

    [Server]
    public void SetReturnPos(Transform newReturnPos)
    {
        returnPos = newReturnPos;
    }

    public bool IsMoving()
    {
        // 입력 체크
        bool hasInput = Mathf.Abs(moveInput.x) > 0.01f || Mathf.Abs(moveInput.y) > 0.01f;

        // 실제 이동 속도 체크 (관성으로 움직이는 것도 포함)
        bool isMoving = rb.linearVelocity.magnitude > 0.1f;

        return hasInput || isMoving;
    }

    [Server]
    //밀기 체크
    private void CheckPush()
    {
        // 밀기 의도 (입력 + 착지) 판정
        isInputPushing = groundCheck.IsGround && Mathf.Abs(moveInput.x) > 0.01f;

        // 직접 벽을 밀고 있는지 확인
        bool pushingWall = isInputPushing && touchingWall;

        // 앞 사람을 밀고 있고, 앞 사람이 이미 힘을 전달 중인지 확인
        bool pushingFrontPlayer = isInputPushing && frontPlayer != null && frontPlayer.isContributingPush;

        // 최종적으로 힘 전달 여부 결정
        isContributingPush = pushingWall || pushingFrontPlayer;
    }

    private void OnRunChanged(bool _, bool newValue)
    {
        animator.SetBool("IsRun", newValue);
    }

    private void OnPushChanged(bool _, bool newValue)
    {
        animator.SetBool("IsPush", newValue);
    }

    private void OnGroundChanged(bool _, bool newValue)
    {
        animator.SetBool("IsGround", newValue);
    }

    private void OnFlipChanged(int _, int newDir)
    {
        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * newDir;
        transform.localScale = scale;
    }

    private void OnColorChanged(int oldValue, int newValue)
    {
        ApplyAppearance();
    }

    private void OnHatChanged(int oldValue, int newValue)
    {
        ApplyAppearance();
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

    [Server]
    public void Die()
    {
        if (isDead) return;
        isDead = true;
        AudioManager.Instance.PlaySFX("Dead");

        RpcDie();
    }

    [ClientRpc]
    public void RpcDie()
    {

        isInputPushing = false;
        moveInput = Vector2.zero;

        // Rigidbody를 Kinematic으로 변경
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.linearVelocity = Vector2.zero;

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