using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class PlatForm : NetworkBehaviour
{
    [Header("조건")]
    [SerializeField] private int targetMoveCnt = 2;
    [SyncVar(hook = nameof(OnRemainingCntChanged))]
    private int remainingRiderCnt;

    [Header("이동")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float minY; // 최소 위치
    [SerializeField] private float maxY; // 최대 위치
    [SerializeField] private float lockTime = 1f; // 대기 시간
    private bool prevIsActive;

    [Header("숫자 스프라이트 설정")]
    [SerializeField] private SpriteRenderer sRenderer; // 스프라이트 렌더러
    [SerializeField] private Sprite[] numbers; // (0: 0, 1: 1, 2: 2, 3: 3...)
    private int cachedTotalStack;

    private HashSet<PlayerMove> riders = new HashSet<PlayerMove>();
    private Rigidbody2D rb;
    private Coroutine lockRoutine;
    private bool isLocked;
    private bool isActive; // 목표 인원 도달 여부

    private void Awake()
    {
        TryGetComponent(out rb);
        rb.bodyType = RigidbodyType2D.Kinematic;
    }
    public override void OnStartServer()
    {
        base.OnStartServer();

        CalculateRiders();
        isActive = cachedTotalStack >= targetMoveCnt;
        prevIsActive = isActive;
    }

    private void FixedUpdate()
    {
        if (!isServer) return;

        bool nextIsActive = cachedTotalStack >= targetMoveCnt;

        // 상태 전이 감지
        if (nextIsActive != prevIsActive && !isLocked)
        {
            StartLock();
        }

        isActive = nextIsActive;
        prevIsActive = isActive;

        MovePlatform();
    }

    private void MovePlatform()
    {
        if (isLocked) return;

        float targetY = isActive ? maxY : minY;
        float newY = Mathf.MoveTowards(rb.position.y, targetY, moveSpeed * Time.fixedDeltaTime);
        rb.MovePosition(new Vector2(rb.position.x, newY));
    }

    private void StartLock()
    {
        if (lockRoutine != null)
            StopCoroutine(lockRoutine);

        lockRoutine = StartCoroutine(LockFlatform_co());
    }

    // 잠시 대기 코루틴
    private IEnumerator LockFlatform_co()
    {
        isLocked = true;
        yield return new WaitForSeconds(lockTime);
        isLocked = false;
        lockRoutine = null;
    }

    public void ServerAttachPlayer(PlayerMove player)
    {
        if (!isServer || player == null) return;
        if (riders.Contains(player)) return;

        riders.Add(player);
        player.SetOnPlatform_Server(transform);
        CalculateRiders();
    }

    public void ServerDetachPlayer(PlayerMove player)
    {
        if (!isServer || player == null) return;
        if (!riders.Contains(player)) return;

        riders.Remove(player);
        player.ClearOnPlatform_Server();
        CalculateRiders();
    }

    // 남은 인원수 표시
    private void UpdateNumberSprite(int remaining)
    {
        if (sRenderer == null || numbers == null || numbers.Length == 0) return;

        // 남은 인원수에 맞는 숫자 스프라이트 설정
        if (remaining >= 0 && remaining < numbers.Length)
        {
            sRenderer.sprite = numbers[remaining];
        }
    }

    private void CalculateRiders()
    {
        riders.RemoveWhere(p => p == null);

        cachedTotalStack = 0;
        foreach (var p in riders)
            cachedTotalStack += p.stackCnt;

        int newRemaining = Mathf.Max(0, targetMoveCnt - cachedTotalStack);
        if (remainingRiderCnt != newRemaining)
            remainingRiderCnt = newRemaining;
    }

    private void OnRemainingCntChanged(int oldValue, int newValue)
    {
        UpdateNumberSprite(newValue);
    }
}