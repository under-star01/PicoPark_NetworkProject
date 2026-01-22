using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonPlatform : NetworkBehaviour
{
    [Header("이동 설정")]
    [SerializeField] private Transform targetPos;
    [SerializeField] private float moveSpeed = 2f;

    [Header("플레이어 감지")]
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private float detectionDistance = 0.5f; // 플레이어 감지 거리
    [SerializeField] private Vector2 detectionSize = new Vector2(1f, 0.3f); // 감지 박스 크기

    private Vector3 startPos;
    private Vector3 endPos;

    [SyncVar]
    private bool isMoving = false;

    private Rigidbody2D rb;
    private BoxCollider2D col;

    private void Awake()
    {
        TryGetComponent(out rb);
        TryGetComponent(out col);

        startPos = transform.position;

        if (targetPos != null)
        {
            endPos = targetPos.position;
        }
    }

    [ServerCallback]
    private void FixedUpdate()
    {
        if (rb == null) return;

        Vector3 target = isMoving ? endPos : startPos;

        // 내려가는 중일 때만 플레이어 체크
        if (!isMoving && Vector3.Distance(rb.position, startPos) > 0.1f)
        {
            // 문 아래에 플레이어가 있는지 체크
            if (IsPlayerBelow())
            {
                // 플레이어가 있으면 움직이지 않음
                return;
            }
        }

        Vector3 newPos = Vector3.MoveTowards(
            rb.position,
            target,
            moveSpeed * Time.fixedDeltaTime
        );

        rb.MovePosition(newPos);
    }

    [Server]
    private bool IsPlayerBelow()
    {
        // 문의 아래쪽 방향으로 Raycast
        Vector2 checkPosition = rb.position;
        Vector2 direction = Vector2.down;

        // BoxCast로 플레이어 감지
        RaycastHit2D hit = Physics2D.BoxCast(
            checkPosition,
            detectionSize,
            0f,
            direction,
            detectionDistance,
            playerLayer
        );

        return hit.collider != null;
    }

    [Server]
    public void MoveToTarget()
    {
        isMoving = true;
    }

    [Server]
    public void MoveToStart()
    {
        isMoving = false;
    }

    // 디버그용 - 감지 범위 표시
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Vector3 center = transform.position + Vector3.down * (detectionDistance / 2f);
        Gizmos.DrawWireCube(center, new Vector3(detectionSize.x, detectionDistance, 0));
    }
}