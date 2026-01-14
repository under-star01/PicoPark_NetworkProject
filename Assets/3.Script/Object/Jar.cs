using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Jar : MonoBehaviour
{
    [Header("내구도 설정")]
    [SerializeField] private int hitCount = 3; // 최대 맞을 수 있는 횟수
    private int currentHitCount = 0;

    [Header("스프라이트 설정")]
    [SerializeField] private Sprite[] damageSprites; // 데미지 단계별 스프라이트 (점점 금가는거)
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        TryGetComponent(out spriteRenderer);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 대포알과 충돌
        if (collision.gameObject.CompareTag("CanonBall"))
        {
            TakeDamage();
        }
    }

    private void TakeDamage()
    {
        currentHitCount++;

        // 스프라이트 변경
        UpdateSprite();

        // 최대 횟수 도달 시 깨짐
        if (currentHitCount >= hitCount)
        {
            Break();
        }
    }

    private void UpdateSprite()
    {
        // 스프라이트 배열이 있고, 현재 히트 카운트에 맞는 스프라이트가 있으면 변경
        if (damageSprites != null && damageSprites.Length > currentHitCount)
        {
            spriteRenderer.sprite = damageSprites[currentHitCount];
        }
    }

    private void Break()
    {
            Destroy(gameObject);
    }

    public void ResetJar()
    {
        currentHitCount = 0;
        UpdateSprite();
    }
}
