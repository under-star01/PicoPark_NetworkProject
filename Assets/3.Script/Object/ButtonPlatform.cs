using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonPlatform : MonoBehaviour
{
    [Header("이동 설정")]
    [SerializeField] private Transform targetPos; // 이동할 목표 위치
    [SerializeField] private float moveSpeed = 2f;

    private Vector3 startPos;
    private Vector3 endPos;
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

    private void FixedUpdate()
    {
        if (rb == null) return;

        Vector3 targetPos = isMoving ? endPos : startPos;
        Vector3 newPos = Vector3.MoveTowards(rb.position, targetPos, moveSpeed * Time.fixedDeltaTime);
        rb.MovePosition(newPos);
    }

    public void MoveToTarget()
    {
        isMoving = true;
    }

    public void MoveToStart()
    {
        isMoving = false;
    }
}
