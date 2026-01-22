using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class MovingWall : NetworkBehaviour
{
    [Header("이동 설정")]
    [SerializeField] private int targetMoveCnt = 2;
    [SerializeField] private float moveSpeed = 2f;

    [Header("스프라이트 설정")]
    [SerializeField] private SpriteRenderer Srenderer;
    [SerializeField] private Sprite[] numbers; // 필요한 인원수 (0: 1명, 1: 2명, 2: 3명...)

    [Header("미는 플레이어 관련 설정")]
    public HashSet<PlayerMove> pushers = new HashSet<PlayerMove>();

    [SyncVar(hook = nameof(OnRemainingCntChanged))]
    private int remainingCnt;
    private float pushDir; 
    
    private Rigidbody2D rb;

    private void Awake()
    {
        TryGetComponent(out rb);
    }

    public override void OnStartServer()
    {
        base.OnStartServer();

        remainingCnt = targetMoveCnt;
    }

    [ServerCallback]
    private void FixedUpdate()
    {
        // null 값 정리!
        pushers.RemoveWhere(p => p == null);

        // 방향 결정 (첫 명 기준)
        if (pushers.Count > 0)
        {
            PlayerMove first = GetAnyPusher();
            float dir = transform.position.x - first.transform.position.x;
            pushDir = dir >= 0 ? 1f : -1f;
        }

        int newRemaining = Mathf.Max(0, targetMoveCnt - pushers.Count);
        if (remainingCnt != newRemaining)
        {
            remainingCnt = newRemaining;
        }

        bool shouldMove = pushers.Count >= targetMoveCnt;

        rb.bodyType = shouldMove ? RigidbodyType2D.Dynamic : RigidbodyType2D.Kinematic;
        rb.linearVelocity = shouldMove ? new Vector2(pushDir * moveSpeed, rb.linearVelocity.y) : new Vector2(0f, rb.linearVelocity.y);
    }

    // SyncVar Hook 설정
    private void OnRemainingCntChanged(int oldValue, int newValue)
    {
        UpdateNumberSprite(newValue);
    }

    // 숫자 스프라이트 업데이트
    private void UpdateNumberSprite(int remaining)
    {
        if (Srenderer == null || numbers == null || numbers.Length == 0) return;

        // 남은 인원수에 맞는 숫자 스프라이트 설정
        if (remaining >= 0 && remaining < numbers.Length)
        {
            Srenderer.sprite = numbers[remaining];
        }
    }

    private PlayerMove GetAnyPusher()
    {
        foreach (var p in pushers)
            return p;

        return null;
    }

    [Server]
    public void UpdateContributor(PlayerMove player, bool isContributing)
    {
        if (isContributing)
            pushers.Add(player);
        else
            pushers.Remove(player);
    }
}
