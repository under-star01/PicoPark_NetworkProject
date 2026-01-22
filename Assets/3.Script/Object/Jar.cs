using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Jar : NetworkBehaviour
{
    [Header("내구도 설정")]
    [SerializeField] private int hitCount = 3; // 최대 맞을 수 있는 횟수

    [SyncVar(hook = nameof(OnHitCountChanged))]
    private int currentHitCount = 0;

    [Header("스프라이트 설정")]
    [SerializeField] private Sprite[] damageSprites; // 데미지 단계별 스프라이트 (점점 금가는거)
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        TryGetComponent(out spriteRenderer);
    }

    private void Start()
    {
        if (spriteRenderer != null && damageSprites.Length > 0)
        {
            spriteRenderer.sprite = damageSprites[0];
        }
    }

    [ServerCallback]
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 대포알과 충돌
        if (collision.gameObject.CompareTag("CanonBall"))
        {
            TakeDamageServer();
        }
    }

    [Server]
    private void TakeDamageServer()
    {
        if (currentHitCount >= hitCount) return;

        currentHitCount++;

        if (currentHitCount >= hitCount)
        {
            BreakServer();
        }
    }

    [Server]
    private void BreakServer()
    {
        NetworkServer.Destroy(gameObject);
    }

    private void OnHitCountChanged(int oldValue, int newValue)
    {
        UpdateSprite(newValue);
    }

    private void UpdateSprite(int hit)
    {
        if (spriteRenderer == null) return;

        if (damageSprites != null && hit > 0 && hit - 1 < damageSprites.Length)
        {
            spriteRenderer.sprite = damageSprites[hit - 1];
        }
    }
}
