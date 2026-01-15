using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlatForm : MonoBehaviour
{
    [Header("조건")]
    [SerializeField] private int targetMoveCnt = 2;

    [Header("이동")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float minY; // 최소 위치
    [SerializeField] private float maxY; // 최대 위치
    [SerializeField] private float lockTime = 0.5f; // 최대 위치 도달시, 대기 시간

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

    private void FixedUpdate()
    {
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
        riders.Add(p);
    }

    public void RemoveRider(PlayerMove p)
    {
        riders.Remove(p);
    }
}
