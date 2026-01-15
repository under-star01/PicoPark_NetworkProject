using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingWall : MonoBehaviour
{
    [Header("이동 설정")]
    [SerializeField] private int targetMoveCnt = 2;
    [SerializeField] private int pushCnt = 0;

    [Header("스프라이트 설정")]
    [SerializeField] private SpriteRenderer Srenderer;
    [SerializeField] private Sprite[] numbers; // 필요한 인원수 (0: 1명, 1: 2명, 2: 3명...)

    public HashSet<PlayerMove> pushers = new HashSet<PlayerMove>();

    private Rigidbody2D rb;
    private int prevRemainingCnt = -1; // 이전 프레임 남은 인원수

    private void Awake()
    {
        TryGetComponent(out rb);

        // 초기 숫자 스프라이트 설정
        UpdateNumberSprite();
    }

    private void FixedUpdate()
    {
        rb.linearVelocityX = 0f;

        // 현재 밀고 있는 인원수
        pushCnt = pushers.Count;

        // 남은 인원수 계산 (필요 인원 - 현재 밀고 있는 인원)
        int remainingCnt = Mathf.Max(0, targetMoveCnt - pushCnt);

        // 남은 인원수가 변경되면 숫자 스프라이트 업데이트
        if (remainingCnt != prevRemainingCnt)
        {
            UpdateNumberSprite(remainingCnt);
            prevRemainingCnt = remainingCnt;
        }
    }
    public void AddPusher(PlayerMove p)
    {
        pushers.Add(p);

        if (pushers.Count >= targetMoveCnt)
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
        }
    }

    public void RemovePusher(PlayerMove p)
    {
        pushers.Remove(p);

        if (pushers.Count < targetMoveCnt)
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
        }
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

    // 초기 숫자 표시 (Awake용)
    private void UpdateNumberSprite()
    {
        UpdateNumberSprite(targetMoveCnt);
    }
}
