using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class FlatForm : NetworkBehaviour
{
    [Header("조건")]
    [SerializeField] private int targetMoveCnt = 2;
    [SyncVar(hook = nameof(OnRemainingCntChanged))]
    private int moveCurrentCnt;

    [Header("이동")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float minY; // 최소 위치
    [SerializeField] private float maxY; // 최대 위치
    [SerializeField] private float lockTime = 0.5f; // 최대 위치 도달시, 대기 시간

    [Header("숫자 스프라이트 설정")]
    [SerializeField] private SpriteRenderer sRenderer; // 스프라이트 렌더러
    [SerializeField] private Sprite[] numbers; // (0: 0, 1: 1, 2: 2, 3: 3...)

    private HashSet<PlayerMove> riders = new HashSet<PlayerMove>();
    private Rigidbody2D rb;
    private Coroutine lockRoutine;
    private bool isLocked;
    private bool isActive; // 목표 인원 도달 여부

    private void Awake()
    {
        TryGetComponent(out rb);
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        UpdateNumberSprite(moveCurrentCnt);
    }

    public override void OnStartServer()
    {
        base.OnStartServer();

        rb.bodyType = RigidbodyType2D.Kinematic;
        moveCurrentCnt = targetMoveCnt; // 초기 숫자
    }

    private void FixedUpdate()
    {
        if (!isServer) return;

        if (!isLocked)
        {
            isActive = GetTotalCeilCnt() >= targetMoveCnt;
        }

        MovePlatform();
    }

    private int GetTotalCeilCnt()
    {
        int total = 0;
        foreach (PlayerMove player in riders)
        {
            if (player == null) continue;
            total += player.stackCnt;
        }

        int newRemaining = Mathf.Max(0, targetMoveCnt - total);

        if (moveCurrentCnt != newRemaining)
            moveCurrentCnt = newRemaining;

        return total;
    }

    private void MovePlatform()
    {
        float targetY = isActive ? maxY : minY;
        float newY = Mathf.MoveTowards(rb.position.y, targetY, moveSpeed * Time.fixedDeltaTime);
        rb.MovePosition(new Vector2(rb.position.x, newY));

        // 최대 위치 도달시, 잠시 멈춤
        if (isActive && Mathf.Abs(rb.position.y - maxY) < 0.01f && !isLocked)
        {
            if (lockRoutine != null)
            {
                StopCoroutine(lockRoutine);
            }
            lockRoutine = StartCoroutine(LockFlatform_co());
        }
    }

    // 잠시 대기 코루틴
    private IEnumerator LockFlatform_co()
    {
        isLocked = true;
        yield return new WaitForSeconds(lockTime);
        isLocked = false;
    }

    public void AddRider(PlayerMove p)
    {
        if (!isServer) return;
        riders.Add(p);
    }

    public void RemoveRider(PlayerMove p)
    {
        if (!isServer) return;
        riders.Remove(p);
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

    private void OnRemainingCntChanged(int oldValue, int newValue)
    {
        UpdateNumberSprite(newValue);
    }
}