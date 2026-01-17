using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonPlatform : NetworkBehaviour
{
    [Header("이동 설정")]
    [SerializeField] private Transform targetPos; // 이동할 목표 위치
    [SerializeField] private float moveSpeed = 2f;

    private Vector3 startPos;
    private Vector3 endPos;

    [SyncVar]
    private bool isMoving = false;
    private Rigidbody2D rb;

    private void Awake()
    {
        TryGetComponent(out rb);
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
        Vector3 newPos = Vector3.MoveTowards(
            rb.position,
            target,
            moveSpeed * Time.fixedDeltaTime
        );

        rb.MovePosition(newPos);
    }

    // 버튼 OnPress 연결
    [Server]
    public void MoveToTarget()
    {
        isMoving = true;
    }

    // 버튼 OffPress 연결
    [Server]
    public void MoveToStart()
    {
        isMoving = false;
    }
}
