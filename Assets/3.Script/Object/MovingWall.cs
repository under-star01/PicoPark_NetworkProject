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
    private float pushDir; 
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

        int newRemaining = Mathf.Max(0, targetMoveCnt - pushers.Count);
        if (remainingCnt != newRemaining)
            remainingCnt = newRemaining;

        if (!canMove)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        rb.linearVelocity = new Vector2(pushDir * 2f, rb.linearVelocity.y);
    }

    // 서버 전용 로직
    public void AddPusher(PlayerMove p)
    {
        if (!isServer || p == null) return;
        
        pushers.Add(p);

        // 방향 저장
        pushDir = Mathf.Sign(p.transform.localScale.x);

        if (pushers.Count >= targetMoveCnt)
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
            canMove = true;
        }
    }

    public void RemovePusher(PlayerMove p)
    {
        if (!isServer || p == null) return;

        pushers.Remove(p);

        if (pushers.Count < targetMoveCnt)
        {
            canMove = false;
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.linearVelocity = Vector2.zero;
        }
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
