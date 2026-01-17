using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class MovingWall : NetworkBehaviour
{
    [Header("이동 설정")]
    [SerializeField] private int targetMoveCnt = 2;

    [Header("스프라이트 설정")]
    [SerializeField] private SpriteRenderer Srenderer;
    [SerializeField] private Sprite[] numbers; // 필요한 인원수 (0: 1명, 1: 2명, 2: 3명...)

    public HashSet<PlayerMove> pushers = new HashSet<PlayerMove>();
    
    [SyncVar(hook = nameof(OnRemainingCntChanged))]
    private int remainingCnt;
    private bool canMove = false;

    private Rigidbody2D rb;

    private void Awake()
    {
        TryGetComponent(out rb);
    }

    public override void OnStartServer()
    {
        base.OnStartServer();

        rb.bodyType = RigidbodyType2D.Kinematic;
        remainingCnt = targetMoveCnt;
    }

    private void FixedUpdate()
    {
        if (!isServer) return;

        int newRemaining = Mathf.Clamp(targetMoveCnt - pushers.Count, 0, targetMoveCnt);
        if (remainingCnt != newRemaining)
            remainingCnt = newRemaining;

        if (!canMove)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        rb.linearVelocity = new Vector2(GetPushDir() * 2f, rb.linearVelocity.y);
    }

    // 서버 전용 로직
    public void AddPusher(PlayerMove p)
    {
        if (!isServer || p == null) return;

        if (!pushers.Add(p)) return;

        if (pushers.Count >= targetMoveCnt)
        {
            canMove = true;
            rb.bodyType = RigidbodyType2D.Dynamic;
        }
    }

    public void RemovePusher(PlayerMove p)
    {
        if (!isServer || p == null) return;

        if (!pushers.Remove(p)) return;

        if (pushers.Count < targetMoveCnt)
        {
            canMove = false;
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Kinematic;
        }
    }

    private float GetPushDir()
    {
        foreach (var p in pushers)
            return Mathf.Sign(p.transform.localScale.x);
        return 0f;
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
}
